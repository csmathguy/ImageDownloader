using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;

namespace ImageDownloader
{
    class ImageDownloader
    {
        public string URL { get; set; }
        public string FolderPath { get; set; }

        public ImageDownloader() { }
        public ImageDownloader(string url, string folderPath)
        {
            this.URL = url;
            this.FolderPath = folderPath;
        }

        public void DownloadImagesFromUrl()
        {
            bool validUrl = false;
            string htmlCode = null;
            string host = null;
            const string imgTagPattern = @"<img\b[^\<\>]+?\bsrc\s*=\s*[""'](?<SRC>.+?)[""'][^\<\>]*?\>";

            //ZH, if the url or folder path are not set, we will not be able to continue
            if (this.URL == null || this.FolderPath == null)
            {
                throw new ArgumentException("URL or Folder Path do not have a value.");
            }

            //ZH, check URL string formatting, should start with http or https
            if (!this.URL.StartsWith("http://") && !this.URL.StartsWith("https://"))
            {
                this.URL = "http://" + this.URL;
            }

            //ZH, create folder if it doesn't already exist
            if (!Directory.Exists(this.FolderPath))
            {
                Directory.CreateDirectory(this.FolderPath);
            }

            //ZH, Ouput File used for displaying error to users (this assume file exist)
            StreamWriter outputFile = new StreamWriter(this.FolderPath + @"\_output.txt");

            //ZH, convert the URL into a URI object for easy access to the Host (Domain), Record any error that may occur.
            try {
                Uri myUri = new Uri(this.URL);
                host = "http://" + myUri.Host;
                validUrl = true;
            } catch (System.UriFormatException e)
            {
                outputFile.WriteLine("Invalid URL '" + this.URL + "' " + e.Message);
            }

            if (validUrl)
            {
                using (WebClient client = new WebClient())
                {
                    try {
                        htmlCode = client.DownloadString(this.URL);
                        validUrl = true;

                        //ZH, for each img tag in the html of the URL, validate and format it and download it to the folder location.
                        foreach (Match match in Regex.Matches(htmlCode, imgTagPattern, RegexOptions.IgnoreCase))
                        {
                            string imagePath = match.Groups["SRC"].Value.ToLower();
                            if (HasImageExtension(imagePath))
                            {
                                //ZH, get the file name (the string following the last '/' in the image path.
                                string imageFileName = imagePath.Substring(imagePath.LastIndexOf('/'));
                                string filePath = this.FolderPath + @"\" + imageFileName;

                                //ZH, Fix issue with Relative path (adding a / so that it can be converted into a absolute path)
                                if (imagePath.StartsWith("../"))
                                {
                                    imagePath = "/" + imagePath;
                                }

                                //ZH, change relative protocol to absolute path
                                if (imagePath.StartsWith("//"))
                                {
                                    imagePath = "http:" + imagePath;
                                }

                                //ZH, change relative path to absolute path
                                if (!imagePath.StartsWith("http://") && !imagePath.StartsWith("https://"))
                                {
                                    imagePath = host + imagePath;
                                }

                                //ZH, Attempt to download each image, if there is a problem record it in the output file.
                                try
                                {
                                    client.DownloadFile(imagePath, filePath);
                                }
                                catch (Exception e)
                                {
                                    outputFile.WriteLine("Error downloading image path: " + imagePath + " :: " + e.Message);
                                }
                            }
                        }
                    } catch (Exception e)
                    {
                        outputFile.WriteLine(e.Message);
                    }
                }
            }

            outputFile.Close();
        }

        /**
         * Verify the url has an image extension
        */
        private bool HasImageExtension(string url)
        {
            url = url.ToLower();
            return (url.EndsWith(".png") || url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".gif") || url.EndsWith(".bmp"));
        }
    }
}

