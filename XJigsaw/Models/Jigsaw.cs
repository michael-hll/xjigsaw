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

        /// <summary>
        /// To tell if this jigsaw is from saved history.
        /// </summary>
        [Ignore]
        public bool IsSelected { get; set; }

        /// <summary>
        /// To tell if this jigsaw is from app startup load.
        /// </summary>
        [Ignore]
        public bool IsLoaded { get; set; }

        /// <summary>
        /// To tell if this jigsaw has been built and filled into the jigsaw panel container.
        /// </summary>
        [Ignore]
        public bool IsApplied { get; set; }

        /// <summary>
        /// To tell if the jigsaw tile position in intial state. If user tapped then
        /// it's value is false, otherwise it's ture
        /// </summary>
        [Ignore]
        public bool IsTilePositionInitial { get; set; }

        /// <summary>
        /// To tell if this jigsaw has been deleted.
        /// </summary>
        [Ignore]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// To tell if the selected jigsaw has been built in the main jigsaw page.
        /// </summary>
        [Ignore]
        public bool IsSelectedItemNotProcessed { get; set; }

        /// <summary>
        /// To tell if the jigsaw tiles postion has been chenged.
        /// </summary>
        [Ignore]
        public bool IsTilePositionChanged { get; set; }

        /// <summary>
        /// When there is no stored jigsaw from database and app create a new jigsaw during startup
        /// then IsFreshNew value is set to true.
        /// </summary>
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
            IsTilePositionInitial = true;
            IsDeleted = false;
            IsSelectedItemNotProcessed = false;
            IsTilePositionChanged = false;
            IsFreshNew = false;
        }
    }
}
