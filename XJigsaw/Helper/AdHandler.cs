using System;
using MarcTron.Plugin;
using Plugin.Toast;
using XJigsaw.Views;
using Xamarin.Forms;

namespace XJigsaw.Helper
{
    public class AdHandler
    {
        readonly JigsawPage jigsawPage;
        public AdHandler(ContentPage page)
        {
            jigsawPage = (JigsawPage)page;

            CrossMTAdmob.Current.OnRewardedVideoAdLoaded += Current_OnRewardedVideoAdLoaded;
            CrossMTAdmob.Current.OnRewardedVideoAdClosed += Current_OnRewardedVideoAdClosed;
            CrossMTAdmob.Current.OnRewardedVideoAdCompleted += Current_OnRewardedVideoAdCompleted;
            CrossMTAdmob.Current.OnRewardedVideoAdFailedToLoad += Current_OnRewardedVideoAdFailedToLoad;
            CrossMTAdmob.Current.OnRewardedVideoAdLeftApplication += Current_OnRewardedVideoAdLeftApplication;
            CrossMTAdmob.Current.OnRewardedVideoStarted += Current_OnRewardedVideoStarted;
            CrossMTAdmob.Current.OnRewardedVideoAdOpened += Current_OnRewardedVideoAdOpened;
            CrossMTAdmob.Current.OnRewarded += Current_OnRewarded;
        }

        private void Current_OnRewarded(object sender, MarcTron.Plugin.CustomEventArgs.MTEventArgs e)
        {
            IsAdPlayingCompleted = true;
        }

        public bool IsAdPlayingCompleted { get; set; }

        private void Current_OnRewardedVideoStarted(object sender, EventArgs e)
        {
            jigsawPage.NotifyPlayAdStatus(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_AD_Playing);
        }

        private void Current_OnRewardedVideoAdLoaded(object sender, EventArgs e)
        {
            jigsawPage.NotifyPlayAdStatus(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_AD_Loading);
            CrossMTAdmob.Current.ShowRewardedVideo();
            IsAdPlayingCompleted = false;
        }

        private void Current_OnRewardedVideoAdLeftApplication(object sender, EventArgs e)
        {
            jigsawPage.NotifyPlayAdStatus(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_AD_UserLeaving);
        }

        private void Current_OnRewardedVideoAdFailedToLoad(object sender, MarcTron.Plugin.CustomEventArgs.MTEventArgs e)
        {
            jigsawPage.SaveCurrentJigsaw(IsAdPlayingCompleted, true, XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_AD_FailedLoading);
            jigsawPage.NotifyPlayAdStatus("");
        }

        private void Current_OnRewardedVideoAdCompleted(object sender, EventArgs e)
        {
            IsAdPlayingCompleted = true;
            jigsawPage.NotifyPlayAdStatus("");
        }

        private void Current_OnRewardedVideoAdClosed(object sender, EventArgs e)
        {
            jigsawPage.SaveCurrentJigsaw(IsAdPlayingCompleted);
            jigsawPage.NotifyPlayAdStatus("");
        }

        private void Current_OnRewardedVideoAdOpened(object sender, EventArgs e)
        {
            jigsawPage.NotifyPlayAdStatus(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_AD_StartPlaying);
        }

        public void PlayAds()
        {
            CrossMTAdmob.Current.LoadRewardedVideo("ca-app-pub-2979719193529190/4555456327");
            jigsawPage.NotifyPlayAdStatus(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_AD_StartLoading);
        }
    }
}
