using RozkładJazdyv2.Model;
using RozkładJazdyv2.Model.LinesPage;
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
        private List<GridView> _ClickedGridViews;
        private bool _IsPageCached;

        public LinesViewPage()
        {
            this.InitializeComponent();
            _ClickedGridViews = new List<GridView>();
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
            await Model.LinesPage.LinesViewManager.AddLineTypeToListViewAsync("Ulubione", Line.FAVOURITE_BIT, this, LineSelectionChangedAsync);
            await Model.LinesPage.LinesViewManager.AddLineTypeToListViewAsync("Tramwaje", Line.TRAM_BITS, this, LineSelectionChangedAsync);
            await Model.LinesPage.LinesViewManager.AddLineTypeToListViewAsync("Autobusy", Line.BUS_BITS, this, LineSelectionChangedAsync);
            await Model.LinesPage.LinesViewManager.AddLineTypeToListViewAsync("Minibusy", Line.MINI_BIT, this, LineSelectionChangedAsync);
            await Model.LinesPage.LinesViewManager.AddLineTypeToListViewAsync("Nocne", Line.NIGHT_BUS_BIT, this, LineSelectionChangedAsync);
            await Model.LinesPage.LinesViewManager.AddLineTypeToListViewAsync("Inne", Line.AIRPORT_BIT, this, LineSelectionChangedAsync);
            HideLoadingStackPanel();
            _IsPageCached = true;
        }

        private async Task ShowLinePageAsync(ChangeLineParameter changeLineParameter)
        {
            await Task.Delay(100);
            MainFrameHelper.GetMainFrame().Navigate(typeof(LinePage), changeLineParameter);
        }

        private async void LineSelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            var gridView = sender as GridView;
            if (gridView.SelectedIndex == -1)
                return;
            AddGridViewToCacheList(gridView);
            var line = gridView.SelectedItem as Line;
            if (line.Schedules == null)
                line.Schedules = await GetLineSchedules(line);
            if (line.Schedules.Count() == 1)
                await ShowLinePageAsync(new ChangeLineParameter() { Line = line, SelectedSchedule = line.Schedules.ElementAt(0) });
            else
                Model.LinesPage.FlyoutHelper.ShowFlyOutAtLineGrid(line, ScheduleClickedAsync);
            ResetClickedGrids();
        }

        private async void ScheduleClickedAsync(object sender, RoutedEventArgs e)
        {
            var clickedButton = (Button)sender;
            var selectedLine = (Line)(clickedButton.DataContext);
            var selectedSchedule = selectedLine.Schedules.First(p => p.Name == (string)clickedButton.Content);
            await ShowLinePageAsync(new ChangeLineParameter() { Line = selectedLine, SelectedSchedule = selectedSchedule });
        }

        private async Task<List<Schedule>> GetLineSchedules(Line line)
        {
            string query = $"SELECT * FROM Schedule WHERE idOfLine = {line.Id};";
            return await SQLServices.QueryAsync<Schedule>(query);
        }

        private void ResetClickedGrids()
        {
            foreach(var gridView in _ClickedGridViews)
                gridView.SelectedIndex = -1;
        }

        private void AddGridViewToCacheList(GridView gridView)
        {
            if (_ClickedGridViews.FirstOrDefault(p => p == gridView) == null)
                _ClickedGridViews.Add(gridView);
        }

        private void HideLoadingStackPanel()
            => LoadingStackPanel.Visibility = Visibility.Collapsed;
    }
}
