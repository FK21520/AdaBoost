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
        public int _num_file; // ファイルの数
        public List<string> func { get; set; } //機能の選択　ドロップダウンリスト
        public FUNCTION()
        {
            InitializeComponent();
            func = new List<string> { "一つのデータを選択して分割", "あらかじめ分割されたデータを使用" };
            // ComboBox にデータをバインド
            func_combox.ItemsSource = func;
            func_combox.SelectedItem = "一つのデータを選択して分割"; //初期値
        }

        private void FunctionProc(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox.SelectedItem != null)
            {
                string select_item = comboBox.SelectedItem.ToString();

                if (select_item == "一つのデータを選択して分割")
                {
                   _num_file = 1;
                }
                else
                {
                    _num_file = 2;
                }
            }
        }

        private void SelectClick(object sender, RoutedEventArgs e)
        {
            int number = _num_file;
            NavigationService.Navigate(new SELECTION(number));
        }
    }
}
