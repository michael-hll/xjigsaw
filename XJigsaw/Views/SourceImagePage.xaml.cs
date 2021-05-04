using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace XJigsaw.Views
{
    public partial class SourceImagePage : ContentPage
    {
        public SourceImagePage()
        {
            InitializeComponent();
        }

        public static Image SourceImage = new Image();

        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.sourceImage.Source = SourceImage.Source;
        }
    }
}
