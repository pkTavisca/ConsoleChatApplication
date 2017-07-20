using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class AsyncAndAwait
    {
        public int  SomeMethod()
        {
            WaitingMethod();
            return 1;
        }

        public async Task WaitingMethod()
        {
            Console.WriteLine("Before await");



            await Task.Delay(2000);



            Console.WriteLine("After await");




            return;
        }
    }
}
