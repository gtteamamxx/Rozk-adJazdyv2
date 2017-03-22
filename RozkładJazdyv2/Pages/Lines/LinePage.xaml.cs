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
        private static ChangeLineParameter _ActualShowingLineParameters;
        private static Line _SelectedLine
        {
            get { return _ActualShowingLineParameters.Line; }
            set { _ActualShowingLineParameters.Line = value; }
        }
        private static Schedule _SelectedSchedule
        {
            get{ return _ActualShowingLineParameters.SelectedSchedule; }
            set { _ActualShowingLineParameters.SelectedSchedule = value; }
        }
        private static bool _IsRefreshingPageNeeded;
        private ObservableCollection<LineViewBusStop> _LineFirstTrackBusStops;
        private ObservableCollection<LineViewBusStop> _LineSecondTrackBusStops;

        public LinePage()
        {
            this.InitializeComponent();
            _LineFirstTrackBusStops = new ObservableCollection<LineViewBusStop>();
            _LineSecondTrackBusStops = new ObservableCollection<LineViewBusStop>();
            this.Loaded += LinePage_LoadedAsync;
        }

        private async void LinePage_LoadedAsync(object sender, RoutedEventArgs e)
        {
            if (_IsRefreshingPageNeeded == true)
                await UpdateLineInfoAsync();
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
            _SelectedSchedule = await GetScheduleTracksAsync(_SelectedSchedule);
            SetTrackGridStyle(_SelectedSchedule.Tracks);
            _SelectedSchedule = await GetTracksBusStopsAsync(_SelectedSchedule);
        }

        private async Task<Schedule> GetTracksBusStopsAsync(Schedule schedule)
        {
            string query = string.Empty;
            int trackNumber = 1;
            foreach (var track in schedule.Tracks)
            {
                query = $"SELECT * FROM BusStop WHERE IdOfTrack = {track.Id} AND IdOfSchedule = {track.IdOfSchedule};";
                if (track.BusStops == null)
                    track.BusStops = await SQLServices.QueryAsync<BusStop>(query);
                AddStopsToViewByTrack(trackNumber++, track);
                LineFirstTrackProgressRing.IsActive = false;
                LineSecondTrackProgressRing.IsActive = false;
            }
            return schedule;
        }

        private async Task<Schedule> GetScheduleTracksAsync(Schedule schedule)
        {
            string query = $"SELECT * FROM Track WHERE IdOfSchedule = {schedule.Id};";
            if(schedule.Tracks == null)
                schedule.Tracks = await SQLServices.QueryAsync<Track>(query);
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
            string editedName = GetBusStopEditedName(busStop);
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

        private string GetBusStopEditedName(BusStop busStop)
        {
            string editedName = string.Empty;
            if (busStop.IsVariant)
                editedName = $"-- {busStop.Name}";
            else if (busStop.IsBusStopZone)
                editedName = $"[S] {busStop.Name}";
            else
                editedName = busStop.Name;
            return editedName;
        }

        private void SetBusStopViewAttribute(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var busStop = (args.Item as LineViewBusStop).BusStop;
            var textBlock = ((Grid)args.ItemContainer.ContentTemplateRoot).Children[0] as TextBlock;
            // due to bug, we have to set default style firstly
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            textBlock.FontWeight = FontWeights.Normal;
            if (busStop.IsLastStopOnTrack)
            {
                textBlock.Foreground = new SolidColorBrush(Color.FromArgb(127, 255, 0, 0));
                textBlock.FontWeight = FontWeights.ExtraBold;
            }
            else if (busStop.IsVariant)
            {
                textBlock.Foreground = new SolidColorBrush(Colors.DarkGray);
                textBlock.FontWeight = FontWeights.ExtraLight;
            }
            if (busStop.IsOnDemand)
                textBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
            if (busStop.IsBusStopZone)
                textBlock.Foreground = new SolidColorBrush(Colors.Yellow);
        }

        private void UpdateLineHeaderTexts()
        {
            LineScheduleNameTextBlock.Text = _SelectedSchedule.Name;
            LineNumberTextBlock.Text = _SelectedLine.EditedName;
            LineLogoTextBlock.Text = GetLineLogoByType(_SelectedLine.Type);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var changeLineParameter = e.Parameter as ChangeLineParameter;
            if (_ActualShowingLineParameters == null)
            {
                _ActualShowingLineParameters = changeLineParameter;
                _IsRefreshingPageNeeded = true;
                return;
            }
            if (changeLineParameter.Line.Id != _ActualShowingLineParameters.Line.Id
                && changeLineParameter.SelectedSchedule.Id != _ActualShowingLineParameters.SelectedSchedule.Id)
            {
                _ActualShowingLineParameters = changeLineParameter;
                _IsRefreshingPageNeeded = true;
            }
        }

        private string GetLineLogoByType(int type)
        {
            if ((type & Line.BIG_BUS_BIT) > 0)
                return "\xE806";
            if ((type & Line.TRAM_BITS) > 0)
                return "\xEB4D";
            if ((type & Line.AIRPORT_BIT) > 0)
                return "\xEB4C";
            if ((type & Line.TRAIN_BIT) > 0)
                return "\xE7C0";
            return "\xE806";
        }
    }
}
