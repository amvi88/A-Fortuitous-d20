using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using FortuitousD20.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FortuitousD20.Commands
{
    public class DiceCommand : BaseCommandModule
    {
        private const string RollPattern = @"(?<numberOfDice>\d*)d(?<numberOfSides>\d*)(k(?<order>h|l)(?<selectionNumber>(\d*)))?";
        private readonly IDiceRoller _diceRoller;
        private Regex _diceRegex;


        public DiceCommand(IDiceRoller diceRoller)
        {
            _diceRoller = diceRoller ?? throw new ArgumentNullException(nameof(diceRoller));
            _diceRegex =  new Regex(RollPattern, RegexOptions.Compiled);
        }

        [Command("roll")]
        [Description("Roll the dice(s)!")]
        public async Task Roll(CommandContext ctx, [RemainingText, Description("The dice to roll.")] string rollConfiguration)
        {
            await ctx.TriggerTypingAsync();
            try 
            {
                var diceResults = await GetRolls(rollConfiguration);
                await ctx.RespondAsync($"`[{string.Join(',', diceResults)}] => `**[{diceResults.Sum()}]**");
            }
            catch(ArgumentException)
            {
                await ctx.RespondAsync("Sorry I don´t understand try 1d20 or 1d20kh3 for instance");
            }
        }

        protected async Task<IEnumerable<int>> GetRolls(string rollConfiguration)
        {
            if (string.IsNullOrWhiteSpace(rollConfiguration))
            {
                throw new ArgumentException(nameof(rollConfiguration));
            }

            var match = _diceRegex.Match(rollConfiguration);
            if (!match.Success)
            {
                throw new ArgumentException(nameof(rollConfiguration));
            }

            var numberOfDices = byte.Parse(match.Groups["numberOfDice"].Value);
            var numberOfSides = byte.Parse(match.Groups["numberOfSides"].Value);
            var order = match.Groups["order"].Value ?? null;


            var selection = match.Groups["selectionNumber"].Value;
            var selectionNumber = string.IsNullOrWhiteSpace(selection) ? (int?)null : int.Parse(selection);


            var diceResults = new List<int>();

            foreach (var index in Enumerable.Range(1, numberOfDices))
            {
                var roll = await _diceRoller.RollDice(numberOfSides);
                diceResults.Add(roll);
            }

            if (!string.IsNullOrEmpty(order) && selectionNumber.HasValue)
            {
                diceResults = (order == "l") ? diceResults.OrderBy(x => x).ToList() : diceResults.OrderByDescending(x => x).ToList();
                diceResults = diceResults.Take(selectionNumber.Value).ToList();
            }

            return diceResults;
        }
    }
}
