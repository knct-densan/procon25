using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace chino.Solvers
{
    /// <summary>
    /// Solverアルゴリズム1.
    /// 様々なサイズのIDA*によって端から複数個同時に揃えていく.
    /// </summary>
    static class Solver1
    {
        private static int[,] _map2d;
        // ピース番号と, _map2dでの番地の対応を保持する
        // 第一インデクサはピース番号. 第二インデクサが0なら行番号, 1なら列番号を意味する
        private static int[,] _map2d_inverse;
        private static int _blankRow = -1;
        private static int _blankColumn = -1;
        private static int _selectableTime; // 残りの選択可能回数

        /// <summary>
        /// a, b, c, ..の順で並んだ2Dマップを0, 1, 2, ..の順に並び替える回答を生成する (2x2以上のマップに対応)
        /// </summary>
        /// <param name="map2d"></param>
        /// <returns>回答</returns>
        public static String run(int[,] map2d)
        {
            _map2d = map2d;
            _map2d_inverse = new int[Problem.partNum, 2];
            _dfs_map2d_inverse = new int[Problem.partNum, 2];
            constructInverse(Problem.row, Problem.column);
            _blankRow = -1;
            _blankColumn = -1;
            _selectableTime = Problem.selectionLimit;
            StringBuilder answerBuilder = new StringBuilder();  // 各行 "X番地Y番地"+OP(UDLR) の構成になっている.

            // 完成後のマップ
            int[,] distMap = new int[Problem.row, Problem.column];
            for (int i = 0; i < Problem.partNum; i++)
            {
                distMap[i / Problem.column, i % Problem.column] = i;
            }

            // 幅を揃えていく
            int row = Problem.row;
            int column = Problem.column;
            bool transposed = false;
            List<int> tmpEdge = new List<int>();
            while (row > 2 || column > 2)
            {
                tmpEdge.Clear();
                // 右を揃える
                if (column >= row)
                {
                    int j = column - 1;
                    for (int i = 0; i < row; i++)
                    {
                        tmpEdge.Add(distMap[i, j]);
                    }

                    // 転置状態なら元に戻す
                    if (transposed)
                    {
                        _map2d = transpose(_map2d, column, row);
                        Util.swap(ref _blankRow, ref _blankColumn);
                        constructInverse(row, column);
                        transposed = false;
                    }

                     // 右端を確定させる
                    answerBuilder.Append(alignTheRightMostEdge(row, column, tmpEdge));

                    // blankが確定した右端の要素であった場合
                    if (_blankColumn == j)
                    {
                        _blankRow = -1;
                        _blankColumn = -1;
                    }

                    column--;
                }
                // 下を揃える
                else
                {
                    int i = row - 1;
                    for (int j = 0; j < column; j++)
                    {
                        tmpEdge.Add(distMap[i, j]);
                    }

                    // 通常状態なら転置する
                    if (!transposed)
                    {
                        _map2d = transpose(_map2d, row, column);
                        Util.swap(ref _blankRow, ref _blankColumn);
                        constructInverse(column, row);
                        transposed = true;
                    }

                    // 右端(転置前は下端)を確定させる
                    answerBuilder.Append(normalizeTransposed(alignTheRightMostEdge(column, row, tmpEdge)));

                    // blankが確定した右端の要素であった場合
                    if (_blankColumn == i)
                    {
                        _blankRow = -1;
                        _blankColumn = -1;
                    }

                    row--;
                }
            }

            if (transposed)
            {
                Util.swap(ref _map2d[0, 1], ref _map2d[1, 0]);
                Util.swap(ref _blankRow, ref _blankColumn);
                constructInverse(2, 2);
            }

            // 2x2 solver {0, 1, Problem.column, Problem.column + 1}
            // この時点で選択回数は1回以上残っている
            if (_blankColumn == -1) // どこも選択されていない場合 (残り選択回数は2回以上残っている)
            {
                answerBuilder.Append(setBlank(0, 0));
            }
            if (_map2d[_blankRow, _blankColumn] <= 1)
            {
                answerBuilder.Append(buildSideOnRegion(0, 0, 2, 2, Position.UP, new List<int> { 0, 1 }, true));
                if (_map2d[1, 0] != Problem.column)
                {
                    answerBuilder.Append(setBlank(1, 0));
                    answerBuilder.Append(slideRight());
                }
            }
            else
            {
                answerBuilder.Append(buildSideOnRegion(
                    0, 0, 2, 2, Position.DOWN, new List<int> { Problem.column, Problem.column + 1 }, true));
                if (_map2d[0, 0] != 0)
                {
                    answerBuilder.Append(setBlank(0, 0));
                    answerBuilder.Append(slideRight());
                }
            }

            //
            // 回答生成
            //
            String newline = "\r\n";
            StringBuilder retSb = new StringBuilder();

            // 選択回数
            retSb.Append(Problem.selectionLimit - _selectableTime).Append(newline);

            String[] lines = splitUnderBuilding(answerBuilder.ToString());
            for (int i = 0; i < lines.Length; i++)
            {
                String op = Common.optimizeLine(lines[i].Substring(2));

                // 選択画像位置
                retSb.Append(lines[i].Substring(0, 2)).Append(newline);

                // 交換回数
                retSb.Append(op.Length).Append(newline);

                // 交換操作
                retSb.Append(op).Append(newline);
            }
            return retSb.ToString();
        }

        private static void constructInverse(int row, int column)
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    int p = _map2d[i, j];
                    _map2d_inverse[p, 0] = i;
                    _map2d_inverse[p, 1] = j;
                }
            }
        }

        /// <param name="map2d"></param>
        /// <param name="row">現在のmap2dの縦幅</param>
        /// <param name="column">現在のmap2dの横幅</param>
        /// <returns></returns>
        private static int[,] transpose(int[,] map2d, int row, int column)
        {
            int[,] transposed = new int[column, row];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    transposed[j, i] = map2d[i, j];
                }
            }
            return transposed;
        }

        private static char splitterOfUnderBuilding = '\n';

        private static String setBlank(int row, int column)
        {
            _blankRow = row;
            _blankColumn = column;
            _selectableTime -= 1;
            return splitterOfUnderBuilding + column.ToString("X") + row.ToString("X");
        }

        private static String[] splitUnderBuilding(String building)
        {
            return building.Split(new char[]{splitterOfUnderBuilding}, StringSplitOptions.RemoveEmptyEntries);
        }

        private static String normalizeTransposed(String transed)
        {
            String[] lines = splitUnderBuilding(transed);

            if (lines.Length > 0) // && transed.Length > 0
            {
                StringBuilder ret = new StringBuilder();
                int i = 1;
                if (transed[0] == splitterOfUnderBuilding)
                {
                    i = 0;
                }
                else
                {
                    ret.Append(Common.translateTransposedOperations(lines[0]));
                }
                for (; i < lines.Length; i++)
                {
                    ret.Append(splitterOfUnderBuilding);

                    // row <-> column
                    ret.Append(lines[i][1]); 
                    ret.Append(lines[i][0]);

                    ret.Append(Common.translateTransposedOperations(lines[i].Substring(2)));
                }
                return ret.ToString();
            }
            return "";
        }

        /// <summary>
        /// 上下左右を移動. times:回数
        /// </summary>
        private static String slide(int stepRow, int stepColumn, char oneOp, int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                int nextRow = _blankRow + stepRow;
                int nextColumn = _blankColumn + stepColumn;
                int blank = _map2d[_blankRow, _blankColumn];
                int nextBlank = _map2d[nextRow, nextColumn];
                Util.swap(ref _map2d_inverse[blank, 0], ref _map2d_inverse[nextBlank, 0]);
                Util.swap(ref _map2d_inverse[blank, 1], ref _map2d_inverse[nextBlank, 1]);
                Util.swap(ref _map2d[_blankRow, _blankColumn], ref _map2d[nextRow, nextColumn]);
                _blankRow = nextRow;
                _blankColumn = nextColumn;
            }
            return (new string(oneOp, times));
        }

        private static String slideUp(int times = 1)
        {
            return slide(-1, 0, 'U', times);
        }
        private static String slideDown(int times = 1)
        {
            return slide(1, 0, 'D', times);
        }
        private static String slideLeft(int times = 1)
        {
            return slide(0, -1, 'L', times);
        }
        private static String slideRight(int times = 1)
        {
            return slide(0, 1, 'R', times);
        }
            
        private static String findAndSetNewBlank(
            int centerRow, int centerColumn, List<int> avoided, int row, int column)
        {
            return findAndSetNewBlank(centerRow, centerColumn, avoided, 0, 0, row - 1, column - 1);
        }

        private static String findAndSetNewBlank(
            int centerRow, int centerColumn, List<int> avoided,
            int upperLeftRow, int upperLeftColumn, int lowerRightRow, int lowerRightColumn)
        {
            if (_selectableTime == 2)
            {
                return setBlank(_map2d_inverse[0, 0], _map2d_inverse[0, 1]);
            }

            // centerの近くで, blankを置ける場所を探す
            int i = -1, j = -1;
            int span = 0;
            while (true)
            {
                int top = centerRow - span;
                int bottom = centerRow + span;
                int left = centerColumn - span;
                int right = centerColumn + span;

                for (j = left; j <= right; j++)
                {
                    if (j < upperLeftColumn || j > lowerRightColumn)
                    {
                        continue;
                    }

                    i = top;
                    if (i >= upperLeftRow 
                        && i <= lowerRightRow
                        && !avoided.Contains(_map2d[i, j]))
                    {
                        goto found;
                    }

                    i = bottom;
                    if (i >= upperLeftRow 
                        && i <= lowerRightRow
                        && !avoided.Contains(_map2d[i, j]))
                    {
                        goto found;
                    }
                }

                for (i = top + 1; i < bottom; i++)
                {
                    if (i < upperLeftRow || i > lowerRightRow)
                    {
                        continue;
                    }

                    j = left;
                    if (j >= upperLeftColumn
                        && j <= lowerRightColumn
                        && !avoided.Contains(_map2d[i, j]))
                    {
                        goto found;
                    }

                    j = right;
                    if (j >= upperLeftColumn
                        && j <= lowerRightColumn
                        && !avoided.Contains(_map2d[i, j]))
                    {
                        goto found;
                    }
                }

                span++;
            }

        found:
            return (setBlank(i, j));
        }

        /// <summary>
        /// 指定した部分四角形の要素の中で, lstに含まれるものを返す
        /// (notがtrueならlstに含まれないものを返す)
        /// </summary>
        private static List<int> findAllOnRegion(
            int upperLeftRow, int upperLeftColumn, int partRow, int partColumn, List<int> lst, bool not = false)
        {
            List<int> ret = new List<int>();
            for (int i = 0; i < partRow; i++)
            {
                int addrI = upperLeftRow + i;
                for (int j = 0; j < partColumn; j++)
                {
                    int addrJ = upperLeftColumn + j;
                    if (!not)
                    {
                        if (lst.Contains(_map2d[addrI, addrJ]))
                        {
                            ret.Add(_map2d[addrI, addrJ]);
                        }
                    }
                    else
                    {
                        if (!lst.Contains(_map2d[addrI, addrJ]))
                        {
                            ret.Add(_map2d[addrI, addrJ]);
                        }
                    }
                }
            }
            return ret;
        }

        private static bool isTheEdge(int row, int column, List<int> edge)
        {
            int j = column - 1;
            for (int i = 0; i < row; i++)
            {
                if (_map2d[i, j] != edge[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// _map2dの右端を`edge`にする. 戻り値はそのための命令.
        /// </summary>
        /// <param name="row">_map2dの使用できる縦幅 (0 ~ N-1 なら N) </param>
        /// <param name="column">_map2dの使用できる横幅</param>
        /// <param name="edge"></param>
        /// <returns></returns>
        private static String alignTheRightMostEdge(int row, int column, List<int> edge)
        {
            // ==== TODO ====
            // 1. 移動するブロックを1列隙間なくまとめる
            // 2. 移動方向(横と縦, 両方の移動方法を工夫する)
            // 横(右端近くへ移動させる): 下から上になぞった後は, 上から下など
            // 縦(右端へ寄せられたピースを目的のブロックへ移動させる): 下を揃えた後は上を揃えるなど
            // 3. 選択方法の工夫
            // 4. 全体見直し (色々最適化方法がある)
            StringBuilder ret = new StringBuilder();

            // 既に揃っていたら終了
            if (isTheEdge(row, column, edge))
            {
                return "";
            }

            // edgeの要素を全て右端近くへ移動させる: 全て幅から2個目までに収まるまで
            while (edge.FindIndex(x => column - _map2d_inverse[x, 1] > 2) != -1)
            {
                // 右へ移動させる目標物
                int target = edge[0];
                for (int i = 1; i < edge.Count; i++)
                {
                    int piece = edge[i];
                    if (// 左端に近い方から
                        _map2d_inverse[piece, 1] < _map2d_inverse[target, 1]
                        || // 同一列なら, blankに近い方から
                        (_map2d_inverse[piece, 1] == _map2d_inverse[target, 1]
                        && Math.Abs(_blankRow - _map2d_inverse[piece, 0])
                        < Math.Abs(_blankRow - _map2d_inverse[target, 0])))
                    {
                        target = piece;
                    }
                }
                int targetRow = _map2d_inverse[target, 0];
                int targetColumn = _map2d_inverse[target, 1];

                // blankが未設定, 端の要素に含まれている, または現時点から離れた場所にtargetがある場合
                if (_blankRow == -1 
                    || edge.Contains(_map2d[_blankRow, _blankColumn])
                    || (_selectableTime > 2 
                    && Math.Abs(_blankRow - targetRow) + Math.Abs(_blankColumn - targetColumn) > (Problem.row + Problem.column) / 2))
                {
                    ret.Append(findAndSetNewBlank(targetRow, targetColumn, edge, row, column));
                }

                // blankを移動する
                {
                    bool modified = false;

                    // blankがtargetの2個以上左にある場合
                    if (targetColumn - _blankColumn >= 2)
                    {
                        ret.Append(slideRight(targetColumn - _blankColumn - 1));
                        modified = true;
                    }
                    // blankがtargetの3つ以上右にある場合
                    else if (_blankColumn - targetColumn >= 3)
                    {
                        ret.Append(slideLeft(_blankColumn - targetColumn));
                        modified = true;
                    }

                    // blankがtargetの3つ以上, 上もしくは下にある場合
                    //
                    // 上
                    if (targetRow - _blankRow >= 3)
                    {
                        ret.Append(slideDown(targetRow - _blankRow - 2));
                        modified = true;
                    }
                    // 下
                    else if (_blankRow - targetRow >= 3)
                    {
                        ret.Append(slideUp(_blankRow - targetRow - 2));
                        modified = true;
                    }

                    if (modified)
                    {
                        continue;
                    }
                }

                // targetを移動させる
                {
                    // ==== TODO =====
                    // 移動方法を効率的なものにする
                    // (例えば3つ以下のedge要素を含む領域の中から
                    // 最も右寄りのものを選ぶ.
                    // 今は2x3, 3x3サイズのIDA*に限られるが, 
                    // どの領域にも1,2個しかedge要素がない場合は
                    // 2x3~6のものを使うようにする.
                    // (上下に要素がなければ3x4を使用など)

                    int upperLeftRow = Math.Min(_blankRow, targetRow); 
                    int upperLeftColumn = Math.Min(_blankColumn, targetColumn);
                    int partRow = Math.Abs(_blankRow - targetRow) + 1;
                    int partColumn = 3; // targetは右端から2以上離れているため, 3以上取れる

                    while (upperLeftRow + partRow < row && partRow < 3)
                    {
                        partRow++;
                    }
                    while (upperLeftRow > 0 && partRow < 3)
                    {
                        upperLeftRow--;
                        partRow++;
                    }

                    List<int> side = findAllOnRegion(upperLeftRow, upperLeftColumn, partRow, partColumn, edge);
                    if (side.Count > partRow && upperLeftColumn + partColumn == column)
                    {
                        partColumn--;
                        side = findAllOnRegion(upperLeftRow, upperLeftColumn, partRow, partColumn, edge);
                    }
                    if (side.Count <= partRow)
                    {
                        ret.Append(buildSideOnRegion(upperLeftRow, upperLeftColumn,
                            partRow, partColumn, Position.RIGHT, side, false));
                        continue;
                    }
                }

                // 移動できなかった場合 (右の区間に多くのedge要素がある場合)
                {
                    // 右側に既にedge要素がある行
                    bool[] occupied = new bool[row];

                    for (int j = Math.Min(column - 1, targetColumn + 2); j > targetColumn; j--)
                    {
                        for (int i = 0; i < row; i++)
                        {
                            if (edge.Contains(_map2d[i, j]))
                            {
                                occupied[i] = true;
                            }
                        }
                    }

                    int toRow = -1;
                    for (int step = 0; ; step++)
                    {
                        int i = targetRow - step;
                        if (i >= 0 && !occupied[i])
                        {
                            toRow = i;
                            break;
                        }

                        i = targetRow + step;
                        if (i < row && !occupied[i])
                        {
                            toRow = i;
                            break;
                        }
                    }

                    // blankがtargetの2つ以上右側にある場合, (blankはtargetの横最大1つ違うところに置く)
                    if (_blankColumn - targetColumn >= 2)
                    {
                        ret.Append(slideLeft(_blankColumn - targetColumn - 1));
                    }

                    // blankがtargetの2つ以上, 上か下側にある場合
                    if (targetRow - _blankRow >= 2)
                    {
                        ret.Append(slideDown(targetRow - _blankRow - 1));
                    }
                    else if (_blankRow - targetRow >= 2)
                    {
                        ret.Append(slideUp(_blankRow - targetRow - 1));
                    }

                    // targetの横が通りぬけ可能であったら, 2x3で一個だけ右移動する
                    if (targetRow == toRow)
                    {
                        int upperLeftRow = Math.Min(_blankRow, targetRow);
                        int upperLeftColumn = Math.Min(_blankColumn, targetColumn);
                        int partRow = 2;
                        int partColumn = 3;

                        if (upperLeftRow == row - 1)
                        {
                            upperLeftRow--;
                        }

                        ret.Append(buildSideOnRegion(upperLeftRow, upperLeftColumn,
                            partRow, partColumn, Position.RIGHT, new List<int>{target}, false));
                    }
                    // 上へ移動
                    else if (targetRow > toRow)
                    {
                        int upperLeftRow = toRow;
                        int upperLeftColumn = Math.Min(_blankColumn, targetColumn);
                        int partRow = Math.Max(targetRow, _blankRow) - toRow + 1;
                        int partColumn = 2;

                        if (partRow > 6)
                        {
                            upperLeftRow += partRow - 6;
                            partRow = 6;
                        }

                        ret.Append(buildSideOnRegion(upperLeftRow, upperLeftColumn,
                            partRow, partColumn, Position.UP, new List<int>{target}, false));
                    }
                    // 下へ移動
                    else // targetRow < toRow
                    {
                        int upperLeftRow = Math.Min(targetRow, _blankRow);
                        int upperLeftColumn = Math.Min(_blankColumn, targetColumn);
                        int partRow = Math.Min(toRow - upperLeftRow + 1, 6);
                        int partColumn = 2;

                        ret.Append(buildSideOnRegion(upperLeftRow, upperLeftColumn,
                            partRow, partColumn, Position.DOWN, new List<int>{target}, false));
                    }
                }
            }

            // 既に揃っていたら終了
            if (isTheEdge(row, column, edge))
            {
                return ret.ToString();
            }

            // 縦移動 [右端のrow x 3の区間]
            // (目的位置へ揃えるためには2x3以上のIDA*ではないと揃えられない場合がある)
            int buildingTop = 0;
            while (buildingTop < row)
            {
                int buildingRow = 3;
                int restRow = row - buildingTop;
                if (restRow == 4 || restRow == 2)
                {
                    buildingRow = 2;
                }
                List<int> buildingEdge = edge.GetRange(buildingTop, buildingRow);
                int nextBuildingTop = buildingTop + buildingRow;

                // 目的の位置と同じ列ブロックへ移動させる
                while(buildingEdge.FindIndex(x => _map2d_inverse[x, 0] >= nextBuildingTop) != -1
                    || _blankRow == -1 
                    || edge.Contains(_map2d[_blankRow, _blankColumn])
                    || _blankColumn < column - 3
                    || _blankRow >= nextBuildingTop)
                {
                    int mostDistantRow = -1;
                    int mostDistantColumn = -1;
                    foreach (int elem in buildingEdge)
                    {
                        if (mostDistantRow < _map2d_inverse[elem, 0])
                        {
                            mostDistantRow = _map2d_inverse[elem, 0];
                            mostDistantColumn = _map2d_inverse[elem, 1];
                        }
                    }
                    if (_blankRow == -1 || edge.Contains(_map2d[_blankRow, _blankColumn]))
                    {
                        ret.Append(findAndSetNewBlank(mostDistantRow, mostDistantColumn, edge,
                            0, column - 3, row - 1, column - 1));
                        continue;
                    }                    
                    if (_blankColumn < column - 3)
                    {
                        // column -　3には, edgeの要素はない (横移動の終了条件)
                        ret.Append(slideRight(column - 3 - _blankColumn));
                        continue;
                    }
                    if (mostDistantRow - _blankRow > 2)
                    {
                        ret.Append(slideDown(mostDistantRow - _blankRow - 2));
                        continue;
                    }
                    if (_blankRow - mostDistantRow > 1)
                    {
                        ret.Append(slideUp(_blankRow - mostDistantRow - 1));
                        continue;
                    }

                    int upperLeftRow = Math.Min(_blankRow, mostDistantRow); 
                    int upperLeftColumn = column - 3;
                    int partRow = Math.Abs(_blankRow - mostDistantRow) + 1;
                    int partColumn = 3;
                    for (int i = 0; i < 3; i++)
                    {
                        if (partRow < 4 && upperLeftRow > buildingTop)
                        {
                            upperLeftRow--;
                            partRow++;
                        }
                    }

                    List<int> side = 
                        findAllOnRegion(upperLeftRow, upperLeftColumn, partRow, partColumn, buildingEdge);
                    ret.Append(buildSideOnRegion(upperLeftRow, upperLeftColumn, partRow, partColumn, Position.UP, side, false));
                }

                // 端で揃える
                ret.Append(buildSideOnRegion(buildingTop, column - 3, buildingRow, 3, Position.RIGHT, buildingEdge, true));

                //
                // 揃えた端の左側に, edge要素ではないものを詰める (今は column - 3 <= _blankColumn < column - 1)
                //
                // == TODO ==
                // * 移動最適化
                while (true)
                {
                    // blankが横に挟まっていたらblankを移動する
                    if (nextBuildingTop != row && nextBuildingTop > _blankRow)
                    {
                        ret.Append(slideDown(nextBuildingTop - _blankRow));
                        continue;
                    }

                    // 揃えた端左側で一番上にある, edge要素を探索
                    int packingRow = -1; 
                    for (int i = buildingTop; i < nextBuildingTop && packingRow == -1; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            if (edge.Contains(_map2d[i, column - 2 - j]))
                            {
                                packingRow = i;
                                break;
                            }
                        }
                    }

                    // edge要素が横に挟まっていない場合で
                    if (packingRow == -1)
                    {
                        // それ以外では完了
                        break;
                    }

                    // edgeに含まれず, 端から2,3番目に位置し, 現在２番目の高さに位置する要素を探す
                    // (正しいマップなら探索し終わる)
                    int secondRow = -1;
                    int count = 0;
                    for (int i = packingRow; i < row && secondRow == -1; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            int addrRow = i;
                            int addrColumn = column - 2 - j;

                            if (!(addrRow == _blankRow && addrColumn == _blankColumn)
                                && !edge.Contains(_map2d[addrRow, addrColumn]))
                            {
                                count++;
                                if (count == 2)
                                {
                                    secondRow = i;
                                    break;
                                }
                            }
                        }
                    }

                    if (_blankRow - secondRow > 3)
                    {
                        ret.Append(slideUp(_blankRow - secondRow - 3));
                    }
                    else if (secondRow - _blankRow > 3)
                    {
                        ret.Append(slideDown(secondRow - _blankRow - 3));
                    }

                    int upperLeftRow = -1;
                    int upperLeftColumn = column - 3;
                    int partRow = -1;
                    int partColumn = 2;

                    // packingRowまで一つのIDA*で到達できる場合
                    if ((partRow = Math.Max(secondRow, _blankRow) - packingRow + 1) <= 6)
                    {
                        upperLeftRow = packingRow;
                    }
                    // それ以外は6x2を適用する
                    else
                    {
                        upperLeftRow = Math.Max(secondRow, _blankRow) - 5;
                        partRow = 6;
                    }

                    List<int> set = findAllOnRegion(upperLeftRow, upperLeftColumn, partRow, partColumn, edge, true);
                    set.Remove(_map2d[_blankRow, _blankColumn]);
                    if (set.Count > 2)
                    {
                        set = set.Take(2).ToList();
                    }

                    ret.Append(buildSideOnRegion(
                        upperLeftRow, upperLeftColumn, partRow, partColumn, Position.UP, set, false));
                }

                // 次の列ブロックを揃える
                buildingTop = nextBuildingTop;
            }

            return ret.ToString();
        }

        /// <summary>
        /// [upperLeftRow, upperLeftColumn]番地から始まる partRow x partColumn サイズの
        /// _map2d内四角形領域に対してbuildSideを適用する
        /// </summary>
        private static String buildSideOnRegion(
            int upperLeftRow, int upperLeftColumn, int partRow, int partColumn
            , Position pos, List<int> side, bool ordered = false)
        {
            // buildSideに操作される部分四角形を組み立てる
            int[,] partMap2d = new int[partRow, partColumn];
            for (int i = 0; i < partRow; i++)
            {
                int originalRow = upperLeftRow + i;
                for (int j = 0; j < partColumn; j++)
                {
                    partMap2d[i, j] = _map2d[originalRow, upperLeftColumn + j];
                }
            }

            // 実行 (返り値は <命令,partMap2d位置でのblankRow, blankColumn>)
            Tuple<String, int, int> t = buildSide(partMap2d, partRow, partColumn, _blankRow - upperLeftRow
                , _blankColumn - upperLeftColumn, pos, side, ordered);

            // 結果を反映する
            for (int i = 0; i < t.Item1.Length; i++)
            {
                // _map2d, _map2d_inverse, _blankRow, _blankColumnを更新する
                switch (t.Item1[i])
                {
                    case 'U':
                        slideUp();
                        break;
                    case 'D':
                        slideDown();
                        break;
                    case 'L':
                        slideLeft();
                        break;
                    case 'R':
                        slideRight();
                        break;
                }
            }
            return t.Item1;
        }



        /////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////      IDA*     ///////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////

        ///
        /// 本来DFSの引数として用いる値を, 簡潔な記述と高速化のためグローバルに置いておく
        ///
        private static int[,] _dfs_map2d;
        private static int[,] _dfs_map2d_inverse;
        private static int _dfs_row;
        private static int _dfs_column;
        private static int _dfs_blankRow;
        private static int _dfs_blankColumn;
        private static List<int> _dfs_side;
        private static Position _dfs_pos;
        private static bool _dfs_ordered;
        private static int _dfs_limit;
        private static char[] _dfs_op;
        private static int _dfs_depth;

        /// <summary>
        /// IDA*アルゴリズムを用いて, posで指定したmap2dの最外辺を`side`に揃える.
        /// (orderedがtrueならば, sideは順番通りに並べられる)
        /// </summary>
        /// <returns></returns>
        private static Tuple<String, int, int> buildSide(int[,] map2d, int row, int column,
            int blankRow, int blankColumn, Position pos, List<int> side, bool ordered = false)
        {
            _dfs_map2d = map2d;
            _dfs_row = row;
            _dfs_column = column;
            _dfs_blankRow = blankRow;
            _dfs_blankColumn = blankColumn;
            _dfs_side = side;
            _dfs_pos = pos;
            _dfs_ordered = ordered;
            _dfs_op = new char[1000];
            _dfs_op[0] = '_';
            _dfs_depth = 0;

            // _dfs_inverseの構築
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    int p = map2d[i, j];
                    _dfs_map2d_inverse[p, 0] = i;
                    _dfs_map2d_inverse[p, 1] = j;
                }
            }

            // dfs (解に到達するまで深さを大きくしていく)
            for (_dfs_limit = 1; dfs() == false; _dfs_limit++);

            return new Tuple<string,int,int>(
                new String(_dfs_op.Skip(1).Take(_dfs_depth).ToArray())
                , _dfs_blankRow
                , _dfs_blankColumn);
        }

        /// <summary>
        /// buildSide内部から呼び出される深さ制限付きDFS.
        /// </summary>
        /// <returns>探索に成功した場合はtrueを返す</returns>
        private static bool dfs()
        {
            int h = heuristic();
            // 完成しているなら
            if (h == 0)
            {
                // 終了
                return true;
            }
            // 深さ制限を超えていた場合
            if (!(_dfs_depth + h < _dfs_limit))
            {
                // 失敗
                return false;
            }

            // 前の操作
            char last = _dfs_op[_dfs_depth];

            if (// UP
                (last != 'D' && _dfs_blankRow > 0 && next(- 1, 0, 'U'))
                // DOWN
                || (last != 'U' && _dfs_blankRow < _dfs_row - 1 && next( 1, 0, 'D'))
                // LEFT
                || (last != 'R' && _dfs_blankColumn > 0 && next(0, - 1, 'L'))
                // RIGHT
                || (last != 'L' && _dfs_blankColumn < _dfs_column - 1 && next(0, 1, 'R')))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// もう一つ深く潜る
        /// </summary>
        /// <param name="nextBlankRow"></param>
        /// <param name="nextBlankColumn"></param>
        /// <param name="oneOp"></param>
        /// <returns></returns>
        private static bool next(int stepRow, int stepColumn, char oneOp)
        {
            int originalBlankRow = _dfs_blankRow;
            int originalBlankColumn = _dfs_blankColumn;
            int nextBlankRow = originalBlankRow + stepRow;
            int nextBlankColumn = originalBlankColumn + stepColumn;
            int blank = _dfs_map2d[originalBlankRow, originalBlankColumn];
            int nextBlank = _dfs_map2d[nextBlankRow, nextBlankColumn];

            // 移動
            Util.swap(ref _dfs_map2d_inverse[blank, 0], ref _dfs_map2d_inverse[nextBlank, 0]);
            Util.swap(ref _dfs_map2d_inverse[blank, 1], ref _dfs_map2d_inverse[nextBlank, 1]);
            _dfs_map2d[originalBlankRow, originalBlankColumn] = nextBlank;
            _dfs_map2d[nextBlankRow, nextBlankColumn] = blank;
            _dfs_blankRow = nextBlankRow;
            _dfs_blankColumn = nextBlankColumn;
            _dfs_depth += 1;
            _dfs_op[_dfs_depth] = oneOp;

            if(dfs())
            {
                return true;
            }

            // 復元
            _dfs_depth -= 1;
            _dfs_blankRow = originalBlankRow;
            _dfs_blankColumn = originalBlankColumn;
            _dfs_map2d[originalBlankRow, originalBlankColumn] = blank;
            _dfs_map2d[nextBlankRow, nextBlankColumn] = nextBlank;
            Util.swap(ref _dfs_map2d_inverse[blank, 0], ref _dfs_map2d_inverse[nextBlank, 0]);
            Util.swap(ref _dfs_map2d_inverse[blank, 1], ref _dfs_map2d_inverse[nextBlank, 1]);

            return false;
        }

        /// <summary>
        /// ヒューリスティック関数. _dfs_posで指定されている最外辺までへの
        /// _dfs_sideの各要素のマンハッタン距離を図り, その合計を返す.
        /// orderedがtrueなら_dfs_sideの順番通りに辺を揃えなければならないため,
        /// 縦座標と横座標の両方でマンハッタン距離が図られる.
        /// </summary>
        /// <returns></returns>
        private static int heuristic()
        {
            int sum = 0;
            switch(_dfs_pos)
            {
                case Position.UP:
                    for (int j = 0; j < _dfs_side.Count; j++)
                    {
                        int piece = _dfs_side[j];
                        sum += _dfs_map2d_inverse[piece, 0];
                        if (_dfs_ordered)
                        {
                            sum += Math.Abs(j - _dfs_map2d_inverse[piece, 1]);
                        }
                    }
                    return sum;

                case Position.DOWN:
                    for (int j = 0; j < _dfs_side.Count; j++)
                    {
                        int piece = _dfs_side[j];
                        sum += (_dfs_row - 1) - _dfs_map2d_inverse[piece, 0];
                        if (_dfs_ordered)
                        {
                            sum += Math.Abs(j - _dfs_map2d_inverse[piece, 1]);
                        }
                    }
                    return sum;

                case Position.LEFT:
                    for (int i = 0; i < _dfs_side.Count; i++)
                    {
                        int piece = _dfs_side[i];
                        sum += _dfs_map2d_inverse[piece, 1];
                        if (_dfs_ordered)
                        {
                            sum += Math.Abs(i - _dfs_map2d_inverse[piece, 0]);
                        }
                    }
                    return sum;

                case Position.RIGHT:
                    for (int i = 0; i < _dfs_side.Count; i++)
                    {
                        int piece = _dfs_side[i];
                        sum += (_dfs_column - 1) - _dfs_map2d_inverse[piece, 1];
                        if (_dfs_ordered)
                        {
                            sum += Math.Abs(i - _dfs_map2d_inverse[piece, 0]);
                        }
                    }
                    return sum;
            }
            return -1;
        }
    }
}
