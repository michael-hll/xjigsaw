using SQLite;

namespace XJigsaw.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public string NameOther { get; set; }
        public int Level { get; set; }
        public string CreatedDateTime { get; set; }
    }
}
