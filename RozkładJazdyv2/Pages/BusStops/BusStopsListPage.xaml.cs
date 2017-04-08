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
using RozkładJazdyv2.Model.BusStopListPage;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RozkładJazdyv2.Model.LinesPage;
using Windows.ApplicationModel.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RozkładJazdyv2.Pages.BusStops
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BusStopsListPage : Page
    {
        private ObservableCollection<BusStopName> _ListOfBusStopNames;
        private ObservableCollection<BusStopDependency> _BusStopDependencies;
        private Dictionary<BusStopName, Grid> _BusStopNamesDict;

        private bool _LoadingDependencyLines;
        private BusStopName _LastClickedBusStop;

        private BusStopName _SelectedBusStop;
        
        public BusStopsListPage()
        {
            this.InitializeComponent();
            this.SetIsBackFromPageAllowed(true);

            _BusStopDependencies = new ObservableCollection<BusStopDependency>();
            _BusStopNamesDict = new Dictionary<BusStopName, Grid>();
            _ListOfBusStopNames = Timetable.Instance.BusStopsNames.OrderBy(p => p.Name).ToObservableCollection();

            this.Loaded += BusStopsListPage_Loaded;
        }

        private void BusStopsListPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_SelectedBusStop != null)
            {
                _ListOfBusStopNames = Timetable.Instance.BusStopsNames.OrderBy(p => p.Name).ToObservableCollection();
                BusStopsListView.SelectionChanged += BusStopsListView_SelectionChanged1;
                BusStopsListView.SelectedItem = _SelectedBusStop;
                _SelectedBusStop = null;
            }

            async void BusStopsListView_SelectionChanged1(object s, SelectionChangedEventArgs f)
            {
                BusStopsListView.SelectionChanged -= BusStopsListView_SelectionChanged1;
                await Task.Delay(150);
                BusStopsListView.ScrollIntoView(BusStopsListView.SelectedItem);
            }
        }

        private async void BusStopsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedBusStopName = BusStopsListView.SelectedItem as BusStopName;
            if (selectedBusStopName == null || selectedBusStopName == _LastClickedBusStop)
                return;

            if (_LoadingDependencyLines && selectedBusStopName != _LastClickedBusStop)
            {
                BusStopsListView.SelectedItem = _LastClickedBusStop;
                return;
            }

            _LastClickedBusStop = selectedBusStopName;
            SetLoadingStatus(true);

            _BusStopDependencies.Clear();

            await AddDependencyLinesToView(selectedBusStopName.Id);

            SetLoadingStatus(false);
        }

        private async Task AddDependencyLinesToView(int busStopId)
        {
            List<BusStop> selectedBusStopsList = await Task.Run( () => SQLServices.QueryTimetable<BusStop>($"SELECT * FROM BusStop WHERE IdOfName = {busStopId};"));

            foreach (BusStop busStop in selectedBusStopsList)
            {
                Line line = Timetable.Instance.Lines.First(p => p.Id == busStop.IdOfLine);
                Schedule schedule = SQLServices.QueryTimetable<Schedule>($"SELECT * FROM Schedule WHERE Id = {busStop.IdOfSchedule};").First();
                Track track = SQLServices.QueryTimetable<Track>($"SELECT * FROM Track WHERE Id = {busStop.IdOfTrack};").First();

                track.BusStops = new List<BusStop>().Add<BusStop>(busStop);
                schedule.Tracks = new List<Track>().Add<Track>(track);

                await AddDependencyLineToList(line, schedule, track);
            }
        }

        private async Task AddDependencyLineToList(Line line, Schedule schedule, Track track)
        {
            BusStopDependency tempDependency = null;
            if ((tempDependency = _BusStopDependencies.FirstOrDefault(p => p.Line.Id == line.Id)) == null)
            {
                Line dependencyLine = new Line(lockUpdateFavouriteSqlStatus: true)
                {
                    Id = line.Id,
                    Name = line.EditedName,
                    Type = line.Type,
                    IsFavourite = line.IsFavourite
                };

                dependencyLine.Schedules = new List<Schedule>().Add<Schedule>(schedule);
                BusStopDependency busStopDependency = new BusStopDependency()
                {
                    Line = dependencyLine
                };
                await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, 
                    () => _BusStopDependencies.Add(busStopDependency));
                return;
            }

            Schedule tempSchedule = null;
            if ((tempSchedule = tempDependency.Line.Schedules.FirstOrDefault(p => p.Id == schedule.Id)) == null)
            {
                tempDependency.Line.Schedules.Add(schedule);
                return;
            }
            tempSchedule.Tracks.Add(track);
        }

        private void SearchAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _BusStopDependencies.Clear();
            _ListOfBusStopNames = Timetable.Instance.BusStopsNames
                                                    .Where(p => p.Name.ToLower().Contains(sender.Text.ToLower()))
                                                    .OrderBy(p => p.Name)
                                                    .ToObservableCollection<BusStopName>();
            BusStopsListView.ItemsSource = _ListOfBusStopNames;
        }

        private void SetLoadingStatus(bool status)
        {
            _LoadingDependencyLines = status;
            LoadingBusStopDependenciesProgressRing.IsActive = status;
        }

        private async void Line_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Line selectedLine = Timetable.Instance.Lines.First(p => 
                p.Id == ((BusStopDependency)(button.DataContext)).Line.Id);
            await selectedLine.GetSchedules();
            RozkładJazdyv2.Model.LinesPage.FlyoutHelper.ShowFlyOutWithSchedulesAtLineGrid
                ((Grid)button.Parent, selectedLine, RozkładJazdyv2.Pages.Lines.LinesListPage.ScheduleClickedAsync);
        }

        private async void Track_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView.SelectedIndex == -1)
                return;
            
            Track selectedTrackDependency = listView.SelectedItem as Track;
            Line selectedLine = Timetable.Instance.Lines.First(p => p.Id == selectedTrackDependency.IdOfLine);
            await selectedLine.GetSchedules();

            Schedule selectedSchedule = selectedLine.Schedules.First(p => p.Id == selectedTrackDependency.IdOfSchedule);
            await selectedSchedule.GetTracks();

            Track selectedTrack = selectedSchedule.Tracks.First(p => p.Id == selectedTrackDependency.Id);
            await selectedTrack.GetBusStops();

            ShowBusStopPage(selectedLine, selectedTrack, selectedSchedule);
            listView.SelectedIndex = -1;
        }

        private void ShowBusStopPage(Line line, Track track, Schedule schedule)
        {
            Model.LinesPage.ChangeBusStopParametr pageBusStopParametr = new Model.LinesPage.ChangeBusStopParametr()
            {
                BusStop = track.BusStops.First(p => p.IdOfName == _LastClickedBusStop.Id),
                Track = track,
                Line = line,
                Schedule = schedule
            };

            MainFrameHelper.GetMainFrame().Navigate(typeof(Pages.Lines.LineBusStopPage), pageBusStopParametr);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var changeBusStopParametr = e.Parameter as Model.BusStopListPage.ChangeBusStopParametr;
            if (changeBusStopParametr == null)
                return;
            _SelectedBusStop = changeBusStopParametr.BusStopName;
        }

        private void BusStopMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            BusStopName busStop = (sender as MenuFlyoutItem).DataContext as BusStopName;
            busStop.IsFavourite = !busStop.IsFavourite;
            Grid grid = _BusStopNamesDict.First(p => p.Key == busStop).Value;
            ChangeBusStopFavouriteSign(
                grid.Children.ElementAt(1) as TextBlock,
                busStop.FavouriteText); 
        }

        private void BusStopMenuFlyoutItem_Loading(FrameworkElement sender, object args)
        {
            var textBlock = (sender as MenuFlyoutItem);
            BusStopName busStop = textBlock.DataContext as BusStopName;
            textBlock.Text = busStop.IsFavourite ? "Usuń z ulubionych" : "Dodaj do ulubionych";
        }

        private void ChangeBusStopFavouriteSign(TextBlock textBlock, string value)
            => textBlock.Text = value;

        private void BusStopGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
            => ShowAttachedFlyout(sender);

        private void BusStopGrid_Holding(object sender, HoldingRoutedEventArgs e)
            => ShowAttachedFlyout(sender);

        private void ShowAttachedFlyout(object sender)
            => FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

        private void BusStopsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            BusStopName busStopName = args.Item as BusStopName;
            if (_BusStopNamesDict.Any(p => p.Key == busStopName))
                return;
            Grid grid = (args.ItemContainer as ListViewItem).ContentTemplateRoot as Grid;
            _BusStopNamesDict.Add(busStopName, grid);
        }
    }
}
