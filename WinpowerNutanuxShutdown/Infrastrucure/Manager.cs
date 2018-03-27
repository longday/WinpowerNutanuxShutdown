using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace WinpowerNutanuxShutdown.Infrastrucure
{
    public class Manager
    {
        private readonly Config _config;
        private readonly Logger _logger;
        private readonly UpsController _upsController;
        private readonly NutanixController _nutanixController;

        public Manager()
        {
            var config = new LoggingConfiguration();
#if DEBUG
            var consoleTarget = new ColoredConsoleTarget
            {
                Layout = @"${date:format=HH\:mm\:ss} ${message}"
            };
            config.AddTarget("console", consoleTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));
#endif
            var fileTarget = new FileTarget
            {
                Layout = @"${date:format=HH\:mm\:ss} ${message}",
                BufferSize = 3000,
                AutoFlush = false
            };
            config.AddTarget("file", fileTarget);
            fileTarget.FileName = @"${basedir}/logs/${date:format=yy-MM}/${date:format=yy-MM-dd}.txt";
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, fileTarget));
            LogManager.Configuration = config;
            _logger = LogManager.GetCurrentClassLogger();


            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", true)
                .AddJsonFile("settings.current.json", true)
                .Build()
                .Get<Config>();

            if (_config.UpsUrls.Any() == false || _config.CheckIntervalSec < 1 || _config.LowBattaryPercent < 1)
            {
                _logger.Fatal("Got config exception.");
                throw new Exception("Config error");
            }

            _upsController = new UpsController(_config);
            _nutanixController = new NutanixController(_config);
            //_logger.Info("Started with config: " + Newtonsoft.Json.JsonConvert.SerializeObject(_config));
        }

        public void Run()
        {
            _upsController.ReadUps();
            while (_upsController.ShouldShutdown() == false)
            {
                Thread.Sleep(_config.CheckIntervalSec * 1000);
                _upsController.ReadUps();
            } 
            Shutdown();
        }

        private void Shutdown()
        {
            _logger.Info("Shutdown Starting!");

            _logger.Info("Stage 1: shuting down Vms");
            while (_nutanixController.ShutdownVms() != true)
            {
                //alert stage 1 rerun
                _logger.Info("Stage 1: fail, sleeping 60 sec");
                Thread.Sleep(60 * 1000);
                _upsController.ReadUps();
            }
            //alert stage 1 complete
            _upsController.ReadUps();

            _logger.Info("Stage 2: stop cluster");
            while (_nutanixController.StopCluster() != true)
            {
                //alert stage 2 rerun
                _logger.Info("Stage 2: fail, sleeping 60 sec");
                Thread.Sleep(60 * 1000);
                _upsController.ReadUps();
            }
            //alert stage 2 complete
            _upsController.ReadUps();

            _logger.Info("Stage 3: stop cvm and node");
            _nutanixController.ShutdownNodesAndCvms();
            //alert stage 3 complete
            _upsController.ReadUps();
        }
    }
}
