using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcOpenBuilder
{
    public enum POUType
    { 
        Program, 
        Function,
        Function_Block
    }
    public struct UDT
    {
        public List<UDTMemeber> Memebers; 
    }
    public struct UDTMemeber
    {
        public string Name;
        public string Type;
        public string Value; 
    }
}
