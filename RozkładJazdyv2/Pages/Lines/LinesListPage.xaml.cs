using RozkładJazdyv2.Model;
using RozkładJazdyv2.Model.LinesPage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RozkładJazdyv2.Pages.Lines
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LinesViewPage : Page
    {
        public enum LinesType
        {
            Favourites,
            Trams,
            Busses,
            Fast_Busses,
            Night_Busses,
            Mini_Busses,
            Others
        }

        private static List<Tuple<LinesType, GridView>> _LinesGridViews;

        private bool _IsPageCached;

        private ObservableCollection<Grid> _SearchLinesGrids;

        public LinesViewPage()
        {
            this.InitializeComponent();
            this.SetIsBackFromPageAllowed(true);

            _LinesGridViews = new List<Tuple<LinesType, GridView>>();
            _SearchLinesGrids = new ObservableCollection<Grid>();

            RegisterHooks();

            this.Loaded += LinesViewPage_Loaded;
        }

        public static void RefreshLineGridView(LinesType type, int acceptedLinesSumBit)
            => Model.LinesPage.LinesListViewManager.RefreshGridView(_LinesGridViews.First(p => p.Item1 == type).Item2, acceptedLinesSumBit);

        private void RegisterHooks()
            => SearchLineAutoSuggestBox.SuggestionChosen += SearchLineAutoSuggestBox_SuggestionChosenAsync;


        private async void SearchLineAutoSuggestBox_SuggestionChosenAsync(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            Grid mainGrid = (Grid)sender.Parent;
            Grid selectedGrid = args.SelectedItem as Grid;
            Line selectedLine = selectedGrid.DataContext as Line;

            selectedLine = await FillLineSchedulesAsync(selectedLine);

            await ShowLinePageBySchedulesAsync(selectedLine, mainGrid, ScheduleClickedAsync);
        }

        private async void LinesViewPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_IsPageCached == true)
                return;

            await LoadLinesToView();

            HideLoadingStackPanel();
            ShowSearchLineAutoSuggestBox();
        }


        public static async Task ShowLinePageBySchedulesAsync(Line line, Grid lineBackgroundGrid, RoutedEventHandler scheduleClickedAsyncFunction)
        {
            if (line.Schedules == null)
                line.Schedules = await GetLineSchedules(line);

            if (line.Schedules.Count() == 1)
            {
                if (line.Schedules[0].Name.Contains("zawie")) //line is stopped
                    FlyoutHelper.ShowFlyOutLineIsStoppedAtLineGrid(lineBackgroundGrid, line);
                else
                    await ShowLinePageAsync(new ChangeLineParameter() { Line = line, SelectedSchedule = line.Schedules.ElementAt(0) });
            }
            else
                FlyoutHelper.ShowFlyOutWithSchedulesAtLineGrid(lineBackgroundGrid, line, scheduleClickedAsyncFunction);
        }


        private async Task LoadLinesToView()
        {
            _IsPageCached = true;

            LinesListViewManager.SetInstance(LinesScrollViewer);
            int busBits = GetBusBitsWithoutFastBus();

            await LinesListViewManager.AddLineTypeToListViewAsync(LinesType.Favourites, "Ulubione", Line.FAVOURITE_BIT, LineSelectionChangedAsync);
            await LinesListViewManager.AddLineTypeToListViewAsync(LinesType.Trams, "Tramwaje", Line.TRAM_BITS, LineSelectionChangedAsync);
            await Task.Delay(100);
            await LinesListViewManager.AddLineTypeToListViewAsync(LinesType.Busses, "Autobusy", busBits, LineSelectionChangedAsync);
            await LinesListViewManager.AddLineTypeToListViewAsync(LinesType.Fast_Busses, "Autobusy przyśpieszone", Line.FAST_BUS_BIT, LineSelectionChangedAsync);
            await LinesListViewManager.AddLineTypeToListViewAsync(LinesType.Night_Busses, "Nocne", Line.NIGHT_BUS_BIT, LineSelectionChangedAsync);
            await LinesListViewManager.AddLineTypeToListViewAsync(LinesType.Mini_Busses, "Minibusy", Line.MINI_BIT, LineSelectionChangedAsync);
            await LinesListViewManager.AddLineTypeToListViewAsync(LinesType.Others, "Inne", Line.AIRPORT_BIT, LineSelectionChangedAsync);

            _LinesGridViews = LinesListViewManager.GetLineTypesGridViewList();
        }

        private int GetBusBitsWithoutFastBus()
        {
            int busBits = Line.BUS_BITS;
            busBits &= ~(Line.FAST_BUS_BIT);
            return busBits;
        }

        private static async Task ShowLinePageAsync(ChangeLineParameter changeLineParameter)
        {
            await Task.Delay(100);
            MainFrameHelper.GetMainFrame().Navigate(typeof(LinePage), changeLineParameter);
        }

        private async void LineSelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            var gridView = sender as GridView;
            if (gridView.SelectedIndex == -1)
                return;

            var line = gridView.SelectedItem as Line;
            line = await FillLineSchedulesAsync(line);

            await ShowLinePageBySchedulesAsync(line, line.GridObjectInLinesList, ScheduleClickedAsync);

            ResetClickedGrids();
        }

        private async Task<Line> FillLineSchedulesAsync(Line line)
        {
            if (line.Schedules == null)
                line.Schedules = await GetLineSchedules(line);

            return line;
        }

        public static async void ScheduleClickedAsync(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            Line selectedLine = (Line)(clickedButton.DataContext);

            Schedule selectedSchedule = selectedLine.Schedules.First(p => p.Name == (string)clickedButton.Content);

            await ShowLinePageAsync(
                new ChangeLineParameter()
                {
                    Line = selectedLine,
                    SelectedSchedule = selectedSchedule
                });
        }

        private static async Task<List<Schedule>> GetLineSchedules(Line line)
        {
            string query = $"SELECT * FROM Schedule WHERE idOfLine = {line.Id};";
            return await SQLServices.QueryTimetableAsync<Schedule>(query);
        }

        private void ResetClickedGrids()
        {
            foreach (var tuple in _LinesGridViews)
                tuple.Item2.SelectedIndex = -1;
        }

        private void SearchLineAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _SearchLinesGrids.Clear();
            if (sender.Text.Trim().Length == 0)
                return;

            Timetable.Instance.Lines.Where(p => p.EditedName.StartsWith(sender.Text)).ToList().ForEach(p =>
            {
                var searchLineGrid = new Grid()
                {
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(5),
                    Background = new SolidColorBrush(Color.FromArgb(255, 55, 58, 69)),
                    DataContext = p
                };

                searchLineGrid.Children.Add(new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White),
                    Text = p.EditedName
                });

                if (p.IsFavourite)
                {
                    searchLineGrid.Children.Add(new TextBlock()
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Foreground = new SolidColorBrush(Colors.Yellow),
                        FontSize = 10,
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        Text = "\xE00B"
                    });
                }

                _SearchLinesGrids.Add(searchLineGrid);
            });
        }

        private void HideLoadingStackPanel()
            => LoadingStackPanel.Visibility = Visibility.Collapsed;

        private void ShowSearchLineAutoSuggestBox()
            => SearchLineAutoSuggestBox.Visibility = Visibility.Visible;
    }
}
