using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RickSoft.ORM.Engine.Attributes;
using RickSoft.ORM.Engine.Model;

namespace Example
{
    [DataTable("competitive")]
    public class Competitive : DatabaseObject
    {

        [DataField("competitive_id", true)]
        public int Id { get; set; }

        [DataField("profile_id")]
        [DoNotDuplicate(DoNotDuplicateField.Key)]
        public int ProfileId { get; set; }

        [DataField("season_id")]
        public int SeasonId { get; set; }

        [DataField("date")]
        [SelectOption(OptionType.OrderBy, OrderingMode.Descending)]
        public DateTime Date { get; set; }

        [DataField("wins")]
        [DoNotDuplicate(DoNotDuplicateField.Field)]
        public int Wins { get; set; }

        [DataField("lost")]
        [DoNotDuplicate(DoNotDuplicateField.Field)]
        public int Lost { get; set; }

        [DataField("ties")]
        [DoNotDuplicate(DoNotDuplicateField.Field)]
        public int Ties { get; set; }

        [DataField("play_time")]
        [DoNotDuplicate(DoNotDuplicateField.Field)]
        public int PlayTime { get; set; }

        public Competitive()
        {
            Date = DateTime.Now;
        }
        
    }
}
