using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using chino.Solvers;

namespace chino.Solvers.y1rInternal
{
    class PuzzleSolver
    {
        private Puzzle _puzzle;
        public Puzzle Puzzle
        {
            get
            {
                return this._puzzle;
            }
        }

        public Puzzle initial;

        private const int MAX_DEPTH = 3;

        /// <summary>
        /// コンストラクタ.
        /// 引数のpuzzleをクローンする.
        /// </summary>
        /// <param name="puzzle">パズル</param>
        public PuzzleSolver(Puzzle puzzle)
        {
            this._puzzle = (Puzzle)puzzle.Clone();
            initial = (Puzzle)puzzle.Clone();
        }

        public PuzzleSolver(Puzzle initial, Puzzle puzzle)
        {
            this._puzzle = (Puzzle)puzzle.Clone();
            this.initial = (Puzzle)initial.Clone();
        }

        /// <summary>
        /// パズルをサイズダウンします.
        /// </summary>
        /// <returns>解けたらtrue, 解けなければfalseを返します</returns>
        public bool Solve()
        {
            Puzzle best = null;
            List<Puzzle> nodes = new List<Puzzle>(8 * this._puzzle.Row * this._puzzle.Column);

            PuzzleSolver solver;

            /*
            if (this._puzzle.RowOrig == this._puzzle.Row + 2 || this._puzzle.ColumnOrig == this._puzzle.Column + 2)
            {
                y1r.test((Puzzle)initial.Clone(), this._puzzle.GetSolution());
                Console.ReadKey();
            }
            */

            
            if (this._puzzle.Column == 2 || this._puzzle.Row == 2 || ( this._puzzle.Column <= 3 && this._puzzle.Row <= 3 ))
            {
                    return true;
            }

            if (this._puzzle.maxChoice != 1)
            {
                for (int i = 0; i < this._puzzle.Column * this._puzzle.Row; i++)
                {
                    Puzzle tmp = (Puzzle)this._puzzle.Clone();
                    tmp.Choice(i);

                    if (tmp.Column > 3)
                    {
                        if (tmp.CanUse(Puzzle.Position.Left))
                        {
                            solver = new PuzzleSolver(initial, tmp);
                            if (solver.Solve(Puzzle.Position2.LD))
                                nodes.Add(solver._puzzle);

                            solver = new PuzzleSolver(initial, tmp);
                            if (solver.Solve(Puzzle.Position2.LU))
                                nodes.Add(solver._puzzle);
                        }

                        if (tmp.CanUse(Puzzle.Position.Right))
                        {
                            solver = new PuzzleSolver(initial, tmp);
                            if (solver.Solve(Puzzle.Position2.RD))
                                nodes.Add(solver._puzzle);

                            solver = new PuzzleSolver(initial, tmp);
                            if (solver.Solve(Puzzle.Position2.RU))
                                nodes.Add(solver._puzzle);
                        }
                    }
                    if (tmp.Row > 3)
                    {
                        if (tmp.CanUse(Puzzle.Position.Up))
                        {
                            solver = new PuzzleSolver(initial, tmp);
                            if (solver.Solve(Puzzle.Position2.UR))
                                nodes.Add(solver._puzzle);

                            solver = new PuzzleSolver(initial, tmp);
                            if (solver.Solve(Puzzle.Position2.UL))
                                nodes.Add(solver._puzzle);
                        }

                        if (tmp.CanUse(Puzzle.Position.Down))
                        {
                            solver = new PuzzleSolver(initial, tmp);
                            if (solver.Solve(Puzzle.Position2.DR))
                                nodes.Add(solver._puzzle);

                            solver = new PuzzleSolver(initial, tmp);
                            if (solver.Solve(Puzzle.Position2.DL))
                                nodes.Add(solver._puzzle);
                        }
                    }
                }
            }
            else
            {
                // 幅と高さのどちらかが3でなければサイズダウンする.
                if (this._puzzle.Column > 3)
                {
                    if (this._puzzle.CanUse(Puzzle.Position.Left))
                    {
                        solver = new PuzzleSolver(initial, this._puzzle);
                        if (solver.Solve(Puzzle.Position2.LD))
                            nodes.Add(solver._puzzle);

                        solver = new PuzzleSolver(initial, this._puzzle);
                        if (solver.Solve(Puzzle.Position2.LU))
                            nodes.Add(solver._puzzle);
                    }

                    if (this._puzzle.CanUse(Puzzle.Position.Right))
                    {
                        solver = new PuzzleSolver(initial, this._puzzle);
                        if (solver.Solve(Puzzle.Position2.RD))
                            nodes.Add(solver._puzzle);

                        solver = new PuzzleSolver(initial, this._puzzle);
                        if (solver.Solve(Puzzle.Position2.RU))
                            nodes.Add(solver._puzzle);
                    }
                }
                if (this._puzzle.Row > 3)
                {
                    if (this._puzzle.CanUse(Puzzle.Position.Up))
                    {
                        solver = new PuzzleSolver(initial, this._puzzle);
                        if (solver.Solve(Puzzle.Position2.UR))
                            nodes.Add(solver._puzzle);

                        solver = new PuzzleSolver(initial, this._puzzle);
                        if (solver.Solve(Puzzle.Position2.UL))
                            nodes.Add(solver._puzzle);
                    }
                    if (this._puzzle.CanUse(Puzzle.Position.Down))
                    {
                        solver = new PuzzleSolver(initial, this._puzzle);
                        if (solver.Solve(Puzzle.Position2.DR))
                            nodes.Add(solver._puzzle);

                        solver = new PuzzleSolver(initial, this._puzzle);
                        if (solver.Solve(Puzzle.Position2.DL))
                            nodes.Add(solver._puzzle);
                    }
                }
            }


            // 貪欲法
            
            while (nodes.Count != 0)
            {
                best = nodes.Min();
                nodes.Remove(best);
                solver = new PuzzleSolver(initial, best);
                if (solver.Solve())
                {
                    this._puzzle = solver.Puzzle;
                    return true;
                }
            }
            

            /*
            List<Puzzle> list = new List<Puzzle>(100);
            for (int i = 0; i < (2 < nodes.Count ? 2 : nodes.Count); i++)
            {
                best = nodes.Min();
                nodes.Remove(best);
                solver = new PuzzleSolver(best);
                if (solver.Solve())
                {
                    list.Add(solver._puzzle);
                }
            }

            best = list.Min();

            // TODO
            if (best == null)
                return false;
            else
            {
                this._puzzle = best;
                return true;
            }
            */

            return false;
            
            
/*            
            best = nodes.Min();

            if (best == null)
                return false;

            solver = new PuzzleSolver(best);
            if (!solver.Solve())
                return false;

            this._puzzle = solver.Puzzle;
*/            
            
            
            
            // 最適解  
            
            /*
            for (int i = 0; i < nodes.Count; i++)
            {
                solver = new PuzzleSolver(nodes[i]);
                if (!solver.Solve())
                    nodes.RemoveAt(i);
                else
                    nodes[i] = solver._puzzle;
            }

            best = nodes.Min();
            this._puzzle = best;
            */

//            return true;
        }

        /// <summary>
        /// パズルの指定された場所を揃えます.
        /// </summary>
        /// <param name="pos">揃える場所</param>
        /// <returns></returns>
        public bool Solve(Puzzle.Position2 pos)
        {
            Puzzle.Position target;
            bool status = false;
            if (pos == Puzzle.Position2.UL || pos == Puzzle.Position2.UR)
                target = Puzzle.Position.Up;
            else if (pos == Puzzle.Position2.DL || pos == Puzzle.Position2.DR)
                target = Puzzle.Position.Down;
            else if (pos == Puzzle.Position2.LD || pos == Puzzle.Position2.LU)
                target = Puzzle.Position.Left;
            else
                target = Puzzle.Position.Right;


            if (this._puzzle.CanUse(target))
            {
                this._puzzle.DumpPuzzle();
//                Console.WriteLine("揃える場所:{0}", System.Enum.GetName(typeof(Puzzle.Position), pos));
                switch (pos)
                {
                    case Puzzle.Position2.UR:
                        status =  SolveFirstRow1() && SolveFirstRow2();
                        break;

                    case Puzzle.Position2.DR:
                        status =  SolveLastRow1() && SolveLastRow2();
                        break;

                    case Puzzle.Position2.LD:
                        status = SolveFirstColumn1() && SolveFirstColumn2();
                        break;

                    case Puzzle.Position2.RD:
                        status = SolveLastColumn1() && SolveLastColumn2();
                        break;

                    case Puzzle.Position2.UL:
                        status = SolveFirstRow3() && SolveFirstRow4();
                        break;

                    case Puzzle.Position2.DL:
                        status = SolveLastRow3() && SolveLastRow4();
                        break;

                    case Puzzle.Position2.LU:
                        status = SolveFirstColumn3() && SolveFirstColumn4();
                        break;

                    case Puzzle.Position2.RU:
                        status = SolveLastColumn3() && SolveLastColumn4();
                        break;
                }
            }

/*            if (status)
            {
                y1r.test(initial, this.Puzzle.GetSolution());
            }
 */

            return status;
        }

        private bool SolveFirstRow1()
        {
            int now;
            int goal;

            for (int i = 1; i < this._puzzle.Column - 1; i++)
            {
                goal = this._puzzle.CoordToPosition( i, 1);
                now = this._puzzle.TileToPosition(goal);
                this._puzzle.DumpPuzzle();
                int tmp1 = -1, tmp2 = -1;
                this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
//                Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
                DoMove(now, goal, Puzzle.Position.Right, Puzzle.Position2.UR);
                this._puzzle.DoMove(Puzzle.Position.Down); // 退避
            }

            return true;
        }

        /*
         * 1 2 X 3
         * X X X 4
         * X X X B
         * ...
         *  |
         *  \/
         * 1 2 3 4
         * X X X B
         * ...
         * "LUURD"
         */

        private bool SolveFirstRow2()
        {
            int now;
            int goal;

            goal = this._puzzle.CoordToPosition(this._puzzle.Column, 1);
            now = this._puzzle.TileToPosition(
                                   this._puzzle.CoordToPosition
                                   (this._puzzle.Column - 1, 1)
                                   );
            this._puzzle.DumpPuzzle();
            int tmp1 = -1, tmp2 = -1;
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
//            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Down, Puzzle.Position2.UR);

            goal = this._puzzle.CoordToPosition(this._puzzle.Column, 2);
            now = this._puzzle.TileToPosition(
                                    this._puzzle.CoordToPosition
                                    (this._puzzle.Column, 1)
                                    );
            this._puzzle.DumpPuzzle();
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
//            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Down, Puzzle.Position2.UR);

            this._puzzle.DoMoves("LUURD");

            return this._puzzle.DoCut(Puzzle.Position.Up);
        }

        private bool SolveFirstRow3()
        {
            int now;
            int goal;

            for (int i = this._puzzle.Column; 2 < i; i--)
            {
                goal = this._puzzle.CoordToPosition(i, 1);
                now = this._puzzle.TileToPosition(goal);
                this._puzzle.DumpPuzzle();
                int tmp1 = -1, tmp2 = -1;
                this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
                DoMove(now, goal, Puzzle.Position.Left, Puzzle.Position2.UL);
                this._puzzle.DoMove(Puzzle.Position.Down); // 退避
            }

            return true;
        }

        /*
         * 2 X 3 4
         * 1 X X X
         * B X X X
         * ...
         *  |
         *  \/
         * 1 2 3 4
         * B X X X
         * ...
         * "RUULD"
         */

        private bool SolveFirstRow4()
        {
            int now;
            int goal;

            goal = this._puzzle.CoordToPosition(1, 1);
            now = this._puzzle.TileToPosition(
                                   this._puzzle.CoordToPosition
                                   (2, 1)
                                   );
            this._puzzle.DumpPuzzle();
            int tmp1 = -1, tmp2 = -1;
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
            //            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Down, Puzzle.Position2.UL);

            goal = this._puzzle.CoordToPosition(1, 2);
            now = this._puzzle.TileToPosition(
                                    this._puzzle.CoordToPosition
                                    (1, 1)
                                    );
            this._puzzle.DumpPuzzle();
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
            //            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Down, Puzzle.Position2.UL);

            this._puzzle.DoMoves("RUULD");

            return this._puzzle.DoCut(Puzzle.Position.Up);
        }

        private bool SolveLastRow1()
        {
            int now;
            int goal;

            for (int i = 1; i < this._puzzle.Column - 1; i++)
            {
                goal = this._puzzle.CoordToPosition(i, this._puzzle.Row);
                now = this._puzzle.TileToPosition(goal);
                this._puzzle.DumpPuzzle();
                int tmp1 = -1, tmp2 = -1;
                this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
//                Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
                DoMove(now, goal, Puzzle.Position.Right, Puzzle.Position2.DR);
                this._puzzle.DoMove(Puzzle.Position.Up); // 退避
            }

            return true;
        }

        /*  X  X X  X
         *  X  X X  B
         *  X  X X  15
         *  12 13 X  14
         *  |
         *  \/
         *  X  X  X  X
         *  X  X  X  X
         *  X  X  X  B
         *  12 13 14 15
         *  "LDDRU"
         */

        private bool SolveLastRow2()
        {
            int now;
            int goal;

            goal = this._puzzle.CoordToPosition(this._puzzle.Column, this._puzzle.Row);
            now = this._puzzle.TileToPosition(
                                   this._puzzle.CoordToPosition
                                   (this._puzzle.Column - 1, this._puzzle.Row)
                                   );
            this._puzzle.DumpPuzzle();
            int tmp1 = -1, tmp2 = -1;
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
//            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Up, Puzzle.Position2.DR);

            goal = this._puzzle.CoordToPosition(this._puzzle.Column, this._puzzle.Row - 1);
            now = this._puzzle.TileToPosition(
                                    this._puzzle.CoordToPosition
                                    (this._puzzle.Column, this._puzzle.Row)
                                    );
            this._puzzle.DumpPuzzle();
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
//            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Up, Puzzle.Position2.DR);

            this._puzzle.DoMoves("LDDRU");

            return this._puzzle.DoCut(Puzzle.Position.Down);
        }

        private bool SolveLastRow3()
        {
            int now;
            int goal;

            for (int i = this._puzzle.Column; 2 < i; i--)
            {
                goal = this._puzzle.CoordToPosition(i, this._puzzle.Row);
                now = this._puzzle.TileToPosition(goal);
                this._puzzle.DumpPuzzle();
                int tmp1 = -1, tmp2 = -1;
                this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
                //                Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
                DoMove(now, goal, Puzzle.Position.Left, Puzzle.Position2.DL);
                this._puzzle.DoMove(Puzzle.Position.Up); // 退避
            }

            return true;
        }

        /*  X  X X X
         *  B  X X X
         *  12 X X X
         *  13 X 14 15
         *  |
         *  \/
         *  X  X  X  X
         *  X  X  X  X
         *  X  X  X  B
         *  12 13 14 15
         *  "RDDLU"
         */

        private bool SolveLastRow4()
        {
            int now;
            int goal;

            goal = this._puzzle.CoordToPosition(1, this._puzzle.Row);
            now = this._puzzle.TileToPosition(
                                   this._puzzle.CoordToPosition
                                   (2, this._puzzle.Row)
                                   );
            this._puzzle.DumpPuzzle();
            int tmp1 = -1, tmp2 = -1;
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
            //            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Up, Puzzle.Position2.DL);

            goal = this._puzzle.CoordToPosition(1, this._puzzle.Row - 1);
            now = this._puzzle.TileToPosition(
                                    this._puzzle.CoordToPosition
                                    (1, this._puzzle.Row)
                                    );
            this._puzzle.DumpPuzzle();
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
            //            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Up, Puzzle.Position2.DL);

            this._puzzle.DoMoves("RDDLU");

            return this._puzzle.DoCut(Puzzle.Position.Down);
        }

        private bool SolveFirstColumn1()
        {
            int now;
            int goal;

            for (int i = 1; i < this._puzzle.Row - 1; i++)
            {
                goal = this._puzzle.CoordToPosition(1, i);
                now = this._puzzle.TileToPosition(goal);
                this._puzzle.DumpPuzzle();
                int tmp1 = -1, tmp2 = -1;
                this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
//                Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
                DoMove(now, goal, Puzzle.Position.Down, Puzzle.Position2.LD);
                this._puzzle.DoMove(Puzzle.Position.Right); // 退避
            }

            return true;
        }

        /* 0 X X X
         * 4 X X X
         * X X X X
         * 12 13 B X
         * |
         * \/
         * 0 X X X
         * 4 X X X
         * 12 X X X
         * 13 B X X
         * "ULLDR"
         */

        private bool SolveFirstColumn2()
        {
            int now;
            int goal;

            goal = this._puzzle.CoordToPosition(1, this._puzzle.Row);
            now = this._puzzle.TileToPosition(
                                   this._puzzle.CoordToPosition
                                   (1, this._puzzle.Row - 1)
                                   );
            this._puzzle.DumpPuzzle();
            int tmp1 = -1, tmp2 = -1;
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
//            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Right, Puzzle.Position2.LD);

            goal = this._puzzle.CoordToPosition(2, this._puzzle.Row);
            now = this._puzzle.TileToPosition(
                                    this._puzzle.CoordToPosition
                                    (1, this._puzzle.Row)
                                    );
            this._puzzle.DumpPuzzle();
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
//            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Right, Puzzle.Position2.LD);
            this._puzzle.DoMoves("ULLDR");

            return this._puzzle.DoCut(Puzzle.Position.Left);
        }

        private bool SolveFirstColumn3()
        {
            int now;
            int goal;

            for (int i = this._puzzle.Row; 2 < i; i--)
            {
                goal = this._puzzle.CoordToPosition(1, i);
                now = this._puzzle.TileToPosition(goal);
                this._puzzle.DumpPuzzle();
                int tmp1 = -1, tmp2 = -1;
                this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
                //                Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
                DoMove(now, goal, Puzzle.Position.Up, Puzzle.Position2.LU);
                this._puzzle.DoMove(Puzzle.Position.Right); // 退避
            }

            return true;
        }

        /* 0 X X X
         * 4 X X X
         * X X X X
         * 12 13 B X
         * |
         * \/
         * 4 0 B X
         * X X X X
         * 12 X X X
         * 13 B X X
         * "DLLUR"
         */

        private bool SolveFirstColumn4()
        {
            int now;
            int goal;

            goal = this._puzzle.CoordToPosition(1, 1);
            now = this._puzzle.TileToPosition(
                                   this._puzzle.CoordToPosition
                                   (1, 2)
                                   );
            this._puzzle.DumpPuzzle();
            int tmp1 = -1, tmp2 = -1;
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
            //            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Right, Puzzle.Position2.LU);

            goal = this._puzzle.CoordToPosition(2, 1);
            now = this._puzzle.TileToPosition(
                                    this._puzzle.CoordToPosition
                                    (1, 1)
                                    );
            this._puzzle.DumpPuzzle();
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
            //            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Right, Puzzle.Position2.LU);
            this._puzzle.DoMoves("DLLUR");

            return this._puzzle.DoCut(Puzzle.Position.Left);
        }

        private bool SolveLastColumn1()
        {
            int now;
            int goal;

            for (int i = 1; i < this._puzzle.Row - 1; i++)
            {
                goal = this._puzzle.CoordToPosition(this._puzzle.Column, i);
                now = this._puzzle.TileToPosition(goal);
                if (goal == now)
                    continue;
                this._puzzle.DumpPuzzle();
                int tmp1 = -1, tmp2 = -1;
                this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
//                Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
                DoMove(now, goal, Puzzle.Position.Down, Puzzle.Position2.RD);
                this._puzzle.DoMove(Puzzle.Position.Left);
            }

            return true;
        }

        /*
         * X X X 3
         * X X X 7
         * X X X X
         * X B 15 11
         * |
         * \/
         * X X X 3
         * X X X 7
         * X X X 11
         * X X B 15
         * "URRDL"
         */

        private bool SolveLastColumn2()
        {
            int now;
            int goal;

            goal = this._puzzle.CoordToPosition(this._puzzle.Column, this._puzzle.Row);
            now = this._puzzle.TileToPosition(
                                   this._puzzle.CoordToPosition
                                   (this._puzzle.Column, this._puzzle.Row - 1)
                                   );
            this._puzzle.DumpPuzzle();
            int tmp1 = -1, tmp2 = -1;
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
//            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Left, Puzzle.Position2.RD);

            goal = this._puzzle.CoordToPosition(this._puzzle.Column - 1, this._puzzle.Row);
            now = this._puzzle.TileToPosition(
                                    this._puzzle.CoordToPosition
                                    (this._puzzle.Column, this._puzzle.Row)
                                    );
            this._puzzle.DumpPuzzle();
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
//            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Left, Puzzle.Position2.RD);
            this._puzzle.DoMoves("URRDL");

            return this._puzzle.DoCut(Puzzle.Position.Right);
        }

        private bool SolveLastColumn3()
        {
            int now;
            int goal;

            for (int i = this._puzzle.Row; 2 < i; i--)
            {
                goal = this._puzzle.CoordToPosition(this._puzzle.Column, i);
                now = this._puzzle.TileToPosition(goal);
                if (goal == now)
                    continue;
                this._puzzle.DumpPuzzle();
                int tmp1 = -1, tmp2 = -1;
                this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
                //                Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
                DoMove(now, goal, Puzzle.Position.Up, Puzzle.Position2.RU);
                this._puzzle.DoMove(Puzzle.Position.Left);
            }

            return true;
        }

        /*
         * X X X 3
         * X X X 7
         * X X X X
         * X B 15 11
         * |
         * \/
         * X B 3 7
         * X X X X
         * X X X 11
         * X X X 15
         * "DRRUL"
         */

        private bool SolveLastColumn4()
        {
            int now;
            int goal;

            goal = this._puzzle.CoordToPosition(this._puzzle.Column, 1);
            now = this._puzzle.TileToPosition(
                                   this._puzzle.CoordToPosition
                                   (this._puzzle.Column, 2)
                                   );
            this._puzzle.DumpPuzzle();
            int tmp1 = -1, tmp2 = -1;
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
            //            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Left, Puzzle.Position2.RU);

            goal = this._puzzle.CoordToPosition(this._puzzle.Column - 1, 1);
            now = this._puzzle.TileToPosition(
                                    this._puzzle.CoordToPosition
                                    (this._puzzle.Column, 1)
                                    );
            this._puzzle.DumpPuzzle();
            this._puzzle.PositionToCoord(goal, ref tmp1, ref tmp2);
            //            Console.WriteLine("col:{0:d}, row:{1:d}", tmp1, tmp2);
            DoMove(now, goal, Puzzle.Position.Left, Puzzle.Position2.RU);
            this._puzzle.DoMoves("DRRUL");

            return this._puzzle.DoCut(Puzzle.Position.Right);
        }

        private bool DoMove(int now, int goal, Puzzle.Position blankPos, Puzzle.Position2 forbid)
        {
            MoveBlank(ref now, blankPos, forbid);
//            Console.WriteLine("START");
            int nowColumn = -1, nowRow = -1, goalColumn = -1, goalRow = -1;

            this._puzzle.PositionToCoord(now, ref nowColumn, ref nowRow);
            this._puzzle.PositionToCoord(goal, ref goalColumn, ref goalRow);

            StringBuilder move = new StringBuilder();
            Nullable<char> moveDirection = null;
            char blankPosChar = this._puzzle.PositionToChar(blankPos);
            char blankPosCharReverse = this._puzzle.PositionToChar(this._puzzle.ReversePosition(blankPos));

            while (nowColumn != goalColumn || nowRow != goalRow)
            {
                move.Clear();

                if (nowRow < goalRow)
                {
                    if (this._puzzle.CanMove(nowColumn, nowRow + 1, goal, forbid))
                    {
                        move.Append('D');
                        nowRow++;
                    }
                }
                else if (goalRow < nowRow)
                {
                    if (this._puzzle.CanMove(nowColumn, nowRow - 1, goal, forbid))
                    {
                        move.Append('U');
                        nowRow--;
                    }
                }

                if (nowColumn < goalColumn)
                {
                    if (this._puzzle.CanMove(nowColumn + 1, nowRow, goal, forbid))
                    {
                        move.Append('R');
                        nowColumn++;
                    }
                }
                else if (goalColumn < nowColumn)
                {
                    if (this._puzzle.CanMove(nowColumn - 1, nowRow, goal, forbid))
                    {
                        move.Append('L');
                        nowColumn--;
                    }
                }
                if (move.Length == 0)
                    throw new Exception();

                if (move.Length == 2)
                    moveDirection = null;
                else
                {
                    moveDirection = move[0];
                }

                move.Append('_');
                move.Append(blankPosChar);

                if (!DoMoveHelper(move, blankPosChar, blankPosCharReverse, forbid, goal))
                    throw new Exception();
//                Console.WriteLine("END");
            }

            return true;
        }

        /// <summary>
        /// 使用するマクロを選んで移動をする
        /// </summary>
        /// <param name="macro">移動したい方向</param>
        /// <param name="blankPosChar">ブランクの場所</param>
        /// <param name="blankPosCharReverse">ブランクの場所反転</param>
        /// <param name="forbid">並べている場所</param>
        /// <param name="goal">目的地</param>
        /// <returns></returns>
        private bool DoMoveHelper( StringBuilder macro, char blankPosChar, char blankPosCharReverse, Puzzle.Position2 forbid, int goal )
        {
            // 2 moves
            if (macro.Length == 3)
            {
                char moveDirection = macro[0];
                if (moveDirection == blankPosChar || moveDirection == blankPosCharReverse)
                {
                    macro.Append('_');
                    if (blankPosChar == 'U' || blankPosChar == 'D')
                    {
                        macro.Append('R');
                        if (!this._puzzle.DoMoves(MACRO[macro.ToString()], forbid, goal))
                        {
                            macro[macro.Length - 1] = 'L';
                            if (!this._puzzle.DoMoves(MACRO[macro.ToString()], forbid, goal))
                                throw new Exception();
                        }
                    }
                    if (blankPosChar == 'L' || blankPosChar == 'R')
                    {
                        macro.Append('D');
                        if (!this._puzzle.DoMoves(MACRO[macro.ToString()], forbid, goal))
                        {
                            macro[macro.Length - 1] = 'U';
                            if (!this._puzzle.DoMoves(MACRO[macro.ToString()], forbid, goal))
                                throw new Exception();
                        }
                    }
                    return true;
                }
                else if (!this._puzzle.DoMoves(MACRO[macro.ToString()], forbid, goal))
                    return false;
                return true;
            }
            // 3 moves
            else if (macro.Length == 4)
            {
                if (!this._puzzle.DoMoves(MACRO[macro.ToString()], forbid, goal))
                {
                    StringBuilder macro1 = new StringBuilder();
                    StringBuilder macro2 = new StringBuilder();
                    macro1.Append(macro[0]);
                    macro2.Append(macro[1]);
                    macro1.Append(macro[2]);
                    macro1.Append(macro[3]);
                    macro2.Append(macro[2]);
                    macro2.Append(macro[3]);
                    if (!(DoMoveHelper(macro1, blankPosChar, blankPosCharReverse, forbid, goal) && DoMoveHelper(macro2, blankPosChar, blankPosCharReverse, forbid, goal)))
                    {
                        macro1.Length = 3;
                        macro2.Length = 3;
                        if (!(DoMoveHelper(macro2, blankPosChar, blankPosCharReverse, forbid, goal) && DoMoveHelper(macro1, blankPosChar, blankPosCharReverse, forbid, goal)))
                            throw new Exception();
                    }
                }
                return true;
            }

            throw new Exception();
        }

        /// <summary>
        /// ブランクを移動させる
        /// </summary>
        /// <param name="goal">ブランクを横に置きたい対象の場所</param>
        /// <param name="blankPos">ブランクの位置</param>
        /// <param name="forbid">現在並べている場所</param>
        /// <returns></returns>
        private bool MoveBlank(ref int goal, Puzzle.Position blankPos, Puzzle.Position2 forbid)
        {
//            Console.WriteLine("MoveBlank_start");
            this._puzzle.DumpPuzzle();
            int goal_column = -1, goal_row = -1;

            this._puzzle.PositionToCoord(goal, ref goal_column, ref goal_row);

            bool status = false;
            int blank_row = -1, blank_column = -1;
            this._puzzle.PositionToCoord(this._puzzle.BlankPos, ref blank_column, ref blank_row);

            switch (blankPos)
            {
                case Puzzle.Position.Up:
                    goal_row--;
                    status = goal_row >= 1 && !(blank_row > goal_row && blank_column == goal_column);
                    break;

                case Puzzle.Position.Down:
                    goal_row++;
                    status = goal_row <= this._puzzle.Row && !(blank_row < goal_row && blank_column == goal_column);
                    break;

                case Puzzle.Position.Left:
                    goal_column--;
                    status = goal_column >= 1 && !(blank_column > goal_column && blank_row == goal_row);
                    break;

                case Puzzle.Position.Right:
                    goal_column++;
                    status = goal_column <= this._puzzle.Column && !(blank_column < goal_column && blank_row == goal_row);
                    break;
            }

            if (status)
            {
                // ブランクを対象の上下に置く場合
                if (blankPos == Puzzle.Position.Up || blankPos == Puzzle.Position.Down)
                {
                    while (blank_row != goal_row)
                    {
                        if (blank_row > goal_row)
                        {
                            this._puzzle.DoMove(Puzzle.Position.Up);
                            blank_row--;
                        }
                        else if (goal_row > blank_row)
                        {
                            this._puzzle.DoMove(Puzzle.Position.Down);
                            blank_row++;
                        }
                    }
                    while (blank_column != goal_column)
                    {
                        if (blank_column > goal_column)
                        {
                            this._puzzle.DoMove(Puzzle.Position.Left);
                            blank_column--;
                        }
                        else if (goal_column > blank_column)
                        {
                            this._puzzle.DoMove(Puzzle.Position.Right);
                            blank_column++;
                        }
                    }
                }
                // ブランクを対象の左右に置く場合
                else
                {
                    while (blank_column != goal_column)
                    {
                        if (blank_column > goal_column)
                        {
                            this._puzzle.DoMove(Puzzle.Position.Left);
                            blank_column--;
                        }
                        else if (goal_column > blank_column)
                        {
                            this._puzzle.DoMove(Puzzle.Position.Right);
                            blank_column++;
                        }
                    }
                    while (blank_row != goal_row)
                    {
                        if (blank_row > goal_row)
                        {
                            this._puzzle.DoMove(Puzzle.Position.Up);
                            blank_row--;
                        }
                        else if (goal_row > blank_row)
                        {
                            this._puzzle.DoMove(Puzzle.Position.Down);
                            blank_row++;
                        }
                    }
                }

            }
            else
            {
                Puzzle.Position rev = this._puzzle.ReversePosition(blankPos);
                MoveBlank(ref goal, rev, forbid);
                this._puzzle.DoMove(blankPos);
                switch (blankPos)
                {
                    case Puzzle.Position.Up:
                        goal += this._puzzle.Column;
                        break;

                    case Puzzle.Position.Down:
                        goal -= this._puzzle.Column;
                        break;

                    case Puzzle.Position.Left:
                        goal++;
                        break;

                    case Puzzle.Position.Right:
                        goal--;
                        break;
                }
            }

            this._puzzle.DumpPuzzle();
//            Console.WriteLine("MoveBlank_end");
            return true;
        }

        /* MOVE MACROS Thanks to Mr.Nishi !*/

        // MACRO_[move]_[blank place] ( _[way] )

        // 独自追加したものあり
        // 以下論文にない追加したもの

        // Up および Down の 右回り または 左回り を一部追加

        // Up_Right, Up_Left, Down_Right, Down_Left の全てに
        // 下からの移動( _D )がなかったので追加

        Dictionary<string, string> MACRO =
            new Dictionary<string, string>()
            {
                {"U_L", "URDLU"},
                {"U_R", "ULDRU"},
                {"U_U_L", "DLUUR"},
                {"U_U_R", "DRUUL"},
                {"U_D_L", "LUURD"},
                {"U_D_R", "RUULD"},
                {"D_L", "DRULD"},
                {"D_R", "DLURD"},
                {"D_U_L", "LDDRU"},
                {"D_U_R", "RDDLU"},
                {"D_D_L", "ULDDR"},
                {"D_D_R", "URDDL"},
                {"L_U", "LDRUL"},
                {"L_D", "LURDL"},
                {"L_L_U", "RULLD"},
                {"L_L_D", "RDLLU"},
                {"L_R_U", "ULLDR"},
                {"L_R_D", "DLLUR"},
                {"R_U", "RDLUR"},
                {"R_D", "RULDR"},
                {"R_L_U", "URRDL"},
                {"R_L_D", "DRRUL"},
                {"R_R_U", "LURRD"},
                {"R_R_D", "LDRRU"},
                {"UR_U", "DRULUR"},
                {"UR_D", "RULURD"},
                {"UR_L", "URDRUL"},
                {"UR_R", "LURDRU"},
                {"UL_U", "DLURUL"},
                {"UL_D", "LURULD"},
                {"UL_L", "RULDLU"},
                {"UL_R", "ULDLUR"},
                {"DR_U", "RDLDRU"},
                {"DR_D", "URDLDR"},
                {"DR_L", "DRURDL"},
                {"DR_R", "LDRURD"},
                {"DL_U", "LDRDLU"},
                {"DL_D", "ULDRDL"},
                {"DL_L", "RDLULD"},
                {"DL_R", "DLULDR"}
            };
    }
}
