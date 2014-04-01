using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MongoBackupManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainVM _myVM;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _myVM = new MainVM();

            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;
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
