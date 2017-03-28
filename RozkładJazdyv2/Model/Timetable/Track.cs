using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public class Track
    {
        [PrimaryKey]
        [Indexed]
        public int Id { get; set; }

        public int IdOfName { get; set; }
        public int IdOfLine { get; set; }
        public int IdOfSchedule { get; set; }

        [Ignore]
        public string Name { get { return Timetable.Instance.TracksNames.First(p => p.Id == this.IdOfName).Name; } }
        [Ignore]
        public string Url { get; set; }
        [Ignore]
        public List<BusStop> BusStops { get; set; }

        public async Task GetBusStops()
        {
            if (this.BusStops != null)
                return;

            string query = $"SELECT * FROM BusStop WHERE IdOfTrack = {this.Id} AND IdOfSchedule = {this.IdOfSchedule};";
            this.BusStops = await SQLServices.QueryTimetableAsync<BusStop>(query);
        }
    }
}
