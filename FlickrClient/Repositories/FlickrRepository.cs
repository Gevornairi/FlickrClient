using FlickrClient.Enums;
using FlickrClient.Exceptions;
using FlickrClient.Factories;
using FlickrClient.FlickrConnect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlickrClient.Repositories
{
    public class FlickrRepository
    {
        #region Constructor

        public FlickrRepository()
        {
            flFactory = new FlickrFactory();
            exManager = new ExceptionManager();
        }

        #endregion

        #region Modifiers

        private FlickrFactory flFactory;
        private ExceptionManager exManager;

        #endregion

        #region Functionality

        public async Task<PhotoCollection> LoadImages( int pageNumber, string title)
        {
            var f = flFactory.GetInstance();
            var photos = new PhotoCollection();

            f.PhotosSearchAsync(new PhotoSearchOptions { Tags = "colorful", PerPage = 100, SortOrder = PhotoSearchSortOrder.InterestingnessDescending, Page = pageNumber, Text = title }, (data) =>
            {
                photos = data.Result;

            });
            
            return photos;
        }

        public async Task<PlaceInfo> GetGeoInfo(string id)
        {
            var f = flFactory.GetInstance();
            var placeInfo = new PlaceInfo();

            f.PhotosGeoGetLocationAsync(id, (data) =>
            {
                placeInfo = data.Result;

            });

            return placeInfo;
        }

        #endregion
    }
}
