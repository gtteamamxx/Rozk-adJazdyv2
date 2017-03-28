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
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RozkładJazdyv2.Pages.Favourites
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FavouritePage : Page
    {
        private ObservableCollection<Line> _FavouriteLines;

        public FavouritePage()
        {
            this.InitializeComponent();
            this.SetIsBackFromPageAllowed(true);

            _FavouriteLines = new ObservableCollection<Line>();
            SetLinesGridViewStyle();

            this.Loaded += FavouritePage_Loaded;
        }

        private void FavouritePage_Loaded(object sender, RoutedEventArgs e)
        {
            bool areBussesInFavourites, areStopsInFavourites;

            LoadingProgressRing.IsActive = true;

            areBussesInFavourites = LoadFavouriteLines();
            areStopsInFavourites = LoadFavouriteStops();

            if (areBussesInFavourites || areStopsInFavourites)
            {
                HideNoItemsInFavouritesInfo();

                if (areBussesInFavourites) // if we have both busses & favourites, we have to show left border on busses grid
                    ShowFavouritesBussesColumn(hideLeftBorder: !areStopsInFavourites);

                if (areStopsInFavourites)
                {
                    if (!areBussesInFavourites)
                        ;
                    else
                        ;
                }
            }
            else
            {
                HideFavouritesScheme();
                ShowNoItemsInFavouritesInfo();
            }

            LoadingProgressRing.IsActive = false;
        }

        private bool LoadFavouriteLines()
        {
            _FavouriteLines.Clear();

            foreach (Line line in Timetable.Instance.Lines)
            {
                if (_FavouriteLines.FirstOrDefault(p => p == line) != null)
                    continue;

                if (line.IsFavourite)
                    _FavouriteLines.Add(line);
            }

            return _FavouriteLines.Count() > 0;
        }

        private bool LoadFavouriteStops()
        {
            return false;
        }

        private void ShowFavouritesBussesColumn(bool hideLeftBorder = true)
        {
            BusStopsColumnGrid.Visibility = Visibility.Visible;
            BusStopsColumnGrid.BorderBrush = new SolidColorBrush(hideLeftBorder ? Colors.Transparent : Colors.White);
        }

        private void HideFavouritesScheme()
        {
            BusStopsColumnGrid.Visibility = Visibility.Collapsed;
            LinesColumnGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowNoItemsInFavouritesInfo()
        {
            InfoStackPanelLogoTextBlock.Visibility = Visibility.Visible;
            InfoStackPanelTextBlock.Text = $"Nie masz żadnej rzeczy w ulubionych." +
                $"{Environment.NewLine}Wróć tu gdy już coś dodasz.";
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

            await Lines.LinesViewPage.ShowLinePageBySchedulesAsync(selectedLine, selectedLineGridInGridView, Lines.LinesViewPage.ScheduleClickedAsync);
        }

    }
}
