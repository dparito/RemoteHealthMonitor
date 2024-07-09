using System.Net;
using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Drawing;
using YamlDotNet.Core;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        var filepath = @"C:\Users\30793978\source\repos\RemoteHealthMonitor\SshPoc\Utils\test_limits_config.yaml";
        var isSerialized = false;
        if (!isSerialized)
        {
            /*** Example 
            // Sample object
            var person = new Person
            {
                Name = "Abe Lincoln",
                Age = 25,
                HeightInInches = 6f + 4f / 12f,
                Addresses = new Dictionary<string, Address>{
                { "home", new  Address() {
                    Street = "2720  Sundown Lane",
                    City = "Kentucketsville",
                    State = "Calousiyorkida",
                    Zip = "99978",
                }},
                { "work", new  Address() {
                    Street = "1600 Pennsylvania Avenue NW",
                    City = "Washington",
                    State = "District of Columbia",
                    Zip = "20500",
                }},
            }
            };

            using var writer = new StreamWriter(filepath);

            // Serialize
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            serializer.Serialize(writer: writer, graph: person);
            ***/

            var testLimits = new TestLimits
            {
                GpuBurn = new GpuBurn { MatrixMulResult = "FAIL" },
                Asapp = new Asapp { MaxTemp = 65 },
                WifiFlooding = new WifiFlooding { TimeoutPhrase = "timeout" },
                Latency = new Latency { MaxFrameLatency = 3 },
            };

            using var writer = new StreamWriter(filepath);

            // Serialize
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            serializer.Serialize(writer: writer, graph: testLimits);
            
        }
        else
        {
            /*** Example
            string yaml = File.ReadAllText(filepath);
            // Deserialize
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var personBack = deserializer.Deserialize<Person>(yaml);
            Console.WriteLine($"{personBack.Name}");
            Console.WriteLine($"{personBack.Age}");
            Console.WriteLine($"{personBack.HeightInInches}");
            Console.WriteLine($"{personBack.Addresses["home"].Street}");
            Console.WriteLine($"{personBack.Addresses["home"].City}");
            Console.WriteLine($"{personBack.Addresses["home"].State}");
            Console.WriteLine($"{personBack.Addresses["home"].Zip}");
            Console.WriteLine($"{personBack.Addresses["work"].Street}");
            Console.WriteLine($"{personBack.Addresses["work"].City}");
            Console.WriteLine($"{personBack.Addresses["work"].State}");
            Console.WriteLine($"{personBack.Addresses["work"].Zip}");
            ***/

            string yaml = File.ReadAllText(filepath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var limitsBack = deserializer.Deserialize<TestLimits>(yaml);

            Debug.WriteLine($"GPU Burn.Max Utilization = {limitsBack.GpuBurn.MatrixMulResult}");
            Debug.WriteLine($"ASAPP.Max Temperature = {limitsBack.Asapp.MaxTemp}");
            Debug.WriteLine($"WiFi Flood.Timeout Phrase = {limitsBack.WifiFlooding.TimeoutPhrase}");
            Debug.WriteLine($"Latency.Max Frame Latency = {limitsBack.Latency.MaxFrameLatency}");
        }
    }
}

/*** Example
class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public float HeightInInches { get; set; }
    public Dictionary<string, Address> Addresses { get; set; }
}

class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }
}
***/

class TestLimits
{
    public GpuBurn GpuBurn { get; set; }
    public Asapp Asapp { get; set; }
    public WifiFlooding WifiFlooding { get; set; }
    public Latency Latency { get; set; }
}

class GpuBurn
{
    public string MatrixMulResult { get; set; }
}

class Asapp
{
    public int MaxTemp { get; set; }
}

class WifiFlooding
{
    public string TimeoutPhrase { get; set; }
}

class Latency
{
    public int MaxFrameLatency { get; set; }
}

