using Storm.Api.Launchers;

namespace TaskList.Api;

public class Program
{
	public static async Task Main(string[] args)
	{
		DefaultLauncherOptions.SkipOrmLiteLicenseCheck = true;

		await DefaultLauncher<Startup>.BuildWebHost(args).RunAsync();
	}
}