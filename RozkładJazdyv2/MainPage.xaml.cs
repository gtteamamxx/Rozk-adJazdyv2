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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using RozkładJazdyv2.Model;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Text;
using System.Threading.Tasks;

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x415

namespace RozkładJazdyv2
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        enum InfoStackPanelTextId
        {
            Run_App_Text = 0,
            Looking_For_Timetable,
            DownOrload_Timetable,
            Saving_Timetable
        }

        public MainPage()
        {
            this.InitializeComponent();
            ShowStartApplicationInfo();
            SetPhoneStatusBarColor(Colors.White, Colors.Gray);
            FadeInOnStart();
            InitSQLFile();
            HookEvents();
            this.Loaded += MainPage_LoadedAsync;
        }

        private void SetPhoneStatusBarColor(Color foregroundColor, Color backgroundColor)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar.GetForCurrentView().ForegroundColor = foregroundColor;
                StatusBar.GetForCurrentView().BackgroundOpacity = 1;
                StatusBar.GetForCurrentView().BackgroundColor = backgroundColor;
            }
        }

        private async void MainPage_LoadedAsync(object sender, RoutedEventArgs e)
        {
            ShowStartApplicationLoadedInfo();
            await LoadBusTimetableAsync();
        }

        private void HookEvents()
        {
            EventHelper.OnLinesInfoDownloaded += OnLinesInfoDownloaded;
            EventHelper.OnLineDownloaded += OnLineDownloaded;
            EventHelper.OnAllLinesDownloaded += OnAllLinesDownloaded;
            EventHelper.OnSqlSavingChanged += OnSqlSavingChanged;
            EventHelper.OnSqlSaved += OnSqlSaved;
            EventHelper.OnSqlLoadingChanged += OnSqlLoadingChanged;
            DownloadTimetableRetryButton.Click += DownloadTimetableButtonClick;
        }

        #region Events Handlers

        private void OnSqlSavingChanged(int step, int maxSteps)
            => UpdateTimetableSavingText(step, maxSteps);

        private void OnAllLinesDownloaded()
            => ShowTimetableDownloadedText();

        private void OnLineDownloaded(Line line, int linesCount)
            => UpdateLineDownloadedText(line, linesCount);

        private void OnSqlLoadingChanged(int step, int maxSteps)
            => UpdateTimetableLoadingText(step, maxSteps);

        private void OnLinesInfoDownloaded()
            => TimetableInfoText.Text = "Trwa pobieranie informacji o poszczególnych liniach...";

        private void OnSqlSaved()
        {
            ShowTimetableSavedText();
            HideButtons();
            //todo show menu
        }

        #endregion

        #region Events Updating functions

        private void ShowFoundTimetableText(int step)
        {
            if (step == 1)
                AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.Looking_For_Timetable, "Znaleziono rozkład jazdy...");
        }

        private void UpdateTimetableLoadingText(int step, int maxSteps)
        {
            ShowFoundTimetableText(step);
            double percent = ((step * 100.0) / maxSteps);
            AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.DownOrload_Timetable, "Wczytywanie rozkładu jazdy... {0} / {1}", step, maxSteps);
            TimetableInfoText.Text = string.Format("Trwa wczytywanie rozkładu [{0:00}%]", percent);
            TimetableProgressBar.Value = percent;
        }

        private void UpdateTimetableSavingText(int step, int maxSteps)
        {
            double percent = ((step * 100.0) / maxSteps);
            AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.Saving_Timetable, "Trwa zapisywanie rozkładu...");
            TimetableInfoText.Text = string.Format("Trwa zapisywanie rozkładu [{0:00}%]", percent);
            TimetableProgressBar.Value = percent;
        }

        private void ShowTimetableDownloadedText()
        {
            AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.DownOrload_Timetable, "Pobieranie rozkładu zakończone...");
            TimetableProgressBar.Value = 0;
            TimetableInfoText.Text = "Pobieranie rozkładu zakończone. Trwa zapisywanie ...";
        }

        private void UpdateLineDownloadedText(Line line, int linesCount)
        {
            double percent = ((line.Id + 1) * 100.0 / linesCount);
            TimetableProgressBar.Value = percent;
            TimetableInfoText.Text = string.Format("Pobieranie linii: {0} [{1:00}%]", line.Name, percent);
            AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.DownOrload_Timetable, "Trwa pobieranie linii: {0} / {1}", line.Id + 1, linesCount);
        }

        #endregion

        #region Loading timetable
        private async Task LoadBusTimetableAsync()
        {
            ShowTimetableProgressBar();
            ShowLookingForTimetableText();
            SetProgressRingVisibility(Visibility.Visible);
            bool isTimetableLoaded = await Timetable.LoadTimetableFromLocalCacheAsync();
            SetProgressRingVisibility(Visibility.Collapsed);
            SetTimetableProgressBarVisibility(Visibility.Collapsed);
            if (!isTimetableLoaded)
            {
                AskToDownloadDatabaseFromInternetAsync();
                return;
            }
            TimetableLoaded();
        }
        
        private void TimetableLoaded()
        {
            TimetableInfoText.Text = "Rozkład jazdy wczytany!";
            //todo

        }
        #endregion

        #region Downloading timetable

        private async Task<bool> DownloadBusTimetableAsync()
        {
            ShowDownloadBusTimetableInfo();
            bool isTimetableDownloaded = await Timetable.DownloadTimetableFromInternetAsync();
            return isTimetableDownloaded;
        }

        private async void DownloadTimetableButtonClick(object sender, RoutedEventArgs e)
        {
            SetProgressRingVisibility(Visibility.Visible);
            bool isTimetableDownloaded = await DownloadBusTimetableAsync();
            if (!isTimetableDownloaded)
                CreateRetryDownloadInfo();
            else
            {
                bool isDatabaseSaved = await SQLServices.SaveDatabaseAsync();
                if(!isDatabaseSaved)
                {
                    CreateRetryDownloadInfo();
                    ShowTimetableWasNotSavedInfo();
                }
            }      
            SetProgressRingVisibility(Visibility.Collapsed);
        }

        #endregion

        #region Showing text

        private void ShowTimetableWasNotSavedInfo()
            => AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.Saving_Timetable, "Rozkład nie został zapisany...");

        private void ShowTimetableSavedText()
            => AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.Saving_Timetable, "Rozkład jazdy zapisany...");

        private void ShowLookingForTimetableText()
            => AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.Looking_For_Timetable, "Szukanie rozkładu jady...");

        private void ShowStartApplicationLoadedInfo()
            => AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.Run_App_Text, "Trwa uruchamianie aplikacji... OK");

        private void ShowAskToDownloadDatabaseFromInternetInfo()
            => AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.Looking_For_Timetable, "Nie znaleziono rozkładu jazdy...");

        private void ShowLoadBusTimetableInfo()
            => AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.Looking_For_Timetable, "Trwa wczytywanie rozkładu jazdy...");

        private void ShowStartApplicationInfo()
            => AddTextToInfoStackPanel("Trwa uruchamianie aplikacji...");
        #endregion

        #region Setting UI Visibilities
        private void SetProgressRingVisibility(Visibility visibility)
            => ProgressRing.Visibility = visibility;

        private void SetTimetableProgressBarVisibility(Visibility visibility)
            => TimetableProgressBar.Visibility = visibility;

        private void SetRetryDownloadButtonVisibility(Visibility visibility)
            => DownloadTimetableRetryButton.Visibility = visibility;

        private void ShowTimetableProgressBar()
        {
            TimetableProgressBar.Visibility = Visibility.Visible;
            TimetableProgressBar.Value = 0;
        }

        private void SetTimetableInfoTextVisibility(Visibility visibility)
            => TimetableInfoText.Visibility = Visibility;

        private void HideButtons()
            => DownloadTimetableButton.Visibility =
                DownloadTimetableRetryButton.Visibility = Visibility.Collapsed;
        #endregion

        #region Others

        private void InitSQLFile()
            => SQLServices.InitSQL();

        private void FadeInOnStart()
            => AnimationHelper.CraeteFadeInAnimation(MainGrid, 2.0);

        private void AskToDownloadDatabaseFromInternetAsync()
        {
            ShowAskToDownloadDatabaseFromInternetInfo();
            ShowAndFadeInOnDownloadInfo();
            DownloadTimetableButton.Click += DownloadTimetableButtonClick;
        }

        private void ShowTimetableNotDownloadedError()
        {
            AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.DownOrload_Timetable,
                "Nie można pobrać rozkładu jazdy...");
            TimetableInfoText.Text =
                "Wystąpił problem podczas pobierania. Sprawdź połączenie z internetem i spróbuj ponownie.";
            SetTimetableProgressBarVisibility(Visibility.Collapsed);
            TimetableInfoText.Visibility = Visibility.Visible;
        }

        private void ShowDownloadBusTimetableInfo()
        {
            AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.DownOrload_Timetable,
                "Trwa pobieranie rozkładu jazdy...");
            TimetableInfoText.Text = "Trwa pobieranie informacji o liniach...";
            HideButtons();
            ShowTimetableProgressBar();
        }

        private void CreateRetryDownloadInfo()
        {
            ShowTimetableNotDownloadedError();
            SetRetryDownloadButtonVisibility(Visibility.Visible);
            if (CheckIfInfoExist(InfoStackPanelTextId.Saving_Timetable))
                DeleteTextInInfoStackPanel(InfoStackPanelTextId.Saving_Timetable);
        }

        private void ShowAndFadeInOnDownloadInfo()
        {
            TimetableInfoText.Visibility = Visibility.Visible;
            DownloadTimetableButton.Visibility = Visibility.Visible;
            AnimationHelper.CraeteFadeInAnimation(DownloadTimetableButton, 1.0);
            AnimationHelper.CraeteFadeInAnimation(TimetableInfoText, 1.0);
        }

        #endregion

        #region Editing info in stackpanel

        private bool CheckIfInfoExist(InfoStackPanelTextId index)
            => RunInfoStackPanel.Children.Count() -1 >= (int)index;

        private void DeleteTextInInfoStackPanel(InfoStackPanelTextId index)
            => RunInfoStackPanel.Children.RemoveAt((int)index);

        private void AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId index, string text, params object[] args)
        {
            if (CheckIfInfoExist(index))
                EditTextInInfoStackPanel(index, string.Format(text, args));
            else
                AddTextToInfoStackPanel(string.Format(text, args));
        }

        private void EditTextInInfoStackPanel(InfoStackPanelTextId index, string text, params object[] args)
            => ((TextBlock)RunInfoStackPanel.Children[(int)index]).Text = (args == null || args.Length == 0) 
                                                                            ? text : string.Format(text, args);

        private void AddTextToInfoStackPanel(string text)
            => RunInfoStackPanel.Children
                .Add(new TextBlock()
                {
                    Text = text,
                    FontSize = 12,
                    FontWeight = FontWeights.Light,
                    TextAlignment = TextAlignment.Left
                });
        #endregion
    }
}
