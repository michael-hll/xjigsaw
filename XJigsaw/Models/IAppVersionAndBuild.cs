using System;
namespace XJigsaw.Models
{
    /* https://www.c-sharpcorner.com/article/how-to-get-app-version-and-build-number-in-xamarin-forms/ */

    public interface IAppVersionAndBuild
    {
        string GetShortVersion();
        string GetAppVersion();
    }
}
