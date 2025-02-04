using AdaBoostAlgorithm;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Windows;
using OxyPlot.Wpf;
using System.Diagnostics.CodeAnalysis;

namespace DataProcessing
{
    //csv読み込み
    class READCSV
    {
        //読み込みと出力を分ける
        public (double[,] X, int[] label) Read(string file_path)
        {
            try
            {
                var lines = File.ReadAllLines(file_path).Skip(1).ToArray(); //1行目をのぞいた列の長さ   
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

    //その他vデータの処理
    class TOOL
    {
        public void MakeSplitData(string before_file_path)
        {
            if (string.IsNullOrEmpty(before_file_path))
            {
                MessageBox.Show("ファイルがが選択されていません。", "エラー");
            }
            else
            {
                READCSV read_csv = new READCSV();
                var (X_data, label_data) = read_csv.Read(before_file_path);
                var data = new List<DataRow>();

                for (int i = 0; i < label_data.Length; i++)
                {
                    data.Add(new DataRow { x = X_data[i, 0], y = X_data[i, 1], label = label_data[i] });
                }

                // ランダムシード値を固定
                int seed = 42;
                var random = new Random(seed);
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

        public (string train_data, string test_data) SplitData(string file_path) //データ教師とテストに分割
        {
            //分割したデータの保存先のファイルパス
            string train_name = "./splitTrainData.csv";
            string test_name = "./splitTestData.csv";
            string train_data_path = string.Empty;
            string test_data_path = string.Empty;

            MakeSplitData(file_path);
            train_data_path = train_name;
            test_data_path = test_name;

            return (train_data_path, test_data_path);
        }
    }

    //交差検証法による評価
    class CLOSSVALIDATION
    {
        private List<PlotView> plots = new List<PlotView>(); // 各フォールドのプロットを保存するリスト
        public double average_score { get; private set; }  // プロパティとして保持

        public void CLOSSVALIDATIONMETHOD(int k, int weak_id, double[,] data, int[] label)
        {
            var (fold_data, fold_label) = KFoldSplit(data, label, k);
            double sum_score = 0;

            for (int i = 0; i < fold_label.Count; i++)
            {
                var test_data = ConvertListTo2DArray(fold_data[i]);
                var test_label = fold_label[i].ToArray();
                var train_data = ConvertListTo2DArray(fold_data.Where((_, index) => index != i).SelectMany(f => f).ToList());
                var train_label = fold_label.Where((_, index) => index != i).SelectMany(f => f).ToList().ToArray();
                
                ADABOOST adaboost = new ADABOOST(weak_id);
                adaboost.Fit(train_data, train_label);

                int[] prediction = adaboost.Predict(test_data);

                double accuracy = AccuracyScore(test_label, prediction);

                sum_score += accuracy;

                PLOT plotter = new PLOT();
                PlotView plotView = plotter.PlotDecisionRegion(test_data, test_label, accuracy, adaboost);
                plots.Add(plotView);
            }
            average_score = sum_score / k;
            Console.WriteLine($"avrage: {average_score:P2}"); //スコア平均
        }

        public List<PlotView> GetPlots()
        {
            return plots;
        }

        //データをf個に分割
        private (List<List<double[]>> fold_data, List<List<int>> fold_label) KFoldSplit(double[,] data, int[] label, int f)
        {
            int seed = 42; //シード地を固定
            var random = new Random(seed);

            var data_list = new List<(double[], int)>(); //データとラベルを結合
            int row_count = data.GetLength(0);// 列の長さ
            int col_count = data.GetLength(1);// 行の長さ

            List<int[]> row = new List<int[]>();

            for (int i = 0; i < row_count; i++)
            {
                double[] temp = new double[2]; //一時的の意保存するための配列を作成
                temp[0] = data[i, 0];
                temp[1] = data[i, 1];
                data_list.Add((temp, label[i]));
            }

            var shuffled_data = data_list.OrderBy(x => random.Next()).ToList();

            // フォールドに分割
            var fold_data = new List<List<double[]>>();
            var fold_label = new List<List<int>>();
            int fold_size = (int)Math.Ceiling((double)shuffled_data.Count / f);

            for (int i = 0; i < f; i++)
            {
                var fold = shuffled_data.Skip(i * fold_size).Take(fold_size).ToList();
                var data_part = fold.Select(item => item.Item1).ToList(); // 特徴量
                var label_part = fold.Select(item => item.Item2).ToList(); // ラベル

                fold_data.Add(data_part);
                fold_label.Add(label_part);
            }
            return (fold_data, fold_label);
        }

        private static double[,] ConvertListTo2DArray(List<double[]> list) //listをdouble[,]に変換
        {
            int row_count = list.Count;
            int colCount = list[0].Length;
            var result = new double[row_count, colCount];
            for (int i = 0; i < row_count; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    result[i, j] = list[i][j];
                }
            }
            return result;
        }

        //正答率計算
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

            return (double)correct_count / true_label.Length;
        }
    }
}
   

