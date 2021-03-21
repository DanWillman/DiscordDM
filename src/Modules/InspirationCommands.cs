using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

public class InspirationCommands : IModule
{
    private MongoDataAccess mongo = new MongoDataAccess();

    [Command("inspire")]
    [Description("Used to give inspiration to a given player")]
    public async Task Inspire(CommandContext ctx, 
                                [Description("User that earned inspiration")]DiscordUser user, 
                                [Description("What the user did to earn this. "), RemainingText]string message)
    {
        try
        {
            var history = mongo.GetPlayerHistory(user, ctx.Guild);
            var allUsers = await ctx.Guild.GetAllMembersAsync();
            var guildUser = allUsers.Where(x => x.Id == user.Id).FirstOrDefault();
            
            if(history == null)
            {
                history = new PlayerHistory(user, ctx.Guild);
            }
            if (history.IsInspired != null && history.IsInspired.Value)
            {
                await ctx.RespondAsync($"{guildUser.Nickname} already has inspiration since {history.InspiriationDate.ToString("G")} for `{history.InspirationReason}`");
                return;
            }

            history.InspirationReason = message;
            history.InspiriationDate = DateTime.Now;
            history.IsInspired = true;

            mongo.UpsertHistory(history);

            await ctx.RespondAsync($"Ok! {guildUser.Mention} you are hereby inspired! Use it wisely");
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"Something went wrong {DiscordEmoji.FromName(ctx.Client, ":thinking:")} - {ex}");
        }
        
    }

    [Command("discourage")]
    [Description("Remove inspiration from the player")]
    public async Task RemoveInspiration(CommandContext ctx,
                                            [Description("User losing inspiration")]DiscordUser user)
    {
        try
        {
            var history = mongo.GetPlayerHistory(user, ctx.Guild);
            var allUsers = await ctx.Guild.GetAllMembersAsync();
            var guildUser = allUsers.Where(x => x.Id == user.Id).FirstOrDefault();

            if(history == null)
            {
                history = new PlayerHistory(user, ctx.Guild);
            }
            if ((history.IsInspired.HasValue && !history.IsInspired.Value) || !history.IsInspired.HasValue)
            {
                await ctx.RespondAsync($"I couldn't remove inspiration from {guildUser.Nickname} because they don't have any. Maybe the DM should be more generous?");
                return;
            }

            history.IsInspired = false;
            history.InspiriationDate = DateTime.MinValue;
            history.InspirationReason = string.Empty;

            mongo.UpsertHistory(history);

            await ctx.RespondAsync($"Ok, {guildUser.Nickname} no longer has inspiration. Hope it was worth it {guildUser.Mention}");
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"Something went wrong {DiscordEmoji.FromName(ctx.Client, ":thinking:")} - {ex}");
        }
        
    }

    [Command("inspirations")]
    [Description("List all inspirations currently active on the server")]
    public async Task GetInspirations(CommandContext ctx)
    {
        var inspirations = mongo.GetAllInspiration(ctx.Guild);
        StringBuilder sb = new StringBuilder();

        try
        {
            if (inspirations.Count > 0)
        {
            foreach(var insp in inspirations)
            {
                sb.Append($"{insp.User.Mention} earned inspiration on {insp.InspiriationDate.ToString("G")} for `{insp.InspirationReason}`{System.Environment.NewLine}");
            }

            await ctx.RespondAsync($"Ok, here's what I found...");
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(sb.ToString());
        }
        else
        {
            await ctx.RespondAsync($"There's no inspirations on this server {DiscordEmoji.FromName(ctx.Client, ":thinking:")}");
        }
        }
        catch (Exception ex) 
        {
            await ctx.RespondAsync($"Something went wrong {DiscordEmoji.FromName(ctx.Client, ":thinking:")} - {ex}");
        }

    }
}