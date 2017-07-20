using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Lambda
    {
        public static void PatternThroughLambda()
        {
            int[] a = { 1, 2, 3, 4, 5, 6 };
            Func<int, bool> f = x => a[x] > 3;
            Console.Write(f.Invoke(5));
        }
    }
}
