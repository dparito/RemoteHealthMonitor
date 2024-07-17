using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SshPoc
{
    public class ConfigParser
    {
        private string _filepath = @"C:\Users\30793978\source\repos\RemoteHealthMonitor\SshPoc\Utils\test_limits_config.yaml";
        
        public ConfigParser()
        {
            string yaml = File.ReadAllText(_filepath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            Limits = deserializer.Deserialize<TestLimits>(yaml);

            Debug.WriteLine($"GPU Burn.Command to run = {Limits.GpuBurn.CommandToRun}");
            Debug.WriteLine($"GPU Burn.Max Utilization = {Limits.GpuBurn.TestLimit}");
            Debug.WriteLine($"ASAPP.Command to run = {Limits.Asapp.CommandToRun}");
            Debug.WriteLine($"ASAPP.Max Temperature = {Limits.Asapp.TestLimit}");
            Debug.WriteLine($"WiFi Flood.Command to run = {Limits.WifiFlooding.CommandToRun}");
            Debug.WriteLine($"WiFi Flood.Worst time diff = {Limits.WifiFlooding.TestLimit}");
            Debug.WriteLine($"Latency.Command to run = {Limits.Latency.CommandToRun}");
            Debug.WriteLine($"Latency.Max Frame Latency = {Limits.Latency.TestLimit}");
        }

        public TestLimits Limits { get; set; }

        public class TestLimits
        {
            public GpuBurnTestSessionModel GpuBurn { get; set; }
            public AsappTestSessionModel Asapp { get; set; }
            public WifiFloodTestSessionModel WifiFlooding { get; set; }
            public LatencyTestSessionModel Latency { get; set; }
        }
    }

    

    public class GpuBurn
    {
        public string MatrixMulResult { get; set; }
    }

    public class Asapp
    {
        public int MaxTemp { get; set; }
    }

    public class WifiFlooding
    {
        public string TimeoutPhrase { get; set; }
    }

    public class Latency
    {
        public int MaxFrameLatency { get; set; }
    }

}
