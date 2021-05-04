using System;
using System.Collections.Generic;
using XJigsaw.Models;
using XJigsaw.ViewModels;
using Xamarin.Forms;
using XJigsaw.Resources;

namespace XJigsaw.Views
{
    public partial class SettingPage : ContentPage
    {
        public SettingPage()
        {
            InitializeComponent();
            BindingContext = App.ShellInstance.JigsawSettings;
        }

        void stepperLevel_ValueChanged(object sender, EventArgs args)
        {
            var stepper = (Stepper)sender;
            labelLevelValue.Text = BestScoreViewModel.FormatLevelById((int)stepper.Value);
            App.ShellInstance.JigsawSettings.Level = (int)stepper.Value;
            //Console.WriteLine($"Level: {stepper.Value}");
            App.Database.SaveSettingAsync(App.ShellInstance.JigsawSettings).Wait();
        }

        void pickerRatio_SelectedIndexChanged(object sender, EventArgs args)
        {
            var pickerRatio = (Picker)sender;
            //Console.WriteLine($"Ratio item: {pickerRatio.SelectedItem}");
            string selectedRatio = (string)pickerRatio.SelectedItem;
            if (selectedRatio == "自动" || selectedRatio.ToUpper() == "AUTO")
            {
                App.ShellInstance.JigsawSettings.ImageRatio = "自动";
                App.ShellInstance.JigsawSettings.ImageRatioOther = "Auto";
                double height = JigsawPage.ContainerHeight - JigsawPage.ToolBarFrame.Height - JigsawPage.StatusBarFrame.Height;
                double width = JigsawPage.ContainerWidth;
                App.ShellInstance.JigsawSettings.AspectRatio_X = (int)width;
                App.ShellInstance.JigsawSettings.AspectRatio_Y = (int)height;
            }
            else
            {
                App.ShellInstance.JigsawSettings.ImageRatio = selectedRatio;
                App.ShellInstance.JigsawSettings.ImageRatioOther = selectedRatio;
                App.ShellInstance.JigsawSettings.AspectRatio_X = int.Parse(selectedRatio.Split(':')[1].Trim());
                App.ShellInstance.JigsawSettings.AspectRatio_Y = int.Parse(selectedRatio.Split(':')[0].Trim());
            }
            App.Database.SaveSettingAsync(App.ShellInstance.JigsawSettings).Wait();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (pickerRatio.ItemsSource == null)
            {
                var ratioList = new List<string>();
                ratioList.Add("1:1");
                ratioList.Add("4:3");
                ratioList.Add("3:2");
                ratioList.Add("16:10");
                ratioList.Add("16:9");
                ratioList.Add(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Zidong);
                pickerRatio.ItemsSource = ratioList;
            }
            if (AboutViewModel.User.Level == 2)
                stepperLevel.IsEnabled = false;
            else
            {
                stepperLevel.IsEnabled = true;
                stepperLevel.Maximum = AboutViewModel.User.Level + 1;
                if (stepperLevel.Maximum > 32)
                    stepperLevel.Maximum = 32;
            }
            /* testing code */
            //stepperLevel.IsEnabled = true;
            //stepperLevel.Maximum = 32;
            /* testing code */
            stepperLevel.Value = App.ShellInstance.JigsawSettings.Level;
            labelLevelValue.Text = BestScoreViewModel.FormatLevelById((int)stepperLevel.Value);
            if (App.ShellInstance.JigsawSettings.ImageRatio == "自动" || App.ShellInstance.JigsawSettings.ImageRatio == "Auto")
                pickerRatio.SelectedItem = App.ShellInstance.JigsawSettings.GetCorrectImageRatio(AppResources.Culture.Name);
            else
                pickerRatio.SelectedItem = App.ShellInstance.JigsawSettings.ImageRatio;
            stepperOpacity.Value = App.ShellInstance.JigsawSettings.Opacity;
            labelOpacityValue.Text = $"{string.Format("{0:N2}", stepperOpacity.Value)}";
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        void stepperOpacity_ValueChanged(System.Object sender, Xamarin.Forms.ValueChangedEventArgs e)
        {
            var stepper = (Stepper)sender;
            labelOpacityValue.Text = $"{string.Format("{0:N2}", stepper.Value)}";
            App.ShellInstance.JigsawSettings.Opacity = stepper.Value;
            Console.WriteLine($"Opactity: {stepper.Value}");
            App.Database.SaveSettingAsync(App.ShellInstance.JigsawSettings).Wait();
        }

        void switchSound_Toggled(System.Object sender, Xamarin.Forms.ToggledEventArgs e)
        {
            App.Database.SaveSettingAsync(App.ShellInstance.JigsawSettings).Wait();
        }
    }
}
