using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.IO; 
using DataProcessing;

namespace AdaBoost
{
    public partial class SELECTION : Page
    {
        private int _num_file;
        public List<string>  proc{ get; set; } //テスト方式の選択　ドロップダウンリスト
        private int _fold_flag;

        public SELECTION(int num_file)
        {
            InitializeComponent();
            _num_file = num_file;
            proc = new List<string>(); //コンストラクタで初期化
            if (_num_file == 1)
            {
                test_panel.Visibility = Visibility.Collapsed; //下のテキストボックスを非表示
                proc = new List<string> { "シンプル", "クロスバリデーション" };
                DataContext = this;
                proc_combox.SelectedItem = "シンプル"; //初期値
                train_data.Content = "データを選択";
            }
            else
            {
                proc_combox.Visibility = Visibility.Collapsed; //ドロップダウンリストを非表示
                fold_pannel.Visibility = Visibility.Collapsed; //分割数指定のテキストボックスを非表示
                train_data.Content = "教師データを選択";
                test_data.Content = "テストデータを選択";
            }
            weak_id_pannel.Text = "50"; //弱識別機数の初期値
            fold_text.Text = "3"; //分割数の初期値
        }

        //ドロップダウンリストの処理
        private void TestProc(object sender, SelectionChangedEventArgs e)
        {
            ComboBox? comboBox = sender as ComboBox;
            if (comboBox?.SelectedItem != null)
            {
                string? select_item = comboBox.SelectedItem.ToString(); //選択
             
                if(select_item == "シンプル")
                {
                    _fold_flag = 0;
                    fold_pannel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _fold_flag = 1;
                    fold_pannel.Visibility = Visibility.Visible;
                }
            }
        }

        //プロット処理
        private void PlotClick(object sender, RoutedEventArgs e)
        {
            string file_path = train_path_text_box.Text;
            string weak_id = weak_id_pannel.Text; //弱識別機の数
            int number = int.Parse(weak_id); //キャストでint型にする
            int fold_num = 0;
            if (string.IsNullOrEmpty(file_path) && _num_file == 1)
            {
                MessageBox.Show("ファイルがが選択されていません。", "エラー");   
            }
            else if(File.Exists(file_path) && _num_file == 1) //データが1つの場合
            {
                if (_fold_flag == 0)// 交差検証法,0でオフ1でオン
                {
                    TOOL tool = new TOOL();
                    fold_num = 0;
                    var (train_data_path, test_data_path) = tool.SplitData(file_path);
                    var plot_window = new PLOTWINDOW(number, fold_num, train_data_path, test_data_path);
                    plot_window.Show();
                }
                else
                {
                    string get_fold_num = fold_text.Text;
                    if (int.TryParse(get_fold_num, out fold_num))
                    {
                        var plot_window = new PLOTWINDOW(number, fold_num, file_path);
                        plot_window.Show();
                    }
                    else
                    {
                        MessageBox.Show("数字のみ入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            else//データが2つの場合
            { 
                string train_data_path = train_path_text_box.Text;
                string test_data_path = test_path_text_box.Text;
                
                if (!string.IsNullOrEmpty(train_data_path) && !string.IsNullOrEmpty(test_data_path) && !string.IsNullOrEmpty(weak_id))
                {
                    //MessageBox.Show($"入力されたファイルパス: {train_data} {test_data} 弱識別機の数:{weak_id}");
                    fold_num = 0; //初期化
                    var plot_window = new PLOTWINDOW(number,fold_num, train_data_path, test_data_path);
                    plot_window.Show();
                }
                else if (!string.IsNullOrEmpty(train_data_path) && !string.IsNullOrEmpty(test_data_path) && string.IsNullOrEmpty(weak_id))
                {
                    MessageBox.Show("弱識別機の数が入力されていません。", "エラー");
                }
                else if (string.IsNullOrEmpty(train_data_path) || string.IsNullOrEmpty(test_data_path) && !string.IsNullOrEmpty(weak_id))
                {
                    MessageBox.Show("ファイルがが選択されていません。3", "エラー");
                }else if (!File.Exists(train_data_path) && !File.Exists(test_data_path))
                {
                    MessageBox.Show("ファイルが存在しません。4", "エラー");
                }
            }
        }

        private void SelectTrainFile(object sender, RoutedEventArgs e)
        {
            // ファイル選択ダイアログを開く
            OpenFileDialog open_file = new OpenFileDialog();
            if (open_file.ShowDialog() == true)
            {
                // 選択されたファイルのパスをテキストボックスに表示
                train_path_text_box.Text = open_file.FileName;
            }
        }

        private void SelectTestFile(object sender, RoutedEventArgs e)
        {
            // ファイル選択ダイアログを開く
            OpenFileDialog open_file = new OpenFileDialog();
            if (open_file.ShowDialog() == true)
            {
                // 選択されたファイルのパスをテキストボックスに表示
                test_path_text_box.Text = open_file.FileName;
            }
        }

        //苗の画面に戻る
        private void ReturnButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new FUNCTION());   
        }
    }
}
