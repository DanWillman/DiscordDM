using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

public class RollTrackerCommands : IModule
{
    private List<PlayerHistory> historyData = new List<PlayerHistory>();

    private MongoDataAccess mongo = new MongoDataAccess();
    
    [Command("fumble")]
    [Description("Attaches a fumble to the provided player- generally a natural 1")]
    public async Task Fumble(CommandContext ctx, 
        [Description("User that fumbled")]DiscordUser user)
    {
        try
        {
            ReadHistory(ctx.Guild);

            if (historyData == null)
            {
                historyData = new List<PlayerHistory>();
                var userHistory = new PlayerHistory(user, ctx.Guild);
                userHistory.Fumbles += 1;
                historyData.Add(userHistory);
            }
            else
            {
                var userHistory = historyData.Where(x => x.User == user).FirstOrDefault();
                if (userHistory == null)
                {
                    userHistory = new PlayerHistory(user, ctx.Guild);
                }
                else
                {
                    historyData.Remove(userHistory);
                }
                
                userHistory.Fumbles += 1;
                historyData.Add(userHistory);
            }

            UpdateHistory();

            await ctx.RespondAsync($"Hey {user.Mention}, looks like you fumbled. Good job!");
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"Whoops, something went wrong: {ex.Message}");
        }        
    }

    [Command("critical")]
    [Description("Attaches a critical to the provided player- generally a natural 20")]
    public async Task Critical(CommandContext ctx, 
            [Description("User that criticalled")]DiscordUser user)
    {
        try
        {
            ReadHistory(ctx.Guild);

            if (historyData == null)
            {
                historyData = new List<PlayerHistory>();
                var userHistory = new PlayerHistory(user, ctx.Guild);
                userHistory.Criticals += 1;
                historyData.Add(userHistory);
            }
            else
            {
                var userHistory = historyData.Where(x => x.User == user).FirstOrDefault();
                if (userHistory == null)
                {
                    userHistory = new PlayerHistory(user, ctx.Guild);
                }
                else
                {
                    historyData.Remove(userHistory);
                }
                
                userHistory.Criticals += 1;
                historyData.Add(userHistory);
            }

            UpdateHistory();

            await ctx.RespondAsync($"Look out for {user.Mention}, looks like we've got a badass over here");
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"Whoops, something went wrong: {ex.Message}");
        }
    }

    [Command("report")]
    [Description("Lists all criticals and fumbles for players on this server. ")]
    public async Task Report(CommandContext ctx)
    {
        ReadHistory(ctx.Guild);

        foreach(PlayerHistory hist in historyData.Where(x => x.Server == ctx.Guild))
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync($"Looks like {hist.User.Mention} has {hist.Criticals} criticals and {hist.Fumbles} fumbles");
        }

        await ctx.RespondAsync($"Ok, that's all my data. If something seems wrong, send more rum.");
    }

    private void UpdateHistory()
    {
        foreach(var history in historyData)
        {
            mongo.UpsertHistory(history);
        }
    }

    private void ReadHistory(DiscordGuild server)
    {
        try
        {
            historyData = mongo.GetPlayerHistories(server);
        }
        catch(Exception ex)
        {
            System.Console.WriteLine($"Encountered error reading data: {ex.GetType()} {System.Environment.NewLine}{ex.Message}");
        }
    }
}