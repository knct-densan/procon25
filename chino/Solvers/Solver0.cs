using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace chino.Solvers
{
    /// <summary>
    /// Solverアルゴリズム0.
    /// 2x2の状態になるまで, 右端からパズルを1つ1つ確定させていく.
    /// Mx2(縦がM, 横が2)の状態になった場合は, パズル全体の転置を行う.
    /// </summary>
    static class Solver0
    {
        /// <summary>
        /// 書き換える対象の2次元マップ ([縦, 横]でアクセス)
        /// </summary>
        private static int[,] _map2d;

        /// <summary>
        /// blankピースの位置
        /// </summary>
        private static int _blankRow;
        private static int _blankColumn;

        /// <summary>
        /// a, b, c, ..の順で並んだ2Dマップを0, 1, 2, ..の順に並び替える回答を生成する (2x2以上のマップに対応)
        /// </summary>
        /// <param name="map2d"></param>
        /// <returns>回答</returns>
        public static String run(int[,] map2d)
        {
            _map2d = map2d; // map2dをグローバルに置いておく
            StringBuilder line1Sb = new StringBuilder(); // ライン1の回答

            //
            // 2x2の状態になるまでパズルの端を確定させていく
            //

            // 最終的に[0, 0]に移動するピースをblankとして選ぶ
            _blankRow = -1;
            int firstBlankRow = -1, firstBlankColumn = -1;
            for (int i = 0; i < Problem.row; i++)
            {
                for (int j = 0; j < Problem.column; j++)
                {
                    if (_map2d[i, j] == 0)
                    {
                        _blankRow = firstBlankRow = i;
                        _blankColumn = firstBlankColumn = j;
                        break;
                    }
                }
                if (_blankRow != -1)
                {
                    break;
                }
            }

            // 確定させていくパズルの右端を格納する.
            List<int> tmpEdge = new List<int>();

            // 横
            for (int j = Problem.column - 1; j >= 2; j--)
            {
                tmpEdge.Clear();
                for (int i = 0; i < Problem.row; i++)
                {
                    tmpEdge.Add(i * Problem.column + j);
                }

                // 右端を確定させる
                line1Sb.Append(alignTheRightMostEdge(Problem.row, j + 1, tmpEdge));
            }

            // 転置
            int[,] originalMap2d = _map2d;
            int[,] map2dTransposed = new int[2, Problem.row];
            for (int j = 0; j < Problem.row; j++)
            {
                map2dTransposed[0, j] = _map2d[j, 0];
                map2dTransposed[1, j] = _map2d[j, 1];
            }
            _map2d = map2dTransposed;
            Util.swap(ref _blankRow, ref _blankColumn);

            // 縦
            for (int i = Problem.row - 1; i >= 2; i--)
            {
                tmpEdge.Clear();
                for (int j = 0; j < 2; j++)
                {
                    tmpEdge.Add(i * Problem.column + j);
                }

                // 右端(転置前は下端)を確定させる
                line1Sb.Append(Common.translateTransposedOperations(alignTheRightMostEdge(2, i + 1, tmpEdge)));
            }

            // [0,0] ~ [1,1]までの部分を, 転置後のマップから得る
            _map2d = originalMap2d;
            _map2d[0, 0] = map2dTransposed[0, 0];
            _map2d[0, 1] = map2dTransposed[1, 0];
            _map2d[1, 0] = map2dTransposed[0, 1];
            _map2d[1, 1] = map2dTransposed[1, 1];
            Util.swap(ref _blankRow, ref _blankColumn);

            // 2x2 solver {0, 1, Problem.column, Problem.column + 1} (TODO: 回転方向を工夫可能)
            while (_map2d[0, 0] != 0 || _map2d[0, 1] != 1)
            {
                if (_blankRow == 0)
                {
                    if (_blankColumn == 0)
                    {
                        line1Sb.Append(slideRight());
                    }
                    else // _blankColumn == 1
                    {
                        line1Sb.Append(slideDown());
                    }
                }
                else // _blankRow == 1
                {
                    if (_blankColumn == 0)
                    {
                        line1Sb.Append(slideUp());
                    }
                    else // _blankColumn == 1
                    {
                        line1Sb.Append(slideLeft());
                    }
                }
            }

            int selectionCount = 1;
            String line1 = line1Sb.ToString();

            // 二回目の選択を行う場合
            if (_map2d[1, 0] != Problem.column)
            {
                selectionCount++;
            }
            
            // 回答最適化
            line1 = Common.optimizeLine(line1);


            //
            // 回答生成
            //
            String newline = "\r\n";
            StringBuilder retSb = new StringBuilder();

            // 選択回数
            retSb.Append(selectionCount).Append(newline);

            // ライン1 選択画像位置 (16進数: [i行目, j列目]なら ji の順)
            retSb.Append(firstBlankColumn.ToString("X")).Append(firstBlankRow.ToString("X")).Append(newline);

            // ライン1 交換回数
            retSb.Append(line1.Length).Append(newline);

            // ライン1 交換操作
            retSb.Append(line1).Append(newline);

            // ライン2
            if (selectionCount == 2)
            {
                retSb.Append("01").Append(newline);
                retSb.Append(1).Append(newline);
                retSb.Append("R").Append(newline);
            }


            return retSb.ToString();
        }

#region slideメソッド群
        /// <summary>
        /// 上下左右を移動. times:回数
        /// </summary>
        private static String slideUp(int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                Util.swap(ref _map2d[_blankRow, _blankColumn], ref _map2d[_blankRow - 1, _blankColumn]);
                _blankRow--;
            }
            return (new String('U', times));
        }
        private static String slideDown(int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                Util.swap(ref _map2d[_blankRow, _blankColumn], ref _map2d[_blankRow + 1, _blankColumn]);
                _blankRow++;
            }
            return (new String('D', times));
        }
        private static String slideLeft(int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                Util.swap(ref _map2d[_blankRow, _blankColumn], ref _map2d[_blankRow, _blankColumn - 1]);
                _blankColumn--;
            }
            return (new String('L', times));
        }
        private static String slideRight(int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                Util.swap(ref _map2d[_blankRow, _blankColumn], ref _map2d[_blankRow, _blankColumn + 1]);
                _blankColumn++;
            }
            return (new String('R', times));
        }
#endregion

        /// <summary>
        /// _map2dの右端を`edge`にする. 戻り値はそのための命令.
        /// </summary>
        /// <remarks>
        /// 移動の基本は, 移動対象の横にblankを配置しその2つが含まれる2x2の部分パズルを回転させること.
        /// そのため2x2より大きいマップにしか対応していない.
        /// </remarks>
        /// <param name="row">_map2dの使用できる縦幅 (0 ~ N-1 なら N) </param>
        /// <param name="column">_map2dの使用できる横幅</param>
        /// <param name="edge"></param>
        /// <returns></returns>
        private static String alignTheRightMostEdge(int row, int column, List<int> edge)
        {
            StringBuilder sb = new StringBuilder();

            // edgeの上を row-2 個を揃える.
            for (int edgeIndex = 0; edgeIndex < row - 2; edgeIndex++)
            {
                sb.Append(moveTo(row, column, edge[edgeIndex], edgeIndex, column - 1));
            }

            //
            // edgeの下２つを揃える.
            //

            // まず下から２番目の所に, 下から一番目に置かれる予定のパズルを置く.
            sb.Append(moveTo(row, column, edge[row - 1], row - 2, column - 1));

            // 移動方法を変更するなら true
            bool specificMoving = false;

            // blankが[row - 1, column - 1]ではない位置に置かれており,
            // [row - 1 , column - 1]がedgeの下から二番目に置かれるものだったら, 置換の方法を変更する
            // (_blankColumnが column - 1 である場合, [row - 1, column - 1]以外の場所には置かれない)
            if (_blankColumn != column - 1)
            {
                if (_map2d[row - 1, column - 1] == edge[row - 2])
                {
                    specificMoving = true;

                    // blankを [row - 1, column - 1] に移動する
                    sb.Append(slideDown((row - 1) - _blankRow));
                    sb.Append(slideRight((column - 1) - _blankColumn));
                }
            }
            else if (_map2d[row - 1, column -2] == edge[row - 2])
                //blankが [row - 1, column - 1] 置かれており
                //隣がedgeの下から二番目に置かれるものだったら, 置換の方法を変更する
            {
                specificMoving = true;
            }

            // else以降の方法が使えないパターンの場合
            if (specificMoving)
            {
                // [row - 2, column - 1] が端の下から一番目に置かれるもの
                // [row - 1, column - 1] が blank,
                // [row - 1, column - 2] が端の下から二番目に置かれるもの
                // という配置になっている

                // 手動で求めたもの ULLDRURDLLURDRUL
                sb.Append(slideUp());
                sb.Append(slideLeft(2));
                sb.Append(slideDown());
                sb.Append(slideRight());
                sb.Append(slideUp());
                sb.Append(slideRight());
                sb.Append(slideDown());
                sb.Append(slideLeft(2));
                sb.Append(slideUp());
                sb.Append(slideRight());
                sb.Append(slideDown());
                sb.Append(slideRight());
                sb.Append(slideUp());
                sb.Append(slideLeft());
            }
            else 
            {
                // 下から二番目に置かれるものを, [row - 2, column - 2] へ移動する
                sb.Append(moveTo(row, column, edge[row - 2], row - 2, column - 2));

                // blankを [row - 1, column - 2] へ移動する
                if (_blankRow != row - 1 || _blankColumn != column - 2)
                {
                    if (_blankColumn == column - 1) // この条件ならば,  _blankRow == row - 1
                    {
                        sb.Append(slideLeft());
                    }
                    else if (_blankColumn == column - 2)
                    {
                        // 回りこむ
                        sb.Append(slideLeft());
                        sb.Append(slideDown((row - 1) - _blankRow));
                        sb.Append(slideRight());
                    }
                    else // _blankColumn < column - 2
                    {
                        sb.Append(slideDown((row - 1) - _blankRow));
                        sb.Append(slideRight((column - 2) - _blankColumn));
                    }
                }

                // 揃える RUL
                sb.Append(slideRight());
                sb.Append(slideUp());
                sb.Append(slideLeft());
            }

            return sb.ToString();
        }

        /// <summary>
        /// targetとblankが作る2x2パズルを回転させることで, to(Row|Column)までtargetを到達させる)
        /// </summary>
        /// <remarks>
        /// 回転させて,また別の2x2パズルを回転させて.. を繰り返す.
        /// alignTheLeftMostEdgeの実装都合上, 右端上の内容(すでに固定済み)を壊さないように移動を行う.
        /// (アルゴリズムの簡潔化のために, toRow は 0 から row - 2 までに限られ, 左側への移動はできない)
        ///
        /// アルゴリズムの流れ
        /// 1. blankをtarget横へ移動
        /// (右側移動なら最初に blank を target の下におく. 上下方向移動なら左側. : 右上を壊さないために)
        /// 
        /// 2. 目的の位置までの回転を繰り返す (まず上下移動. 次に右移動)
        /// 
        /// (= 右側移動の例 =)
        /// 
        /// | target |        |
        /// | blank  |        |
        /// 
        /// から 
        /// 
        /// | blank  | target |
        /// |        |        |
        /// 
        ///
        /// (= 下側移動の場合 =)
        /// 
        /// | blank  | target |
        /// |        |        |
        /// 
        /// から
        /// 
        /// |        | blank  |
        /// |        | target |
        /// 
        /// 
        /// </remarks>
        /// <param name="row">_map2dの使用できる縦幅 (0 ~ N-1 なら N) </param>
        /// <param name="column">_map2dの使用できる横幅</param>
        /// <param name="target">移動する対象</param>
        /// <param name="toRow">目的の行位置 (0 ~ row-2)</param>
        /// <param name="toColumn">目的の列位置</param>
        /// <returns>移動命令</returns>
        private static String moveTo(int row, int column, int target, int toRow, int toColumn)
        {
            StringBuilder sb = new StringBuilder(); // 命令
            List<Position> way = new List<Position>(); // targetの移動方向

            // 移動するピースの番地を保持する
            int targetRow = -1, targetColumn = -1, originalTargetRow = -1, originalTargetColumn = -1;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    if (_map2d[i, j] == target)
                    {
                        targetRow = originalTargetRow = i;
                        targetColumn = originalTargetColumn = j;
                        break;
                    }
                }
                if (targetRow != -1)
                {
                    break;
                }
            }

            // 既に目的の位置だった場合
            if (targetRow == toRow && targetColumn == toColumn)
            {
                return String.Empty;
            }

            //
            // 上下移動の道筋を作成する (blank は targetの左横)
            //
            if (targetRow != toRow)
            {
                // targetColumn が 0 ならば一つだけ右移動する
                if (targetColumn == 0) {
                    way.Add(Position.RIGHT);
                    targetColumn = 1;
                }

                if (targetRow < toRow) // 下移動
                {
                    for (; targetRow  < toRow; targetRow++)
                    {
                        way.Add(Position.DOWN);
                    }
                }
                else 
                {
                    for (; targetRow > toRow; targetRow--)
                    {
                        way.Add(Position.UP);
                    }
                }
            }

            //
            // 右移動の道筋を作成
            //
            // 制約(簡略化のため): 
            //   toRow は 0 から row - 2 までに限られる => 現在のtargetを移動させずとも, blankを下に置くことが出来る
            //   左側への移動はできない => 条件分岐を減らす
            // 
            for (; targetColumn < toColumn; targetColumn++)
            {
                way.Add(Position.RIGHT);
            }

            // target(Row|Column) の復元
            targetRow = originalTargetRow;
            targetColumn = originalTargetColumn;

            // 実際に移動を行う
            for (int i = 0, len = way.Count(), skipCount = 0; i < len - skipCount; i++)
            {
                Position direction = way[i];
                if (direction == Position.UP || direction == Position.DOWN)
                {
                    // 一列内の移動で完了出来るパターン
                    if (_blankColumn == targetColumn)
                    {
                        if (_blankRow > targetRow && direction == Position.DOWN)
                        {
                            sb.Append(slideUp(_blankRow - targetRow));
                            targetRow++;
                            continue;
                        }
                        else if (_blankRow < targetRow && direction == Position.UP)
                        {
                            sb.Append(slideDown(targetRow - _blankRow));
                            targetRow--;
                            continue;
                        }
                    }

                    //
                    // blank を [targetRow, targetColumn - 1] へ移動させる (横移動 -> 縦移動)
                    //
                    if (!(_blankRow == targetRow && _blankColumn == targetColumn - 1))
                    {
                        // 
                        // 効率化:
                        //   targetとblankが同じ行で, blankが右側にあるのならば
                        //   実際の使用において, まだ右側への移動がある. これを1つ飛ばす.
                        //
                        // この条件のときは, blankをtargetの横に置くために回りこまなければならない.
                        // 効率化のため, 実際の使用パターンに依存した方法で移動を行う.
                        //
                        if (_blankRow == targetRow && _blankColumn > targetColumn)
                        {
                            skipCount++;
                            sb.Append(slideLeft(_blankColumn - targetColumn));
                            targetColumn++;
                        } 
                        else 
                        {
                            // blankの横移動 (右上を壊さないように横移動が先)
                            if (_blankColumn >= targetColumn)
                            {
                                sb.Append(slideLeft((_blankColumn - targetColumn) + 1));
                            }
                            else 
                            {
                                sb.Append(slideRight((targetColumn - _blankColumn) - 1));
                            }

                            // 縦移動
                            if (_blankRow >= targetRow)
                            {
                                sb.Append(slideUp(_blankRow - targetRow));
                            }
                            else
                            {
                                sb.Append(slideDown(targetRow - _blankRow));
                            }
                        }
                    }

                    if (direction == Position.UP)
                    {
                        // URD
                        sb.Append(slideUp());
                        sb.Append(slideRight());
                        sb.Append(slideDown());

                        targetRow--;
                    }
                    else // direction == Position.Down 
                    {
                        // DRU
                        sb.Append(slideDown());
                        sb.Append(slideRight());
                        sb.Append(slideUp());

                        targetRow++;
                    }
                }

                else // direction == Position.RIGHT
                {
                    // 一行内の移動で完了出来るパターン
                    if (_blankRow == targetRow)
                    {
                        if (_blankColumn > targetColumn) // && direction == RIGHT
                        {
                            sb.Append(slideLeft(_blankColumn - targetColumn));
                            targetColumn++;
                            continue;
                        }
                    }

                    // 一番左下にtargetがあるパターン
                    if (targetColumn == 0 && targetRow == row - 1)
                    {
                        //
                        // blank を [row - 1, 1] へ移動させる
                        //

                        // 横移動
                        if (_blankColumn == 0) // 右移動
                        {
                            sb.Append(slideRight());
                        }
                        else // 左移動
                        {
                            sb.Append(slideLeft(_blankColumn - 1));
                        }

                        // 縦移動 (下移動)
                        sb.Append(slideDown(targetRow - _blankRow));

                        // blankを左移動 (targbetを右移動)
                        sb.Append(slideLeft());
                        targetColumn++;
                        continue;
                    }

                    //
                    // blank を [targetRow + 1, targetColumn] へ移動させる (縦移動 -> 横移動)
                    //
                    if (!(_blankRow == targetRow + 1 && _blankColumn == targetColumn))
                    {
                        // 右端上を壊さないように
                        if (_blankColumn != targetColumn && _blankColumn == column - 1)
                        {
                            sb.Append(slideLeft());
                        }

                        // この条件のときは回りこむ
                        if (_blankColumn == targetColumn && _blankRow < targetRow)
                        {
                            // 右を壊さないように, 左に回りこむ.
                            if (_blankColumn >= column - 2)
                            {
                                sb.Append(slideLeft());
                            }
                            // 右に回りこむ.
                            else
                            {
                                sb.Append(slideRight());
                            }
                        }

                        // blankの縦移動
                        if (_blankRow > targetRow)
                        {
                            sb.Append(slideUp((_blankRow - targetRow) - 1));
                        }
                        else
                        {
                            sb.Append(slideDown((targetRow - _blankRow) + 1));
                        }

                        // 横移動
                        if (_blankColumn > targetColumn)
                        {
                            sb.Append(slideLeft(_blankColumn - targetColumn));
                        }
                        else
                        {
                            sb.Append(slideRight(targetColumn - _blankColumn));
                        }
                    }

                    // RUL
                    sb.Append(slideRight());
                    sb.Append(slideUp());
                    sb.Append(slideLeft());
                    targetColumn++;
                }
            }

            return sb.ToString();
        }
    }
}
