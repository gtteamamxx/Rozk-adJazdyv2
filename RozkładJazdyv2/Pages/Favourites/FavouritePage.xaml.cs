using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using RozkładJazdyv2.Model;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RozkładJazdyv2.Pages.Favourites
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FavouritePage : Page
    {
        public FavouritePage()
        {
            this.InitializeComponent();
            this.SetIsBackFromPageAllowed(true);

            this.Loaded += FavouritePage_Loaded;
        }

        private void FavouritePage_Loaded(object sender, RoutedEventArgs e)
        {
            bool areBussesInFavourites, areStopsInFavourites;

            LoadingProgressRing.IsActive = true;

            areBussesInFavourites = LoadFavouriteLines();
            areStopsInFavourites = LoadFavouriteStops();

            if (areBussesInFavourites || areStopsInFavourites)
                HideInfoStackPanel();
            else
                ShowNoItemsInFavouritesInfo();

            LoadingProgressRing.IsActive = false;
        }

        private bool LoadFavouriteLines()
        {
            return false;
        }

        private bool LoadFavouriteStops()
        {

            return false;
        }

        private void HideInfoStackPanel()
        {
            InfoStackPanel.Visibility = Visibility.Collapsed;
        }

        private void ShowNoItemsInFavouritesInfo()
        {
            InfoStackPanel.Visibility = Visibility.Visible;
            InfoStackPanelTextBlock.Visibility = Visibility.Visible;
        }
    }
}
