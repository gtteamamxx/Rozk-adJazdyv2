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
using RozkładJazdyv2.Model;
using RozkładJazdyv2.Model.RSS;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RozkładJazdyv2.Pages.RSS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CommunicatesPage : Page
    {
        private ObservableCollection<RssItem> _Communicates;

        public CommunicatesPage()
        {
            this.InitializeComponent();
            this.SetIsBackFromPageAllowed(true);

            _Communicates = new ObservableCollection<RssItem>();

            this.Loaded += CommunicatesPage_Loaded;
        }

        private async void CommunicatesPage_Loaded(object sender, RoutedEventArgs e)
        {
            HideNoInternetInfoAndShowListView();
            SetLoadingStatus(true);

            if (!CheckInternetConnection())
            {
                ShowNoInternetInfoAndHideListView();
                SetLoadingStatus(false);
                return;
            }

            _Communicates = await new Model.Internet.RssService(Model.Internet.RssService.KZKGOP_COMMUNICATES_FEED)
                .GetCommunicatesAsync();
            CommunicatesListView.ItemsSource = _Communicates;
            SetLoadingStatus(false);
        }

        private void NoInternetButton_Click(object sender, RoutedEventArgs e)
            => CommunicatesPage_Loaded(null, null);

        private void SetLoadingStatus(bool value)
            => LoadingProgressRing.IsActive = value;

        private void HideNoInternetInfoAndShowListView()
        {
            NoInternetInfoStackPanel.Visibility = Visibility.Collapsed;
            CommunicatesListView.Visibility = Visibility.Visible;
        }

        private void ShowNoInternetInfoAndHideListView()
        {
            NoInternetInfoStackPanel.Visibility = Visibility.Visible;
            CommunicatesListView.Visibility = Visibility.Collapsed;
        }

        private bool CheckInternetConnection()
            => Model.InternetConnectionService.IsInternetConnectionAvailable();
    }
}
