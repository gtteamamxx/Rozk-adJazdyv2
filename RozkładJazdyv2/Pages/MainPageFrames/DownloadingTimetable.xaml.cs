using RozkładJazdyv2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=234238

namespace RozkładJazdyv2.Pages.MainPageFrames
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class DownloadingTimetable : Page
    {
        private MainPage _MainPageInstance;

        public DownloadingTimetable()
        {
            this.InitializeComponent();
            HookEvents();
            this.Loaded += DownloadingTimetable_Loaded;
        }

        private void HookEvents()
        {
            EventHelper.OnLinesInfoDownloaded += () => DownloadingInfoText.Text = 
                                                     $"Pobieranie informacji o liniach zakończone...{Environment.NewLine}Pobieranie poszczególnych linii...";
            EventHelper.OnLineDownloaded += EventHelper_OnLineDownloaded;
            EventHelper.OnAllLinesDownloaded += EventHelper_OnAllLinesDownloaded;
            DownloadButton.Click += async (s, e) => await DownloadTimetableAsync();
        }

        private void DownloadingTimetable_Loaded(object sender, RoutedEventArgs e)
            => AskUserToDownloadTimetable();

        protected override void OnNavigatedTo(NavigationEventArgs e)
            => _MainPageInstance = e.Parameter as MainPage;

        private void EventHelper_OnAllLinesDownloaded()
        {
            _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.OnfoStackPanelTextIndex.Downloading_Timetable, 
                                                            "Pobieranie rozkładu zakończone...");
            DownloadingInfoText.Text = "Pobieranie rozkładu zakończone. Trwa zapisywanie ...";
            _MainPageInstance.ChangePage(typeof(SavingTimetable), _MainPageInstance);
        }

        private void EventHelper_OnLineDownloaded(Line line, int linesCount)
        {
            double percent = ((line.Id + 1) * 100.0 / linesCount);
            DownloadingProgressBar.Value = percent;
            var busStopsCount = (line.Schedules.SelectMany(p => p.Tracks.SelectMany(f => f.BusStops))).Count();
            DownloadingInfoText.Text = string.Format("Pobieranie linii: {0} [{1:00}%]{2}Przystanków: {3}", line.Name, percent,
                                                        Environment.NewLine, busStopsCount);
            _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.OnfoStackPanelTextIndex.Downloading_Timetable, 
                                                            "Trwa pobieranie linii: {0} / {1}", line.Id + 1, linesCount);
        }

        private void AskUserToDownloadTimetable()
        {
            SetProgressRingVisiblity(Visibility.Collapsed);
            SetDownloadButtonVisibility(Visibility.Visible);
            DownloadingInfoText.Text = $"Wymagany jest rozkład offine.{Environment.NewLine}Czy chcesz go teraz pobrać?";
        }

        private async Task DownloadTimetableAsync()
        {
            SetDownloadInfoVisibility(Visibility.Visible);
            SetDownloadButtonVisibility(Visibility.Collapsed);
            DownloadingInfoText.Text = "Trwa pobieranie informacji o liniach...";
            _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.OnfoStackPanelTextIndex.Downloading_Timetable, "Pobieranie linii...");
            bool isTimetableDownloaded = await Timetable.DownloadTimetableFromInternetAsync();
            SetDownloadInfoVisibility(Visibility.Collapsed);
            if (!isTimetableDownloaded)
            {
                _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.OnfoStackPanelTextIndex.Downloading_Timetable,
                                                                "Błąd podczas pobierania linii...");
                DownloadingInfoText.Text = $"Wystąpil problem podczas pobierania linii.{Environment.NewLine}Sprawdź połączenie z internetem i spróbuj ponownie.";
                SetDownloadButtonVisibility(Visibility.Visible);
            }
        }

        private void SetDownloadInfoVisibility(Visibility vis)
        {
            SetProgressBarVisiblity(vis);
            SetProgressRingVisiblity(vis);
        }

        private void SetProgressBarVisiblity(Visibility vis)
            => DownloadingProgressBar.Visibility = vis;

        private void SetProgressRingVisiblity(Visibility vis)
            => DownloadingProgressRing.Visibility = vis;

        private void SetDownloadButtonVisibility(Visibility vis)
            => DownloadButton.Visibility = vis;
    }
}
