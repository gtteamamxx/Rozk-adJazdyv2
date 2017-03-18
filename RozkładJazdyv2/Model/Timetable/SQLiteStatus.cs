using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public class SQLiteStatus
    {
        public enum LoadStatus
        {
            Failed = 0,
            Succes
        }
        [PrimaryKey]
        [Unique]
        public LoadStatus Status { get; set; }
    }
}
