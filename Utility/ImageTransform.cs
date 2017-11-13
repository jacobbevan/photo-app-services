using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.Primitives;

namespace photo_api.Utility
{
    public static class ImageTransform
    {
        public static void WriteThumbnail(Stream inStream, Stream outStream)
        {
            if (inStream == null)
            {
                throw new ArgumentNullException(nameof(inStream));
            }

            if (outStream == null)
            {
                throw new ArgumentNullException(nameof(outStream));
            }

            using (var image = Image.Load<Rgba32>(inStream))
            {
                var side = Math.Min(image.Height, image.Width);
                var cropRect = new Rectangle
                {
                    X = (image.Width - side)/2,
                    Y = (image.Height - side)/2,
                    Width = side,
                    Height = side
                };
                image.Mutate(x=>x.Crop(cropRect).Resize(512,512));
                image.SaveAsJpeg(outStream);
            }            
        }
    } 
    
}