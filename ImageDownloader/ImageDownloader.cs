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

        public ImageDownloader() { }


        public void DownloadImagesFromUrl(string url, string folderPath)
        {
            bool validUrl = false;
            string htmlCode = null;
            string host = null;
            const string imgTagPattern = @"<img\b[^\<\>]+?\bsrc\s*=\s*[""'](?<SRC>.+?)[""'][^\<\>]*?\>";

            //ZH, if the url or folder path are not set, we will not be able to continue
            if (url == null || folderPath == null)
            {
                throw new ArgumentException("URL or Folder Path do not have a value.");
            }

            //ZH, check URL string formatting, should start with http or https
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "http://" + url;
            }

            //ZH, create folder if it doesn't already exist
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            //ZH, Ouput File used for displaying error to users (this assume file exist)
            StreamWriter outputFile = new StreamWriter(folderPath + @"\_output.txt");

            //ZH, convert the URL into a URI object for easy access to the Host (Domain), Record any error that may occur.
            try
            {

                Uri myUri = new Uri(url);
                host = "http://" + myUri.Host;
                validUrl = true;

            } catch (System.UriFormatException e)
            {
                outputFile.WriteLine("Invalid URL '" + url + "' " + e.Message);
            }

            if (validUrl)
            {
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        htmlCode = client.DownloadString(url);
                        validUrl = true;

                        //ZH, for each img tag in the html of the URL, validate and format it and download it to the folder location.
                        foreach (Match match in Regex.Matches(htmlCode, imgTagPattern, RegexOptions.IgnoreCase))
                        {
                            string imagePath = match.Groups["SRC"].Value.ToLower();
                            if (HasImageExtension(imagePath))
                            {
                                //ZH, get the file name (the string following the last '/' in the image path.
                                string imageFileName = imagePath.Substring(imagePath.LastIndexOf('/'));
                                string filePath = folderPath + @"\" + imageFileName;

                                imagePath = MakePathAbsolute(imagePath, host);

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

        /**
         * Given a path (relative or absolute) and a host name, modify path to make sure it is an absolute path.
         */
        private string MakePathAbsolute(string path, string host)
        {
            //ZH, Fix issue with Relative path (adding a / so that it can be converted into a absolute path)
            if (path.StartsWith("../"))
            {
                path = "/" + path;
            }

            //ZH, change relative protocol to absolute path
            if (path.StartsWith("//"))
            {
                path = "http:" + path;
            }

            //ZH, change relative path to absolute path
            if (!path.StartsWith("http://") && !path.StartsWith("https://"))
            {
                path = host + path;
            }

            return path;
        }
    }
}

