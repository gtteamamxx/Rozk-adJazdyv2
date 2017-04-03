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
using Windows.UI.Text;
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
    public sealed partial class LinePage : Page
    {
        public static ChangeLineParameter ActualShowingLineParameters;

        private static Line _SelectedLine
        {
            get => ActualShowingLineParameters.Line;
            set => ActualShowingLineParameters.Line = value;
        }
        private static Schedule _SelectedSchedule
        {
            get => ActualShowingLineParameters.SelectedSchedule;
            set => ActualShowingLineParameters.SelectedSchedule = value;
        }

        private static bool _IsRefreshingPageNeeded;
        private static bool _IsOppositeBusStopSelecting;

        private static List<ListView> _ListViewsList;

        private ObservableCollection<LineViewBusStop> _LineFirstTrackBusStops;
        private ObservableCollection<LineViewBusStop> _LineSecondTrackBusStops;

        private Flyout _LastOpenedFlyout;

        public LinePage()
        {
            this.InitializeComponent();
            this.SetIsBackFromPageAllowed(true);

            _LineFirstTrackBusStops = new ObservableCollection<LineViewBusStop>();
            _LineSecondTrackBusStops = new ObservableCollection<LineViewBusStop>();

            _ListViewsList = 
                new List<ListView>().Add<ListView>(LineFirstTrackListView)
                                    .Add<ListView>(LineSecondTrackListView);

            HookEvents();
            this.Loaded += LinePage_LoadedAsync;
        }

        private async Task ShowLinePageAsync(ChangeLineParameter changeLineParameter)
        {
            await Task.Delay(100);
            MainFrameHelper.GetMainFrame().Navigate(typeof(LinePage), changeLineParameter);
        }

        private async void LinePage_LoadedAsync(object sender, RoutedEventArgs e)
        {
            if (_IsRefreshingPageNeeded == true)
                await UpdateLineInfoAsync();
        }

        private void LineScheduleNameButton_Click(object sender, RoutedEventArgs e)
        {
            if (_SelectedLine.Schedules.Count() > 1)
                _LastOpenedFlyout = FlyoutHelper.ShowFlyOutWithLSchedulesAtButtonInLinePage(sender as Button, _SelectedLine, ScheduleClickedAsync);
        }

        private async void ScheduleClickedAsync(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            Schedule selectedSchedule = _SelectedLine.Schedules.First(p => p.Name == (string)clickedButton.Content);

            await ShowLinePageAsync(new ChangeLineParameter() { Line = _SelectedLine, SelectedSchedule = selectedSchedule });

            HideFlyOutOnScheduleButton();
        }

        private void HideFlyOutOnScheduleButton()
        {
            if (_LastOpenedFlyout != null)
            {
                _LastOpenedFlyout.Hide();
                _LastOpenedFlyout = null;
            }
        }

        private async Task UpdateLineInfoAsync()
        {
            UpdateLineHeaderTexts();
            await UpdateLineTracksAsync();

            _IsRefreshingPageNeeded = false;
        }

        private async Task UpdateLineTracksAsync()
        {
            LineFirstTrackProgressRing.IsActive = true;
            LineSecondTrackProgressRing.IsActive = true;

            _LineFirstTrackBusStops.Clear();
            _LineSecondTrackBusStops.Clear();

            await _SelectedSchedule.GetTracks();
            SetTrackGridStyle(_SelectedSchedule.Tracks);

            _SelectedSchedule = await GetTracksBusStopsAsync(_SelectedSchedule);
        }

        private void UpdateLineHeaderTexts()
        {
            LineScheduleNameTextBlock.Text = _SelectedSchedule.Name;
            LineNumberTextBlock.Text = _SelectedLine.EditedName;
            LineLogoTextBlock.Text = _SelectedLine.GetLineLogoByType();

            UpdateFavouriteText();
        }

        private void UpdateFavouriteText()
        {
            LineFavouriteButtonContentTextBlock.Text = _SelectedLine.IsFavourite ?
                "Usuń linię z ulubionych" : "Dodaj linię do ulubionych";
            LineFavouriteHeartSignTextBlock.Text = _SelectedLine.IsFavourite ? "\xE00C" : "\xE00B";

            LineFavouriteHeartSignTextBlock.Foreground = new SolidColorBrush(_SelectedLine.IsFavourite ?
                Colors.GreenYellow : Colors.White);
        }

        private async Task<Schedule> GetTracksBusStopsAsync(Schedule schedule)
        {
            string query = string.Empty;
            int trackNumber = 1;

            foreach (var track in schedule.Tracks)
            {
                await track.GetBusStops();

                AddStopsToViewByTrack(trackNumber++, track);

                LineFirstTrackProgressRing.IsActive = false;
                LineSecondTrackProgressRing.IsActive = false;
            }
            return schedule;
        }

        private void SetTrackGridStyle(List<Track> tracks)
        {
            if (tracks.Count() == 1)
            {
                Grid.SetColumnSpan(LineFirstGrid, 2);
                LineSecondGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                LineSecondGrid.Visibility = Visibility.Visible;
                Grid.SetColumnSpan(LineFirstGrid, 1);
            }
        }

        private void AddStopsToViewByTrack(int trackId, Track track)
        {
            string trackName = $"Kierunek: {track.Name}";

            foreach (var busStop in track.BusStops)
                AddEditedBusStopClassToTrackList(trackId, busStop);

            SetTrackName(trackId, trackName);
        }

        private void SetTrackName(int trackId, string trackName)
        {
            if (trackId == 1)
                LineFirstTrackName.Text = trackName;
            else
                LineSecondTrackName.Text = trackName;
        }

        private void AddEditedBusStopClassToTrackList(int trackId, BusStop busStop)
        {
            string editedName = busStop.GetBusStopEditedName();

            var editedBusStop = new LineViewBusStop()
            {
                Name = editedName,
                BusStop = busStop
            };

            if (trackId == 1)
                _LineFirstTrackBusStops.Add(editedBusStop);
            else
                _LineSecondTrackBusStops.Add(editedBusStop);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var changeLineParameter = e.Parameter as ChangeLineParameter;

            if (ActualShowingLineParameters == null)
            {
                ActualShowingLineParameters = changeLineParameter;
                _IsRefreshingPageNeeded = true;
                return;
            }

            if (changeLineParameter.Line.Id != ActualShowingLineParameters.Line.Id
                || changeLineParameter.SelectedSchedule.Id != ActualShowingLineParameters.SelectedSchedule.Id)
            {
                bool isRefreshPage = changeLineParameter.Line.Id == ActualShowingLineParameters.Line.Id;

                ActualShowingLineParameters = changeLineParameter;
                _IsRefreshingPageNeeded = true;

                if (isRefreshPage)
                    RefreshPage();
            }
        }

        private async void LineTrackListView_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (_IsOppositeBusStopSelecting)
            {
                listView.ScrollIntoView(listView.SelectedItem);
                return;
            }

            LineViewBusStop selectedBusStopInListView = (listView.SelectedItem as LineViewBusStop);
            if (listView.SelectedIndex == -1 || selectedBusStopInListView == null)
                return;

            var selectedTrack = listView.Name.Contains("First")
                ? _SelectedSchedule.Tracks[0] : _SelectedSchedule.Tracks[1];

            await Task.Delay(100);

            ChangePageToBusPage(selectedBusStopInListView.BusStop, selectedTrack);

            listView.SelectedItem = -1;
        }

        private void LineViewBusStop_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (_SelectedSchedule.Tracks.Count() == 1)
                return;

            LineViewBusStop lineViewBusStop = (sender as Grid).DataContext as LineViewBusStop;
            SelectOppositeBusStopByCurrentlySelected(lineViewBusStop.BusStop);
        }

        private void SelectOppositeBusStopByCurrentlySelected(BusStop selectedBusStop)
        {
            var trackId = GetTrackIdByBusStop(selectedBusStop);
            if (trackId == 0)
                return;

            var oppositeListView = _ListViewsList[trackId == 2 ? 0 : 1];
            foreach (LineViewBusStop lineViewBusStop in oppositeListView.Items)
            {
                if (selectedBusStop.Name == lineViewBusStop.BusStop.Name)
                {
                    _IsOppositeBusStopSelecting = true;
                    oppositeListView.SelectedItem = lineViewBusStop;
                    _IsOppositeBusStopSelecting = false;
                }
            }
        }

        private int GetTrackIdByBusStop(BusStop busStop)
        {
            int tracksCount = _SelectedSchedule.Tracks.Count();

            for (int i = 1; i <= tracksCount; i++)
                if (busStop.IdOfTrack == _SelectedSchedule.Tracks[i - 1].Id)
                    return i;

            return 0;
        }

        private void ChangePageToBusPage(BusStop busStop, Track track)
            => MainFrameHelper.GetMainFrame().Navigate(typeof(LineBusStopPage), new ChangeBusStopParametr()
            {
                BusStop = busStop,
                Track = track,
                Line = ActualShowingLineParameters.Line,
                Schedule = ActualShowingLineParameters.SelectedSchedule
            });

        private void LineFavouriteButton_Click(object sender, RoutedEventArgs e)
        {
            _SelectedLine.IsFavourite = !_SelectedLine.IsFavourite;
            UpdateFavouriteText();
        }

        private void RefreshPage()
            => LinePage_LoadedAsync(this, null);

        private void HookEvents()
            => HookBusStopGridPointerEntered();

        private void HookBusStopGridPointerEntered()
            => BusStopUserControl.OnBusStopGridPointerEntered += LineViewBusStop_PointerEntered;
    }
}
