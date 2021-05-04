using SQLite;

namespace XJigsaw.Models
{
    public class JigsawVersion
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string AppVersion { get; set; }
        public string ShortVersion { get; set; }
        public string UpdateDateTime { get; set; }
    }
}
