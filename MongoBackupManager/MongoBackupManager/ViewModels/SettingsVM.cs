using Ionic.Zip;
using Ionic.Zlib;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;

namespace MongoBackupManager
{
    public class SettingsVM : VMPageBase
    {
        private DispatcherTimer _timer;
        private const int _timerInterval = 1; //in seconds
        private Process _backupProcess;
        private bool _backuping = false;

        public SettingsVM()
        {
            wireCommands();
            _backupFiles = new ObservableCollection<FileVM>();
        }

        public override void Initialize()
        {
            Host = "localhost";
            Port = "27017";
            MongodumpPath = @"C:\Program Files\MongoDB\bin\mongodump.exe";
            BackupPath = @"C:\MongoDBBackup\";
            IsPeriodicBackupOn = true;
            IsCompressOn = true;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(_timerInterval);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            _backupProcess = new Process();
            _backupProcess.ErrorDataReceived += _backupProcess_ErrorDataReceived;
            _backupProcess.Exited += _backupProcess_Exited;
            _backupProcess.OutputDataReceived += _backupProcess_OutputDataReceived;

            getFiles();

            base.Initialize();
        }
        public override void Suspend()
        {
            _timer.Stop();
            _timer.Tick -= _timer_Tick;

            _backupProcess.ErrorDataReceived -= _backupProcess_ErrorDataReceived;
            _backupProcess.Exited -= _backupProcess_Exited;
            _backupProcess.OutputDataReceived -= _backupProcess_OutputDataReceived;

            base.Suspend();
        }

        #region Commands
        public ICommand BackupCommand { get; private set; }
        private void wireCommands()
        {
            BackupCommand = new BaseCommand<object>(backup);
        }
        #endregion
        #region Functions
        void _timer_Tick(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now;
            if (_isPeriodicBackupOn) //Means daily backup action is affect
            {
                if (CurrentTime.Hour == 0 && CurrentTime.Minute == 0)
                {
                    if (!_backuping)
                    {
                        _backuping = true;
                        backup(null);
                    }
                }
                else
                {
                    _backuping = false;
                }
            }
        }

        private void backup(object param)
        {
            try
            {
                //--dbpath {4}
                _backupProcess.StartInfo = new ProcessStartInfo(_mongodumpPath,
                    string.Format("--host {0} --port {1} --username {2} --password {3} --out {4}{0}_{5}",
                    _host, _port, _dbUserName, _dbPassword, _backupPath, DateTime.Now.ToString("ddMMyyyy")));
                _backupProcess.Start();

                MainVM.Instance.AddToLog("Executing Backup Command with Arguments: "
                    + _backupProcess.StartInfo.Arguments);

                _backupProcess.WaitForExit();

                if (_isCompressOn)
                    compress();

                getFiles();
            }
            catch (Exception ex)
            {
                MainVM.Instance.AddToLog("Exception in backup: " + ex.Message);
            }
        }
        void _backupProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            MainVM.Instance.AddToLog("Backup Process Output: " + e.Data);
        }
        void _backupProcess_Exited(object sender, EventArgs e)
        {
            MainVM.Instance.AddToLog("Backup Process Exited");
        }
        void _backupProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            MainVM.Instance.AddToLog("Backup Process Error: " + e.Data);
        }

        private void restore(object param)
        {
            throw new NotImplementedException();
        }

        private void compress()
        {
            var zipPath = string.Format("{1}{0}_{2}", _host, _backupPath, DateTime.Now.ToString("ddMMyyyy"));
            var fileName = string.Format("{1}{0}_{2}.zip", _host, _backupPath, DateTime.Now.ToString("ddMMyyyy"));

            ZipFile f = new ZipFile(fileName);
            f.AddDirectory(zipPath);
            f.CompressionLevel = CompressionLevel.BestCompression;
            f.Save();

            Directory.Delete(zipPath, true);

            MainVM.Instance.AddToLog("Compression Completed");
        }
        private void decompress()
        {
            throw new NotImplementedException();
        }

        private void getFiles()
        {
            BackupFiles.Add(new FileVM { Name = "Lo", CreatedDate = DateTime.Now });
        }
        #endregion
        #region Properties
        private FileVM _selectedFile;
        public FileVM SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                if (value != _selectedFile)
                {
                    _selectedFile = value;
                    NotifyPropertyChanged("SelectedFile");
                }
            }
        }

        private ObservableCollection<FileVM> _backupFiles;
        public ObservableCollection<FileVM> BackupFiles
        {
            get { return _backupFiles; }
            set
            {
                if (value != _backupFiles)
                {
                    _backupFiles = value;
                    NotifyPropertyChanged("BackupFiles");
                }
            }
        }

        private DateTime _currentTime;
        public DateTime CurrentTime
        {
            get { return _currentTime; }
            set
            {
                if (value != _currentTime)
                {
                    _currentTime = value;
                    NotifyPropertyChanged("CurrentTime");
                    NotifyPropertyChanged("CurrentTimeStr");
                }
            }
        }
        public string CurrentTimeStr
        {
            get { return _currentTime.ToString("dd.MM.yyyy HH:mm:ss"); }
        }

        private string _backupPath;
        public string BackupPath
        {
            get { return _backupPath; }
            set
            {
                if (value != _backupPath)
                {
                    _backupPath = value;
                    NotifyPropertyChanged("BackupPath");
                }
            }
        }

        private string _databasePath;
        public string DatabasePath
        {
            get { return _databasePath; }
            set
            {
                if (value != _databasePath)
                {
                    _databasePath = value;
                    NotifyPropertyChanged("DatabasePath");
                }
            }
        }

        private bool _isCompressOn;
        public bool IsCompressOn
        {
            get { return _isCompressOn; }
            set
            {
                if (value != _isCompressOn)
                {
                    _isCompressOn = value;
                    NotifyPropertyChanged("IsCompressOn");
                }
            }
        }

        private bool _isPeriodicBackupOn;
        public bool IsPeriodicBackupOn
        {
            get { return _isPeriodicBackupOn; }
            set
            {
                if (value != _isPeriodicBackupOn)
                {
                    _isPeriodicBackupOn = value;
                    NotifyPropertyChanged("IsPeriodicBackupOn");
                }
            }
        }

        private string _mongodumpPath;
        public string MongodumpPath
        {
            get { return _mongodumpPath; }
            set
            {
                if (value != _mongodumpPath)
                {
                    _mongodumpPath = value;
                    NotifyPropertyChanged("MongodumpPath");
                }
            }
        }

        private string _host;
        public string Host
        {
            get { return _host; }
            set
            {
                if (value != _host)
                {
                    _host = value;
                    NotifyPropertyChanged("Host");
                }
            }
        }

        private string _port;
        public string Port
        {
            get { return _port; }
            set
            {
                if (value != _port)
                {
                    _port = value;
                    NotifyPropertyChanged("Port");
                }
            }
        }

        private string _dbUserName;
        public string DbUserName
        {
            get { return _dbUserName; }
            set
            {
                if (value != _dbUserName)
                {
                    _dbUserName = value;
                    NotifyPropertyChanged("DbUserName");
                }
            }
        }

        private string _dbPassword;
        public string DbPassword
        {
            get { return _dbPassword; }
            set
            {
                if (value != _dbPassword)
                {
                    _dbPassword = value;
                    NotifyPropertyChanged("DbPassword");
                }
            }
        }
        #endregion
    }
}