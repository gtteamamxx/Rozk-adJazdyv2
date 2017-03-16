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
        public enum OnfoStackPanelTextIndex
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

        public MainPage()
        {
            this.InitializeComponent();
            SetPhoneStatusBarColor(Colors.White, Colors.Gray);
            FadeInLogoOnStart();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeTextInInfoStackPanel(OnfoStackPanelTextIndex.App_Run, "Uruchamianie aplikacji... OK");
            ShowLoadTimetableFrame();
        }

        public void ChangeTextInInfoStackPanel(OnfoStackPanelTextIndex index, string text, params object[] args)
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
                    TextAlignment = TextAlignment.Left
                });
        }

        private void ShowLoadTimetableFrame()
            => ChangePage(typeof(Pages.MainPageFrames.LoadingTimetable), this);

        public void ChangePage(Type page, object parametr)
            => ContentFrame.Navigate(page, parametr);

        private void FadeInLogoOnStart()
            => AnimationHelper.CraeteFadeInAnimation(MainGrid, 2.0);

    }
}
