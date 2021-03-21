using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;

class Program
{
    private CancellationTokenSource cts {get;set;}
    private IConfigurationRoot config {get;set;}

    private DiscordClient discord;
    private CommandsNextModule commands;
    private InteractivityModule interactivity;
    private bool fridayMemeDone = false;

    static async Task Main(string[] args) => await new Program().InitBot(args);

    async Task InitBot(string[] args)
    {
        try
        {
            System.Console.WriteLine("[info] Bot startup");
            cts = new CancellationTokenSource();

            System.Console.WriteLine("[info] Loading configs");
            config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange:true)
                .AddJsonFile("config.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

                System.Console.WriteLine("[info] Creating client");
                discord = new DiscordClient(new DiscordConfiguration
                {
                    Token = config.GetValue<string>("bot_token"),
                    TokenType = TokenType.Bot
                });

                interactivity = discord.UseInteractivity(new InteractivityConfiguration()
                {
                    PaginationBehaviour = TimeoutBehaviour.Delete,
                    PaginationTimeout = TimeSpan.FromSeconds(30),
                    Timeout = TimeSpan.FromSeconds(30)
                });

                var deps = BuildDeps();
                commands = discord.UseCommandsNext(new CommandsNextConfiguration()
                {
                    StringPrefix = "!",
                    Dependencies = deps
                });

                System.Console.WriteLine("[info] Loading modules");

                var type = typeof(IModule);
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

                var typeList = types as Type[] ?? types.ToArray();
                foreach (var t in typeList)
                    commands.RegisterCommands(t);

                System.Console.WriteLine($"[info] Loaded {typeList.Count()} modules.");

                System.Console.WriteLine($"Volume exists: {Directory.Exists(@"/app/data")}");

                RunAsync(args).Wait();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
        }
    }

    async Task RunAsync(string[] args)
    {
        System.Console.WriteLine("Connecting");

        discord.GuildAvailable += GuildConnected;

        await discord.ConnectAsync();
        System.Console.WriteLine("Connected!");

        while(!cts.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }

    private async Task GuildConnected(DSharpPlus.EventArgs.GuildCreateEventArgs args)
    {
        System.Console.WriteLine($"Connected to server: {args.Guild.Name}");
    }

    private DependencyCollection BuildDeps()
    {
        using var deps = new DependencyCollectionBuilder();

        deps.AddInstance(interactivity)
            .AddInstance(cts)
            .AddInstance(config)
            .AddInstance(discord);

        return deps.Build();
    }
}
