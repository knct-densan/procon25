using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace chino.Sorters
{
    /// <summary>
    /// 各Sorter共通処理
    /// </summary>
    static class Common
    {
        /// <summary>
        /// diffImgのメモ
        /// </summary>
        private static long[,,] _diffLog;

        /// <summary>
        /// 初期化する
        /// </summary>
        public static void init()
        {
            _diffLog = new long[Problem.partNum, Problem.partNum, 4];
            for (int i = 0; i < Problem.partNum; i++)
            {
                for (int j = i + 1; j < Problem.partNum; j++)
                {
                    _diffLog[i, j, 0] = -1;
                    _diffLog[i, j, 1] = -1;
                    _diffLog[i, j, 2] = -1;
                    _diffLog[i, j, 3] = -1;
                }
            }
        }

        /// <summary>
        /// -1で初期化された2Dマップを返す
        /// </summary>
        /// <returns>全要素が-1で初期化された row x column サイズの二次元配列</returns>
        public static int[,] initmap2d(int row, int column)
        {
            int[,] map2d = new int[row, column];

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    map2d[i, j] = -1;
                }
            }

            return map2d;
        }

        /// <summary>
        /// 初期化された問題サイズの2Dマップを返す
        /// </summary>
        /// <returns></returns>
        public static int[,] initmap2d()
        {
            return initmap2d(Problem.row, Problem.column);
        }

        /// <summary>
        /// ピース1(番地がaddr1) から見たときの pos の位置で ピース2(番地がaddr2) を配置し, 画素値の差分を取る.
        /// </summary>
        /// <param name="addr1"></param>
        /// <param name="addr2"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static long diffImg(int addr1, int addr2, Position pos)
        {
            if (addr2 < addr1)
            {
                Util.swap(ref addr1, ref addr2);
                switch (pos)
                {
                    case Position.UP:
                        pos = Position.DOWN;
                        break;
                    case Position.LEFT:
                        pos = Position.RIGHT;
                        break;
                    case Position.RIGHT:
                        pos = Position.LEFT;
                        break;
                    case Position.DOWN:
                        pos = Position.UP;
                        break;
                }
            }

            long log = _diffLog[addr1, addr2, (int)pos];
            if (log != -1)
            {
                return log;
            }

            long diff = 0;
            if (pos == Position.UP || pos == Position.DOWN)
            {
                int i1;
                int i2;

                if (pos == Position.UP)
                {
                    i1 = 0;
                    i2 = Problem.partHeight - 1;
                }
                else 
                {
                    i1 = Problem.partHeight - 1;
                    i2 = 0;
                }

                for (int j = 0; j < Problem.partWidth; j++)
                {
                    diff += Math.Abs(Problem.imgMaps[addr1, i1, j, 0] - Problem.imgMaps[addr2, i2, j, 0]);
                    diff += Math.Abs(Problem.imgMaps[addr1, i1, j, 1] - Problem.imgMaps[addr2, i2, j, 1]);
                    diff += Math.Abs(Problem.imgMaps[addr1, i1, j, 2] - Problem.imgMaps[addr2, i2, j, 2]);
                }
            }
            else if (pos == Position.RIGHT || pos == Position.LEFT)
            {
                int j1;
                int j2;

                if (pos == Position.RIGHT)
                {
                    j1 = Problem.partWidth - 1;
                    j2 = 0;
                }
                else
                {
                    j1 = 0;
                    j2 = Problem.partWidth - 1;
                }

                for (int i = 0; i < Problem.partHeight; i++)
                {
                    diff += Math.Abs(Problem.imgMaps[addr1, i, j1, 0] - Problem.imgMaps[addr2, i, j2, 0]);
                    diff += Math.Abs(Problem.imgMaps[addr1, i, j1, 1] - Problem.imgMaps[addr2, i, j2, 1]);
                    diff += Math.Abs(Problem.imgMaps[addr1, i, j1, 2] - Problem.imgMaps[addr2, i, j2, 2]);
                }
            }

            return (_diffLog[addr1, addr2, (int)pos] = diff);
        }
    }
}
