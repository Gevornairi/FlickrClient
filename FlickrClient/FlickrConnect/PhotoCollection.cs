using FlickrClient.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FlickrClient.FlickrConnect
{
    /// <remarks/>
    public sealed class PhotoCollection : PagedPhotoCollection, IFlickrParsable
    {
        void IFlickrParsable.Load(XmlReader reader)
        {
            if (reader.LocalName != "photos")
                UtilityMethods.CheckParsingException(reader);

            while (reader.MoveToNextAttribute())
            {
                switch (reader.LocalName)
                {
                    case "total":
                        Total = string.IsNullOrEmpty(reader.Value) ? 0 : int.Parse(reader.Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "perpage":
                    case "per_page":
                        PerPage = string.IsNullOrEmpty(reader.Value) ? 0 : int.Parse(reader.Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "page":
                        Page = string.IsNullOrEmpty(reader.Value) ? 0 : int.Parse(reader.Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "pages":
                        Pages = string.IsNullOrEmpty(reader.Value) ? 0 : int.Parse(reader.Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    default:
                        UtilityMethods.CheckParsingException(reader);
                        break;

                }
            }

            reader.Read();

            while (reader.LocalName == "photo")
            {
                var p = new Photo();
                ((IFlickrParsable)p).Load(reader);
                if (!string.IsNullOrEmpty(p.PhotoId)) Add(p);
            }

            // Skip to next element (if any)
            reader.Skip();

        }

    }
}
