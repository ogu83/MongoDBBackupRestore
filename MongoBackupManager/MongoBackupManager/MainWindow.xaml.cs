using System.Windows;

namespace MongoBackupManager
{
    public partial class MainWindow : Window
    {
        private MainVM _myVM;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _myVM = new MainVM();

            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;
            Application.Current.Exit += Current_Exit;
        }

        void Current_Exit(object sender, ExitEventArgs e)
        {
            _myVM.Suspend();
        }

        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _myVM.Suspend();
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _myVM.Initialize();
        }
    }
}
