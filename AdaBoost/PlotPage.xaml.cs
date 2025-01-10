using AdaBoostAlgorithm;
using OxyPlot.Wpf;
using System.Windows.Controls;

namespace AdaBoost
{
    public partial class PLOTPAGE : Page
    {
        public PLOTPAGE(string train_data, string test_data, int weak_id)
        {
            InitializeComponent();
            READCSV read_csv = new READCSV();
            var (train_X, train_label) = read_csv.Read(train_data);
            var (test_X, test_label) = read_csv.Read(test_data);
            ADABOOST adaboost = new ADABOOST(weak_id);
            adaboost.Fit(train_X, train_label);
            var closs = new CLOSSVALIDATION(3, weak_id, train_X, train_label);
            
            int[] pred = adaboost.Predict(test_X);

            double accuracy = AccuracyScore(test_label, pred);

            Console.WriteLine($"Accuracy: {accuracy:P2}"); // P2で百分率表示

            PLOT plot = new PLOT();
            PlotView plotView = plot.PlotDecisionRegion(test_X, test_label, adaboost);
            plot_grid.Children.Add(plotView);  // WPF のパネルに追加
        }

        public static double AccuracyScore(int[] true_label, int[] predict)
        {
            if (true_label.Length != predict.Length)
            {
                throw new ArgumentException("ラベルと予測の配列の長さが一致しません。");
            }

            // 一致するラベルの数をカウント
            int correct_count = true_label
                .Zip(predict, (trueLabel, pred) => trueLabel == pred ? 1 : 0)
                .Sum();

            // 正解率を計算
            return (double)correct_count / true_label.Length;
        }
    }
}
