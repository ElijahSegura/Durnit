using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Durnit
{
    public class DataNodeModel
    {
        public int address;


        public string URI { get; set; }

        public DataNodeModel(string s)
        {
            URI = s;
        }
    }
}
