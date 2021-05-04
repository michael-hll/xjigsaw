using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using XJigsaw.Models;

namespace XJigsaw.ViewModels
{
    public class BestScoreViewModel : BaseViewModel
    {
        public BestScoreViewModel()
        {
        }

        ObservableCollection<Level> levelsHasScore = new ObservableCollection<Level>();
        public ObservableCollection<Level> LevelsHasScore { get { return levelsHasScore; } }
        public static List<Level> Levels = null;

        public static Level GetLevelById(int id)
        {
            foreach (var item in Levels)
            {
                if (item.ID == id)
                    return item;
            }
            return null;
        }

        public static string FormatLevelById(int id)
        {
            if (XJigsaw.Resources.AppResources.Culture.Name.Contains("zh"))
                return $"{GetLevelById(id).Name}({id}x{id})";
            else
                return $"{GetLevelById(id).NameOther}({id}x{id})";
        }
    }
}
