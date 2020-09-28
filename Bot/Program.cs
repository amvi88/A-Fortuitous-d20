using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;
using DSharpPlus.Interactivity.Enums;
using Microsoft.Extensions.DependencyInjection;
using FortuitousD20.Providers;

namespace FortuitousD20
{
    internal class Program
    {
        private CancellationTokenSource _cts { get; set; }
        private IConfiguration _config;
        private DiscordClient _discord;
        private CommandsNextExtension _commands;
        private InteractivityExtension _interactivity;

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
                    PaginationBehaviour = PaginationBehaviour.Ignore,
                    Timeout = TimeSpan.FromSeconds(30)
                });

                var deps = RegisterDependencies();

                _commands = _discord.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefixes = new[] { _config.GetValue<string>("discord:CommandPrefix") },
                    Services = deps
                });

                _commands.RegisterCommands(typeof(StatusCommand).Assembly);

               
                RunAsync(args).Wait();
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        private ServiceProvider RegisterDependencies()
        {
            var deps = new ServiceCollection()
                .AddSingleton(_interactivity)
                .AddSingleton(_cts)
                .AddSingleton(_config)
                .AddSingleton(_discord)
                .AddScoped<IDiceRoller,DiceRoller>();

            return deps.BuildServiceProvider();
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