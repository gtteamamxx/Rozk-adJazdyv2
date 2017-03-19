﻿using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public class Line
    {
        public const int TRAM_BITS = TRAM_BIT;
        public const int BUS_BITS = NORMAL_BUS_BIT | BIG_BUS_BIT | FAST_BUS_BIT;

        public const int NORMAL_BUS_BIT = 1 << 1;
        public const int FAST_BUS_BIT = 1 << 2;
        public const int TRAM_BIT = 1 << 3;
        public const int MINI_BIT = 1 << 4;
        public const int AIRPORT_BIT = 1 << 5;
        public const int UPDATED_BIT = 1 << 6;
        public const int REPLACMENT_BIT = 1 << 7;
        public const int BIG_BUS_BIT = 1 << 8;
        public const int NIGHT_BUS_BIT = 1 << 9;
        public const int FREE_BIT = 1 << 11;
        public const int TRAIN_BIT = 1 << 12;
        public const int FAVOURITE_BIT = 1 << 13;

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
