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
    public class LinesListViewManager
    {
        private LinesListViewManager() { }

        private static List<Tuple<LinesListPage.LinesType, GridView>> _LinesTypesGridView;
        private static ScrollViewer _LinesTypeScrollViewer;
        private static int _LastAcceptedLineBits;

        public static void SetInstance(ScrollViewer linesScrollViewer)
        {
            if (_LinesTypeScrollViewer == null)
                _LinesTypeScrollViewer = linesScrollViewer;

            if (_LinesTypesGridView == null)
                _LinesTypesGridView = new List<Tuple<LinesListPage.LinesType, GridView>>();
        }

        public static void RefreshGridView(GridView gridView, int acceptedLinesBit)
        {
            Grid contentGrid = gridView.Parent as Grid;

            if (!(contentGrid is Grid))
            {
                contentGrid = gridView.DataContext as Grid;

                if (!(contentGrid is Grid))
                    return;
            }

            gridView.Items.Clear();

            _LastAcceptedLineBits = acceptedLinesBit;
            AddLinesToGridView(ref gridView, acceptedLinesBit);
            CheckIfLineIsEmptyAndHideGridViewIfItIs(contentGrid);
        }

        public static List<Tuple<LinesListPage.LinesType, GridView>> GetLineTypesGridViewList()
            => _LinesTypesGridView;

        public static async Task AddLineTypeToListViewAsync(LinesListPage.LinesType type, string name, int acceptedLinesBit,
                                                            SelectionChangedEventHandler selectionChangedFunction)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                Grid contentGrid = new Grid();
                AddRowDefinitionsToContentGrid(ref contentGrid);
                AddPanelGridToContentGrid(ref contentGrid, name);

                GridView gridView = AddLinesGridViewToContentGrid(contentGrid, acceptedLinesBit, selectionChangedFunction);

                CheckIfLineIsEmptyAndHideGridViewIfItIs(contentGrid);
                await AddContentGridToPageAsync(contentGrid, gridView, type);
            });
        }
        private static void CheckIfLineIsEmptyAndHideGridViewIfItIs(Grid grid)
            => SetContentGridVisible(grid, IsLineTypeEmpty(grid) 
                ? Visibility.Collapsed : Visibility.Visible);
        
        private static void SetContentGridVisible(Grid grid, Visibility visibleState)
            => grid.Visibility = visibleState;

        private static async Task AddContentGridToPageAsync(Grid grid, GridView gridView, LinesListPage.LinesType type)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                StackPanel scrollViewerStacKPanel = _LinesTypeScrollViewer.Content as StackPanel;
                scrollViewerStacKPanel.Children.Add(grid);

                gridView.DataContext = grid;
                _LinesTypesGridView.Add(new Tuple<LinesListPage.LinesType, GridView>(type, gridView));
            });
        }

        private static bool IsLineTypeEmpty(Grid grid)
        {
            GridView linesGridView = grid.Children.ElementAt(1) as GridView;
            return linesGridView.Items.Count() == 0;
        }

        private static GridView AddLinesGridViewToContentGrid(Grid grid, int acceptedLinesBit, SelectionChangedEventHandler selectionChangedFunction)
        {
            GridView linesGridView = new GridView()
            {
                Margin = new Thickness(10),
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            linesGridView.SelectionChanged += selectionChangedFunction;
            linesGridView.ContainerContentChanging += LinesGridView_ContainerContentChanging;

            Grid.SetRow(linesGridView, 1);

            linesGridView.ItemsPanel = App.Current.Resources["LinesGridViewItemPanelTemplate"] as ItemsPanelTemplate;
            linesGridView.ItemTemplate = App.Current.Resources["LineDataTemplate"] as DataTemplate;

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
            Grid gridOfLine = ((Grid)((Pages.Lines.LineUserControl)args.ItemContainer.ContentTemplateRoot).Content);

            if ((_LastAcceptedLineBits & Line.FAVOURITE_BIT) == Line.FAVOURITE_BIT)
                return;

            Line lineClass = ((Line)args.Item);
            lineClass.GridObjectInLinesList = gridOfLine;
        }

        private static void AddPanelGridToContentGrid(ref Grid grid, string name)
        {
            Grid panelGrid = new Grid()
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
            Grid panelGrid = (Grid)sender;
            GridView linesGridView = (GridView)((Grid)panelGrid.Parent).Children.ElementAt(1);
            TextBlock arrowTextBlock = (TextBlock)panelGrid.Children.ElementAt(1);

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
