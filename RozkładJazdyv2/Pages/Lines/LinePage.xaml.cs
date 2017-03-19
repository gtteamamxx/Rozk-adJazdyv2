using RozkładJazdyv2.Model;
using RozkładJazdyv2.Model.LinesPage;
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
    public sealed partial class LinePage : Page
    {
        private static ChangeLineParameter _ActualShowingParameters;
        private static Line _SelectedLine => _ActualShowingParameters.Line;
        private static Schedule _SelectedSchedule => _ActualShowingParameters.SelectedSchedule;
        private static bool _IsRefreshingPageNeeded;

        public LinePage()
        {
            this.InitializeComponent();
            this.Loaded += LinePage_Loaded;
        }

        private void LinePage_Loaded(object sender, RoutedEventArgs e)
        {
            if(_IsRefreshingPageNeeded == true)
                UpdateLineInfo();
        }

        private void UpdateLineInfo()
        {
            UpdateLineHeaderTexts();
            // todo
            _IsRefreshingPageNeeded = false;
        }

        private void UpdateLineHeaderTexts()
        {
            LineScheduleNameTextBlock.Text = _SelectedSchedule.Name;
            LineNumberTextBlock.Text = _SelectedLine.EditedName;
            LineLogoTextBlock.Text = GetLineLogoByType(_SelectedLine.Type);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var changeLineParameter = e.Parameter as ChangeLineParameter;
            if (_ActualShowingParameters == null)
            {
                _ActualShowingParameters = changeLineParameter;
                _IsRefreshingPageNeeded = true;
            }
            else
            {
                if (changeLineParameter.Line.Id != _ActualShowingParameters.Line.Id
                    && changeLineParameter.SelectedSchedule.Id != _ActualShowingParameters.SelectedSchedule.Id)
                {
                    _ActualShowingParameters = changeLineParameter;
                    _IsRefreshingPageNeeded = true;
                }
            }
        }

        private string GetLineLogoByType(int type)
        {
            if ((type & Line.BIG_BUS_BIT) > 0)
                return "\xE806";
            if ((type & Line.TRAM_BITS) > 0)
                return "\xE812";
            return "\xE806";
        }
    }
}
