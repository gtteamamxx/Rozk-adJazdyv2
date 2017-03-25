using RozkładJazdyv2.Model;
using RozkładJazdyv2.Model.LinesPage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RozkładJazdyv2.Pages.Lines
{
    public sealed partial class BusStopUserControl : UserControl
    {
        public delegate void BusStopGrid_PointerEntered(object sender, PointerRoutedEventArgs e);
        public static event BusStopGrid_PointerEntered OnBusStopGridPointerEntered;

        public BusStopUserControl()
        {
            this.InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                if (s.DataContext != null)
                    UpdateViewLayout(s.DataContext as LineViewBusStop);
            };
        }

        private void UpdateViewLayout(LineViewBusStop lineViewBusStop)
        {
            var busStop = lineViewBusStop.BusStop;
            BusStopNameTextBlock.Text = lineViewBusStop.Name;
            // due to bug, we have to set default style firstly
            BusStopNameTextBlock.Foreground = new SolidColorBrush(Colors.White);
            BusStopNameTextBlock.FontWeight = FontWeights.Normal;
            if (busStop.IsLastStopOnTrack)
            {
                BusStopNameTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(127, 255, 0, 0));
                BusStopNameTextBlock.FontWeight = FontWeights.ExtraBold;
            }
            else if (busStop.IsVariant)
            {
                BusStopNameTextBlock.Foreground = new SolidColorBrush(Colors.DarkGray);
                BusStopNameTextBlock.FontWeight = FontWeights.ExtraLight;
            }
            else if (busStop.IsOnDemand)
                BusStopNameTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
            else if (busStop.IsBusStopZone)
                BusStopNameTextBlock.Foreground = new SolidColorBrush(Colors.Yellow);
        }

        private void LineViewBusStop_PointerEntered(object sender, PointerRoutedEventArgs e)
            => OnBusStopGridPointerEntered?.Invoke(sender, e);
    }
}
