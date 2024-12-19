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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AdaBoostAlgorithm;
using OxyPlot.Wpf;

namespace AdaBoost
{
    /// <summary>
    /// Page1.xaml の相互作用ロジック
    /// </summary>
    public partial class SELECTION : Page
    {
        public SELECTION()
        {
            InitializeComponent();
            string file_path = "./adaboost_dataset.csv";
            READCSV read_csv = new READCSV();
            var (X, label) = read_csv.Read(file_path);
            ADABOOST adaboost = new ADABOOST(50);
            adaboost.Fit(X, label);

            int[] pred = adaboost.Predict(X);

            double accuracy = AccuracyScore(label, pred);

            Console.WriteLine($"Accuracy: {accuracy:P2}"); // P2で百分率表示

            for(int i = 0; i < X.GetLength(0); i++)
            {
                if (pred[i] != label[i])
                {
                    Console.WriteLine($"{i}, {label[i]}, {pred[i]}");
                }
            }
            PLOT plot = new PLOT();
            var plot_view = plot.PlotScatter(X, label);
            MainGrid.Children.Add(plot_view); // WPF のウィンドウで表示

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
