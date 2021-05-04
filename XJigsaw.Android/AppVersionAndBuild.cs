using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using XJigsaw.Droid;
using XJigsaw.Models;

[assembly: Dependency(typeof(AppVersionAndBuild))]
namespace XJigsaw.Droid
{
    public class AppVersionAndBuild : IAppVersionAndBuild
    {
        public string GetShortVersion()
        {
            return AppInfo.VersionString;
        }

        public string GetAppVersion()
        {
            return AppInfo.BuildString;
        }
    }
}
