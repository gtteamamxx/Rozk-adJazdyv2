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
            Looking_For_Timetable = 1,
            Download_Timetable = 2
        }

        public MainPage()
        {
            this.InitializeComponent();

            ShowStartApplicationInfo();
            ShowProgressRing();
            SetPhoneStatusBarColor(Colors.White, Colors.Gray);
            FadeInOnStart();
            InitSQLFile();
            HookDownloadProgressEvents();

            this.Loaded += MainPage_Loaded;
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

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ShowStartApplicationLoadedInfo();
            LoadBusTimetable();
        }

        private void HookDownloadProgressEvents()
        {
            EventHelper.OnLinesInfoDownloaded += OnLinesInfoDownloaded;
            EventHelper.OnLineDownloaded += OnLineDownloaded;
        }

        private void OnLineDownloaded(Line line, int linesCount)
        {
            double percent = ((line.Id + 1) * 100.0 / linesCount);
            DownloadTimetableProgressBar.Value = percent;
            DownloadTimetableTextBlock.Text = string.Format("Pobieranie linii: {0} [{1:00}%]", line.Name, percent);
            EditTextInInfoStackPanel(InfoStackPanelTextId.Download_Timetable, "Trwa pobieranie linii: {0} / {1}", line.Id + 1, linesCount);
        }

        private void AskToDownloadDatabaseFromInternetAsync()
        {
            ShowAskToDownloadDatabaseFromInternetInfo();
            ShowAndFadeInOnDownloadInfo();
            DownloadTimetableButton.Click += DownloadTimetableButtonClick;
        }

        private void LoadBusTimetable()
        {
            bool isTimetableLoaded = Timetable.LoadTimetableFromLocalCache();
            if (!isTimetableLoaded)
            {
                HideProgressRing();
                AskToDownloadDatabaseFromInternetAsync();
                return;
            }
            HideProgressRing();
            //todo timetable loaded
        }

        private async Task DownloadBusTimetableAsync()
        {
            ShowDownloadBusTimetableInfo();
            bool isTimetableDownloaded = await Timetable.DownloadTimetableFromInternetAsync();
            if (isTimetableDownloaded)
            {
                //todo timetable downloaded
                return;
            }

            CreateRetryDownloadInfo();
        }

        private async void DownloadTimetableButtonClick(object sender, RoutedEventArgs e)
        {
            ShowProgressRing();
            await DownloadBusTimetableAsync();
            HideProgressRing();
        }

        private void ShowProgressOfDownloadingTimetable()
        {
            DownloadTimetableProgressBar.Visibility = Visibility.Visible;
            DownloadTimetableProgressBar.Value = 0;
        }

        private void ShowTimetableNotDownloadedError()
        {
            AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.Download_Timetable,
                "Nie można pobrać rozkładu jazdy...");
            DownloadTimetableTextBlock.Text =
                "Wystąpił problem podczas pobierania. Sprawdź połączenie z internetem i spróbuj ponownie.";
            DownloadTimetableTextBlock.Visibility = Visibility.Visible;
        }

        private void ShowDownloadBusTimetableInfo()
        {
            AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.Download_Timetable,
                "Trwa pobieranie rozkładu jazdy...");
            DownloadTimetableTextBlock.Text = "Trwa pobieranie informacji o liniach...";
            HideDownloadInfo();
            ShowProgressOfDownloadingTimetable();
        }

        private void CreateRetryDownloadInfo()
        {
            ShowTimetableNotDownloadedError();
            ShowRetryDownloadButton();
            DownloadTimetableRetryButton.Click += DownloadTimetableButtonClick;
        }

        private void ShowAndFadeInOnDownloadInfo()
        {
            DownloadTimetableTextBlock.Visibility = Visibility.Visible;
            DownloadTimetableButton.Visibility = Visibility.Visible;

            AnimationHelper.CraeteFadeInAnimation(DownloadTimetableButton, 1.0);
            AnimationHelper.CraeteFadeInAnimation(DownloadTimetableTextBlock, 1.0);
        }

        private void OnLinesInfoDownloaded()
            => DownloadTimetableTextBlock.Text = "Trwa pobieranie informacji o poszczególnych liniach...";

        private void ShowStartApplicationLoadedInfo()
            => AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.Run_App_Text,
                "Trwa uruchamianie aplikacji... OK");

        private void ShowAskToDownloadDatabaseFromInternetInfo()
            => AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.Looking_For_Timetable,
                "Nie znaleziono rozkładu jazdy...");

        private void ShowLoadBusTimetableInfo()
            => AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId.Looking_For_Timetable,
                "Trwa wczytywanie rozkładu jazdy...");

        private void ShowStartApplicationInfo()
            => AddTextToInfoStackPanel("Trwa uruchamianie aplikacji...");

        private void ShowProgressRing()
            => ProgressRing.Visibility = Visibility.Visible;

        private void HideProgressRing()
            => ProgressRing.Visibility = Visibility.Collapsed;

        private void HideDownloadInfo()
            => DownloadTimetableButton.Visibility =
                DownloadTimetableRetryButton.Visibility = Visibility.Collapsed;

        private void ShowRetryDownloadButton()
            => DownloadTimetableRetryButton.Visibility = Visibility.Visible;

        private void InitSQLFile()
            => SQLServices.InitSQL();

        private void FadeInOnStart()
            => AnimationHelper.CraeteFadeInAnimation(MainGrid, 2.0);

        #region Adding Info to StackPanelInfo
        private void AddTextToInfoStackPanelOrEditIfExist(InfoStackPanelTextId index, string text, params object[] args)
        {
            bool editText = RunInfoStackPanel.Children.Count() - 1 >= (int)index;

            if (editText)
                EditTextInInfoStackPanel(index, string.Format(text, args));
            else
                AddTextToInfoStackPanel(string.Format(text, args));
        }

        private void EditTextInInfoStackPanel(InfoStackPanelTextId index, string text, params object[] args)
            => ((TextBlock)RunInfoStackPanel.Children[(int)index]).Text = args.Length == 0 ? text : string.Format(text, args);

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
