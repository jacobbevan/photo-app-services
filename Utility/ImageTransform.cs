using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MetadataExtractor;
using photo_api.Models;
using SixLabors.ImageSharp;
using SixLabors.Primitives;

namespace photo_api.Utility
{
    public static class ImageTransform
    {
        private static Regex _regPixels = new Regex( @"(?<size>[0-9]+)\spixels");
        public static string SeekTag(IEnumerable<MetadataExtractor.Directory> results,  string directory, string tagName)
        {
            return results.FirstOrDefault(d=>d.Name == directory)?.Tags.FirstOrDefault(f=>f.Name==tagName)?.Description;
        }

        public static int? SeekPixels(IEnumerable<MetadataExtractor.Directory> results, string directory, string tagName)
        {
            var text = SeekTag(results, directory, tagName);

            if(string.IsNullOrEmpty(text))
            {
                return null;
            }

            var match = _regPixels.Match(text);
            if(match.Success)
            {
                var size = match.Groups.FirstOrDefault(g=>g.Name == "size")?.Value;
                
                return System.Int32.Parse(size);

            }

            return null;

        }

        public static int? SeekHeight(IEnumerable<MetadataExtractor.Directory> results)
        {
            return SeekPixels(results, "JPEG", "Image Height");            
        }

        public static int? SeekWidth(IEnumerable<MetadataExtractor.Directory> results)
        {
            return SeekPixels(results, "JPEG", "Image Width");                        
        }

        public static DateTime? SeekCreated(IEnumerable<MetadataExtractor.Directory> results)
        {
            DateTime result;
            var dateTimeStr = SeekTag(results, "Exif IFD0", "Date/Time");

            if(string.IsNullOrEmpty(dateTimeStr))
                return null;

            if(DateTime.TryParseExact(dateTimeStr, "yyyy:MM:dd HH:mm:ss",CultureInfo.InvariantCulture,DateTimeStyles.None, out result))
                return result;

            return null;
        }


        public static ImageSummary CreateImageSummary(string id, Stream image)
        {
            //height 1
            //width 3

            // "QuickTime Movie Header"
                // "Created" : "Tue Apr 25 15:56:01 2017"
            //

            //"JPEG"
                // "Image Height" : "3264 pixels"
                // "Image Width"

            //"Exif IFD0"
            //"Date/Time" "2016:02:01 03:50:27"

            //"Exif SubIFD"
            //  "Date/Time Original" : "2016:02:01 03:50:27"

            var results = MetadataExtractor.ImageMetadataReader.ReadMetadata(image);

            var height = SeekHeight(results);
            var width = SeekWidth(results);
            var created = SeekCreated(results);

            List<string> captionElements = new List<string>();

            if(created != null)
            {
                captionElements.Add(created.Value.ToString("dd-MMM-yy"));
            }
            if(height != null && width != null)
            {
                captionElements.Add($"{width.Value}x{height.Value}");                
            }


            return new ImageSummary
            {
                Id = id,  
                Height = SeekHeight(results),
                Width = SeekWidth(results),
                Created = created ?? DateTime.Now,
                Caption = string.Join(" ", captionElements)
            };
        }

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