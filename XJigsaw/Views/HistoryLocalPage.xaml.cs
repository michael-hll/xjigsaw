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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            updateImageButtonVisible();
            updateSelectionCount();
        }

        public static HistoryLocalPage HistoryLocalPageInstance = null;

        void chooseImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            if (collectionView.SelectedItems.Count == 1)
            {
                JigsawListItem item = (JigsawListItem)collectionView.SelectedItems[0];

                _ = ((HistoryLocalViewModel)BindingContext).SelectJigsawListItemAsync(item);

                CrossToastPopUp.Current.ShowToastMessage(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_History_Loacal_SelectedNote);
                this.collectionView.SelectedItems.Clear();
                this.updateSelectionCount();
            }
        }

        async void refreshImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            enableImageButtons(false);
            await ((HistoryLocalViewModel)BindingContext).GetJigsawsListAysnc();
            updateImageButtonVisible();
            updateSelectionCount();
            enableImageButtons(true);
        }

        async void deleteImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            enableImageButtons(false);
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
            updateImageButtonVisible();
            updateSelectionCount();
            enableImageButtons(true);
        }

        void collectionView_SelectionChanged(System.Object sender, Xamarin.Forms.SelectionChangedEventArgs e)
        {
            updateImageButtonVisible();
            updateSelectionCount();
        }

        void updateImageButtonVisible()
        {
            this.chooseImageButton.IsVisible = this.collectionView.SelectedItems.Count == 1;
            this.deleteImageButton.IsVisible = this.collectionView.SelectedItems.Count > 0;
        }

        public void updateSelectionCount()
        {
            this.labelSelectedCount.Text =
                $"{XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_History_Loacal_SelectedCount} {this.collectionView.SelectedItems.Count}";
        }

        public void clearSelection()
        {
            this.collectionView.SelectedItems.Clear();
        }

        public void scrollToLatest()
        {
            this.collectionView.ScrollTo(0);
        }

        public void enableImageButtons(bool isEnable)
        {
            this.refreshImageButton.IsEnabled = isEnable;
            this.chooseImageButton.IsEnabled = isEnable;
            this.deleteImageButton.IsEnabled = isEnable;
        }

    }
}
