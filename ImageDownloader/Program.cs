using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace ImageDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            string userInput = "";
            ImageDownloader imageDownloader = new ImageDownloader();

            while (userInput.ToLower() != "exit")
            {
                Console.WriteLine("<url> <filepath> ('exit' to quit)");
                Console.Write(DateTime.Now + " >> ");
                userInput = Console.ReadLine().Trim(' ');
                string[] userParameters = userInput.Split(' ');
                if (userParameters.Length == 2)
                {
                    string url = userParameters[0];
                    string saveTo = userParameters[1];

                    imageDownloader.URL = url;
                    imageDownloader.FolderPath = saveTo;
                    imageDownloader.DownloadImagesFromUrl();

                } else
                {
                    if (userInput.ToLower() != "exit")
                    {
                        Console.WriteLine("Invalid Parameters.");
                    }
                }
            } 
        }
    }
}
