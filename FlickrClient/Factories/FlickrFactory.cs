using FlickrClient.FlickrConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlickrClient.Factories
{
    public class FlickrFactory
    {
        public Flickr GetInstance()
        {
            return new Flickr("3a68f22971d8d66b521b362c312c175c");
        }
    }
}
