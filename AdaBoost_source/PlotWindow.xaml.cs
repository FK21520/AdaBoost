using System.Windows;

namespace AdaBoost
{
    public partial class PLOTWINDOW : Window
    {
        public PLOTWINDOW(int weak_id, int fold_num, params string[] file_path)
        {
            InitializeComponent();
            plot_frame.Navigate(new PLOTPAGE(weak_id, fold_num,  file_path));
        }
    }
}
