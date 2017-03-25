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

        private static ScrollViewer _LinesTypeScrollViewer;

        public static void SetInstance(ScrollViewer linesScrollViewer)
        {
            if (_LinesTypeScrollViewer == null)
                _LinesTypeScrollViewer = linesScrollViewer;
        }

        public static async Task AddLineTypeToListViewAsync(string name, int acceptedLinesBit, Pages.Lines.LinesViewPage page,
                                                            SelectionChangedEventHandler selectionChangedFunction)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                var contentGrid = new Grid();
                AddRowDefinitionsToContentGrid(ref contentGrid);
                AddPanelGridToContentGrid(ref contentGrid, name);
                await AddLinesGridViewToContentGridAsync(contentGrid, acceptedLinesBit, page, selectionChangedFunction);
                bool isLineTypeEmpty = IsLineTypEmpty(contentGrid);
                if (isLineTypeEmpty)
                    return;
                await AddContentGridToPageAsync(contentGrid);
            });
        }

        private static async Task AddContentGridToPageAsync(Grid grid)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                var scrollViewerGrid = _LinesTypeScrollViewer.Content as StackPanel;
                scrollViewerGrid.Children.Add(grid);
            });
        }

        private static bool IsLineTypEmpty(Grid grid)
        {
            var linesGridView = grid.Children.ElementAt(1) as GridView;
            return linesGridView.Items.Count() == 0;
        }

        private static async Task AddLinesGridViewToContentGridAsync(Grid grid, int acceptedLinesBit,
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
            foreach (var line in Timetable.Instance.Lines)
                if ((line.Type & acceptedLinesBit) > 0)
                    linesGridView.Items.Add(line);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                grid.Children.Add(linesGridView) );
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
