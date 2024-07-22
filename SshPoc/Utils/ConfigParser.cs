﻿using System;
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
        //private string _filepath = @"test_limits_config.yaml";
        private string _filepath = @"C:\Users\SN Test 2\source\repos\RemoteHealthMonitor\SshPoc\Utils\test_limits_config.yaml";

        public ConfigParser()
        {
            string yaml = File.ReadAllText(_filepath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            TestLimits = deserializer.Deserialize<TestLimits>(yaml);

            Debug.WriteLine($"GPU Burn.Command = {TestLimits.GpuBurn.CommandToRun}");
            Debug.WriteLine($"GPU Burn.Max Utilization = {TestLimits.GpuBurn.MatrixMulResult}");
            Debug.WriteLine($"ASAPP.Command = {TestLimits.Asapp.CommandToRun}");
            Debug.WriteLine($"ASAPP.Max Temperature = {TestLimits.Asapp.MaxTemp}");
            Debug.WriteLine($"WiFi Flood.Command = {TestLimits.WifiFlooding.CommandToRun}");
            Debug.WriteLine($"WiFi Flood.TimeoutinMs= {TestLimits.WifiFlooding.TimeoutinMs}");
            Debug.WriteLine($"LatencyLoopback.Command= {TestLimits.LatencyLoopback.CommandToRun}");
            Debug.WriteLine($"LatencyInjector.Command= {TestLimits.LatencyInjector.CommandToRun}");
            Debug.WriteLine($"LatencyAnalyzer.Command= {TestLimits.LatencyAnalyzer.CommandToRun}");
            Debug.WriteLine($"LatencyAnalyzer.MaxFrameLatency= {TestLimits.LatencyAnalyzer.MaxFrameLatency}");
        }

        public TestLimits TestLimits { get; set; }
    }

    public class TestLimits
    {
        public GpuBurn GpuBurn { get; set; }
        public Asapp Asapp { get; set; }
        public WifiFlooding WifiFlooding { get; set; }
        public LatencyLoopback LatencyLoopback { get; set; }
        public LatencyInjector LatencyInjector { get; set; }
        public LatencyAnalyzer LatencyAnalyzer { get; set; }
    }

    public class GpuBurn
    {
        public string MatrixMulResult { get; set; }
        public int SleepInMs { get; set; }
        public string CommandToRun { get; set; }
    }

    public class Asapp
    {
        public int MaxTemp { get; set; }
        public double DeltaTempPerMeasurement { get; set; }
        public string CommandToRun { get; set; }
    }

    public class WifiFlooding
    {
        public int TimeoutinMs { get; set; }
        public string CommandToRun { get; set; }
    }

    public class LatencyLoopback
    {
        public string CommandToRun { get; set; }
    }

    public class LatencyInjector
    {
        public string CommandToRun { get; set; }
    }

    public class LatencyAnalyzer
    {
        public int MaxFrameLatency { get; set; }
        public string CommandToRun { get; set; }
    }

}
