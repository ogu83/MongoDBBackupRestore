using System;

namespace MongoBackupManager
{
    public class MainVM : VMPageBase
    {
        public static MainVM Instance { get; private set; }

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

            Settings.Initialize();
            base.Initialize();
        }
        public override void Suspend()
        {
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
