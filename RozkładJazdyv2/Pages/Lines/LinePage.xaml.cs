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
        private static ChangeLineParameter _ActualShowingParameters;
        private static Line _SelectedLine
        {
            get { return _ActualShowingParameters.Line; }
            set { _ActualShowingParameters.Line = value; }
        }
        private static Schedule _SelectedSchedule
        {
            get{ return _ActualShowingParameters.SelectedSchedule; }
            set { _ActualShowingParameters.SelectedSchedule = value; }
        }
        private static bool _IsRefreshingPageNeeded;
        private ObservableCollection<BusStop> _LineFirstTrackBusStops;
        private ObservableCollection<BusStop> _LineSecondTrackBusStops;
 

        public LinePage()
        {
            this.InitializeComponent();
            _LineFirstTrackBusStops = new ObservableCollection<BusStop>();
            _LineSecondTrackBusStops = new ObservableCollection<BusStop>();
            this.Loaded += LinePage_LoadedAsync;
        }

        private void SetBusStopViewAttribute(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var busStop = args.Item as BusStop;
            var textBlock = ((args.ItemContainer.ContentTemplateRoot as Grid).Children.ElementAt(0) as TextBlock);
            if (busStop.IsBusStopZone)
                textBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
            if (busStop.IsOnDemand)
                textBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            if (busStop.IsVariant)
            {
                var text = textBlock.Text;
                textBlock.Text = $"-- {text}";
                textBlock.FontWeight = FontWeights.ExtraLight;
                textBlock.FontSize = 13;
                textBlock.Foreground = new SolidColorBrush(Colors.Aqua);
            }
            args.Handled = false;
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
        }

        private async Task<Schedule> GetScheduleTracksAsync(Schedule schedule)
        {
            string query = $"SELECT * FROM Track WHERE IdOfSchedule = {schedule.Id};";
            if(schedule.Tracks == null)
                schedule.Tracks = await SQLServices.QueryAsync<Track>(query);
            if (schedule.Tracks.Count() == 1)
            {
                Grid.SetColumnSpan(LineFirstGrid, 2);
                LineSecondGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                LineSecondGrid.Visibility = Visibility.Visible;
                Grid.SetColumnSpan(LineFirstGrid, 1);
            }
            int trackNumber = 1;
            foreach(var track in schedule.Tracks)
            {
                query = $"SELECT * FROM BusStop WHERE IdOfTrack = {track.Id} AND IdOfSchedule = {track.IdOfSchedule};";
                if(track.BusStops == null)
                    track.BusStops = await SQLServices.QueryAsync<BusStop>(query);
                string trackName = $"Kierunek: {track.Name}";
                if (trackNumber++ == 1)
                {
                    LineFirstTrackName.Text = trackName;
                    LineFirstTrackProgressRing.IsActive = false;
                    foreach (var busStop in track.BusStops)
                        _LineFirstTrackBusStops.Add(busStop);
                }
                else
                {
                    LineSecondTrackProgressRing.IsActive = false;
                    LineSecondTrackName.Text = trackName;
                    foreach (var busStop in track.BusStops)
                        _LineSecondTrackBusStops.Add(busStop);
                }
            }
            return schedule;
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
            if (_ActualShowingParameters == null)
            {
                _ActualShowingParameters = changeLineParameter;
                _IsRefreshingPageNeeded = true;
            }
            else
            {
                if (changeLineParameter.Line.Id != _ActualShowingParameters.Line.Id
                    && changeLineParameter.SelectedSchedule.Id != _ActualShowingParameters.SelectedSchedule.Id)
                {
                    _ActualShowingParameters = changeLineParameter;
                    _IsRefreshingPageNeeded = true;
                }
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
