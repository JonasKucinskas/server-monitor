using monitor;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public class SystemData
{
    public int id { get; set; }
    public string ip { get; set; }
    public int port { get; set; }
    public string name { get; set; }
    public int ownerId { get; set; }
    public DateTime creationDate { get; set; }
}