using System;
using System.IO;
using System.Threading.Tasks;
using Plugin.ImageEdit;
using Plugin.ImageEdit.Abstractions;
using Xamarin.Forms;
using XJigsaw.Helper;

namespace XJigsaw.Models
{
    public class JigsawListItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public byte[] ImageBytes { get; set; }
        public string ImageSize { get; set; }
        public string Level { get; set; }
        public string Steps { get; set; }
        public string ImageRatio { get; set; }
        public string CreatedDateTime { get; set; }

        private ImageSource imageSource = null;
        public ImageSource ImageSource
        {
            get
            {
                if (imageSource == null)
                {
                    var stream = new MemoryStream(ImageBytes);
                    imageSource = ImageSource.FromStream(() => stream);
                }
                return imageSource;
            }
        }

        async Task GenerateImageSource(byte[] ImageBytes, string name)
        {
            IEditableImage imageEdit = await CrossImageEdit.Current.CreateImageAsync(ImageBytes);
            string fileName = $"{name}.jpg";
            var fileFullName = Path.Combine(Utility.IMAGE_TEMP_FOLDER, fileName);
            File.WriteAllBytes(fileFullName, imageEdit.ToJpeg(100));
            imageSource = ImageSource.FromFile(fileFullName);
        }

        public JigsawListItem(int id, string name, byte[] bytes)
        {
            ID = id;
            Name = name;
            ImageBytes = bytes;
            Task.Run(async () => await GenerateImageSource(bytes, name)).Wait();
        }

    }
}
