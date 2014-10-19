using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using chino.Solvers.y1rInternal;

namespace chino.Solvers
{
    static class y1r
    {
        public static String run(int[,] map2d)
        {
            // y1r-Solverは2xM, Nx2のパズルには対応していないため,
            // この時は横長, 縦長に強いSolver1を流用する
            if (Problem.row == 2 || Problem.column == 2)
            {
                return Solver1.run(map2d);
            }

            int[] puzzle = new int[Problem.partNum];
            for (int i = 0; i < Problem.row; i++)
                for (int j = 0; j < Problem.column; j++)
                    puzzle[i * Problem.column + j] = map2d[i, j];

            Puzzle initial = new Puzzle( puzzle, Problem.column, Problem.row, Problem.selectionLimit, Problem.selectionCost, Problem.replacementCost);
            PuzzleSolver solver = new PuzzleSolver(initial, initial);

            solver.Solve();

    //        test(initial, solver.Puzzle.GetSolution());

            Puzzle miniMap = solver.Puzzle;


            return MiniPuzzleSolver.Solve(miniMap);

        }

        public static void test( Puzzle initial, String proconFormat ){
            String[] lines = proconFormat.Split( new string[]{Environment.NewLine}, StringSplitOptions.None);
            int choiceCount = int.Parse( lines[0] );
            for( int i = 0; i < choiceCount; i++ )
            {
                String pos = lines[1 + i * 3];
                int moveCount = int.Parse(lines[2 + i * 3]);
                int blankColumn = int.Parse(pos[0].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier) + 1;
                int blankRow = int.Parse(pos[1].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier) + 1;
                int blank = initial[blankColumn,blankRow];
                initial.Choice(blank);
                for( int j = 0; j < moveCount; j++ )
                {
                    Puzzle.Position move = initial.CharToPosition(lines[(i + 1) * 3][j]);
                    if (!initial.DoMove(move))
                        throw new Exception();
                }
            }

            Console.WriteLine();
            for (int i = 1; i <= Problem.row; i++)
            {
                for (int j = 1; j <= Problem.column; j++)
                {
                    Console.Write(initial.Data[initial.CoordToPosition(j, i)] + "\t");
                }
                Console.WriteLine();
            }
        }
    }
}
