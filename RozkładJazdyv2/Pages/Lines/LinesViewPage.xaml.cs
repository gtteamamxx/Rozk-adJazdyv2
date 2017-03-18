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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RozkładJazdyv2.Pages.Lines
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LinesViewPage : Page
    {
        private List<Line> _Lines => Timetable.Instance.Lines;
        private bool _IsPageCached;

        public LinesViewPage()
        {
            this.InitializeComponent();
            this.Loaded += LinesViewPage_Loaded;
        }

        private async void LinesViewPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_IsPageCached == true)
                return;
            await LoadLinesToView();
        }

        private async Task LoadLinesToView()
        {
            Model.LinesPage.LinesViewManager.SetInstance(LinesScrollViewer);
            await Model.LinesPage.LinesViewManager.AddLineTypeToListViewAsync("Tramwaje", Line.TRAM_BITS, this);
            await Model.LinesPage.LinesViewManager.AddLineTypeToListViewAsync("Autobusy", Line.BUS_BITS, this);
            await Model.LinesPage.LinesViewManager.AddLineTypeToListViewAsync("Minibusy", Line.MINI_BIT, this);
            await Model.LinesPage.LinesViewManager.AddLineTypeToListViewAsync("Nocne", Line.NIGHT_BUS_BIT, this);
            LoadingProgressRing.IsActive = false;
            _IsPageCached = true;
        }
    }
}
