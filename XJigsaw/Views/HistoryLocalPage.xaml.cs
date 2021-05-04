using System;
using System.Collections.Generic;
using XJigsaw.ViewModels;
using Xamarin.Forms;

namespace XJigsaw.Views
{
    public partial class HistoryLocalPage : ContentPage
    {
        public HistoryLocalPage()
        {
            InitializeComponent();
            BindingContext = new HistoryLocalViewModel();
            HistoryLocalPageInstance = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        public static HistoryLocalPage HistoryLocalPageInstance = null;
    }
}
