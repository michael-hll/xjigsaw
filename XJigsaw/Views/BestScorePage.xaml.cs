using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using XJigsaw.Models;
using XJigsaw.ViewModels;

namespace XJigsaw.Views
{
    public partial class BestScorePage : ContentPage
    {
        BestScoreViewModel model = null;

        public BestScorePage()
        {
            InitializeComponent();
            model = (BestScoreViewModel)this.BindingContext;
            this.listView.ItemsSource = model.LevelsHasScore;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ObservableCollection<Level> levelsHasScore = model.LevelsHasScore;
            levelsHasScore.Clear();
            foreach (var level in BestScoreViewModel.Levels)
            {
                if (level.BestScore > 0)
                {
                    levelsHasScore.Add(level);
                }
            }
            this.listView.ItemsSource = levelsHasScore;
        }
    }
}
