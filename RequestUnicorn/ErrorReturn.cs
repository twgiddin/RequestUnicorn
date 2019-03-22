using System;
using System.Collections.Generic;
using System.Text;

namespace RequestUnicorn
{
    //How the node.js function was modeling the return message
    public class ErrorReturn
    {
        public string Error { get; set; }
        public string Reference { get; set; }
    }
}
