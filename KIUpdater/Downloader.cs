using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KIUpdater
{
    class Downloader
    {
        private Extractor extractor;
        private Updater updater;
        private InstallLocationFinder installFinder;
        private SavedGamesLocationFinder savedGamesFinder;
        private string savePath;

        public Downloader(Extractor extractor, InstallLocationFinder installFinder, SavedGamesLocationFinder savedGamesFinder, string downloadSavePath)
        {
            this.extractor = extractor;
            this.installFinder = installFinder;
            this.savedGamesFinder = savedGamesFinder;
            this.savePath = downloadSavePath;
        }

        public void Download(string url)
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += Wc_DownloadDataCompleted;
                wc.DownloadFileTaskAsync(new System.Uri(url), savePath).Wait(); 
            }
        }

        private void Wc_DownloadDataCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Console.WriteLine("");
            Console.WriteLine("Download Completed - extracting");
            if (extractor.Extract(savePath))
            {
                Console.WriteLine("Files extracted");

                updater = new Updater(installFinder, savedGamesFinder, extractor.DestinationPath);

                if (updater.Update())
                    Console.WriteLine("Update Succeeded");
                else
                    Console.WriteLine("Update Failed");
            } 
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.Write("\r...Progress: " + e.ProgressPercentage + "%  ");
        }
    }
}
