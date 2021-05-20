using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using XJigsaw.Models;
using XJigsaw.Views;
using Xamarin.Forms;
using XJigsaw.Resources;
using XJigsaw.Helper;
using Xamarin.Essentials;

namespace XJigsaw.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = Resources.AppResources.XJigsaw_Jigsaw_Title;
            DeleteDatabaseDataCommand = new Command(async () =>
            {
                var result = await AboutPage.AboutPageInstance.DisplayAlert(
                    Resources.AppResources.XJigsaw_Jigsaw_Warning,
                    Resources.AppResources.XJigsaw_Jigsaw_ConfirmDeleteAllData,
                    Resources.AppResources.XJigsaw_Jigsaw_Yes,
                    Resources.AppResources.XJigsaw_Jigsaw_No);
                if (result)
                {
                    //await App.Database.DeleteAllSettings();
                    //await App.Database.DeleteAllJigsaws();
                    File.Delete(Path.Combine(Utility.DATABASE_FOLDER, Utility.DATABASE_NAME));
                }
            });

            MailToCommand = new Command(() =>
            {
                try
                {
                    Launcher.OpenAsync(new Uri($"mailto:{EmailAddress}?subject=X Jigsaw {AppVersion}"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Sent eamil exception: {ex.Message}");
                }
            });

            PropertyChanged += AboutViewModel_PropertyChanged;
        }



        private void AboutViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        public static User User = null;

        public string UserName
        {
            get
            {
                if (AppResources.Culture.Name.Contains("zh"))
                    return $"{Resources.AppResources.XJigsaw_Jigsaw_UserName}: {User.Name}";
                else
                    return $"{Resources.AppResources.XJigsaw_Jigsaw_UserName}: {User.NameOther}";
            }
        }

        string emailAddress;
        public string EmailAddress
        {
            get
            {
                return emailAddress;
            }
            set
            {
                emailAddress = value;
            }
        }

        string userLevel = "";
        public string UserLevel
        {
            get
            {
                return $"{Resources.AppResources.XJigsaw_Jigsaw_UserLevel}: {userLevel}";
            }
            set
            {
                userLevel = value;
                OnPropertyChanged();
            }
        }



        public string AppVersion
        {
            get
            {
                return $"{Utility.GetShortVersion()} ({Resources.AppResources.XJigsaw_Jigsaw_About_Build} {Utility.GetAppVersion()})";
            }
        }

        public ICommand DeleteDatabaseDataCommand { get; }
        public ICommand MailToCommand { get; }
    }
}