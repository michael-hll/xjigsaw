using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.ImageEdit;
using Plugin.ImageEdit.Abstractions;
using XJigsaw.Helper;
using XJigsaw.Models;
using XJigsaw.Views;
using Xamarin.Forms;
using XJigsaw.Resources;

namespace XJigsaw.ViewModels
{
    public class HistoryLocalViewModel : BaseViewModel
    {
        const int MaximumItemCount = 50;
        const int PageSize = 10;
        bool isRefreshing;

        public ObservableCollection<JigsawListItem> JigsawListItems { get; private set; } = new ObservableCollection<JigsawListItem>();
        public HashSet<int> JigsawIDs = new HashSet<int>();
        public int MaxID { get; set; } = 0;

        public ICommand LoadMoreDataCommand => new Command(GetNextPageOfData);
        public ICommand RefreshCommand => new Command(async () => await RefreshDataAsync());
        public ICommand SelectCommand => new Command<JigsawListItem>(async (item) => await SelectJigsawListItemAsync(item));
        public ICommand DeleteCommand => new Command<JigsawListItem>(async (item) => await DeleteJigsawItemAsync(item));

        string itemCount = "";
        public string ItemCount
        {
            get
            {
                return itemCount;
            }
            set
            {
                itemCount = value;
                OnPropertyChanged();
            }
        }

        public string LabelInfor
        {
            get
            {
                return $"{Resources.AppResources.XJigsaw_Jigsaw_History_Loacal_Infor}";
            }
        }

        public async Task SelectJigsawListItemAsync(JigsawListItem item)
        {
            List<Jigsaw> jigsaws = await App.Database.GetJigsawByID(item.ID);
            JigsawPage.CurrentJigsaw = jigsaws[0];
            JigsawPage.CurrentJigsaw.IsLoaded = false;
            JigsawPage.CurrentJigsaw.IsSelected = true;
            JigsawPage.CurrentJigsaw.IsApplied = false;
            JigsawPage.CurrentJigsaw.IsTilePositionInitial = true;
            JigsawPage.CurrentJigsaw.IsDeleted = false;
            JigsawPage.CurrentJigsaw.IsSelectedItemNotProcessed = true;
            App.ShellInstance.JigsawSettings.CurrentJigsawId = JigsawPage.CurrentJigsaw.ID;
            await App.Database.SaveSettingAsync(App.ShellInstance.JigsawSettings);
            var fileFullName = Path.Combine(Utility.IMAGE_TEMP_FOLDER, Utility.CURRENT_SOURCE_FILE_NAME);
            IEditableImage imageEdit = await CrossImageEdit.Current.CreateImageAsync(JigsawPage.CurrentJigsaw.Bytes);
            File.WriteAllBytes(fileFullName, imageEdit.ToJpeg(100));
            SourceImagePage.SourceImage.Source = ImageSource.FromFile(fileFullName);
        }

        public async Task DeleteJigsawItemAsync(JigsawListItem item)
        {
            await App.Database.DeleteJigsawById(item.ID);
            JigsawListItems.Remove(item);
            ItemCount = $"{Resources.AppResources.XJigsaw_Jigsaw_HistoryTotalCount}: {JigsawListItems.Count}";
            HistoryLocalPage.HistoryLocalPageInstance.UpdateDBSize();

            if (JigsawPage.CurrentJigsaw.ID == item.ID)
            {
                JigsawPage.CurrentJigsaw.ID = 0;
                JigsawPage.CurrentJigsaw.IsTilePositionInitial = true;
                JigsawPage.CurrentJigsaw.IsDeleted = true;
                App.ShellInstance.JigsawSettings.CurrentJigsawId = 0;
                await App.Database.SaveSettingAsync(App.ShellInstance.JigsawSettings);
            }
        }

        public HistoryLocalViewModel()
        {
            Title = Resources.AppResources.XJigsaw_Jigsaw_Title;
            Task.Run(async () => await GetJigsawsListAysnc()).Wait();
            PropertyChanged += HistoryLocalViewModel_PropertyChanged;
        }

        private void HistoryLocalViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public Task GetJigsawsListAysnc()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(0.1));
                HistoryLocalPage.HistoryLocalPageInstance.EnableImageButtons(false);
                List<Jigsaw> jigsaws = await App.Database.GetJigsawsAsync(MaxID);
                ProgressBar progressBar = HistoryLocalPage.HistoryLocalPageInstance.ProgressBar;
                progressBar.IsVisible = jigsaws.Count > 0;
                int i = 0;
                foreach (var jigsaw in jigsaws)
                {
                    i++;
                    await progressBar.ProgressTo(i / (double)jigsaws.Count, 1, Easing.Linear);
                    if (JigsawIDs.Contains(jigsaw.ID)) continue;
                    JigsawListItem item = JigsawToJigsawListItem(jigsaw);
                    JigsawListItems.Insert(0, item);
                    JigsawIDs.Add(jigsaw.ID);
                    if (jigsaw.ID > MaxID) MaxID = jigsaw.ID;
                }
                progressBar.IsVisible = false;
                ItemCount = $"{AppResources.XJigsaw_Jigsaw_HistoryTotalCount}: {JigsawListItems.Count}";
                HistoryLocalPage.HistoryLocalPageInstance.ClearSelection();
                HistoryLocalPage.HistoryLocalPageInstance.UpdateSelectionCount();
                HistoryLocalPage.HistoryLocalPageInstance.UpdateDBSize();
                HistoryLocalPage.HistoryLocalPageInstance.ScrollToLatest();
                HistoryLocalPage.HistoryLocalPageInstance.EnableImageButtons(true);
                if (jigsaws.Count == 0)
                    HistoryLocalPage.HistoryLocalPageInstance.NoteNoUpdates();
            });
            return Task.CompletedTask;
        }

        JigsawListItem JigsawToJigsawListItem(Jigsaw jigsaw)
        {
            JigsawListItem item = new JigsawListItem(jigsaw.ID, jigsaw.Name, jigsaw.BytesSmall);
            // udpate
            item.ImageSize = $"{AppResources.XJigsaw_Jigsaw_PictureSize}: {jigsaw.ImageHeight}x{jigsaw.ImageWidth}";
            item.Level = $"{AppResources.XJigsaw_Jigsaw_PictureLevel}: {BestScoreViewModel.FormatLevelById(jigsaw.Level)}";
            item.Steps = $"{AppResources.XJigsaw_Jigsaw_PictureSteps}: {jigsaw.Steps}";
            item.ImageRatio = $"{AppResources.XJigsaw_Jigsaw_PictureRatio}: {jigsaw.ImageRatio}";
            item.CreatedDateTime = $"{AppResources.XJigsaw_Jigsaw_PictureCreationDate}: {DateTime.Parse(jigsaw.CreatedDateTime).ToString(Utility.DATETIME_FORMAT_SHOW)}";
            return item;
        }

        public void UpdateJigsawItem(Jigsaw jigsaw)
        {
            int index = -1;
            foreach (JigsawListItem item in JigsawListItems)
            {
                if (item.ID == jigsaw.ID)
                {
                    index = JigsawListItems.IndexOf(item);
                    break;
                }
            }
            if (index >= 0)
            {
                JigsawListItems.RemoveAt(index);
                JigsawListItem item = JigsawToJigsawListItem(jigsaw);
                JigsawListItems.Insert(index, item);
                if (index == 0) HistoryLocalPage.HistoryLocalPageInstance.ScrollToLatest();
            }
        }

        public bool IsRefreshing
        {
            get { return isRefreshing; }
            set
            {
                isRefreshing = value;
                OnPropertyChanged();
            }
        }

        void GetNextPageOfData()
        {
        }

        public async Task RefreshDataAsync()
        {
            IsRefreshing = true;
            await GetJigsawsListAysnc();
            //TODO: GetNextPageOfData();
            IsRefreshing = false;
        }
    }
}
