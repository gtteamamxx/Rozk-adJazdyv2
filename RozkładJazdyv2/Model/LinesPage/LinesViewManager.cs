using RozkładJazdyv2.Pages.Lines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace RozkładJazdyv2.Model.LinesPage
{
    public class LinesViewManager
    {
        private LinesViewManager() { }

        private static List<Tuple<LinesViewPage.LinesType, GridView>> _LinesTypesGridView;
        private static ScrollViewer _LinesTypeScrollViewer;

        public static void SetInstance(ScrollViewer linesScrollViewer)
        {
            if (_LinesTypeScrollViewer == null)
                _LinesTypeScrollViewer = linesScrollViewer;
            if (_LinesTypesGridView == null)
                _LinesTypesGridView = new List<Tuple<LinesViewPage.LinesType, GridView>>();
        }

        public static void RefreshGridView(GridView gridView, int acceptedLinesBit)
        {
            var contentGrid = gridView.Parent as Grid;
            if (!(contentGrid is Grid))
            {
                contentGrid = gridView.DataContext as Grid;
                if (!(contentGrid is Grid))
                    return;
            }
            gridView.Items.Clear();
            AddLinesToGridView(ref gridView, acceptedLinesBit);
            CheckIfLineIsEmptyAndHideGridViewIfItIs(contentGrid);
        }

        public static List<Tuple<LinesViewPage.LinesType, GridView>> GetLineTypesGridViewList()
            => _LinesTypesGridView;

        public static async Task AddLineTypeToListViewAsync(LinesViewPage.LinesType type, string name, int acceptedLinesBit, Pages.Lines.LinesViewPage page,
                                                            SelectionChangedEventHandler selectionChangedFunction)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                var contentGrid = new Grid();
                AddRowDefinitionsToContentGrid(ref contentGrid);
                AddPanelGridToContentGrid(ref contentGrid, name);
                var gridView = AddLinesGridViewToContentGrid(contentGrid, acceptedLinesBit, page, selectionChangedFunction);
                CheckIfLineIsEmptyAndHideGridViewIfItIs(contentGrid);
                await AddContentGridToPageAsync(contentGrid, gridView, type);
            });
        }
        private static void CheckIfLineIsEmptyAndHideGridViewIfItIs(Grid grid)
        {
            bool isLineTypeEmpty = IsLineTypeEmpty(grid);
            if (isLineTypeEmpty)
                SetContentGridVisible(grid, Visibility.Collapsed);
            else
                SetContentGridVisible(grid, Visibility.Visible);
        }

        private static void SetContentGridVisible(Grid grid, Visibility visibleState)
            => grid.Visibility = visibleState;

        private static async Task AddContentGridToPageAsync(Grid grid, GridView gridView, LinesViewPage.LinesType type)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                var scrollViewerGrid = _LinesTypeScrollViewer.Content as StackPanel;
                scrollViewerGrid.Children.Add(grid);
                gridView.DataContext = grid;
                _LinesTypesGridView.Add(new Tuple<LinesViewPage.LinesType, GridView>(type, gridView));
            });
        }

        private static bool IsLineTypeEmpty(Grid grid)
        {
            var linesGridView = grid.Children.ElementAt(1) as GridView;
            return linesGridView.Items.Count() == 0;
        }

        private static GridView AddLinesGridViewToContentGrid(Grid grid, int acceptedLinesBit,
                            Pages.Lines.LinesViewPage page, SelectionChangedEventHandler selectionChangedFunction)
        {
            var linesGridView = new GridView()
            {
                Margin = new Thickness(10),
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            linesGridView.SelectionChanged += selectionChangedFunction;
            linesGridView.ContainerContentChanging += LinesGridView_ContainerContentChanging;
            Grid.SetRow(linesGridView, 1);
            linesGridView.ItemsPanel = page.Resources["LinesGridViewItemPanelTemplate"] as ItemsPanelTemplate;
            linesGridView.ItemTemplate = page.Resources["LineDataTemplate"] as DataTemplate;
            AddLinesToGridView(ref linesGridView, acceptedLinesBit);
            grid.Children.Add(linesGridView);
            return linesGridView;
        }

        private static void AddLinesToGridView(ref GridView gridView, int acceptedLinesBit)
        {
            foreach (var line in Timetable.Instance.Lines)
                if ((line.Type & acceptedLinesBit) > 0)
                    gridView.Items.Add(line);
        }

        private static void LinesGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var gridOfLine = ((Grid)args.ItemContainer.ContentTemplateRoot);
            Line lineClass = ((Line)args.Item);
            lineClass.GridObjectInLinesList = gridOfLine;
        }

        private static void AddPanelGridToContentGrid(ref Grid grid, string name)
        {
            var panelGrid = new Grid()
            {
                Height = 30,
                Background = new SolidColorBrush(Colors.AliceBlue),
                BorderBrush = new SolidColorBrush(Colors.Blue),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 2, 0, 2)
            };
            panelGrid.Children.Add(new TextBlock()
            {
                Text = name,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Black),
                FontSize = 20
            });
            panelGrid.Children.Add(new TextBlock()
            {
                Text = "\xE74B",
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Foreground = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(0, 0, 10, 0),
                FontSize = 18,
                FontWeight = FontWeights.ExtraBold,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            });
            panelGrid.Tapped += PannelGridTapped;
            grid.Children.Add(panelGrid);
        }

        private static void AddRowDefinitionsToContentGrid(ref Grid grid)
        {
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
        }

        private static void PannelGridTapped(object sender, TappedRoutedEventArgs e)
        {
            var panelGrid = sender as Grid;
            var linesGridView = ((Grid)panelGrid.Parent).Children.ElementAt(1);
            var arrowTextBlock = panelGrid.Children.ElementAt(1) as TextBlock;
            if (linesGridView.Visibility == Visibility.Collapsed)
            {
                arrowTextBlock.Text = "\xE74B";
                linesGridView.Visibility = Visibility.Visible;
            }
            else
            {
                arrowTextBlock.Text = "\xE74A";
                linesGridView.Visibility = Visibility.Collapsed;
            }
        }
    }
}
