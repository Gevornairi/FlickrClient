using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace FlickrClient.Helpers
{
    public class Location
    {
        public Geopoint Geopoint { get; set; }

        public Point Anchor { get { return new Point(0.5, 1); } }
    }
}
