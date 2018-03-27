using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using RestSharp;

namespace WinpowerNutanuxShutdown.Infrastrucure
{
    class UpsController
    {
        private readonly Logger _logger;
        private readonly Config _config;
        private static readonly List<UpsInfo> PreviousUpsInfo = new List<UpsInfo>();
        private static readonly List<UpsInfo> CurrentUpsInfo = new List<UpsInfo>();
        

        public UpsController(Config config)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _config = config;
        }

        public void ReadUps()
        {
            PreviousUpsInfo.Clear();
            PreviousUpsInfo.AddRange(CurrentUpsInfo);
            CurrentUpsInfo.Clear();

            foreach (var upsUrl in _config.UpsUrls)
            {
                try
                {
                    var restClient = new RestClient(upsUrl);
                    var request = new RestRequest(Method.GET);
                    var response = restClient.Execute<UpsInfo>(request);
                    CurrentUpsInfo.Add(response.Data);
                    LogUps(response.Data, _config.UpsUrls.IndexOf(upsUrl).ToString());
                }
                catch (Exception e)
                {
                    _logger.Error($"Ups data fetch ({upsUrl}) error: " + e.Message);
                }
            }
        }
        public void LogUps(UpsInfo info, string id)
        {
            var logMessage = "";
            var lastInfo = PreviousUpsInfo.FirstOrDefault(c => c.Key == info.Key) ?? new UpsInfo();

            if (info.Status != lastInfo.Status)
            {
                logMessage += $"status:{info.Status}; ";
            }
            if (info.BatCapacity != lastInfo.BatCapacity)
            {
                logMessage += $"bat:{info.BatCapacity}; ";
            }
            if (info.BatV != lastInfo.BatV)
            {
                logMessage += $"bat:{info.BatV}; ";
            }
            if (info.InVolt != lastInfo.InVolt)
            {
                logMessage += $"in:{info.InVolt}; ";
            }
            if (info.OutVolt != lastInfo.OutVolt)
            {
                logMessage += $"out:{info.OutVolt}; ";
            }
            if (info.OutW != lastInfo.OutW)
            {
                logMessage += $"out:{info.OutW}; ";
            }
            if (info.LastEvent1 != lastInfo.LastEvent1)
            {
                logMessage += $"last event 1: {info.LastEvent1}; ";
            }
            if (info.LastEvent2 != lastInfo.LastEvent2)
            {
                logMessage += $"last event 2: {info.LastEvent2}; ";
            }
            if (info.UpsTemp != lastInfo.UpsTemp)
            {
                logMessage += $"t: {info.UpsTemp}; ";
            }
            /*if (info.BatTimeRemain != lastInfo.BatTimeRemain)
            {
                logMessage += $" time remain: {info.BatTimeRemain}";
            }*/
            if (logMessage.Length > 0)
            {
                _logger.Info($"{id}:: {logMessage}");
            }
        }
        public bool ShouldShutdown()
        {
            if (CurrentUpsInfo.All(c => c.IsDischarging == false))
            {
                return false;
            }

            if (CurrentUpsInfo.All(c => c.BatCapacityInt > _config.LowBattaryPercent))
            {
                return false;
            }

            if (PreviousUpsInfo.All(c => c.BatCapacityInt > _config.LowBattaryPercent))
            {
                return false;
            }
            return true;
        }
    }
}
