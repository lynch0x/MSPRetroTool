using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro.Client
{
    internal class SOAPObject
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public SOAPObject(string name, object value)
        {
            Name = name;
            Value = value;
        }
        public SOAPObject(string name)
        {
            Name = name;
        }
    }
}
