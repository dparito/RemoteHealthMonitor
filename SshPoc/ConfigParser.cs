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
            TestLimits = deserializer.Deserialize<TestLimits>(yaml);

            Debug.WriteLine($"GPU Burn.Max Utilization = {TestLimits.GpuBurn.MatrixMulResult}");
            Debug.WriteLine($"ASAPP.Max Temperature = {TestLimits.Asapp.MaxTemp}");
            Debug.WriteLine($"WiFi Flood.Timeout Phrase = {TestLimits.WifiFlooding.TimeoutPhrase}");
            Debug.WriteLine($"Latency.Max Frame Latency = {TestLimits.Latency.MaxFrameLatency}");
        }

        public TestLimits TestLimits { get; set; }
    }

    public class TestLimits
    {
        public GpuBurn GpuBurn { get; set; }
        public Asapp Asapp { get; set; }
        public WifiFlooding WifiFlooding { get; set; }
        public Latency Latency { get; set; }
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
