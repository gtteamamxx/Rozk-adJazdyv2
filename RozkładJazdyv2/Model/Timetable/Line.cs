using RozkładJazdyv2.Pages.Lines;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

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
        public const int FREE_BIT = 1 << 10;
        public const int TRAIN_BIT = 1 << 11;
        public const int FAVOURITE_BIT = 1 << 12;

        public static int GetBusBitsWithoutFastBus()
        {
            int busBits = Line.BUS_BITS;
            busBits &= ~(Line.FAST_BUS_BIT);
            return busBits;
        }

        [PrimaryKey]
        [Indexed]
        public int Id { get; set; }

        public string Name { get; set; }
        public int Type { get; set; }

        [Ignore]
        public string Url { get; set; }
        [Ignore]
        public List<Schedule> Schedules { get; set; }
        [Ignore]
        public string EditedName => (this.Type & TRAM_BIT) == TRAM_BIT ? $"{this.Name}T" : this.Name;
        [Ignore]
        public Grid GridObjectInLinesList { get; set; }
        [Ignore]
        public string FavouriteText => IsFavourite ? "\xE00B" : "\x0000";

        public bool IsFavourite
        {
            get => (this.Type & FAVOURITE_BIT) == FAVOURITE_BIT;
            set
            {
                if ((!value && !IsFavourite) || (value && IsFavourite))
                    return;

                if (!value)
                {
                    this.Type &= ~(FAVOURITE_BIT);
                    RemoveLineFromFavouritesAsync();
                }
                else
                {
                    this.Type |= FAVOURITE_BIT;
                    AddLineToFavouriteAsync();
                }

                IsFavourite = value;

                LinesListPage.RefreshLineGridView(Pages.Lines.LinesListPage.LinesType.Favourites, FAVOURITE_BIT);
                RefreshLineGridInLinesList();
            }
        }

        #region Class methods

        public string GetLineLogoByType()
        {
            if ((this.Type & Line.BIG_BUS_BIT) > 0)
                return "\xE806";
            if ((this.Type & Line.TRAM_BITS) > 0)
                return "\xEB4D";
            if ((this.Type & Line.AIRPORT_BIT) > 0)
                return "\xEB4C";
            if ((this.Type & Line.TRAIN_BIT) > 0)
                return "\xE7C0";
            return "\xE806";
        }

        public async Task GetSchedules()
        {
            if (this.Schedules != null)
                return;
            string query = $"SELECT * FROM Schedule WHERE idOfLine = {this.Id};";
            this.Schedules = await SQLServices.QueryTimetableAsync<Schedule>(query);
        }

        private void RefreshLineGridInLinesList()
        {
            if(this.GridObjectInLinesList != null)
                (this.GridObjectInLinesList.Children[1] as TextBlock).Text 
                    = this.FavouriteText; // how to force update bindings?
        }

        private async void RemoveLineFromFavouritesAsync()
            => await SQLServices.ExecuteFavouriteAsync($"DELETE FROM Line WHERE Name = '{this.EditedName}';");

        private async void AddLineToFavouriteAsync()
            => await SQLServices.ExecuteFavouriteAsync($"INSERT INTO Line(Id, Name) VALUES(NULL,'{this.EditedName}');");

        #endregion
    }
}
