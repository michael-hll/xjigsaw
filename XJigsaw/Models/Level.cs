using System;
using XJigsaw.Helper;
using SQLite;
using XJigsaw.Resources;

namespace XJigsaw.Models
{
    public class Level
    {
        [PrimaryKey]
        public int ID { get; set; }
        [NotNull]
        public string Name { get; set; }
        public string NameOther { get; set; }
        public long BestScore { get; set; }
        public string ScoreDateTime { get; set; }

        [Ignore]
        public string FormatedBestScore
        {
            get
            {
                if (BestScore > 0)
                    return Utility.FormatTimeSpan(TimeSpan.FromTicks(BestScore));
                else
                    return "";
            }
        }

        [Ignore]
        public string FromatedName
        {
            get
            {
                if (AppResources.Culture.Name.Contains("zh"))
                    return $"{Name}({ID}x{ID})";
                else
                    return $"{NameOther}({ID}x{ID})";
            }
        }
    }
}
