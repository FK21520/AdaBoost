using AdaBoostAlgorithm;
using OxyPlot.Wpf;
using System.Windows.Controls;

namespace AdaBoost
{
    public partial class PLOTPAGE : Page
    {
        public PLOTPAGE(string file_path, int weak_id)
        {
            InitializeComponent();
            READCSV read_csv = new READCSV();
            var (X, label) = read_csv.Read(file_path);
            ADABOOST adaboost = new ADABOOST(weak_id);
            adaboost.Fit(X, label);

            int[] pred = adaboost.Predict(X);

            double accuracy = AccuracyScore(label, pred);

            Console.WriteLine($"Accuracy: {accuracy:P2}"); // P2で百分率表示

            for (int i = 0; i < X.GetLength(0); i++)
            {
                if (pred[i] != label[i])
                {
                    Console.WriteLine($"{i}, {label[i]}, {pred[i]}");
                }
            }

            PLOT plot = new PLOT();
            PlotView plotView = plot.PlotDecisionRegion(X, label, adaboost);

            // WPF での表示 (適切な WPF アプリケーションで使用)
            plot_grid.Children.Add(plotView);  // WPF のパネルに追加

            // ここでは Console で結果を表示している
            Console.WriteLine("プロットを表示しました。");
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
