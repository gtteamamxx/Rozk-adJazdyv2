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
    public sealed partial class SavingTimetable : Page
    {
        private MainPage _MainPageInstance;

        public SavingTimetable()
        {
            this.InitializeComponent();
            HookEvents();
            this.Loaded += async (s, e) => await SaveTimetableAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
            => _MainPageInstance = (MainPage)e.Parameter;

        private void HookEvents()
        {
            EventHelper.OnSqlSavingChanged += EventHelper_OnSqlSavingChanged;
            EventHelper.OnSqlSaved += async () => await EventHelper_OnSqlSavedAsync();
            SavingRetryButton.Click += async (s, e) => await SaveTimetableAsync();
        }

        private async Task EventHelper_OnSqlSavedAsync()
        {
            SetSavingTimetableInfoVisibility(Visibility.Collapsed);
            _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.InfoStackPanelTextIndex.Saving_Timetable,
                                                            "Rozkład zapisany...");
            await Task.Delay(500);
            ShowMainMenu();
        }

        private void EventHelper_OnSqlSavingChanged(int step, int maxSteps)
        {
            double percent = ((step * 100.0) / maxSteps);
            _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.InfoStackPanelTextIndex.Saving_Timetable,
                                                            "Zapisywanie rozkładu... {0} / {1}", step, maxSteps);
            SavingInfoText.Text = string.Format("Trwa zapisywanie rozkładu [{0:00}%]", percent);
            SavingProgressBar.Value = percent;
        }

        private async Task SaveTimetableAsync()
        {
            _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.InfoStackPanelTextIndex.Saving_Timetable, 
                                                            "Zapisywanie rozkładu...");
            SavingInfoText.Text = "Trwa zapisywanie rozkładu...";
            SetSavingRetryButtonVisibility(Visibility.Collapsed);
            SetSavingTimetableInfoVisibility(Visibility.Visible);
            var isTimetableSaved = await SQLServices.SaveDatabaseAsync();
            if(!isTimetableSaved)
            {
                SetSavingTimetableInfoVisibility(Visibility.Collapsed);
                SetSavingRetryButtonVisibility(Visibility.Visible);
                _MainPageInstance.ChangeTextInInfoStackPanel(MainPage.InfoStackPanelTextIndex.Saving_Timetable,
                                                                "Bład podczas zapisywania rozkładu...");
                SavingInfoText.Text = $"Wystąpił problem podczas zapisywania rozkładu.{Environment.NewLine}Czy chcesz spróbować ponownie?";
            }
        }

        private void SetSavingTimetableInfoVisibility(Visibility vis)
        {
            SetSavingProgressBarVisibility(vis);
            SetSavingProgressRingVisibility(vis);
        }

        private void SetSavingProgressBarVisibility(Visibility vis)
            => SavingProgressBar.Visibility = vis;

        private void SetSavingProgressRingVisibility(Visibility vis)
            => SavingProgressRing.Visibility = vis;

        private void SetSavingRetryButtonVisibility(Visibility vis)
            => SavingRetryButton.Visibility = vis;

        private void ShowMainMenu()
            => MainFrameHelper.GetMainFrame().Navigate(typeof(MainMenu));
    }
}
