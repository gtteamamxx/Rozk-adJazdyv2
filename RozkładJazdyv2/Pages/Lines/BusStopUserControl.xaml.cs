using RozkładJazdyv2.Model;
using RozkładJazdyv2.Model.LinesPage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RozkładJazdyv2.Pages.Lines
{
    public sealed partial class BusStopUserControl : UserControl
    {
        public delegate void BusStopGrid_PointerEntered(object sender, PointerRoutedEventArgs e);
        public static event BusStopGrid_PointerEntered OnBusStopGridPointerEntered;

        public BusStopUserControl()
        {
            this.InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                if (s.DataContext != null)
                    UpdateViewLayout(s.DataContext as LineViewBusStop);
            };
        }

        private void UpdateViewLayout(LineViewBusStop lineViewBusStop)
        {
            BusStop busStop = lineViewBusStop.BusStop;

            UpdateBusStopName(lineViewBusStop);

            SetDefaultStyle(busStop);
            SetBusStopStyle(busStop);
        }

        private void SetBusStopStyle(BusStop busStop)
        {
            if (busStop.IsLastStopOnTrack)
                SetLastStopOnTrackStyle();
            else if (busStop.IsVariant)
                SetIsVariantStyle();
            else if (busStop.IsOnDemand)
                SetIsOnDemandStyle();
            else if (busStop.IsBusStopZone)
                SetIsBusStopZoneStyle();
        }

        private void SetIsVariantStyle()
        {
            BusStopNameTextBlock.Foreground = new SolidColorBrush(Colors.DarkGray);
            BusStopNameTextBlock.FontWeight = FontWeights.ExtraLight;
        }

        private void SetLastStopOnTrackStyle()
        {
            BusStopNameTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(127, 255, 0, 0));
            BusStopNameTextBlock.FontWeight = FontWeights.ExtraBold;
        }

        private void SetDefaultStyle(BusStop busStop)
        {
            BusStopNameTextBlock.Foreground = new SolidColorBrush(Colors.White);
            BusStopNameTextBlock.FontWeight = FontWeights.Normal;
            UpdateFavouriteStyle(busStop);
        }

        private void UpdateFavouriteStyle(BusStop busStop, bool? fav = null)
        {
            bool isFavBusStop = false;
            if (fav == null)
                isFavBusStop = busStop.IsFavourite();
            else
                isFavBusStop = fav ?? false;

            this.FavouriteFlyoutTextBlock.Text = (isFavBusStop ? "Usuń z ulubionych" : "Dodaj do ulubionych");
            FauvoriteTextBlock.Text = isFavBusStop ? "\xE00B" : "";
        }

        private void SetIsBusStopZoneStyle()
            => BusStopNameTextBlock.Foreground = new SolidColorBrush(Colors.Yellow);

        private void SetIsOnDemandStyle()
            => BusStopNameTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));

        private void UpdateBusStopName(LineViewBusStop lineViewBusStop)
            => BusStopNameTextBlock.Text = lineViewBusStop.Name;

        private void LineViewBusStop_PointerEntered(object sender, PointerRoutedEventArgs e)
            => OnBusStopGridPointerEntered?.Invoke(sender, e);

        private void Favourite_Click(object sender, RoutedEventArgs e)
        {
            BusStop busStop = ((sender as MenuFlyoutItem).DataContext as LineViewBusStop).BusStop;
            bool isBusStopFavourite = busStop.IsFavourite();
            Timetable.Instance.BusStopsNames.First(p => p.Id == busStop.IdOfName).IsFavourite = (isBusStopFavourite = !isBusStopFavourite);
            UpdateFavouriteStyle(busStop, isBusStopFavourite);
        }

        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
            => ShowFlyoutAtGrid(sender);

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
            => ShowFlyoutAtGrid(sender);

        private void ShowFlyoutAtGrid(object sender)
            => FlyoutBase.ShowAttachedFlyout((FrameworkElement) sender);
    }
}
