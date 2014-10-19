using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace chino.Sorters
{
    /// <summary>
    /// Sorterアルゴリズム0 (自作簡易アルゴリズム)
    /// 確定していないピースとエッジの組み合わせ中で最も画素値差分が小さいものを確定させていく
    ///
    /// 精度: 80~90% (手動組み合わせ用)
    /// 速度: 速い (16x16で2,3秒, 3x3でゼロコンマ数秒)
    /// </summary>
    static class Sorter0
    {
        /// <summary>
        /// マップが固定されているならtrue (coreを外部公開するために初期値をfalseにしておく)
        /// </summary>
        private static bool _mapFixed = false;

        /// <summary>
        /// 初期化する
        /// </summary>
        public static void init()
        {
            // do nothing
        }

        /// <summary>
        /// 現在のanswerMapから, 元画像を復元しその2Dマップを返す
        /// </summary>
        /// <param name="answerMap"></param>
        /// <returns></returns>
        public static int[,] run(int[] answerMap)
        {
            int[,] map2d = Common.initmap2d();
            List<int> rest = new List<int>(answerMap); // マップに追加されてないピースのリスト

            // 先頭を左上に追加
            map2d[0, 0] = rest[0];
            rest.RemoveAt(0);

            core(map2d, rest);
            return map2d;
        }

        /// <summary>
        /// 特定のピースを固定し, 元画像の復元を行う.
        /// </summary>
        /// <param name="answerMap"></param>
        /// <param name="answerMapFixed"></param>
        /// <returns></returns>
        public static int[,] run(int[] answerMap, bool[] answerMapFixed)
        {
            int[,] map2d = Common.initmap2d();
            List<int> rest = new List<int>();

            // 固定されたピースをマップにはめ込む
            for (int i = 0; i < Problem.row; i++)
            {
                for (int j = 0; j < Problem.column; j++)
                {
                    int addr = i * Problem.column + j;
                    if (answerMapFixed[addr])
                    {
                        _mapFixed = true;
                        map2d[i, j] = answerMap[addr];
                    }
                    else
                    {
                        rest.Add(answerMap[addr]);
                    }
                }
            }

            if (_mapFixed == false)
            {
                // 一箇所を左上に追加
                int i = Util.rand.Next(Problem.partNum);
                map2d[0, 0] = rest[i];
                rest.RemoveAt(i);
            }

            core(map2d, rest);
            _mapFixed = false; // マップの固定を外す
            return map2d;
        }

        /// <summary>
        /// Sorter0のコア部分. map2dに残りのピース(rest)をはめ込んでいく. (map2dは書き換えられる)
        /// </summary>
        /// <param name="map2d"></param>
        /// <param name="rest"></param>
        public static void core(int[,] map2d, List<int> rest)
        {
            while (true)
            {
                List<int> edges = allEdges(map2d);

                // すべてが-1でなければ終了
                if (edges.Count == 0)
                {
                    break;
                }

                long bestDiff = long.MaxValue;
                int best = -1;  // 最も差分の小さいrestのアドレス
                int bestY = -1; // bestの入る番地Y
                int bestX = -1; // bestの入る番地X

                for (int i = 0; i < edges.Count; i += 2)
                {
                    int y = edges[i];
                    int x = edges[i + 1];

                    for (int j = 0; j < rest.Count; j++)
                    {
                        long diff = 0;
                        int denominator = 0; // 接続数
                        if (x >= 0)
                        {
                            // 上
                            if (y > 0 && map2d[y - 1, x] != -1)
                            {
                                diff += Common.diffImg(rest[j], map2d[y - 1, x], Position.UP);
                                denominator++;
                            }
                            // 下
                            if (y < Problem.row - 1 && map2d[y + 1, x] != -1)
                            {
                                diff += Common.diffImg(rest[j], map2d[y + 1, x], Position.DOWN);
                                denominator++;
                            }
                        }
                        if (y >= 0)
                        {
                            // 左
                            if (x > 0 && map2d[y, x - 1] != -1)
                            {
                                diff += Common.diffImg(rest[j], map2d[y, x - 1], Position.LEFT);
                                denominator++;
                            }
                            // 右
                            if (x < Problem.column - 1 && map2d[y, x + 1] != -1)
                            {
                                diff += Common.diffImg(rest[j], map2d[y, x + 1], Position.RIGHT);
                                denominator++;
                            }
                        }

                        diff /= denominator;

                        if (diff < bestDiff)
                        {
                            bestDiff = diff;
                            best = j;
                            bestY = y;
                            bestX = x;
                        }
                    }
                }

                {
                    int y = bestY;
                    int x = bestX;

                    if (y == -1)
                    {
                        shiftBottom(map2d); // map2dの要素をすべて下にシフト
                        y++;
                    }
                    else if (x == -1)
                    {
                        shiftLeft(map2d); // map2dの要素をすべて右にシフト
                        x++;
                    }

                    map2d[y, x] = rest[best];
                    rest.RemoveAt(best);
                }
            }
        }
        
        /// <summary>
        /// ２次元配列を右に一つシフトする. 一番右の要素は一番左の値となる.
        /// </summary>
        /// <param name="map2d"></param>
        private static void shiftLeft(int[,] map2d)
        {
            int row = map2d.GetLength(0);
            int column = map2d.GetLength(1);
            for (int i = 0; i < row; i++)
            {
                int j = column - 1;
                int tmp = map2d[i, j];
                for (; j > 0; j--)
                {
                    map2d[i, j] = map2d[i, j - 1];
                }
                map2d[i, 0] = tmp;
            }
        }

        /// <summary>
        /// ２次元配列を下に一つシフトする. 一番下の要素は一番上の値となる.
        /// </summary>
        /// <param name="map2d"></param>
        private static void shiftBottom(int [,] map2d)
        {
            int row = map2d.GetLength(0);
            int column = map2d.GetLength(1);
            for (int j = 0; j < column; j++)
            {
                int i = row - 1;
                int tmp = map2d[i, j];
                for (; i > 0; i--)
                {
                    map2d[i, j] = map2d[i - 1, j];
                }
                map2d[0, j] = tmp;
            }
        }

        /// <summary>
        /// map2dを受け取り, 値が-1でない番地に隣接している,値が-1のアドレスを返す
        /// </summary>
        /// <param name="map2d"></param>
        /// <returns>値が-1でない番地に隣接している,値が-1のアドレスを格納したList<int> (y1, x1, y2, x2, y3, ...) </returns>
        private static List<int> allEdges(int[,] map2d)
        {
            List<int> ret = new List<int>();
            int row = map2d.GetLength(0);
            int column = map2d.GetLength(1);
            int i;
            int j;

            for (i = 0; i < row; i++)
            {
                for (j = 0; j < column; j++)
                {
                    if (map2d[i, j] == -1
                        &&
                        ((j > 0 && map2d[i, j - 1] != -1) // 左
                        || (j < column - 1 && map2d[i, j + 1] != -1) // 右
                        || (i > 0 && map2d[i - 1, j] != -1) // 上
                        || (i < row - 1 && map2d[i + 1, j] != -1))) // 下 
                    {
                        ret.Add(i);
                        ret.Add(j);
                    }
                }
            }

            if (_mapFixed == false)
            {
                // マップが固定されていない場合は, 内側([-1, x], [y, -1])も入れる
                bool flag = true;
                i = row - 1;
                var tmpList = new List<int>();

                // [-1, x]
                for (j = 0; j < column; j++)
                {
                    if (map2d[i, j] != -1)
                    {
                        flag = false;
                        break;
                    }
                    if (map2d[0, j] != -1)
                    {
                        tmpList.Add(-1);
                        tmpList.Add(j);
                    }
                }
                if (flag)
                {
                    ret.AddRange(tmpList);
                }

                flag = true;
                j = column - 1;
                tmpList.Clear();

                for (i = 0; i < row; i++)
                {
                    if (map2d[i, j] != -1)
                    {
                        flag = false;
                        break;
                    }
                    if (map2d[i, 0] != -1)
                    {
                        tmpList.Add(i);
                        tmpList.Add(-1);
                    }
                }
                if (flag)
                {
                    ret.AddRange(tmpList);
                }
            }
            return ret;
        }
    }
}
