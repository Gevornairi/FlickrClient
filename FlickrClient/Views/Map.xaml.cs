using FlickrClient.FlickrConnect;
using FlickrClient.Helpers;
using FlickrClient.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FlickrClient.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Map : Page
    {
        #region Modifiers

        MapViewModel viewModel = null;

        #endregion

        public Map()
        {
            this.InitializeComponent();
            viewModel = new MapViewModel();
            this.DataContext = viewModel;            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            viewModel.CurrentPhoto = e.Parameter as Photo;
            viewModel.InitialiseData();
            photoMap.Center = new Windows.Devices.Geolocation.Geopoint(
                new Windows.Devices.Geolocation.BasicGeoposition
                { Latitude = viewModel.CurrentPhoto.Latitude, Longitude = viewModel.CurrentPhoto.Longitude });
            base.OnNavigatedTo(e);
        }

        private void OnMapTapped(MapControl sender, MapInputEventArgs args)
        {
            viewModel.Locations.Add(new Location { Geopoint = args.Location });
            sender.Center = args.Location;
        }
    }
}
