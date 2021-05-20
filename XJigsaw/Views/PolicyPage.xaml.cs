using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;
using XJigsaw.Models;

namespace XJigsaw.Views
{
    public partial class PolicyPage : Rg.Plugins.Popup.Pages.PopupPage
    {
        public PolicyPage()
        {
            InitializeComponent();
            this.ParentContainer.HeightRequest = Application.Current.MainPage.Height -
                this.ParentContainer.Padding.Top - this.ParentContainer.Padding.Bottom;
            this.BindingContext = App.ShellInstance.JigsawSettings;

            if (App.ShellInstance.JigsawSettings.IsReadPrivacy)
            {
                this.agreeCheckBox.IsEnabled = false;
                this.agreeCheckBox.IsChecked = true;
                this.agreeCheckBox.Color = Color.Gray;
                this.CloseButton.IsEnabled = true;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
        }

        // ### Methods for supporting animations in your popup page ###

        // Invoked before an animation appearing
        protected override void OnAppearingAnimationBegin()
        {
            base.OnAppearingAnimationBegin();
        }

        // Invoked after an animation appearing
        protected override void OnAppearingAnimationEnd()
        {
            base.OnAppearingAnimationEnd();
        }

        // Invoked before an animation disappearing
        protected override void OnDisappearingAnimationBegin()
        {
            base.OnDisappearingAnimationBegin();
        }

        // Invoked after an animation disappearing
        protected override void OnDisappearingAnimationEnd()
        {
            base.OnDisappearingAnimationEnd();
        }

        protected override Task OnAppearingAnimationBeginAsync()
        {
            return base.OnAppearingAnimationBeginAsync();
        }

        protected override Task OnAppearingAnimationEndAsync()
        {
            return base.OnAppearingAnimationEndAsync();
        }

        protected override Task OnDisappearingAnimationBeginAsync()
        {
            return base.OnDisappearingAnimationBeginAsync();
        }

        protected override Task OnDisappearingAnimationEndAsync()
        {
            return base.OnDisappearingAnimationEndAsync();
        }

        // ### Overrided methods which can prevent closing a popup page ###

        // Invoked when a hardware back button is pressed
        protected override bool OnBackButtonPressed()
        {
            // Return true if you don't want to close this popup page when a back button is pressed
            return base.OnBackButtonPressed();
        }

        // Invoked when background is clicked
        protected override bool OnBackgroundClicked()
        {
            // Return false if you don't want to close this popup page when a background of the popup page is clicked
            //return base.OnBackgroundClicked();
            return false;
        }

        async void CloseButton_Clicked(System.Object sender, System.EventArgs e)
        {
            if (!App.ShellInstance.JigsawSettings.IsReadPrivacy)
            {
                App.ShellInstance.JigsawSettings.IsReadPrivacy = true;
                await App.Database.SaveSettingAsync(App.ShellInstance.JigsawSettings);
            }
            await Navigation.PopPopupAsync();
        }

        void agreeCheckBox_CheckedChanged(System.Object sender, Xamarin.Forms.CheckedChangedEventArgs e)
        {
            this.CloseButton.IsEnabled = this.agreeCheckBox.IsChecked;
        }
    }
}
