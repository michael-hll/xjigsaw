using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XJigsaw.Views;
using XJigsaw.Models;
using System.IO;
using System.Collections.Generic;
using Plugin.ImageEdit.Abstractions;
using Plugin.ImageEdit;
using System.Threading.Tasks;
using XJigsaw.ViewModels;
using XJigsaw.Helper;
using System.Threading;
using System.Globalization;
using XJigsaw.Resources;

namespace XJigsaw
{
    public partial class App : Application
    {
        public static bool IsBackground = false;

        static Database database;

        public static Database Database
        {
            get
            {
                if (database == null)
                {
                    database = new Database(Path.Combine(Utility.DATABASE_FOLDER, Utility.DATABASE_NAME));
                }
                return database;
            }
        }

        public App()
        {
            //Console.WriteLine($"Current app location folder: {Utility.LOCAL_APPLICATION_DATA_FOLDER}");
            //Console.WriteLine($"image folder: {Utility.IMAGE_TEMP_FOLDER}");
            //Console.WriteLine($"database folder: {Utility.DATABASE_FOLDER}");
            if (!Directory.Exists(Utility.DATABASE_FOLDER))
                Directory.CreateDirectory(Utility.DATABASE_FOLDER);
            if (!Directory.Exists(Utility.IMAGE_TEMP_FOLDER))
                Directory.CreateDirectory(Utility.IMAGE_TEMP_FOLDER);
            else
            {
                Directory.Delete(Utility.IMAGE_TEMP_FOLDER, true);
                Directory.CreateDirectory(Utility.IMAGE_TEMP_FOLDER);
            }

            // Load localizations
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
            AppResources.Culture = Thread.CurrentThread.CurrentUICulture;

            InitializeComponent();

            //File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "puzzle.db3"));

            MainPage = new AppShell();
            ShellInstance = (AppShell)MainPage;
            // Load latest jigsaw and settings
            Task.Run(() => LoadData()).Wait();
        }

        public static AppShell ShellInstance;

        protected override void OnStart()
        {
            IsBackground = false;
            base.OnStart();
            //Console.WriteLine("OnStart");
        }

        protected override void OnSleep()
        {
            IsBackground = true;
            base.OnSleep();
            //Console.WriteLine("OnSleep");
        }

        protected override void OnResume()
        {
            IsBackground = false;
            base.OnResume();
            //Console.WriteLine("OnResume");
        }

        async Task LoadData()
        {
            //Load versions
            List<JigsawVersion> versions = await App.Database.GetJigsawVersionAsync();
            if (versions.Count > 0)
            {
                //Console.WriteLine($"Stored Appversion:{ versions[0].AppVersion }, short version: {versions[0].ShortVersion}");
                bool isNewVersionCome = false;
                if (Utility.GetAppVersion() != versions[0].AppVersion)
                {
                    versions[0].AppVersion = Utility.GetAppVersion();
                    isNewVersionCome = true;
                }
                if (Utility.GetShortVersion() != versions[0].ShortVersion)
                {
                    versions[0].ShortVersion = Utility.GetShortVersion();
                    isNewVersionCome = true;
                }
                if (isNewVersionCome)
                    await App.Database.SaveJigsawVersionAsync(versions[0]);
            }
            else
            {
                JigsawVersion version = new JigsawVersion();
                version.AppVersion = Utility.GetAppVersion();
                version.ShortVersion = Utility.GetShortVersion();
                version.UpdateDateTime = DateTime.Now.ToString(Utility.DATETIME_FORMAT);
                await App.Database.SaveJigsawVersionAsync(version);
            }

            // Load settings
            List<Setting> settings = await App.Database.GetSettingAsync();
            if (settings.Count > 0)
            {
                App.ShellInstance.JigsawSettings = settings[0];
            }
            else
            {
                App.ShellInstance.JigsawSettings = new Setting();
                await App.Database.SaveSettingAsync(App.ShellInstance.JigsawSettings);
            }

            // Testing data
            // App.ShellInstance.JigsawSettings.IsReadPrivacy = false;

            // Load Users
            //await App.Database.DeleteAllUsers();
            List<User> users = await App.Database.GetAllUsersAsync();
            if (users.Count > 0)
            {
                AboutViewModel.User = users[0];
            }
            else
            {
                AboutViewModel.User = new User
                {
                    NameOther = "Default User",
                    Name = "默认用户",
                    Level = 2,
                    CreatedDateTime = DateTime.Now.ToString(Utility.DATETIME_FORMAT)
                };
                await App.Database.InsertUserAsync(AboutViewModel.User);
            }

            // Load latest jigsaw            
            List<Jigsaw> jigsaws;
            if (App.ShellInstance.JigsawSettings.CurrentJigsawId > 0)
                jigsaws = await App.Database.GetJigsawByID(App.ShellInstance.JigsawSettings.CurrentJigsawId);
            else
            {
                jigsaws = await App.Database.GetLatestJigsawsAsync(1);
            }

            if (jigsaws.Count > 0)
            {
                JigsawPage.CurrentJigsaw = jigsaws[0];

                var fileFullName = Path.Combine(Utility.IMAGE_TEMP_FOLDER, Utility.CURRENT_SOURCE_FILE_NAME);
                IEditableImage imageEdit = await CrossImageEdit.Current.CreateImageAsync(JigsawPage.CurrentJigsaw.Bytes);
                File.WriteAllBytes(fileFullName, imageEdit.ToJpeg(100));
                SourceImagePage.SourceImage.Source = ImageSource.FromFile(fileFullName);
            }
            else
            {
                JigsawPage.CurrentJigsaw = new Jigsaw();
                JigsawPage.CurrentJigsaw.IsFreshNew = true;
            }
            JigsawPage.CurrentJigsaw.IsLoaded = true;
            JigsawPage.CurrentJigsaw.IsSelected = false;
            JigsawPage.CurrentJigsaw.IsApplied = false;
            JigsawPage.CurrentJigsaw.IsTilePositionInitial = true;

            // Get all level details
            //await App.Database.DeleteAllLevels();
            BestScoreViewModel.Levels = await GetAllLevels();
        }

        async Task<List<Level>> GetAllLevels()
        {
            List<Level> existings = await App.Database.GetAllLevelsAsync();
            try
            {
                if (existings.Count == 0)
                {
                    existings = GenerateInitLevels();
                    await App.Database.InsertAllLevelAsync(existings);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return existings;
        }

        List<Level> GenerateInitLevels()
        {
            List<Level> levels = new List<Level>
            {
                new Level{ ID = 2, Name = "草民", NameOther = "Level 2"},
                new Level{ ID = 3, Name = "童生 - 县", NameOther = "Level 3"},
                new Level{ ID = 4, Name = "童生 - 府", NameOther = "Level 4"},
                new Level{ ID = 5, Name = "童生 - 院", NameOther = "Level 5"},
                new Level{ ID = 6, Name = "秀才", NameOther = "Level 6"},
                new Level{ ID = 7, Name = "举人", NameOther = "Level 7"},
                new Level{ ID = 8, Name = "贡士", NameOther = "Level 8"},
                new Level{ ID = 9, Name = "进士", NameOther = "Level 9"},
                new Level{ ID = 10, Name = "探花", NameOther = "Level 10"},
                new Level{ ID = 11, Name = "榜眼", NameOther = "Level 11"},
                new Level{ ID = 12, Name = "状元", NameOther = "Level 12"},
                new Level{ ID = 13, Name = "县尉", NameOther = "Level 13"},
                new Level{ ID = 14, Name = "司马", NameOther = "Level 14"},
                new Level{ ID = 15, Name = "县令", NameOther = "Level 15"},
                new Level{ ID = 16, Name = "京府判官", NameOther = "Level 16"},
                new Level{ ID = 17, Name = "监察御史", NameOther = "Level 17"},
                new Level{ ID = 18, Name = "大夫", NameOther = "Level 18"},
                new Level{ ID = 19, Name = "侍御史", NameOther = "Level 19"},
                new Level{ ID = 20, Name = "司郎中", NameOther = "Level 20"},
                new Level{ ID = 21, Name = "防御使", NameOther = "Level 21"},
                new Level{ ID = 22, Name = "副都指挥使", NameOther = "Level 22"},
                new Level{ ID = 23, Name = "少府", NameOther = "Level 23"},
                new Level{ ID = 24, Name = "都指挥使", NameOther = "Level 24"},
                new Level{ ID = 25, Name = "中丞", NameOther = "Level 25"},
                new Level{ ID = 26, Name = "翰林学士", NameOther = "Level 26"},
                new Level{ ID = 27, Name = "尚书", NameOther = "Level 27"},
                new Level{ ID = 28, Name = "太尉", NameOther = "Level 28"},
                new Level{ ID = 29, Name = "枢密使", NameOther = "Level 29"},
                new Level{ ID = 30, Name = "太师", NameOther = "Level 30"},
                new Level{ ID = 31, Name = "皇帝", NameOther = "Level 31"},
                new Level{ ID = 32, Name = "神", NameOther = "Level 32"}
            };
            return levels;

            /*  2: 草民
                3: 童生 - 县
                4: 童生 - 府
                5: 童生 - 院
                6: 秀才 
                7: 举人
                8: 贡士
                9: 进士
                10: 探花
                11: 榜眼
                12: 状元
                13: 县尉 - 从九品
                14: 司马 - 正九品
                15: 县令 - 从八品
                16: 京府判官 - 正八品
                17: 监察御史 - 从七品
                18: 大夫 - 正七品
                19: 侍御史 - 从六品
                20: 司郎中 - 正六品
                21: 防御使 - 从五品
                22: 副都指挥使 - 正五品
                23: 少府 - 从四品
                24: 殿前副都指挥使 - 正四品
                25: 中丞 - 从三品
                26: 翰林学士 - 正三品
                27: 尚书 - 从二品
                28: 太尉 - 正二品
                29: 枢密使 - 从一品
                30: 太师 - 正一品
                31: 皇帝 
                32: 神 */
        }
    }
}
