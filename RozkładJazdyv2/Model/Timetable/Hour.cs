using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public class Hour
    {
        [PrimaryKey]
        [Indexed]
        public int Id { get; set; }
        public int IdOfName { get; set; }
        public int IdOfBusStop { get; set; }
        [Ignore]
        public string Name { get { return Timetable.Instance.HoursNames.First(p => p.Id == this.IdOfName).Name; } }
        public string Hours { get; set; }
    }
}
