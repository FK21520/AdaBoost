using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using AdaBoostAlgorithm;
using System.IO; 
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace AdaBoost
{
    /// <summary>
    /// Page1.xaml の相互作用ロジック
    /// </summary>
    public partial class SELECTION : Page
    {
        private int _num_file;
        //分割したデータの保存先のファイルパス
        private string train_name = "./splitTrainData.csv";
        private string test_name = "./splitTestData.csv";
        public SELECTION(int num_file)
        {
            InitializeComponent();
            _num_file = num_file;
            if (_num_file == 1)
            {
                test_panel.Visibility = Visibility.Collapsed;
            }
            weak_id_pannel.Text = "50"; //初期値
        }

        private (string train_data, string test_data) SplitData(string file_path) //データ教師とテストに分割
        { 
            string train_data = string.Empty;
            string test_data = string.Empty;
            if (_num_file == 1)
            {   
                MakeSplitData(file_path);
                train_data = train_name;
                test_data = test_name;
            }
            else
            {
                train_data = train_path_text_box.Text;
                test_data = test_path_text_box.Text;
   
            }

            return (train_data, test_data);
        }

        //データを分けてcsvに保存
        private void MakeSplitData(string before_file_path)
        {
            if (string.IsNullOrEmpty(before_file_path))
            {
                MessageBox.Show("ファイルがが選択されていません。", "エラー");
            }
            else
            {
                READCSV read_csv = new READCSV();
                var (X_data, z_data) = read_csv.Read(before_file_path);

                var data = new List<DataRow>();

                for (int i = 0; i < z_data.Length; i++)
                {
                    data.Add(new DataRow { x = X_data[i, 0], y = X_data[i, 1], label = z_data[i] });
                }

                // ランダムシード値を固定
                int seed = 42; 
                var random = new Random(seed);
                //var shuffle_data = new List<>
                var shuffled_data = new List<DataRow>(data);
                // トレーニングデータとテストデータを分割 (80%:20%)
                int split_index = (int)(shuffled_data.Count * 0.8);
                var after_train_data = shuffled_data.GetRange(0, split_index);
                var after_test_data = shuffled_data.GetRange(split_index, shuffled_data.Count - split_index);

                // トレーニングデータをCSVファイルに保存
                SaveToCsv("splitTrainData.csv", after_train_data);

                // テストデータをCSVファイルに保存
                SaveToCsv("splitTestData.csv", after_test_data);
            }
        }

         // CSVファイルに保存するメソッド
        static void SaveToCsv(string filePath, List<DataRow> data)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(data);
            }
        }

        // データクラス
        public class DataRow
        {
            public double x { get; set; } // X の1番目の値
            public double y { get; set; } // X の2番目の値
            public int label { get; set; } // ラベル
        }

        private void PlotClick(object sender, RoutedEventArgs e)
        {
            string file_path = train_path_text_box.Text;
            if (string.IsNullOrEmpty(file_path) && _num_file == 1)
            {
                //ファイルが既に存在している売位に削除する
                if (File.Exists(train_name) || File.Exists(test_name))
                {
                    File.Delete(train_name);
                    File.Delete(test_name);
                }
                else
                {
                    MessageBox.Show("ファイルがが選択されていません。2", "エラー");
                }
            }
            else
            { 
                var (train_data, test_data) = SplitData(file_path);
                string weak_id = weak_id_pannel.Text; //弱識別機の数
                int number = int.Parse(weak_id); //キャストでint型にする

                if (!string.IsNullOrEmpty(train_data) && !string.IsNullOrEmpty(test_data) && !string.IsNullOrEmpty(weak_id))
                {
                    MessageBox.Show($"入力されたファイルパス: {train_data} {test_data} 弱識別機の数:{weak_id}");
                    var plot_window = new PLOTWINDOW(train_data, test_data, number);
                    plot_window.Show();
                }
                else if (!string.IsNullOrEmpty(train_data) && !string.IsNullOrEmpty(test_data) && string.IsNullOrEmpty(weak_id))
                {
                    MessageBox.Show("弱識別機の数が入力されていません。", "エラー");
                }
                else if (string.IsNullOrEmpty(train_data) || string.IsNullOrEmpty(test_data) && !string.IsNullOrEmpty(weak_id))
                {
                    MessageBox.Show("ファイルがが選択されていません。", "エラー");
                }else if (!File.Exists(train_data) && !File.Exists(test_data))
                {
                    MessageBox.Show("ファイルが存在しません。", "エラー");
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
