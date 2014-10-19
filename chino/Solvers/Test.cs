using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace chino.Solvers
{
    static class Test
    {
        /// <summary>
        /// Solverのテスト用メソッド.
        /// 適当なanswerMapを作り, solveを実行して返って来た回答文字列で,
        /// マップが復元されるかを確かめる. (100回テストする)
        /// </summary>
        /// <returns>
        /// テストが成功したか表す真偽値(失敗ならfalse), 
        /// 失敗した場合の縦の分割数(row), 横の分割数(column), 選択回数制限, 選択コスト, 交換コスト, answerMapをまとめたタプルを返す.
        /// 成功した場合は <true, -1, -1, -1, null> が返る.
        /// タプルの中の物以外の問題設定はProblemのものを流用する.
        /// </returns>
        public static Tuple<bool, int, int, int, int, int, int[]> test(int solverNum)
        {
            int saveRow = Problem.row;
            int saveColumn = Problem.column;
            int savePartNum = Problem.partNum;
            int saveSelectionLimit = Problem.selectionLimit;
            int saveSelectionCost = Problem.selectionCost;
            int saveReplacementCost = Problem.replacementCost;

            bool result = true;
            int resultRow = -1;
            int resultColumn = -1;
            int resultSelectionLimit = -1;
            int resultSelectionCost = -1;
            int resultReplacementCost = -1;
            int[] resultAnswerMap = null;

            for (int _ = 0; _ < 100; _++)
            {
                Problem.row = Util.rand.Next(2, 17);
                Problem.column = Util.rand.Next(2, 17);
                Problem.partNum = Problem.row * Problem.column;
                Problem.selectionLimit = Util.rand.Next(2, 17); // 配布されたプログラムによると2~16
                Problem.selectionCost = Util.rand.Next(1, 301);
                Problem.replacementCost = Util.rand.Next(1, 101);

                List<int> tmp = new List<int>();
                for (int i = 0; i < Problem.partNum; i++)
                {
                    tmp.Add(i);
                }
                Util.randomizeList(tmp);

                int[] answerMap = tmp.ToArray();
                String answer = Solver.run(answerMap, solverNum);
                int[] restored = restore(answer);

                for (int i = 0; i < Problem.partNum; i++)
                {
                    // 復元が失敗していた場合
                    if(restored[i] != answerMap[i])
                    {
                        result = false;
                        resultRow = Problem.row;
                        resultColumn = Problem.column;
                        resultSelectionLimit = Problem.selectionLimit;
                        resultSelectionCost = Problem.selectionCost;
                        resultReplacementCost = Problem.replacementCost;
                        resultAnswerMap = answerMap;
                        goto end;
                    }
                }
            }
                
            end:
            Problem.row = saveRow;
            Problem.column = saveColumn;
            Problem.partNum = savePartNum;
            Problem.selectionLimit = saveSelectionLimit;
            Problem.selectionCost = saveSelectionCost;
            Problem.replacementCost = saveReplacementCost;

            return (new Tuple<bool, int, int, int, int, int, int[]>(
                result, resultRow, resultColumn, resultSelectionLimit, resultSelectionCost, resultReplacementCost, resultAnswerMap));
        }

        private static int[] restore(String answer)
        {
            String[] lines = answer.Split(new[] { "\r\n" }, StringSplitOptions.None);
            int selectNum = int.Parse(lines[0]);

            // 制限オーバーの場合, フォーマットエラー
            if (selectNum > Problem.selectionLimit)
            {
                new Exception();
            }

            // 2Dマップの構築
            int[,] map2d = new int[Problem.row, Problem.column];
            for (int i = 0; i < Problem.row; i++)
            {
                for (int j = 0; j < Problem.column; j++)
                {
                    map2d[i, j] = i * Problem.column + j;
                }
            }

            int lineAddr = 1;
            for (int _ = 0; _ < selectNum; _++)
            {
                String pos = lines[lineAddr++]; // "XY"
                if (!((Char.IsDigit(pos[0]) || Char.IsUpper(pos[0]))
                    && (Char.IsDigit(pos[1]) || Char.IsUpper(pos[1]))))
                {
                    // フォーマットエラー
                    new Exception();                    
                }
                int j = Convert.ToInt32(pos.Substring(0, 1), 16);
                int i = Convert.ToInt32(pos.Substring(1, 1), 16);

                int exchangeNum = Convert.ToInt32(lines[lineAddr++]);
                String op = lines[lineAddr++];

                for (int k = 0; k < exchangeNum; k++)
                {
                    switch(op[k])
                    {
                        case 'U': // UP
                            Util.swap(ref map2d[i, j], ref map2d[i - 1, j]);
                            i--;
                            break;

                        case 'D': // DOWN
                            Util.swap(ref map2d[i, j], ref map2d[i + 1, j]);
                            i++;
                            break;

                        case 'R': // RIGHT
                            Util.swap(ref map2d[i, j], ref map2d[i, j + 1]);
                            j++;
                            break;

                        case 'L': // LEFT
                            Util.swap(ref map2d[i, j], ref map2d[i, j - 1]);
                            j--;
                            break;

                        default:  // フォーマットエラー
                            new Exception();
                            break;
                    }
                }
            }

            int[] ret = new int[Problem.partNum];
            for (int i = 0; i < Problem.row; i++)
            {
                for (int j = 0; j < Problem.column; j++)
                {
                    ret[i * Problem.column + j] = map2d[i, j];
                }
            }

            return ret;
        }
    }
}
