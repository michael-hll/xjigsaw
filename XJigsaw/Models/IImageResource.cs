using System;
using Xamarin.Forms;

namespace XJigsaw.Models
{
    public interface IImageResource
    {
        Size GetSize(string fileName);
    }
}
