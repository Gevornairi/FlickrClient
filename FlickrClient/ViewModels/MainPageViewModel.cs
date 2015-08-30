using FlickrClient.Exceptions;
using FlickrClient.FlickrConnect;
using FlickrClient.Helpers;
using FlickrClient.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace FlickrClient.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        #region Constructor

        public MainPageViewModel()
        {
            SearchIconCommand = new DelegateCommand(SearchIconConmmandHandler, null, false);
            SearchImages(String.Empty);
            exManager = new ExceptionManager();
            navService = new NavigationService();
            SearchTextVisibility = Visibility.Collapsed;
                        
        }

        #endregion

        #region Modifiers

        ExceptionManager exManager = null;
        NavigationService navService = null;
        ObservableCollection<Photo> _photos;
        Visibility _searchTextVisibility;
        string _searchText;
        Photo _selectedPhoto;

        int _currentPage = 0;

        #endregion

        #region Properties

        public DelegateCommand SearchIconCommand { get; private set; }

        public ObservableCollection<Photo> Photos
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

        public int CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                }
            }
        }

        public Visibility SearchTextVisibility
        {
            get
            {
                return _searchTextVisibility;
            }
            set
            {
                if (_searchTextVisibility != value)
                {
                    _searchTextVisibility = value;
                    OnChange("SearchTextVisibility");
                }
            }
        }

        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnChange("SearchText");
                    SearchImages(SearchText);
                }
            }
        }

        public Photo SelectedPhoto
        {
            get
            {
                return _selectedPhoto;
            }
            set
            {
                if (_selectedPhoto != value)
                {
                    _selectedPhoto = value;
                    OnChange("SelectedPhoto");
                    navService.Navigate(typeof(FlickrClient.Views.PhotoInfo), _selectedPhoto);
                }
            }
        }

        #endregion

        #region Functionality

        private void SearchImages(string title)
        {
            try
            {

                var flicrRepository = new FlickrRepository();
                Photos = new ScrollableObservableCollection(flicrRepository.LoadImages(CurrentPage, title).Result, title);

            }
            catch (Exception ex)
            {
                exManager.PublishError(ex.Message);
            }
        }

        private void SearchIconConmmandHandler(Object state)
        {
            if (SearchTextVisibility == Visibility.Collapsed)
            {
                SearchTextVisibility = Visibility.Visible;
            }
            else
            {
                SearchTextVisibility = Visibility.Collapsed;
            }
        }

        

        #endregion
    }
}
