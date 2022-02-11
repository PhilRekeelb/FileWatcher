using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher
{
    class FileWatcher
    {
        private String sourcepath = @"D:\Downloads\";
        private String destinationpath = @"D:\Downloads\";
        private String filter = "*.zip";
        private String configurationPath;

        private System.IO.FileSystemWatcher fileSystemWatcher;
        private System.Diagnostics.EventLog eventLog;

        public FileWatcher(string configpath, System.Diagnostics.EventLog eventLog)
        {
            this.configurationPath = configpath;
            this.eventLog = eventLog;

            try
            { 
                ReadConfigFile();
            }
            catch(Exception ex)
            {
                eventLog.WriteEntry(ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            
        }

        public void ReadConfigFile()
        {
            if (System.IO.File.Exists(this.configurationPath))
            {
                // read JSON directly from a file
                using (System.IO.StreamReader file = System.IO.File.OpenText(this.configurationPath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    Newtonsoft.Json.Linq.JObject o2 = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.Linq.JToken.ReadFrom(reader);

                    this.sourcepath = o2["source"].ToString();
                    this.destinationpath = o2["destination"].ToString();
                    this.filter = o2["filter"].ToString();
                }
            }
        }

        public void SetupFileSystemWatcher()
        {
            if (System.IO.Directory.Exists(this.sourcepath))
            {
                this.fileSystemWatcher = new System.IO.FileSystemWatcher();
                fileSystemWatcher.Path = this.sourcepath;
                fileSystemWatcher.Filter = this.filter;
                fileSystemWatcher.EnableRaisingEvents = true;
                fileSystemWatcher.NotifyFilter = System.IO.NotifyFilters.LastAccess | System.IO.NotifyFilters.LastWrite;
                fileSystemWatcher.Changed += new System.IO.FileSystemEventHandler(this.fileSystemWatcher_Changed);
                fileSystemWatcher.Created += new System.IO.FileSystemEventHandler(this.fileSystemWatcher_Created);
            }
        }

        private void fileSystemWatcher_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            eventLog.WriteEntry("File '" + e.FullPath + "' changed!");

            CreateDestinationPath();

            if (!IsFileLocked(e.FullPath))
            {
                MoveFileToDestination(e);
            }
        }

        private void fileSystemWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            eventLog.WriteEntry("File '" + e.FullPath + "' changed!");

            CreateDestinationPath();

            if (!IsFileLocked(e.FullPath))
            {
                MoveFileToDestination(e);
            }
        }

        private void CreateDestinationPath()
        {
            if (!System.IO.Directory.Exists(destinationpath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(destinationpath);
                }
                catch (Exception ex)
                {
                    eventLog.WriteEntry(ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
                }
            }
        }
        private void MoveFileToDestination(System.IO.FileSystemEventArgs e)
        {
            try
            {
                if (System.IO.File.Exists(destinationpath + e.Name))
                    System.IO.File.Delete(destinationpath + e.Name);

                System.IO.File.Move(e.FullPath, destinationpath + e.Name);
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry(ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
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
            catch (System.IO.IOException)
            {
                Locked = true;
            }
            return Locked;
        }

        override public String ToString()
        {
            return this.configurationPath + ":\n\nsource: '" + this.sourcepath + "'\ndestination: '" + this.destinationpath + "'\nfilter: '" + this.filter + "'";
        }
    }
}
