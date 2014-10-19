using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace chino
{
    /// <summary>
    /// 問題データを表現するクラス。
    /// </summary>
    static class Problem
    {
        /// <summary>
        /// 原画像の横幅
        /// </summary>
        public static int width;

        /// <summary>
        /// 原画像の高さ
        /// </summary>
        public static int height;

        /// <summary>
        /// 分割個数
        /// </summary>
        public static int partNum;

        /// <summary>
        /// 横の分割数
        /// </summary>
        public static int column;

        /// <summary>
        /// 縦の分割数
        /// </summary>
        public static int row;

        /// <summary>
        /// 分割後の横幅
        /// </summary>
        public static int partWidth;

        /// <summary>
        /// 分割後の高さ
        /// </summary>
        public static int partHeight;

        /// <summary>
        /// 選択可能回数
        /// </summary>
        public static int selectionLimit;

        /// <summary>
        /// 選択コスト
        /// </summary>
        public static int selectionCost;

        /// <summary>
        /// 交換コスト
        /// </summary>
        public static int replacementCost;

        /// <summary>
        /// 最大輝度
        /// </summary>
        public static int colorMax;

        /// <summary>
        /// 表示用分割画像へのパスを保持する配列 [(0,0), (0,1), (N-1, M-1)] 
        /// M x N (Mが横, Nが縦: 募集要項ではこの順番) の分割画像の (i, j) 番のものを取得する場合は
        /// paths[M * i + j] と記述する。
        /// </summary>
        public static String[] imgPaths;

        /// <summary>
        /// 分割画像それぞれのデータマップ
        /// 1つめはn番目へのインデクサ(imgPaths同様にアクセス). 2つめはM行目. 3つめはN列目. 4つめはピクセル(0:R,1:G,2:B)
        /// </summary>
        public static byte[,,,] imgMaps;

        /// <summary>
        /// 問題ファイルへのパス
        /// </summary>
        private static String _originalFilePath;

        /// <summary>
        /// ファイルの保存先
        /// </summary>
        private static String _workingDirectoryPath;

        /// <summary>
        /// Problemの初期化を行う
        /// </summary>
        /// <param name="problemPath">問題ファイルへのパス</param>
        /// <param name="workingDirectoryPath">画像の保存先を指定</param>
        public static void init(String problemPath, String workingDirectoryPath)
        {
            _originalFilePath = problemPath;
            _workingDirectoryPath = workingDirectoryPath;

            // 問題情報の抽出
            extractInformation();

            // 分割画像の生成
            generateDivisionImages();
        }

        /// <summary>
        /// 問題情報の抽出
        /// </summary>
        /// invalidなファイルだった場合, 故意に例外を発生させる.
        private static void extractInformation()
        {
            FileStream stream = new FileStream(_originalFilePath, FileMode.Open, FileAccess.Read);
            String[] separator = { "#", " " };
            String[] line = new String[6];
            String[] temp;
            byte[] map; // M x N

            for (int i = 0; i < 6; i++) {
                char tmp;
                StringBuilder sb = new StringBuilder();
                while ((tmp = Convert.ToChar(stream.ReadByte())) != '\n') {
                    sb.Append(tmp);
                }
                line[i] = sb.ToString();
            }

            // LINE 0
            if (line[0] != "P6")
            {
                throw new Exception();
            }
            
            // LINE 1 (分割数)
            temp = line[1].Split(separator, StringSplitOptions.RemoveEmptyEntries);
            column = int.Parse(temp[0]);
            row = int.Parse(temp[1]);
            partNum = row * column;

            // LINE 2 (選択可能回数)
            selectionLimit = int.Parse(
                line[2].Split(separator, StringSplitOptions.RemoveEmptyEntries)[0]
                );

            // LINE 3 (レート)
            temp = line[3].Split(separator, StringSplitOptions.RemoveEmptyEntries);
            replacementCost = int.Parse(temp[0]);
            selectionCost = int.Parse(temp[1]);

            // LINE 4 (ピクセル数)
            temp = line[4].Split(separator, StringSplitOptions.RemoveEmptyEntries);
            width = int.Parse(temp[0]);
            height = int.Parse(temp[1]);

            // LINE 5 (最大輝度)
            colorMax = int.Parse(
                line[5].Split(separator, StringSplitOptions.RemoveEmptyEntries)[0]
                );

            // 断片画像のサイズ
            partWidth = width / column;
            partHeight = height / row;

            // LINE 6 (BINARY DATA)
            map = new byte[height * width * 3];
            if (stream.Read(map, 0, map.Length) != map.Length)
            {
                throw new Exception();
            }

            // Construct imgMaps
            imgMaps = new byte[partNum, partHeight, partWidth, 3];
            for (int i = 0; i < row; i++)
            {
                int padHeight = i * partHeight;
                for (int j = 0; j < column; j++)
                {
                    int padWidth = j * partWidth;
                    for (int k = 0; k < partHeight; k++)
                    {
                        for (int l = 0; l < partWidth; l++)
                        {
                            int addr1 = i * column + j;
                            int addr2 = ((padHeight + k) * width + (padWidth + l)) * 3;
                            imgMaps[addr1, k, l, 0] = map[addr2];
                            imgMaps[addr1, k, l, 1] = map[addr2+1];
                            imgMaps[addr1, k, l, 2] = map[addr2+2];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 分割画像の生成
        /// </summary>
        private static void generateDivisionImages()
        {
            String convertPath = Path.GetFullPath(@".\imagemagick\convert.exe");
            String bmpPath 
                = _workingDirectoryPath + @"\"
                + Path.GetFileName(_originalFilePath).Replace("ppm", "bmp");
            String partPath
                = _workingDirectoryPath + @"\" + @"part_%d.bmp";
            ProcessStartInfo info = new ProcessStartInfo();
            info.CreateNoWindow = true;
            info.UseShellExecute = false;

            // 画像変換
            info.FileName = convertPath;
            info.Arguments = _originalFilePath + " " + bmpPath;
            Process.Start(info).WaitForExit();

            // 画像分割
            info.FileName = convertPath;
            info.Arguments =
                @"-crop " + partWidth + "x" + partHeight + " " + bmpPath + " " + partPath;
            Process.Start(info).WaitForExit();

            // imgPathsの設定
            imgPaths = new String[partNum];
            for (int i = 0; i < partNum; i++)
            {
                imgPaths[i] = _workingDirectoryPath + @"\" + "part_" + i + ".bmp";
            }
        }
    }
}
