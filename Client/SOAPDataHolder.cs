using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro.Client
{
    internal struct SOAPDataHolder
    {
        public string Name { get; set; }

        public object Value { get; set; }
        public bool isObject { get; set; }
        public bool isHeader { get; set; }
    }
}
