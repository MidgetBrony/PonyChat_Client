using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace IceChat.Forms
{
    public partial class PonyChatEdit : Form
    {
        public PonyChatEdit()
        {
            InitializeComponent();

            OpenSetting();
        }

        private void OpenSetting()
        {
            string currentFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + "PonyChat" + Path.DirectorySeparatorChar + "Client";
            string serversFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatServer.xml";
            XmlTextReader reader = new XmlTextReader(serversFile);
            while (reader.Read())
            {
                XmlNodeType nodeType = reader.NodeType;
                if (nodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "ServerName":
                            textServername.Text = reader.ReadString();
                            break;
                        case "NickName":
                            textNickName.Text = reader.ReadString();
                            break;
                        case "AdvancedSettings":
                            if (reader.ReadString() == "true")
                            {
                                checkAdvancedSettings.Checked = true;
                            }
                            else
                                checkAdvancedSettings.Checked = false;
                            break;
                    }
                }
            }
            reader.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkAdvancedSettings_CheckedChanged(object sender, EventArgs e)
        {
            //add or remote BNC/Proxy/Server Notes
            if (checkAdvancedSettings.Checked)
                AddAdvancedTabs();
            else
                RemoveAdvancedTabs();
        }

        private void RemoveAdvancedTabs()
        {
            this.tabControlSettings.TabPages.Remove(tabPageProxy);
            this.tabControlSettings.TabPages.Remove(tabPageBNC);
            this.tabControlSettings.TabPages.Remove(tabPageNotes);
            this.tabControlSettings.TabPages.Remove(tabPageIgnore);

            this.checkUseSASL.Visible = false;
            this.textSASLPass.Visible = false;
            this.textSASLUser.Visible = false;
            label10.Visible = false;
            label9.Visible = false;
        }

        private void AddAdvancedTabs()
        {
            this.tabControlSettings.TabPages.Remove(tabPageDefault);

            this.tabControlSettings.TabPages.Add(tabPageIgnore);
            this.tabControlSettings.TabPages.Add(tabPageNotes);
            this.tabControlSettings.TabPages.Add(tabPageProxy);
            this.tabControlSettings.TabPages.Add(tabPageBNC);

            this.tabControlSettings.TabPages.Add(tabPageDefault);

            this.checkUseSASL.Visible = true;
            this.textSASLPass.Visible = true;
            this.textSASLUser.Visible = true;
            label10.Visible = true;
            label9.Visible = true;
        }
    }
}
