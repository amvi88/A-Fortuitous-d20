using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

public class StatusCommand
{
    [Command("status")]
    [Description("Simple command tocheck the status of the bot!")]
    public async Task Alive(CommandContext ctx)
    {
        await ctx.TriggerTypingAsync();
        await ctx.RespondAsync("I'm alive!");
    }
}