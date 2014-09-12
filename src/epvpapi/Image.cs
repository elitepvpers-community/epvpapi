using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace epvpapi
{
    public class Image
    {
        /// <summary>
        /// Name that will be used for uploading and storing the <c>Image</c>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Extension (.jpg, .png...)
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Binary file data
        /// </summary>
        public byte[] Data { get; set; }

        public Image()
        {
            Data = new byte[1024];
        }

        /// <summary>
        /// Returns a valid <c>Image</c> object containing the image from the given path
        /// </summary>
        /// <param name="path"> Path to the image being loaded </param>
        /// <returns> <c>Image</c> object containing the actual image from the given path </returns>
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
        /// Downloads the <c>Image</c> from the given URL
        /// </summary>
        /// <param name="url"> URL from where to download </param>
        /// <param name="imageFormat"> Extension of the <c>Image</c> used for uploading lateron </param>
        /// <returns> <c>Image</c> object containing the just downloaded image </returns>
        public static Image FromWeb(Uri url, string imageFormat = ".jpeg")
        {
            return new Image() { Data = new WebClient().DownloadData(url), Name = "Unnamed", Format = imageFormat };
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
