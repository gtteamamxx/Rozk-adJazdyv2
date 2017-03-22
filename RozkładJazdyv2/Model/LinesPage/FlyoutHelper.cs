using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace RozkładJazdyv2.Model.LinesPage
{
    public class FlyoutHelper
    {
        private FlyoutHelper() { }

        public static void ShowFlyOutAtLineGrid(Grid lineGrid, Line line, RoutedEventHandler scheduleClickedEvent)
        {
            var inivisibleButton = GetLineGridInvisibleButton(lineGrid);
            var flyout = CreateFlyOutAtButton(ref inivisibleButton, line, ref scheduleClickedEvent);
            bool isGridLineGrid = lineGrid == line.GridObjectInLinesList;
            flyout.Closed += (s, e) =>
            {
                lineGrid.Children.Remove(inivisibleButton);
                if (isGridLineGrid)
                    RemoveLineGridBorder(ref lineGrid);
            };
            if(isGridLineGrid)
                SetLineGridBorder(ref lineGrid);
            lineGrid.Children.Add(inivisibleButton);
            flyout.ShowAt(inivisibleButton);
        }

        public static Flyout ShowFlyOutAtButtonInLinePage(Button button, Line line, RoutedEventHandler scheduleClickedEven)
        {
            var flyout = CreateFlyOutAtButton(ref button, line, ref scheduleClickedEven, true);
            flyout.ShowAt(button);
            return flyout; 
        }

        private static void SetLineGridBorder(ref Grid lineGrid)
        {
            lineGrid.BorderBrush = new SolidColorBrush(Colors.LightGreen);
            lineGrid.BorderThickness = new Thickness(1);
        }

        private static Flyout CreateFlyOutAtButton(ref Button button, Line line, 
                ref RoutedEventHandler scheduleClickedEvent, bool hideActuallyShowedSchedule = false)
        {
            var contentGrid = new Grid();
            List<Schedule> schedules = new List<Schedule>();
            if (hideActuallyShowedSchedule)
            {
                var actuallyShowedSchedule = Pages.Lines.LinePage.ActualShowingLineParameters.SelectedSchedule;
                schedules = line.Schedules.Where(p => p.Id != actuallyShowedSchedule.Id).ToList();
            }
            else
                schedules = line.Schedules;

            AddRowsToGridBySchedulesNum(ref contentGrid, schedules);
            AddHeaderToContentGrid(ref contentGrid);
            AddSchedulesToGridContent(ref contentGrid, schedules, ref scheduleClickedEvent, line);
            return new Flyout() { Content = contentGrid };
        }

        private static void AddHeaderToContentGrid(ref Grid contentGrid)
            => contentGrid.Children.Add(new TextBlock()
            {
                Text = "Wybierz rozkład:",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White)
            });

        private static void AddSchedulesToGridContent(ref Grid contentGrid, List<Schedule> listOfSchedules,
                                                        ref RoutedEventHandler scheduleClickedEvent, Line line)
        {
            int schedulesCount = listOfSchedules.Count();
            for (int i = 0; i < schedulesCount; i++)
            {
                var schedule = listOfSchedules[i];
                var scheduleButton = new Button()
                {
                    Content = schedule.Name,
                    BorderBrush = new SolidColorBrush(Colors.Blue),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(5),
                    DataContext = line
                };
                Grid.SetRow(scheduleButton, i+1);
                contentGrid.Children.Add(scheduleButton);
                scheduleButton.Click += scheduleClickedEvent;
            }
        }

        private static void AddRowsToGridBySchedulesNum(ref Grid contentGrid, List<Schedule> listOfSchedules)
        {
            int schedulesCount = listOfSchedules.Count();
            for (int i = 0; i <= schedulesCount; i++)
                contentGrid.RowDefinitions.Add(
                    new RowDefinition() { Height = GridLength.Auto });
        }

        private static Button GetLineGridInvisibleButton(Grid lineGrid)
            => new Button() { Visibility = Visibility.Collapsed };

        private static void RemoveLineGridBorder(ref Grid lineGrid)
            => lineGrid.BorderThickness = new Thickness(0);
    }
}
