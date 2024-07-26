using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        public string version = "";
        public string serverVersion = "";
        public string path = @"Game\";
        public string exeName = "TestWork2Games.exe";
        public string url = "https://launcher-game.local/";
        public bool downLoading = false;
        private void Form1_Load(object sender, EventArgs e)
        {
            Directory.CreateDirectory(Path.GetFullPath(path));
            if (!File.Exists(Path.GetFullPath(path + exeName)))
            {
                RegistryKey key;
                key = Registry.CurrentUser.CreateSubKey("GamesWork");
                key.SetValue("ver", "none");
                key.Close();
                version = "none";
            }
            else
            {
                version = (string)Registry.CurrentUser.OpenSubKey("GamesWork").GetValue("ver");
            }


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + "index.php");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();


            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream recivestream = response.GetResponseStream();
                StreamReader readSteam = null;
                if (response.CharacterSet == null)
                {
                    readSteam = new StreamReader(recivestream);
                }
                else
                {
                    readSteam = new StreamReader(recivestream, Encoding.GetEncoding(response.CharacterSet));
                }

                string date = readSteam.ReadToEnd();
                serverVersion = date;
                response.Close();
                readSteam.Close();
            }

            else
            {
                MessageBox.Show("404");
            }

            label1.Text = version + "/" + serverVersion;

            if (version != serverVersion)
            {
                if (File.Exists(Path.GetFullPath(path + exeName)))
                {
                    button1.Text = "Update";
                    button1.Click += UpdateGame;
                }
                else
                {
                    button1.Text = "DownGame";
                    button1.Click += UpdateGame;
                }
            }
            else
            {
                button1.Text = "Start";
                button1.Click += Play;
            }


        }

        private void UpdateGame(object sender, EventArgs e)
        {
            downLoading = false;
            button1.Enabled = false;
            DirectoryInfo dir = new DirectoryInfo(Path.GetFullPath(path));
            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo file in dir.GetDirectories())
            {
                file.Delete(true);
            }

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadProgressChanged += (s, g) =>
                {
                    if (g.ProgressPercentage == 100)
                    {
                        if (!File.Exists(Path.GetFullPath(path + exeName)))
                        {
                            ZipFile.ExtractToDirectory("game.zip", path);
                            RegistryKey key;
                            key = Registry.CurrentUser.CreateSubKey("GamesWork");
                            key.SetValue("ver", serverVersion);
                            key.Close();
                            version = serverVersion;
                            label1.Text = version + "/" + serverVersion;

                            button1.Text = "Play";
                            button1.Click -= UpdateGame;
                            button1.Click += Play;

                        }

                        button1.Enabled = true;
                        downLoading = false;
                    }
                };

                webClient.DownloadFileAsync(new Uri(url + "TestWork2Games.zip"), "Game.zip");
            }
        }

        private void Play(object sender, EventArgs e)
        {
            Process.Start(path + exeName);
        }
    }
}
