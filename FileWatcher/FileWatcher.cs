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

namespace FileWatcher
{
    //https://docs.microsoft.com/de-de/dotnet/framework/windows-services/walkthrough-creating-a-windows-service-application-in-the-component-designer
    public partial class FileWatcher : ServiceBase
    {
        private static String EVENTSOURCE = "FileWatcher";
        private static String LOGNAME = "Application";
        private static String SOURCEPATH = @"D:\Downloads";
        private static String DESTINATIONPATH = @"D:\Downloads\Spiele";

        public FileWatcher()
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

            if (System.IO.Directory.Exists(SOURCEPATH))
            {
                fileSystemWatcher.Path = SOURCEPATH;
            }

            eventLog.WriteEntry("Filewatcher started!");
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            eventLog.WriteEntry("Filewatcher terminated!");
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        private void fileSystemWatcher_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            eventLog.WriteEntry("File '" + e.FullPath + "' created!");

        }

        private void fileSystemWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            eventLog.WriteEntry("File '" + e.FullPath + "' changed!");
            if (!System.IO.Directory.Exists(DESTINATIONPATH))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(DESTINATIONPATH);
                }
                catch(Exception ex)
                {
                    eventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
                }
            }

            if (!IsFileLocked(e.FullPath))
            { 
                try
                {
                    if (System.IO.File.Exists(DESTINATIONPATH + @"\" + e.Name))
                        System.IO.File.Delete(DESTINATIONPATH + @"\" + e.Name);

                    System.IO.File.Move(e.FullPath, DESTINATIONPATH + @"\" + e.Name);
                }
                catch (Exception ex)
                {
                    eventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
                }
            }
        }
        public bool IsFileLocked(string filename)
        {
            bool Locked = false;
            try
            {
                System.IO.FileStream fs =
                            System.IO.File.Open(filename, System.IO.FileMode.OpenOrCreate,
                            System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                fs.Close();
            }
            catch (System.IO.IOException ex)
            {
                Locked = true;
            }
            return Locked;
        }
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
