/******************************************************************************\
 * IceChat 9 Internet Relay Chat Client
 *
 * Copyright (C) 2013 Paul Vanderzee <snerf@icechat.net>
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;

namespace IceChat
{
    public partial class FormFirstRun : Form
    {
        private string[] Default_Servers = new string[] { "irc.ponychat.net"};
        private string[] Default_Aliases = new string[] { "/op /mode # +o $1", "/deop /mode # -o $1", "/voice /mode # +v $1", "/devoice /mode # -v $1", "/b /ban # $1", "/j /join $1 $2-", "/n /names $1", "/w /whois $1 $1", "/k /kick # $1 $2-", "/q /query $1", "/v /version $1", "/about //say Operating System  [$os Build No. $osbuild]  - Uptime [$uptime]  - $icechat" };

        private string[] Nicklist_Popup = new string[] { "Information", ".Display User Info:/userinfo $nick", ".Whois user:/whois $nick $nick", ".DNS User:/dns $nick", "Commands", ".Query user:/query $nick", "Op Commands", ".Voice user:/mode # +v $nick", ".DeVoice user:/mode # -v $nick", ".Op user:/mode # +o $nick", ".Deop user:/mode # -o $nick", ".Kick:/kick # $nick", ".Ban:/ban # $mask($host,2)", "CTCP", ".Ping:/ping $nick", ".Version:/version $nick", "DCC", ".Send:/dcc send $nick", ".Chat:/dcc chat $nick", "Slaps", ".Brick:/me slaps $nick with a big red brick", ".Trout:/me slaps $nick with a &#x3;4r&#x3;8a&#x3;9i&#x3;11n&#x3;13b&#x3;17o&#x3;26w trout" };
        private string[] Channel_Popup = new string[] { "Information", ".Channel Info:/chaninfo" };
        private string[] Console_Popup = new string[] { "Server Commands", ".Server Links Here:/links", ".Message of the Day:/motd", "AutoPerform:/autoperform", "Autojoin:/autojoin" };
        private string[] Query_Popup = new string[] { "Info:/userinfo $1", "Whois:/whois $nick", "-", "Ignore:/ignore $1", "-", ".Ping:/ctcp $1 ping", ".Time:/ctcp $1 time", ".Version:/ctcp $1 version", "DCC", ".Send:/dcc send $1", ".Chat:/dcc chat $1" };
        private string[] Buddy_Popup = new string[] { "Query:/query $1", "Whois:/whois $1" };

        private int _currentStep;

        private string _nickName;
        private string _currentFolder;

        private IceChatOptions icechatOptions;
        private IceChatFontSetting icechatFonts;
        private IceChatColors icechatColors;
        private IceChatMessageFormat icechatMessages;

        internal delegate void SaveOptionsDelegate(IceChatOptions options, IceChatFontSetting fonts);
        internal event SaveOptionsDelegate SaveOptions;

        public FormFirstRun(string currentFolder)
        {
            InitializeComponent();
            this.icechatOptions = new IceChatOptions();
            this.icechatFonts = new IceChatFontSetting();
            this.icechatColors = new IceChatColors();
            this.icechatMessages = new IceChatMessageFormat();

            this.buttonNext.Image = StaticMethods.LoadResourceImage("next.svg.png");
            this.buttonBack.Image = StaticMethods.LoadResourceImage("previous.svg.png");

            _nickName = "Default";
            _currentFolder = currentFolder;

            foreach (string s in Default_Servers)
            {
                comboData.Items.Add(s);
            }

            comboData.Text = "irc.ponychat.net";

            CurrentStep = 0;
        }

        private void FormFirstRun_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && _currentStep != 3)
                e.Cancel = true;
        }
        
        private int CurrentStep
        {
            get { return _currentStep; }
            set
            {
                _currentStep = value;
                
                switch (value)
                {
                    case 0:
                        textData.Text = _nickName;
                        labelHeader.Text = "Nick Name";
                        labelDesc.Text = "Enter a nickname in the field below." + Environment.NewLine + "A nickname (or handle) is what you will be known as on PonyChat. It's similar to your real life name except on PonyChat, your nickname will be unique. You will be the only person on the network with your chosen nickname." + Environment.NewLine+"*Note*: Mane 6 Names are already taken to be used for our Service Bots. Please use a custom name/OC Name for best results.";
                        labelField.Text = "Nickname:";
                        
                        comboData.Visible = false;
                        textData.Visible = true;
                        buttonBack.Visible = false;
                        buttonNext.Visible = true;

                        break;

                    case 1:
                        _nickName = textData.Text;
                        labelHeader.Text = "Server Info";
                        labelDesc.Text = "This has already been added for you to go through the ponychat Round-Robin. If you know how to edit the config you can add a specific server that you want.";
                        labelField.Text = "Server Address:";

                        labelField.Visible = false;
                        textData.Visible = false;
                        comboData.Visible = false;
                        buttonBack.Visible = true;
                        buttonNext.Visible = true;
                        
                        break;

                    case 2:
                        labelHeader.Text = "Done";
                        labelDesc.Text = "Your information has been saved. Simply select a server from the Favorite Server List, and click the 'Connect' button.";
                        labelDesc.Text += Environment.NewLine + Environment.NewLine + "Default Nick Name: " + _nickName ;
                        if (comboData.Text.Length > 0)
                            labelDesc.Text += Environment.NewLine + "Default Server: " + comboData.Text;

                        labelField.Text = "";
                        textData.Visible = false;
                        comboData.Visible = false;
                        buttonBack.Visible = true;
                        buttonNext.Visible = true;
                       
                        break;

                    case 3:
                        //save the information
                        MakeDefaultFiles();

                        this.Close();
                        break;
                }
                
            }
            
        }
        
        private void MakeDefaultFiles()
        {
            //make the server file
            string serversFile = _currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatServer.xml";

            IceChatServers servers = new IceChatServers();

            int ID = 1;

            _nickName = _nickName.Replace(" ", "");
            _nickName = _nickName.Replace("#", "");
            _nickName = _nickName.Replace(",", "");
            _nickName = _nickName.Replace("`", "");

            FormMain.Instance.IceChatOptions.DefaultNick = _nickName;



            //check for other theme files
            int totalThemes = 1;
            
            DirectoryInfo currentFolder = new DirectoryInfo(_currentFolder);
            FileInfo[] xmlFiles = currentFolder.GetFiles("*.xml"); 
            foreach (FileInfo fi in xmlFiles)
            {
                if (fi.Name.StartsWith("Colors-"))
                {
                    totalThemes++;
                }
            }


            FormMain.Instance.IceChatOptions.Theme = new ThemeItem[totalThemes];
            
            FormMain.Instance.IceChatOptions.Theme[0] = new ThemeItem();
            FormMain.Instance.IceChatOptions.Theme[0].ThemeName = "Default";
            FormMain.Instance.IceChatOptions.Theme[0].ThemeType = "XML";

            int t = 1;
            foreach (FileInfo fi in xmlFiles)
            {
                if (fi.Name.StartsWith("Colors-"))
                {
                    string themeName = fi.Name.Replace("Colors-", "").Replace(".xml", ""); ;
                    FormMain.Instance.IceChatOptions.Theme[t] = new ThemeItem();
                    FormMain.Instance.IceChatOptions.Theme[t].ThemeName = themeName;
                    FormMain.Instance.IceChatOptions.Theme[t].ThemeType = "XML";
                    t++;
                }
            }

            FormMain.Instance.IceChatOptions.CurrentTheme = "Default";


            if (comboData.Text.Length > 0)
            {
                ServerSetting s = new ServerSetting();
                s.ID = ID;
                s.ServerName = comboData.Text;
                s.NickName = _nickName;
                s.AltNickName = _nickName + "_";
                s.ServerPort = "6697";
                s.FullName = FormMain.Instance.IceChatOptions.DefaultFullName;
                s.IdentName = FormMain.Instance.IceChatOptions.DefaultIdent;
                s.QuitMessage = FormMain.Instance.IceChatOptions.DefaultQuitMessage;
                s.SetModeI = true;
                s.UseSSL = true;
                s.SSLAcceptInvalidCertificate = true;
                s.AutoStart = true;

                //Auto Join PonyChat Channel
                s.AutoJoinChannels = new string[] { "#ponychat" };
                s.AutoJoinEnable = true;
                s.AutoJoinDelay = true;

                ID++;

                servers.AddServer(s);
            }
            
            foreach (string server in comboData.Items)
            {
                if (server != comboData.Text && server.Length > 0)
                {
                    ServerSetting ss = new ServerSetting();
                    ss.ID = ID;
                    ss.ServerName = server;
                    ss.NickName = _nickName;
                    ss.AltNickName = _nickName + "_";
                    ss.ServerPort = "6697";
                    ss.SetModeI = true;
                    ss.FullName = FormMain.Instance.IceChatOptions.DefaultFullName;
                    ss.IdentName = FormMain.Instance.IceChatOptions.DefaultIdent;
                    ss.QuitMessage = FormMain.Instance.IceChatOptions.DefaultQuitMessage;
                    ss.UseSSL = true;
                    ss.SSLAcceptInvalidCertificate = true;
                    ss.AutoStart = true;

                    //Auto-Join #PonyChat Channel
                    ss.AutoJoinChannels = new string[] { "#ponychat" };
                    ss.AutoJoinEnable = true;

                    ID++;

                    servers.AddServer(ss);
                }
            }

            XmlSerializer serializer = new XmlSerializer(typeof(IceChatServers));
            TextWriter textWriter = new StreamWriter(FormMain.Instance.ServersFile);
            serializer.Serialize(textWriter, servers);
            textWriter.Close();
            textWriter.Dispose();


            //make the default aliases file
            string aliasesFile = _currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatAliases.xml";
            IceChatAliases aliasList = new IceChatAliases();
            
            foreach (string alias in Default_Aliases)
            {
                AliasItem a = new AliasItem();
                string name = alias.Substring(0,alias.IndexOf(" ")).Trim();
                string command = alias.Substring(alias.IndexOf(" ") + 1);
                a.AliasName = name;
                a.Command = new String[] { command };

                aliasList.AddAlias(a);
            }

            XmlSerializer serializerA = new XmlSerializer(typeof(IceChatAliases));
            TextWriter textWriterA = new StreamWriter(aliasesFile);
            serializerA.Serialize(textWriterA, aliasList);
            textWriterA.Close();
            textWriterA.Dispose();

            
            //make the default popups file
            string popupsFile = _currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatPopups.xml";
            IceChatPopupMenus popupList = new IceChatPopupMenus();

            popupList.AddPopup(newPopupMenu("Console", Console_Popup));
            popupList.AddPopup(newPopupMenu("Channel", Channel_Popup));
            popupList.AddPopup(newPopupMenu("Query", Query_Popup));
            popupList.AddPopup(newPopupMenu("NickList", Nicklist_Popup));

            XmlSerializer serializerP = new XmlSerializer(typeof(IceChatPopupMenus));
            TextWriter textWriterP = new StreamWriter(popupsFile);
            serializerP.Serialize(textWriterP, popupList);
            textWriterP.Close();
            textWriterP.Dispose();

        }

        private PopupMenuItem newPopupMenu(string type, string[] menu)
        {
            PopupMenuItem p = new PopupMenuItem();
            p.PopupType = type;
            p.Menu = menu;
            return p;
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            //check if a nickname has been set
            if (_currentStep == 0)
            {
                if (textData.Text == "Default")
                {
                    MessageBox.Show("Please Choose a Default Nick Name");
                    return;
                }
            }
            CurrentStep++;
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            CurrentStep--;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            _nickName = "Pony_" + rnd.Next(0, 999999999);
            MakeDefaultFiles();

            this.Close();
        }
    }            
}

public class IniParser
{
    private String iniFilePath;
    private Dictionary<SectionPair, string> keyPairs = new Dictionary<SectionPair, string>();

    private struct SectionPair
    {
        public string Section;
        public string Key;
    }

    public IniParser(String iniPath)
    {
        TextReader iniFile = null;
        String strLine = null;
        String currentRoot = null;
        String[] keyPair = null;

        iniFilePath = iniPath;

        if (File.Exists(iniPath))
        {
            try
            {
                iniFile = new StreamReader(iniPath);

                strLine = iniFile.ReadLine();

                while (strLine != null)
                {
                    //strLine = strLine.Trim().ToUpper();
                    strLine = strLine.Trim();

                    if (strLine != "")
                    {
                        if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                        {
                            currentRoot = strLine.Substring(1, strLine.Length - 2);
                        }
                        else
                        {
                            keyPair = strLine.Split(new char[] { '=' }, 2);

                            SectionPair sectionPair;
                            String value = null;

                            if (currentRoot == null)
                                currentRoot = "ROOT";

                            sectionPair.Section = currentRoot;
                            sectionPair.Key = keyPair[0];

                            if (keyPair.Length > 1)
                                value = keyPair[1];

                            //System.Diagnostics.Debug.WriteLine(value + ":" + keyPair[0] + ":" + currentRoot);

                            keyPairs.Add(sectionPair, value);
                        }
                    }

                    strLine = iniFile.ReadLine();
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw ex;
            }
            finally
            {
                if (iniFile != null)
                    iniFile.Close();
            }
        }
        else
            throw new FileNotFoundException("Unable to locate " + iniPath);

    }

    public String GetSetting(String sectionName, String settingName, String defaultValue)
    {
        SectionPair sectionPair;
        sectionPair.Section = sectionName;
        sectionPair.Key = settingName;
        try
        {
            if (keyPairs[sectionPair] == null)
                return defaultValue;
            else
                return (string)keyPairs[sectionPair];
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    public bool GetSettingBool(String sectionName, String settingName, bool defaultValue)
    {
        SectionPair sectionPair;
        sectionPair.Section = sectionName;
        sectionPair.Key = settingName;

        bool value = defaultValue;
        try
        {
            if (keyPairs[sectionPair].ToString().Equals("1"))
                value = true;
        }
        catch { }
        return value;
    }

    public String[] EnumSection(String sectionName)
    {
        ArrayList tmpArray = new ArrayList();

        foreach (SectionPair pair in keyPairs.Keys)
        {
            if (pair.Section.ToUpper() == sectionName.ToUpper())
            {
                //these are added out of order, we need to sort them
                tmpArray.Add(keyPairs[pair].ToString().Replace(((char)3).ToString(), "&#x3;").Replace(((char)2).ToString(), "&#x2;").Replace(((char)0).ToString(), ""));
            }
        }

        return (String[])tmpArray.ToArray(typeof(String));
    }

    public String[] EnumSectionTheme(String sectionName)
    {
        ArrayList tmpArray = new ArrayList();

        foreach (SectionPair pair in keyPairs.Keys)
        {
            if (pair.Section.ToUpper() == sectionName.ToUpper())
            {
                //these are added out of order, we need to sort them
                tmpArray.Add(pair.Key + (char)255 + keyPairs[pair].ToString().Replace(((char)3).ToString(), "&#x3;").Replace(((char)2).ToString(), "&#x2;").Replace(((char)0).ToString(), ""));
            }
        }

        return (String[])tmpArray.ToArray(typeof(String));
    }
}