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
            Initialization i = new Initialization();
            var nameNodeInfo = i.Start("dummy.xml");
            Console.WriteLine(nameNodeInfo.Address + ": " + nameNodeInfo.Port);
        }
    }
}
