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

namespace RozkładJazdyv2.Pages.MainPageFrames
{
    public sealed partial class LoadingTimetable : Page
    {
        private MainPage _MainPageInstance;

        public LoadingTimetable()
        {
            this.InitializeComponent();
            RegisterHooks();
            InitSQLFile();
            this.Loaded += LoadingTimetable_Loaded;
        }

        private void InitSQLFile()
            => SQLServices.InitSQL();

        protected override void OnNavigatedTo(NavigationEventArgs e)
            => _MainPageInstance = e.Parameter as MainPage;

        private void RegisterHooks()
            => EventHelper.OnSqlLoadingChanged += EventHelper_OnSqlLoadingChanged;

        private void EventHelper_OnSqlLoadingChanged(int step, int maxSteps)
        {
            double percent = ((step * 100.0) / maxSteps);

            _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.InfoStackPanelTextIndex.Loading_Timetable, "Wczytywanie rozkładu jazdy... {0} / {1}", step, maxSteps);
            LoadingInfoText.Text = string.Format("Trwa wczytywanie rozkładu [{0:00}%]", percent);

            LoadingProgressBar.Value = percent;
        }

        private async void LoadingTimetable_Loaded(object sender, RoutedEventArgs e)
        {
            _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.InfoStackPanelTextIndex.Loading_Timetable, "Wczytywanie rozkładu...");
            await LoadTimetableAsync();
        }

        private async Task LoadTimetableAsync()
        {
            bool isTimetableLoaded = await Timetable.LoadTimetableFromLocalCacheAsync();
            if (!isTimetableLoaded)
            {
                _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.InfoStackPanelTextIndex.Loading_Timetable, "Błąd podczas wczytywania rozkładu...");
                _MainPageInstance.ChangePage(typeof(DownloadingTimetable), _MainPageInstance);
                return;
            }

            TimetableLoaded();

            await Task.Delay(500);
            ShowMainMenu();
        }

        private void TimetableLoaded()
        {
            LoadingProgressRing.Visibility = Visibility.Collapsed;
            LoadingProgressBar.Value = 100;

            LoadingInfoText.Text = "Rozkład wczytany...";
            _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.InfoStackPanelTextIndex.Loading_Timetable, "Rozkład wczytany...");
        }

        private void ShowMainMenu()
            => MainFrameHelper.GetMainFrame().Navigate(typeof(MainMenu));
    }
}
