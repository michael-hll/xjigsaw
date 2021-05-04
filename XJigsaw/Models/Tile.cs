using System.Collections.Generic;
using Xamarin.Forms;

namespace XJigsaw.Models
{
    public class Tile
    {
        public Tile(int row, int col, Image image, bool isEmptyTile = false,
            double width = 0, double height = 0, int level = 0, double opacity = 1)
        {
            double padding = 0.5;
            if (level > 16) padding = 0.25;

            image.Aspect = Aspect.AspectFill;           

            TileView = new ContentView
            {
                Padding = new Thickness(padding),
                Content = image
            };
            TileView.IsVisible = false;

            Opacity = opacity;
            OriginalRow = row;
            OriginalCol = col;
            Row = row;
            Col = col;
            IsEmptyTile = isEmptyTile;
            Width = width;
            Height = height;

            if (isEmptyTile)
            {
                ContentView view = (ContentView)TileView;
                
                double imageSize = 24;
                if (Dictionary.Count > 400) imageSize = 12;
                double verticalPadding = (height - imageSize) / 2;
                double horizonPadding = (width - imageSize) / 2;

                view.Padding = new Thickness(horizonPadding,verticalPadding);               
            }

            // Add TileView to dictionary for obtaining Tile from TileView
            Dictionary.Add(TileView, this);
        }

        public static Dictionary<View, Tile> Dictionary { get; } = new Dictionary<View, Tile>();

        int row = 0;
        
        public int Row {
            get { return row; }
            set
            {
                row = value;
                if (row == OriginalRow && col == OriginalCol)
                    this.TileView.Opacity = 1;
                else
                    this.TileView.Opacity = Opacity;
            }
        }

        int col = 0;
        public int Col {
            get { return col; }
            set
            {
                col = value;
                if (row == OriginalRow && col == OriginalCol)
                    this.TileView.Opacity = 1;
                else
                    this.TileView.Opacity = Opacity;
            }
        }

        public double Opacity { get; set; }

        public int OriginalRow { set; get; }

        public int OriginalCol { set; get; }

        public double Width { get; set; }

        public double Height { get; set; }

        public bool IsEmptyTile { get; set; }

        public View TileView { private set; get; }

        public static Dictionary<string, string> Locations { get;  } = new Dictionary<string, string>();

        /// <summary>
        /// Covnert all Tiles loactions to string format in order to save it into database column with string type
        /// </summary>
        /// <returns></returns>
        public static string LocationsToString()
        {
            string output = "";
            foreach (KeyValuePair<View, Tile> entry in Dictionary)
            {
                // do something with entry.Value or entry.Key
                string location = entry.Value.OriginalLocation() + ":" + entry.Value.Location();
                if (output == "")
                    output = location;
                else
                    output += "," + location;
            }
            return output;
        }

        /// <summary>
        /// Convert all tile locations from string format to a dictionary, the key is the original location,
        /// and the value is the saved location
        /// </summary>
        /// <param name="locationsInString"></param>
        public static void StringToLocations(string locationsInString)
        {
            if (string.IsNullOrEmpty(locationsInString)) return;
            Locations.Clear();
            string[] locationsArray = locationsInString.Split(',');
            for (int i = 0; i < locationsArray.Length; i++)
            {
                string[] splitedLocation = locationsArray[i].Split(':');
                Locations.Add(splitedLocation[0], splitedLocation[1]);
            }
        }

        public string OriginalLocation()
        {
            return $"{OriginalRow}-{OriginalCol}";
        }

        public string Location()
        {
            return $"{Row}-{Col}";
        }

        public static bool IsSuccess()
        {
            foreach (KeyValuePair<View, Tile> entry in Dictionary)
            {
                if (entry.Value.Row != entry.Value.OriginalRow || entry.Value.Col != entry.Value.OriginalCol)
                    return false;
            }
            return true;
        }
    }
}
