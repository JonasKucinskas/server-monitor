using System;
using System.IO;
using Newtonsoft.Json;

public class Deserealizer
{
    public static DataPackage Deserealize(string json)
    {
        return JsonConvert.DeserializeObject<DataPackage>(json);;
    }
}