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
    public sealed partial class LineBusStopPage : Page
    {
        private static BusStop _SelectedBusStop;
        private static Line _SelectedLine => LinePage.ActualShowingLineParameters.Line;
        private static Schedule _SelectedSchedule => LinePage.ActualShowingLineParameters.SelectedSchedule;
        private static Track _SelectedTrack;

        private static bool _IsRefreshingPageNeeded;

        private int[] _LastUpdatedHourData = new int[3] { 0, 0, 0 }; // hour, minute, state

        private object _LastShowedHourItemSource;
        private ObservableCollection<LineViewBusStop> _ListOfBusStopsInTrack;

        public LineBusStopPage()
        {
            this.InitializeComponent();
            this.SetIsBackFromPageAllowed(true);

            _ListOfBusStopsInTrack = new ObservableCollection<LineViewBusStop>();

            this.Loaded += LineBusStopPage_LoadedAsync;
        }

        private async void LineBusStopPage_LoadedAsync(object sender, RoutedEventArgs e)
        {
            await Task.Delay(100);
            if (_IsRefreshingPageNeeded)
            {
                _IsRefreshingPageNeeded = false;

                ClearListViewItems();

                UpdateLineHeaderInfo();

                LoadingProgressRing.IsActive = true;
                await UpdateHoursAsync();
                LoadingProgressRing.IsActive = false;

                await UpdateLettersAsync();
            }
        }

        private void ClearListViewItems()
        {
            DayTypeHoursListView.Items.Clear();
            LettersListView.Items.Clear();
            AdditionalInfoListView.Items.Clear();
            _ListOfBusStopsInTrack.Clear();
        }

        private void UpdateLineHeaderInfo()
        {
            LineScheduleNameTextBlock.Text = _SelectedSchedule.Name;
            LineNumberTextBlock.Text = _SelectedLine.EditedName;
            LineLogoTextBlock.Text = _SelectedLine.GetLineLogoByType();
            LineTrackNameTextBlock.Text = _SelectedTrack.Name;

            UpdateStopsInComboBox();
        }

        private void UpdateStopsInComboBox()
        {
            foreach (var busStop in _SelectedTrack.BusStops)
            {
                string editedName = busStop.GetBusStopEditedName();

                LineViewBusStop editedBusStop = new LineViewBusStop()
                {
                    Name = editedName,
                    BusStop = busStop
                };

                _ListOfBusStopsInTrack.Add(editedBusStop);

                if (busStop.Id == _SelectedBusStop.Id)
                    BusStopsComboBox.SelectedItem = editedBusStop;
            }
        }

        private async Task UpdateHoursAsync()
        {
            if (_SelectedBusStop.Hours == null)
                _SelectedBusStop.Hours = await GetBusStopHoursAsync(_SelectedBusStop);

            if (_SelectedBusStop.Hours.Count > 0)
                await AddAllHoursToViewAsync(_SelectedBusStop.Hours);

            AddAdditionalInfoToView(_SelectedBusStop);
        }

        private void AddAdditionalInfoToView(BusStop busStop)
        {
            if (_SelectedBusStop.IsLastStopOnTrack)
                AddLastStopInfo();

            AddTicketInfo();
        }

        private async Task AddAllHoursToViewAsync(List<Hour> hours)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                foreach (var hour in hours)
                {
                    LineViewHour lineViewHour = new LineViewHour
                    {
                        Name = hour.Name,
                        Hours = Regex.Matches(hour.Hours, @"\d?\d:\d\d?[^\s]").Cast<Match>().Select(p => p.Value).ToList()
                    };

                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        DayTypeHoursListView.Items.Add(lineViewHour));

                    _LastUpdatedHourData = new int[3] { 0, 0, 0 };
                }
            });
        }

        private async Task<List<Hour>> GetBusStopHoursAsync(BusStop busStop)
        {
            string query = $"SELECT * FROM Hour WHERE IdOfBusStop = {_SelectedBusStop.Id};";
            List<Hour> hours = await SQLServices.QueryTimetableAsync<Hour>(query);
            return hours;
        }

        private async Task UpdateLettersAsync()
        {
            var letters = await GetLettersAsync();
            letters.ForEach(p => LettersListView.Items.Add(p));
        }

        private async Task<List<Letter>> GetLettersAsync()
        {
            IEnumerable<LineViewHour> lineViewHourList = DayTypeHoursListView.Items.Select(p => (LineViewHour)p);

            string query = $"SELECT * FROM Letter WHERE IdOfBusStop = {_SelectedBusStop.Id};";
            List<Letter> letters = (await SQLServices.QueryTimetableAsync<Letter>(query)).GroupBy(p => p.IdOfName).Select(p => p.First()).ToList();

            return letters;
        }

        private void HourGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var hourString = ((string)args.Item);
            var hourTextBlock = (TextBlock)args.ItemContainer.ContentTemplateRoot;
            var hourGridViewItem = (GridViewItem)args.ItemContainer;

            SetHourDefaultStyle(hourTextBlock, hourGridViewItem);

            bool isHourWithLetter = !Char.IsDigit(hourString.Last());
            if (isHourWithLetter)
            {
                hourTextBlock.Foreground = new SolidColorBrush(Colors.Wheat);
                hourString = hourString.Remove(hourString.Length - 1, 1);
            }

            AddBackgroudEffectToHour(sender, hourString, hourGridViewItem, hourTextBlock);
        }

        private void SetHourDefaultStyle(TextBlock hourTextBlock, GridViewItem hourGridViewItem)
        {
            hourTextBlock.Foreground = new SolidColorBrush(Colors.White);
            hourGridViewItem.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void AddBackgroudEffectToHour(ListViewBase sender, string hourString, GridViewItem hourGrid, TextBlock hourTextBlock)
        {
            if (_LastShowedHourItemSource != sender.ItemsSource)
                _LastUpdatedHourData = new int[3] { 0, 0, 0 };

            int hour = 0, minute = 0, checkHour = 0, checkMinute = 0;

            bool isFirstHourToCheck = _LastUpdatedHourData[2] == 0;
            bool isCheckingHourBackgroundAvailable = _LastUpdatedHourData[2] <= 1;

            SplitHourStringToHourAndMinute(hourString, ref hour, ref minute);
            GetHourAndMinuteToCompareWith(isFirstHourToCheck, ref checkHour, ref checkMinute);

            CheckIfBackgroundEffectIsAvailable(sender, isCheckingHourBackgroundAvailable, isFirstHourToCheck,
                hour, checkHour, minute, checkMinute, hourTextBlock, hourGrid);
        }

        private void CheckIfBackgroundEffectIsAvailable(ListViewBase sender, bool isCheckingHourBackgroundAvailable,
            bool isFirstHourToCheck, int hour, int checkHour, int minute,
                int checkMinute, TextBlock hourTextBlock, GridViewItem hourGrid)
        {
            if (isCheckingHourBackgroundAvailable)
                if (hour > checkHour || (hour == checkHour && minute >= checkMinute))
                    AddEffectToLHour(sender, isFirstHourToCheck, hourTextBlock, hourGrid);
        }

        private void AddEffectToLHour(ListViewBase sender, bool isFirstHourToCheck, TextBlock hourTextBlock,
                                        GridViewItem hourGrid)
        {
            hourTextBlock.Foreground = new SolidColorBrush(Colors.White);
            hourGrid.Background = new SolidColorBrush(isFirstHourToCheck
                ? Color.FromArgb(100, 255, 0, 0) : Color.FromArgb(100, 255, 255, 0));

            _LastUpdatedHourData = new int[3] { 0, 0, isFirstHourToCheck ? 1 : 2 };
            _LastShowedHourItemSource = sender.ItemsSource;
        }

        private void GetHourAndMinuteToCompareWith(bool isFirstHourToCheck, ref int checkHour, ref int checkMinute)
        {
            checkHour = isFirstHourToCheck ? DateTime.Now.Hour : _LastUpdatedHourData[0];
            checkMinute = isFirstHourToCheck ? DateTime.Now.Minute : _LastUpdatedHourData[1];
        }

        private void SplitHourStringToHourAndMinute(string hourString, ref int hour, ref int minute)
        {
            string[] splittedHour = hourString.Split(":".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            hour = int.Parse(splittedHour[0]);
            minute = int.Parse(splittedHour[1]);
        }

        private void BusStopsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = BusStopsComboBox.SelectedItem as LineViewBusStop;
            if (selectedItem == null || selectedItem.BusStop.Id == _SelectedBusStop.Id)
                return;

            MainFrameHelper.GetMainFrame().Navigate(typeof(LineBusStopPage), new ChangeBusStopParametr()
            {
                BusStop = selectedItem.BusStop,
                Track = _SelectedTrack
            });
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
                bool isRefreshPage = false;
                _SelectedBusStop = busStop;

                if (_SelectedTrack.Id == changeBusStopParametr.Track.Id)
                    isRefreshPage = true;

                _SelectedTrack = changeBusStopParametr.Track;
                _IsRefreshingPageNeeded = true;

                if (isRefreshPage)
                    RefreshPage();
            }
        }


        private void AddLastStopInfo()
            => AdditionalInfoListView.Items.Add(new AdditionalInfo()
            {
                Info = "Wybrany przystanek jest ostatnim przystankiem na danej trasie. Prezentowane godziny są godzinami przyjazdu."
            });

        private void AddTicketInfo()
            => AdditionalInfoListView.Items.Add(new AdditionalInfo()
            {
                Info = "Na linii obowiązuje wsiadanie pierwszymi drzwami. Proszę okazać kierowcy ważny bilet lub dokument uprawniający do przejazdu."
            });

        private void RefreshPage()
            => LineBusStopPage_LoadedAsync(this, null);
    }
}