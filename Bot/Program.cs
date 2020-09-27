using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;

namespace FortuitousD20
{
    internal class Program
    {
        private CancellationTokenSource _cts { get; set; }
        private IConfiguration _config;
        private DiscordClient _discord;
        private CommandsNextModule _commands;
        private InteractivityModule _interactivity;

        /* Use the async main to create an instance of the class and await it(async main is only available in C# 7.1 onwards). */
        static async Task Main(string[] args) => await new Program().InitBot(args);

        async Task InitBot(string[] args)
        {
            try
            {
                _cts = new CancellationTokenSource(); 

                _config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddUserSecrets<Program>()
                    .Build();

                _discord = new DiscordClient(new DiscordConfiguration
                {
                    Token = _config.GetValue<string>("discord:token"),
                    TokenType = TokenType.Bot
                });

                _interactivity = _discord.UseInteractivity(new InteractivityConfiguration()
                {
                    PaginationBehaviour = TimeoutBehaviour.Delete,
                    PaginationTimeout = TimeSpan.FromSeconds(30),
                    Timeout = TimeSpan.FromSeconds(30)
                });

                var deps = BuildDeps();

                _commands = _discord.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefix = _config.GetValue<string>("discord:CommandPrefix"),
                    Dependencies = deps
                });

                _commands.RegisterCommands(typeof(StatusCommand).Assembly);

               
                RunAsync(args).Wait();
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        private DependencyCollection BuildDeps()
        {
            using var deps = new DependencyCollectionBuilder();

            deps.AddInstance(_interactivity)
                .AddInstance(_cts)
                .AddInstance(_config)
                .AddInstance(_discord);

            return deps.Build();
        }

        async Task RunAsync(string[] args)
        {
            await _discord.ConnectAsync();
            
            while (!_cts.IsCancellationRequested)
            {
                Console.Out.WriteLine("Waiting");
                await Task.Delay(TimeSpan.FromMinutes(1));
            }            
        }                
    } 
}