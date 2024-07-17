using System.Net;
using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Drawing;
using YamlDotNet.Core;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

class Program
{
    static void Main(string[] args)
    {
        string path = Directory.GetCurrentDirectory();
        
        var filepath = @"../../../../SshPoc/Utils/test_limits_config.yaml";
        var testLimits = new TestLimits
        {
            GpuBurn = new GpuBurn { CommandToRun = "echo Allspark | sudo -S /home/allspark/ServiceApps/gpuBurn.sh", 
                                    MatrixMulResult = "FAIL" },
            Asapp = new Asapp { CommandToRun = "echo Allspark | sudo -S /home/allspark/serviceApps/asapp -O -M -T",
                                MaxTemp = 60 },
            WifiFlooding = new WifiFlooding { CommandToRun = "ping -i 0.2 -s 65507 192.168.1.1",
                                              TimeoutinMs = 100 },
            Latency = new Latency { CommandToRun = "echo Allspark | sudo -S /home/allspark/ServiceApps/loopback.sh",
                                    MaxFrameLatency = 3 },
        };

        using var writer = new StreamWriter(filepath);

        /*SERIALIZE*/
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        serializer.Serialize(writer: writer, graph: testLimits);
        Thread.Sleep(2000);
        writer.Flush();
        writer.Close();
        writer.Dispose();
        Console.WriteLine("Done");

        Thread.Sleep(2000);

        /*DESERIALIZE*/
        string yaml = File.ReadAllText(filepath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var limitsBack = deserializer.Deserialize<TestLimits>(yaml);

        Console.WriteLine($"GPU Burn.Command = {limitsBack.GpuBurn.CommandToRun}");
        Console.WriteLine($"GPU Burn.Max Utilization = {limitsBack.GpuBurn.MatrixMulResult}");

        Console.WriteLine($"ASAPP.Command = {limitsBack.Asapp.CommandToRun}");
        Console.WriteLine($"ASAPP.Max Temperature = {limitsBack.Asapp.MaxTemp}");

        Console.WriteLine($"WiFi Flood.Command = {limitsBack.WifiFlooding.CommandToRun}");
        Console.WriteLine($"WiFi Flood.TimeoutinMs= {limitsBack.WifiFlooding.TimeoutinMs}");
        
        Console.WriteLine($"Latency.Command= {limitsBack.Latency.CommandToRun}");
        Console.WriteLine($"Latency.Max Frame Latency = {limitsBack.Latency.MaxFrameLatency}");
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
    public string CommandToRun { get; set; }
}

class Asapp
{
    public int MaxTemp { get; set; }
    public string CommandToRun { get; set; }
}

class WifiFlooding
{
    public int TimeoutinMs { get; set; }
    public string CommandToRun { get; set; }
}

class Latency
{
    public int MaxFrameLatency { get; set; }
    public string CommandToRun { get; set; }
}

