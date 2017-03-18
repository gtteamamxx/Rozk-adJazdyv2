using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using RozkładJazdyv2.Model;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Text;
using Windows.UI.Core;

namespace RozkładJazdyv2
{
    public sealed partial class MainPage : Page
    {
        private static readonly Color _INFOSTACKPANEL_TEXTCOLOR = Colors.White;
        private string APP_VERSION { get { return Model.Application.Version.VERSION; } }

        public enum InfoStackPanelTextIndex
        {
            App_Run,
            Loading_Timetable,
            Downloading_Timetable,
            Saving_Timetable
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

        private void ShowBackButton()
        {
            //button is showing only when user is at PC
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void RegisterBackButtonPress()
            => SystemNavigationManager.GetForCurrentView().BackRequested += Model.Application.BackButtonManager.BackButtonPressed;
        
        public MainPage()
        {
            this.InitializeComponent();
            SetPhoneStatusBarColor(Colors.White, Colors.Black);
            ShowBackButton();
            RegisterBackButtonPress();
            FadeInLogoOnStart();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeTextInInfoStackPanel(InfoStackPanelTextIndex.App_Run, "Uruchamianie aplikacji... OK");
            ShowLoadTimetableFrame();
            MainFrameHelper.SetMainFrame(this.Frame);
        }

        public void ChangeTextInInfoStackPanel(InfoStackPanelTextIndex index, string text, params object[] args)
        {
            bool editText = InfoStackPanel.Children.Count() - 1 >= (int)index;
            if (editText)
                ((TextBlock)InfoStackPanel.Children[(int)index]).Text = string.Format(text, args);
            else
                InfoStackPanel.Children.Add(new TextBlock()
                {
                    Text = string.Format(text, args),
                    FontSize = 12,
                    FontWeight = FontWeights.Light,
                    Foreground = new SolidColorBrush(_INFOSTACKPANEL_TEXTCOLOR),
                    TextAlignment = TextAlignment.Left
                });
        }

        private void ShowLoadTimetableFrame()
            => ChangePage(typeof(Pages.MainPageFrames.LoadingTimetable), this);

        public void ChangePage(Type page, object parametr)
            => ContentFrame.Navigate(page, parametr);

        private void FadeInLogoOnStart()
            => AnimationHelper.CraeteFadeInAnimation(MainGrid, 1.0);
    }
}
