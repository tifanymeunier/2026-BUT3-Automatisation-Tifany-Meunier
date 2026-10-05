using System.Net;
using System.Net.Http.Json;
using Storm.Api.Dtos;
using TaskList.Api.Actions.Health;
using TaskList.Api.Actions.Tasks;
using TaskList.Api.Dtos;

namespace TaskList.Api.Tests;

[Collection(nameof(TaskListApiCollection))]
public class TaskEndpointsTests(TaskListApiFactory factory) : IAsyncLifetime
{
	private HttpClient _client = null!;

	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	public async ValueTask InitializeAsync()
	{
		await factory.ResetDatabaseAsync();
		_client = factory.CreateClient();
	}

	public ValueTask DisposeAsync()
	{
		_client.Dispose();
		return ValueTask.CompletedTask;
	}

	[Fact]
	public async Task Get_returns_an_empty_list_when_there_is_no_task()
	{
		List<TaskDto> tasks = await ReadDataAsync<List<TaskDto>>(await _client.GetAsync("/api/v1/tasks", Ct));

		Assert.Empty(tasks);
	}

	[Fact]
	public async Task Post_creates_a_task()
	{
		HttpResponseMessage response = await _client.PostAsJsonAsync(
			"/api/v1/tasks", new CreateTaskBody { Title = "Set up the pipeline" }, Ct);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);

		TaskDto created = await ReadDataAsync<TaskDto>(response);
		Assert.Equal("Set up the pipeline", created.Title);
		Assert.False(created.IsCompleted);
		Assert.Null(created.CompletedAt);
		Assert.NotEqual(Guid.Empty, created.Id);
	}

	[Fact]
	public async Task Post_trims_the_title()
	{
		TaskDto created = await CreateTaskAsync("   Trim me   ");

		Assert.Equal("Trim me", created.Title);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public async Task Post_rejects_a_blank_title(string title)
	{
		HttpResponseMessage response = await _client.PostAsJsonAsync(
			"/api/v1/tasks", new CreateTaskBody { Title = title }, Ct);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		Assert.Equal(TaskErrors.TITLE_REQUIRED, (await ReadResponseAsync(response)).ErrorCode);
	}

	[Fact]
	public async Task Post_rejects_a_title_longer_than_400_characters()
	{
		HttpResponseMessage response = await _client.PostAsJsonAsync(
			"/api/v1/tasks", new CreateTaskBody { Title = new string('x', 401) }, Ct);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		Assert.Equal(TaskErrors.TITLE_TOO_LONG, (await ReadResponseAsync(response)).ErrorCode);
	}

	[Fact]
	public async Task Get_returns_the_newest_task_first()
	{
		await CreateTaskAsync("First");
		await CreateTaskAsync("Second");

		List<TaskDto> tasks = await ReadDataAsync<List<TaskDto>>(await _client.GetAsync("/api/v1/tasks", Ct));

		Assert.Equal(["Second", "First"], tasks.Select(task => task.Title));
	}

	[Fact]
	public async Task Put_completing_a_task_stamps_CompletedAt()
	{
		TaskDto created = await CreateTaskAsync("Ship it");

		HttpResponseMessage response = await _client.PutAsJsonAsync(
			$"/api/v1/tasks/{created.Id}", new UpdateTaskBody { IsCompleted = true }, Ct);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		TaskDto updated = await ReadDataAsync<TaskDto>(response);
		Assert.True(updated.IsCompleted);
		Assert.NotNull(updated.CompletedAt);
	}

	[Fact]
	public async Task Put_reopening_a_task_clears_CompletedAt()
	{
		TaskDto created = await CreateTaskAsync("Ship it");
		await _client.PutAsJsonAsync($"/api/v1/tasks/{created.Id}", new UpdateTaskBody { IsCompleted = true }, Ct);

		HttpResponseMessage response = await _client.PutAsJsonAsync(
			$"/api/v1/tasks/{created.Id}", new UpdateTaskBody { IsCompleted = false }, Ct);

		TaskDto updated = await ReadDataAsync<TaskDto>(response);
		Assert.False(updated.IsCompleted);
		Assert.Null(updated.CompletedAt);
	}

	[Fact]
	public async Task Put_renames_a_task_without_touching_its_completion_state()
	{
		TaskDto created = await CreateTaskAsync("Old name");
		await _client.PutAsJsonAsync($"/api/v1/tasks/{created.Id}", new UpdateTaskBody { IsCompleted = true }, Ct);

		HttpResponseMessage response = await _client.PutAsJsonAsync(
			$"/api/v1/tasks/{created.Id}", new UpdateTaskBody { Title = "New name" }, Ct);

		TaskDto updated = await ReadDataAsync<TaskDto>(response);
		Assert.Equal("New name", updated.Title);
		Assert.True(updated.IsCompleted);
	}

	[Fact]
	public async Task Put_returns_404_for_an_unknown_task()
	{
		HttpResponseMessage response = await _client.PutAsJsonAsync(
			$"/api/v1/tasks/{Guid.NewGuid()}", new UpdateTaskBody { IsCompleted = true }, Ct);

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		Assert.Equal(TaskErrors.TASK_NOT_FOUND, (await ReadResponseAsync(response)).ErrorCode);
	}

	[Fact]
	public async Task Delete_removes_the_task()
	{
		TaskDto created = await CreateTaskAsync("Delete me");

		HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/tasks/{created.Id}", Ct);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.True((await ReadResponseAsync(response)).IsSuccess);

		List<TaskDto> tasks = await ReadDataAsync<List<TaskDto>>(await _client.GetAsync("/api/v1/tasks", Ct));
		Assert.Empty(tasks);
	}

	[Fact]
	public async Task Delete_returns_404_for_an_unknown_task()
	{
		HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/tasks/{Guid.NewGuid()}", Ct);

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task Health_endpoint_reports_the_database_as_healthy()
	{
		HttpResponseMessage response = await _client.GetAsync("/api/v1/health", Ct);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal("Healthy", (await ReadDataAsync<HealthDto>(response)).Status);
	}

	private async Task<TaskDto> CreateTaskAsync(string title)
	{
		HttpResponseMessage response = await _client.PostAsJsonAsync(
			"/api/v1/tasks", new CreateTaskBody { Title = title }, Ct);
		response.EnsureSuccessStatusCode();

		return await ReadDataAsync<TaskDto>(response);
	}

	private static async Task<T> ReadDataAsync<T>(HttpResponseMessage response)
	{
		Response<T>? body = await response.Content.ReadFromJsonAsync<Response<T>>(Ct);

		Assert.NotNull(body);
		Assert.True(body.IsSuccess, body.ErrorMessage);
		Assert.NotNull(body.Data);

		return body.Data;
	}

	private static async Task<Response> ReadResponseAsync(HttpResponseMessage response)
	{
		Response? body = await response.Content.ReadFromJsonAsync<Response>(Ct);

		Assert.NotNull(body);

		return body;
	}
}