using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model.LinesPage
{
    public class ChangeBusStopParametr
    {
        public Track Track { get; set; }
        public BusStop BusStop { get; set; }
        public Schedule Schedule { get; set; }
        public Line Line { get; set; }
    }
}
