using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chino.Sorters
{
    /// <summary>
    /// Sorterアルゴリズム1 ( http://ci.nii.ac.jp/naid/110006549357 の最優秀アルゴリズム )
    /// 
    /// 精度: 95%~
    /// 速度: RyuJIT上だと16x16が5秒.
    /// オーダー: O((N!)^2) [N: 分割数]
    /// </summary>
    static class Sorter1
    {
        ///
        /// このアルゴリズムの実装において一番の難所は, ブロック同士の結合である.
        /// 最終的にブロック同士が結合できなかった場合は,一番大きいブロックを元にし,
        /// 二番目以降に大きいブロックを崩してアルゴリズム0に適用する.
        ///
        /// "接合ピースがp_{rank1}の個数分, 評価値Cを乗算する"とあるが,
        /// 個数が0で, 元々の評価値が0以下であった場合には, 元々の評価値を用いる.
        ///

        /// <summary>
        /// 一つのピースとポジションにおける, 他ピースとの差分を小さい順で格納する. 
        /// 配列の第一インデクサはピース番号, 二番目はボジション. 
        /// それぞれの要素は 結合するピース番号 と 差分 を格納したタプルのリスト
        /// </summary>
        private static List<Tuple<int, long>>[,] _diffLists;

        /// <summary>
        /// 初期化する
        /// </summary>
        public static void init()
        {
            // アルゴリズム1を2度目以降使用するために前のオブジェクトを破棄する
            _diffLists = null;
        }

        /// <summary>
        /// 現在のanswerMapから, 元画像を復元しその2Dマップを返す
        /// </summary>
        /// <param name="answerMap"></param>
        /// <returns></returns>
        public static int[,] run(int[] answerMap)
        {
            // _diffListsの構築
            constructDiffLists();
            
            // ピース同士を結合させ, ブロックを形成する
            List<int[,]> blocks = join(answerMap);

            // 一番大きなブロックを元に答えマップを作成する
            blocks.Sort((x, y) => (y.GetLength(0) * y.GetLength(1)) - (x.GetLength(0) * x.GetLength(1))); // 降順ソート
            int[,] map2d = expand(blocks[0]);

            // サイズが2以上なら(= 完成しなかったなら)
            if (blocks.Count > 1)
            {
                // 二番目以降に大きいブロックに属していたピースのリスト
                List<int> rest = new List<int>();

                for (int k = 1; k < blocks.Count; k++)
                {
                    int[,] block = blocks[k];
                    int row = block.GetLength(0);
                    int column = block.GetLength(1);

                    for (int i = 0; i < row; i++)
                    {
                        for (int j = 0; j < column; j++)
                        {
                            if (block[i, j] != -1)
                            {
                                rest.Add(block[i, j]);
                            }
                        }
                    }
                }

                // アルゴリズム0に適用する
                Sorter0.core(map2d, rest);
            }

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
            // Sorter0のものを流用する
            return Sorter0.run(answerMap, answerMapFixed);
        }


        /// <summary>
        /// _diffListsを構築する
        /// </summary>
        private static void constructDiffLists()
        {
            if (_diffLists == null)
            {
                _diffLists = new List<Tuple<int,long>>[Problem.partNum, 4];

                for (int i = 0; i < Problem.partNum; i++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        _diffLists[i, k] = new List<Tuple<int, long>>();
                        Position pos = (Position)k;

                        for (int j = 0; j < Problem.partNum; j++)
                        {
                            _diffLists[i, k].Add(new Tuple<int, long>(j, Common.diffImg(i, j, pos)));
                        }

                        // 自分自身を削除
                        _diffLists[i, k].RemoveAt(i);

                        // ソート
                        _diffLists[i, k].Sort((x, y) => x.Item2.CompareTo(y.Item2));
                    }
                }
            }
        }

        /// <summary>
        /// 受け取ったブロックを指定サイズのブロックに拡大する (縦も横も元のサイズより大きくなければならない)
        /// </summary>
        /// <param name="block"></param>
        /// <returns>拡大されたブロック</returns>
        private static int[,] expand(int[,] block, int row, int column)
        {
            int[,] ret = Common.initmap2d(row, column);
            int blockRow = block.GetLength(0);
            int blockColumn = block.GetLength(1);

            // 元のブロックを写す
            for (int i = 0; i < blockRow; i++)
            {
                for (int j = 0; j < blockColumn; j++)
                {
                    ret[i, j] = block[i, j];
                }
            }

            return ret;
        }

        /// <summary>
        /// 受け取ったブロックを問題サイズのブロックに拡大する
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private static int[,] expand(int[,] block)
        {
            return expand(block, Problem.row, Problem.column);
        }

        /// <summary>
        /// ピースやブロックの組み合わせの中で最も評価の高いものを結合する作業をつづけ,
        /// 最終的に残ったブロックを返す.
        /// </summary>
        /// <param name="pieces"></param>
        /// <returns></returns>
        private static List<int[,]> join(int[] pieces)
        {
            // ブロック(1つ以上のピースが結合された2次元マップ)のリストを構築
            List<int[,]> blocks = new List<int[,]>();
            for (int i = 0; i <  pieces.Length; i++) {
                int[,] block = new int[1, 1];
                block[0, 0] = pieces[i];
                blocks.Add(block);
            }

            while (true)
            {
                long[,] result = new long[blocks.Count, blocks.Count];
                Parallel.For(0, blocks.Count, i =>
                {
                    for (int j = i + 1; j < blocks.Count; j++)
                    {
                        result[i, j] = searchTheBestJoint(blocks[i], blocks[j]).Item5;
                    }
                });

                long best = long.MinValue;
                int bestI = -1, bestJ = -1;
                for (int i = 0; i < blocks.Count; i++)
                {
                    for (int j = i + 1; j < blocks.Count; j++)
                    {
                        if (result[i, j] > best)
                        {
                            best = result[i, j];
                            bestI = i;
                            bestJ = j;
                        }
                    }
                }

                // これ以上ブロックの結合ができない場合は終了
                if (best == long.MinValue)
                {
                    break;
                }

                // 最も評価値の高い組み合わせを格納する
                // タプルの要素
                //   1つめ: blocks における block1 の位置
                //   2つめ: blocks における block2 の位置
                //   3つめ: block1の行番号 (i1)
                //   4つめ: block1の列番号 (j1)
                //   5つめ: block2の行番号 (i2)
                //   6つめ: block2の列番号 (j2)
                //     block1とblock2は(i1, j1)と(j1,j2)で結合される
                //   7つめ: 評価値
                Tuple<int, int, int, int, int, int, long> bestCombi;
                var tmp = searchTheBestJoint(blocks[bestI], blocks[bestJ]);
                bestCombi = new Tuple<int, int, int, int, int, int, long>(
                    bestI, bestJ, tmp.Item1, tmp.Item2, tmp.Item3, tmp.Item4, tmp.Item5);

                // 結合したブロックを追加
                blocks.Add(joinTwoBlocks(
                    blocks[bestCombi.Item1],
                    blocks[bestCombi.Item2],
                    bestCombi.Item3,
                    bestCombi.Item4,
                    bestCombi.Item5,
                    bestCombi.Item6));

                // 結合前のブロックを削除
                if (bestCombi.Item1 > bestCombi.Item2)
                {
                    blocks.RemoveAt(bestCombi.Item1);
                    blocks.RemoveAt(bestCombi.Item2);
                }
                else
                {
                    blocks.RemoveAt(bestCombi.Item2);
                    blocks.RemoveAt(bestCombi.Item1);
                }
            }

            return blocks;
        }

        /// <summary>
        /// 2つのブロックに関して, 最も評価値の高い結合部分を探す
        /// </summary>
        /// <param name="block1"></param>
        /// <param name="block2"></param>
        /// <returns>bestCombiの第三要素以降のタプル (joinメソッド内のコメントを参照してください)</returns>
        private static Tuple<int, int, int, int, long>
            searchTheBestJoint(int[,] block1, int[,] block2)
        {
            int block1Row = block1.GetLength(0);
            int block1Column = block1.GetLength(1);
            int block2Row = block2.GetLength(0);
            int block2Column = block2.GetLength(1);
            List<Tuple<int, int, int, int>> joints = new List<Tuple<int, int, int, int>>(); // bestCombiの第3~6要素
            Tuple<int, int, int, int> bestJoint = new Tuple<int, int, int, int>(-1, -1, -1, -1); // 評価が最高の結合
            long bestEvalValue = long.MinValue; // 評価値の最高


            //
            // joints(結合アドレス集合)を構築する
            //

            // block1の内部,右端,下端　と block2の [0, 0] の結合
            for (int i = 0; i <= block1Row; i++)
            { 
                for (int j = 0; j <= block1Column; j++)
                {
                    joints.Add(new Tuple<int,int,int,int>(i, j, 0, 0));
                }
            }

            // block1の左端,上端 と block2 内部の結合
            
            // 左端 
            for (int j = 1; j <= block2Column; j++) // 終了条件は '<='
            { 
                // block1の[i, 0]と, block2の[0, j]を結合させる
                for (int i = block1Row - 1; i >= 0; i--)
                {
                    joints.Add(new Tuple<int, int, int, int>(i, 0, 0, j));
                }
                // block1の[0, 0]と, block2の[i, j]を結合させる
                for (int i = 1; i <= block2Row; i++)
                {
                    joints.Add(new Tuple<int, int, int, int>(0, 0, i, j));
                }
            }

            // 上端 
            for (int i = 1; i <= block2Row; i++)
            {
                // block1の[0, j]と, block2の[i, 0]を結合させる
                for (int j = block1Column - 1; j >= 0; j--)
                {
                    joints.Add(new Tuple<int, int, int, int>(0, j, i, 0));
                }
                // block1の[0, 0]と, block2の[i, j]を結合させる
                for (int j = 1; j <= block2Column; j++)
                {
                    joints.Add(new Tuple<int, int, int, int>(0, 0, i, j));
                }
            }

            foreach(var j in joints)
            {
                // それぞれのjointを評価する
                long v = evalJoint(block1, block2, j.Item1, j.Item2, j.Item3, j.Item4);

                // 評価が最も良かった場合
                if (v > bestEvalValue)
                {
                    // 更新
                    bestJoint = j;
                    bestEvalValue = v;
                }
            }

            return new Tuple<int, int, int, int, long>(
                bestJoint.Item1, bestJoint.Item2, bestJoint.Item3, bestJoint.Item4, bestEvalValue);
        }

        /// <summary>
        /// block1の[i1, j1]とblock2の[i2, j2]で結合を行った場合の評価値を計算する
        /// </summary>
        /// <remarks>
        /// 指定する位置が大きい方向にはみ出た位置([i, column])になることはある
        /// ([-1, n] や [i1, j1]と[i2, j2]の両方がはみ出た位置になる場合は無視)
        /// </remarks>
        /// <param name="block1"></param>
        /// <param name="block2"></param>
        /// <param name="i1"></param>
        /// <param name="j1"></param>
        /// <param name="i2"></param>
        /// <param name="j2"></param>
        /// <returns>評価できなかった場合には long.MinValue が返る</returns>
        private static long evalJoint(int[,] block1, int[,] block2, int i1, int j1, int i2, int j2)
        {
            // blockのサイズ
            int block1Row = block1.GetLength(0);
            int block1Column = block1.GetLength(1);
            int block2Row = block2.GetLength(0);
            int block2Column = block2.GetLength(1);

            // block1とblock2の位置合わせ (正方向)
            int block1RowPad = 0;
            int block1ColumnPad = 0;
            int block2RowPad = 0;
            int block2ColumnPad = 0;

            if (i1 > i2)
            {
                block2RowPad = i1 - i2;
            }
            else
            {
                block1RowPad = i2 - i1;
            }

            if (j1 > j2)
            {
                block2ColumnPad = j1 - j2;
            }
            else
            {
                block1ColumnPad = j2 - j1;
            }

            // 結合後のサイズ
            int joinedRow = Math.Max(block1Row + block1RowPad, block2Row + block2RowPad);
            int joinedColumn = Math.Max(block1Column + block1ColumnPad, block2Column + block2ColumnPad);

            // サイズチェック: 結合後のサイズが, 最終のマップサイズをはみ出していた場合終了
            if(!(joinedRow <= Problem.row && joinedColumn <= Problem.column))
            {
                return long.MinValue;
            }

            //
            // 評価を行う
            //

            // 評価用
            int rank1Num = 0;
            long evalValue = 0;

            for (int i = 0; i < block1Row; i++)
            {
                for (int j = 0; j < block1Column; j++)
                {
                    // blankなら飛ばす
                    if (block1[i, j] == -1)
                    {
                        continue;
                    }

                    // 結合されたマップ上でのアドレス
                    int block1_i_OnJoined = i + block1RowPad;
                    int block1_j_OnJoined = j + block1ColumnPad;

                    // [i, j] と結合する block2 におけるピースのアドレス
                    int block2_i = block1_i_OnJoined - block2RowPad;
                    int block2_j = block1_j_OnJoined - block2ColumnPad;

                    // 高速化のため結果を変数に置く
                    bool rowOnRegion = block2_i >= 0 && block2_i < block2Row;
                    bool columnOnRegion = block2_j >= 0 && block2_j < block2Column;

                    // 結合する箇所がどちらともblankでなかった場合は終了
                    if (rowOnRegion && columnOnRegion
                        && block2[block2_i, block2_j] != -1)
                    {
                        return long.MinValue;
                    }

                    // 上下左右に関して評価する
                    if (rowOnRegion)
                    {
                        int tmp;

                        // 左
                        tmp = block2_j - 1;
                        if (tmp >= 0 && tmp < block2Column && block2[block2_i, tmp] != -1)
                        {
                            long v = evalTwoPieces(block1[i, j], block2[block2_i, tmp], Position.LEFT);
                            evalValue += v;
                            if (v >= 0)
                            {
                                rank1Num++;
                            }
                        }

                        // 右
                        tmp = block2_j + 1;
                        if (tmp >= 0 && tmp < block2Column && block2[block2_i, tmp] != -1)
                        {
                            long v = evalTwoPieces(block1[i, j], block2[block2_i, tmp], Position.RIGHT);
                            evalValue += v;
                            if (v >= 0)
                            {
                                rank1Num++;
                            }
                        }
                    }
                    if (columnOnRegion)
                    {
                        int tmp;

                        // 上
                        tmp = block2_i - 1;
                        if (tmp >= 0 && tmp < block2Row && block2[tmp, block2_j] != -1)
                        {
                            long v = evalTwoPieces(block1[i, j], block2[tmp, block2_j], Position.UP);
                            evalValue += v;
                            if (v >= 0)
                            {
                                rank1Num++;
                            }
                        }

                        // 下
                        tmp = block2_i + 1;
                        if (tmp >= 0 && tmp < block2Row && block2[tmp, block2_j] != -1)
                        {
                            long v = evalTwoPieces(block1[i, j], block2[tmp, block2_j], Position.DOWN);
                            evalValue += v;
                            if (v >= 0)
                            {
                                rank1Num++;
                            }
                        }
                    }
                }
            }

            // 評価値が0以下ならばその値を返す
            if (evalValue <= 0)
            {
                return evalValue;
            }

            return evalValue * rank1Num;
        }

        /// <summary>
        /// 参考アルゴリズムの評価関数. ピース1(番地がaddr1) から見たときの pos の位置で ピース2(番地がaddr2) を配置し, 画素値の差分を取る.
        /// </summary>
        /// <param name="piece1"></param>
        /// <param name="piece2"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private static long evalTwoPieces(int addr1, int addr2, Position pos)
        {
            var diffList = _diffLists[addr1, (int)pos];
            int rank = diffList.FindIndex(t => t.Item1 == addr2);

            //
            // 注意:
            //   最小ピース数は4.
            //

            if (rank == 0) // rank1
            {
                return (diffList[1].Item2 - diffList[0].Item2);
            }
            else if (rank <= 8) // rank2 ~ 9
            {
                return (diffList[0].Item2 - diffList[rank].Item2);
            }

            // rank10~
            return diffList[0].Item2 - diffList[9].Item2;
        }

        /// <summary>
        /// block1とblock2を結合させる　(block1の[i1, j1]と[i2, j2]で結合を行う)
        /// </summary>
        /// <param name="block1"></param>
        /// <param name="block2"></param>
        /// <param name="i1"></param>
        /// <param name="j1"></param>
        /// <param name="i2"></param>
        /// <param name="j2"></param>
        /// <returns></returns>
        private static int[,] joinTwoBlocks(
            int[,] block1, int[,] block2, int i1, int j1, int i2, int j2)
        {
            // blockのサイズ
            int block1Row = block1.GetLength(0);
            int block1Column = block1.GetLength(1);
            int block2Row = block2.GetLength(0);
            int block2Column = block2.GetLength(1);

            // block1とblock2の位置合わせ (正方向)
            int block1RowPad = 0;
            int block1ColumnPad = 0;
            int block2RowPad = 0;
            int block2ColumnPad = 0;

            if (i1 > i2)
            {
                block2RowPad = i1 - i2;
            }
            else
            {
                block1RowPad = i2 - i1;
            }

            if (j1 > j2)
            {
                block2ColumnPad = j1 - j2;
            }
            else
            {
                block1ColumnPad = j2 - j1;
            }

            // 結合後のサイズ
            int joinedRow = Math.Max(block1Row + block1RowPad, block2Row + block2RowPad);
            int joinedColumn = Math.Max(block1Column + block1ColumnPad, block2Column + block2ColumnPad);

            // 結合後のマップ
            int[,] joined = Common.initmap2d(joinedRow, joinedColumn);

            // block1を結合後のマップに写す
            for (int i = 0; i < block1Row; i++)
            {
                for (int j = 0; j < block1Column; j++)
                {
                    joined[i + block1RowPad, j + block1ColumnPad] = block1[i, j];
                }
            }

            // block2を結合後のマップに写す
            for (int i = 0; i < block2Row; i++)
            {
                for (int j = 0; j < block2Column; j++)
                {
                    if (block2[i, j] != -1)
                    {
                        joined[i + block2RowPad, j + block2ColumnPad] = block2[i, j];
                    }
                }
            }

            return joined;
        }
    }
}
