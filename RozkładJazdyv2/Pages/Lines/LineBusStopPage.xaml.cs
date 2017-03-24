using RozkładJazdyv2.Model;
using RozkładJazdyv2.Model.LinesPage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class LineBusStopPage : Page
    {
        private static BusStop _SelectedBusStop;
        private static Line _SelectedLine => LinePage.ActualShowingLineParameters.Line;
        private static Schedule _SelectedSchedule => LinePage.ActualShowingLineParameters.SelectedSchedule;
        private static Track _SelectedTrack;
        private static bool _IsRefreshingPageNeeded;

        public LineBusStopPage()
        {
            this.InitializeComponent();
            this.Loaded += LineBusStopPage_LoadedAsync;
        }

        private async void LineBusStopPage_LoadedAsync(object sender, RoutedEventArgs e)
        {
            if (_IsRefreshingPageNeeded)
            {
                ClearListViewItems();
                UpdateLineHeaderInfo();
                LoadingProgressRing.IsActive = true;
                await UpdateHoursAsync();
                LoadingProgressRing.IsActive = false;
                await UpdateLettersAsync();
                _IsRefreshingPageNeeded = false;
            }
        }

        private void ClearListViewItems()
        {
            DayTypeHoursListView.Items.Clear();
            LettersListView.Items.Clear();
            AdditionalInfoListView.Items.Clear();
        }

        private void UpdateLineHeaderInfo()
        {
            LineBusStopNameTextBlock.Text = _SelectedBusStop.Name;
            LineScheduleNameTextBlock.Text = _SelectedSchedule.Name;
            LineNumberTextBlock.Text = _SelectedLine.EditedName;
            LineLogoTextBlock.Text = _SelectedLine.GetLineLogoByType();
            LineTrackNameTextBlock.Text = _SelectedTrack.Name;
        }

        private async Task UpdateHoursAsync()
        {
            if(_SelectedBusStop.Hours == null)
                _SelectedBusStop.Hours = await GetBusStopHoursAsync(_SelectedBusStop);

            if (_SelectedBusStop.Hours.Count > 0)
                await AddAllHoursToViewAsync(_SelectedBusStop.Hours);

            AddAdditionalInfoToView(_SelectedBusStop);
        }

        private void AddAdditionalInfoToView(BusStop busStop)
        {
            if(_SelectedBusStop.IsLastStopOnTrack)
                 AddLastStopInfo();
        }

        private void AddLastStopInfo()
            => AdditionalInfoListView.Items.Add(new AdditionalInfo() {
                Info = "Wybrany przystanek jest ostatnim przystankiem na danej trasie. Prezentowane godziny są godzinami przyjazdu." });
        
        private async Task AddAllHoursToViewAsync(List<Hour> hours)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                foreach (var hour in hours)
                {
                    var lineViewHour = new LineViewHour
                    {
                        Name = hour.Name,
                        Hours = Regex.Matches(hour.Hours, @"\d?\d:\d\d?[^\s]").Cast<Match>().Select(p => p.Value).ToList()
                    };
                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        DayTypeHoursListView.Items.Add(lineViewHour));
                }
            });
        }

        private async Task<List<Hour>> GetBusStopHoursAsync(BusStop busStop)
        {
            string query = $"SELECT * FROM Hour WHERE IdOfBusStop = {_SelectedBusStop.Id};";
            List<Hour> hours = await SQLServices.QueryAsync<Hour>(query);
            return hours;
        }

        private async Task UpdateLettersAsync()
        {
            var letters = await GetLettersAsync();
            letters.ForEach(p => LettersListView.Items.Add(p));
        }

        private async Task<List<Letter>> GetLettersAsync()
        {
            var lineViewHourList = DayTypeHoursListView.Items.Select(p => (LineViewHour)p);
            string query = $"SELECT * FROM Letter WHERE IdOfBusStop = {_SelectedBusStop.Id};";
            var letters = (await SQLServices.QueryAsync<Letter>(query)).GroupBy(p => p.IdOfName).Select(p => p.First()).ToList();
            return letters;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var changeBusStopParametr = e.Parameter as ChangeBusStopParametr;
            var busStop = changeBusStopParametr.BusStop;
            if (_SelectedBusStop == null)
            {
                _SelectedBusStop = busStop;
                _SelectedTrack = changeBusStopParametr.Track;
                _IsRefreshingPageNeeded = true;
            }
            else if (busStop.Id != _SelectedBusStop.Id)
            {
                _SelectedBusStop = busStop;
                _SelectedTrack = changeBusStopParametr.Track;
                _IsRefreshingPageNeeded = true;
            }
        }
    }
}