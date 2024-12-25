using System.IO;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;

namespace AdaBoostAlgorithm
{
    internal class DECISIONSTUMP
    {
        private int axis;
        private int sign;
        private double threshold;

        private double[,] X;
        private int[] z;

        public DECISIONSTUMP(int axis = 0, int sign = 1, double threshold = 0.0)
        {
            string file_path = "./adaboost_dataset.csv";
            READCSV read_csv = new READCSV();
            var (X_data, z_data) = read_csv.Read(file_path);

            // メンバー変数にでーたを格納
            X = X_data;
            z = z_data;

            this.axis = axis;
            this.sign = sign;
            this.threshold = threshold;
        } 

        //ソート関数
        private (double[,] sort_X, int[] sort_label, double[] sort_sample_weight) SortData(
        double[,] X, int[] label, double[] sample_weight, int axis)
        {
            int rows = X.GetLength(0);
            int cols = X.GetLength(1);

            // 指定した列を基準にインデックスをソート
            double[] column = Enumerable.Range(0, rows)
                            .Select(ii => X[ii, axis])
                            .ToArray();

            int[] sort_index = column
                .Select((value, index) => new { Value = value, Index = index })
                .OrderBy(item => item.Value)
                .Select(item => item.Index)
                .ToArray();

            // ソート後のデータ作成
            double[,] sort_X = new double[rows, cols];
            int[] sort_label = sort_index.Select(ii => label[ii]).ToArray();
            double[] sort_sample_weight = sort_index.Select(i => sample_weight[i]).ToArray();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    sort_X[i, j] = X[sort_index[i], j];
                }
            }
            return (sort_X, sort_label, sort_sample_weight);
        }

        private (int sign, double threshold, double error) FitOnedim(double[,] X, int[] label, double[] sample_weight, int axis)
        {
            int N = label.Length; //配列の要素数
            
            //ソート
            var (sort_X, sort_label, sort_sample_weight) = SortData(X, label, sample_weight, axis);

            //予測の計算
            int[,] pred = new int[N - 1, N];
            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    pred[i, j] = j <= i ? -1 : 1; //三角行列
                }
            }

            //ご分類の計算
            int[,] miss = new int[N - 1, N];
            for(int i = 0; i < N -1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    miss[i, j] = pred[i, j] != sort_label[j] ? 1 : 0; //正解なら0,誤りは1 
                }              
            }

            //誤差の計算
            double[,] error = new double[2, N - 1];           
            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    error[0, i] += miss[i, j] * sort_sample_weight[j];
                }
            }

            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    error[1, i] += (1 - miss[i, j]) * sort_sample_weight[j];
                }
            }
            
            int rows = error.GetLength(0); // error の行数
            int col = error.GetLength(1); // error の列数
            double min_error = double.MaxValue;
            int min_row = -1, min_col = -1;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (error[i, j] < min_error)
                    {
                        min_error = error[i, j];
                        min_row = i;
                        min_col = j;
                    }
                }
            }

            // 閾値と信頼値の計算
            int sign = -2 * min_row + 1;
            if (min_col + 1 >= sort_X.GetLength(0))
            {
                throw new InvalidOperationException("Threshold calculation index out of range.");
            }
            double threshold = (sort_X[min_col, axis] + sort_X[min_col + 1, axis]) / 2.0;
            double errors = min_error;          
            return (sign, threshold, errors);
        }

        public void DsFit(double[,] X, int[] label, double[] sample_weight)
        {
            int N = X.GetLength(0); //列数
            int D = X.GetLength(1); //行数
           
            int[] sign = new int[D];
            double[] threshold = new double[D];
            double[] error = new double[D];

            // 要素がすべて 1/N の配列を生成
            if (sample_weight == null)
            {
                sample_weight = Enumerable.Repeat(1.0 / N, N).ToArray();
            }

            for (int axis = 0; axis < D; axis++)
            {
                // 各次元ごとに fit_onedim を実行して結果を収集
                (int signs, double thresholds, double errors) = FitOnedim(X, label, sample_weight, axis);
                sign[axis] = signs;
                threshold[axis] = thresholds;
                error[axis] = errors;
            }

            // 誤差が最小の次元を選択
            int best_axis = Array.IndexOf(error, error.Min());
            this.axis = best_axis;
            this.sign = sign[best_axis];
            this.threshold = threshold[best_axis];           
        }

        public int[] DsPredict(double[,] X)
        {
            int N = X.GetLength(0);
            int[] predictions = new int[N];

            for (int i = 0; i < N; i++)
            {
                double value = X[i, axis];
                predictions[i] = (value < threshold) ? -sign : sign;
            }
            return predictions;
        }
    }

    class ADABOOST
    {
        private int num_classifiers; //クラスの数
        private double[] alpha;
        private List<DECISIONSTUMP> classifiers; //クラス

        public ADABOOST(int num_classifiers)
        {
            this.num_classifiers = num_classifiers; 
            alpha = new double[num_classifiers];
            classifiers = new List<DECISIONSTUMP>(num_classifiers);

            for (int i = 0; i < num_classifiers; i++)
            {
                classifiers.Add(new DECISIONSTUMP());
            }
        }

        public void Fit(double[,] X, int[] label)
        {
            int N = label.Length;
            double[] weight = Enumerable.Repeat(1.0 / N, N).ToArray(); //重みも初期化

            Console.WriteLine($"{num_classifiers}");
            for (int m = 0; m < num_classifiers; m++)
            {
                //基本分類器の訓練
                classifiers[m].DsFit(X, label, weight);

                //誤りの判定
                int[] base_pred = classifiers[m].DsPredict(X); // 同じ値をN回繰り返す
                bool[] miss = base_pred.Zip(label, (pred, actual) => pred != actual).ToArray();
             
                // イプシロンの計算
                double eps = weight.Zip(miss, (w, m) => m ? w : 0.0).Sum();
                if (eps == 0)
                {
                    eps = Double.Epsilon;  // 誤差が0の場合の対策      
                }

                //s信頼地の計算(アルファ)
                alpha[m] = Math.Log(1.0 / eps - 1);

                //重みの更新
                for (int w = 0; w < N; w++)
                {
                    weight[w] *= Math.Exp(alpha[m] * (miss[w] ? 1 : 0));

                }

                //重みの正規化
                double sum_weight = weight.Sum();
                for (int i = 0; i < N; i++)
                {
                    weight[i] /= sum_weight;
                }
            }             
        }

        public int[] Predict(double[,] X)
        {
            int N = X.GetLength(0);
            double[] final_predict = new double[N];

            //予測の集約
            for (int i = 0; i < num_classifiers; i++)
            {
                int[] predictions = classifiers[i].DsPredict(X);

                for (int j = 0; j < N; j++)
                {
                    final_predict[j] += alpha[i] * predictions[j];
                }
            }
            return final_predict.Select(p => p >= 0 ? 1 : -1).ToArray();
        }

    }

    class READCSV
    {
        //読み込みと出力を分ける
        public (double[,] X, int[] label) Read(string file_path)
        {
            try
            {
                var lines = File.ReadAllLines(file_path).Skip(1).ToArray();
                var x = lines.Select(line => double.Parse(line.Split(',')[0])).ToArray();
                var y = lines.Select(line => double.Parse(line.Split(',')[1])).ToArray();
                var label = lines.Select(line => int.Parse(line.Split(',')[2])).ToArray();

                double[,] X = CombineTo2D(x, y);

                return (X, label);  
            }
            catch (Exception ex)
            {
                Console.WriteLine("ファイルの読み込み中にエラーが発生しました: " + ex.Message);
                throw;
            }
        }

        //2次元データを結合して2次元データにする
        static double[,] CombineTo2D(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("配列の長さが一致しません");
            }

            double[,] result = new double[x.Length, 2];

            for (int i = 0; i < x.Length; i++)
            {
                result[i, 0] = x[i];  
                result[i, 1] = y[i];  

            }
            return result;
        }
    }

    //データポイントを用意してデータポイントに対して予測を行い、境界の色分けをする
    class PLOT
    {
        public PlotView PlotDecisionRegion(double[,] X, int[] label, ADABOOST model)
        {
            int N = X.GetLength(0);
            double min_x = X.Cast<double>().Min();
            double max_x = X.Cast<double>().Max();
            double min_y = X.Cast<double>().Min();
            double max_y = X.Cast<double>().Max();

            // 予測範囲のグリッドを作成
            int resolution = 140; // 解像度
            double step = Math.Max((max_x - min_x) / resolution, (max_y - min_y) / resolution);

            var plot_model = new PlotModel { Title = "AdaBoost Decision Region" };

            //辞書でラベルの色を定義
            var label_color = new Dictionary<int, OxyColor>
                {
                    { -1, OxyColors.Blue },
                    { 1, OxyColors.Red }
                };

   
            // 各ラベルごとに散布図シリーズを作成
            foreach (var labels in label_color.Keys)
            {
                var grid_scatter_series = new ScatterSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerFill = OxyColor.FromAColor(50, label_color[labels]),
                    Title = $"Label {labels}"
                };

                var scatter_series = new ScatterSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerFill = label_color[labels],
                    Title = $"Label {labels}"
                };

                // 決定境界をプロット
                for (double x = min_x; x <= max_x; x += step)
                {
                    for (double y = min_y; y <= max_y; y += step)
                    {
                        double[,] grid_point = new double[,] { { x, y } };
                        int[] prediction = model.Predict(grid_point);

                        if (prediction[0] == labels) 
                        {
                            grid_scatter_series.Points.Add(new ScatterPoint(x, y));
                        }
                    }
                }
                plot_model.Series.Add(grid_scatter_series);

                // 元のデータポイントを追加
                for (int i = 0; i < X.GetLength(0); i++)
                {
                    if (label[i] == labels)
                    {
                        scatter_series.Points.Add(new ScatterPoint(X[i, 0], X[i, 1]));
                    }
                }
                plot_model.Series.Add(scatter_series);
            }
            return new PlotView { Model = plot_model };
        }
    }
}
