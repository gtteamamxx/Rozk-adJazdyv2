using RozkładJazdyv2.Model;
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
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RozkładJazdyv2.Pages.Lines
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LineBusStopPage : Page
    {
        public static BusStop SelectedBusStop;
        private static Line _SelectedLine => LinePage.ActualShowingLineParameters.Line;
        private static Schedule _SelectedSchedule => LinePage.ActualShowingLineParameters.SelectedSchedule;
        private static bool _IsRefreshingPageNeeded;

        public LineBusStopPage()
        {
            this.InitializeComponent();
            this.Loaded += LineBusStopPage_Loaded;
        }

        private void LineBusStopPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_IsRefreshingPageNeeded)
            {
                UpdateLineHeaderInfo();
                UpdateHours();
                _IsRefreshingPageNeeded = false;
            }
        }

        private void UpdateLineHeaderInfo()
        {
            LineBusStopNameTextBlock.Text = SelectedBusStop.Name;
            LineScheduleNameTextBlock.Text = _SelectedSchedule.Name;
            LineNumberTextBlock.Text = _SelectedLine.EditedName;
            LineLogoTextBlock.Text = _SelectedLine.GetLineLogoByType();
        }

        private void UpdateHours()
        {

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var busStop = e.Parameter as BusStop;
            if (SelectedBusStop == null)
            {
                SelectedBusStop = busStop;
                _IsRefreshingPageNeeded = true;
            }
            else if (busStop.Id != SelectedBusStop.Id)
            {
                SelectedBusStop = e.Parameter as BusStop;
                _IsRefreshingPageNeeded = true;
            }
        }
    }
}
