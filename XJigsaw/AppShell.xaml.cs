using System;
using System.Collections.Generic;
using XJigsaw.Models;
using XJigsaw.ViewModels;
using XJigsaw.Views;
using Xamarin.Forms;

namespace XJigsaw
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            tabBars.CurrentItem = jigsawTab;
            HistoryTab = historyTab;
            AboutTab = aboutTab;
            JigsawTab = jigsawTab;
        }

        public Setting JigsawSettings = null;
        public Tab HistoryTab = null;
        public Tab AboutTab = null;
        public Tab JigsawTab = null;
    }
}
