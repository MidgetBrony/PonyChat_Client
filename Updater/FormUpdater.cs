/******************************************************************************\
 * IceChat 9 Internet Relay Chat Client
 *
 * Copyright (C) 2012 Paul Vanderzee <snerf@icechat.net>
 *                                    <www.icechat.net> 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 * Please consult the LICENSE.txt file included with this project for
 * more details
 *
\******************************************************************************/


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.IO.Compression;

namespace IceChatUpdater
{
    public partial class FormUpdater : Form
    {
        private string currentFolder;
        private System.Net.WebClient webClient;
        private string currentFile;
        

        private Stack<Uri> localFiles = new Stack<Uri>();
        private Stack<Uri> moveFiles = new Stack<Uri>();

        public FormUpdater(string[] args)
        {
            InitializeComponent();

            if (args.Length > 0)
            {
                foreach (string arg in args)
                    currentFolder = arg;
            }
            else
                currentFolder = Application.StartupPath;
           
            labelFolder.Text = currentFolder;
            labelCurrentFile.Text = "";
            label3.Text = Application.StartupPath;

            CheckForUpdate();
            
            //string remoteUri = "http://download-codeplex.sec.s-msft.com/Download/SourceControlFileDownload.ashx?ProjectName=icechat&changeSetId=" + revision;

        }

        private void CheckForUpdate()
        {

            //get the current version of IceChat 2009 in the Same Folder
            System.Diagnostics.FileVersionInfo fv;
            double currentVersion;
            try
            {
                fv = System.Diagnostics.FileVersionInfo.GetVersionInfo(currentFolder + Path.DirectorySeparatorChar + "IceChat2009.exe");
                System.Diagnostics.Debug.WriteLine(fv.FileVersion);
                labelCurrent.Text = "Current Version: " + fv.FileVersion;
                currentVersion = Convert.ToDouble(fv.FileVersion.Replace(".", String.Empty));
            }
            catch(Exception)
            {
                currentVersion = 000000000;
                labelCurrent.Text = "Current Version: 0.0.0000.0000";
            }
            
            //delete the current update.xml file if it exists
            if (File.Exists(Application.StartupPath + System.IO.Path.DirectorySeparatorChar + "update9.xml"))
                File.Delete(Application.StartupPath + System.IO.Path.DirectorySeparatorChar + "update9.xml");

            System.Net.WebClient webClient = new System.Net.WebClient();
            webClient.DownloadFile("http://www.icechat.net/update9.xml", Application.StartupPath + System.IO.Path.DirectorySeparatorChar + "update9.xml");
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(Application.StartupPath + System.IO.Path.DirectorySeparatorChar + "update9.xml");
            
            System.Xml.XmlNodeList version = xmlDoc.GetElementsByTagName("version");
            System.Xml.XmlNodeList versiontext = xmlDoc.GetElementsByTagName("versiontext");

            labelLatest.Text = "Latest Version: " + versiontext[0].InnerText;

            if (Convert.ToDouble(version[0].InnerText) > currentVersion)
            {
                XmlNodeList files = xmlDoc.GetElementsByTagName("file");
                foreach (XmlNode node in files)
                {
                    listFiles.Items.Add(node.InnerText);
                }

                buttonDownload.Visible = true;
                labelUpdate.Visible = true;
            }
            else
                labelNoUpdate.Visible = true;
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            //download the files in the File List box
            //check to make sure icechat 9 is not running
                Process[] pArry = Process.GetProcesses();

                foreach (Process p in pArry)
                {
                    string s = p.ProcessName;
                    s = s.ToLower();
                    if (s.CompareTo("icechat2009") == 0)
                    {
                        //MessageBox.Show(Path.GetDirectoryName(p.Modules[0].FileName).ToLower();
                        if (Path.GetDirectoryName(p.Modules[0].FileName).ToLower() == currentFolder.ToLower())
                        {
                            MessageBox.Show("Please Close IceChat 9 before we update");
                            return;
                        }
                    }
                }
            
            webClient = new System.Net.WebClient();
            this.Cursor = Cursors.WaitCursor;
            
            this.labelSize.Visible = true;
            this.progressBar.Visible = true;

            this.buttonDownload.Enabled = false;
            //System.Collections.ArrayList localFiles = new System.Collections.ArrayList();
            
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_DownloadFileCompleted);            
            webClient.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            foreach (string file in listFiles.Items)
            {
                string f = System.IO.Path.GetFileName(file);
                //System.Diagnostics.Debug.WriteLine(f);
                if (File.Exists(Application.StartupPath + System.IO.Path.DirectorySeparatorChar + f))
                    File.Delete(Application.StartupPath + System.IO.Path.DirectorySeparatorChar + f);

                System.Diagnostics.Debug.WriteLine(file);
                
                Uri uri = new Uri(file);
                localFiles.Push(uri);
                moveFiles.Push(uri);
                    
            }
            
            Uri u = localFiles.Pop();
            string localFile = Path.GetFileName(u.ToString());
            currentFile = u.ToString();
            labelCurrentFile.Text = currentFile;

            webClient.DownloadFileAsync(u, Application.StartupPath + System.IO.Path.DirectorySeparatorChar + localFile);
           
        }

        private void webClient_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.ProgressPercentage + ":" + e.BytesReceived + ":" + e.TotalBytesToReceive);
            this.progressBar.Value = e.ProgressPercentage;
            labelSize.Text = e.BytesReceived + "/" + e.TotalBytesToReceive + " (" + e.ProgressPercentage + "%)";            
        }

        private void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("download done:" + e.UserState + ":" + currentFile);

            //go to the next file in the list
            if (localFiles.Count > 0)
            {
                Uri u = localFiles.Pop();

                string localFile = Path.GetFileName(u.ToString());
                currentFile = u.ToString();
                labelCurrentFile.Text = currentFile;
                System.Diagnostics.Debug.WriteLine("download+" + localFile);
                webClient.DownloadFileAsync(u, Application.StartupPath + System.IO.Path.DirectorySeparatorChar + localFile);
            }
            else
            {

                this.Cursor = Cursors.Default;
                MessageBox.Show("Completed Download");

                //now see if IceChat is running
                //and close it

                

                buttonDownload.Enabled = false;
                /*
                Process[] pArry = Process.GetProcesses();

                foreach (Process p in pArry)
                {
                    string s = p.ProcessName;
                    s = s.ToLower();
                    if (s.CompareTo("icechat2009") == 0)
                    {
                        if (Path.GetDirectoryName(p.Modules[0].FileName).ToLower() == currentFolder.ToLower())
                        {
                            MessageBox.Show("Closing IceChat to update it");
                            try
                            {
                                p.Kill();
                                //p.CloseMainWindow();

                                //wait a bit and then copy the files to this folder, and VOILA
                                p.WaitForExit();

                                System.Threading.Thread.Sleep(3000);


                            }
                            catch (Exception ee)
                            {
                                MessageBox.Show(ee.Message + ":" + ee.Source);
                            }

                        }
                    }
                }
                */
                foreach (Uri f in moveFiles)
                {
                    if (File.Exists(currentFolder + System.IO.Path.DirectorySeparatorChar + Path.GetFileName(f.ToString())))
                        File.Delete(currentFolder + System.IO.Path.DirectorySeparatorChar + Path.GetFileName(f.ToString()));

                    System.Threading.Thread.Sleep(500);

                    File.Copy(Application.StartupPath + System.IO.Path.DirectorySeparatorChar + Path.GetFileName(f.ToString()), currentFolder + System.IO.Path.DirectorySeparatorChar + Path.GetFileName(f.ToString()));

                    //delete the files out of the update folder
                    File.Delete(Application.StartupPath + System.IO.Path.DirectorySeparatorChar + Path.GetFileName(f.ToString()));
                }


                MessageBox.Show("Files updated, you are welcome to restart IceChat");

            }

        }
    }
}
