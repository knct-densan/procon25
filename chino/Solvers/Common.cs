using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace chino.Solvers
{
    static class Common
    {
        /// <summary>
        /// 回答ラインを最適化する
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static String optimizeLine(String line)
        {
            StringBuilder ret = new StringBuilder(line.Length);
            bool dup = true;

            foreach (char move in line)
            {
                int len = ret.Length;
                if (len == 0)
                {
                    dup = true;
                }
                else
                {
                    char tail = ret[len - 1];
                    if ((tail == 'U' && move == 'D')
                        || (tail == 'D' && move == 'U')
                        || (tail == 'L' && move == 'R')
                        || (tail == 'R' && move == 'L'))
                    {
                        dup = false;
                    }
                    else
                    {
                        dup = true;
                    }
                }

                if (dup)
                {
                    ret.Append(move);
                }
                else
                {
                    ret.Length--;
                }
            }
            return ret.ToString();
        }

        /// <summary>
        /// 転置されたマップに対する命令を, 転置する前の状態のものに置き換える.
        /// </summary>
        /// <param name="operations"></param>
        /// <returns></returns>
        public static String translateTransposedOperations(String operations)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < operations.Length; i++)
            {
                switch (operations[i])
                {
                    case 'U':
                        sb.Append('L');
                        break;
                    case 'L':
                        sb.Append('U');
                        break;
                    case 'R':
                        sb.Append('D');
                        break;
                    case 'D':
                        sb.Append('R');
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
