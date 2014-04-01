using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoBackupManager
{
    public class FileVM : VMBase
    {
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
    }
}
