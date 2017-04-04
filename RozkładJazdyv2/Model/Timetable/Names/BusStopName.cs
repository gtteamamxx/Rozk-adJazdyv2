using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public class BusStopName
    {
        [PrimaryKey]
        [Indexed]
        public int Id { get; set; }
        public string Name { get; set; }

        private bool _IsFavouriteFunc()
            => (SQLServices.QueryFavourite<BusStopName>($"SELECT * FROM BusStopName WHERE Name = '{GetBytes(this.Name)}';")).Count() > 0;

        public bool IsFavourite
        {
            get
            {
                return Task.Run(() => _IsFavouriteFunc()).Result;
            }
            set
            {
                if (value)
                    SaveBusStopAsFavourite();
                else
                    DeleteBusStopFromFavourite();

                void SaveBusStopAsFavourite()
                    =>  SQLServices.ExecuteFavourite($@"INSERT INTO BusStopName(Id, Name) VALUES(NULL,'{GetBytes(this.Name)}');");

                void DeleteBusStopFromFavourite()
                    =>  SQLServices.ExecuteFavourite($@"DELETE FROM BusStopName WHERE Name = '{GetBytes(this.Name)}';");
            }
        }

        private string GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return String.Join(" ", bytes.Select(p => $"{p}"));
        }

        public string FavouriteText =>this.IsFavourite ? "\xE00B" : "\x0000";
    }
}
