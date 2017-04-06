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
using System.Collections.ObjectModel;
using Windows.UI;
using System.Threading.Tasks;
using System.Threading;
using Windows.ApplicationModel.Core;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RozkładJazdyv2.Pages.Favourites
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FavouritePage : Page
    {
        private ObservableCollection<Line> _FavouriteLines;
        private ObservableCollection<BusStopName> _FavouriteBusStops;

        public FavouritePage()
        {
            this.InitializeComponent();
            this.SetIsBackFromPageAllowed(true);

            _FavouriteLines = new ObservableCollection<Line>();
            _FavouriteBusStops = new ObservableCollection<BusStopName>();
            SetLinesGridViewStyle();

            BusStopsListView.SelectionChanged += BusStopsListView_SelectionChanged;
            this.Loaded += FavouritePage_LoadedAsync;
        }

        private void FavouritePage_LoadedAsync(object sender, RoutedEventArgs e)
        {
            BusStopsColumnGrid.Visibility = Visibility.Collapsed;
            BusStopsColumnGrid.Visibility = Visibility.Collapsed;

            LoadingProgressRing.IsActive = true;

            var areLinesInFavourites = LoadFavouriteLines();
            var areStopsInFavourites = LoadFavouriteStops();

            if (areLinesInFavourites || areStopsInFavourites)
            {
                HideNoItemsInFavouritesInfo();
                AlignBothColumns(areLinesInFavourites, areStopsInFavourites);

                if (areLinesInFavourites) // if we have both busses & favourites, we have to show left border on busses grid
                    ShowFavouritesLinesColumn(hideLeftBorder: !areStopsInFavourites);

                if (areStopsInFavourites)
                    ShowFavouritesBusStopsColumn();
            }
            else
            {
                HideFavouritesScheme();
                ShowNoItemsInFavouritesInfo();
            }

            LoadingProgressRing.IsActive = false;
        }

        private void AlignBothColumns(bool linesColumn, bool busStopsColumn)
        {
            if (linesColumn && busStopsColumn)
            {
                Grid.SetColumnSpan(LinesColumnGrid, 1);
                Grid.SetColumnSpan(BusStopsColumnGrid, 1);
                Grid.SetColumn(LinesColumnGrid, 1);
                Grid.SetColumn(BusStopsColumnGrid, 0);
            }
            else if (linesColumn && !busStopsColumn)
            {
                Grid.SetColumn(LinesColumnGrid, 0);
                Grid.SetColumnSpan(LinesColumnGrid, 2);
            }
            else if (!linesColumn && busStopsColumn)
            {
                Grid.SetColumnSpan(BusStopsColumnGrid, 2);
                Grid.SetColumn(BusStopsColumnGrid, 0);
            }
        }

        private bool LoadFavouriteLines()
        {
            _FavouriteLines.Clear();

            foreach (Line line in Timetable.Instance.Lines)
            {
                if (line.IsFavourite)
                    _FavouriteLines.Add(line);
            }

            return _FavouriteLines.Count() > 0;
        }

        private bool LoadFavouriteStops()
        {
            _FavouriteBusStops.Clear();

            List<BusStopName> favBusStops = SQLServices.QueryFavourite<BusStopName>("SELECT * FROM BusStopName");
            foreach (BusStopName favBusStopName in favBusStops)
            {
                var name = GetString(GetBytes(favBusStopName.Name));
                BusStopName properBusStopName = Timetable.Instance.BusStopsNames.FirstOrDefault(p => p.Name == name);
                if (properBusStopName == null)
                    continue;
                 _FavouriteBusStops.Add(properBusStopName);
            }

            return _FavouriteBusStops.Count() > 0;

            byte[] GetBytes(string str)
            {
                var bytesString = str.Split(new char[] { ' ' });
                byte[] bytes = bytesString.Select(p => (byte)int.Parse(p)).ToArray<byte>();
                return bytes;
            }

            string GetString(byte[] bytes)
            {
                char[] chars = new char[bytes.Length / sizeof(char)];
                System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
                return new string(chars);
            }
        }

        private void ShowFavouritesBusStopsColumn()
        {
            BusStopsColumnGrid.Visibility = Visibility.Visible;
        }

        private void ShowFavouritesLinesColumn(bool hideLeftBorder = true)
        {
            LinesColumnGrid.Visibility = Visibility.Visible;
            LinesColumnGrid.BorderThickness = new Thickness(hideLeftBorder ? 0.0 : 1.0, 0, 0, 0);
        }

        private void HideFavouritesScheme()
        {
            BusStopsColumnGrid.Visibility = Visibility.Collapsed;
            LinesColumnGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowNoItemsInFavouritesInfo()
        {
            InfoStackPanelLogoTextBlock.Visibility = Visibility.Visible;
            InfoStackPanelTextBlock.Text = $"Nie masz żadnej rzeczy w ulubionych.{Environment.NewLine}Wróć tu gdy już coś dodasz.";
            InfoStackPanelTextBlock.Visibility = Visibility.Visible;
        }

        private void HideNoItemsInFavouritesInfo()
        {
            InfoStackPanelLogoTextBlock.Visibility = Visibility.Collapsed;
            InfoStackPanelTextBlock.Visibility = Visibility.Collapsed;
        }

        private void SetLinesGridViewStyle()
        {
            FavouritesLinesGridView.ItemsPanel = App.Current.Resources["LinesGridViewItemPanelTemplate"] as ItemsPanelTemplate;
            FavouritesLinesGridView.ItemTemplate = App.Current.Resources["LineDataTemplate"] as DataTemplate;
        }

        private async void FavouritesLinesGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Line selectedLine = FavouritesLinesGridView.SelectedItem as Line;
            if (FavouritesLinesGridView.SelectedIndex == -1 || selectedLine == null)
                return;

            int selectedLineIndexInGridViewItemsList = FavouritesLinesGridView.Items.IndexOf(selectedLine);
            Grid selectedLineGridInGridView = (FavouritesLinesGridView.ItemsPanelRoot
                                                    .Children
                                                    .ElementAt(selectedLineIndexInGridViewItemsList)
                                                    as GridViewItem)
                                                    .ContentTemplateRoot as Grid;

            await Lines.LinesListPage.ShowLinePageBySchedulesAsync(selectedLine, selectedLineGridInGridView, Lines.LinesListPage.ScheduleClickedAsync);
        }

        private void BusStopsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BusStopsListView.SelectedIndex == -1 || BusStopsListView.SelectedItem == null)
                return;

            BusStopName selectedBusStop = ((ListView)sender).SelectedItem as BusStopName;
            MainFrameHelper.GetMainFrame().Navigate(typeof(Pages.BusStops.BusStopsListPage), 
                new Model.BusStopListPage.ChangeBusStopParametr() { BusStopName = selectedBusStop });

            BusStopsListView.SelectedIndex = -1;
        }

        private void BusStop_Holding(object sender, HoldingRoutedEventArgs e)
            => ShowFlyoutAtGrid(sender);

        private void BusStop_RightTapped(object sender, RightTappedRoutedEventArgs e)
            => ShowFlyoutAtGrid(sender);

        private void ShowFlyoutAtGrid(object sender)
            => FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

        private void BusStopFlyout_Click(object sender, RoutedEventArgs e)
        {
            BusStopName busStopName = ((sender as MenuFlyoutItem).DataContext as BusStopName);
            busStopName.IsFavourite = false;
            _FavouriteBusStops.Remove(busStopName);
        }

        private void MenuFlyoutBusStopItem_Loaded(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem textBlock = (sender as MenuFlyoutItem);
            BusStopName busStopName = textBlock.DataContext as BusStopName;
            textBlock.Text = "Usuń z ulubionych";
        }
    }
}
