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
using System.Windows.Shapes;

namespace AdaBoost
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class PLOTWINDOW : Window
    {
        public PLOTWINDOW(string file_path, int weak_id)
        {
            InitializeComponent();
            plot_frame.Navigate(new PLOTPAGE(file_path, weak_id));
        }
    }
}
