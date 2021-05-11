using Stormlion.ImageCropper;
using System;
using Xamarin.Forms;

namespace Test
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

        }

        protected async void OnClickedRectangle(object sender, EventArgs e)
        {
            imageView.Source = null;
            new ImageCropper()
            {
//                PageTitle = "Test Title",
//                AspectRatioX = 1,
//                AspectRatioY = 1,
                Success = (imageFile) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        imageView.Source = ImageSource.FromFile(imageFile);
                    });
                },
                Faiure = () => {
                    Console.WriteLine("Error capturando la imagen o haciendo crop.");
                }
            }.Show(this);
        }

        private void OnClickedCircle(object sender, EventArgs e)
        {
            imageView.Source = null;
            new ImageCropper()
            {
                CropShape = ImageCropper.CropShapeType.Oval,
                Success = (imageFile) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        imageView.Source = ImageSource.FromFile(imageFile);
                    });
                },
                Faiure = () => {
                    Console.WriteLine("Error capturando la imagen o haciendo crop.");
                }
            }.Show(this);
        }
    }
}
