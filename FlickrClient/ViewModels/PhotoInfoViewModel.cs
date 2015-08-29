using FlickrClient.FlickrConnect;
using FlickrClient.Helpers;
using FlickrClient.Models;
using FlickrClient.Repositories;
using FlickrClient.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace FlickrClient.ViewModels
{
    public class PhotoInfoViewModel : BaseViewModel
    {
        #region Constructor

        public PhotoInfoViewModel()
        {
            BackCommand = new DelegateCommand(BackCommandHandler, null, false);
            MapCommand = new DelegateCommand(MapCommandHandler, null, false);
            navService = new NavigationService();
            flRepository = new FlickrRepository();
        }

        #endregion

        #region Modifiers

        ObservableCollection<PhotoDetailsModel> _photos;
        NavigationService navService = null;
        Visibility _mapCommandVisibility = Visibility.Visible;
        FlickrRepository flRepository = null;

        #endregion

        #region Properties

        public Photo CurrentPhoto { get; set; }

        public ObservableCollection<PhotoDetailsModel> Photos
        {
            get
            {
                return _photos;
            }
            set
            {
                if (_photos != value)
                {
                    _photos = value;
                    OnChange("Photos");
                }
            }
        }

        public DelegateCommand BackCommand { get; private set; }

        public DelegateCommand MapCommand { get; private set; }

        public Visibility MapCommandVisibility
        {
            get
            {
                return _mapCommandVisibility;
            }
            set
            {
                if (_mapCommandVisibility != value)
                {
                    _mapCommandVisibility = value;
                    OnChange("MapCommandVisibility");
                }
            }
        }

        #endregion

        #region Functionality

        public void InitialiseData()
        {
            var largePhotos = new ObservableCollection<PhotoDetailsModel>();

            if (!String.IsNullOrEmpty(CurrentPhoto.Medium640Url))
            {
                var item = new PhotoDetailsModel();
                item.Header = CurrentPhoto.Title;
                item.Url = CurrentPhoto.Medium640Url;
                largePhotos.Add(item);
            }

            if (!String.IsNullOrEmpty(CurrentPhoto.Medium800Url))
            {
                var item = new PhotoDetailsModel();
                item.Header = CurrentPhoto.Title;
                item.Url = CurrentPhoto.Medium800Url;
                largePhotos.Add(item);
            }

            if (!String.IsNullOrEmpty(CurrentPhoto.MediumUrl))
            {
                var item = new PhotoDetailsModel();
                item.Header = CurrentPhoto.Title;
                item.Url = CurrentPhoto.MediumUrl;
                largePhotos.Add(item);
            }

            if (!String.IsNullOrEmpty(CurrentPhoto.OriginalUrl))
            {
                var item = new PhotoDetailsModel();
                item.Header = CurrentPhoto.Title;
                item.Url = CurrentPhoto.OriginalUrl;
                largePhotos.Add(item);
            }

            if (!String.IsNullOrEmpty(CurrentPhoto.LargeUrl))
            {
                var item = new PhotoDetailsModel();
                item.Header = CurrentPhoto.Title;
                item.Url = CurrentPhoto.LargeUrl;
                largePhotos.Add(item);
            }

            if (!String.IsNullOrEmpty(CurrentPhoto.Large2048Url))
            {
                var item = new PhotoDetailsModel();
                item.Header = CurrentPhoto.Title;
                item.Url = CurrentPhoto.Large2048Url;
                largePhotos.Add(item);
            }

            if (!String.IsNullOrEmpty(CurrentPhoto.Large1600Url))
            {
                var item = new PhotoDetailsModel();
                item.Header = CurrentPhoto.Title;
                item.Url = CurrentPhoto.Large1600Url;
                largePhotos.Add(item);
            }

            Photos = largePhotos;

            var placeInfo = flRepository.GetGeoInfo(CurrentPhoto.PhotoId).Result;

            if (placeInfo != null)
            {
                if (placeInfo.Longitude == 0 || placeInfo.Latitude == 0)
                {
                    MapCommandVisibility = Visibility.Collapsed;
                }
                else
                {
                    CurrentPhoto.Latitude = placeInfo.Latitude;
                    CurrentPhoto.Longitude = placeInfo.Longitude;
                }
            }
            else
            {
                MapCommandVisibility = Visibility.Collapsed;
            }
        }

        private void BackCommandHandler(Object state)
        {
            navService.Navigate(typeof(MainPage)); 
        }

        private void MapCommandHandler(Object state)
        {
            navService.Navigate(typeof(Map), CurrentPhoto);
        }

        #endregion
    }
}
