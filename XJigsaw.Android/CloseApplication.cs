using System;
using Android.App;
using Xamarin.Forms;
using XJigsaw.Models;

[assembly: Xamarin.Forms.Dependency(typeof(ICloseApplication))]
namespace XJigsaw.Droid
{
    public class CloseApplication : ICloseApplication
    {
        public void CloseApp()
        {
            //Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
            var activity = (Activity)Forms.Context;
            activity.FinishAffinity();

        }
    }
}
