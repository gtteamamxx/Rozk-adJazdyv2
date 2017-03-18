using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public class Line
    {
        [PrimaryKey]
        [Indexed]
        public int Id { get; set; }
        [Ignore]
        public string Url { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        [Ignore]
        public List<Schedule> Schedules { get; set; }
    }
}
