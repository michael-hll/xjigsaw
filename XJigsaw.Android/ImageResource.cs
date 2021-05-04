using System;
using XJigsaw.Droid;
using XJigsaw.Models;
using Xamarin.Forms;
using Android.Graphics;

[assembly: Dependency(typeof(ImageResource))]
namespace XJigsaw.Droid
{
    public class ImageResource : Java.Lang.Object, IImageResource
    {
        public Size GetSize(string fileName)
        {
            using (var opt = new BitmapFactory.Options() { InJustDecodeBounds = true })
            using (BitmapFactory.DecodeFile(fileName, opt))
            {
                return new Size((double)opt.OutWidth, (double)opt.OutHeight);
            }

        }
    }
}
