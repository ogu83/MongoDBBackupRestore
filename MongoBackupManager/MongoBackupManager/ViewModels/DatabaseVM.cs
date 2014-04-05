using System;

namespace MongoBackupManager
{
    public class DatabaseVM : VMBase
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
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
    }
}
