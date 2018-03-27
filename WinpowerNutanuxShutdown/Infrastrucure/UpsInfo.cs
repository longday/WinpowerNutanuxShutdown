using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WinpowerNutanuxShutdown.Infrastrucure
{
    class UpsInfo
    {
        private static readonly Regex CleanNumbers = new Regex("\\D");


        public int BatCapacityInt
        {
            get
            {
                try
                {
                    var clean = CleanNumbers.Replace(BatCapacity, "");
                    return int.Parse(clean);
                }
                catch (Exception)
                {
                    return -1;
                }
            }
        }

        //status "Normal"
        //status "AC Fail, Discharge"
        public bool IsDischarging => Status.ToLower().Contains("discharge") || Status.ToLower().Contains("ac fail");

        public string Key { get; set; }
        public UpsDevice Device { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }
        public string Model { get; set; }
        public string LoadPercent { get; set; }
        public int LoadPercentMax { get; set; }
        public string BatTimeRemain { get; set; }
        public string BatCapacity { get; set; }
        public string CfgBatNumber { get; set; }
        public string RedundantNumber { get; set; }
        public bool SupportTest { get; set; }
        public string LoadSegment2State { get; set; }
        public int StatusColor { get; set; }
        public string EmdHumidity { get; set; }
        public string LoadSegment1State { get; set; }
        public bool NoModule { get; set; }
        public int ExtStatus { get; set; }
        public string LastEvent2 { get; set; }
        public string OutVa { get; set; }
        public string OutW { get; set; }
        public int IStatus { get; set; }
        public string LastEvent1 { get; set; }
        public string UpsTemp { get; set; }
        public int WorkMode { get; set; }
        public string EmdAlarm2 { get; set; }
        public string EmdTemp { get; set; }
        public string CfgKva { get; set; }
        public string EmdAlarm1 { get; set; }
        public string AbmState { get; set; }
        public string BatTemp { get; set; }
        public string OutA { get; set; }
        public string InVolt { get; set; }
        public string OutFreq { get; set; }
        public string InFreq { get; set; }
        public int Ls2 { get; set; }
        public string BatV { get; set; }
        public string OutVolt { get; set; }
        public string BypassFreq { get; set; }
        public int OidType { get; set; }
        public int Ls1 { get; set; }
        public string BypassVolt { get; set; }
        public string Warning { get; set; }
    }
    public class UpsDevice
    {
        public string Key { get; set; }
        public int Id { get; set; }
        public int Protocol { get; set; }
        public int PortIndex { get; set; }
        public object Ip { get; set; }
        public string Status { get; set; }
        public int UpsIndex { get; set; }
        public string StatusIcon { get; set; }
        public bool HasWarn { get; set; }
    }
}
