using FlickrClient.FlickrConnect;
using FlickrClient.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace FlickrClient.ViewModels
{
    public class MapViewModel : BaseViewModel
    {
        #region Constructor

        public MapViewModel()
        {
            navService = new NavigationService();
            BackCommand = new DelegateCommand(BackCommandHandler, null, false);
        }

        #endregion

        #region Modifiers

        NavigationService navService = null;

        private ObservableCollection<Location> _locations = new ObservableCollection<Location>();

        #endregion

        #region Properties

        public ObservableCollection<Location> Locations
        {
            get
            {

                return _locations;
            }
            set
            {
                if (_locations != value)
                {
                    _locations = value;
                    OnChange("Locations");
                }                
            }
        }

        public Photo CurrentPhoto { get; set; }

        public DelegateCommand BackCommand { get; private set; }

        #endregion

        #region Functionality

        private void BackCommandHandler(Object state)
        {
            navService.Navigate(typeof(FlickrClient.Views.PhotoInfo), CurrentPhoto);
        }

        public void InitialiseData()
        {
            Locations.Add(new Location { Geopoint = new Geopoint(new BasicGeoposition() { Latitude = CurrentPhoto.Latitude, Longitude = CurrentPhoto.Longitude }) });
        }

        #endregion
    }
}
