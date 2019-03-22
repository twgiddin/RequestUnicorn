using System;
using System.Collections.Generic;
using System.Text;

namespace RequestUnicorn
{
    //Unicorn class template based on the original json object that the node.js code was using
    public class Unicorn
    {
        public string Name { get; set; }

        public string Color { get; set; }

        public string Gender { get; set; }
    }
}
