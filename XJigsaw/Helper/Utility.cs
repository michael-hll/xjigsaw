using System;
using XJigsaw.Models;
using Xamarin.Forms;
using System.Collections.Generic;
using System.IO;
using XJigsaw.Views;
using XJigsaw.ViewModels;
using Rg.Plugins.Popup.Extensions;

namespace XJigsaw.Helper
{
    public class Utility
    {
        /* Release configurations */
        public const bool CHECK_POLICY_READ = false;

        public const string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss.fff zzz";
        public const string DATETIME_FORMAT_SHOW = "yyyy-MM-dd hh:mm:ss";
        public const string DATABASE_NAME = "puzzle.db3";
        public const string CURRENT_SOURCE_FILE_NAME = "current_jigsaw_image.jpg";
        public const int STATUS_BAR_MINIMUM_WIDTH = 300;
        public static readonly string LOCAL_APPLICATION_DATA_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static readonly string DATABASE_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "db");
        public static readonly string IMAGE_TEMP_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "image_tmp");
        public static readonly string DATABASE_FULL_NAME = Path.Combine(DATABASE_FOLDER, DATABASE_NAME);

        public static Size GetImageSize(string fileName)
        {
            var imageSize = DependencyService.Get<IImageResource>().GetSize(fileName);
            return imageSize;
        }

        public static string FormatTimeSpan(TimeSpan ts)
        {
            return String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
        }

        public static string GetShortVersion()
        {
            // short version
            return DependencyService.Get<IAppVersionAndBuild>().GetShortVersion();
        }

        public static string GetAppVersion()
        {
            // app version
            return DependencyService.Get<IAppVersionAndBuild>().GetAppVersion();
        }

        public static void CreateEmail(List<string> emailAddresses, List<string> ccs, string subject, string body, string htmlBody)
        {
            DependencyService.Get<IEmailService>().CreateEmail(emailAddresses, ccs, subject, body, htmlBody);
        }

        public static string GetImageResolutionLabel(JigsawPage jigsawPage)
        {
            if (jigsawPage.JigsawLayout.WidthRequest < STATUS_BAR_MINIMUM_WIDTH)
                return "";
            return $"{JigsawPage.CurrentJigsaw.ImageHeight}x{JigsawPage.CurrentJigsaw.ImageWidth}";
        }

        public static string GetLevelLabel(JigsawPage jigsawPage)
        {
            if (jigsawPage.JigsawLayout.WidthRequest < STATUS_BAR_MINIMUM_WIDTH)
                return $"{JigsawPage.CurrentJigsaw.Level}x{JigsawPage.CurrentJigsaw.Level}";
            return BestScoreViewModel.FormatLevelById(JigsawPage.CurrentJigsaw.Level);
        }

        /**
         * Static method to tell if it's iOS
         */
        public static bool IsiOS()
        {
            return Device.RuntimePlatform == Device.iOS;
        }

        public static string GetFileSize(string fileFullName)
        {
            if (!File.Exists(fileFullName))
                return "0.00 B";

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = new FileInfo(fileFullName).Length;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##}{1}", len, sizes[order]);
        }

    }
}
