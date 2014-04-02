using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace MongoBackupManager
{
    public class FileVM : VMBase
    {
        public FileVM()
        {
            
        }
        public FileVM(FileInfo f)
            : this()
        {
            Name = f.Name;
            CreatedDate = f.CreationTime;
            Path = f.FullName;
        }
        
        #region Properties
        private DateTime _createdDate;
        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set
            {
                if (value != _createdDate)
                {
                    _createdDate = value;
                    NotifyPropertyChanged("CreatedDate");
                    NotifyPropertyChanged("CreatedDateStr");
                }
            }
        }
        public string CreatedDateStr
        {
            get { return _createdDate.ToString("dd.MM.yyyy HH:mm"); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                if (value != _path)
                {
                    _path = value;
                    NotifyPropertyChanged("Path");
                }
            }
        }
        #endregion
    }
}