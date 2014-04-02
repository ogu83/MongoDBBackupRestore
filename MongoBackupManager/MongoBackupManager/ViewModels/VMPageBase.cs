using System.Xml.Serialization;

namespace MongoBackupManager
{
    public abstract class VMPageBase : VMBase
    {
        private bool _isBusy;
        [XmlIgnore]
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    NotifyPropertyChanged("IsBusy");
                }
            }
        }
    }
}
