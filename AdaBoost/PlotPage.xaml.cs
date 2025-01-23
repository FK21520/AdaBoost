using AdaBoostAlgorithm;
using OxyPlot.Wpf;
using System.Windows.Controls;
using DataProcessing;
using System.Windows;

namespace AdaBoost
{
    public partial class PLOTPAGE : Page
    {
        private List<PlotView> plotViews = new List<PlotView>();

        public PLOTPAGE(int weak_id, int fold_num, params string[] file_path)
        {
            InitializeComponent();
            READCSV read_csv = new READCSV();
            ADABOOST adaboost = new ADABOOST(weak_id);
            if (file_path.Length == 1)//選択データが一つ
            {
                var (X, label) = read_csv.Read(file_path[0]);
                var closs = new CLOSSVALIDATION(fold_num, weak_id, X, label);
                plotViews = closs.GetPlots();

                foreach (var plotView in plotViews)
                {
                    plotView.HorizontalAlignment = HorizontalAlignment.Stretch;
                    plotView.VerticalAlignment = VerticalAlignment.Stretch;
                    plotView.SetValue(Grid.RowProperty, 0); // 1行目に配置
                    plot_stack_panel.Items.Add(plotView);
                }
            }
            else if(file_path.Length == 2)//選択データがつ
            {
                var (train_X, train_label) = read_csv.Read(file_path[0]);
                var (test_X, test_label) = read_csv.Read(file_path[1]);
                adaboost.Fit(train_X, train_label);
                int[] pred = adaboost.Predict(test_X);
                double accuracy = AccuracyScore(test_label, pred);

                PLOT plot = new PLOT();
                PlotView plotView = plot.PlotDecisionRegion(test_X, test_label, adaboost);
                plotView.HorizontalAlignment = HorizontalAlignment.Stretch;
                plotView.VerticalAlignment = VerticalAlignment.Stretch;
                plot_stack_panel.Items.Add(plotView);
            }
            this.SizeChanged += SizeChange;
        }

        //グラフのサイズ調整
        private void SizeChange(object sender, SizeChangedEventArgs e)
        {
            double size = Math.Min(plot_stack_panel.ActualWidth / plotViews.Count, plot_stack_panel.ActualHeight);
            foreach (var plot_view in plotViews)
            {
                plot_view.Width = size;
                plot_view.Height = size;
            }
        }

        private double AccuracyScore(int[] true_label, int[] predict)
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
