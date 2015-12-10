using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Durnit.Exceptions
{
    class SyntaxErrorException : Exception
    {
        public SyntaxErrorException(string problemElement) : base(problemElement) { }
    }
}
