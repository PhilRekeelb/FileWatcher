using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher
{

    public partial class FileWatcher : ServiceBase
    {
        private static String EVENTSOURCE = "Filewatcher";
        private static String LOGNAME = "Application";

        public FileWatcher()
        {
            InitializeComponent();

            eventLog = new System.Diagnostics.EventLog();
            if(!System.Diagnostics.EventLog.Exists(EVENTSOURCE))
            {
                System.Diagnostics.EventLog.CreateEventSource(EVENTSOURCE, LOGNAME);
            }

            eventLog.Source = EVENTSOURCE;
            eventLog.Log = LOGNAME;

        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
