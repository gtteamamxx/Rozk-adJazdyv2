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
    public sealed partial class LoadingTimetable : Page
    {
        private MainPage _MainPageInstance;

        public LoadingTimetable()
        {
            this.InitializeComponent();
            RegisterHooks();
            this.Loaded += LoadingTimetable_Loaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
            => _MainPageInstance = e.Parameter as MainPage;

        private void RegisterHooks()
            => EventHelper.OnSqlLoadingChanged += EventHelper_OnSqlLoadingChanged;

        private void EventHelper_OnSqlLoadingChanged(int step, int maxSteps)
        {
            double percent = ((step * 100.0) / maxSteps);
            _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.OnfoStackPanelTextIndex.Loading_Timetable, "Wczytywanie rozkładu jazdy... {0} / {1}", step, maxSteps);
            LoadingInfoText.Text = string.Format("Trwa wczytywanie rozkładu [{0:00}%]", percent);
            LoadingProgressBar.Value = percent;
        }

        private async void LoadingTimetable_Loaded(object sender, RoutedEventArgs e)
        {
            _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.OnfoStackPanelTextIndex.Loading_Timetable, "Wczytywanie rozkładu...");
            await LoadTimetableAsync();
        }

        private async Task LoadTimetableAsync()
        {
            bool isTimetableLoaded = await Timetable.LoadTimetableFromLocalCacheAsync();
            if (!isTimetableLoaded)
            {
                _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.OnfoStackPanelTextIndex.Loading_Timetable, "Błąd podczas wczytywania rozkładu...");
                _MainPageInstance.ChangePage(typeof(DownloadingTimetable), _MainPageInstance);
                return;
            }
            _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.OnfoStackPanelTextIndex.Loading_Timetable, "Rozkład wczytany...");
            await Task.Delay(500);
            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            //todo
        }

    }
}
