using SQLite;

namespace XJigsaw.Models
{
    public class Setting
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public int Level { get; set; }
        public string ImageRatio { get; set; }
        public string ImageRatioOther { get; set; }
        public int AspectRatio_X { get; set; }
        public int AspectRatio_Y { get; set; }
        public double Opacity { get; set; }
        public int CurrentJigsawId { get; set; }
        public bool IsPlaySound { get; set; }

        public Setting()
        {
            Level = 3;
            ImageRatio = "自动";
            ImageRatioOther = "Auto";
            AspectRatio_X = 0;
            AspectRatio_Y = 0;
            Opacity = 0.5;
            CurrentJigsawId = -1;
            IsPlaySound = false;
        }

        public string GetCorrectImageRatio(string cultureName)
        {
            if (cultureName.Contains("zh"))
                return ImageRatio;
            else
                return ImageRatioOther;
        }
    }
}
