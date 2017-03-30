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
using RozkładJazdyv2.Model.BusStopListPage;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RozkładJazdyv2.Pages.BusStops
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BusStopsListPage : Page
    {
        private List<BusStopName> _ListOfBusStopNames => Timetable.Instance.BusStopsNames.OrderBy(p => p.Name).ToList();
        private ObservableCollection<BusStopDependency> _BusStopDependencies;

        public BusStopsListPage()
        {
            this.InitializeComponent();
            this.SetIsBackFromPageAllowed(true);

            _BusStopDependencies = new ObservableCollection<BusStopDependency>();

            this.Loaded += BusStopsListPage_Loaded;
        }

        private void BusStopsListPage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void ResetClickedItems(ListView listView)
        {
            
        }

        private async void BusStopsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedBusStopName = BusStopsListView.SelectedItem as BusStopName;
            if (selectedBusStopName == null)
                return;

            ResetClickedItems(sender as ListView);

            _BusStopDependencies.Clear();
            List<BusStop> selectedBusStopsList = await SQLServices.QueryTimetableAsync<BusStop>($"SELECT * FROM BusStop WHERE IdOfName = {selectedBusStopName.Id};");

            foreach(BusStop busStop in selectedBusStopsList)
            {
                Line line = Timetable.Instance.Lines.First(p => p.Id == busStop.IdOfLine);
                Schedule schedule = (await SQLServices.QueryTimetableAsync<Schedule>($"SELECT * FROM Schedule WHERE Id = {busStop.IdOfSchedule};")).First();
                Track track = (await SQLServices.QueryTimetableAsync<Track>($"SELECT * FROM Track WHERE Id = {busStop.IdOfTrack};")).First();

                track.BusStops = new List<BusStop>().Add<BusStop>(busStop);
                schedule.Tracks = new List<Track>().Add<Track>(track);

                bool dependencyLineAlereadyExist = false;

                BusStopDependency tempDependency = null;
                if ((tempDependency = _BusStopDependencies.FirstOrDefault(p => p.Line.Id == line.Id)) != null)
                {
                    if (tempDependency.Line.Schedules
                                           .SelectMany(p => p.Tracks)
                                           .FirstOrDefault(p => p.Id == track.Id) != null)
                    {
                        continue;
                    }

                    if (!tempDependency.Line.Schedules.Any(p => p.Id == schedule.Id))
                    {
                        tempDependency.Line.Schedules.Add(schedule);
                        continue;
                    }

                    dependencyLineAlereadyExist = true;
                }

                Line dependencyLine = new Line(lockUpdateFavouriteSqlStatus: true)
                {
                    Id = line.Id,
                    Name = line.Name,
                    Type = line.Type,
                    IsFavourite = line.IsFavourite
                };

                if (!dependencyLineAlereadyExist)
                {
                    dependencyLine.Schedules = new List<Schedule>().Add<Schedule>(schedule);

                    BusStopDependency busStopDependency = new BusStopDependency()
                    {
                        Line = dependencyLine
                    };

                    _BusStopDependencies.Add(busStopDependency);
                }
                else
                {
                    tempDependency.Line.Schedules.First(p => p.Id == track.IdOfSchedule).Tracks.Add(track);
                }
            }

            ;
        }
    }
}
