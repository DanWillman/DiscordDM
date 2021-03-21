using System;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using DSharpPlus.Entities;

public class MongoDataAccess
{
    private readonly string CollectionName = "dnd";
    private readonly string ConnectionString = "mongodb://dnd_mongo:27017";
    private readonly string DatabaseName = "dnd";

    private readonly IMongoCollection<PlayerHistory> collection;

    public MongoDataAccess()
    {
        var client = new MongoClient(ConnectionString);
        var database = client.GetDatabase(DatabaseName);

        collection = database.GetCollection<PlayerHistory>(CollectionName);
        collection.Indexes.CreateOne(new CreateIndexModel<PlayerHistory>(Builders<PlayerHistory>.IndexKeys.Ascending(hist => hist.User.Username)));
    }

    public List<PlayerHistory> GetPlayerHistories(DiscordGuild server) => collection.Find(x => x.Server.Id == server.Id).ToList();

    public List<PlayerHistory> GetAllInspiration(DiscordGuild server) => collection.Find(x => (x.Server.Id == server.Id && x.IsInspired.Value)).ToList();

    public PlayerHistory GetPlayerHistory(PlayerHistory history) =>
        collection.Find<PlayerHistory>(x => x.User.Id == history.User.Id && x.Server.Id == history.Server.Id).FirstOrDefault();

    public PlayerHistory GetPlayerHistory(DiscordUser user, DiscordGuild server) =>
        collection.Find<PlayerHistory>(x => x.User.Id == user.Id && x.Server.Id == server.Id).FirstOrDefault();

    public PlayerHistory UpsertHistory(PlayerHistory history) 
    {
        if(string.IsNullOrWhiteSpace(history.Id))
        {
            history.Id = ObjectId.GenerateNewId().ToString();
        }

        collection.ReplaceOne(x => x.User.Id == history.User.Id && x.Server.Id == history.Server.Id, history, new ReplaceOptions() { IsUpsert = true });
        return GetPlayerHistory(history);
    }

    public void DeleteHistory(PlayerHistory history) => collection.DeleteOne(h => h.User.Username == history.User.Username);
}