using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

///
/// プログラム全体で用いる定数やメソッド等を定義している
///

namespace chino
{
    /// <summary>
    /// パズルの上下左右を表す定数 (列挙型)
    /// </summary>
    enum Position { UP, RIGHT, LEFT, DOWN };

    static class Util
    {
        public static Random rand = new Random();

        public static void swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public static void randomizeList<T>(List<T> lst)
        {
            lst.Sort((x, y) => Util.rand.NextDouble().CompareTo(0.5));
        }

        public static bool isRunning(Thread t)
        {
            return (t != null && t.IsAlive);
        }

        public static Thread makeThread(Action target)
        {
            return new Thread(new ThreadStart(() => 
            {
                try
                {
                    target();
                }
                catch (ThreadAbortException) { }
                catch (Exception) { }
            }));
        }
    }
}
