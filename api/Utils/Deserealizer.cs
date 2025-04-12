using System;
using System.IO;
using Newtonsoft.Json;

public class Deserealizer
{
    public static T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}