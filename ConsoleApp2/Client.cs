using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ConsoleApp2
{
    class Client
    {
        public string IpAddess { get; set; }
        public string Name { get; set; }

        public Client(string IpAddess)
        {
            this.IpAddess = IpAddess;
        }

        override
        public string ToString()
        {
            if (Name == null) return IpAddess;
            else return Name;
        }
    }
}
