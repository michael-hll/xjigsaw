using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using XJigsaw.Models;
using XJigsaw.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XJigsaw.Resources;

namespace XJigsaw.Views
{
    public partial class AboutPage : ContentPage
    {
        public static ContentPage AboutPageInstance = null;

        public AboutPage()
        {
            InitializeComponent();
            AboutPageInstance = this;

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ((AboutViewModel)this.BindingContext).EmailAddress = labelEmailAdd.Text;

            if (labelEmailAdd.GestureRecognizers.Count == 0)
            {
                labelEmailAdd.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = ((AboutViewModel)this.BindingContext).MailToCommand
                });
            }

            // Get versions
            if (AppResources.Culture.Name.Contains("zh"))
                ((AboutViewModel)this.BindingContext).UserLevel = BestScoreViewModel.GetLevelById(AboutViewModel.User.Level).Name;
            else
                ((AboutViewModel)this.BindingContext).UserLevel = BestScoreViewModel.GetLevelById(AboutViewModel.User.Level).NameOther;

        }
    }
}