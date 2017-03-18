using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public class Letter
    {
        [PrimaryKey]
        [Indexed]
        public int Id { get; set; }
        public int IdOfBusStop { get; set; }
        public int IdOfName { get; set; }
        public string Name { get; set; }
    }
}
