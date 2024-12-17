using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace AdaBoostAlgorithm
{
    internal class DECISIONSTUMP
    {
        private int? axis;
        private int? sign;
        private double? threshold;

        private double[,] X;
        private int[] z;

        

        public DECISIONSTUMP(int? axis = null, int? sign = null, double? threshold = null)
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

            //Console.WriteLine("x: " + string.Join(", ", x));
            //Console.WriteLine("y: " + string.Join(", ", y));
            //Console.WriteLine("z: " + string.Join(", ", z));

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

            //Console.WriteLine("sort: " + string.Join(", ", sort_result));

            //予測の計算
            int[,] pred = new int[N - 1, N];
            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    pred[i, j] = j <= i ? -1 : 1;
                }
            }

            //誤差の計算
            double[] errs = new double[2]; // 正と負の誤差

            for (int i = 0; i < N - 1; i++)
            {
                errs[0] = 0.0;
                errs[1] = 0.0;

                for (int j = 0; j < N; j++)
                {
                    if (pred[i, j] != sort_label[j])
                    {
                        errs[0] += sort_sample_weight[j];
                    }
                    else
                    {
                        errs[1] += sort_sample_weight[j];
                    }
                }
            }


            // 閾値と信頼値の計算
            int min_index = Array.IndexOf(errs, errs.Min());
            int sign = (errs[min_index] < 0.5) ? 1 : -1; // 信頼値の符号設定
            double threshold = (sort_X[min_index, axis] + sort_X[min_index + 1, axis]) / 2.0; //閾値
            double error = errs[min_index]; //エラー

            return (sign, threshold, error);
        }

        public void Fit(double[,] X, int[] label, double[] sample_weight)
        {
            int N = X.GetLength(0); //列数
            int D = X.GetLength(1); //行数

            int[] signs = new int[D];
            double[] thresholds = new double[D];
            double[] errors = new double[D];

            // 要素がすべて 1/N の配列を生成
            if (sample_weight == null)
            {
                sample_weight = Enumerable.Repeat(1.0 / N, N).ToArray();
            }

            for (int axis = 0; axis < D; axis++)
            {
                // 各次元ごとに fit_onedim を実行して結果を収集
                (int sign, double threshold, double error) = FitOnedim(X, label, sample_weight, axis);
                signs[axis] = sign;
                thresholds[axis] = threshold;
                errors[axis] = error;
            }

            // 誤差が最小の次元を選択
            int best_axis = Array.IndexOf(errors, errors.Min());
            this.axis = best_axis;
            this.sign = signs[best_axis];
            this.threshold = thresholds[best_axis];
        }
        public int[] DpPredict(double[,] X)
        {
            int N = X.GetLength(0);
            int[] predictions = new int[N];

            for (int i = 0; i < N; i++)
            {
                double value = X[i, axis.Value];
                predictions[i] = (value < threshold) ? -sign.Value : sign.Value;
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

            for (int m = 0; m < num_classifiers; m++)
            {
                //基本分類器の訓練
                classifiers[m].Fit(X, label, weight);

                //誤りの判定
                
                int[] base_pred = classifiers[m].DpPredict(X); // 同じ値をN回繰り返す
                bool[] miss = base_pred.Zip(label, (pred, actual) => pred != actual).ToArray();
                for(int i = 0; i < base_pred.Length; i++)
                {
                    Console.WriteLine($"{base_pred[i]}, {label[i]}");
                }
               
                //for(int i = 0; i < miss.GetLength(0); i++)
                //{
                //    if (miss[i] == false)
                //    {
                //        //Console.WriteLine("Fale");
                //    }else if (miss[i] == true)
                //    {
                //        Console.WriteLine($"{i}");
                //    }
                //}
               
                //イプシロンの計算
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
                int[] predictions = classifiers[i].DpPredict(X);

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

   
}
