using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace AdaBoost
{
    /// <summary>
    /// Page1.xaml の相互作用ロジック
    /// </summary>
    public partial class FUNCTION : Page
    {
        public int num_file; // ファイルの数
        public FUNCTION()
        {
            InitializeComponent();
        }

        private void Select1Click(object sender, RoutedEventArgs e)
        {
            num_file = 1;
            NavigationService.Navigate(new SELECTION(num_file));
        }

        private void Select2Click(object sender, RoutedEventArgs e)
        {
            num_file = 2;
            NavigationService.Navigate(new SELECTION(num_file));
        }
    }
}
