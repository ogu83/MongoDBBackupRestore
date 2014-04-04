using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using xml = System.Xml;
using xmlser = System.Xml.Serialization;

namespace MongoBackupManager
{
    public abstract class VMBase : INotifyPropertyChanged
    {
        protected static string LINE_BREAK = "\r\n";
        protected static string _appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        #region XML
        protected async Task<string> toXmlString<T>() where T : VMBase
        {
            string retVal = string.Empty;
            xmlser.XmlSerializer ser = new xmlser.XmlSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                ser.Serialize(stream, this);
                using (var reader = xml.XmlReader.Create(stream))
                {
                    retVal = await reader.ReadContentAsStringAsync();
                }
            }
            return retVal;
        }
        protected byte[] toXmlBytes<T>() where T : VMBase
        {
            xmlser.XmlSerializer ser = new xmlser.XmlSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                ser.Serialize(stream, this);
                return stream.ToArray();
            }
        }
        protected static async Task<T> fromXmlString<T>(string xmlContent) where T : VMBase
        {
            T retVal = null;
            xmlser.XmlSerializer ser = new xmlser.XmlSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(xmlContent);
                    using (xml.XmlReader reader = xml.XmlReader.Create(stream))
                    {
                        if (ser.CanDeserialize(reader))
                            retVal = ser.Deserialize(reader) as T;
                        else
                            throw new TypeInitializationException(typeof(T).FullName, new xml.XmlException("Cannot Deserialize"));
                    }
                }
            }
            return retVal;
        }
        protected static T fromXmlBytes<T>(byte[] xmlBytes) where T : VMBase
        {
            xmlser.XmlSerializer ser = new xmlser.XmlSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream(xmlBytes))
                return ser.Deserialize(stream) as T;
        }
        #endregion
        #region Marshaling
        protected byte[] toUnmanagedBytes()
        {
            var size = Marshal.SizeOf(this);
            // Both managed and unmanaged buffers required.
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            // Copy object byte-to-byte to unmanaged memory.
            Marshal.StructureToPtr(this, ptr, false);
            // Copy data from unmanaged memory to managed buffer.
            Marshal.Copy(ptr, bytes, 0, size);
            // Release unmanaged memory.
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }
        protected static T fromUnmanagedBytes<T>(byte[] bytes) where T : VMBase
        {
            var size = bytes.Length;
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, ptr, size);
            var retVal = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return retVal;
        }
        #endregion
        #region Storage
        protected void saveAsXmlAsync<T>(string fileName) where T : VMBase
        {
            File.WriteAllBytes(fileName, toXmlBytes<T>());
        }
        protected static T loadXmlFileAsync<T>(string fileName) where T : VMBase
        {
            return fromXmlBytes<T>(File.ReadAllBytes(fileName));
        }
        #endregion
        #region INotifyPropertyChanged Interface
        public event PropertyChangedEventHandler PropertyChanged;
        protected async void NotifyPropertyChangedAsync(string info)
        {
            await Application.Current.MainWindow.Dispatcher.InvokeAsync(new Action(() =>
            {
                NotifyPropertyChanged(info);
            }));
        }
        protected void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public virtual void Initialize() { }
        public virtual void Suspend() { }
        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        public static bool IsVMBase(object o)
        {
            return (o as VMBase != null);
        }

        public event EventHandler Changed;

        private bool _isChanged;
        [xmlser.XmlIgnore()]
        public bool IsChanged
        {
            get { return _isChanged; }
            set
            {
                _isChanged = value;
                NotifyPropertyChanged("IsChanged");
                if (Changed != null)
                    Changed(this, null);
            }
        }
    }
}