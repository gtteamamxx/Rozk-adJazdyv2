using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public class BusStop
    {
        [PrimaryKey]
        [Indexed]
        public int Id { get; set; }

        public int IdOfName { get; set; }
        public int IdOfLine { get; set; }
        public int IdOfSchedule { get; set; }
        public int IdOfTrack { get; set; }
        public bool IsVariant { get; set; }
        public bool IsLastStopOnTrack { get; set; }
        public bool IsBusStopZone { get; set; }

        [Ignore]
        public string Url { get; set; }
        [Ignore]
        public bool IsOnDemand { get { return Name.Contains("n/ż"); } }
        [Ignore]
        public string Name { get { return Timetable.Instance.BusStopsNames.First(p => p.Id == this.IdOfName).Name; } } //todo
        [Ignore]
        public List<Hour> Hours { get; set; }

        public string GetBusStopEditedName()
        {
            string editedName = string.Empty;

            if (this.IsVariant)
                editedName = $"-- {this.Name}";
            else if (this.IsBusStopZone)
                editedName = $"[S] {this.Name}";
            else
                editedName = this.Name;

            return editedName;
        }
    }
}
