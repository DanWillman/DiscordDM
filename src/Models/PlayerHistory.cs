using System;
using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class PlayerHistory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id {get;set;}
    
    public DiscordUser User { get; set; }
    public int Criticals {get;set;}
    public int Fumbles {get;set;}

    public DiscordGuild Server {get;set;}

    public bool? IsInspired {get;set;}

    public DateTime InspiriationDate {get;set;} 

    public string InspirationReason {get;set;}

    public PlayerHistory(DiscordUser user, DiscordGuild server)
    {
        User = user;
        Server = server;
    }
}