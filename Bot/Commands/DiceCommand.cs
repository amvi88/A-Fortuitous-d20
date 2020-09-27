using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FortuitousD20.Commands
{
    public class DiceCommand
    {
        private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
        private Regex DiceRegex = new Regex(@"(?<numberOfDice>\d*)d(?<numberOfSides>\d*)(k(?<order>h|l)(?<selectionNumber>(\d*)))?", RegexOptions.Compiled);

        [Command("roll")]
        [Description("Roll the dice(s)!")]
        public async Task Roll(CommandContext ctx, [RemainingText, Description("The dice to roll.")] string rollConfiguration)
        {
            await ctx.TriggerTypingAsync();
            try 
            {
                var diceResults = GetRolls(rollConfiguration);
                await ctx.RespondAsync($"`[{string.Join(',', diceResults)}] => `**[{diceResults.Sum()}]**");
            }
            catch(ArgumentException)
            {
                await ctx.RespondAsync("Sorry I don´t understand try 1d20 or 1d20kh3 for instance");
            }
        }

        protected IEnumerable<int> GetRolls(string rollConfiguration)
        {
            if (string.IsNullOrWhiteSpace(rollConfiguration))
            {
                throw new ArgumentException(nameof(rollConfiguration));
            }

            var match = DiceRegex.Match(rollConfiguration);
            if (!match.Success)
            {
                throw new ArgumentException(nameof(rollConfiguration));
            }

            var numberOfDices = byte.Parse(match.Groups["numberOfDice"].Value);
            var numberOfSides = byte.Parse(match.Groups["numberOfSides"].Value);
            var order = match.Groups.ContainsKey("order") ? match.Groups["order"].Value : null;
            var selectionNumber = match.Groups.ContainsKey("selectionNumber") ? int.Parse(match.Groups["selectionNumber"].Value) : (int?)null;
            var diceResults = Enumerable.Range(1, numberOfDices).Select(x => RollDice(numberOfSides)).Select(t => t.Result);

            if (!string.IsNullOrEmpty(order) && selectionNumber.HasValue)
            {
                diceResults = (order == "l") ? diceResults.OrderBy(x => x) : diceResults.OrderByDescending(x => x);
                diceResults = diceResults.Take(selectionNumber.Value);
            }

            return diceResults;
        }

        private async Task<int> RollDice(byte numberSides)
        {
            if (numberSides <= 0)
            {
                throw new ArgumentOutOfRangeException("numberSides");
            }

            // Create a byte array to hold the random value.
            byte[] randomNumber = new byte[1];
            do
            {
                // Fill the array with a random value.
                rngCsp.GetBytes(randomNumber);
            }
            while (!IsFairRoll(randomNumber[0], numberSides));
            // Return the random number mod the number
            // of sides.  The possible values are zero-
            // based, so we add one.
            return (int)((randomNumber[0] % numberSides) + 1);
        }

        private bool IsFairRoll(byte roll, byte numSides)
        {
            int fullSetsOfValues = Byte.MaxValue / numSides;
            return roll < numSides * fullSetsOfValues;
        }
    }
}
