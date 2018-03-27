using System.Collections.Generic;

namespace WinpowerNutanuxShutdown.Infrastrucure
{
    public class Config
    {
        public int CheckIntervalSec { get; set; }
        public List<string> UpsUrls { get; set; }
        public int LowBattaryPercent { get; set; }
        public int VmGracefulShutdownTimeoutSec { get; set; }
        public NutanixSshCommands NutanixSshCommands { get; set; }
        public List<NodeConfig> RootNodes { get; set; }
        public List<NodeConfig> CvmNodes { get; set; }
    }

    public class NutanixSshCommands
    {
        public string RunningVms { get; set; }
        public string GracefulShutdownVms { get; set; }
        public string ForceShutdownVms { get; set; }
        public string ClusterStatus { get; set; }
        public string ClusterStop { get; set; }
        public string CvmShutdown { get; set; }
        public string NodeShutdown { get; set; }
    }

    public class NodeConfig
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
    }
    
}
