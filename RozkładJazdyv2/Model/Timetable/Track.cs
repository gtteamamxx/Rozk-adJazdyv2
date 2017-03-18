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
    }
}
