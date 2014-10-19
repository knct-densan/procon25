using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chino.Solvers.y1rInternal
{
    static class MiniPuzzleSolver
    {
        public static String Solve( Puzzle map )
        {
            Puzzle initialNode = map;

            Puzzle end = null;
            var moves = Enum.GetValues(typeof(Puzzle.Position));

            PriorityQueue<Puzzle> nodes = new PriorityQueue<Puzzle>();

            nodes.Push(initialNode);

            while (nodes.Count != 0)
            {
                Puzzle currentNode = nodes.Top;
                nodes.Pop();

                if (currentNode.MD == 0)
                {
                    end = currentNode;
                    break;
                }

                if (currentNode.Blank != -1)
                {
                    foreach (var move in moves)
                    {
                        if (currentNode.NowStatus.answer.Count != 0)
                        {
                            Puzzle.Position now = currentNode.NowStatus.answer[currentNode.NowStatus.answer.Count - 1];
                            if (currentNode.ReversePosition(now) == (Puzzle.Position)move)
                                continue;
                        }
                        Puzzle newPuzzle = (Puzzle)currentNode.Clone();
                        if (newPuzzle.DoMove((Puzzle.Position)move))
                        {
                            newPuzzle.CalculateMD();
                            nodes.Push(newPuzzle);
                        }
                    }
                }

                if (currentNode.maxChoice != 0 && ( currentNode.NowStatus.answer.Count != 0 || currentNode.Blank == -1 ))
                {
                    for (int i = 0; i < currentNode.Data.Count(); i++)
                    {
                        if (i == currentNode.Blank)
                            continue;

                        Puzzle newPuzzle = (Puzzle)currentNode.Clone();
                        newPuzzle.Choice(i);
                        newPuzzle.CalculateMD();
                        nodes.Push(newPuzzle);
                    }
                }
            }

            if (end != null){
                return end.GetSolution();
            }
            else
                return "";
        }
    }
}