using System;
using System.Collections.Generic;
using System.Text;

namespace RequestUnicorn
{
    public class PickupLocationBody
    {
        public PickupLocation PickupLocation { get; set; }
    }
    public class PickupLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
