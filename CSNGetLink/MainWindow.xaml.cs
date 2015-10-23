using CSNGetLink;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CSNDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string url = "";
        HttpWebResponse response;
        string typeMusic = "";
        StringBuilder sb;
        string folderIdm = "";

        public MainWindow()
        {
            InitializeComponent();
            idmFolderTextBox.Text = IdmFolderSetting.Default.IdmFolder;
        }
  
        private void btnGet_Click(object sender, RoutedEventArgs e)
        {

            url = urlTextBox.Text;
            response = null;
            folderIdm = idmFolderTextBox.Text + "\\IDMan.exe";
            // Update the value.
            IdmFolderSetting.Default.IdmFolder = idmFolderTextBox.Text;
            // Save the config file.
            IdmFolderSetting.Default.Save();
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();

            if (t128Radio.IsChecked == true)
            {
                typeMusic = "128";
            }
            if (t320Radio.IsChecked == true)
            {
                typeMusic = "320";
            }
            if (tLosslessRadio.IsChecked == true)
            {
                typeMusic = "flac";
            }
            sb = new StringBuilder();

            if (url.Contains("http://chiasenhac.com") || url.Contains("https://chiasenhac.com")
                || url.Contains("http://www.chiasenhac.com") || url.Contains("https://chiasenhac.com"))
            {
                progressBar.Visibility = Visibility.Visible;
            }
            else
            {
                progressBar.Visibility = Visibility.Hidden;
            }
        }
        
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
           urlTextBox.Text = "";
           resTextBlock.Text = "";
           progressBar.Visibility = Visibility.Hidden;
           progressBar.Value = 0;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {

            if (url.Contains("http://chiasenhac.com") || url.Contains("https://chiasenhac.com")
                || url.Contains("http://www.chiasenhac.com") || url.Contains("https://chiasenhac.com"))
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException)
                {
                    MessageBox.Show("Nhập playlist url của trang web chiasenhac", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                finally
                {
                    // Don't forget to close your response.
                    if (response != null && "OK".Equals(response.StatusCode.ToString()))
                    {
                        HtmlWeb web = new HtmlWeb();
                        HtmlDocument doc = web.Load(url);

                        int max = 0;
                        int i = 0;

                        foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//span//a"))
                        {
                            string downloadUrl = link.Attributes["href"].Value;
                            if (downloadUrl.Contains("_download.html"))
                            {
                                HtmlDocument d = web.Load(downloadUrl);

                                foreach (HtmlNode l in d.DocumentNode.SelectNodes("//a[@href]"))
                                {
                                    string hrefValue = l.Attributes["href"].Value;
                                    if (hrefValue.Contains("chiasenhac.com/downloads"))
                                    {
                                        max++;
                                    }
                                }
                            }
                        }

                        foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//span//a"))
                        {

                            string downloadUrl = link.Attributes["href"].Value;
                            int progressPercentage = Convert.ToInt32(((double)i / max) * 100);
                            i++;
                            (sender as BackgroundWorker).ReportProgress(progressPercentage);
                            if (downloadUrl.Contains("_download.html"))
                            {
                                HtmlDocument d = web.Load(downloadUrl);
                 
                                foreach (HtmlNode l in d.DocumentNode.SelectNodes("//a[@href]"))
                                {
                                    string hrefValue = l.Attributes["href"].Value;
                                    
                                    if (hrefValue.Contains("chiasenhac.com/downloads"))
                                    {
                                        //progressBar.Value = i++;
                                        string s = l.InnerHtml;
                                        (sender as BackgroundWorker).ReportProgress(progressPercentage);
                                        if (typeMusic.Equals("flac"))
                                        {
                                            if (hrefValue.Contains("320"))
                                            {
                                                hrefValue = hrefValue.Replace("320", typeMusic).Replace("MP3 flackbps", "FLAC Lossless").Replace(".mp3", "." + typeMusic);
                                                sb.Append("■ " + hrefValue + "\n");
                                                if (File.Exists(folderIdm))
                                                {
                                                    Process.Start(folderIdm, "/a /d " + '"' + hrefValue + '"');
                                                }
                                                
                                                break;
                                                
                                            }
                                        }
                                        else if (hrefValue.Contains(typeMusic))
                                        {
                                            sb.Append("■ " + hrefValue + "\n");
                                            if (File.Exists(folderIdm))
                                            {
                                                Process.Start(folderIdm, "/a /d " + '"' + hrefValue + '"');
                                            }
                                            break;
                                        }
                                    }
                                }

                            }
                        
                        }
                        response.Close();
                    }
                }
            }
            else
            {
                MessageBox.Show("Nhập playlist url của trang web chiasenhac", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (progressBar.Value == 0)
            {
                progressBar.Visibility = Visibility.Hidden;
            }
            else
            {
                resTextBlock.Text = sb.ToString();
                if (File.Exists(folderIdm))
                {
                    Process.Start(folderIdm, "/s");
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = fbd.ShowDialog();
            idmFolderTextBox.Text = fbd.SelectedPath;
            // Update the value.
            IdmFolderSetting.Default.IdmFolder = idmFolderTextBox.Text;
            // Save the config file.
            IdmFolderSetting.Default.Save();
        }

        

    }
}
