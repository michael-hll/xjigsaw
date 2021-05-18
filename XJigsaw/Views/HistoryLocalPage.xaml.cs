using System;
using System.Collections.Generic;
using XJigsaw.ViewModels;
using Xamarin.Forms;
using XJigsaw.Models;
using System.IO;
using XJigsaw.Helper;
using Plugin.ImageEdit.Abstractions;
using Plugin.ImageEdit;
using Plugin.Toast;
using System.Threading.Tasks;
using XJigsaw.Resources;

namespace XJigsaw.Views
{
    public partial class HistoryLocalPage : ContentPage
    {
        public HistoryLocalPage()
        {
            InitializeComponent();
            BindingContext = new HistoryLocalViewModel();
            HistoryLocalPageInstance = this;
            this.ProgressBar = this.progressBar;
        }

        public ProgressBar ProgressBar;
        public static int UpdatedJigsawID = 0;

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateImageButtonVisible();
            UpdateSelectionCount();
            UpdateJigsawListItem();

        }

        private async void UpdateJigsawListItem()
        {
            if (UpdatedJigsawID > 0)
            {
                List<Jigsaw> jigsaws = await App.Database.GetJigsawByID(UpdatedJigsawID);
                if (jigsaws.Count > 0)
                {
                    ((HistoryLocalViewModel)BindingContext).UpdateJigsawItem(jigsaws[0]);
                }
                UpdatedJigsawID = 0;
            }
        }

        public static HistoryLocalPage HistoryLocalPageInstance = null;

        void ChooseImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            if (collectionView.SelectedItems.Count == 1)
            {
                JigsawListItem item = (JigsawListItem)collectionView.SelectedItems[0];

                _ = ((HistoryLocalViewModel)BindingContext).SelectJigsawListItemAsync(item);

                CrossToastPopUp.Current.ShowToastMessage(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_History_Local_SelectedNote);
                this.collectionView.SelectedItems.Clear();
                this.UpdateSelectionCount();
            }
        }

        public void NoteNoUpdates()
        {
            CrossToastPopUp.Current.ShowToastMessage(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_History_Loacal_NoUpdates);
        }

        async void RefreshImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            EnableImageButtons(false);
            refreshView.IsEnabled = false;
            await ((HistoryLocalViewModel)BindingContext).RefreshDataAsync();
            UpdateImageButtonVisible();
            EnableImageButtons(true);
            refreshView.IsEnabled = true;
        }

        async void DeleteImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            EnableImageButtons(false);
            var result = await this.DisplayAlert(
                XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Warning,
                XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_ConfirmDeleteCurrent,
                XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Yes,
                XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_No);
            if (result)
            {
                foreach (var item in collectionView.SelectedItems)
                {
                    JigsawListItem jigsawListItem = (JigsawListItem)item;
                    _ = ((HistoryLocalViewModel)BindingContext).DeleteJigsawItemAsync(jigsawListItem);
                }
            }
            this.collectionView.SelectedItems.Clear();
            UpdateImageButtonVisible();
            UpdateSelectionCount();
            EnableImageButtons(true);
        }

        void CollectionView_SelectionChanged(System.Object sender, Xamarin.Forms.SelectionChangedEventArgs e)
        {
            UpdateImageButtonVisible();
            UpdateSelectionCount();
        }

        void UpdateImageButtonVisible()
        {
            this.chooseImageButton.IsVisible = this.collectionView.SelectedItems.Count == 1;
            this.deleteImageButton.IsVisible = this.collectionView.SelectedItems.Count > 0;
        }

        public void UpdateSelectionCount()
        {
            this.labelSelectedCount.Text =
                $"{XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_History_Local_SelectedCount} {this.collectionView.SelectedItems.Count} ";
        }

        public void UpdateDBSize()
        {
            this.labelDBSize.Text =
                $"{XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_History_Loacal_DB_Size} {Utility.GetFileSize(Utility.DATABASE_FULL_NAME)} ";
        }

        public void ClearSelection()
        {
            this.collectionView.SelectedItems.Clear();
        }

        public async void ScrollToLatest()
        {
            if (((HistoryLocalViewModel)BindingContext).JigsawListItems.Count > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(0.1));
                this.collectionView.ScrollTo(0);
            }
        }

        public void EnableImageButtons(bool isEnable)
        {
            this.refreshImageButton.IsEnabled = isEnable;
            this.chooseImageButton.IsEnabled = isEnable;
            this.deleteImageButton.IsEnabled = isEnable;
        }

    }
}
