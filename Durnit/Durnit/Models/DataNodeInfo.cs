using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Durnit
{
    public class DataNodeInfo
    {
        public string URIAddress { get; set; }
        public int HowManyFriends { get; set; }
        public List<string> Files { get; set; }
        public List<string> connections { get; set; }
    }
}
