using Microsoft.Win32;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace AdaBoost
{
    /// <summary>
    /// Page1.xaml の相互作用ロジック
    /// </summary>
    public partial class SELECTION : Page
    {
        private int _num_file;
        public SELECTION(int num_file)
        {
            InitializeComponent();
            _num_file = num_file;
            Console.WriteLine($"{num_file}");
        }

        private void PlotClick(object sender, RoutedEventArgs e)
        {
            string file_path = FilePathTextBox.Text;
            string weak_id = WeakId.Text; //弱識別機の数

            int number = int.Parse(weak_id);
            // パスを表示
            if (!string.IsNullOrEmpty(file_path) && !string.IsNullOrEmpty(weak_id))
            {
                MessageBox.Show($"入力されたファイルパス: {file_path} 弱識別機の数:{weak_id}");
                var plot_window = new PLOTWINDOW(file_path, number);
                plot_window.Show();
            }
            else if (string.IsNullOrEmpty(file_path) && !string.IsNullOrEmpty(weak_id))
            {
                MessageBox.Show("ファイルパスが入力されていません。", "エラー");
            }
            else if (!string.IsNullOrEmpty(file_path) && string.IsNullOrEmpty(weak_id))
            {
                MessageBox.Show("弱識別機の数が指定されていません。", "エラー");
            }
            else if (!string.IsNullOrEmpty(file_path) && string.IsNullOrEmpty(weak_id))
            {
                MessageBox.Show("ファイルと弱識別機の数が指定されていません。", "エラー");
            }
           
        }

        private void SelectFile(object sender, RoutedEventArgs e)
        {
            // ファイル選択ダイアログを開く
            OpenFileDialog open_file = new OpenFileDialog();
            if (open_file.ShowDialog() == true)
            {
                // 選択されたファイルのパスをテキストボックスに表示
                FilePathTextBox.Text = open_file.FileName;
            }
        }
    }
}
