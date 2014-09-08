using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace epvpapi
{
    /// <summary>
    /// Represents an Image
    /// </summary>
    public class Image
    {
        /// <summary>
        /// Name of the picture in the used file system
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Extension of the picture in the used file system
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Binary file data of the picture
        /// </summary>
        public byte[] Data { get; set; }

        public Image()
        { }

        public static Image FromFileSystem(string path)
        {
            if(!Image.IsValid(path)) throw new ArgumentException("Provided path is not a valid path to an image");

            var image = new Image()
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Format = Path.GetExtension(path)
            };

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] binaryData = new byte[fs.Length];
                fs.Read(binaryData, 0, binaryData.Length);
                fs.Close();
                image.Data = binaryData;
            }

            return image;
        }

        /// <summary>
        /// Checks if a provided path is a valid path to a <c>Picture</c>
        /// </summary>
        /// <param name="path"> path to the <c>Picture</c></param>
        /// <returns></returns>
        public static bool IsValid(string path)
        {
            Regex regex = new Regex(".*\\.(png|PNG|jpg|JPG|jpeg|JPEG|gif|GIF|bmp|BMP)");
            return regex.IsMatch(path);
        }
    }
}
