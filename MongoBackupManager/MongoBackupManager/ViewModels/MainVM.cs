using System;
using System.IO;

namespace MongoBackupManager
{
    public class MainVM : VMPageBase
    {
        public static MainVM Instance { get; private set; }
        
        private static string _logFilePath
        {
            get { return string.Format("{0}\\{1}", _appDataFolder, "MongoDBBackupLog.txt"); }
        }

        public MainVM()
        {
            _settings = new SettingsVM();
            Instance = this;
        }

        public override void Initialize()
        {
            Settings = SettingsVM.Load();
            if (Settings == null)
                Settings = new SettingsVM();
            else
                Settings.IsPropertiesInitialized = true;

            try
            {
                if (File.Exists(_logFilePath))
                    Log = File.ReadAllText(_logFilePath);
            }
            catch (Exception ex)
            {
                AddToLog(string.Format("Getting file: {0}, Error: {1}", _logFilePath, ex.Message));
            }

            Settings.Initialize();
            base.Initialize();
        }
        public override void Suspend()
        {
            File.WriteAllText(_logFilePath, Log);

            Settings.Suspend();
            base.Suspend();
        }

        #region Functions
        public void AddToLog(string value)
        {
            Log = string.Format("{0} | {1}.{2}{2}{3}",
                DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), value, LINE_BREAK, _log);
        }
        #endregion
        #region Properties
        private string _log;
        public string Log
        {
            get { return _log; }
            set
            {
                if (value != _log)
                {
                    _log = value;
                    NotifyPropertyChanged("Log");
                }
            }
        }

        private SettingsVM _settings;
        public SettingsVM Settings
        {
            get { return _settings; }
            set
            {
                if (value != _settings)
                {
                    _settings = value;
                    NotifyPropertyChanged("Settings");
                }
            }
        }
        #endregion
    }
}
