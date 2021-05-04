using XJigsaw.iOS;
using XJigsaw.Models;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ImageResource))]
namespace XJigsaw.iOS
{
    public class ImageResource : IImageResource
    {
        public Size GetSize(string fileName)
        {
            UIImage image = UIImage.FromFile(fileName);
            return new Size((double)image.Size.Width, (double)image.Size.Height);
        }
    }
}
