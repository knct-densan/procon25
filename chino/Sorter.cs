using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace chino
{
    /// <summary>
    /// 画像処理を行い, パズルの元の配置を導くメソッド群
    /// </summary>
    static class Sorter
    {
        //
        //  == Sorter API == 
        //
        //  1. 初期化        [ init(sorterNum) ]
        //  2. 実行(並び替え) [ run(answerMap), run(answerMap, answerMapFixed) ]
        //
        //  実行すると, 受け取ったanswerMapを, 指定したSorterに従って書き換える
        //

        //
        //  == Sorters API == (SortersはSorterによってディスパッチされる)
        //
        //  1. 初期化        [ init() ]
        //  2. 実行(並び替え) [ run(answerMap), run(answerMap, answerMapFixed) ]
        //
        //  実行してもSortersは受け取ったanswerMapを書き換えず, 並び替え後の2Dマップを返す 
        //

        /// <summary>
        /// 使用するSorterの番号
        /// </summary>
        private static int _sorterNum;

        /// <summary>
        /// Sorterの初期化
        /// </summary>
        /// <param name="sorterNum">使用するSorterの番号</param>
        public static void init(int sorterNum)
        {
            _sorterNum = sorterNum;
            Sorters.Common.init();
            Sorters.Sorter0.init();
            Sorters.Sorter1.init();
        }

        /// <summary>
        /// _answerMapの並び替え(書き換え)を行う
        /// </summary>
        /// <param name="answerMap">書き換え対象の答えマップ</param>
        public static void run(int[] answerMap)
        {
            switch (_sorterNum)
            {
                case 0:
                    setAnswerMap(answerMap, Sorters.Sorter0.run(answerMap));
                    break;
                case 1:
                    setAnswerMap(answerMap, Sorters.Sorter1.run(answerMap));
                    break;
            }
        }

        /// <summary>
        /// answerMapFixedに応じてmapの特定ピースを固定化したあと, 並び替え(書き換え)を行う
        /// </summary>
        /// <param name="answerMap">書き換え対象の答えマップ</param>
        /// <param name="answerMapFixed"></param>
        public static void run(int[] answerMap, bool[] answerMapFixed)
        {
            switch(_sorterNum)
            {
                case 0:
                    setAnswerMap(answerMap, Sorters.Sorter0.run(answerMap, answerMapFixed));
                    break;
                case 1:
                    setAnswerMap(answerMap, Sorters.Sorter1.run(answerMap, answerMapFixed));
                    break;
            }
        }

        /// <summary>
        /// map2dに従って, 内部表現のanswerMapを設定する
        /// </summary>
        private static void setAnswerMap(int[] answerMap, int[,] map2d)
        {
            int len0 = map2d.GetLength(0);
            int len1 = map2d.GetLength(1);
            for (int i = 0; i < len0; i++)
            {
                int margin = i * len1;
                for (int j = 0; j < len1; j++)
                {
                    answerMap[margin + j] = map2d[i, j];
                }
            }
        }
    }
}
