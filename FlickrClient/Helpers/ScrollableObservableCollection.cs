using FlickrClient.FlickrConnect;
using FlickrClient.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;

namespace FlickrClient.Helpers
{
    public class ScrollableObservableCollection : ObservableCollection<Photo>, ISupportIncrementalLoading
    {
        public ScrollableObservableCollection(IEnumerable<Photo> items, string title) : base(items)
        {
            HasMoreItems = true;
            this.title = title;
        }
        public bool HasMoreItems { get; set; }
        private bool isRunning = false;
        private int currentPage = 0;
        private string title;

        public Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (isRunning) //not thread safe
            {
                throw new InvalidOperationException("Only one operation in flight at a time");
            }

            isRunning = true;

            int itemsCount = 0;

            return AsyncInfo.Run(async c =>
           {
               try
               {
                   currentPage++;
                   FlickrRepository repository = new FlickrRepository();
                   var result = await repository.LoadImages(currentPage, title);

                   var photos = new PhotoCollection();
                   if (result != null)
                   {
                       photos = result;
                       HasMoreItems = result.Any();
                       itemsCount = result.Count;
                   }

                   foreach (var item in photos)
                   {
                       this.Add(item);
                   }

                   isRunning = false;
               }
               catch
               {
                   HasMoreItems = false;
               }

               LoadMoreItemsResult res = new LoadMoreItemsResult() { Count = (uint)itemsCount };
               return res;

           });


        }
    }
}
