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

        public static async Task AddLineTypeToListViewAsync(string name, int acceptedLinesBit, Pages.Lines.LinesViewPage page)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var contentGrid = new Grid();
                AddRowDefinitionsToContentGrid(ref contentGrid);
                AddPanelGridToContentGrid(ref contentGrid, name);
                AddLinesGridViewToContentGrid(ref contentGrid, acceptedLinesBit, page);
                AddContentGridToPage(contentGrid);
            });
        }

        private static void AddContentGridToPage(Grid grid)
        {
            var scrollViewerGrid = _LinesTypeScrollViewer.Content as StackPanel;
            scrollViewerGrid.Children.Add(grid);
        }

        private static void AddLinesGridViewToContentGrid(ref Grid grid, int acceptedLinesBit, Pages.Lines.LinesViewPage page)
        {
            var linesGridView = new GridView()
            {
                Margin = new Thickness(10),
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(linesGridView, 1);
            linesGridView.ItemsPanel = page.Resources["LinesGridViewItemPanelTemplate"] as ItemsPanelTemplate;
            linesGridView.ItemTemplate = page.Resources["LineDataTemplate"] as DataTemplate;
            foreach (var line in Timetable.Instance.Lines)
                if ((line.Type & acceptedLinesBit) > 0)
                    linesGridView.Items.Add(line);
            grid.Children.Add(linesGridView);
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
