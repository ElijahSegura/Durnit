using Durnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyStartupApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(args[0]);
            Console.WriteLine(args[1]);
            Initialization i = new Initialization(args[0], args[1]);
            i.Start("dummy.xml");
            Console.WriteLine("HELLO");
        }
    }
}
