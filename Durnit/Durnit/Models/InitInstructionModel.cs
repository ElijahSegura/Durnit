using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Durnit.Models
{
    public class InitInstructionModel
    {
        public string Address { get; set; }
        public string Port { get; set; }
        public InitInstructions Instruction { get; set; }
        public string NameNodeAddress { get; set; }
        public string NameNodePort { get; set; }
    }
}
