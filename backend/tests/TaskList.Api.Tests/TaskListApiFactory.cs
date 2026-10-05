using System.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceStack.OrmLite;
using Storm.Api.Databases.Services;
using Storm.Api.Launchers;
using TaskList.Api.Entities;
using Testcontainers.MsSql;

namespace TaskList.Api.Tests;

/// <summary>
/// Boots the API on a TestServer against a throwaway SQL Server container, so the tests exercise
/// the real OrmLite dialect and the real migrations. Set TASKLIST_TEST_CONNECTION_STRING to point
/// at an already-running SQL Server instead (handy on a CI runner that provides one as a service).
/// </summary>
public class TaskListApiFactory : IAsyncLifetime
{
	private const string TESTDATABASENAME = "tasklist_tests";

	private IHost _host = null!;

	public async ValueTask InitializeAsync()
	{
		DefaultLauncherOptions.SkipOrmLiteLicenseCheck = true;
		_host = DefaultLauncher<Startup>.WebHostBuilder(
				[],
				configureWebHost: builder => builder.UseTestServer(),
				configureConfiguration: (_, configuration) => configuration.AddInMemoryCollection(
					new Dictionary<string, string?>
					{
						["Database:type"] = "SQLiteMemory",
					}))
			.Build();

		await _host.StartAsync();
	}

	public async ValueTask DisposeAsync()
	{
		await _host.StopAsync();
		_host.Dispose();
	}

	public HttpClient CreateClient() => _host.GetTestClient();

	/// <summary>Empties the tasks table so each test starts from a known state.</summary>
	public async Task ResetDatabaseAsync()
	{
		using IServiceScope scope = _host.Services.CreateScope();
		IDatabaseService databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
		IDbConnection connection = await databaseService.GetConnection(TestContext.Current.CancellationToken);

		await connection.DeleteAllAsync<TaskEntity>();
	}
}