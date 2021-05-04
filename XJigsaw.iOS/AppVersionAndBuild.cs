using System;
using Foundation;
using Xamarin.Forms;
using XJigsaw.iOS;
using XJigsaw.Models;

[assembly: Dependency(typeof(AppVersionAndBuild))]
namespace XJigsaw.iOS
{
    public class AppVersionAndBuild : IAppVersionAndBuild
    {

        public string GetShortVersion()
        {
            return NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();
        }
        public string GetAppVersion()
        {
            return NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString();
        }
    }
}
