using System.Windows;

namespace WpfApp1
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
     
        public MainWindow()
        {

            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();


         

        }
        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            realtime subWindow = new realtime();
            subWindow.Show();
            this.Close();
        }
        private void ButtonClickedPushup(object sender, RoutedEventArgs e) 
        {
            Pushups subWindow = new Pushups();
            subWindow.Show();
            this.Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Plank subWindow = new Plank();
            subWindow.Show();
            this.Close();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
