using Ionic.Zip;
using Ionic.Zlib;
using MongoDB.Driver;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace MongoBackupManager
{
    public class SettingsVM : VMPageBase
    {
        public SettingsVM()
        {
            wireCommands();

            _backupFiles = new ObservableCollection<FileVM>();
            _databases = new ObservableCollection<DatabaseVM>();
        }

        public override void Initialize()
        {
            if (!IsPropertiesInitialized)
            {
                Host = "localhost";
                Port = 27017;
                MongodumpPath = @"C:\Program Files\MongoDB\bin\mongodump.exe";
                MongorestorePath = @"C:\Program Files\MongoDB\bin\mongorestore.exe";
                BackupPath = @"C:\MongoDBBackup\";
                IsPeriodicBackupOn = true;
                IsCompressOn = true;
                IsPropertiesInitialized = true;
            }

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(_timerInterval);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            _backupProcess = new Process();
            _backupProcess.ErrorDataReceived += _backupProcess_ErrorDataReceived;
            _backupProcess.Exited += _backupProcess_Exited;
            _backupProcess.OutputDataReceived += _backupProcess_OutputDataReceived;

            _restoreProcess = new Process();
            _restoreProcess.ErrorDataReceived += _restoreProcess_ErrorDataReceived;
            _restoreProcess.Exited += _restoreProcess_Exited;
            _restoreProcess.OutputDataReceived += _restoreProcess_OutputDataReceived;

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

            _restoreProcess.ErrorDataReceived -= _restoreProcess_ErrorDataReceived;
            _restoreProcess.Exited -= _restoreProcess_Exited;
            _restoreProcess.OutputDataReceived -= _restoreProcess_OutputDataReceived;

            Save();

            base.Suspend();
        }

        #region Variables
        private static string _settingsFilePath
        {
            get { return string.Format("{0}\\{1}", _appDataFolder, "MongoDBBackupManagerSettings.xml"); }
        }

        private DispatcherTimer _timer;
        private const int _timerInterval = 1; //in seconds

        private const string _fileDateExtFormatStr = "ddMMyy_HHmmss";

        private Process _backupProcess;
        private Process _restoreProcess;

        private bool _backuping;
        #endregion
        #region Commands
        [XmlIgnore]
        public ICommand BackupCommand { get; private set; }
        [XmlIgnore]
        public ICommand RestoreCommand { get; private set; }
        [XmlIgnore]
        public ICommand DeleteCommand { get; private set; }
        [XmlIgnore]
        public ICommand TestConnectionCommand { get; private set; }

        private void wireCommands()
        {
            BackupCommand = new BaseCommand<object>(backup);
            DeleteCommand = new BaseCommand<object>(delete);
            RestoreCommand = new BaseCommand<object>(restore);
            TestConnectionCommand = new BaseCommand<object>(testConnection);
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

        /// <summary>
        /// Tests the connection to the mongod instance at the host 
        /// and gets the databases information at the instance
        /// </summary>
        /// <param name="param"></param>
        void testConnection(object param)
        {
            MongoServer server = null;
            try
            {
                if (string.IsNullOrEmpty(_host))
                    throw new Exception("Host name required");
                else if (_port == 0)
                    throw new Exception("Port required");
                else
                {
                    var crediantial = MongoCredential.CreateMongoCRCredential("admin", _dbUserName, _dbPassword);
                    server = new MongoServer(new MongoServerSettings
                    {
                        Server = new MongoServerAddress(_host, _port),
                        Credentials = new MongoCredential[] { crediantial }
                    });
                    MainVM.Instance.AddToLog(string.Format("Try to connect {0}:{1}", _host, _port));
                    server.Connect();
                    MainVM.Instance.AddToLog("Connection success, getting databases");
                    var databaseNames = server.GetDatabaseNames();
                    if (databaseNames != null)
                    {
                        var databases = databaseNames.Select(d => new DatabaseVM
                        {
                            Name = d,
                            CreatedDate = DateTime.Now,
                            IsSelected = true
                        });
                        DataBases = new ObservableCollection<DatabaseVM>(databases);
                        MainVM.Instance.AddToLog(string.Format("{0} database found in the {1}", databaseNames.Count(), _host));
                    }
                    else
                    {
                        MainVM.Instance.AddToLog(string.Format("No database found in the {0}", databaseNames.Count()));
                    }
                }
            }
            catch (Exception ex)
            {
                MainVM.Instance.AddToLog(string.Format("Error in Connection: {0}", ex.Message));
            }
            finally
            {
                if (server != null)
                {
                    server.Disconnect();
                    MainVM.Instance.AddToLog(string.Format("Disconnected to the host: {0}", _host));
                }
            }
        }

        /// <summary>
        /// Backup the MongoDB into a zip file
        /// </summary>
        /// <param name="param"></param>
        private void backup(object param)
        {
            if (AllDataBasesSelected)
                backupInstance();
            else
            {
                var selectedDatabases = DataBases.Where(d => d.IsSelected);
                foreach (var d in selectedDatabases)
                    backupInstance(d.Name);
            }
        }
        private void backupInstance(string databaseName = "")
        {
            //--dbpath
            try
            {
                var path = string.Format("{0}{1}_{2}{3}",
                    _backupPath, _host, DateTime.Now.ToString(_fileDateExtFormatStr), databaseName);

                var argumentString = "--host {0} --port {1} --username {2} --password {3} --out {4}";
                if (!string.IsNullOrEmpty(databaseName))
                    argumentString = string.Format("{0} -db {1}", argumentString, databaseName);

                _backupProcess.StartInfo = new ProcessStartInfo(_mongodumpPath,
                    string.Format(argumentString,
                    _host, _port, _dbUserName, _dbPassword, path));
                _backupProcess.Start();

                if (!string.IsNullOrEmpty(_dbPassword))
                    MainVM.Instance.AddToLog("Backup started with arguments: "
                        + _backupProcess.StartInfo.Arguments.Replace(_dbPassword, "*****"));

                _backupProcess.WaitForExit();
                _backupProcess.Refresh();

                MainVM.Instance.AddToLog("Backup Completed");

                if (_isCompressOn)
                {
                    var fileName = string.Format("{0}.zip", path);
                    compress(path, fileName);
                }
            }
            catch (Exception ex)
            {
                MainVM.Instance.AddToLog("Exception in backup: " + ex.Message);
            }
            finally
            {
                getFiles();
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

        /// <summary>
        /// Restores the selected zip file to the mongoDB
        /// </summary>
        /// <param name="param"></param>
        private void restore(object param)
        {
            if (MessageBox.Show("Current data will be replaced with restored data, do you want to continue?", "Restore Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    //mongorestore --port <port number> <path to the backup>
                    var path = decompress(_selectedFile.Path);
                    _restoreProcess.StartInfo = new ProcessStartInfo(_mongorestorePath,
                        string.Format("--host {0} --port {1} --username {2} --password {3} {4}",
                        _host, _port, _dbUserName, _dbPassword, path));
                    _restoreProcess.Start();

                    if (!string.IsNullOrEmpty(_dbPassword))
                        MainVM.Instance.AddToLog("Restore started with arguments: "
                            + _restoreProcess.StartInfo.Arguments.Replace(_dbPassword, "*****"));

                    _restoreProcess.WaitForExit();

                    MainVM.Instance.AddToLog("Restore Completed");

                    Directory.Delete(path, true);
                }
                catch (Exception ex)
                {
                    MainVM.Instance.AddToLog("Exception in restore: " + ex.Message);
                }

            }
        }
        void _restoreProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            MainVM.Instance.AddToLog("Restore Process Output: " + e.Data);
        }
        void _restoreProcess_Exited(object sender, EventArgs e)
        {
            MainVM.Instance.AddToLog("Restore Process Exited");
        }
        void _restoreProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            MainVM.Instance.AddToLog("Restore Process Error: " + e.Data);
        }

        /// <summary>
        /// Compress the given file to a .zip
        /// </summary>
        /// <param name="zipPath">Full Path</param>
        /// <param name="fileName">Full File Name</param>
        private void compress(string zipPath, string fileName)
        {
            ZipFile f = new ZipFile(fileName);
            f.AddDirectory(zipPath);
            f.CompressionLevel = CompressionLevel.BestCompression;
            f.Save();

            Directory.Delete(zipPath, true);

            MainVM.Instance.AddToLog("Compression Completed");
        }
        /// <summary>
        /// Decompress the zip file and return the folder path
        /// </summary>
        /// <param name="filename">zip file name</param>
        /// <returns></returns>
        private string decompress(string filename)
        {
            ZipFile f = new ZipFile(filename);
            string folder = filename.Replace(".zip", "");
            f.ExtractAll(folder);
            MainVM.Instance.AddToLog("Decompression Completed");
            return folder;
        }

        /// <summary>
        /// Get files in the backup folder
        /// </summary>
        private void getFiles()
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(BackupPath);
                var files = dirInfo.GetFiles("*.zip");
                var bFiles = files.Select(f => new FileVM(f));
                BackupFiles = new ObservableCollection<FileVM>(bFiles);
            }
            catch (Exception ex)
            {
                MainVM.Instance.AddToLog("Error while gettin backup files: " + ex.Message);
            }
        }

        /// <summary>
        /// Deletes the selected file
        /// </summary>
        /// <param name="param"></param>
        private void delete(object param)
        {
            if (MessageBox.Show("Backup file "
                + _selectedFile.Name
                + " will be deleted permanently, do you want to continue?", "Delete Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    File.Delete(_selectedFile.Path);
                }
                catch (Exception ex)
                {
                    MainVM.Instance.AddToLog("Error in delete file:" + ex.Message);
                }
                finally
                {
                    getFiles();
                }
            }
        }

        public void Save()
        {
            saveAsXmlAsync<SettingsVM>(_settingsFilePath);
        }
        public static SettingsVM Load()
        {
            if (File.Exists(SettingsVM._settingsFilePath))
                return SettingsVM.loadXmlFileAsync<SettingsVM>(SettingsVM._settingsFilePath);
            else
                return null;
        }
        #endregion
        #region Properties
        [XmlIgnore]
        public bool IsPropertiesInitialized { get; set; }

        private bool _isFileSelected;
        [XmlIgnore]
        public bool IsFileSelected
        {
            get { return _isFileSelected; }
            set
            {
                if (value != _isFileSelected)
                {
                    _isFileSelected = value;
                    NotifyPropertyChanged("IsFileSelected");
                }
            }
        }

        private FileVM _selectedFile;
        [XmlIgnore]
        public FileVM SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                if (value != _selectedFile)
                {
                    _selectedFile = value;
                    NotifyPropertyChanged("SelectedFile");
                    IsFileSelected = _selectedFile != null;
                }
            }
        }

        private ObservableCollection<FileVM> _backupFiles;
        [XmlIgnore]
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

        private ObservableCollection<DatabaseVM> _databases;
        [XmlIgnore]
        public ObservableCollection<DatabaseVM> DataBases
        {
            get { return _databases; }
            set
            {
                if (value != _databases)
                {
                    _databases = value;
                    NotifyPropertyChanged("DataBases");
                }
            }
        }
        public bool AllDataBasesSelected
        {
            get
            {
                if (DataBases.Count == 0)
                    return true;
                else
                    return DataBases.All(d => d.IsSelected);
            }
        }

        private DateTime _currentTime;
        [XmlIgnore]
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

        private string _mongorestorePath;
        public string MongorestorePath
        {
            get { return _mongorestorePath; }
            set
            {
                if (value != _mongorestorePath)
                {
                    _mongorestorePath = value;
                    NotifyPropertyChanged("MongorestorePath");
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

        private int _port;
        public int Port
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