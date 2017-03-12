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
        private static int _Id;
        public Schedule(bool increment = false)
        {
            if (increment)
                Id = _Id++;
        }

        [PrimaryKey]
        [Indexed]
        public int Id { get; }
        public int IdOfLine { get; set; }
        [Ignore]
        public string Url { get; set; }
        public string Name { get; set; }
        public bool IsActualSchedule { get; set; }
    }
}
