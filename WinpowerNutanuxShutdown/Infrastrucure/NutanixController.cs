using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using NLog;
using Renci.SshNet;

namespace WinpowerNutanuxShutdown.Infrastrucure
{
    class NutanixController
    {
        private readonly Logger _logger;
        private readonly Config _config;
        
        public NutanixController(Config config)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _config = config;
        }

        public bool ShutdownVms()
        {
            var sshClient = Connect(_config.CvmNodes).First();

            int CountRunningVms()
            {
                var runningVmList = ExecuteCommand(_config.NutanixSshCommands.RunningVms, sshClient);
                var runningVmsCount = runningVmList.Split("\n", StringSplitOptions.RemoveEmptyEntries).Length - 1;
                _logger.Info("VMs running: " + runningVmsCount);
                return runningVmsCount;
            }

            ExecuteCommand(_config.NutanixSshCommands.GracefulShutdownVms, sshClient);

            var sw = new Stopwatch();
            sw.Restart();

            _logger.Info("Graceful shutdown timeout, sec: " + _config.VmGracefulShutdownTimeoutSec);
            int runningVms;
            while (sw.ElapsedMilliseconds < _config.VmGracefulShutdownTimeoutSec * 1000)
            {
                Thread.Sleep(10 * 1000);
                runningVms = CountRunningVms();
                if (runningVms < 1)
                {
                    _logger.Info("No vms running, continue");
                    sshClient.Dispose();
                    return true;
                }
            }

            ExecuteCommand(_config.NutanixSshCommands.ForceShutdownVms, sshClient);
            Thread.Sleep(10 * 1000);
            runningVms = CountRunningVms();
            sshClient.Dispose();
            if (runningVms < 1)
            {
                _logger.Info("No vms running, continue");
                return true;
            }
            return false;
        }

        public bool StopCluster()
        {
            var sshClient = Connect(_config.CvmNodes).First();
            
            _logger.Info("Starting interactive shell...");
            var shell = sshClient.CreateShellStream("xterm", 160, 24, 800, 600, 1024 * 16);
            _logger.Info("Shell write: " + _config.NutanixSshCommands.ClusterStop);
            shell.WriteLine(_config.NutanixSshCommands.ClusterStop);
            var res = shell.Expect("):");
            _logger.Info(res);
            shell.WriteLine("Y");
            res = shell.Expect("INFO cluster:");
            _logger.Info(res);
            shell.Close();

            var clusterStatus = ExecuteCommand(_config.NutanixSshCommands.ClusterStatus, sshClient);
            sshClient.Dispose();
            if (clusterStatus.Contains("stop"))
            {
                _logger.Info("Cluster not running, continue");
                return true;
            }
            return false;
        }

        public bool ShutdownNodesAndCvms()
        {
            var sshClients = Connect(_config.RootNodes, true);
            /*
               virsh shutdown `virsh list --name`
               Domain NTNX-41e78849-A-CVM is being shutdown
               error: command 'shutdown' requires <domain> option
            */
            foreach (var sshClient in sshClients)
            {
                _logger.Info("Shutdown Host/Cvm : " + sshClient.ConnectionInfo.Host);
                ExecuteCommand(_config.NutanixSshCommands.CvmShutdown, sshClient);
                ExecuteCommand(_config.NutanixSshCommands.NodeShutdown, sshClient);
            }
            return true;
        }

        private List<SshClient> Connect(IEnumerable<NodeConfig> nodeConfigs, bool connectAll = false)
        {
            var list = new List<SshClient>();
            foreach (var node in nodeConfigs)
            {
                try
                {
                    _logger.Info("Ssh connecting to: " + node.Host);
                    var ssh = new SshClient(new ConnectionInfo(node.Host, node.Login, new PasswordAuthenticationMethod(node.Login, node.Password)){Timeout = TimeSpan.FromSeconds(7)});
                    ssh.Connect();
                    _logger.Info($"Ssh {node.Login}@{node.Host} connected");
                    list.Add(ssh);
                    if (connectAll == false)
                    {
                        break;
                    }
                }
                catch (Exception re)
                {
                    _logger.Info($"connection failed, host: {node.Host}, ex: {re.Message}");
                }
            }

            return list;
        }
        private string ExecuteCommand(string cmd, SshClient client)
        {
            _logger.Info("Executing: " + cmd);
            var result = client.RunCommand(cmd).Result.Trim();
            _logger.Info(result);
            return result;
        }
    }
}
