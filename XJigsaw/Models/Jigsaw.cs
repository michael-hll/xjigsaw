using System;
using System.Collections.Generic;
using XJigsaw.Helper;
using SQLite;
using Xamarin.Forms;

namespace XJigsaw.Models
{
    public class Jigsaw
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public byte[] Bytes { get; set; }
        public byte[] BytesSmall { get; set; }
        public string ImageRatio { get; set; }
        public int AspectRatio_X { get; set; }
        public int AspectRatio_Y { get; set; }
        public long ImageWidth { get; set; }
        public long ImageHeight { get; set; }
        public int Level { get; set; }
        public string LocationsInString { get; set; }
        public int Steps { get; set; }
        public double Opacity { get; set; }
        public string CreatedDateTime { get; set; }
        public string UpdatedDateTime { get; set; }

        [Ignore]
        public bool IsSelected { get; set; }
        [Ignore]
        public bool IsLoaded { get; set; }
        [Ignore]
        public bool IsApplied { get; set; }
        [Ignore]
        public bool IsJigsawInitiallized { get; set; }
        [Ignore]
        public bool IsDeleted { get; set; }
        [Ignore]
        public bool IsProcessing { get; set; }
        [Ignore]
        public bool IsUpdated { get; set; }
        [Ignore]
        public bool IsFreshNew { get; set; }


        public Jigsaw()
        {
            ImageRatio = Resources.AppResources.XJigsaw_Jigsaw_Zidong;
            AspectRatio_X = 0;
            AspectRatio_Y = 0;
            ImageWidth = 0;
            ImageHeight = 0;
            Level = 3;
            LocationsInString = "";
            Steps = 0;
            Opacity = 1;
            Name = Guid.NewGuid().ToString();
            CreatedDateTime = DateTime.Now.ToString(Utility.DATETIME_FORMAT);
            UpdatedDateTime = DateTime.Now.ToString(Utility.DATETIME_FORMAT);
            IsSelected = false;
            IsLoaded = false;
            IsApplied = false;
            IsJigsawInitiallized = true;
            IsDeleted = false;
            IsProcessing = false;
            IsUpdated = false;
            IsFreshNew = false;
        }
    }
}
