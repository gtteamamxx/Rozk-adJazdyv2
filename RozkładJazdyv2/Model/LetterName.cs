using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public class LetterName
    {
        [PrimaryKey]
        [Indexed]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
