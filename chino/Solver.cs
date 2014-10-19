using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace chino
{
    /// <summary>
    /// 並び替え後のマップを元に, 回答を生成するメソッド群
    /// </summary>
    static class Solver
    {
        //
        //  == Solver API == 
        //
        //  1. 回答を得る   [ run(answerMap, solverNum) ]
        //
          
        //
        //  == Solvers API == (Solverによってディスパッチされる)
        //
        //  1. 回答を得る   [ run(map2d) ]
        //
        //  各ソルバーは, map2dを 0, 1, 2, .. の並びにするような回答を返せば良い
        //

        /// <summary>
        /// 0, 1, 2, .. で並んだ2DマップをanswerMapの順に並び替える回答を生成する
        /// </summary>
        /// <param name="answerMap">目標のマップ</param>
        /// <returns>回答</returns>
        public static String run(int[] answerMap, int solverNum)
        {
            //
            // 処理の内部では実装しやすいように, 目的の状態を 0, 1, 2, .. とする
            // そのため初期状態を変換する (生成される回答は同一となる)
            //
            // 0, 1, 2, ... => answerMap
            //
            // ||  変換
            // \/
            //
            // a, b, c, ... => 0, 1, 2, ..
            //
            // 

            // 変換
            int[] initialMapTranslated = new int[Problem.partNum];
            for (int i = 0; i < Problem.partNum; i++)
            {
                initialMapTranslated[answerMap[i]] = i;
            }

            // 2Dマップの生成
            int[,] map2d = new int[Problem.row, Problem.column];
            for (int i = 0; i < Problem.row; i++)
            {
                for (int j = 0; j < Problem.column; j++)
                {
                    map2d[i, j] = initialMapTranslated[i * Problem.column + j];
                }
            }

            switch(solverNum)
            {
                case 0:
                    return Solvers.Solver0.run(map2d);
                case 1:
                    return Solvers.Solver1.run(map2d);
                case 2:
                    return Solvers.y1r.run(map2d);
            }
            return null;
        }
    }
}
