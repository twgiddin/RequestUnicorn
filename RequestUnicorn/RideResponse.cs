using System;
using System.Collections.Generic;
using System.Text;

namespace RequestUnicorn
{
    //RideResponse class template based on the original json object that the node.js code was using
    public class RideResponse
    {
        public string RideId { get; set; }
        public Unicorn Unicorn { get; set; }
        public string UnicornName { get; set; }
        public string ETA { get; set; }
        public string Rider { get; set; }


    }
}
