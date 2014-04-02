using System.Reflection;
using System.Xml.Serialization;

namespace MongoBackupManager
{
    public abstract class VMPageBase : VMBase
    {
        public string AppTitle { get { return Assembly.GetExecutingAssembly().GetName().Name + " v:" + AppVersion; } }
        public string AppVersion { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }

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
