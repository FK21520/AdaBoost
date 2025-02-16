using System.Windows;

namespace AdaBoost
{
    public partial class MAINWINDOW : Window
    {
        public MAINWINDOW()
        {
            InitializeComponent();
            MainFrame.Navigate(new FUNCTION());
        }
    }
}