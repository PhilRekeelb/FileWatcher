using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace FileWatcher
{
    //https://docs.microsoft.com/de-de/dotnet/framework/windows-services/walkthrough-creating-a-windows-service-application-in-the-component-designer
    public partial class FileWatcherService : ServiceBase
    {
        private static String EVENTSOURCE = "FileWatcher";
        private static String LOGNAME = "Application";

        private static String CONFIGPATH = @"C:\Temp\";

        private List<FileWatcher> fileWatchers;

        public FileWatcherService()
        {
            InitializeComponent();

            InitializeEventLog();

        }
        private void InitializeEventLog()
        {
            try
            {
                CreateAndAssignEventSource();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(LOGNAME, ex.ToString(), EventLogEntryType.Error);
            }
        }

        private void CreateAndAssignEventSource()
        {
            eventLog = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists(EVENTSOURCE))
            {
                System.Diagnostics.EventLog.CreateEventSource(EVENTSOURCE, LOGNAME);
            }

            eventLog.Source = EVENTSOURCE;
            eventLog.Log = LOGNAME;
        }

        protected override void OnStart(string[] args)
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);


            if (System.IO.Directory.Exists(CONFIGPATH))
            {
                String[] ConfigurationFiles = System.IO.Directory.GetFiles(CONFIGPATH, "*.json");

                if (ConfigurationFiles.Length > 0)
                {
                    foreach (String file in ConfigurationFiles)
                    {
                        try
                        {
                            FileWatcher fileWatcher = new FileWatcher(file, eventLog);
                            
                            eventLog.WriteEntry(fileWatcher.ToString());

                            fileWatcher.SetupFileSystemWatcher();

                            fileWatchers.Add(fileWatcher);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
        }

        protected override void OnStop()
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

    }

    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };
}
