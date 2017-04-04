using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public class Schedule
    {
        [PrimaryKey]
        [Indexed]
        public int Id { get; set; }

        public int IdOfLine { get; set; }
        public string Name { get; set; }
        public bool IsActualSchedule { get; set; }

        [Ignore]
        public string Url { get; set; }
        [Ignore]
        public List<Track> Tracks { get; set; }

        public void GetTracks()
        {
            if (this.Tracks != null)
                return;

            string query = $"SELECT * FROM Track WHERE IdOfSchedule = {this.Id};";
            this.Tracks = SQLServices.QueryTimetable<Track>(query);
        }
    }
}
