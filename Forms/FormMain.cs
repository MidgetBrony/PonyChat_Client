/******************************************************************************\
 * IceChat 9 Internet Relay Chat Client
 * PonyChat IRC Client
 * 
 * Copyright (C) 2013 Paul Vanderzee <snerf@icechat.net>
 *                                    <www.icechat.net> 
 *                                    
 * Copyright (C) 2013 MidgetBrony <MidgetBrony@ponychat.net>
 *                              
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
 * Please consult the LICENSE.md file included with this project for
 * more details
 *
\******************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

using IceChatPlugin;
using IceChatTheme;

namespace IceChat
{
    public partial class FormMain : Form
    {
        public static FormMain Instance;

        private string optionsFile;
        private string messagesFile;
        private string fontsFile;
        private string colorsFile;
        private string soundsFile; 
        private string favoriteChannelsFile;
        private string serversFile;
        private string aliasesFile;
        private string popupsFile;
        private string pluginsFile;
        private string channelSettingsFile;

        private string currentFolder;
        private string logsFolder;
        private string pluginsFolder;
        private string emoticonsFile;
        private string scriptsFolder;
        private string soundsFolder;
        private string picturesFolder;

        private List<LanguageItem> languageFiles;
        private LanguageItem currentLanguageFile;

        private StreamWriter errorFile;

        private IceChatOptions iceChatOptions;
        private IceChatColors iceChatColors;
        private IceChatMessageFormat iceChatMessages;
        private IceChatFontSetting iceChatFonts;
        private IceChatAliases iceChatAliases;
        private IceChatPopupMenus iceChatPopups;
        private IceChatEmoticon iceChatEmoticons;
        private IceChatLanguage iceChatLanguage;
        private IceChatSounds iceChatSounds;
        private IceChatPluginFile iceChatPlugins;
        private ChannelSettings channelSettings;
        
        private bool IsForeGround;
        //private System.Threading.Mutex mutex;

        private List<IPluginIceChat> loadedPlugins;
        private List<IThemeIceChat> loadedPluginThemes;

        private IdentServer identServer;

        private delegate IceTabPage AddWindowDelegate(IRCConnection connection, string windowName, IceTabPage.WindowType windowType);

        private delegate void CurrentWindowDelegate(string data, int color);
        private delegate void WindowMessageDelegate(IRCConnection connection, string name, string data, int color, bool scrollToBottom);
        private delegate void CurrentWindowMessageDelegate(IRCConnection connection, string data, int color, bool scrollToBottom);
        private delegate void AddInputpanelTextDelegate(string data);
        private delegate void AddPanelDelegate(Panel panel);

        private System.Timers.Timer flashTrayIconTimer;
        private int flashTrayCount;

        private System.Timers.Timer flashTaskBarIconTimer;
        private int flashTaskBarCount;

        private System.Media.SoundPlayer player;
        private MP3Player mp3Player;

        private bool muteAllSounds;
        private string autoStartCommand = null;
        private FormWindowState previousWindowState;

        private ToolStripRenderer menuRenderer = null;
        private ToolStripRenderer toolStripRender = null;

        [StructLayout(LayoutKind.Sequential)]
        private struct OSVERSIONINFOEX
        {
            public int dwOSVersionInfoSize;
            public int dwMajorVersion;
            public int dwMinorVersion;
            public int dwBuildNumber;
            public int dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
            public short wServicePackMajor;
            public short wServicePackMinor;
            public short wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public Int32 dwFlags;
            public UInt32 uCount;
            public Int32 dwTimeout;
        }

        [DllImport("kernel32.dll")]
        private static extern bool GetVersionEx(ref OSVERSIONINFOEX osVersionInfo);

        [DllImport("user32.dll")]
        private static extern Int32 FlashWindowEx(ref FLASHWINFO pwfi);

        private const int VER_NT_WORKSTATION = 1;
        private const int VER_NT_DOMAIN_CONTROLLER = 2;
        private const int VER_NT_SERVER = 3;
        private const int VER_SUITE_SMALLBUSINESS = 1;
        private const int VER_SUITE_ENTERPRISE = 2;
        private const int VER_SUITE_TERMINAL = 16;
        private const int VER_SUITE_DATACENTER = 128;
        private const int VER_SUITE_SINGLEUSERTS = 256;
        private const int VER_SUITE_PERSONAL = 512;
        private const int VER_SUITE_BLADE = 1024;

        public const string ProgramID = "PonyChat IRC";
        public const string VersionID = "Beta 1.0";

        /// <summary>
        /// All the Window Message Types used for Coloring the Tab Text for Different Events
        /// </summary>
        internal enum ServerMessageType
        {
            Default = 0,
            Message = 1,            
            Action = 2,
            JoinChannel = 3,
            PartChannel = 4,
            QuitServer = 5,
            ServerMessage = 6,
            Other = 7,
            ServerNotice = 8
        }

        public FormMain(string[] args, Form splash)
        {
            FormMain.Instance = this;

            if (StaticMethods.IsRunningOnMono())
                player = new System.Media.SoundPlayer();
            else
                mp3Player = new MP3Player();

            bool forceCurrentFolder = false;

            if (args.Length > 0)
            {
                string prevArg = "";
                foreach (string arg in args)
                {
                    if (prevArg.Length == 0)
                    {
                        prevArg = arg;
                        if (arg.ToLower().StartsWith("irc://"))
                        {
                            //parse out the server name and channel name
                            string server = arg.Substring(6).TrimEnd();
                            if (server.IndexOf("/") != -1)
                            {
                                string host = server.Split('/')[0];
                                string channel = server.Split('/')[1];
                                autoStartCommand = "/joinserv " + host + " #" + channel;
                            }
                            else
                                autoStartCommand = "/server " + arg.Substring(6).TrimEnd();

                        }
                    }
                    else
                    {
                        switch (prevArg.ToLower())
                        {
                            case "-profile":
                                currentFolder = arg;
                                //check if the folder exists, ir not, create it
                                if (!Directory.Exists(currentFolder))
                                    Directory.CreateDirectory(currentFolder);

                                forceCurrentFolder = true;
                                break;
                        }

                        prevArg = "";
                    }
                }
            }
            
            //mutex = new System.Threading.Mutex(true, "IceChatMutex");

            #region Settings Files 
            
            //check if the xml settings files exist in current folder
            if (currentFolder == null)
                currentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (!File.Exists(currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatServer.xml") && !forceCurrentFolder)
            {
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + "PonyChat" + Path.DirectorySeparatorChar + "Client"))
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + "PonyChat" + Path.DirectorySeparatorChar + "Client");

                currentFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + "PonyChat" + Path.DirectorySeparatorChar + "Client";
            }

            //load all files from the Local AppData folder, unless it exist in the current folder
            serversFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatServer.xml";
            optionsFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatOptions.xml";
            messagesFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatMessages.xml";
            fontsFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatFonts.xml";
            colorsFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatColors.xml";
            soundsFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatSounds.xml";
            favoriteChannelsFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatChannels.xml";
            aliasesFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatAliases.xml";
            popupsFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatPopups.xml";
            pluginsFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatPlugins.xml";
            emoticonsFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "Emoticons" + System.IO.Path.DirectorySeparatorChar + "IceChatEmoticons.xml";
            channelSettingsFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "ChannelSettings.xml";

            logsFolder = currentFolder + System.IO.Path.DirectorySeparatorChar + "Logs";
            scriptsFolder = currentFolder + System.IO.Path.DirectorySeparatorChar + "Scripts";
            soundsFolder = currentFolder + System.IO.Path.DirectorySeparatorChar + "Sounds";
            picturesFolder = currentFolder + System.IO.Path.DirectorySeparatorChar + "Pictures";

            //pluginsFolder = currentFolder + System.IO.Path.DirectorySeparatorChar + "Plugins";
            pluginsFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + System.IO.Path.DirectorySeparatorChar + "Plugins";
            //pluginsFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        
            if (!Directory.Exists(pluginsFolder))
                Directory.CreateDirectory(pluginsFolder);

            if (!Directory.Exists(scriptsFolder))
                Directory.CreateDirectory(scriptsFolder);

            if (!Directory.Exists(soundsFolder))
                Directory.CreateDirectory(soundsFolder);

            if (!Directory.Exists(picturesFolder))
                Directory.CreateDirectory(picturesFolder);

            if (!Directory.Exists(currentFolder + Path.DirectorySeparatorChar + "Update"))
                Directory.CreateDirectory(currentFolder + Path.DirectorySeparatorChar + "Update");

            #endregion

            languageFiles = new List<LanguageItem>();
            
            DirectoryInfo languageDirectory = null;

            languageDirectory = new DirectoryInfo(currentFolder + System.IO.Path.DirectorySeparatorChar + "Languages");
            if (!Directory.Exists(currentFolder + System.IO.Path.DirectorySeparatorChar + "Languages"))
                Directory.CreateDirectory(currentFolder + System.IO.Path.DirectorySeparatorChar + "Languages");
            
            if (languageDirectory != null)
            {
                // scan the language directory for xml files and make LanguageItems for each file
                FileInfo[] langFiles = languageDirectory.GetFiles("*.xml");
                foreach (FileInfo fi in langFiles)
                {                    
                    string langFile = languageDirectory.FullName + System.IO.Path.DirectorySeparatorChar + fi.Name;
                    LanguageItem languageItem = LoadLanguageItem(langFile);
                    if (languageItem != null) languageFiles.Add(languageItem);                    
                }

                if (languageFiles.Count == 0)
                {
                    currentLanguageFile = new LanguageItem();
                    languageFiles.Add(currentLanguageFile);     // default language English
                }
            }

            LoadOptions();
            LoadColors();
            LoadSounds();

            // use the language saved in options if availlable,
            // if not (e.g. user deleted xml file) default is used
            foreach (LanguageItem li in languageFiles)
            {
                if (li.LanguageName == iceChatOptions.Language)
                {
                    currentLanguageFile = li;
                    break;
                }
            }

            LoadLanguage(); // The language class MUST be loaded before any GUI component is created

            //check if we have any servers/settings saved, if not, load firstrun
            if (!File.Exists(serversFile))
            {
                FormFirstRun firstRun = new FormFirstRun(currentFolder);
                firstRun.SaveOptions += new FormFirstRun.SaveOptionsDelegate(FirstRunSaveOptions);
                firstRun.ShowDialog(this);
            }

            InitializeComponent();

            //load icons from Embedded Resources
            this.Icon = System.Drawing.Icon.FromHandle(StaticMethods.LoadResourceImage("PonyChat.ico").GetHicon());
            this.notifyIcon.Icon = System.Drawing.Icon.FromHandle(StaticMethods.LoadResourceImage("PonyChat.ico").GetHicon());

            this.notifyIcon.Visible = iceChatOptions.ShowSytemTrayIcon;

            //disable this by default
            this.toolStripUpdate.Visible = false;

            serverListToolStripMenuItem.Checked = iceChatOptions.ShowServerTree;
            panelDockLeft.Visible = serverListToolStripMenuItem.Checked;
            splitterLeft.Visible = serverListToolStripMenuItem.Checked;

            nickListToolStripMenuItem.Checked = iceChatOptions.ShowNickList;
            panelDockRight.Visible = nickListToolStripMenuItem.Checked;
            panelDockRight.TabControl.Alignment = TabAlignment.Right;
            splitterRight.Visible = nickListToolStripMenuItem.Checked;
            
            statusBarToolStripMenuItem.Checked = iceChatOptions.ShowStatusBar;
            statusStripMain.Visible = statusBarToolStripMenuItem.Checked;

            mainChannelBar.Visible = iceChatOptions.ShowTabBar;

            this.Text = ProgramID + " - " + VersionID;
            this.notifyIcon.Text = ProgramID + " :: " + VersionID;

            if (!Directory.Exists(logsFolder))
                Directory.CreateDirectory(logsFolder);

            try
            {
                errorFile = new StreamWriter(logsFolder + System.IO.Path.DirectorySeparatorChar + "errors.log", true);
            }
            catch (IOException io)
            {
                System.Diagnostics.Debug.WriteLine("Can not create errors.log:" + io.Message);
            }
            catch (Exception eo)
            {
                System.Diagnostics.Debug.WriteLine("Can not create errors.log:" + eo.Message);
            }

            if (!iceChatOptions.TimeStamp.EndsWith(" "))
                iceChatOptions.TimeStamp += " ";

            if (iceChatOptions.WindowSize != null)
            {
                if (!iceChatOptions.SaveWindowPosition)
                {
                    this.Width = Screen.PrimaryScreen.WorkingArea.Width;
                    this.Height = Screen.PrimaryScreen.WorkingArea.Height;
                }
                else
                {
                    if (iceChatOptions.WindowSize.Width != 0)
                    {
                        this.Size = iceChatOptions.WindowSize;
                        this.WindowState = iceChatOptions.WindowState;
                    }
                    else
                    {
                        this.Width = Screen.PrimaryScreen.WorkingArea.Width;
                        this.Height = Screen.PrimaryScreen.WorkingArea.Height;
                    }
                }
            }

            if (iceChatOptions.WindowLocation != null && iceChatOptions.SaveWindowPosition)
            {
                //check if the location is valid, could try and place it on a 2nd screen that no longer exists
                if (Screen.AllScreens.Length == 1)
                {
                   if (Screen.PrimaryScreen.Bounds.Contains(iceChatOptions.WindowLocation)) 
                        this.Location = iceChatOptions.WindowLocation;

                }
                else
                {
                    //check if we are in the bounds of the screen location
                    foreach (Screen screen in Screen.AllScreens)
                        if (screen.Bounds.Contains(iceChatOptions.WindowLocation))
                            this.Location = iceChatOptions.WindowLocation;                            
                }                
            }
            
            statusStripMain.Visible = iceChatOptions.ShowStatusBar;

            LoadAliases();
            LoadPopups();
            LoadEmoticons();
            LoadMessageFormat();
            LoadFonts();

            bool fileThemeFound = true;

            if (iceChatOptions.CurrentTheme == null)
            {
                iceChatOptions.CurrentTheme = "Default";
                defaultToolStripMenuItem.Checked = true;
            }
            else
            {
                //load in the new color theme, if it not Default
                if (iceChatOptions.CurrentTheme != "Default")
                {
                    string themeFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "Colors-" + iceChatOptions.CurrentTheme + ".xml";
                    if (File.Exists(themeFile))
                    {
                        XmlSerializer deserializer = new XmlSerializer(typeof(IceChatColors));
                        TextReader textReader = new StreamReader(themeFile);
                        iceChatColors = (IceChatColors)deserializer.Deserialize(textReader);
                        textReader.Close();
                        textReader.Dispose();
                        
                        colorsFile = themeFile;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Color Theme File not found:" + themeFile);
                        fileThemeFound = false;
                    }

                    themeFile = currentFolder + System.IO.Path.DirectorySeparatorChar + "Messages-" + iceChatOptions.CurrentTheme + ".xml";
                    if (File.Exists(themeFile))
                    {
                        XmlSerializer deserializer = new XmlSerializer(typeof(IceChatMessageFormat));
                        TextReader textReader = new StreamReader(themeFile);
                        iceChatMessages = (IceChatMessageFormat)deserializer.Deserialize(textReader);
                        textReader.Close();
                        textReader.Dispose();

                        messagesFile = themeFile;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Messages Theme File not found:" + themeFile);
                        fileThemeFound = false;
                    }
                }
                else
                    defaultToolStripMenuItem.Checked = true;

            }

            if (iceChatOptions.Theme == null)
            {
                iceChatOptions.Theme = new ThemeItem[1];
                iceChatOptions.Theme[0] = new ThemeItem();
                iceChatOptions.Theme[0].ThemeName = "Default";                
                iceChatOptions.Theme[0].ThemeType = "XML";
                iceChatOptions.CurrentTheme = "Default";
                System.Diagnostics.Debug.WriteLine("create default theme");
                defaultToolStripMenuItem.Checked = true;

            }

            channelList = new ChannelList();            
            channelList.Dock = DockStyle.Fill;
            buddyList = new BuddyList();
            buddyList.Dock = DockStyle.Fill;

            statusStripMain.BackColor = IrcColor.colors[iceChatColors.StatusbarBackColor];
            toolStripStatus.ForeColor = IrcColor.colors[iceChatColors.StatusbarForeColor];
            inputPanel.SetInputBoxColors();
            channelList.SetListColors();
            buddyList.SetListColors();

            this.nickList.Header = iceChatLanguage.consoleTabTitle;

            this.nickListTab.BackColor = IrcColor.colors[iceChatColors.PanelHeaderBG1];
            this.nickListTab.ForeColor = IrcColor.colors[iceChatColors.PanelHeaderForeColor];
            

            serverListTab.BackColor = IrcColor.colors[iceChatColors.PanelHeaderBG1];
            serverListTab.ForeColor = IrcColor.colors[iceChatColors.PanelHeaderForeColor];

            channelListTab = new TabPage("Favorite Channels");
            Panel channelPanel = new Panel();
            channelPanel.Dock = DockStyle.Fill;
            channelPanel.Controls.Add(channelList);
            channelListTab.Controls.Add(channelPanel);
            channelListTab.BackColor = IrcColor.colors[iceChatColors.PanelHeaderBG1];
            channelListTab.ForeColor = IrcColor.colors[iceChatColors.PanelHeaderForeColor];

            buddyListTab = new TabPage("Buddy List");
            Panel buddyPanel = new Panel();
            buddyPanel.Dock = DockStyle.Fill;
            buddyPanel.Controls.Add(buddyList);
            buddyListTab.Controls.Add(buddyPanel);
            buddyListTab.BackColor = IrcColor.colors[iceChatColors.PanelHeaderBG1];
            buddyListTab.ForeColor = IrcColor.colors[iceChatColors.PanelHeaderForeColor];

            panelDockLeft.Width = iceChatOptions.LeftPanelWidth;
            panelDockRight.Width = iceChatOptions.RightPanelWidth;

            //Load the panel items in order
            if (iceChatOptions.LeftPanels != null)
            {
                foreach (string arrayitem in iceChatOptions.LeftPanels)
                {
                    if (arrayitem == serverListTab.Text)
                        this.panelDockLeft.TabControl.TabPages.Add(serverListTab);
                    else if (arrayitem == channelListTab.Text)
                        this.panelDockLeft.TabControl.TabPages.Add(channelListTab);
                    else if (arrayitem == nickListTab.Text)
                        this.panelDockLeft.TabControl.TabPages.Add(nickListTab);
                    else if (arrayitem == buddyListTab.Text)
                        this.panelDockLeft.TabControl.TabPages.Add(buddyListTab);
                }
            }
            if (iceChatOptions.RightPanels != null)
            {
                foreach (string arrayitem in iceChatOptions.RightPanels)
                {
                    if (arrayitem == serverListTab.Text)
                        this.panelDockRight.TabControl.TabPages.Add(serverListTab);
                    else if (arrayitem == nickListTab.Text)
                        this.panelDockRight.TabControl.TabPages.Add(nickListTab);
                    else if (arrayitem == channelListTab.Text)
                        this.panelDockRight.TabControl.TabPages.Add(channelListTab);
                    else if (arrayitem == buddyListTab.Text)
                        this.panelDockRight.TabControl.TabPages.Add(buddyListTab);
                }
            }

            //If any panels are missing
            if (!panelDockLeft.TabControl.TabPages.Contains(serverListTab) && !panelDockRight.TabControl.TabPages.Contains(serverListTab))
                this.panelDockLeft.TabControl.TabPages.Add(serverListTab);
            if (!panelDockLeft.TabControl.TabPages.Contains(nickListTab) && !panelDockRight.TabControl.TabPages.Contains(nickListTab))
                this.panelDockRight.TabControl.TabPages.Add(nickListTab);
            if (!panelDockLeft.TabControl.TabPages.Contains(channelListTab) && !panelDockRight.TabControl.TabPages.Contains(channelListTab))
                this.panelDockRight.TabControl.TabPages.Add(channelListTab);
            if (!panelDockLeft.TabControl.TabPages.Contains(buddyListTab) && !panelDockRight.TabControl.TabPages.Contains(buddyListTab))
                this.panelDockRight.TabControl.TabPages.Add(buddyListTab);
            
          
            //hide the left or right panel if it is empty
            if (panelDockLeft.TabControl.TabPages.Count == 0)
            {
                this.splitterLeft.Visible = false;
                panelDockLeft.Visible = false;
            }
            if (panelDockRight.TabControl.TabPages.Count == 0)
            {
                this.splitterRight.Visible = false;
                panelDockRight.Visible = false;
            }


            nickList.Font = new Font(iceChatFonts.FontSettings[3].FontName, iceChatFonts.FontSettings[3].FontSize);
            serverTree.Font = new Font(iceChatFonts.FontSettings[4].FontName, iceChatFonts.FontSettings[4].FontSize);
            mainChannelBar.TabFont = new Font(iceChatFonts.FontSettings[8].FontName, iceChatFonts.FontSettings[8].FontSize);


            inputPanel.OnCommand +=new InputPanel.OnCommandDelegate(inputPanel_OnCommand);
            inputPanel.InputBoxFont = new Font(iceChatFonts.FontSettings[5].FontName, iceChatFonts.FontSettings[5].FontSize);

            inputPanel.ShowColorPicker = iceChatOptions.ShowColorPicker;
            inputPanel.ShowEmoticonPicker = iceChatOptions.ShowEmoticonPicker;
            inputPanel.ShowSendButton = iceChatOptions.ShowSendButton;

            inputPanel.ShowWideTextPanel = iceChatOptions.ShowMultilineEditbox;

            if (iceChatOptions.ShowEmoticons == false)
                inputPanel.ShowEmoticonPicker = false;

            mainChannelBar.OnTabClosed += new ChannelBar.TabClosedDelegate(mainTabControl_OnTabClosed);
            mainChannelBar.SelectedIndexChanged += new ChannelBar.TabEventHandler(TabSelectedIndexChanged);

            panelDockLeft.Initialize();
            panelDockRight.Initialize();

            if (iceChatOptions.DockLeftPanel == true)
                panelDockLeft.DockControl();

            if (iceChatOptions.DockRightPanel == true)
                panelDockRight.DockControl();

            CreateDefaultConsoleWindow();
            LoadChannelSettings();

            //****
            WindowMessage(null, "Console", "Data Folder: " + currentFolder, 4, true);
            WindowMessage(null, "Console", "Plugins Folder: " + pluginsFolder, 4, true);

            serverTree.NewServerConnection += new NewServerConnectionDelegate(NewServerConnection);
            serverTree.SaveDefault += new ServerTree.SaveDefaultDelegate(OnDefaultServerSettings);

            loadedPlugins = new List<IPluginIceChat>();
            loadedPluginThemes = new List<IThemeIceChat>();

            //load any plugin addons
            LoadPlugins();

            //load the plugin settings file
            LoadPluginFiles();
            
            //****
            WindowMessage(null, "Console", "Using Theme - " + iceChatOptions.CurrentTheme, 4, true);

            //set any plugins as disabled
            //add any items top the pluginsFile if they do not exist, or remove any that do not
            foreach (IPluginIceChat ipc in loadedPlugins)
            {
                bool found = false;
                for (int i = 0; i < iceChatPlugins.listPlugins.Count; i++)
                {
                    if (iceChatPlugins.listPlugins[i].PluginFile.Equals(ipc.FileName))
                    {
                        found = true;

                        //check if the plugin is enabled or not
                        if (iceChatPlugins.listPlugins[i].Enabled == false)
                        {
                            WindowMessage(null, "Console", "Disabled Plugin - " + ipc.Name + " v" + ipc.Version, 4, true);

                            foreach (ToolStripMenuItem t in pluginsToolStripMenuItem.DropDownItems)
                                if (t.ToolTipText.ToLower() == ipc.FileName.ToLower())
                                    t.Image = StaticMethods.LoadResourceImage("CloseButton.png");

                            ipc.Enabled = false;
                        }
                    }
                }

                if (found == false)
                {
                    //plugin file not found in plugin Items file, add it
                    PluginItem item = new PluginItem();
                    item.Enabled = true;
                    item.PluginFile = ipc.FileName;
                    iceChatPlugins.AddPlugin(item);
                    SavePluginFiles();
                }
            }

            if (iceChatPlugins.listPlugins.Count != loadedPlugins.Count)
            {
                //find the file that is missing
                List<int> removeItems = new List<int>();
                for (int i = 0; i < iceChatPlugins.listPlugins.Count; i++)
                {
                    bool found = false;
                    foreach (IPluginIceChat ipc in loadedPlugins)
                    {
                        if (iceChatPlugins.listPlugins[i].PluginFile.Equals(ipc.FileName))
                            found = true;
                    }

                    if (found == false)
                        removeItems.Add(i);
                }

                if (removeItems.Count > 0)
                {
                    try
                    {
                        foreach (int i in removeItems)
                            iceChatPlugins.listPlugins.Remove(iceChatPlugins.listPlugins[i]);
                    }
                    catch { }

                    SavePluginFiles();
                }
            }
            //initialize each of the plugins on its own thread
            foreach (IPluginIceChat ipc in loadedPlugins)
            {
                if (ipc.Enabled == true)
                {
                    System.Threading.Thread initPlugin = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(InitializePlugin));
                    initPlugin.Start(ipc);                    
                    //ipc.MainProgramLoaded();
                }
            }

            if (fileThemeFound == false)
            {
                //check for the Plugin File theme
                foreach (IThemeIceChat theme in FormMain.Instance.IceChatPluginThemes)
                {
                    if (theme.Name == iceChatOptions.CurrentTheme)
                    {
                        //update, this is the match
                        iceChatColors.ChannelAdminColor = theme.ChannelAdminColor;
                        iceChatColors.ChannelBackColor = theme.ChannelBackColor;
                        iceChatColors.ChannelHalfOpColor = theme.ChannelHalfOpColor;
                        iceChatColors.ChannelJoinColorChange = theme.ChannelJoinColorChange;
                        iceChatColors.ChannelListBackColor = theme.ChannelListBackColor;
                        iceChatColors.ChannelListForeColor = theme.ChannelListForeColor;
                        iceChatColors.ChannelOpColor = theme.ChannelOpColor;
                        iceChatColors.ChannelOwnerColor = theme.ChannelOwnerColor;
                        iceChatColors.ChannelPartColorChange = theme.ChannelPartColorChange;
                        iceChatColors.ChannelRegularColor = theme.ChannelRegularColor;
                        iceChatColors.ChannelVoiceColor = theme.ChannelVoiceColor;
                        iceChatColors.ConsoleBackColor = theme.ConsoleBackColor;
                        iceChatColors.InputboxBackColor = theme.InputboxBackColor;
                        iceChatColors.InputboxForeColor = theme.InputboxForeColor;
                        iceChatColors.MenubarBackColor = theme.MenubarBackColor;
                        iceChatColors.NewMessageColorChange = theme.NewMessageColorChange;
                        iceChatColors.NickListBackColor = theme.NickListBackColor;
                        iceChatColors.OtherMessageColorChange = theme.OtherMessageColorChange;
                        iceChatColors.PanelHeaderBG1 = theme.PanelHeaderBG1;
                        iceChatColors.PanelHeaderBG2 = theme.PanelHeaderBG2;
                        iceChatColors.PanelHeaderForeColor = theme.PanelHeaderForeColor;
                        iceChatColors.QueryBackColor = theme.QueryBackColor;
                        iceChatColors.RandomizeNickColors = theme.RandomizeNickColors;
                        iceChatColors.ServerListBackColor = theme.ServerListBackColor;
                        iceChatColors.ServerMessageColorChange = theme.ServerMessageColorChange;
                        iceChatColors.ServerQuitColorChange = theme.ServerQuitColorChange;
                        iceChatColors.StatusbarBackColor = theme.StatusbarBackColor;
                        iceChatColors.StatusbarForeColor = theme.StatusbarForeColor;
                        iceChatColors.TabBarChannelJoin = theme.TabBarChannelJoin;
                        iceChatColors.TabBarChannelPart = theme.TabBarChannelPart;
                        iceChatColors.TabBarCurrent = theme.TabBarCurrent;
                        iceChatColors.TabBarDefault = theme.TabBarDefault;
                        iceChatColors.TabBarNewMessage = theme.TabBarNewMessage;
                        iceChatColors.TabBarOtherMessage = theme.TabBarOtherMessage;
                        iceChatColors.TabBarServerMessage = theme.TabBarServerMessage;
                        iceChatColors.TabBarServerQuit = theme.TabBarServerQuit;
                        iceChatColors.ToolbarBackColor = theme.ToolbarBackColor;
                        iceChatColors.UnreadTextMarkerColor = theme.UnreadTextMarkerColor;

                        inputPanel.SetInputBoxColors();

                        //menuMainStrip.BackColor = IrcColor.colors[iceChatColors.MenubarBackColor];
                        statusStripMain.BackColor = IrcColor.colors[iceChatColors.StatusbarBackColor];
                        toolStripStatus.ForeColor = IrcColor.colors[iceChatColors.StatusbarForeColor];

                        serverListTab.BackColor = IrcColor.colors[iceChatColors.PanelHeaderBG1];
                        serverListTab.ForeColor = IrcColor.colors[iceChatColors.PanelHeaderForeColor];
                        nickListTab.BackColor = IrcColor.colors[iceChatColors.PanelHeaderBG1];
                        nickListTab.ForeColor = IrcColor.colors[iceChatColors.PanelHeaderForeColor];
                        channelListTab.BackColor = IrcColor.colors[iceChatColors.PanelHeaderBG1];
                        channelListTab.ForeColor = IrcColor.colors[iceChatColors.PanelHeaderForeColor];
                        buddyListTab.BackColor = IrcColor.colors[iceChatColors.PanelHeaderBG1];
                        buddyListTab.ForeColor = IrcColor.colors[iceChatColors.PanelHeaderForeColor];

                        System.Diagnostics.Debug.WriteLine("set colors");

                        inputPanel.SetInputBoxColors();
                        channelList.SetListColors();
                        buddyList.SetListColors();
                        nickList.Invalidate();
                        serverTree.Invalidate();
                        mainChannelBar.Invalidate();
                    }
                }
            }

            //add the themes to the view menu
            if (iceChatOptions.Theme != null)
            {
                foreach (ThemeItem theme in FormMain.Instance.IceChatOptions.Theme)
                {
                    if (!theme.ThemeName.Equals("Default"))
                    {
                        ToolStripMenuItem t = new ToolStripMenuItem(theme.ThemeName);
                        if (iceChatOptions.CurrentTheme == theme.ThemeName)
                            t.Checked = true;
                        
                        t.Click += new EventHandler(themeChoice_Click);
                        themesToolStripMenuItem.DropDownItems.Add(t);
                    }
                }
            }

            this.FormClosing += new FormClosingEventHandler(FormMainClosing);
            this.Resize += new EventHandler(FormMainResize);

            if (iceChatOptions.IdentServer && !System.Diagnostics.Debugger.IsAttached)
                identServer = new IdentServer();

            if (iceChatLanguage.LanguageName != "English") ApplyLanguage(); // ApplyLanguage can first be called after all child controls are created

            //get a new router ip
            if (iceChatOptions.DCCAutogetRouterIP == true)
            {
                System.Threading.Thread dccThread = new System.Threading.Thread(getLocalIPAddress);
                dccThread.Name = "DCCIPAutoUpdate";
                dccThread.Start();
            }

            splash.Close();
            splash.Dispose();


            this.Activated += new EventHandler(FormMainActivated);

            nickList.ShowNickButtons = iceChatOptions.ShowNickButtons;
            serverTree.ShowServerButtons = iceChatOptions.ShowServerButtons;

            // check for background images for nicklist and server tree
            if (iceChatOptions.NickListImage != null && iceChatOptions.NickListImage.Length > 0)
            {
                if (File.Exists(picturesFolder + System.IO.Path.DirectorySeparatorChar + iceChatOptions.NickListImage))
                    this.nickList.BackGroundImage = picturesFolder + System.IO.Path.DirectorySeparatorChar + iceChatOptions.NickListImage;
                else if (File.Exists(iceChatOptions.NickListImage))
                    this.nickList.BackGroundImage = iceChatOptions.NickListImage;

            }
            if (iceChatOptions.ServerTreeImage != null && iceChatOptions.ServerTreeImage.Length > 0)
            {
                if (File.Exists(picturesFolder + System.IO.Path.DirectorySeparatorChar + iceChatOptions.ServerTreeImage))
                    this.serverTree.BackGroundImage = picturesFolder + System.IO.Path.DirectorySeparatorChar + iceChatOptions.ServerTreeImage;
                else if (File.Exists(iceChatOptions.NickListImage))
                    this.serverTree.BackGroundImage = iceChatOptions.ServerTreeImage;
            }

            this.flashTrayIconTimer = new System.Timers.Timer(2000);
            this.flashTrayIconTimer.Enabled = false;            
            this.flashTrayIconTimer.Elapsed += new System.Timers.ElapsedEventHandler(flashTrayIconTimer_Elapsed);
            this.notifyIcon.Tag = "off";
            this.flashTrayCount = 0;

            this.flashTaskBarIconTimer = new System.Timers.Timer(2000);
            this.flashTaskBarIconTimer.Enabled = false;
            this.flashTaskBarIconTimer.Elapsed += new System.Timers.ElapsedEventHandler(flashTaskBarIconTimer_Elapsed);
            this.Tag = "off";
            this.flashTrayCount = 0;


            //new toolstrip renderer for the main menu strip
            menuMainStrip.RenderMode = ToolStripRenderMode.System;

            //System.Diagnostics.Debug.WriteLine(inputPanel.ShowEmoticonPicker);

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                //check for an update and setup DDE, if NOT in debugger            
                System.Threading.Thread checkThread = new System.Threading.Thread(checkForUpdate);
                checkThread.Name = "CheckUpdateThread";
                checkThread.Start();

                //SetupIRCDDE();
            }

        }

        private void FormMainActivated(object sender, EventArgs e)
        {
            if (iceChatOptions.IsOnTray == true)
            {
                minimizeToTrayToolStripMenuItem.PerformClick();
                IsForeGround = false;
            }
            else
                IsForeGround = true;


            //check if we have an autostart command from the irc://
            if (autoStartCommand != null && autoStartCommand.Length > 0)
                ParseOutGoingCommand(null, autoStartCommand);
            else
            {
                //auto start any Auto Connect Servers
                foreach (ServerSetting s in serverTree.ServersCollection.listServers)
                {
                    if (s.AutoStart)
                    {
                        NewServerConnection(s);
                    }
                }
            }
            
            //remove the event handler, because it only needs to be run once, on startup
            this.Activated -= FormMainActivated;

            this.Activated += new EventHandler(FormMain_Activated);
            this.Deactivate += new EventHandler(FormMain_Deactivate);
        }

        private void SetupIRCDDE()
        {
            
            //check if we have elevated (admin level)
            //System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            //System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            //bool isElevated = principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);

            //MessageBox.Show("is elevated:" + isElevated);
            /*
            if (GetOperatingSystemName().IndexOf("Windows") > -1 && isElevated == true)
            {
                Microsoft.Win32.RegistryKey rKey = null;
                try
                {
                    rKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("irc", true);
                    if (rKey != null)
                    {
                        Microsoft.Win32.RegistryKey sKey = rKey.OpenSubKey(@"shell\open\command");
                        string key = sKey.GetValue("").ToString();
                        
                        //check if the path is the same
                        if (key.IndexOf(Application.ExecutablePath) != 1)
                        {
                            //path is not the same, delete it
                            Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(@"irc\shell");
                            Microsoft.Win32.Registry.ClassesRoot.DeleteSubKey(@"irc");
                        }
                        rKey.Close();
                        rKey = null;
                    }
                }
                catch (System.UnauthorizedAccessException)
                {
                    //MessageBox.Show("cant delete");
                    rKey.Close();
                    return;
                }
                catch (System.Exception)
                {
                    //MessageBox.Show("error");
                    rKey.Close();
                    return;    
                }

                if (rKey == null)
                {
                    try
                    {
                        string user = Environment.UserDomainName + "\\" + Environment.UserName;

                        RegistrySecurity rs = new RegistrySecurity();
                        rs.AddAccessRule(new RegistryAccessRule(user,
                                    RegistryRights.FullControl,
                                    InheritanceFlags.None,
                                    PropagationFlags.None,
                                    AccessControlType.Allow));

                        rKey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey("irc", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree, rs);
                        rKey.SetValue("", "URL:IRC Protocol");
                        rKey.SetValue("URL Protocol", "");

                        rKey = rKey.CreateSubKey(@"Shell");
                        rKey.SetValue("", "", Microsoft.Win32.RegistryValueKind.String);
                        
                        rKey = rKey.CreateSubKey(@"open");                        
                        rKey.SetValue("", "", Microsoft.Win32.RegistryValueKind.String);
                        
                        rKey = rKey.CreateSubKey(@"command");
                        
                        rKey.SetValue("", "\"" + Application.ExecutablePath + "\" %1", Microsoft.Win32.RegistryValueKind.String);

                        rKey.Close();
                    }
                    catch (System.UnauthorizedAccessException)
                    {
                        //MessageBox.Show("Unauthorized Error");
                    }
                    catch (System.Exception)
                    {
                        //MessageBox.Show("Exception Error");
                    }
                }
                else
                {
                    rKey.Close();
                }

            }
            */
        }

        private void InitializePlugin(object pluginObject)
        {
            try
            {
                IPluginIceChat plugin = (IPluginIceChat)pluginObject;
                plugin.Initialize();
                plugin.MainProgramLoaded();

                if (plugin.Enabled == true)
                {
                    Panel[] bottomPanels = plugin.AddMainPanel();
                    if (bottomPanels != null && bottomPanels.Length > 0)
                    {
                        foreach (Panel p in bottomPanels)
                        {
                            if (p.Dock == DockStyle.Top)
                            {
                                p.Tag = "plugin";
                                //this.Controls.Add(p);
                                this.Invoke((MethodInvoker)delegate()
                                {
                                    this.Controls.Add(p);
                                });
                            }
                            else if (p.Dock == DockStyle.Bottom)
                            {
                                p.Tag = "plugin";
                                this.Invoke((MethodInvoker)delegate()
                                {
                                    this.Controls.Add(p);
                                    this.splitterBottom.Visible = true;
                                });
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + ":" + e.StackTrace);
            }
        }


        private void FirstRunSaveOptions(IceChatOptions options, IceChatFontSetting fonts)
        {
            this.iceChatOptions = options;
            this.iceChatFonts = fonts;
            
            SaveFonts();
            SaveOptions();           

        }

        private void closeWindow_Click(object sender, EventArgs e)
        {
            //close the current window
            mainChannelBar.CloseCurrentTab();
        }

        private void UpdateIcon(string iconName, string tag)
        {
            if (!this.IsHandleCreated && !this.IsDisposed) return;
            
            this.Invoke((MethodInvoker)delegate()
            {
                this.Icon = System.Drawing.Icon.FromHandle(StaticMethods.LoadResourceImage(iconName).GetHicon());
                this.Tag = tag;
            });
        }

        private void UpdateTrayIcon(string iconName, string tag)
        {
            if (!this.IsHandleCreated && !this.IsDisposed) return;
            
            this.Invoke((MethodInvoker)delegate()
            {
                this.notifyIcon.Icon = System.Drawing.Icon.FromHandle(StaticMethods.LoadResourceImage(iconName).GetHicon());
                this.notifyIcon.Tag = tag;
            });
        }

        private void FlashTaskBar()
        {
            if (StaticMethods.IsRunningOnMono())
            {
                //cant run flashwindowex
                this.flashTaskBarIconTimer.Enabled = true;
                this.flashTaskBarIconTimer.Start();
            }
            else
            {
                //need to invoke
                if (!this.IsHandleCreated && !this.IsDisposed) return;
                
                this.Invoke((MethodInvoker)delegate()
                {

                    FLASHWINFO fw = new FLASHWINFO();

                    fw.cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO)));
                    fw.hwnd = this.Handle;
                    fw.dwFlags = 3;
                    fw.dwTimeout = 0;
                    fw.uCount = (uint)iceChatOptions.FlashTaskBarNumber;

                    FlashWindowEx(ref fw);
                });
            }
        }

        private void flashTaskBarIconTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                if (this.Tag.Equals("on"))
                {
                    UpdateIcon("new-tray-icon.ico", "off");
                }
                else
                {
                    UpdateIcon("tray-icon-flash.ico", "on");
                }

                flashTaskBarCount++;

                if (flashTaskBarCount == iceChatOptions.FlashTaskBarNumber)
                {
                    this.flashTaskBarIconTimer.Stop();
                    UpdateIcon("new-tray-icon.ico", "off");
                    flashTaskBarCount = 0;
                }

            }
            else
            {
                this.flashTaskBarIconTimer.Stop();
                UpdateIcon("new-tray-icon.ico", "off");
            }
        }

        private void flashTrayIconTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.notifyIcon.Visible == true)
            {
                if (this.notifyIcon.Tag.Equals("on"))
                {
                    UpdateTrayIcon("PonyChat.ico", "off");
                }
                else
                {
                    UpdateTrayIcon("PonyChat.ico", "on");
                }
                
                flashTrayCount++;

                if (flashTrayCount == 10)
                {
                    this.flashTrayIconTimer.Stop();
                    UpdateTrayIcon("PonyChat.ico", "off");
                    flashTrayCount = 0;
                }
            }
            else
            {
                this.flashTrayIconTimer.Stop();
                UpdateTrayIcon("PonyChat.ico", "off");
            }
        }

        private void FormMain_Deactivate(object sender, EventArgs e)
        {
            IsForeGround = false;
        }

        private void FormMain_Activated(object sender, EventArgs e)
        {
            IsForeGround = true;
            FocusInputBox();
        }

        private void FormMainResize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                if (iceChatOptions.MinimizeToTray)
                {
                    this.notifyIcon.Visible = true;
                    this.Hide();
                }
            }
        }
        
        /// <summary>
        /// Save Default Server Settings
        /// </summary>
        private void OnDefaultServerSettings()
        {
            SaveOptions();
        }

        #region Load Language File

        private LanguageItem LoadLanguageItem(string languageFileName)
        {
            if (File.Exists(languageFileName))
            {
                LanguageItem languageItem = null;
                XmlSerializer deserializer = new XmlSerializer(typeof(LanguageItem));
                TextReader textReader = new StreamReader(languageFileName);
                try
                {
                    languageItem = (LanguageItem)deserializer.Deserialize(textReader);
                    languageItem.LanguageFile = languageFileName;
               }
                catch
                {
                    languageItem = null;
                }
                finally
                {
                    textReader.Close();
                    textReader.Dispose();
                }
                return languageItem;
            }
            else
            {
                return null;
            }
        }

        private void LoadLanguage()
        {
            if (File.Exists(currentLanguageFile.LanguageFile))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(IceChatLanguage));
                TextReader textReader = new StreamReader(currentLanguageFile.LanguageFile);
                iceChatLanguage = (IceChatLanguage)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
            }
            else
            {
                iceChatLanguage = new IceChatLanguage();
                //write the language file
                XmlSerializer serializer = new XmlSerializer(typeof(IceChatLanguage));
                TextWriter textWriter = new StreamWriter(currentFolder + Path.DirectorySeparatorChar + "Languages" + Path.DirectorySeparatorChar + "English.xml");
                serializer.Serialize(textWriter,iceChatLanguage);
                textWriter.Close();
                textWriter.Dispose();
            }
        }

        private void ApplyLanguage()
        {
            mainToolStripMenuItem.Text = iceChatLanguage.mainToolStripMenuItem;
            minimizeToTrayToolStripMenuItem.Text = iceChatLanguage.minimizeToTrayToolStripMenuItem;
            debugWindowToolStripMenuItem.Text = iceChatLanguage.debugWindowToolStripMenuItem;
            exitToolStripMenuItem.Text = iceChatLanguage.exitToolStripMenuItem;
            optionsToolStripMenuItem.Text = iceChatLanguage.optionsToolStripMenuItem;
            iceChatSettingsToolStripMenuItem.Text = iceChatLanguage.iceChatSettingsToolStripMenuItem;
            iceChatColorsToolStripMenuItem.Text = iceChatLanguage.iceChatColorsToolStripMenuItem;
            iceChatEditorToolStripMenuItem.Text = iceChatLanguage.iceChatEditorToolStripMenuItem;
            pluginsToolStripMenuItem.Text = iceChatLanguage.pluginsToolStripMenuItem;
            viewToolStripMenuItem.Text = iceChatLanguage.viewToolStripMenuItem;
            serverListToolStripMenuItem.Text = iceChatLanguage.serverListToolStripMenuItem;
            nickListToolStripMenuItem.Text = iceChatLanguage.nickListToolStripMenuItem;
            statusBarToolStripMenuItem.Text = iceChatLanguage.statusBarToolStripMenuItem;
            helpToolStripMenuItem.Text = iceChatLanguage.helpToolStripMenuItem;
            aboutToolStripMenuItem.Text = iceChatLanguage.aboutToolStripMenuItem;
            toolStripStatus.Text = iceChatLanguage.toolStripStatus;
            
            channelListTab.Text = iceChatLanguage.tabPageFaveChannels;
            nickListTab.Text = iceChatLanguage.tabPageNicks;
            serverListTab.Text = iceChatLanguage.serverTreeHeader;

            channelList.ApplyLanguage();
            nickList.ApplyLanguage();
            serverTree.ApplyLanguage();
            inputPanel.ApplyLanguage();

            mainChannelBar.Invalidate();
        }

        #endregion

        private void FormMainClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                //check if there are any connections open
                if (iceChatOptions.AskQuit)
                {
                    foreach (IRCConnection c in serverTree.ServerConnections.Values)
                    {
                        if (c.IsConnected)
                        {
                            DialogResult dr = MessageBox.Show("You are currently connected! are you sure you want to close the Ponychat Client?", "Close PonyChat", MessageBoxButtons.OKCancel);
                            if (e.CloseReason == CloseReason.UserClosing && dr == DialogResult.Cancel)
                            {
                                e.Cancel = true;
                                return;
                            }
                            else
                                break;
                        }
                    }
                }

                if (identServer != null)
                {
                    identServer.Stop();
                    identServer = null;
                }

                //disconnect all the servers
                foreach (IRCConnection c in serverTree.ServerConnections.Values)
                {
                    if (c.IsConnected)
                    {
                        c.AttemptReconnect = false;
                        ParseOutGoingCommand(c, "//quit " + c.ServerSetting.QuitMessage);
                    }
                }

                if (iceChatOptions.SaveWindowPosition)
                {
                    //save the window position , as long as its not minimized
                    if (this.WindowState != FormWindowState.Minimized)
                    {

                        if (Screen.AllScreens.Length > 1)
                        {
                            foreach (Screen screen in Screen.AllScreens)
                                if (screen.Bounds.Contains(this.Location))
                                    iceChatOptions.WindowLocation = new Point(this.Location.X, this.Location.Y);
                        }
                        else
                            iceChatOptions.WindowLocation = this.Location;
                                                
                        iceChatOptions.WindowSize = this.Size;
                        
                        iceChatOptions.WindowState = this.WindowState;

                        if (!panelDockRight.IsDocked)
                            iceChatOptions.RightPanelWidth = panelDockRight.Width;
                        if (!panelDockLeft.IsDocked)
                            iceChatOptions.LeftPanelWidth = panelDockLeft.Width;

                        iceChatOptions.DockLeftPanel = panelDockLeft.IsDocked;
                        iceChatOptions.DockRightPanel = panelDockRight.IsDocked;

                        //Save the side panels
                        string[] leftPanels = new string[panelDockLeft.TabControl.TabPages.Count];
                        for (int i = 0; i < panelDockLeft.TabControl.TabPages.Count; i++)
                        {
                            leftPanels[i] = panelDockLeft.TabControl.TabPages[i].Text;
                        }
                        iceChatOptions.LeftPanels = leftPanels;

                        string[] rightPanels = new string[panelDockRight.TabControl.TabPages.Count];
                        for (int i = 0; i < panelDockRight.TabControl.TabPages.Count; i++)
                        {
                            rightPanels[i] = panelDockRight.TabControl.TabPages[i].Text;
                        }
                        iceChatOptions.RightPanels = rightPanels;

                    }

                    SaveOptions();
                }

                //mutex.ReleaseMutex();

                //unload and dispose of all the plugins
                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    try
                    {
                        ipc.Dispose();
                    }
                    catch (Exception eee)
                    {
                        MessageBox.Show(eee.Message + ":" + eee.StackTrace);
                        continue;
                    }
                }

                for (int i = 0; i < loadedPlugins.Count; i++)
                {
                    loadedPlugins.RemoveAt(i);
                }


                if (errorFile != null)
                {
                    errorFile.Flush();
                    errorFile.Close();
                    errorFile.Dispose();
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ":" + ee.StackTrace);                
            }
        }
        
        /// <summary>
        /// Play the specified sound file (currently only supports WAV files)
        /// </summary>
        /// <param name="sound"></param>
        internal void PlaySoundFile(string key)
        {
            IceChatSounds.SoundEntry sound = IceChatSounds.getSound(key);
            if (sound != null && !muteAllSounds)
            {
                string file = sound.getSoundFile();
                if (file != null && file.Length > 0)
                {
                    try
                    {
                        if (iceChatOptions.SoundUseExternalCommand && iceChatOptions.SoundExternalCommand.Length > 0)
                            ParseOutGoingCommand(inputPanel.CurrentConnection, iceChatOptions.SoundExternalCommand + " " + file);                        
                        else    
                            ParseOutGoingCommand(inputPanel.CurrentConnection, "/play " + file);
                        
                        //player.SoundLocation = @file;
                        //player.Play();
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Create a Default Tab for showing Welcome Information
        /// </summary>
        private void CreateDefaultConsoleWindow()
        {
            IceTabPage p = new IceTabPage(IceTabPage.WindowType.Console, "Console");
            p.AddConsoleTab(iceChatLanguage.consoleTabWelcome);
            
            mainTabControl.AddTabPage(p);
            mainTabControl.BringFront(p);

            mainChannelBar.AddTabPage(ref p);

            WindowMessage(null, "Console", "\x00034Welcome to " + ProgramID + " " + VersionID, 1, false);
            WindowMessage(null, "Console", "\x00034** This is a Release Candidate version, fully functional, not all the options are added **", 1, false);
            WindowMessage(null, "Console", "\x00034Please visit \x00030,4#RainbowFactory\x0003 on \x00030,2irc://irc.ponychat.net/rainbowfactory\x0003 if you wish to help with this project", 1, true);
            
            StatusText("Welcome to " + ProgramID + " " + VersionID);
        }

        #region Internal Properties

        /// <summary>
        /// Gets the instance of the Nick List
        /// </summary>
        internal NickList NickList
        {
            get { return nickList; } 
        }
        /// <summary>
        /// Gets the instance of the Server Tree
        /// </summary>
        internal ServerTree ServerTree
        {
            get { return serverTree; }
        }
        /// <summary>
        /// Gets the instance of the Main Tab Control
        /// </summary>
        internal IceTabControl TabMain
        {
            get { return mainTabControl; }
        }
        internal ChannelBar ChannelBar
        {
            get { return mainChannelBar; }
        }

        /// <summary>
        /// Gets the instance of the InputPanel
        /// </summary>
        internal InputPanel InputPanel
        {
            get
            {
                return this.inputPanel;
            }
        }

        internal IceChatOptions IceChatOptions
        {
            get
            {
                return this.iceChatOptions;
            }
        }

        internal IceChatMessageFormat MessageFormats
        {
            get
            {
                return this.iceChatMessages;
            }
        }

        internal IceChatFontSetting IceChatFonts
        {
            get
            {
                return this.iceChatFonts;
            }
        }
        
        internal IceChatColors IceChatColors
        {
            get
            {
                return this.iceChatColors;
            }
        }

        internal ChannelSettings ChannelSettings
        {
            get
            {
                return this.channelSettings;
            }
        }

        internal IceChatSounds IceChatSounds
        {
            get
            {
                return this.iceChatSounds;
            }
        }

        internal IceChatAliases IceChatAliases
        {
            get
            {
                return iceChatAliases;
            }
            set
            {
                iceChatAliases = value;
                //save the aliases
                SaveAliases();
            }
        }

        internal IceChatPopupMenus IceChatPopupMenus
        {
            get
            {
                return iceChatPopups;
            }
            set
            {
                iceChatPopups = value;
                //save the popups
                SavePopups();
            }

        }

        internal List<IPluginIceChat> LoadedPlugins
        {
            get { return loadedPlugins; }
        }

        internal IceChatEmoticon IceChatEmoticons
        {
            get
            {
                return iceChatEmoticons;
            }
            set
            {
                iceChatEmoticons = value;
                //save the Emoticons
                SaveEmoticons();
            }
        }

        internal IceChatLanguage IceChatLanguage
        {
            get
            {
                return iceChatLanguage;
            }
        }

        internal List<LanguageItem> IceChatLanguageFiles
        {
            get
            {
                return languageFiles;
            }
        }

        internal LanguageItem IceChatCurrentLanguageFile
        {
            get
            {
                return currentLanguageFile;
            }
            set
            {
                if (currentLanguageFile != value)
                {
                    currentLanguageFile = value;
                    LoadLanguage();
                    ApplyLanguage();
                }
           }
        }

        internal string FavoriteChannelsFile
        {
            get
            {
                return favoriteChannelsFile;
            }
        }

        internal BuddyList BuddyList
        {
            get
            {
                return this.buddyList;
            }
        }

        internal string MessagesFile
        {
            get
            {
                return messagesFile;
            }
            set
            {
                messagesFile = value;
            }
        }

        internal string ColorsFile
        {
            get
            {
                return colorsFile;
            }
            set
            {
                colorsFile = value;
            }
        }

        internal string ServersFile
        {
            get
            {
                return serversFile;
            }
        }

        internal string AliasesFile
        {
            get
            {
                return aliasesFile;
            }
        }

        internal string PopupsFile
        {
            get
            {
                return popupsFile;
            }
        }

        internal List<IPluginIceChat> IceChatPlugins
        {
            get
            {
                return loadedPlugins;
            }
        }

        internal List<IThemeIceChat> IceChatPluginThemes
        {
            get
            {
                return loadedPluginThemes;
            }
        }

        public string LogsFolder
        {
            get
            {
                return logsFolder;
            }
        }

        public string CurrentFolder
        {
            get
            {
                return currentFolder;
            }
        }

        internal string EmoticonsFolder
        {
            get
            {
                return System.IO.Path.GetDirectoryName(emoticonsFile);
            }
        }

        internal string EmoticonsFile
        {
            get
            {
                return emoticonsFile;
            }
        }

        internal void StatusText(string data)
        {
            try
            {
                if (!this.IsHandleCreated && !this.IsDisposed) return;
                
                this.Invoke((MethodInvoker)delegate()
                {
                    toolStripStatus.Text = "Status: " + data;
                });
            }
            catch (Exception e) 
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        #endregion

        #region Private Properties
        /// <summary>
        /// Set focus to the Input Panel
        /// </summary>
        internal void FocusInputBox()
        {
            inputPanel.FocusTextBox();
        }

        /// <summary>
        /// Sends a Message to a Named Window
        /// </summary>
        /// <param name="connection">Which Connection to use</param>
        /// <param name="name">Name of the Window</param>
        /// <param name="data">Message to send</param>
        /// <param name="color">Color number of the message</param>
        internal void WindowMessage(IRCConnection connection, string name, string data, int color, bool scrollToBottom)
        {
            if (this.InvokeRequired)
            {
                WindowMessageDelegate w = new WindowMessageDelegate(WindowMessage);
                this.Invoke(w, new object[] { connection, name, data, color, scrollToBottom} );
            }
            else
            {
                if (name == "Console")
                {
                    mainChannelBar.GetTabPage("Console").AddText(connection, data, color, scrollToBottom);
                    if (connection != null)
                        if (connection.IsFullyConnected)
                            if (!connection.ServerSetting.DisableSounds)
                                PlaySoundFile("conmsg");
                }
                else
                {
                    foreach (IceTabPage t in mainChannelBar.TabPages)
                    {
                        if (t.TabCaption == name)
                        {
                            if (t.Connection == connection)
                            {
                                t.TextWindow.AppendText(data, color);
                                if (scrollToBottom)
                                    t.TextWindow.ScrollToBottom();
                                return;
                            }
                        }
                    }
                    
                    WindowMessage(connection, "Console", data, color, scrollToBottom);
                }
            }
        }
        /// <summary>
        /// Send a Message to the Current Window
        /// </summary>
        /// <param name="connection">Which Connection to use</param>
        /// <param name="data">Message to send</param>
        /// <param name="color">Color number of the message</param>
        internal void CurrentWindowMessage(IRCConnection connection, string data, int color, bool scrollToBottom)
        {
            if (this.InvokeRequired)
            {
                CurrentWindowMessageDelegate w = new CurrentWindowMessageDelegate(CurrentWindowMessage);
                this.Invoke(w, new object[] { connection, data, color, scrollToBottom });
            }
            else
            {
                //check what type the current window is
                if (CurrentWindowType == IceTabPage.WindowType.ChannelList)
                {
                    //do nothing, send it to the console
                    mainChannelBar.GetTabPage("Console").AddText(connection, data, color, false);
                }
                else if (CurrentWindowType != IceTabPage.WindowType.Console)
                {
                    IceTabPage t = mainChannelBar.CurrentTab;
                    if (t != null)
                    {
                        if (t.Connection == connection)
                        {
                            t.TextWindow.AppendText(data, color);
                        }
                        else
                        {
                            WindowMessage(connection, "Console", data, color, scrollToBottom);
                        }
                    }
                }
                else
                {
                    //console window is current window
                    mainChannelBar.GetTabPage("Console").AddText(connection, data, color, false);

                }
            }
        }

        /// <summary>
        /// Gets a Tab Window
        /// </summary>
        /// <param name="connection">Which Connection to use</param>
        /// <param name="name">Name of the Window</param>
        /// <param name="windowType">The Window Type</param>
        /// <returns></returns>
        internal IceTabPage GetWindow(IRCConnection connection, string sCaption, IceTabPage.WindowType windowType)
        {
            foreach (IceTabPage t in mainChannelBar.TabPages)
            {
                if (t.TabCaption.ToLower() == sCaption.ToLower() && t.WindowStyle == windowType)
                {
                    if (t.Connection == null && windowType == IceTabPage.WindowType.DCCFile)
                        return t;
                    else if (t.Connection == null && windowType == IceTabPage.WindowType.Debug)
                        return t;
                    else if (t.Connection == connection)
                        return t;
                }
                else if (t.WindowStyle == windowType && windowType == IceTabPage.WindowType.ChannelList)
                {
                    return t;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Get the Current Tab Window
        /// </summary>
        internal IceTabPage CurrentWindow
        {
            get
            {
                return mainChannelBar.CurrentTab;
            }
        }
        
        /// <summary>
        /// Get the Current Window Type
        /// </summary>
        internal IceTabPage.WindowType CurrentWindowType
        {
            get
            {
                if (mainChannelBar.CurrentTab != null)
                    return mainChannelBar.CurrentTab.WindowStyle;
                else
                {
                    return IceTabPage.WindowType.Console;
                }
            }
        }

        #endregion

        #region Private Methods
        
        /// <summary>
        /// Send a Message through the IRC Connection to the Server
        /// </summary>
        /// <param name="connection">Which Connection to use</param>
        /// <param name="data">RAW IRC Message to send</param>
        private void SendData(IRCConnection connection, string data)
        {
            if (connection != null)
            {
                if (connection.IsConnected)
                {
                    if (connection.IsFullyConnected)
                        connection.SendData(data);
                    else
                        //add to a command queue, which gets run once fully connected, after autoperform/autojoin
                        connection.AddToCommandQueue(data);
                }
                else
                {
                    if (CurrentWindowType == IceTabPage.WindowType.Console)
                        WindowMessage(connection, "Console", "Error: Not Connected to Server (" + data + ")", 4, true);
                    else if (CurrentWindow.WindowStyle != IceTabPage.WindowType.ChannelList && CurrentWindow.WindowStyle != IceTabPage.WindowType.DCCFile)
                    {
                        CurrentWindow.TextWindow.AppendText("Error: Not Connected to Server (" + data + ")", 4);
                        CurrentWindow.TextWindow.ScrollToBottom();
                    }
                    else
                    {
                        WindowMessage(connection, "Console", "Error: Not Connected to Server (" + data + ")", 4, true);
                    }
                }
            }
        }

        #endregion
        /*
        private void NewConnection2(Object setting)
        {
            ServerSetting serverSetting = (ServerSetting)setting;

            IRCConnection c = new IRCConnection(serverSetting);

            c.ChannelMessage += new ChannelMessageDelegate(OnChannelMessage);
            c.ChannelAction += new ChannelActionDelegate(OnChannelAction);
            c.QueryMessage += new QueryMessageDelegate(OnQueryMessage);
            c.QueryAction += new QueryActionDelegate(OnQueryAction);
            c.ChannelNotice += new ChannelNoticeDelegate(OnChannelNotice);

            c.ChangeNick += new ChangeNickDelegate(OnChangeNick);
            c.ChannelKick += new ChannelKickDelegate(OnChannelKick);

            c.OutGoingCommand += new OutGoingCommandDelegate(OutGoingCommand);
            c.JoinChannel += new JoinChannelDelegate(OnChannelJoin);
            c.PartChannel += new PartChannelDelegate(OnChannelPart);
            c.QuitServer += new QuitServerDelegate(OnServerQuit);

            c.JoinChannelMyself += new JoinChannelMyselfDelegate(OnChannelJoinSelf);
            c.PartChannelMyself += new PartChannelMyselfDelegate(OnChannelPartSelf);
            c.ChannelKickSelf += new ChannelKickSelfDelegate(OnChannelKickSelf);

            c.ChannelTopic += new ChannelTopicDelegate(OnChannelTopic);
            c.ChannelMode += new ChannelModeChangeDelegate(OnChannelMode);
            c.UserMode += new UserModeChangeDelegate(OnUserMode);
            c.ChannelInvite += new ChannelInviteDelegate(OnChannelInvite);

            c.ServerMessage += new ServerMessageDelegate(OnServerMessage);
            c.ServerError += new ServerErrorDelegate(OnServerError);
            c.ServerMOTD += new ServerMOTDDelegate(OnServerMOTD);
            c.WhoisData += new WhoisDataDelegate(OnWhoisData);
            c.UserNotice += new UserNoticeDelegate(OnUserNotice);
            c.CtcpMessage += new CtcpMessageDelegate(OnCtcpMessage);
            c.CtcpReply += new CtcpReplyDelegate(OnCtcpReply);
            c.GenericChannelMessage += new GenericChannelMessageDelegate(OnGenericChannelMessage);
            c.ServerNotice += new ServerNoticeDelegate(OnServerNotice);
            c.ChannelListStart += new ChannelListStartDelegate(OnChannelListStart);
            c.ChannelList += new ChannelListDelegate(OnChannelList);
            c.DCCChat += new DCCChatDelegate(OnDCCChat);
            c.DCCFile += new DCCFileDelegate(OnDCCFile);
            c.DCCPassive += new DCCPassiveDelegate(OnDCCPassive);
            c.UserHostReply += new UserHostReplyDelegate(OnUserHostReply);
            c.IALUserData += new IALUserDataDelegate(OnIALUserData);
            c.IALUserChange += new IALUserChangeDelegate(OnIALUserChange);
            c.IALUserPart += new IALUserPartDelegate(OnIALUserPart);
            c.IALUserQuit += new IALUserQuitDelegate(OnIALUserQuit);

            c.BuddyListData += new BuddyListDelegate(OnBuddyList);
            c.BuddyListClear += new BuddyListClearDelegate(OnBuddyListClear);
            c.RawServerIncomingData += new RawServerIncomingDataDelegate(OnRawServerData);
            c.RawServerOutgoingData += new RawServerOutgoingDataDelegate(OnRawServerOutgoingData);

            c.AutoJoin += new AutoJoinDelegate(OnAutoJoin);
            c.AutoRejoin += new AutoRejoinDelegate(OnAutoRejoin);
            c.AutoPerform += new AutoPerformDelegate(OnAutoPerform);

            c.EndofNames += new EndofNamesDelegate(OnEndofNames);
            c.EndofWhoReply += new EndofWhoReplyDelegate(OnEndofWhoReply);
            c.WhoReply += new WhoReplyDelegate(OnWhoReply);
            c.ChannelUserList += new ChannelUserListDelegate(OnChannelUserList);

            c.StatusText += new IceChat.StatusTextDelegate(OnStatusText);
            c.RefreshServerTree += new RefreshServerTreeDelegate(OnRefreshServerTree);
            c.ServerReconnect += new ServerReconnectDelegate(OnServerReconnect);
            c.ServerDisconnect += new ServerReconnectDelegate(OnServerDisconnect);
            c.ServerConnect += new ServerConnectDelegate(OnServerConnect);
            c.ServerForceDisconnect += new ServerForceDisconnectDelegate(OnServerForceDisconnect);
            c.ServerPreConnect += new ServerPreConnectDelegate(OnServerPreConnect);
            c.UserInfoWindowExists += new UserInfoWindowExistsDelegate(OnUserInfoWindowExists);
            c.UserInfoHostFullName += new UserInfoHostFullnameDelegate(OnUserInfoHostFullName);
            c.UserInfoIdleLogon += new UserInfoIdleLogonDelegate(OnUserInfoIdleLogon);
            c.UserInfoAddChannels += new UserInfoAddChannelsDelegate(OnUserInfoAddChannels);

            c.ChannelInfoWindowExists += new ChannelInfoWindowExistsDelegate(OnChannelInfoWindowExists);
            c.ChannelInfoAddBan += new ChannelInfoAddBanDelegate(OnChannelInfoAddBan);
            c.ChannelInfoAddException += new ChannelInfoAddExceptionDelegate(OnChannelInfoAddException);
            c.ChannelInfoTopicSet += new ChannelInfoTopicSetDelegate(OnChannelInfoTopicSet);

            c.WriteErrorFile += new WriteErrorFileDelegate(OnWriteErrorFile);

            OnAddConsoleTab(c);

            mainTabControl.SelectTab(mainChannelBar.GetTabPage("Console"));

            inputPanel.CurrentConnection = c;
            serverTree.AddConnection(c);

            c.ConnectSocket();

        }
        */
        /// <summary>
        /// Create a new Server Connection
        /// </summary>
        /// <param name="serverSetting">Which ServerSetting to use</param>
        private void NewServerConnection(ServerSetting serverSetting)
        {            
            IRCConnection c = new IRCConnection(serverSetting);

            c.ChannelMessage += new ChannelMessageDelegate(OnChannelMessage);
            c.ChannelAction += new ChannelActionDelegate(OnChannelAction);
            c.QueryMessage += new QueryMessageDelegate(OnQueryMessage);
            c.QueryAction += new QueryActionDelegate(OnQueryAction);
            c.ChannelNotice += new ChannelNoticeDelegate(OnChannelNotice);

            c.ChangeNick += new ChangeNickDelegate(OnChangeNick);
            c.ChannelKick += new ChannelKickDelegate(OnChannelKick);

            c.OutGoingCommand += new OutGoingCommandDelegate(OutGoingCommand);
            c.JoinChannel += new JoinChannelDelegate(OnChannelJoin);
            c.PartChannel += new PartChannelDelegate(OnChannelPart);
            c.QuitServer += new QuitServerDelegate(OnServerQuit);

            c.JoinChannelMyself += new JoinChannelMyselfDelegate(OnChannelJoinSelf);
            c.PartChannelMyself += new PartChannelMyselfDelegate(OnChannelPartSelf);
            c.ChannelKickSelf += new ChannelKickSelfDelegate(OnChannelKickSelf);

            c.ChannelTopic += new ChannelTopicDelegate(OnChannelTopic);
            c.ChannelMode += new ChannelModeChangeDelegate(OnChannelMode);
            c.UserMode += new UserModeChangeDelegate(OnUserMode);
            c.ChannelInvite += new ChannelInviteDelegate(OnChannelInvite);

            c.ServerMessage += new ServerMessageDelegate(OnServerMessage);
            c.ServerError += new ServerErrorDelegate(OnServerError);
            c.ServerMOTD += new ServerMOTDDelegate(OnServerMOTD);
            c.WhoisData += new WhoisDataDelegate(OnWhoisData);
            c.UserNotice += new UserNoticeDelegate(OnUserNotice);
            c.CtcpMessage += new CtcpMessageDelegate(OnCtcpMessage);
            c.CtcpReply += new CtcpReplyDelegate(OnCtcpReply);
            c.GenericChannelMessage += new GenericChannelMessageDelegate(OnGenericChannelMessage);
            c.ServerNotice += new ServerNoticeDelegate(OnServerNotice);
            c.ChannelListStart += new ChannelListStartDelegate(OnChannelListStart);
            c.ChannelList += new ChannelListDelegate(OnChannelList);
            c.ChannelListEnd += new ChannelListEndDelegate(OnChannelListEnd);
            c.DCCChat += new DCCChatDelegate(OnDCCChat);
            c.DCCFile += new DCCFileDelegate(OnDCCFile);
            c.DCCPassive += new DCCPassiveDelegate(OnDCCPassive);
            c.UserHostReply += new UserHostReplyDelegate(OnUserHostReply);
            c.IALUserData += new IALUserDataDelegate(OnIALUserData);
            c.IALUserDataAwayOnly += new IALUserDataAwayOnlyDelegate(OnIALUserDataAwayOnly);
            c.IALUserChange += new IALUserChangeDelegate(OnIALUserChange);
            c.IALUserPart += new IALUserPartDelegate(OnIALUserPart);
            c.IALUserQuit += new IALUserQuitDelegate(OnIALUserQuit);

            c.BuddyListData += new BuddyListDelegate(OnBuddyList);
            c.BuddyListClear += new BuddyListClearDelegate(OnBuddyListClear);
            c.RawServerIncomingData += new RawServerIncomingDataDelegate(OnRawServerData);
            c.RawServerOutgoingData += new RawServerOutgoingDataDelegate(OnRawServerOutgoingData);
            c.RawServerIncomingDataOverRide += new RawServerIncomingDataOverRideDelegate(OnRawServerIncomingDataOverRide);

            c.AutoJoin += new AutoJoinDelegate(OnAutoJoin);
            c.AutoRejoin += new AutoRejoinDelegate(OnAutoRejoin);
            c.AutoPerform += new AutoPerformDelegate(OnAutoPerform);

            c.EndofNames += new EndofNamesDelegate(OnEndofNames);
            c.EndofWhoReply += new EndofWhoReplyDelegate(OnEndofWhoReply);
            c.WhoReply += new WhoReplyDelegate(OnWhoReply);
            c.ChannelUserList += new ChannelUserListDelegate(OnChannelUserList);

            c.StatusText += new IceChat.StatusTextDelegate(OnStatusText);
            c.RefreshServerTree += new RefreshServerTreeDelegate(OnRefreshServerTree);
            c.ServerReconnect += new ServerReconnectDelegate(OnServerReconnect);
            c.ServerDisconnect += new ServerReconnectDelegate(OnServerDisconnect);
            c.ServerConnect += new ServerConnectDelegate(OnServerConnect);
            c.ServerForceDisconnect += new ServerForceDisconnectDelegate(OnServerForceDisconnect);
            c.ServerPreConnect += new ServerPreConnectDelegate(OnServerPreConnect);
            c.UserInfoWindowExists += new UserInfoWindowExistsDelegate(OnUserInfoWindowExists);
            c.UserInfoHostFullName += new UserInfoHostFullnameDelegate(OnUserInfoHostFullName);
            c.UserInfoIdleLogon += new UserInfoIdleLogonDelegate(OnUserInfoIdleLogon);
            c.UserInfoAddChannels += new UserInfoAddChannelsDelegate(OnUserInfoAddChannels);
            c.UserInfoAwayStatus += new UserInfoAwayStatusDelegate(OnUserInfoAwayStatus);
            c.UserInfoServer += new UserInfoServerDelegate(OnUserInfoServer);

            c.ChannelInfoWindowExists += new ChannelInfoWindowExistsDelegate(OnChannelInfoWindowExists);
            c.ChannelInfoAddBan += new ChannelInfoAddBanDelegate(OnChannelInfoAddBan);
            c.ChannelInfoAddException += new ChannelInfoAddExceptionDelegate(OnChannelInfoAddException);
            c.ChannelInfoTopicSet += new ChannelInfoTopicSetDelegate(OnChannelInfoTopicSet);

            c.AutoAwayTrigger += new AutoAwayDelegate(OnAutoAwayTrigger);
            c.ServerFullyConnected += new ServerForceDisconnectDelegate(OnServerFullyConnected);

            c.WriteErrorFile += new WriteErrorFileDelegate(OnWriteErrorFile);
            c.ParseIdentifier += new IRCConnection.ParseIdentifierDelegate(OnParseIdentifier); 

            OnAddConsoleTab(c);
            
            serverSetting.CurrentNickName = serverSetting.NickName;

            mainChannelBar.SelectTab(mainChannelBar.GetTabPage("Console"));

            inputPanel.CurrentConnection = c;
            serverTree.AddConnection(c);

            c.ConnectSocket();

        }

        #region Tab Events and Methods

        /// <summary>
        /// Add a new Connection Tab to the Console
        /// </summary>
        /// <param name="connection">Which Connection to add</param>
        private void OnAddConsoleTab(IRCConnection connection)
        {            
            mainChannelBar.GetTabPage("Console").AddConsoleTab(connection);
        }

        /// <summary>
        /// Add a new Tab Window to the Main Tab Control
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="windowName">Window Name of the New Tab</param>
        /// <param name="windowType">Window Type of the New Tab</param>
        internal IceTabPage AddWindow(IRCConnection connection, string windowName, IceTabPage.WindowType windowType)
        {
            if (this.InvokeRequired)
            {
                AddWindowDelegate a = new AddWindowDelegate(AddWindow);
                return (IceTabPage)this.Invoke(a, new object[] { connection, windowName, windowType });
            }
            else
            {
                try
                {
                    IceTabPage page;
                    if (windowType == IceTabPage.WindowType.DCCFile)
                        page = new IceTabPageDCCFile(IceTabPage.WindowType.DCCFile, windowName);
                    else
                    {
                        page = new IceTabPage(windowType, windowName);
                        page.Connection = connection;
                    }

                    if (page.WindowStyle == IceTabPage.WindowType.Channel)
                    {
                        page.TextWindow.Font = new Font(iceChatFonts.FontSettings[1].FontName, iceChatFonts.FontSettings[1].FontSize);
                        page.ResizeTopicFont(iceChatFonts.FontSettings[1].FontName, iceChatFonts.FontSettings[1].FontSize);
                        //send the message
                        string msg = GetMessageFormat("Self Channel Join");
                        msg = msg.Replace("$nick", connection.ServerSetting.CurrentNickName).Replace("$channel", windowName);

                        if (FormMain.Instance.IceChatOptions.LogChannel)
                        {
                            page.TextWindow.SetLogFile();
                        }

                        page.TextWindow.AppendText(msg, 1);
                    }
                    else if (page.WindowStyle == IceTabPage.WindowType.Query)
                    {
                        page.TextWindow.Font = new Font(iceChatFonts.FontSettings[2].FontName, iceChatFonts.FontSettings[2].FontSize);
                        if (FormMain.Instance.IceChatOptions.LogQuery)
                            page.TextWindow.SetLogFile();
                    }
                    else if (page.WindowStyle == IceTabPage.WindowType.Debug)
                    {
                        page.TextWindow.NoColorMode = true;
                        page.TextWindow.Font = new Font("Verdana", 10);
                        page.TextWindow.SetLogFile();
                        page.TextWindow.SetDebugWindow();
                    }
                    else if (page.WindowStyle == IceTabPage.WindowType.Window)
                    {
                        page.TextWindow.Font = new Font("Verdana", 10);
                        if (FormMain.Instance.IceChatOptions.LogWindow)                        
                            page.TextWindow.SetLogFile();
                    }

                    //find the last window index for this connection
                    int index = 0;
                    if (page.WindowStyle == IceTabPage.WindowType.Channel || page.WindowStyle == IceTabPage.WindowType.Query || page.WindowStyle == IceTabPage.WindowType.DCCChat || page.WindowStyle == IceTabPage.WindowType.DCCFile)
                    {
                        for (int i = 1; i < mainChannelBar.TabPages.Count; i++)
                        {
                            if (mainChannelBar.TabPages[i].Connection == connection)
                                index = i + 1;
                        }
                    }

                    if (index == 0)
                    {
                        if (mainTabControl.Visible == false)
                        {
                            page.DockedForm = true;
                            
                            FormWindow fw = new FormWindow(page);
                            fw.Text = page.TabCaption;
                            fw.MdiParent = this;
                            fw.Show();
                        }
                        else
                        {
                            page.DockedForm = false;

                            mainTabControl.AddTabPage(page);
                        }
                        
                        mainChannelBar.AddTabPage(ref page);
                    }
                    else
                    {
                        if (mainTabControl.Visible == false)
                        {
                            FormWindow fw = new FormWindow(page);
                            fw.Text = page.TabCaption;
                            fw.MdiParent = this;
                            fw.Show();
                        }
                        else
                            mainTabControl.AddTabPage(page);

                        mainChannelBar.InsertTabPage(index, ref page);
                    }
                    
                    
                    /*                 
                    if (index == 0)
                    {
                        mainTabControl.AddTabPage(page);
                        mainChannelBar.AddTabPage(ref page);
                    }
                    else
                    {
                        mainTabControl.AddTabPage(page);
                        mainChannelBar.InsertTabPage(index, ref page);
                    }
                    */

                    if (page.WindowStyle == IceTabPage.WindowType.Query && !iceChatOptions.NewQueryForegound)
                    {
                        mainChannelBar.SelectTab(mainChannelBar.CurrentTab);
                        serverTree.SelectTab(mainChannelBar.CurrentTab, false);
                    }
                    else if (page.WindowStyle == IceTabPage.WindowType.Window)
                    {
                        //mainChannelBar.SelectTab(page);                        
                        mainChannelBar.Invalidate(); 
                        serverTree.Invalidate();
                    }
                    else
                    {
                        mainChannelBar.SelectTab(page);
                        nickList.CurrentWindow = page;
                        serverTree.SelectTab(page, false);
                    }

                    if (page.WindowStyle == IceTabPage.WindowType.Channel)
                    {
                        //get channel settings
                        page.ChannelSettings(page.Connection.ServerSetting.NetworkName);
                    }

                    if (page.WindowStyle == IceTabPage.WindowType.Query && iceChatOptions.WhoisNewQuery == true)
                        ParseOutGoingCommand(page.Connection, "/whois " + page.TabCaption + " " + page.TabCaption);

                    return page;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Source + ":" + e.StackTrace);
                }
                return null;
            }
        }
        /// <summary>
        /// Remove a Tab Window from the Main Tab Control
        /// </summary>
        /// <param name="connection">Which Connection it is for</param>
        /// <param name="channel">The Channel/Query Window Name</param>
        internal void RemoveWindow(IRCConnection connection, string windowCaption, IceTabPage.WindowType windowType)
        {
            if (!this.IsHandleCreated && !this.IsDisposed) return; 
            
            this.Invoke((MethodInvoker)delegate()
            {
                IceTabPage t = GetWindow(connection, windowCaption, IceTabPage.WindowType.Channel);
                if (t != null)
                {
                    ChannelBar.TabPages.Remove(t);
                    mainTabControl.Controls.Remove(t);
                    return;
                }

                IceTabPage c = GetWindow(connection, windowCaption, IceTabPage.WindowType.Query);
                if (c != null)
                {
                    ChannelBar.TabPages.Remove(c);
                    mainTabControl.Controls.Remove(c);
                    return;
                }

                IceTabPage dcc = GetWindow(connection, windowCaption, IceTabPage.WindowType.DCCChat);
                if (dcc != null)
                {
                    ChannelBar.TabPages.Remove(dcc);
                    mainTabControl.Controls.Remove(dcc);
                    return;
                }

                IceTabPage cl = GetWindow(connection, "", IceTabPage.WindowType.ChannelList);
                if (cl != null)
                {
                    ChannelBar.TabPages.Remove(cl);
                    mainTabControl.Controls.Remove(cl);
                    return;
                }

                IceTabPage de = GetWindow(null, "Debug", IceTabPage.WindowType.Debug);
                if (de != null)
                {
                    ChannelBar.TabPages.Remove(de);
                    mainTabControl.Controls.Remove(de);
                    return;
                }
                
                IceTabPage wi = GetWindow(null, windowCaption, IceTabPage.WindowType.Window);
                if (wi != null)
                {
                    System.Diagnostics.Debug.WriteLine("remove window:" + windowCaption);
                    ChannelBar.TabPages.Remove(wi);
                    mainTabControl.Controls.Remove(wi);
                    return;
                }
                
                IceTabPage df = GetWindow(null, windowCaption, IceTabPage.WindowType.DCCFile);
                if (df != null)
                {
                    System.Diagnostics.Debug.WriteLine("remove window:" + windowCaption);
                    ChannelBar.TabPages.Remove(df);
                    mainTabControl.Controls.Remove(df);
                    return;
                }
            });
        }

        /// <summary>
        /// Close All Channels/Query Tabs for specified Connection
        /// </summary>
        /// <param name="connection">Which Connection it is for</param>
        internal void CloseAllWindows(IRCConnection connection)
        {
            for (int i = mainChannelBar.TabPages.Count - 1; i > 0; i--)
            {
                if (mainChannelBar.TabPages[i].Connection == connection)
                    mainChannelBar.TabPages.Remove(mainChannelBar.TabPages[i]);

            }
            mainChannelBar.Invalidate();
            
        }

        internal string GetMessageFormat(string MessageName)
        {
            foreach (ServerMessageFormatItem msg in iceChatMessages.MessageSettings)
            {
                if (msg.MessageName.ToLower() == MessageName.ToLower())
                    return msg.FormattedMessage;
            }
            return null;
        }
        
        
        /// <summary>
        /// A New Tab was Selected for the Main Tab Control
        /// Update the Input Panel with the Current Connection
        /// Change the Status text for the Status Bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabSelectedIndexChanged(object sender, TabEventArgs e)
        {
            if (!this.IsHandleCreated && !this.IsDisposed) return; 
            
            this.Invoke((MethodInvoker)delegate()
            {
                if (mainChannelBar.CurrentTab.WindowStyle != IceTabPage.WindowType.Console)
                {
                    //System.Diagnostics.Debug.WriteLine("TabSelected:" + mainChannelBar.CurrentTab.TabCaption);
                    if (mainChannelBar.CurrentTab != null)
                    {
                        IceTabPage t = mainChannelBar.CurrentTab;

                        //if (t.TextWindow != null)
                        //    t.TextWindow.resetUnreadMarker();

                        nickList.RefreshList(t);
                        inputPanel.CurrentConnection = t.Connection;
                        string network = "";

                        if (CurrentWindowType != IceTabPage.WindowType.Debug && CurrentWindowType != IceTabPage.WindowType.DCCFile && CurrentWindowType != IceTabPage.WindowType.Window && t.Connection.ServerSetting.NetworkName.Length > 0)
                            network = " (" + t.Connection.ServerSetting.NetworkName + ")";
                        if (CurrentWindowType == IceTabPage.WindowType.Channel)
                            StatusText(t.Connection.ServerSetting.CurrentNickName + " in channel " + t.TabCaption + " [" + t.ChannelModes + "] {" + t.Connection.ServerSetting.RealServerName + "}" + network);
                        else if (CurrentWindowType == IceTabPage.WindowType.Query)
                            StatusText(t.Connection.ServerSetting.CurrentNickName + " in private chat with " + t.TabCaption + " {" + t.Connection.ServerSetting.RealServerName + "}" + network);
                        else if (CurrentWindowType == IceTabPage.WindowType.DCCChat)
                            StatusText(t.Connection.ServerSetting.CurrentNickName + " in DCC chat with " + t.TabCaption + " {" + t.Connection.ServerSetting.RealServerName + "}" + network);
                        else if (CurrentWindowType == IceTabPage.WindowType.ChannelList)
                            StatusText(t.Connection.ServerSetting.CurrentNickName + " in Channel List for {" + t.Connection.ServerSetting.RealServerName + "}" + network);

                        CurrentWindow.LastMessageType = ServerMessageType.Default;
                        t = null;

                        if (!e.IsHandled)
                            serverTree.SelectTab(mainChannelBar.CurrentTab, false);

                    }
                }
                else
                {
                    //make sure the 1st tab is not selected
                    nickList.RefreshList();
                    nickList.Header = iceChatLanguage.consoleTabTitle;
                    if (mainChannelBar.GetTabPage("Console").ConsoleTab.SelectedIndex != 0)
                    {
                        inputPanel.CurrentConnection = mainChannelBar.GetTabPage("Console").CurrentConnection;

                        string network = "";
                        if (inputPanel.CurrentConnection.ServerSetting.NetworkName.Length > 0)
                            network = " (" + inputPanel.CurrentConnection.ServerSetting.NetworkName + ")";

                        if (inputPanel.CurrentConnection.IsConnected)
                        {
                            if (inputPanel.CurrentConnection.ServerSetting.UseBNC)
                                StatusText(inputPanel.CurrentConnection.ServerSetting.CurrentNickName + " connected to " + inputPanel.CurrentConnection.ServerSetting.BNCIP);
                            else
                                StatusText(inputPanel.CurrentConnection.ServerSetting.CurrentNickName + " connected to " + inputPanel.CurrentConnection.ServerSetting.RealServerName + network);
                        }
                        else
                        {
                            if (inputPanel.CurrentConnection.ServerSetting.UseBNC)
                                StatusText(inputPanel.CurrentConnection.ServerSetting.CurrentNickName + " disconnected from " + inputPanel.CurrentConnection.ServerSetting.BNCIP);
                            else
                                StatusText(inputPanel.CurrentConnection.ServerSetting.CurrentNickName + " disconnected from " + inputPanel.CurrentConnection.ServerSetting.ServerName + network);

                        }

                        if (!e.IsHandled)
                            serverTree.SelectTab(mainChannelBar.GetTabPage("Console").CurrentConnection.ServerSetting, false);
                    }
                    else
                    {
                        inputPanel.CurrentConnection = null;
                        StatusText("Welcome to " + ProgramID + " " + VersionID);
                    }
                }

                inputPanel.FocusTextBox();
            });
        }


       
        /// <summary>
        /// Closes the Tab selected
        /// </summary>
        /// <param name="tab">Which tab to Close</param>        
        private void mainTabControl_OnTabClosed(int nIndex)
        {
            if (mainChannelBar.GetTabPage(nIndex).WindowStyle == IceTabPage.WindowType.Channel)
            {
                foreach (IRCConnection c in serverTree.ServerConnections.Values)
                {
                    if (c == mainChannelBar.GetTabPage(nIndex).Connection)
                    {
                        //check if connected
                        if (c.IsConnected)
                            ParseOutGoingCommand(c, "/part " + mainChannelBar.GetTabPage(nIndex).TabCaption);
                        else
                            RemoveWindow(c, mainChannelBar.GetTabPage(nIndex).TabCaption, mainChannelBar.GetTabPage(nIndex).WindowStyle);

                        return;
                    }
                }
            }
            else if (mainChannelBar.GetTabPage(nIndex).WindowStyle == IceTabPage.WindowType.Query)
            {
                mainTabControl.Controls.Remove(mainChannelBar.GetTabPage(nIndex));
                RemoveWindow(mainChannelBar.GetTabPage(nIndex).Connection, mainChannelBar.GetTabPage(nIndex).TabCaption, IceTabPage.WindowType.Query);
            }
            else if (mainChannelBar.GetTabPage(nIndex).WindowStyle == IceTabPage.WindowType.Console)
            {
                if (mainChannelBar.GetTabPage("Console").CurrentConnection != null)
                {
                    if (mainChannelBar.GetTabPage("Console").CurrentConnection.IsConnected)
                    {
                        ParseOutGoingCommand(mainChannelBar.GetTabPage("Console").CurrentConnection, "/quit");
                    }
                    else
                    {
                        //remove the tab
                        mainChannelBar.GetTabPage("Console").RemoveConsoleTab(mainChannelBar.GetTabPage("Console").ConsoleTab.SelectedIndex);
                    }
                }
            }
            else if (mainChannelBar.GetTabPage(nIndex).WindowStyle == IceTabPage.WindowType.ChannelList)
            {
                //check if channel list is completed or not, do not close if it has not
                if (mainChannelBar.GetTabPage(nIndex).ChannelListComplete)
                {
                    mainTabControl.Controls.Remove(mainChannelBar.GetTabPage(nIndex));
                    RemoveWindow(mainChannelBar.GetTabPage(nIndex).Connection, mainChannelBar.GetTabPage(nIndex).TabCaption, IceTabPage.WindowType.ChannelList);
                }
                else
                    System.Media.SystemSounds.Beep.Play();
            }
            else if (mainChannelBar.GetTabPage(nIndex).WindowStyle == IceTabPage.WindowType.DCCChat)
            {
                mainTabControl.Controls.Remove(mainChannelBar.GetTabPage(nIndex));
                RemoveWindow(mainChannelBar.GetTabPage(nIndex).Connection, mainChannelBar.GetTabPage(nIndex).TabCaption, IceTabPage.WindowType.DCCChat);
            }
            else if (mainChannelBar.GetTabPage(nIndex).WindowStyle == IceTabPage.WindowType.DCCFile)
            {
                mainTabControl.Controls.Remove(mainChannelBar.GetTabPage(nIndex));
                RemoveWindow(mainChannelBar.GetTabPage(nIndex).Connection, mainChannelBar.GetTabPage(nIndex).TabCaption, IceTabPage.WindowType.DCCFile);
            }
            else if (mainChannelBar.GetTabPage(nIndex).WindowStyle == IceTabPage.WindowType.Window)
            {
                mainTabControl.Controls.Remove(mainChannelBar.GetTabPage(nIndex));
                RemoveWindow(mainChannelBar.GetTabPage(nIndex).Connection, mainChannelBar.GetTabPage(nIndex).TabCaption, IceTabPage.WindowType.Window);
            }
            else if (mainChannelBar.GetTabPage(nIndex).WindowStyle == IceTabPage.WindowType.Debug)
            {
                mainTabControl.Controls.Remove(mainChannelBar.GetTabPage(nIndex));
                RemoveWindow(mainChannelBar.GetTabPage(nIndex).Connection, mainChannelBar.GetTabPage(nIndex).TabCaption, IceTabPage.WindowType.Debug);
            }
        }

        #endregion
        
        #region InputPanel Events

        /// <summary>
        /// Parse out command written in Input Box or sent from Plugin
        /// </summary>
        /// <param name="connection">Which Connection it is for</param>
        /// <param name="data">The Message to Parse</param>
        public void ParseOutGoingCommand(IRCConnection connection, string data)
        {
            int error = 0;
            
            try
            {
                data = data.Replace("&#x3;", "\x0003");
                data = data.Replace("&#x2;", "\x0002");
                data = data.Replace("&#x1F;", "\x001F");
                data = data.Replace("&#x1D;", "\x001D");
                data = data.Replace("&#x16;", "\x0016");
                data = data.Replace("&#x0F;", "\x000F");
                
                data = data.Replace(@"\%C", "\x0003");
                data = data.Replace(@"\%B", "\x0002");
                data = data.Replace(@"\%U", "\x001F");
                data = data.Replace(@"\%I", "\x001D");
                data = data.Replace(@"\%R", "\x0016");
                data = data.Replace(@"\%O", "\x000F");
                

                PluginArgs args = new PluginArgs(connection);
                args.Command = data;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        args = ipc.InputText(args);
                }

                data = args.Command;
                
                if (data.Length == 0)
                    return;

                if (data.StartsWith("//"))
                {
                    //parse out identifiers
                    ParseOutGoingCommand(connection, ParseIdentifiers(connection, data, data));
                    return;
                }

                if (data.StartsWith("/"))
                {
                    int indexOfSpace = data.IndexOf(" ");
                    string command = "";
                    string temp = "";

                    if (indexOfSpace > 0)
                    {
                        command = data.Substring(0, indexOfSpace);
                        data = data.Substring(command.Length + 1);
                    }
                    else
                    {
                        command = data;
                        data = "";
                    }

                    //check for aliases
                    foreach (AliasItem a in iceChatAliases.listAliases)
                    {
                        if (a.AliasName == command)
                        {
                            if (a.Command.Length == 1)
                            {
                                data = ParseIdentifierValue(a.Command[0], data);
                                ParseOutGoingCommand(connection, ParseIdentifiers(connection, data, data));
                            }
                            else
                            {
                                //it is a multulined alias, run multiple commands
                                foreach (string c in a.Command)
                                {
                                    data = ParseIdentifierValue(c, data);
                                    ParseOutGoingCommand(connection, ParseIdentifiers(connection, data, data));
                                }
                            }
                            return;
                        }
                    }

                    switch (command.ToLower())
                    {
                        case "/makeexception":
                            throw new Exception("IceChat 9 Test Exception Error");
                        
                        case "/debug":
                            debugWindowToolStripMenuItem.PerformClick();
                            break;
            
                        /*
                        case "/addnicks":
                            if (connection != null)
                            {
                                if (CurrentWindowType == IceTabPage.WindowType.Channel)
                                {
                                    for (int i = 0; i < 250; i++)
                                    {
                                        CurrentWindow.AddNick("nick" + i.ToString(), true);
                                    }                                    
                                }
                            }
                            break;
                        */
                        case "/addlines":
                            for (int i = 0; i < 250; i++)
                            {
                                //pick a random color
                                int randColor = new Random().Next(0, 71);

                                string msg = "The quick brown \x00033,4fox jumps over the\x0003 www.icechat.net lazy dog and gets away with it at icechat.codeplex.com";
                                CurrentWindowMessage(connection, msg, randColor, true);
                            }
                            break;

                        case "/addlines2":
                            for (int i = 0; i < 100; i++)
                            {
                                int randColor = new Random().Next(0, 71);
                                
                                string msg = "The quick brown \x00033,4fox jumps over the\x0003 www.icechat.net lazy dog and gets away with it at http://icechat.codeplex.com";
                                CurrentWindowMessage(connection, msg, randColor, true);
                            }
                            break;

                        case "/addlines3":
                            for (int i = 0; i < 50; i++)
                            {
                                int randColor = new Random().Next(0, 71);

                                string msg = "The quick brown \x00033,4fox jumps over the\x0003 www.icechat.net lazy dog and gets away with it at http://icechat.codeplex.com";
                                CurrentWindowMessage(connection, msg, randColor, true);
                            }
                            break;

                        case "/dump":
                            if (data.Length == 0)
                            {
                                if (CurrentWindowType == IceTabPage.WindowType.Channel)
                                {
                                    //dump the raw data display to a log file
                                    string[] lines = CurrentWindow.TextWindow.DisplayLines;
                                    string dumpFile = CurrentWindow.TextWindow.LogFileLocation + Path.DirectorySeparatorChar + CurrentWindow.TabCaption + ".dump.txt";
                                    try
                                    {
                                        FileStream logFile = new FileStream(dumpFile, FileMode.Append);

                                        for (int i = 0; i < lines.Length; i++)
                                        {
                                            logFile.Write(System.Text.Encoding.Default.GetBytes(lines[i] + "\r\n"), 0, lines[i].Length + 2);
                                        }

                                        logFile.Flush();
                                        logFile.Close();
                                        logFile.Dispose();

                                        System.Diagnostics.Process.Start(dumpFile);
                                    }
                                    catch
                                    {
                                        //errored
                                        System.Diagnostics.Debug.WriteLine("dump file error:" + dumpFile);
                                    }
                                }
                                else if (CurrentWindowType == IceTabPage.WindowType.Console)
                                {
                                    string[] lines = mainChannelBar.GetTabPage("Console").CurrentConsoleWindow().DisplayLines;
                                    string dumpFile = mainChannelBar.GetTabPage("Console").CurrentConsoleWindow().LogFileLocation + Path.DirectorySeparatorChar + "console.dump.txt";

                                    try
                                    {
                                        FileStream logFile = new FileStream(dumpFile, FileMode.Append);

                                        for (int i = 0; i < lines.Length; i++)
                                        {
                                            logFile.Write(System.Text.Encoding.Default.GetBytes(lines[i] + "\r\n"), 0, lines[i].Length + 2);
                                        }

                                        logFile.Flush();
                                        logFile.Close();
                                        logFile.Dispose();

                                        System.Diagnostics.Process.Start(dumpFile);
                                    }
                                    catch
                                    {
                                        //errored
                                        System.Diagnostics.Debug.WriteLine("dump file error:" + dumpFile);
                                    }
                                    
                                }
                            }
                            break;
                        case "/background":
                        case "/bg": //change background image for a window(s)
                            if (data.Length > 0)
                            {
                                //bg windowtype imagefile
                                //bg windowtype windowname imagefile
                                //if imagefile is 'none', erase background image
                                string window = data.Split(' ')[0];
                                string file = "";
                                if (data.IndexOf(' ') > -1)
                                    file = data.Substring(window.Length + 1);

                                switch (window.ToLower())
                                {
                                    case "nicklist":
                                        if (file.Length == 0)
                                        {
                                            //ask for a file/picture
                                            file = GetBackgroundImage();
                                            if (file.Length > 0)
                                            {
                                                this.nickList.BackGroundImage = file;
                                                //save the image file
                                                if (System.IO.Path.GetDirectoryName(file).ToLower().CompareTo(picturesFolder.ToLower()) == 0)
                                                    iceChatOptions.NickListImage = System.IO.Path.GetFileName(file);
                                                else
                                                    iceChatOptions.NickListImage = file;

                                            }
                                            return;
                                        }
                                        else if (file.ToLower() == "none" || file.ToLower() == "remove")
                                        {
                                            this.nickList.BackGroundImage = "";
                                            iceChatOptions.NickListImage = "";
                                        }
                                        else
                                        {
                                            //check if it is a full path or just a pic in the pictures folder
                                            if (File.Exists(picturesFolder + System.IO.Path.DirectorySeparatorChar + file))
                                            {
                                                this.nickList.BackGroundImage = picturesFolder + System.IO.Path.DirectorySeparatorChar + file;
                                                iceChatOptions.NickListImage = file;
                                            }
                                            else if (File.Exists(file))
                                            {
                                                this.nickList.BackGroundImage = file;
                                                iceChatOptions.NickListImage = file;
                                            }
                                        }
                                        
                                        break;                                    
                                    case "serverlist":
                                    case "servertree":
                                        if (file.Length == 0)
                                        {
                                            //ask for a file/picture
                                            file = GetBackgroundImage();
                                            if (file.Length > 0)
                                            {
                                                this.serverTree.BackGroundImage = file;
                                                //save the image file
                                                if (System.IO.Path.GetDirectoryName(file).ToLower().CompareTo(picturesFolder.ToLower()) == 0)
                                                    iceChatOptions.ServerTreeImage = System.IO.Path.GetFileName(file);
                                                else
                                                    iceChatOptions.ServerTreeImage = file;

                                            }                                            
                                            return;
                                        }
                                        else if (file.ToLower() == "none" || file.ToLower() == "remove")
                                        {
                                            this.serverTree.BackGroundImage = "";
                                            iceChatOptions.ServerTreeImage = "";
                                        }
                                        else
                                        {
                                            //check if it is a full path or just a pic in the pictures folder
                                            if (File.Exists(picturesFolder + System.IO.Path.DirectorySeparatorChar + file))
                                            {
                                                this.serverTree.BackGroundImage = picturesFolder + System.IO.Path.DirectorySeparatorChar + file;
                                                iceChatOptions.ServerTreeImage = file;
                                            }
                                            else if (File.Exists(file))
                                            {
                                                this.serverTree.BackGroundImage = file;
                                                iceChatOptions.ServerTreeImage = file;
                                            }
                                        }                                        
                                        break;                                    
                                    case "console":
                                        //check if the file is a URL
                                        if (file.Length > 0)
                                        {
                                            if (File.Exists(picturesFolder + System.IO.Path.DirectorySeparatorChar + file))
                                                mainChannelBar.GetTabPage("Console").CurrentConsoleWindow().BackGroundImage = picturesFolder + System.IO.Path.DirectorySeparatorChar + file;
                                            else
                                            {
                                                //check if this is a full path to the file
                                                if (File.Exists(file))
                                                    mainChannelBar.GetTabPage("Console").CurrentConsoleWindow().BackGroundImage = file;
                                                else
                                                    mainChannelBar.GetTabPage("Console").CurrentConsoleWindow().BackGroundImage = "";
                                            }
                                        }
                                        break;
                                    case "channel":
                                        //get the channel name
                                        if (file.IndexOf(' ') > -1)
                                        {
                                            string channel = file.Split(' ')[0];
                                            //if channel == "all" do it for all

                                            file = file.Substring(channel.Length + 1);
                                            if (channel.ToLower() == "all")
                                            {
                                                foreach (IceTabPage t in mainChannelBar.TabPages)
                                                {
                                                    if (t.WindowStyle == IceTabPage.WindowType.Channel)
                                                    {
                                                        if (File.Exists(picturesFolder + System.IO.Path.DirectorySeparatorChar + file))
                                                            t.TextWindow.BackGroundImage = (picturesFolder + System.IO.Path.DirectorySeparatorChar + file);
                                                        else
                                                            t.TextWindow.BackGroundImage = "";

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
                                                if (t != null)
                                                {
                                                    if (File.Exists(picturesFolder + System.IO.Path.DirectorySeparatorChar + file))
                                                        t.TextWindow.BackGroundImage = (picturesFolder + System.IO.Path.DirectorySeparatorChar + file);
                                                    else
                                                        t.TextWindow.BackGroundImage = "";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //only a channel name specified, no file, erase the image
                                            //if file == "all" clear em all
                                            if (file.ToLower() == "all")
                                            {
                                                foreach (IceTabPage t in mainChannelBar.TabPages)
                                                {
                                                    if (t.WindowStyle == IceTabPage.WindowType.Channel)
                                                        t.TextWindow.BackGroundImage = "";
                                                }
                                            }
                                            else
                                            {
                                                IceTabPage t = GetWindow(connection, file, IceTabPage.WindowType.Channel);
                                                if (t != null)
                                                    t.TextWindow.BackGroundImage = "";
                                            }
                                        }
                                        break;
                                    case "query":
                                        
                                        break;
                                    case "window":
                                        if (file.IndexOf(' ') > -1)
                                        {
                                            string windowName = file.Split(' ')[0];

                                            file = file.Substring(windowName.Length + 1);
                                            IceTabPage t = GetWindow(connection, windowName, IceTabPage.WindowType.Window);
                                            if (t != null)
                                            {
                                                if (File.Exists(picturesFolder + System.IO.Path.DirectorySeparatorChar + file))
                                                    t.TextWindow.BackGroundImage = (picturesFolder + System.IO.Path.DirectorySeparatorChar + file);
                                                else
                                                    t.TextWindow.BackGroundImage = "";
                                            }

                                        }
                                        else
                                        {
                                            IceTabPage t = GetWindow(connection, file, IceTabPage.WindowType.Window);
                                            if (t != null)
                                                t.TextWindow.BackGroundImage = "";

                                        }
                                        break;
                                }
                            }
                            break;
                        case "/bgcolor":
                            //change the background color for the current or selected window
                            if (data.Length > 0)
                            {
                                int result;
                                if (Int32.TryParse(data, out result))
                                    if (result >= 0 && result < 72)
                                        if (CurrentWindowType == IceTabPage.WindowType.Console)
                                            mainChannelBar.GetTabPage("Console").CurrentConsoleWindow().IRCBackColor = result;
                                        else if (CurrentWindowType == IceTabPage.WindowType.Channel || CurrentWindowType == IceTabPage.WindowType.Query || CurrentWindowType == IceTabPage.WindowType.Window)
                                            CurrentWindow.TextWindow.IRCBackColor = result;
                            }
                            break;
                        case "/unloadplugin":
                            if (data.Length > 0)
                            {
                                //get the plugin name, and look for it in the menu items
                                ToolStripMenuItem menuItem = null;
                                foreach (ToolStripMenuItem t in pluginsToolStripMenuItem.DropDownItems)
                                    if (t.ToolTipText.ToLower() == data.ToLower())
                                        menuItem = t;
                                
                                if (menuItem != null)
                                {
                                    //match
                                    IPluginIceChat plugin = (IPluginIceChat)menuItem.Tag;
                                    
                                    //remove any panels added to the main form
                                    Panel[] addedPanels = plugin.AddMainPanel();
                                    if (addedPanels != null && addedPanels.Length > 0)
                                    {
                                        foreach (Panel p in addedPanels)
                                        {
                                            this.Controls.Remove(p);
                                        }
                                    }

                                    plugin.OnCommand -= new OutGoingCommandHandler(Plugin_OnCommand);
                                    loadedPlugins.Remove(plugin);
                                    menuItem.Click -= new EventHandler(OnPluginMenuItemClick);
                                    pluginsToolStripMenuItem.DropDownItems.Remove(menuItem);

                                    plugin.Dispose();

                                    WindowMessage(null, "Console", "Unloaded Plugin - " + plugin.Name, 4, true);                                    
                                    
                                }
                            }
                            break;                        
                        case "/statusplugin":
                            if (data.Length > 0 && data.IndexOf(' ') > 0)
                            {
                                string[] values = data.Split(new char[] { ' ' }, 2);

                                ToolStripMenuItem menuItem = null;
                                foreach (ToolStripMenuItem t in pluginsToolStripMenuItem.DropDownItems)
                                    if (t.ToolTipText.ToLower() == values[1].ToLower())
                                        menuItem = t;

                                if (menuItem != null)
                                {
                                    //match
                                    IPluginIceChat plugin = (IPluginIceChat)menuItem.Tag;
                                    plugin.Enabled = Convert.ToBoolean(values[0]);

                                    if (plugin.Enabled == true)
                                    {
                                        WindowMessage(null, "Console", "Enabled Plugin - " + plugin.Name + " v" + plugin.Version, 4, true);

                                        //init the plugin
                                        System.Threading.Thread initPlugin = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(InitializePlugin));
                                        initPlugin.Start(plugin);

                                        //remove the icon
                                        menuItem.Image = null;
                                    }
                                    else
                                    {
                                        WindowMessage(null, "Console", "Disabled Plugin - " + plugin.Name + " v" + plugin.Version, 4, true);
                                        menuItem.Image = StaticMethods.LoadResourceImage("CloseButton.png");
                                    }
                                }
                            }
                            break;
                        case "/loadplugin":
                            if (data.Length > 0)                            
                            {
                                IPluginIceChat ipc = loadPlugin(pluginsFolder + System.IO.Path.DirectorySeparatorChar + data);
                                if (ipc != null)
                                {
                                    System.Threading.Thread initPlugin = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(InitializePlugin));
                                    initPlugin.Start(ipc);
                                }
                            }                            
                            break;
                        case "/reload":
                            if (data.Length > 0)
                            {
                                switch (data)
                                {
                                    case "alias":
                                    case "aliases":
                                        CurrentWindowMessage(connection, "Aliases file reloaded", 4, true);
                                        LoadAliases();
                                        break;
                                    case "popup":
                                    case "popups":
                                        CurrentWindowMessage(connection, "Popups file reloaded", 4, true);
                                        LoadPopups();
                                        break;
                                    case "emoticon":
                                    case "emoticons":
                                        CurrentWindowMessage(connection, "Emoticons file reloaded", 4, true);
                                        LoadEmoticons();
                                        break;
                                    case "sound":
                                    case "sounds":
                                        CurrentWindowMessage(connection, "Sounds file reloaded", 4, true);
                                        LoadSounds();
                                        break;
                                    case "color":
                                    case "colors":
                                        CurrentWindowMessage(connection, "Colors file reloaded", 4, true);
                                        LoadColors();
                                        menuMainStrip.BackColor = IrcColor.colors[iceChatColors.MenubarBackColor];
                                        statusStripMain.BackColor = IrcColor.colors[iceChatColors.StatusbarBackColor];
                                        toolStripStatus.ForeColor = IrcColor.colors[iceChatColors.StatusbarForeColor];
                                        inputPanel.SetInputBoxColors();
                                        channelList.SetListColors();
                                        buddyList.SetListColors();
                                        break;
                                    case "font":
                                    case "fonts":
                                        CurrentWindowMessage(connection, "Fonts file reloaded", 4, true);
                                        LoadFonts();
                                        nickList.Font = new Font(iceChatFonts.FontSettings[3].FontName, iceChatFonts.FontSettings[3].FontSize);
                                        serverTree.Font = new Font(iceChatFonts.FontSettings[4].FontName, iceChatFonts.FontSettings[4].FontSize);
                                        //menuMainStrip.Font = new Font(iceChatFonts.FontSettings[7].FontName, iceChatFonts.FontSettings[7].FontSize);
                                        break;
                                }
                            }                            
                            break;                        
                        /*
                        case "/reloadplugin":
                            if (data.Length > 0)
                            {
                                //get the plugin name, and look for it in the menu items
                                ToolStripMenuItem menuItem = null;
                                foreach (ToolStripMenuItem t in pluginsToolStripMenuItem.DropDownItems)
                                    if (t.ToolTipText.ToLower() == data.ToLower())
                                        menuItem = t;

                                if (menuItem != null)
                                {
                                    //match
                                                                       
                                    IPluginIceChat plugin = (IPluginIceChat)menuItem.Tag;

                                    plugin.OnCommand -= new OutGoingCommandHandler(Plugin_OnCommand);
                                    loadedPlugins.Remove(plugin);
                                    plugin.Dispose();

                                    Type ObjType = null;
                                    Assembly ass = null;

                                    try
                                    {
                                        //reload the plugin
                                        ass = Assembly.LoadFile(pluginsFolder + System.IO.Path.DirectorySeparatorChar + menuItem.ToolTipText);

                                        //System.Diagnostics.Debug.WriteLine(ass.ToString());
                                        if (ass != null)
                                        {
                                            ObjType = ass.GetType("IceChatPlugin.Plugin");
                                        }
                                        else
                                        {
                                            System.Diagnostics.Debug.WriteLine("assembly is null");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        WriteErrorFile(connection, "ReLoadPlugins Cast:", ex);
                                    }
                                    try
                                    {
                                        // OK Lets create the object as we have the Report Type
                                        if (ObjType != null)
                                        {
                                            //System.Diagnostics.Debug.WriteLine("create instance of " + args);

                                            IPluginIceChat ipi = (IPluginIceChat)Activator.CreateInstance(ObjType);

                                            ipi.MainForm = this;
                                            ipi.MainMenuStrip = this.MainMenuStrip;
                                            ipi.CurrentFolder = currentFolder;

                                            WindowMessage(null, "Console", "Re-Loaded Plugin - " + ipi.Name + " v" + ipi.Version + " by " + ipi.Author, 4, true);

                                            menuItem.Tag = ipi;

                                            ipi.OnCommand += new OutGoingCommandHandler(Plugin_OnCommand);
                                            ipi.Initialize();
                                            loadedPlugins.Add(ipi);
                                        }
                                        else
                                        {
                                            System.Diagnostics.Debug.WriteLine("obj type is null:" + menuItem.ToolTipText);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        WriteErrorFile(connection, "ReLoadPlugins", ex);
                                    }
                                }
                            }
                            break;                        
                         */
                         case "/addtext":
                            if (data.Length > 0)
                            {
                                AddInputPanelText(data);
                            }
                            break;

                        case "/beep":
                            System.Media.SystemSounds.Beep.Play();
                            break;
                        case "/size":
                            CurrentWindowMessage(connection, "Window Size is: " + this.Width + ":" + this.Height, 4, true);   
                            break;
                        case "/ame":    //me command for all channels
                            if (connection != null && data.Length > 0)
                            {
                                foreach (IceTabPage t in mainChannelBar.TabPages)
                                {
                                    if (t.WindowStyle == IceTabPage.WindowType.Channel)
                                    {
                                        if (t.Connection == connection)
                                        {
                                            SendData(connection, "PRIVMSG " + t.TabCaption + " :ACTION " + data + "");
                                            string msg = GetMessageFormat("Self Channel Action");
                                            msg = msg.Replace("$nick", t.Connection.ServerSetting.CurrentNickName).Replace("$channel", t.TabCaption);
                                            msg = msg.Replace("$message", data);

                                            t.TextWindow.AppendText(msg, 1);
                                            t.TextWindow.ScrollToBottom();
                                            t.LastMessageType = ServerMessageType.Action;
                                        }
                                    }
                                }
                            }
                            break;
                        case "/amsg":   //send a message to all channels 
                            if (connection != null && data.Length > 0)
                            {
                                foreach (IceTabPage t in mainChannelBar.TabPages)
                                {
                                    if (t.WindowStyle == IceTabPage.WindowType.Channel)
                                    {
                                        if (t.Connection == connection)
                                        {
                                            SendData(connection, "PRIVMSG " + t.TabCaption + " :" + data);
                                            string msg = GetMessageFormat("Self Channel Message");
                                            msg = msg.Replace("$nick", t.Connection.ServerSetting.CurrentNickName).Replace("$channel", t.TabCaption);

                                            //assign $color to the nickname 
                                            if (msg.Contains("$color"))
                                            {
                                                User u = CurrentWindow.GetNick(t.Connection.ServerSetting.CurrentNickName);
                                                //get the nick color
                                                if (u.nickColor == -1)
                                                {
                                                    if (FormMain.Instance.IceChatColors.RandomizeNickColors == true)
                                                    {
                                                        int randColor = new Random().Next(0, 71);
                                                        if (randColor == FormMain.Instance.IceChatColors.NickListBackColor)
                                                            randColor = new Random().Next(0, 71);
                                                        u.nickColor = randColor;
                                                    }
                                                    else
                                                    {
                                                        //get the correct nickname color for channel status
                                                        for (int y = 0; y < u.Level.Length; y++)
                                                        {
                                                            if (u.Level[y])
                                                            {
                                                                switch (connection.ServerSetting.StatusModes[0][y])
                                                                {
                                                                    case 'q':
                                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelOwnerColor;
                                                                        break;
                                                                    case 'y':
                                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelOwnerColor;
                                                                        break;
                                                                    case 'a':
                                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelAdminColor;
                                                                        break;
                                                                    case 'o':
                                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelOpColor;
                                                                        break;
                                                                    case 'h':
                                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelHalfOpColor;
                                                                        break;
                                                                    case 'v':
                                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelVoiceColor;
                                                                        break;
                                                                    default:
                                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelRegularColor;
                                                                        break;
                                                                }
                                                                break;
                                                            }
                                                        }

                                                    }
                                                    if (u.nickColor == -1)
                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelRegularColor;

                                                }

                                                msg = msg.Replace("$color", "\x0003" + u.nickColor.ToString("00"));
                                            }

                                            msg = msg.Replace("$status", CurrentWindow.GetNick(t.Connection.ServerSetting.CurrentNickName).ToString().Replace(t.Connection.ServerSetting.CurrentNickName , ""));
                                            msg = msg.Replace("$message", data);

                                            t.TextWindow.AppendText(msg, 1);
                                            t.TextWindow.ScrollToBottom();
                                            t.LastMessageType = ServerMessageType.Message;

                                        }
                                    }
                                }
                            }
                            break;
                        case "/anick":
                            if (data.Length > 0)
                            {
                                foreach (IRCConnection c in serverTree.ServerConnections.Values)
                                    if (c.IsConnected)
                                        SendData(c, "NICK " + data);
                            }
                            break;                        
                        case "/autojoin":
                            if (connection != null)
                            {
                                if (data.Length == 0)
                                {
                                    if (connection.ServerSetting.AutoJoinChannels != null)
                                    {
                                        foreach (string chan in connection.ServerSetting.AutoJoinChannels)
                                        {
                                            if (!chan.StartsWith(";"))
                                                SendData(connection, "JOIN " + chan);
                                        }
                                    }
                                }
                                else
                                {
                                    if (connection.ServerSetting.AutoJoinChannels == null)
                                    {
                                        //we have no autojoin channels, so just add it
                                        connection.ServerSetting.AutoJoinChannels = new string[1];
                                        connection.ServerSetting.AutoJoinChannels[0] = data;
                                        CurrentWindowMessage(connection, data + " is added to the Autojoin List", 7, true);
                                        serverTree.SaveServers(serverTree.ServersCollection);
                                    }
                                    else
                                    {
                                        //check if it is in the list first
                                        bool Exists = false;
                                        bool Disabled = false;
                                        
                                        string[] oldAutoJoin = new string[connection.ServerSetting.AutoJoinChannels.Length];
                                        int i = 0;
                                        foreach (string chan in connection.ServerSetting.AutoJoinChannels)
                                        {
                                            if (chan.ToLower() == data.ToLower())
                                            {
                                                //already in the list
                                                Exists = true;
                                                oldAutoJoin[i] = chan;
                                                CurrentWindowMessage(connection, data + " is already in the Autojoin List", 7, true);
                                            }
                                            else if (chan.ToLower() == (";" + data.ToLower()))
                                            {
                                                //already in the list, but disabled
                                                //so lets enable it
                                                Disabled = true;
                                                oldAutoJoin[i] = chan.Substring(1);
                                                Exists = true;
                                                CurrentWindowMessage(connection, data + " is enabled in the Autojoin List", 7, true);
                                            }
                                            else
                                                oldAutoJoin[i] = chan;

                                            i++;
                                        }

                                        if (!Exists)
                                        {
                                            //add a new item
                                            connection.ServerSetting.AutoJoinChannels = new string[connection.ServerSetting.AutoJoinChannels.Length + 1];
                                            i = 0;
                                            foreach (string chan in oldAutoJoin)
                                            {
                                                connection.ServerSetting.AutoJoinChannels[i] = chan;
                                                i++;
                                            }
                                            connection.ServerSetting.AutoJoinChannels[i] = data;
                                            CurrentWindowMessage(connection, data + " is added to the Autojoin List", 7, true);
                                            serverTree.SaveServers(serverTree.ServersCollection);
                                        }
                                        else if (Disabled)
                                        {
                                            connection.ServerSetting.AutoJoinChannels = new string[connection.ServerSetting.AutoJoinChannels.Length];
                                            i = 0;
                                            foreach (string chan in oldAutoJoin)
                                            {
                                                connection.ServerSetting.AutoJoinChannels[i] = chan;
                                            }
                                            serverTree.SaveServers(serverTree.ServersCollection);
                                        }
                                    }
                                }
                            }
                            break;
                        case "/autoperform":
                            if (connection != null)
                            {
                                if (connection.ServerSetting.AutoPerform != null)
                                {
                                    foreach (string ap in connection.ServerSetting.AutoPerform)
                                    {
                                        string autoCommand = ap.Replace("\r", String.Empty);
                                        if (!autoCommand.StartsWith(";"))
                                            ParseOutGoingCommand(connection, autoCommand);
                                    }
                                }
                            }
                            break;
                        case "/aaway":
                            foreach (IRCConnection c in serverTree.ServerConnections.Values)
                            {
                                if (c.IsConnected)
                                {
                                    ParseOutGoingCommand(c, "/away " + data);
                                }
                            }
                            break;
                        case "/away":
                            if (connection != null)
                            {
                                if (connection.ServerSetting.Away)
                                {
                                    connection.ServerSetting.Away = false;
                                    //ParseOutGoingCommand(connection, "/nick " + connection.ServerSetting.DefaultNick);
                                    SendData(connection, "NICK " + connection.ServerSetting.DefaultNick);
                                    TimeSpan t = DateTime.Now.Subtract(connection.ServerSetting.AwayStart);

                                    string s = t.Seconds.ToString() + " secs";
                                    if (t.Minutes > 0)
                                        s = t.Minutes.ToString() + " mins " + s;
                                    if (t.Hours > 0)
                                        s = t.Hours.ToString() + " hrs " + s;
                                    if (t.Days > 0)
                                        s = t.Days.ToString() + " days " + s;


                                    string msg = iceChatOptions.ReturnCommand;
                                    msg = msg.Replace("$awaytime", s);
                                    if (iceChatOptions.SendAwayCommands == true)
                                        ParseOutGoingCommand(connection, msg);                                    
                                        
                                }
                                else
                                {
                                    connection.ServerSetting.Away = true;
                                    connection.ServerSetting.DefaultNick = connection.ServerSetting.CurrentNickName;
                                    connection.ServerSetting.AwayStart = System.DateTime.Now;
                                    //ParseOutGoingCommand(connection, "/nick " + connection.ServerSetting.AwayNickName);
                                    SendData(connection, "NICK " + connection.ServerSetting.AwayNickName);
                                    string msg = iceChatOptions.AwayCommand;
                                    msg = msg.Replace("$awayreason", data);

                                    if (iceChatOptions.SendAwayCommands == true)
                                        ParseOutGoingCommand(connection, msg);                                    
                                }
                            }
                            break;
                        case "/ban":  // /ban #channel nick|address   /mode #channel +b host
                            if (connection != null && data.IndexOf(' ') > 0)
                            {
                                string channel = data.Split(' ')[0];
                                string host = data.Split(' ')[1];
                                ParseOutGoingCommand(connection, "/mode " + channel + " +b " + host);
                            }
                            break;
                        case "/browser":
                            if (data.Length > 0)
                            {
                                if (data.StartsWith("http"))
                                    System.Diagnostics.Process.Start(data);
                                else
                                    System.Diagnostics.Process.Start("http://" + data);
                            }
                            break;
                        case "/buddylist":
                        case "/notify":
                            //add a nickname to the buddy list
                            if (connection != null && data.Length > 0 && data.IndexOf(" ") == -1)
                            {
                                //check if the nickname is already in the buddy list
                                if (connection.ServerSetting.BuddyList != null)
                                {
                                    foreach (BuddyListItem buddy in connection.ServerSetting.BuddyList)
                                    {
                                        if (!buddy.Nick.StartsWith(";"))
                                            if (buddy.Nick.ToLower() == data.ToLower())
                                                return;
                                        else
                                            if (buddy.Nick.Substring(1).ToLower() == data.ToLower())
                                                return;
                                    }
                                }
                                //add in the new buddy list item
                                BuddyListItem b = new BuddyListItem();
                                b.Nick = data;

                                BuddyListItem[] buddies = connection.ServerSetting.BuddyList;
                                Array.Resize(ref buddies, buddies.Length + 1);
                                buddies[buddies.Length - 1] = b;

                                connection.ServerSetting.BuddyList = buddies;

                                serverTree.SaveServers(serverTree.ServersCollection);

                                connection.BuddyListCheck();
                            }
                            break;
                        case "/chaninfo":
                            if (connection != null)
                            {
                                if (data.Length > 0)
                                {
                                    IceTabPage t = GetWindow(connection, data, IceTabPage.WindowType.Channel);
                                    if (t != null)
                                    {
                                        FormChannelInfo fci = new FormChannelInfo(t);
                                        SendData(connection, "MODE " + t.TabCaption + " +b");
                                        //check if mode (e) exists for Exception List
                                        if (connection.ServerSetting.ChannelModeParam.Contains("e"))
                                            SendData(connection, "MODE " + t.TabCaption + " +e");
                                        SendData(connection, "TOPIC :" + t.TabCaption);
                                        fci.Show(this);
                                    }
                                }
                                else
                                {
                                    //check if current window is channel
                                    if (CurrentWindowType == IceTabPage.WindowType.Channel)
                                    {
                                        FormChannelInfo fci = new FormChannelInfo(CurrentWindow);
                                        SendData(connection, "MODE " + CurrentWindow.TabCaption + " +b");
                                        //check if mode (e) exists for Exception List
                                        if (connection.ServerSetting.ChannelModeParam.Contains("e"))
                                            SendData(connection, "MODE " + CurrentWindow.TabCaption + " +e");
                                        SendData(connection, "TOPIC :" + CurrentWindow.TabCaption);
                                        fci.Show(this);
                                    }
                                }
                            }
                            break;
                        case "/clear":
                            if (data.Length == 0)
                            {
                                if (CurrentWindowType != IceTabPage.WindowType.Console)
                                {
                                    CurrentWindow.TextWindow.ClearTextWindow();
                                }
                                else
                                {
                                    //find the current console tab window
                                    mainChannelBar.GetTabPage("Console").CurrentConsoleWindow().ClearTextWindow();
                                }
                            }
                            else
                            {
                                //find a match
                                if (data == "Console")
                                {
                                    mainChannelBar.GetTabPage("Console").CurrentConsoleWindow().ClearTextWindow();
                                    return;
                                }
                                else if (data == "Debug")
                                {
                                    IceTabPage db = GetWindow(null, "Debug", IceTabPage.WindowType.Debug);
                                    if (db != null)
                                    {
                                        db.TextWindow.ClearTextWindow();
                                    }
                                }
                                else if (data.ToLower() == "all console")
                                {
                                    //clear all the console windows and channel/queries
                                    foreach (ConsoleTab c in mainChannelBar.GetTabPage("Console").ConsoleTab.TabPages)
                                        ((TextWindow)c.Controls[0]).ClearTextWindow();
                                }
                                IceTabPage t = GetWindow(connection, data, IceTabPage.WindowType.Channel);
                                if (t != null)
                                    t.TextWindow.ClearTextWindow();
                                else
                                {
                                    IceTabPage q = GetWindow(connection, data, IceTabPage.WindowType.Query);
                                    if (q != null)
                                    {
                                        q.TextWindow.ClearTextWindow();
                                        return;
                                    }
                                    IceTabPage dcc = GetWindow(connection, data, IceTabPage.WindowType.DCCChat);
                                    if (dcc != null)
                                    {
                                        dcc.TextWindow.ClearTextWindow();
                                        return;
                                    }
                                    IceTabPage win = GetWindow(null, data, IceTabPage.WindowType.Window);
                                    if (win != null)
                                    {
                                        win.TextWindow.ClearTextWindow();
                                        return;
                                    }
                                }
                            }
                            break;
                        case "/clearall":
                            //clear all the text windows
                            for (int i = mainChannelBar.TabPages.Count - 1; i >= 0; i--)
                            {
                                if (mainChannelBar.TabPages[i].WindowStyle == IceTabPage.WindowType.Channel || mainChannelBar.TabPages[i].WindowStyle == IceTabPage.WindowType.Query)
                                {
                                    mainChannelBar.TabPages[i].TextWindow.ClearTextWindow();
                                }
                                else if (mainChannelBar.TabPages[i].WindowStyle == IceTabPage.WindowType.Console)
                                {
                                    //clear all console windows
                                    foreach (ConsoleTab c in mainChannelBar.GetTabPage("Console").ConsoleTab.TabPages)
                                    {
                                        ((TextWindow)c.Controls[0]).ClearTextWindow();
                                    }
                                }
                            }
                            break;
                        case "/closeall":
                            for (int i = mainChannelBar.TabPages.Count - 1; i >= 0; i--)
                            {
                                if (mainChannelBar.TabPages[i].WindowStyle == IceTabPage.WindowType.Window)
                                {
                                    RemoveWindow(connection, mainChannelBar.TabPages[i].TabCaption, IceTabPage.WindowType.Window);
                                }
                            }                            
                            break;                        
                        case "/close":
                            if (connection != null && data.Length > 0)
                            {
                                //check if it is a channel list window
                                if (data == "Channels")
                                {
                                    IceTabPage c = GetWindow(connection, "", IceTabPage.WindowType.ChannelList);
                                    if (c != null)
                                        RemoveWindow(connection, "", IceTabPage.WindowType.ChannelList);
                                    return;
                                }
                                //check if it is a query window
                                IceTabPage q = GetWindow(connection, data, IceTabPage.WindowType.Query);
                                if (q != null)
                                {
                                    RemoveWindow(connection, q.TabCaption, IceTabPage.WindowType.Query);
                                    return;
                                }

                                //check if it is a dcc chat window
                                IceTabPage dcc = GetWindow(connection, data, IceTabPage.WindowType.DCCChat);
                                if (dcc != null)
                                    RemoveWindow(connection, dcc.TabCaption, IceTabPage.WindowType.DCCChat);
                            }
                            else if (connection != null)
                            {
                                //check if current window is channel/query/dcc chat
                                if (CurrentWindowType == IceTabPage.WindowType.Query)
                                    RemoveWindow(connection, CurrentWindow.TabCaption, CurrentWindow.WindowStyle);
                                else if (CurrentWindowType == IceTabPage.WindowType.DCCChat)
                                    RemoveWindow(connection, CurrentWindow.TabCaption, CurrentWindow.WindowStyle);
                                else if (CurrentWindowType == IceTabPage.WindowType.Channel)
                                {
                                    SendData(connection, "PART " + CurrentWindow.TabCaption);
                                    RemoveWindow(connection, CurrentWindow.TabCaption, CurrentWindow.WindowStyle);
                                }
                            }
                            else
                            {
                                //check if the current window is the debug window
                                if (CurrentWindowType == IceTabPage.WindowType.Debug)
                                    RemoveWindow(null, "Debug", IceTabPage.WindowType.Debug);
                                else if (data.Length == 0)
                                {
                                    if (CurrentWindowType == IceTabPage.WindowType.Window)
                                        RemoveWindow(null, CurrentWindow.TabCaption, CurrentWindow.WindowStyle);
                                }
                                else if (data == "Debug")
                                {
                                    RemoveWindow(null, "Debug", IceTabPage.WindowType.Debug);
                                }
                                else
                                {
                                    if (data.StartsWith("@"))
                                        RemoveWindow(null, data, IceTabPage.WindowType.Window);
                                }
                            }
                            break;
                        case "/closequery":
                            if (connection != null)
                            {
                                for (int i = mainChannelBar.TabPages.Count - 1; i >= 0; i--)
                                {
                                    if (mainChannelBar.TabPages[i].WindowStyle == IceTabPage.WindowType.Query)
                                    {
                                        if (mainChannelBar.TabPages[i].Connection == connection)
                                        {
                                            RemoveWindow(connection, mainChannelBar.TabPages[i].TabCaption, IceTabPage.WindowType.Query);
                                        }
                                    }
                                }
                            }
                            break;
                        case "/closeallquery":
                            if (connection != null)
                            {
                                for (int i = mainChannelBar.TabPages.Count - 1; i >= 0; i--)
                                {
                                    if (mainChannelBar.TabPages[i].WindowStyle == IceTabPage.WindowType.Query)
                                    {
                                        RemoveWindow(connection, mainChannelBar.TabPages[i].TabCaption, IceTabPage.WindowType.Query);
                                    }
                                }
                            }
                            break;
                        case "/ctcp":
                            if (connection != null && data.IndexOf(' ') > 0)
                            {
                                //ctcp nick ctcptype
                                string nick = data.Substring(0, data.IndexOf(' '));
                                //get the message
                                string ctcp = data.Substring(data.IndexOf(' ') + 1);

                                string msg = GetMessageFormat("Ctcp Send");
                                msg = msg.Replace("$nick", nick); ;
                                msg = msg.Replace("$ctcp", ctcp.ToUpper());
                                CurrentWindowMessage(connection, msg, 7, true);
                                if (ctcp.ToUpper() == "PING")
                                    SendData(connection, "PRIVMSG " + nick + " :" + ctcp.ToUpper() + " " + System.Environment.TickCount.ToString() + "");
                                else
                                    SendData(connection, "PRIVMSG " + nick + " " + ctcp.ToUpper() + "");
                            }
                            break;
                        case "/dcc":
                            if (connection != null && data.IndexOf(' ') > 0)
                            {
                                //get the type of dcc
                                string dccType = data.Substring(0, data.IndexOf(' ')).ToUpper();
                                //get who it is being sent to
                                string nick = data.Substring(data.IndexOf(' ') + 1);
                                
                                switch (dccType)
                                {
                                    case "CHAT":
                                        //start a dcc chat
                                        if (nick.IndexOf(' ') == -1)    //make sure no space in the nick name
                                        {
                                            //check if we already have a dcc chat open with this person
                                            if (!mainChannelBar.WindowExists(connection, nick, IceTabPage.WindowType.DCCChat))
                                            {
                                                //create a new window
                                                AddWindow(connection, nick, IceTabPage.WindowType.DCCChat);
                                                IceTabPage t = GetWindow(connection, nick, IceTabPage.WindowType.DCCChat);
                                                if (t != null)
                                                {
                                                    t.RequestDCCChat();
                                                    string msg = GetMessageFormat("DCC Chat Outgoing");
                                                    msg = msg.Replace("$nick", nick);
                                                    t.TextWindow.AppendText(msg, 1);
                                                    t.TextWindow.ScrollToBottom();
                                                }
                                            }
                                            else
                                            {
                                                mainChannelBar.SelectTab(GetWindow(connection, nick, IceTabPage.WindowType.DCCChat));
                                                serverTree.SelectTab(mainChannelBar.CurrentTab, false);

                                                //see if it is connected or not
                                                IceTabPage dcc = GetWindow(connection, nick, IceTabPage.WindowType.DCCChat);
                                                if (dcc != null)
                                                {
                                                    if (!dcc.IsConnected)
                                                    {
                                                        dcc.RequestDCCChat();
                                                        string msg = GetMessageFormat("DCC Chat Outgoing");
                                                        msg = msg.Replace("$nick", nick);
                                                        dcc.TextWindow.AppendText(msg, 1);
                                                        dcc.TextWindow.ScrollToBottom();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case "SEND":
                                        //was a filename specified, if not try and select one
                                        string file;
                                        if (nick.IndexOf(' ') > 0)
                                        {
                                            file = nick.Substring(nick.IndexOf(' ') + 1);
                                            nick = nick.Substring(0,nick.IndexOf(' '));
                                            
                                            //see if the file exists
                                            if (!File.Exists(file))
                                            {
                                                //file does not exists, just quit
                                                //try from the dccsend folder
                                                if (File.Exists(iceChatOptions.DCCSendFolder + Path.DirectorySeparatorChar + file))
                                                    file=iceChatOptions.DCCSendFolder + Path.DirectorySeparatorChar + file;
                                                else
                                                    return;
                                            }
                                        }
                                        else
                                        {
                                            //ask for a file name
                                            OpenFileDialog dialog = new OpenFileDialog();
                                            dialog.InitialDirectory = iceChatOptions.DCCSendFolder;
                                            dialog.CheckFileExists = true;
                                            dialog.CheckPathExists = true;
                                            if (dialog.ShowDialog() == DialogResult.OK)
                                            {
                                                //returns the full path
                                                System.Diagnostics.Debug.WriteLine(dialog.FileName);
                                                file = dialog.FileName;
                                            }
                                            else
                                                return;

                                        }

                                        //more to it, maybe a file to send                                            
                                        if (!mainChannelBar.WindowExists(null, "DCC Files", IceTabPage.WindowType.DCCFile))
                                            AddWindow(null, "DCC Files", IceTabPage.WindowType.DCCFile);

                                        IceTabPage tt = GetWindow(null, "DCC Files", IceTabPage.WindowType.DCCFile);
                                        if (tt != null)
                                            ((IceTabPageDCCFile)tt).RequestDCCFile(connection, nick, file);                                        

                                        break;
                                }
                            }                            
                            break;
                        case "/describe":   //me command for a specific channel
                            if (connection != null && data.IndexOf(' ') > 0)
                            {
                                //get the channel name
                                string channel = data.Substring(0, data.IndexOf(' '));
                                //get the message
                                string message = data.Substring(data.IndexOf(' ') + 1);
                                //check for the channel
                                IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
                                if (t != null)
                                {
                                    SendData(connection, "PRIVMSG " + t.TabCaption + " :ACTION " + message + "");
                                    string msg = GetMessageFormat("Self Channel Action");
                                    msg = msg.Replace("$nick", inputPanel.CurrentConnection.ServerSetting.CurrentNickName).Replace("$channel", t.TabCaption);
                                    msg = msg.Replace("$message", message);
                                    t.TextWindow.AppendText(msg, 1);
                                    t.TextWindow.ScrollToBottom();
                                    t.LastMessageType = ServerMessageType.Action;
                                }
                            }
                            break;
                        case "/dns":
                            if (data.Length > 0)
                            {
                                if (data.IndexOf(".") > 0)
                                {
                                    //dns a host
                                    try
                                    {                                        
                                        args.Extra = data;
                                        foreach (IPluginIceChat ipc in loadedPlugins)
                                        {
                                            if (ipc.Enabled == true)
                                                args = ipc.DNSResolve(args);
                                        }
                                        if (args.Extra.Length > 0)
                                        {

                                            System.Net.IPAddress[] addresslist = System.Net.Dns.GetHostAddresses(data);
                                            ParseOutGoingCommand(connection, "/echo " + data + " resolved to " + addresslist.Length + " address(es)");
                                            
                                            foreach (System.Net.IPAddress address in addresslist)
                                                ParseOutGoingCommand(connection, "/echo -> " + address.ToString());
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        ParseOutGoingCommand(connection, "/echo " + data + " does not resolve (unknown address)");
                                    }
                                }
                                else
                                {
                                    //dns a nickname (send a userhost)
                                    SendData(connection, "USERHOST " + data);
                                }
                            }
                            break;
                        case "/echo":
                            //currently just echo to the current window
                            //need to check
                            if (data.Length > 0)
                            {
                                if (CurrentWindowType == IceTabPage.WindowType.Channel || CurrentWindowType == IceTabPage.WindowType.Query)
                                {
                                    string msg = GetMessageFormat("User Echo");
                                    msg = msg.Replace("$message","\x000F" + data);

                                    CurrentWindow.TextWindow.AppendText(msg, 1);
                                }
                                else if (CurrentWindowType == IceTabPage.WindowType.Console)
                                {
                                    string msg = GetMessageFormat("User Echo");
                                    msg = msg.Replace("$message", "\x000F" + data);

                                    mainChannelBar.GetTabPage("Console").CurrentConsoleWindow().AppendText(msg, 1);
                                }
                                else if (CurrentWindowType == IceTabPage.WindowType.DCCChat)
                                {
                                    string msg = GetMessageFormat("User Echo");
                                    msg = msg.Replace("$message", "\x000F" + data);
                                    
                                    CurrentWindow.TextWindow.AppendText(msg, 1);
                                }
                                else if (CurrentWindowType == IceTabPage.WindowType.Window)
                                {
                                    string msg = GetMessageFormat("User Echo");
                                    msg = msg.Replace("$message", "\x000F" + data);

                                    CurrentWindow.TextWindow.AppendText(msg, 1);
                                }
                            }
                            break;
                        case "/flash":
                            //used to flash a specific channel or query
                            if (connection != null && data.Length > 0)
                            {
                                string window = data;
                                bool flashWindow = true;
                                if (data.IndexOf(" ") > 0)
                                {
                                    window = data.Substring(0, data.IndexOf(' '));
                                    string t = data.Substring(data.IndexOf(' ') + 1);
                                    if (t.ToLower() == "off")
                                        flashWindow = false;
                                }
                                
                                //check if it is a channel window
                                IceTabPage c = GetWindow(connection, window, IceTabPage.WindowType.Channel);
                                if (c != null)
                                {
                                    c.FlashTab = flashWindow;                                    
                                    mainChannelBar.Invalidate();
                                    serverTree.Invalidate();
                                }
                                else
                                {
                                    //check if it is a query
                                    IceTabPage q = GetWindow(connection, window, IceTabPage.WindowType.Query);
                                    if (q != null)
                                    {
                                        q.FlashTab = flashWindow;
                                        mainChannelBar.Invalidate();
                                        serverTree.Invalidate();
                                    }
                                }
                            
                            }
                            break;                        
                        case "/flashtask":
                        case "/flashtaskbar":
                            FlashTaskBar();
                            break;
                        
                        case "/flashtray":
                            //check if we are minimized                            
                            if (this.notifyIcon.Visible == true)
                            {
                                this.flashTrayIconTimer.Enabled = true;
                                this.flashTrayIconTimer.Start();
                                //show a message in a balloon
                                if (data.Length > 0)
                                {
                                    this.notifyIcon.BalloonTipTitle = "PonyChat Client";
                                    this.notifyIcon.BalloonTipText = data;
                                    this.notifyIcon.ShowBalloonTip(1000);
                                }
                            }
                            break;
                        
                        case "/sound":
                            //change the font of the current window
                            //check if data is a channel
                            if (connection != null && data.Length > 0)
                            {
                                if (data.IndexOf(' ') == -1)
                                {
                                    IceTabPage t = GetWindow(connection, data, IceTabPage.WindowType.Channel);
                                    if (t != null)
                                    {
                                        t.DisableSounds = !t.DisableSounds;                                                                           
                                    }
                                }
                            }
                            break;
                        case "/font":
                            //change the font of the current window
                            //check if data is a channel
                            if (connection != null && data.Length > 0)
                            {
                                if (data.IndexOf(' ') == -1)
                                {
                                    IceTabPage t = GetWindow(connection, data, IceTabPage.WindowType.Channel);
                                    if (t != null)
                                    {
                                        //bring up a font dialog
                                        FontDialog fd = new FontDialog();
                                        //load the current font
                                        fd.Font = t.TextWindow.Font;
                                        
                                        fd.ShowEffects = false;
                                        fd.ShowColor = false;
                                        fd.FontMustExist = true;
                                        fd.AllowVectorFonts = false;
                                        fd.AllowVerticalFonts = false;
                                                                                
                                        if (fd.ShowDialog() != DialogResult.Cancel && fd.Font.Style == FontStyle.Regular)
                                        {
                                            t.TextWindow.Font = fd.Font;
                                        }
                                        else
                                        {
                                            if (fd.Font.Style != FontStyle.Regular)
                                            {
                                                MessageBox.Show("IceChat only supports 'Regular' font styles", "Font Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "/forcequit":
                            if (connection != null)
                            {
                                connection.AttemptReconnect = false;
                                connection.ForceDisconnect();
                            }
                            break;
                        case "/google":
                            if (data.Length > 0)
                                System.Diagnostics.Process.Start("http://www.google.com/search?q=" + data);
                            else
                                System.Diagnostics.Process.Start("http://www.google.com");
                            break;
                        case "/hop":
                            if (connection != null && data.Length == 0)
                            {
                                if (CurrentWindowType == IceTabPage.WindowType.Channel)
                                {
                                    CurrentWindow.ChannelHop = true;
                                    temp = CurrentWindow.TabCaption;
                                    SendData(connection, "PART " + temp);
                                    ParseOutGoingCommand(connection, "/timer joinhop 1 1 /join " + temp);
                                }
                            }
                            else
                            {
                                IceTabPage t = GetWindow(connection, data, IceTabPage.WindowType.Channel);
                                if (t != null)
                                {
                                    t.ChannelHop = true;
                                    SendData(connection, "PART " + t.TabCaption);
                                    ParseOutGoingCommand(connection, "/timer joinhop 1 1 /join " + t.TabCaption);
                                }
                            }
                            break;
                        case "/icechat":
                            if (connection != null)
                                ParseOutGoingCommand(connection, "/me is using " + ProgramID + " " + VersionID);
                            else
                                ParseOutGoingCommand(connection, "/echo using " + ProgramID + " " + VersionID);
                            break;
                        case "/icepath":
                            //To get current Folder and paste it into /me
                            if (connection != null)
                                ParseOutGoingCommand(connection, "/me Build Path = " + Directory.GetCurrentDirectory());
                            else
                                ParseOutGoingCommand(connection, "/echo Build Path = " + Directory.GetCurrentDirectory());
                            break;
                        case "/ignore":
                            if (connection != null)
                            {
                                if (data.Length > 0)
                                {
                                    //check if just a nick/host , no extra params
                                    if (data.IndexOf(" ") == -1)
                                    {
                                        //check if already in ignore list or not
                                        for (int i = 0; i < connection.ServerSetting.IgnoreList.Length;i++ )
                                        {
                                            string checkNick = connection.ServerSetting.IgnoreList[i];
                                            if (connection.ServerSetting.IgnoreList[i].StartsWith(";"))
                                                checkNick = checkNick.Substring(1);
                                            
                                            if (checkNick.ToLower() == data.ToLower())
                                            {
                                                if (connection.ServerSetting.IgnoreList[i].StartsWith(";"))
                                                    connection.ServerSetting.IgnoreList[i] = checkNick;
                                                else
                                                    connection.ServerSetting.IgnoreList[i] = ";" + checkNick;

                                                
                                                serverTree.SaveServers(serverTree.ServersCollection);
                                                return;
                                            }
                                        }

                                        //no match found, add the new item to the IgnoreList
                                        string[] ignores = connection.ServerSetting.IgnoreList;
                                        Array.Resize(ref ignores, ignores.Length + 1);
                                        ignores[ignores.Length - 1] = data;

                                        connection.ServerSetting.IgnoreList = ignores;

                                        serverTree.SaveServers(serverTree.ServersCollection);
                                    }
                                }
                            }
                            break;
                        case "/join":
                            if (connection != null && data.Length > 0)
                            {
                                if (connection.ServerSetting.ChannelTypes != null && Array.IndexOf(connection.ServerSetting.ChannelTypes, data[0]) == -1)
                                {
                                    data = connection.ServerSetting.ChannelTypes[0] + data;
                                }
                                SendData(connection, "JOIN " + data);
                            }
                            break;
                        case "/kick":
                            if (connection != null && data.Length > 0)
                            {
                                //kick #channel nick reason
                                if (data.IndexOf(' ') > 0)
                                {
                                    //get the channel
                                    temp = data.Substring(0, data.IndexOf(' '));
                                    //check if temp is a channel or not
                                    if (Array.IndexOf(connection.ServerSetting.ChannelTypes, temp[0]) == -1)
                                    {
                                        //temp is not a channel, substitute with current channel
                                        //make sure we are in a channel
                                        if (CurrentWindow.WindowStyle == IceTabPage.WindowType.Channel)
                                        {
                                            temp = CurrentWindow.TabCaption;
                                            if (data.IndexOf(' ') > 0)
                                            {
                                                //there is a kick reason
                                                string msg = data.Substring(data.IndexOf(' ') + 1);
                                                data = data.Substring(0, data.IndexOf(' '));
                                                SendData(connection, "KICK " + temp + " " + data + " :" + msg);
                                            }
                                            else
                                            {
                                                SendData(connection, "KICK " + temp + " " + data);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        data = data.Substring(temp.Length + 1);
                                        if (data.IndexOf(' ') > 0)
                                        {
                                            //there is a kick reason
                                            string msg = data.Substring(data.IndexOf(' ') + 1);
                                            data = data.Substring(0, data.IndexOf(' '));
                                            SendData(connection, "KICK " + temp + " " + data + " :" + msg);
                                        }
                                        else
                                        {
                                            SendData(connection, "KICK " + temp + " " + data);
                                        }
                                    }
                                }
                            }
                            break;
                        case "/me":
                            //check if in channel, query, etc
                            if (connection != null && data.Length > 0)
                            {
                                if (CurrentWindowType == IceTabPage.WindowType.Channel || CurrentWindowType == IceTabPage.WindowType.Query)
                                {
                                    SendData(connection, "PRIVMSG " + CurrentWindow.TabCaption + " :ACTION " + data + "");
                                    string msg = GetMessageFormat("Self Channel Action");
                                    msg = msg.Replace("$nick", inputPanel.CurrentConnection.ServerSetting.CurrentNickName).Replace("$channel", CurrentWindow.TabCaption);
                                    msg = msg.Replace("$message", data);

                                    CurrentWindow.TextWindow.AppendText(msg, 1);
                                    CurrentWindow.TextWindow.ScrollToBottom();
                                    CurrentWindow.LastMessageType = ServerMessageType.Action;
                                }
                                else if (CurrentWindowType == IceTabPage.WindowType.DCCChat)
                                {
                                    
                                    IceTabPage c = GetWindow(connection, CurrentWindow.TabCaption, IceTabPage.WindowType.DCCChat);
                                    if (c != null)
                                    {
                                        c.SendDCCData("ACTION " + data + "");
                                        
                                        string msg = GetMessageFormat("DCC Chat Action");
                                        msg = msg.Replace("$nick", inputPanel.CurrentConnection.ServerSetting.CurrentNickName);
                                        msg = msg.Replace("$message", data);

                                        CurrentWindow.TextWindow.AppendText(msg, 1);
                                        CurrentWindow.TextWindow.ScrollToBottom();
                                        CurrentWindow.LastMessageType = ServerMessageType.Action;

                                    }
                                }
                            }
                            break;
                        case "/umode":
                            if (connection != null && data.Length > 0)
                                SendData(connection, "MODE " + connection.ServerSetting.CurrentNickName + " " + data);
                            break;
                        case "/mode":
                            if (connection != null && data.Length > 0)
                                SendData(connection, "MODE " + data);
                            break;
                        case "/modex":
                            if (connection != null)
                                SendData(connection, "MODE " + connection.ServerSetting.CurrentNickName + " +x");
                            break;
                        case "/motd":
                            if (connection != null)
                            {
                                connection.ServerSetting.ForceMOTD = true;
                                SendData(connection, "MOTD");
                            }
                            break;
                        case "/msg":
                        case "/msgsec":
                            if (connection != null && data.IndexOf(' ') > -1)
                            {
                                string nick = data.Substring(0, data.IndexOf(' '));
                                string msg2 = data.Substring(data.IndexOf(' ') + 1);
                                if (nick.StartsWith("="))
                                {
                                    //send to a dcc chat window
                                    nick = nick.Substring(1);

                                    IceTabPage c = GetWindow(connection, nick, IceTabPage.WindowType.DCCChat);
                                    if (c != null)
                                    {
                                        c.SendDCCData(data);
                                        string msg = GetMessageFormat("Self DCC Chat Message");
                                        if (command.ToLower() == "/msgsec")
                                            msg = msg.Replace("$nick", c.Connection.ServerSetting.CurrentNickName).Replace("$message", "*");
                                        else
                                            msg = msg.Replace("$nick", c.Connection.ServerSetting.CurrentNickName).Replace("$message", data);

                                        c.TextWindow.AppendText(msg, 1);

                                    }
                                }
                                else
                                {
                                    SendData(connection, "PRIVMSG " + nick + " :" + msg2);

                                    //get the color for the private message
                                    string msg = GetMessageFormat("Self Channel Message");
                                    msg = msg.Replace("$nick", connection.ServerSetting.CurrentNickName).Replace("$channel", nick);

                                    //check if the nick has a query window open
                                    IceTabPage q = GetWindow(connection, nick, IceTabPage.WindowType.Query);
                                    if (q != null)
                                    {
                                        string nmsg = GetMessageFormat("Self Private Message");
                                        if (command.ToLower() == "/msgsec")
                                            nmsg = nmsg.Replace("$nick", connection.ServerSetting.CurrentNickName).Replace("$message", "*");
                                        else
                                            nmsg = nmsg.Replace("$nick", connection.ServerSetting.CurrentNickName).Replace("$message", msg2);
                                        
                                        q.TextWindow.AppendText(nmsg, 1);
                                        q.LastMessageType = ServerMessageType.Message;
                                        
                                    }
                                    else
                                    {
                                        IceTabPage t = GetWindow(connection, nick, IceTabPage.WindowType.Channel);
                                        if (t != null)
                                        {
                                            if (msg.Contains("$color"))
                                            {
                                                User u = t.GetNick(connection.ServerSetting.CurrentNickName);

                                                //get the nick color
                                                if (u.nickColor == -1)
                                                {
                                                    if (FormMain.Instance.IceChatColors.RandomizeNickColors == true)
                                                    {
                                                        int randColor = new Random().Next(0, 71);
                                                        if (randColor == FormMain.Instance.IceChatColors.NickListBackColor)
                                                            randColor = new Random().Next(0, 71);
                                                        u.nickColor = randColor;
                                                    }
                                                    else
                                                    {
                                                        //get the correct nickname color for channel status
                                                        for (int y = 0; y < u.Level.Length; y++)
                                                        {
                                                            if (u.Level[y])
                                                            {
                                                                switch (connection.ServerSetting.StatusModes[0][y])
                                                                {
                                                                    case 'q':
                                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelOwnerColor;
                                                                        break;
                                                                    case 'y':
                                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelOwnerColor;
                                                                        break;
                                                                    case 'a':
                                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelAdminColor;
                                                                        break;
                                                                    case 'o':
                                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelOpColor;
                                                                        break;
                                                                    case 'h':
                                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelHalfOpColor;
                                                                        break;
                                                                    case 'v':
                                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelVoiceColor;
                                                                        break;
                                                                    default:
                                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelRegularColor;
                                                                        break;
                                                                }
                                                                break;
                                                            }
                                                        }

                                                    }
                                                    if (u.nickColor == -1)
                                                        u.nickColor = FormMain.Instance.IceChatColors.ChannelRegularColor;
                                                }

                                                msg = msg.Replace("$color", "\x0003" + u.nickColor.ToString("00"));
                                            }
                                            msg = msg.Replace("$status", t.GetNick(connection.ServerSetting.CurrentNickName).ToString().Replace(connection.ServerSetting.CurrentNickName, ""));
                                            if (command.ToLower() == "/msgsec")
                                                msg = msg.Replace("$message", "*");
                                            else
                                                msg = msg.Replace("$message", msg2);

                                            t.TextWindow.AppendText(msg, 1);
                                            t.LastMessageType = ServerMessageType.Message;
                                        }
                                        else
                                        {
                                            //send to the current window
                                            if (msg.StartsWith("&#x3;"))
                                            {
                                                //get the color
                                                string color = msg.Substring(0, 6);
                                                int result;
                                                if (Int32.TryParse(msg.Substring(6, 1), out result))
                                                    color += msg.Substring(6, 1);
                                                if (command.ToLower() == "/msgsec")
                                                    msg = color + "*" + nick + "* *";
                                                else
                                                    msg = color + "*" + nick + "* " + data.Substring(data.IndexOf(' ') + 1);
                                            }
                                            CurrentWindowMessage(connection, msg, 4, true);
                                        }
                                    }
                                }
                            }
                            break;
                        case "/nick":
                            if (connection != null && data.Length > 0 && data.IndexOf(' ') == -1)
                            {
                                connection.SendData("NICK " + data);
                                if (connection.ServerSetting.NickName.CompareTo(data) != 0)
                                    connection.ServerSetting.NickName = data;
                            
                            }
                            break;
                        case "/notice":
                            if (connection != null && data.IndexOf(' ') > -1)
                            {
                                string nick = data.Substring(0, data.IndexOf(' '));
                                string msg = data.Substring(data.IndexOf(' ') + 1);
                                SendData(connection, "NOTICE " + nick + " :" + msg);

                                string nmsg = GetMessageFormat("Self Notice");
                                nmsg = nmsg.Replace("$nick", nick).Replace("$message", msg);
                                
                                CurrentWindowMessage(connection, nmsg, 1, true);
                            }
                            break;
                        case "/onotice":
                            if (connection != null && data.IndexOf(' ') > -1)
                            {
                                string nick = data.Substring(0, data.IndexOf(' '));
                                string msg = data.Substring(data.IndexOf(' ') + 1);
                                SendData(connection, "NOTICE @" + nick + " :" + msg);

                                string nmsg = GetMessageFormat("Self Notice");
                                nmsg = nmsg.Replace("$nick", nick).Replace("$message", msg);

                                CurrentWindowMessage(connection, nmsg, 1, true);
                            }
                            break;
                        case "/parse":
                            if (data.Length > 0)
                            {
                                string pattern2 = @"\[style.*\](.+?)\[/style\]";
                                Regex regex = new Regex(pattern2, RegexOptions.IgnoreCase);
                                //[br][Style ff:Tahoma;bgco:blue;co:blue;b;]___.[/style][Style ff:Tahoma;bgco:blue;co:yellow;b;]The Script－Testing Room[/style][Style ff:Tahoma;bgco:blue;co:blue;b;].___[/style][br][Style co:gold;b;]C[Style co:red;b;]RI[Style－co:blue;b;]m[Style co:green;b;]-[Style co:blue;b;]m[Style co:red;b;]IR[Style co:gold;b;]C [Style co:black;]http://crim-mirc.com[br][Style ff:Tahoma;bgco:red;co:red;b;]___.[/style][Style ff:Tahoma;bgco:red;co:white;b;]The Script Testing－Room[/style]
                                
                                MatchCollection m = regex.Matches(@data);
                                System.Diagnostics.Debug.WriteLine(m.Count);
                                string s = Regex.Replace(data, @"\[[^]]+\]", "");


                                string p = regex.Replace(data, "$1");

                                System.Diagnostics.Debug.WriteLine(p);
                                System.Diagnostics.Debug.WriteLine(s);


                            }
                            break;
                        case "/part":
                            if (connection != null && data.Length > 0)
                            {
                                //check if it is a query window
                                IceTabPage q = GetWindow(connection, data, IceTabPage.WindowType.Query);
                                if (q != null)
                                {
                                    RemoveWindow(connection, q.TabCaption, IceTabPage.WindowType.Query);
                                    return;
                                }
                                else if (CurrentWindowType == IceTabPage.WindowType.Query)
                                {
                                    RemoveWindow(connection, CurrentWindow.TabCaption, IceTabPage.WindowType.Query);
                                    return;
                                }
                                else if (CurrentWindowType == IceTabPage.WindowType.DCCChat)
                                {
                                    RemoveWindow(connection, CurrentWindow.TabCaption, IceTabPage.WindowType.DCCChat);
                                    return;
                                }
                                else if (CurrentWindowType == IceTabPage.WindowType.Window)
                                {
                                    RemoveWindow(null, CurrentWindow.TabCaption, IceTabPage.WindowType.Window);
                                    return;
                                }
                                //is there a part message
                                if (data.IndexOf(' ') > -1)
                                {
                                    //check if channel is a valid channel
                                    if (Array.IndexOf(connection.ServerSetting.ChannelTypes, data[0]) != -1)
                                    {
                                        SendData(connection, "PART " + data.Substring(0, data.IndexOf(' ')) + " :" + data.Substring(data.IndexOf(' ') + 1));
                                        RemoveWindow(connection, data.Substring(0, data.IndexOf(' ')), IceTabPage.WindowType.Channel);
                                    }
                                    else
                                    {
                                        //not a valid channel, use the current window
                                        if (CurrentWindowType == IceTabPage.WindowType.Channel)
                                        {
                                            SendData(connection, "PART " + CurrentWindow.TabCaption + " :" + data);
                                            RemoveWindow(connection, CurrentWindow.TabCaption, IceTabPage.WindowType.Channel);
                                        }
                                    }
                                }
                                else
                                {
                                    //see if data is a valid channel;
                                    if (Array.IndexOf(connection.ServerSetting.ChannelTypes, data[0]) != -1)
                                    {
                                        SendData(connection, "PART " + data);
                                        RemoveWindow(connection, data, IceTabPage.WindowType.Channel);
                                    }
                                    else
                                    {
                                        if (CurrentWindowType == IceTabPage.WindowType.Channel)
                                        {
                                            SendData(connection, "PART " + CurrentWindow.TabCaption + " :" + data);
                                            RemoveWindow(connection, CurrentWindow.TabCaption, IceTabPage.WindowType.Channel);
                                        }
                                    }
                                }
                            }
                            else if (connection != null)
                            {
                                //check if current window is channel
                                if (CurrentWindowType == IceTabPage.WindowType.Channel)
                                {
                                    SendData(connection, "PART " + CurrentWindow.TabCaption);
                                    RemoveWindow(connection, CurrentWindow.TabCaption, IceTabPage.WindowType.Channel);
                                }
                                else if (CurrentWindowType == IceTabPage.WindowType.Query)
                                {
                                    RemoveWindow(connection, CurrentWindow.TabCaption, IceTabPage.WindowType.Query);
                                }
                                else if (CurrentWindowType == IceTabPage.WindowType.Window)
                                {
                                    RemoveWindow(null, CurrentWindow.TabCaption, IceTabPage.WindowType.Window);
                                }
                            }
                            break;
                        case "/partall":
                            if (connection != null)
                            {
                                for (int i = mainChannelBar.TabPages.Count - 1; i >= 0; i--)
                                {
                                    if (mainChannelBar.TabPages[i].WindowStyle == IceTabPage.WindowType.Channel)
                                    {
                                        if (mainChannelBar.TabPages[i].Connection == connection)
                                        {
                                            if (connection.IsConnected)
                                                SendData(connection, "PART " + mainChannelBar.TabPages[i].TabCaption);
                                            
                                            RemoveWindow(connection, mainChannelBar.TabPages[i].TabCaption, IceTabPage.WindowType.Channel);
                                        }
                                    }
                                }
                            }
                            break;
                        case "/spartall":
                            for (int i = mainChannelBar.TabPages.Count - 1; i >= 0; i--)
                            {
                                if (mainChannelBar.TabPages[i].WindowStyle == IceTabPage.WindowType.Channel)
                                {
                                    if (mainChannelBar.TabPages[i].Connection.IsConnected)
                                        SendData(mainChannelBar.TabPages[i].Connection, "PART " + mainChannelBar.TabPages[i].TabCaption);

                                    RemoveWindow(mainChannelBar.TabPages[i].Connection, mainChannelBar.TabPages[i].TabCaption, IceTabPage.WindowType.Channel);
                                }
                            }
                            break;                        
                        case "/ping":
                            if (connection != null && data.Length > 0 && data.IndexOf(' ') == -1)
                            {
                                //ctcp nick ping
                                string msg = GetMessageFormat("Ctcp Send");
                                msg = msg.Replace("$nick", data); ;
                                msg = msg.Replace("$ctcp", "PING");
                                CurrentWindowMessage(connection, msg, 7, true);
                                SendData(connection, "PRIVMSG " + data + " :PING " + System.Environment.TickCount.ToString() + "");
                            }                            
                            break;
                        case "/play":   //play a WAV sound or MP3
                            if (data.Length > 4 && (data.ToLower().EndsWith(".wav") || data.ToLower().EndsWith(".mp3")))
                            {
                                //check if the WAV file exists in the Sounds Folder                                
                                if (File.Exists(soundsFolder + System.IO.Path.DirectorySeparatorChar + data))
                                {
                                    try
                                    {
                                        if (StaticMethods.IsRunningOnMono())
                                        {
                                            player.SoundLocation = soundsFolder + System.IO.Path.DirectorySeparatorChar + data;
                                            player.Play();
                                        }
                                        else
                                        {
                                            mp3Player.Open(soundsFolder + System.IO.Path.DirectorySeparatorChar + data);
                                            mp3Player.Play();
                                        }
                                    }
                                    catch { }
                                }
                                //check if the entire path was passed for the sound file
                                else if (File.Exists(data))
                                {
                                    try
                                    {
                                        if (StaticMethods.IsRunningOnMono())
                                        {
                                            player.SoundLocation = @data;
                                            player.Play();
                                        }
                                        else
                                        {
                                            mp3Player.Open(@data);
                                            mp3Player.Play();
                                        }
                                    }
                                    catch { }
                                }
                            }
                            break;                        
                        case "/query":
                            if (connection != null && data.Length > 0)
                            {
                                string nick = "";
                                string msg = "";
                                
                                if (data.IndexOf(" ") > 0)
                                {
                                    //check if there is a message added
                                    nick = data.Substring(0, data.IndexOf(' '));
                                    msg = data.Substring(data.IndexOf(' ') + 1);
                                }
                                else
                                    nick = data;

                                if (!mainChannelBar.WindowExists(connection, nick, IceTabPage.WindowType.Query))
                                    AddWindow(connection, nick, IceTabPage.WindowType.Query);
                                
                                mainChannelBar.SelectTab(GetWindow(connection, nick, IceTabPage.WindowType.Query));
                                serverTree.SelectTab(mainChannelBar.CurrentTab, false);

                                if (msg.Length > 0)
                                {
                                    SendData(connection, "PRIVMSG " + nick + " :" + msg);

                                    string nmsg = GetMessageFormat("Self Private Message");
                                    nmsg = nmsg.Replace("$nick", inputPanel.CurrentConnection.ServerSetting.CurrentNickName).Replace("$message", msg);
                                    
                                    CurrentWindow.TextWindow.AppendText(nmsg, 1);
                                    CurrentWindow.LastMessageType = ServerMessageType.Message;
                                }
                            }
                            break;
                        case "/quit":
                            if (connection != null)
                            {
                                connection.AttemptReconnect = false;

                                if (data.Length > 0)
                                    SendData(connection, "QUIT :" + data);
                                else
                                    SendData(connection, "QUIT :" + ParseIdentifiers(connection, connection.ServerSetting.QuitMessage, ""));
                            }
                            break;
                        case "/aquit":
                        case "/quitall":
                            foreach (IRCConnection c in serverTree.ServerConnections.Values)
                            {
                                if (c.IsConnected)
                                {
                                    c.AttemptReconnect = false;

                                    if (data.Length > 0)
                                        SendData(c, "QUIT :" + data);
                                    else
                                        SendData(c, "QUIT :" + ParseIdentifiers(connection, c.ServerSetting.QuitMessage, ""));
                                }
                            }
                            break;
                        case "/redrawtree":
                            System.Diagnostics.Debug.WriteLine(mainChannelBar.CurrentTab.TabCaption);
                            this.serverTree.Invalidate();
                            break;
                        case "/run":
                            if (data.Length > 0)
                            {
                                if (data.IndexOf("'") == -1)
                                    System.Diagnostics.Process.Start(data);
                                else
                                {
                                    string cmd = data.Substring(0, data.IndexOf("'"));
                                    string arg = data.Substring(data.IndexOf("'") + 1);
                                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(cmd, arg);                                                                        
                                }
                            }
                            break;
                        case "/say":
                            if (connection != null && data.Length > 0)
                            {
                                if (CurrentWindowType == IceTabPage.WindowType.Channel)
                                {
                                    SendData(connection, "PRIVMSG " + CurrentWindow.TabCaption + " :" + data);

                                    string msg = GetMessageFormat("Self Channel Message");
                                    string nick = inputPanel.CurrentConnection.ServerSetting.CurrentNickName;

                                    msg = msg.Replace("$nick", nick).Replace("$channel", CurrentWindow.TabCaption);

                                    //assign $color to the nickname 
                                    if (msg.Contains("$color"))
                                    {
                                        User u = CurrentWindow.GetNick(nick);

                                                //get the nick color
                                        if (u.nickColor == -1)
                                        {
                                            if (FormMain.Instance.IceChatColors.RandomizeNickColors == true)
                                            {
                                                int randColor = new Random().Next(0, 71);
                                                if (randColor == FormMain.Instance.IceChatColors.NickListBackColor)
                                                    randColor = new Random().Next(0, 71);
                                                u.nickColor = randColor;
                                            }
                                            else
                                            {
                                                //get the correct nickname color for channel status
                                                for (int y = 0; y < u.Level.Length; y++)
                                                {
                                                    if (u.Level[y])
                                                    {
                                                        switch (connection.ServerSetting.StatusModes[0][y])
                                                        {
                                                            case 'q':
                                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelOwnerColor;
                                                                break;
                                                            case 'y':
                                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelOwnerColor;
                                                                break;
                                                            case 'a':
                                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelAdminColor;
                                                                break;
                                                            case 'o':
                                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelOpColor;
                                                                break;
                                                            case 'h':
                                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelHalfOpColor;
                                                                break;
                                                            case 'v':
                                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelVoiceColor;
                                                                break;
                                                            default:
                                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelRegularColor;
                                                                break;
                                                        }
                                                        break;
                                                    }
                                                }

                                            }
                                            if (u.nickColor == -1)
                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelRegularColor;

                                        }                                        
                                        msg = msg.Replace("$color", "\x0003" + u.nickColor.ToString("00"));
                                    }

                                    msg = msg.Replace("$status", CurrentWindow.GetNick(nick).ToString().Replace(nick, ""));
                                    msg = msg.Replace("$message", data);

                                    CurrentWindow.TextWindow.AppendText(msg, 1);
                                    CurrentWindow.LastMessageType = ServerMessageType.Message;
                                }
                                else if (CurrentWindowType == IceTabPage.WindowType.Query)
                                {
                                    SendData(connection, "PRIVMSG " + CurrentWindow.TabCaption + " :" + data);

                                    string msg = GetMessageFormat("Self Private Message");
                                    msg = msg.Replace("$nick", inputPanel.CurrentConnection.ServerSetting.CurrentNickName).Replace("$message", data);

                                    CurrentWindow.TextWindow.AppendText(msg, 1);
                                    CurrentWindow.LastMessageType = ServerMessageType.Message;
                                }
                                else if (CurrentWindowType == IceTabPage.WindowType.DCCChat)
                                {
                                    CurrentWindow.SendDCCData(data);

                                    string msg = GetMessageFormat("Self DCC Chat Message");
                                    msg = msg.Replace("$nick", inputPanel.CurrentConnection.ServerSetting.CurrentNickName).Replace("$message", data);
                                    CurrentWindow.TextWindow.AppendText(msg, 1);
                                }
                                else if (CurrentWindowType == IceTabPage.WindowType.Console)
                                {
                                    WindowMessage(connection, "Console", data, 4, true);
                                }
                            }
                            break;
                        case "/joinserv":       //joinserv irc.server.name #channel
                            if (data.Length > 0 && data.IndexOf(' ') > 0)
                            {
                                //check if default nick name has been set
                                if (iceChatOptions.DefaultNick == null || iceChatOptions.DefaultNick.Length == 0)
                                {
                                    CurrentWindowMessage(connection, "No Default Nick Name Assigned. Go to Server Settings and set one under the Default Server Settings section.",4 , false);
                                }
                                else
                                {
                                    ServerSetting s = new ServerSetting();
                                    //get the server name
                                    //if there is a port name. extract it
                                    string server = data.Substring(0,data.IndexOf(' '));
                                    string channel = data.Substring(data.IndexOf(' ')+1);
                                    if (server.Contains(":"))
                                    {
                                        s.ServerName = server.Substring(0, server.IndexOf(':'));
                                        s.ServerPort = server.Substring(server.IndexOf(':') + 1);
                                        if (s.ServerPort.IndexOf(' ') > 0)
                                        {
                                            s.ServerPort = s.ServerPort.Substring(0, s.ServerPort.IndexOf(' '));
                                        }
                                        //check for + in front of port, SSL Connection
                                        if (s.ServerPort.StartsWith("+"))
                                        {
                                            s.ServerPort = s.ServerPort.Substring(1);
                                            s.UseSSL = true;
                                        }
                                    }
                                    else
                                    {
                                        s.ServerName = server;
                                        s.ServerPort = "6667";
                                    }

                                    s.NickName = iceChatOptions.DefaultNick;
                                    s.AltNickName = iceChatOptions.DefaultNick + "_";
                                    s.AwayNickName = iceChatOptions.DefaultNick + "[A]";
                                    s.FullName = iceChatOptions.DefaultFullName;
                                    s.QuitMessage = iceChatOptions.DefaultQuitMessage;
                                    s.IdentName = iceChatOptions.DefaultIdent;
                                    s.IAL = new Hashtable();
                                    s.AutoJoinChannels = new string[] { channel };
                                    s.AutoJoinEnable = true;
                                    Random r = new Random();
                                    s.ID = r.Next(50000, 99999);
                                    NewServerConnection(s);
                                }
                            }                            
                            break;
                        case "/scid": //scid <ServerNumber/NetworkName> /command [parameters]
                            if (data.Length > 0 && data.IndexOf(' ') > -1)
                            {
                                string[] param = data.Split(new char[] {' '}, 2);
                                int result;
                                if (Int32.TryParse(param[0], out result))
                                {
                                    //result is the server id
                                    foreach (IRCConnection c in serverTree.ServerConnections.Values)
                                    {
                                        if (c.ServerSetting.ID == result)
                                        {
                                            ParseOutGoingCommand(c, param[1]);
                                        }
                                    }
                                }
                                else
                                {
                                    //check for a network name match
                                    foreach (IRCConnection c in serverTree.ServerConnections.Values)
                                    {
                                        if (c.ServerSetting.NetworkName.Equals(param[0], StringComparison.OrdinalIgnoreCase))
                                        {
                                            ParseOutGoingCommand(c, param[1]);
                                        }
                                    }
                                }
                            }
                            break;                        
                        case "/server":
                            if (data.Length > 0)
                            {
                                //check if default nick name has been set
                                if (iceChatOptions.DefaultNick == null || iceChatOptions.DefaultNick.Length == 0)
                                {
                                    CurrentWindowMessage(connection, "No Default Nick Name Assigned. Go to Server Settings and set one under the Default Server Settings section.", 4, false);
                                }
                                else if (data.StartsWith("id="))
                                {
                                    string serverID = data.Substring(3);
                                    foreach (ServerSetting s in serverTree.ServersCollection.listServers)
                                    {
                                        if (s.ID.ToString() == serverID)
                                        {
                                            NewServerConnection(s);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    ServerSetting s = new ServerSetting();
                                    //get the server name
                                    //if there is a port name. extract it
                                    if (data.Contains(" "))
                                    {
                                        if (data.StartsWith("-"))
                                        {
                                            //parameters
                                            // [-46epoc]
                                            string switches = data.Substring(0, data.IndexOf(' '));
                                            data = data.Substring(switches.Length + 1);

                                            if (switches.IndexOf('e') > -1)
                                            {
                                                //enable ssl
                                                s.UseSSL = true;
                                                s.SSLAcceptInvalidCertificate = true;
                                            }
                                        }
                                    }
                                    
                                    if (data.Contains(" "))
                                    {
                                        s.ServerName = data.Substring(0, data.IndexOf(' '));
                                        
                                        string sp = data.Substring(data.IndexOf(' ') + 1);
                                        //server address [port] [password]
                                        if (sp.IndexOf(' ') > 0)
                                        {
                                            s.ServerPort = sp.Substring(0, sp.IndexOf(' '));
                                            if (s.ServerPort.StartsWith("+"))
                                            {
                                                s.ServerPort = s.ServerPort.Substring(1);
                                                s.UseSSL = true;
                                                s.SSLAcceptInvalidCertificate = true;
                                            }
                                            s.Password = sp.Substring(sp.IndexOf(' ') + 1);
                                        }
                                        else
                                        {
                                            //no space, check if value is a number or not
                                            
                                            int result;
                                            if (int.TryParse(sp, out result))
                                            {
                                                s.ServerPort = sp;
                                            }
                                            else
                                            {
                                                //check for + in front of port, SSL Connection
                                                if (sp.StartsWith("+"))
                                                {
                                                    s.ServerPort = sp.Substring(1);
                                                    s.UseSSL = true;
                                                    s.SSLAcceptInvalidCertificate = true;
                                                }
                                                else
                                                {
                                                    s.ServerPort = "6667";
                                                    s.Password = sp;
                                                }
                                            }
                                        }
                                    }
                                    else if (data.Contains(":"))
                                    {
                                        s.ServerName = data.Substring(0, data.IndexOf(':'));
                                        s.ServerPort = data.Substring(data.IndexOf(':') + 1);
                                        if (s.ServerPort.IndexOf(' ') > 0)
                                        {
                                            s.ServerPort = s.ServerPort.Substring(0, s.ServerPort.IndexOf(' '));
                                        }
                                        //check for + in front of port, SSL Connection
                                        if (s.ServerPort.StartsWith("+"))
                                        {
                                            s.ServerPort = s.ServerPort.Substring(1);
                                            s.UseSSL = true;
                                            s.SSLAcceptInvalidCertificate = true;
                                        }
                                    }
                                    else
                                    {
                                        s.ServerName = data;
                                        s.ServerPort = "6667";
                                    }

                                    s.NickName = iceChatOptions.DefaultNick;
                                    s.AltNickName = iceChatOptions.DefaultNick + "_";
                                    s.AwayNickName = iceChatOptions.DefaultNick + "[A]";
                                    s.FullName = iceChatOptions.DefaultFullName;
                                    s.QuitMessage = iceChatOptions.DefaultQuitMessage;
                                    s.IdentName = iceChatOptions.DefaultIdent;
                                    s.IAL = new Hashtable();

                                    Random r = new Random();
                                    s.ID = r.Next(50000, 99999);
                                    NewServerConnection(s);
                                }
                            }
                            break;
                        case "/timers":
                            if (connection != null)
                            {
                                if (connection.IRCTimers.Count == 0)
                                    OnServerMessage(connection, "No Timers");
                                else
                                {
                                    foreach (IrcTimer timer in connection.IRCTimers)
                                        OnServerMessage(connection, "[ID=" + timer.TimerID + "] [Interval=" + timer.TimerInterval + "] [Reps=" + timer.TimerRepetitions + "] [Count=" + timer.TimerCounter + "] [Command=" + timer.TimerCommand + "]");
                                }
                            }
                            break;                        
                        case "/timer":
                            if (connection != null)
                            {
                                if (data.Length != 0)
                                {
                                    string[] param = data.Split(' ');
                                    if (param.Length == 2)
                                    {
                                        //check for /timer ID off
                                        if (param[1].ToLower() == "off")
                                        {
                                            connection.DestroyTimer(param[0]);
                                            break;
                                        }
                                    }
                                    else if (param.Length > 3)
                                    {
                                        // param[0] = TimerID
                                        // param[1] = Repetitions
                                        // param[2] = Interval
                                        // param[3+] = Command

                                        string timerCommand = String.Join(" ", param, 3, param.GetUpperBound(0) - 2);
                                        connection.CreateTimer(param[0], Convert.ToInt32(param[1]), Convert.ToDouble(param[2]), timerCommand);
                                    }
                                    else
                                    {
                                        string msg = GetMessageFormat("User Error");
                                        msg = msg.Replace("$message", "/timer [ID] [REPS] [INTERVAL] [COMMAND]");
                                        CurrentWindowMessage(connection, msg, 4, true);
                                    }
                                }
                                else
                                {
                                    string msg = GetMessageFormat("User Error");
                                    msg = msg.Replace("$message", "/timer [ID] [REPS] [INTERVAL] [COMMAND]");
                                    CurrentWindowMessage(connection, msg, 4, true);
                                }
                            }
                            break;
                        case "/topicbar":
                            if (connection != null)
                            {
                                if (data.Length > 0)
                                {
                                    if (data.IndexOf(' ') == -1)
                                    {
                                        //topicbar show //topicbar hide for current channel
                                        if (data.ToLower() == "show")
                                            CurrentWindow.ShowTopicBar = true;
                                        if (data.ToLower() == "hide")
                                            CurrentWindow.ShowTopicBar = false;
                                    }
                                    else
                                    {
                                        //topicbar #channel show
                                        string[] words = data.Split(' ');


                                    }
                                }                            
                            }                            
                            break;
                        case "/topic":
                            if (connection != null)
                            {
                                if (data.Length == 0)
                                {
                                    if (CurrentWindowType == IceTabPage.WindowType.Channel)
                                        SendData(connection, "TOPIC :" + CurrentWindow.TabCaption);
                                }
                                else
                                {
                                    //check if a channel name was passed                            
                                    string word = "";

                                    if (data.IndexOf(' ') > -1)
                                        word = data.Substring(0, data.IndexOf(' '));
                                    else
                                        word = data;

                                    if (Array.IndexOf(connection.ServerSetting.ChannelTypes, word[0]) != -1)
                                    {
                                        IceTabPage t = GetWindow(connection, word, IceTabPage.WindowType.Channel);
                                        if (t != null)
                                        {
                                            if (data.IndexOf(' ') > -1)
                                                SendData(connection, "TOPIC " + t.TabCaption + " :" + data.Substring(data.IndexOf(' ') + 1));
                                            else
                                                SendData(connection, "TOPIC :" + t.TabCaption);
                                        }
                                    }
                                    else
                                    {
                                        if (CurrentWindowType == IceTabPage.WindowType.Channel)
                                            SendData(connection, "TOPIC " + CurrentWindow.TabCaption + " :" + data);
                                    }
                                }
                            }
                            break;
                        case "/update":
                            checkForUpdate();
                            break;
                        case "/userinfo":
                            if (connection != null && data.Length > 0)
                            {
                                FormUserInfo fui = new FormUserInfo(connection);
                                //find the user
                                fui.NickName(data);
                                fui.Show(this);
                            }
                            break;
                        case "/version":
                            if (connection != null && data.Length > 0)
                            {
                                string msg = GetMessageFormat("Ctcp Send");
                                msg = msg.Replace("$nick", data); ;
                                msg = msg.Replace("$ctcp", "VERSION");
                                CurrentWindowMessage(connection, msg, 7, true);
                                SendData(connection, "PRIVMSG " + data + " VERSION");
                            }
                            else
                                SendData(connection, "VERSION");
                            break;
                        case "/who":
                            if (connection != null && data.Length > 0)
                                SendData(connection, "WHO " + data);
                            break;
                        case "/whois":
                            if (connection != null && data.Length > 0)
                                SendData(connection, "WHOIS " + data);
                            break;
                        case "/aline":  //for adding lines to @windows
                            if (data.Length > 0 && data.IndexOf(" ") > -1)
                            {
                                string window = data.Substring(0, data.IndexOf(' '));
                                string msg = data.Substring(data.IndexOf(' ') + 1);
                                if (GetWindow(null, window, IceTabPage.WindowType.Window) == null)
                                    AddWindow(null, window, IceTabPage.WindowType.Window);

                                IceTabPage t = GetWindow(null, window, IceTabPage.WindowType.Window);
                                if (t != null)
                                {
                                    t.TextWindow.AppendText(msg, 1);
                                    t.LastMessageType = ServerMessageType.Message;
                                }
                            }
                            break;
                        case "/window":
                            if (data.Length > 0)
                            {
                                if (data.StartsWith("@") && data.IndexOf(" ") == -1)
                                {
                                    if (GetWindow(null, data, IceTabPage.WindowType.Window) == null)
                                        AddWindow(null, data, IceTabPage.WindowType.Window);
                                }
                            }
                            break;
                        case "/quote":
                        case "/raw":
                            if (connection != null && connection.IsConnected)
                                connection.SendData(data);
                            break;
                        default:
                            //parse the outgoing data
                            if (connection != null)
                                SendData(connection, command.Substring(1) + " " + data);
                            break;
                    }
                }
                else
                {
                    //sends a message to the channel
                    error = 1;
                    if (inputPanel.CurrentConnection != null && connection != null)
                    {
                        if (inputPanel.CurrentConnection.IsConnected)
                        {
                            error = 2;
                            //check if the current window is a Channel/Query, etc
                            if (CurrentWindowType == IceTabPage.WindowType.Channel)
                            {
                                SendData(connection, "PRIVMSG " + CurrentWindow.TabCaption + " :" + data);
                                //check if we got kicked out of the channel or not, and the window is still open
                                if (CurrentWindow.IsFullyJoined)
                                {
                                    error = 3;
                                    string msg = GetMessageFormat("Self Channel Message");
                                    string nick = inputPanel.CurrentConnection.ServerSetting.CurrentNickName;
                                    msg = msg.Replace("$nick", nick).Replace("$channel", CurrentWindow.TabCaption);
                                    error = 4;
                                    //assign $color to the nickname 
                                    if (msg.Contains("$color"))
                                    {
                                        error = 5;
                                        User u = CurrentWindow.GetNick(nick);

                                        //get the nick color
                                        if (u.nickColor == -1)
                                        {
                                            error = 6;
                                            if (FormMain.Instance.IceChatColors.RandomizeNickColors == true)
                                            {
                                                int randColor = new Random().Next(0, 71);
                                                if (randColor == FormMain.Instance.IceChatColors.NickListBackColor)
                                                    randColor = new Random().Next(0, 71);
                                                u.nickColor = randColor;
                                                error = 7;
                                            }
                                            else
                                            {
                                                //get the correct nickname color for channel status
                                                error = 8;
                                                for (int y = 0; y < u.Level.Length; y++)
                                                {
                                                    if (u.Level[y])
                                                    {
                                                        switch (connection.ServerSetting.StatusModes[0][y])
                                                        {
                                                            case 'q':
                                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelOwnerColor;
                                                                break;
                                                            case 'y':
                                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelOwnerColor;
                                                                break;
                                                            case 'a':
                                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelAdminColor;
                                                                break;
                                                            case 'o':
                                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelOpColor;
                                                                break;
                                                            case 'h':
                                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelHalfOpColor;
                                                                break;
                                                            case 'v':
                                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelVoiceColor;
                                                                break;
                                                            default:
                                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelRegularColor;
                                                                break;
                                                        }
                                                        break;
                                                    }
                                                }
                                                error = 9;
                                            }
                                            if (u.nickColor == -1)
                                                u.nickColor = FormMain.Instance.IceChatColors.ChannelRegularColor;
                                            error = 10;
                                        }

                                        msg = msg.Replace("$color", "\x0003" + u.nickColor.ToString("00"));
                                        error = 11;
                                    }
                                    error = 12;
                                    msg = msg.Replace("$status", CurrentWindow.GetNick(nick).ToString().Replace(nick, ""));
                                    error = 13;
                                    msg = msg.Replace("$message", data);
                                    error = 14;
                                    CurrentWindow.TextWindow.AppendText(msg, 1);
                                    CurrentWindow.LastMessageType = ServerMessageType.Message;
                                    error = 15;
                                }
                            }
                            else if (CurrentWindowType == IceTabPage.WindowType.Query)
                            {
                                SendData(connection, "PRIVMSG " + CurrentWindow.TabCaption + " :" + data);

                                string msg = GetMessageFormat("Self Private Message");
                                msg = msg.Replace("$nick", inputPanel.CurrentConnection.ServerSetting.CurrentNickName).Replace("$message", data);

                                CurrentWindow.TextWindow.AppendText(msg, 1);
                                CurrentWindow.LastMessageType = ServerMessageType.Message;
                            }
                            else if (CurrentWindowType == IceTabPage.WindowType.DCCChat)
                            {
                                CurrentWindow.SendDCCData(data);

                                string msg = GetMessageFormat("Self DCC Chat Message");
                                msg = msg.Replace("$nick", inputPanel.CurrentConnection.ServerSetting.CurrentNickName).Replace("$message", data);

                                CurrentWindow.TextWindow.AppendText(msg, 1);
                            }
                            else if (CurrentWindowType == IceTabPage.WindowType.Console)
                            {
                                WindowMessage(connection, "Console", data, 4, true);
                            }
                        }
                        else
                        {
                            WindowMessage(connection, "Console", "Error: Not Connected", 4, true);
                            WindowMessage(connection, "Console", data, 4, true);
                        }
                    }
                    else
                    {
                        if (CurrentWindowType == IceTabPage.WindowType.Window)
                            CurrentWindow.TextWindow.AppendText(data, 1);
                        else                        
                            WindowMessage(null, "Console", data, 4, true);
                    }
                }
            }
            catch (Exception e)
            {
                WriteErrorFile(connection, "ParseOutGoingCommand:" + CurrentWindowType.ToString() + ":" + error + ":" + data, e);
            }
        }
        /// <summary>
        /// Input Panel Text Box had Entered Key Pressed or Send Button Pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void inputPanel_OnCommand(object sender, string data)
        {
            if (data.Length > 0)
            {                
                ParseOutGoingCommand(inputPanel.CurrentConnection, data);
                if (CurrentWindowType == IceTabPage.WindowType.Console)
                    mainChannelBar.CurrentTab.CurrentConsoleWindow().ScrollToBottom();
                else if (CurrentWindowType != IceTabPage.WindowType.DCCFile && CurrentWindowType != IceTabPage.WindowType.ChannelList)
                    CurrentWindow.TextWindow.ScrollToBottom();
            
            
                //auto away settings
                if (inputPanel.CurrentConnection != null)
                {
                    if (data.StartsWith("/") == false)
                    {
                        if (iceChatOptions.AutoReturn == true)
                        {
                            if (inputPanel.CurrentConnection.ServerSetting.Away == true)
                            {
                                //return yourself
                                ParseOutGoingCommand(inputPanel.CurrentConnection, "/away");
                            }
                        }
                    }

                    //reset the auto away timer
                    if (data.StartsWith("/") == false)
                    {
                        if (iceChatOptions.AutoAway == true)
                        {
                            inputPanel.CurrentConnection.SetAutoAwayTimer(iceChatOptions.AutoAwayTime);
                        }
                    }
                }
            }
        }

        private void AddInputPanelText(string data)
        {
            if (this.InvokeRequired)
            {
                AddInputpanelTextDelegate a = new AddInputpanelTextDelegate(AddInputPanelText);
                this.Invoke(a, new object[] { data });
            }
            else
            {
                inputPanel.AppendText(data);
                FocusInputBox();
            }

        }

        #endregion

        /// <summary>
        /// Get the Host from the Full User Host, including Ident
        /// </summary>
        /// <param name="host">Full User Host (user!ident@host)</param>
        /// <returns></returns>
        private string HostFromFullHost(string host)
        {
            if (host.IndexOf("!") > -1)
                return host.Substring(host.LastIndexOf("!") + 1);
            else
                return host;
        }

        /// <summary>
        /// Return the Nick Name from the Full User Host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private string NickFromFullHost(string host)
        {
            if (host.StartsWith(":"))
                host = host.Substring(1);

            if (host.IndexOf("!") > 0)
                return host.Substring(0, host.LastIndexOf("!"));
            else
                return host;
        }

        private string OnParseIdentifier(IRCConnection connection, string message)
        {
            return ParseIdentifiers(connection, message, message);
        }

        private string ParseIdentifier(IRCConnection connection, string data)
        {
            //match all words starting with a $
            try
            {
                string identMatch = "\\$\\b[a-zA-Z_0-9.]+\\b";
                Regex ParseIdent = new Regex(identMatch);
                Match m = ParseIdent.Match(data);

                while (m.Success)
                {
                    switch (m.Value)
                    {
                        case "$me":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.CurrentNickName);
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$cme":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.NickName);
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$altnick":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.AltNickName);
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$ident":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.IdentName);
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$host":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.LocalHost);
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$fullhost":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.CurrentNickName + "!" + connection.ServerSetting.LocalHost);
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$fullname":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.FullName);
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$ip":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.LocalIP.ToString());
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$network":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.NetworkName);
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$port":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.ServerPort);
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$encoding":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.Encoding);
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$quitmessage":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.QuitMessage);
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$servermode":
                            data = ReplaceFirst(data, m.Value, string.Empty);
                            break;
                        case "$serverid":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.ID.ToString());
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$server":
                            if (connection != null)
                            {
                                if (connection.ServerSetting.RealServerName.Length > 0)
                                    data = ReplaceFirst(data, m.Value, connection.ServerSetting.RealServerName);
                                else
                                    data = ReplaceFirst(data, m.Value, connection.ServerSetting.ServerName);
                            }
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$online":
                            if (connection != null)
                            {
                                //check the datediff
                                TimeSpan online = DateTime.Now.Subtract(connection.ServerSetting.ConnectedTime);
                                data = ReplaceFirst(data, m.Value, GetDuration((int)online.TotalSeconds));
                            }
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$serverip":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.ServerIP);
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;
                        case "$localip":
                            if (connection != null)
                                data = ReplaceFirst(data, m.Value, connection.ServerSetting.LocalIP.ToString());
                            else
                                data = ReplaceFirst(data, m.Value, "$null");
                            break;

                        //identifiers that do not require a connection                                
                        case "$theme":
                            data = ReplaceFirst(data, m.Value, iceChatOptions.CurrentTheme);
                            break;
                        case "$colors":
                            data = ReplaceFirst(data, m.Value, colorsFile);
                            break;
                        case "$appdata":
                            data = ReplaceFirst(data, m.Value, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToString());
                            break;
                        case "$ossp":
                            data = ReplaceFirst(data, m.Value, Environment.OSVersion.ServicePack.ToString());
                            break;
                        case "$osbuild":
                            data = ReplaceFirst(data, m.Value, Environment.OSVersion.Version.Build.ToString());
                            break;
                        case "$osplatform":
                            data = ReplaceFirst(data, m.Value, Environment.OSVersion.Platform.ToString());
                            break;
                        case "$osbits":
                            //8 on 64bit -- AMD64
                            string arch = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                            switch (arch)
                            {
                                case "x86":
                                    string arch2 = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432");
                                    if (arch2 == "AMD64")
                                        data = ReplaceFirst(data, m.Value, "64bit");
                                    else
                                        data = ReplaceFirst(data, m.Value, "32bit");
                                    break;
                                case "AMD64":
                                case "IA64":
                                    data = ReplaceFirst(data, m.Value, "64bit");
                                    break;

                            }
                            break;
                        case "$os":
                            data = ReplaceFirst(data, m.Value, GetOperatingSystemName());
                            break;
                        case "$time":
                            data = ReplaceFirst(data, m.Value, DateTime.Now.ToShortTimeString());
                            break;
                        case "$date":
                            data = ReplaceFirst(data, m.Value, DateTime.Now.ToShortDateString());
                            break;
                        case "$icepath":
                        case "$icechatexedir":
                            data = ReplaceFirst(data, m.Value, Directory.GetCurrentDirectory());
                            break;
                        case "$plugindir":
                            data = ReplaceFirst(data, m.Value, pluginsFolder);
                            break;
                        case "$aliasfile":
                            data = ReplaceFirst(data, m.Value, aliasesFile);
                            break;
                        case "$serverfile":
                            data = ReplaceFirst(data, m.Value, serversFile);
                            break;
                        case "$popupfile":
                            data = ReplaceFirst(data, m.Value, popupsFile);
                            break;
                        case "$icechatver":
                            data = ReplaceFirst(data, m.Value, VersionID);
                            break;
                        case "$version":
                            data = ReplaceFirst(data, m.Value, ProgramID + " " + VersionID);
                            break;
                        case "$icechatdir":
                            data = ReplaceFirst(data, m.Value, currentFolder);
                            break;
                        case "$icechathandle":
                            data = ReplaceFirst(data, m.Value, this.Handle.ToString());
                            break;
                        case "$icechat":
                            data = ReplaceFirst(data, m.Value, ProgramID + " " + VersionID + " http://www.icechat.net");
                            break;
                        case "$logdir":
                            data = ReplaceFirst(data, m.Value, logsFolder);
                            break;
                        case "$randquit":
                            Random rand = new Random();
                            int rq = rand.Next(0, QuitMessages.RandomQuitMessages.Length);
                            data = ReplaceFirst(data, m.Value, QuitMessages.RandomQuitMessages[rq]);
                            break;
                        case "$randcolor":
                            Random randcolor = new Random();
                            int rc = randcolor.Next(0, (IrcColor.colors.Length - 1));
                            data = ReplaceFirst(data, m.Value, rc.ToString());
                            break;
                        case "$tickcount":
                        case "$ticks":
                            data = ReplaceFirst(data, m.Value, System.Environment.TickCount.ToString());
                            break;
                        case "$totalwindows":
                            data = ReplaceFirst(data, m.Value, mainChannelBar.TabCount.ToString());
                            break;
                        case "$totalscreens":
                            data = ReplaceFirst(data, m.Value, System.Windows.Forms.Screen.AllScreens.Length.ToString());
                            break;
                        case "$currentwindow":
                        case "$active":
                            data = ReplaceFirst(data, m.Value, CurrentWindow.TabCaption);
                            break;
                        case "$chanlogdir":
                            data = ReplaceFirst(data, m.Value, CurrentWindow.TextWindow.LogFileLocation);
                            break;                        
                        case "$totallines":
                            if (CurrentWindowType == IceTabPage.WindowType.Console)
                            {
                                data = ReplaceFirst(data, m.Value, ((TextWindow)mainChannelBar.GetTabPage("Console").ConsoleTab.SelectedTab.Controls[0]).TotalLines.ToString());
                            }
                            else
                            {
                                if (CurrentWindowType == IceTabPage.WindowType.Channel || CurrentWindowType == IceTabPage.WindowType.DCCChat || CurrentWindowType == IceTabPage.WindowType.Query || CurrentWindowType == IceTabPage.WindowType.Window)
                                {
                                    data = ReplaceFirst(data, m.Value, CurrentWindow.TextWindow.TotalLines.ToString());
                                }
                            }
                            break;
                        case "$framework":
                            data = ReplaceFirst(data, m.Value, System.Environment.Version.ToString());
                            break;
                        case "$totalplugins":
                            data = ReplaceFirst(data, m.Value, loadedPlugins.Count.ToString());
                            break;
                        case "$plugins":
                            string plugins = "";
                            foreach (IPluginIceChat ipc in loadedPlugins)
                            {
                                if (ipc.Enabled == true)
                                    plugins += ipc.Name + " : ";
                            }
                            if (plugins.EndsWith(" : "))
                                plugins = plugins.Substring(0, plugins.Length - 3);
                            data = ReplaceFirst(data, m.Value, plugins);
                            break;
                        case "$uptime2":
                            int systemUpTime = System.Environment.TickCount / 1000;
                            TimeSpan ts = TimeSpan.FromSeconds(systemUpTime);
                            data = ReplaceFirst(data, m.Value, GetDuration(ts.TotalSeconds));
                            break;
                        case "$uptime":
                            System.Diagnostics.PerformanceCounter pc = new System.Diagnostics.PerformanceCounter("System", "System Up Time");
                            pc.NextValue();
                            TimeSpan ts2 = TimeSpan.FromSeconds(pc.NextValue());
                            data = ReplaceFirst(data, m.Value, GetDuration(ts2.TotalSeconds));
                            break;
                        case "$mono":
                            if (StaticMethods.IsRunningOnMono())
                                data = ReplaceFirst(data, m.Value, (string)typeof(object).Assembly.GetType("Mono.Runtime").InvokeMember("GetDisplayName", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding, null, null, null));
                            else
                                data = ReplaceFirst(data, m.Value, "Mono.Runtime not detected");
                            break;
                    }
                    m = m.NextMatch();
                }
            }
            catch (Exception e)
            {
                WriteErrorFile(connection, "ParseIdentifier:" + data, e);
            }
            return data;
        }
        
        private string GetOperatingSystemName()
        {

            string OSName = "Unknown";
            System.OperatingSystem osInfo = System.Environment.OSVersion;

            if (osInfo.Platform == PlatformID.Unix)
            {
                return Environment.OSVersion.ToString();
            }

            OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
            osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));
            if (!GetVersionEx(ref osVersionInfo))
            {
               return OSName;
            }
            
            switch (osInfo.Platform)
            {                
                case PlatformID.Win32NT:

                    switch (osInfo.Version.Major)
                    {
                        case 3:
                            OSName = "Windows NT 3.51";
                            break;
                        case 4:
                            OSName = "Windows NT 4.0";
                            break;
                        case 5:
                            switch (osInfo.Version.Minor)
                            {
                                case 0:
                                    OSName = "Windows 2000";
                                    break;
                                case 1:
                                    OSName = "Windows XP";
                                    break;
                                case 2:                                    
                                    OSName = "Windows Server 2003";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 6:
                            switch (osInfo.Version.Minor)
                            {
                                case 0:
                                    //producttype == VER_NT_WORKSTATION
                                    if (osVersionInfo.dwPlatformId != VER_NT_WORKSTATION)
                                        OSName = "Windows Vista";
                                    else
                                    //producttype != VER_NT_WORKSTATION                                    
                                        OSName = "Windows Server 2008";
                                    break;
                                case 1:
                                    //producttype != VER_NT_WORKSTATION
                                    if (osVersionInfo.dwPlatformId == VER_NT_WORKSTATION)
                                        OSName = "Windws Server 2008 R2";
                                    else
                                        //producttype == VER_NT_WORKSTATION                                                                        
                                        OSName = "Windows 7";
                                    break;
                                case 2:
                                    if (osVersionInfo.dwPlatformId == VER_NT_WORKSTATION)
                                        OSName = "Windws Server 2012";
                                    else
                                        //producttype == VER_NT_WORKSTATION                                                                        
                                        OSName = "Windows 8";
                                    break;
                            }
                            break;
                        default:
                            OSName = "Unknown Win32NT Windows";
                            break;

                    }
                    break;

                case PlatformID.Win32S:
                    break;

                case PlatformID.Win32Windows:
                    switch (osInfo.Version.Major)
                    {
                        case 0:
                            OSName = "Windows 95";
                            break;
                        case 10:
                            if (osInfo.Version.Revision.ToString() == "2222A")
                                OSName = "Windows 98 Second Edition";
                            else
                                OSName = "Windows 98";
                            break;
                        case 90:
                            OSName = "Windows ME";
                            break;
                        default:
                            OSName = "Unknown Win32 Windows";
                            break;
                    }
                    break;
                case PlatformID.WinCE:
                    break;
                default:
                    break;

            }
            return OSName;

        }
        private string ParseIdentifierValue(string data, string dataPassed)
        {
            //split up the data into words
            string[] parsedData = data.Split(' ');

            //the data that was passed for parsing identifiers
            string[] passedData = dataPassed.Split(' ');

            //will hold the updates message/data after identifiers are parsed
            string[] changedData = data.Split(' ');
        
            int count = -1;

            //search for identifiers that are numbers
            //used for replacing values passed to the function
            foreach (string word in parsedData)
            {
                count++;

                if (word.StartsWith("//") && count == 0)
                    changedData[count] = word.Substring(1);

                //parse out identifiers (start with a $)
                if (word.StartsWith("$"))
                {
                     switch (word)
                     {
                         case "$+":
                             break;
                         default:
                             //search for identifiers that are numbers
                             //used for replacing values passed to the function
                             int result;
                             int z = 1;

                             while (z < word.Length)
                             {
                                 if (Int32.TryParse(word.Substring(z, 1), out result))
                                     z++;
                                 else
                                     break;
                             }

                             //check for - after numbered identifier
                             if (z > 1)
                             {
                                 //get the numbered identifier value
                                 int identVal = Int32.Parse(word.Substring(1, z - 1));
                                 
                                 if (identVal <= passedData.Length)
                                 {
                                     //System.Diagnostics.Debug.WriteLine(identVal + ":" +  passedData[identVal - 1]);
                                     //System.Diagnostics.Debug.WriteLine(z + ":" + word.Length);
                                     //System.Diagnostics.Debug.WriteLine(word.Substring(z,1));
                                     if (word.Length > z)
                                         if (word.Substring(z, 1) == "-")
                                         {
                                             //System.Diagnostics.Debug.WriteLine("change - " + identVal + ":" + passedData.Length);
                                             changedData[count] = String.Join(" ", passedData, identVal - 1, passedData.Length - identVal + 1);
                                             continue;
                                         }
                                     //System.Diagnostics.Debug.WriteLine("change normal ");
                                     changedData[count] = passedData[identVal - 1];
                                 }
                                 else
                                     changedData[count] = "";
                             }
                             break;
                     }
                 }
             }
            return String.Join(" ",changedData);
        } 
        /// <summary>
        /// Parse out $identifiers for outgoing commands
        /// </summary>
        /// <param name="connection">Which Connection it is for</param>
        /// <param name="data">The data to be parsed</param>
        /// <returns></returns>
        private string ParseIdentifiers(IRCConnection connection, string data, string dataPassed)
        {
            string[] changedData = null;
            
            try
            {

                //parse the initial identifiers
                data = ParseIdentifier(connection, data);

                //parse out the $1,$2.. identifiers
                data = ParseIdentifierValue(data, dataPassed);

                //$+ is a joiner identifier, great for joining 2 words together
                data = data.Replace(" $+ ", string.Empty);



                //parse out the current channel #
                if (CurrentWindowType == IceTabPage.WindowType.Channel)
                {
                    data = data.Replace(" # ", " " + CurrentWindow.TabCaption + " ");
                }

                //split up the data into words
                string[] parsedData = data.Split(' ');

                //the data that was passed for parsing identifiers
                string[] passedData = dataPassed.Split(' ');

                //will hold the updates message/data after identifiers are parsed
                changedData = data.Split(' ');

                int count = -1;
                string extra = "";
                bool askExtra = false;
                bool askSecure = false;

                foreach (string word in parsedData)
                {
                    count++;

                    if (word.StartsWith("//") && count == 0)
                        changedData[count] = word.Substring(1);

                    if (askExtra)
                    {
                        //continueing a $?= 
                        extra += " " + word;
                        changedData[count] = null;
                        if (extra[extra.Length - 1] == extra[0])
                        {
                            askExtra = false;
                            //ask the question
                            InputBoxDialog i = new InputBoxDialog();
                            i.PasswordChar = askSecure;
                            i.FormCaption = "Enter Value";
                            i.FormPrompt = extra.Substring(1,extra.Length-2);

                            i.ShowDialog();
                            if (i.InputResponse.Length > 0)
                                changedData[count] = i.InputResponse;
                            i.Dispose();                            
                        }
                    }

                    //parse out identifiers (start with a $)
                    if (word.StartsWith("$"))
                    {
                        switch (word)
                        {

                            default:
                                int result;
                                if (word.StartsWith("$?=") && word.Length > 5)
                                {
                                    //check for 2 quotes (single or double)
                                    string ask = word.Substring(3);
                                    //check what kind of a quote it is
                                    char quote = ask[0];
                                    if (quote == ask[ask.Length - 1])
                                    {
                                        //ask the question
                                        extra = ask;
                                        InputBoxDialog i = new InputBoxDialog();
                                        i.FormCaption = "Enter Value";
                                        i.FormPrompt = extra.Substring(1, extra.Length - 2);
                                        
                                        i.ShowDialog();
                                        if (i.InputResponse.Length > 0)
                                            changedData[count] = i.InputResponse;
                                        else
                                            changedData[count] = null;
                                        i.Dispose();
                                    }
                                    else
                                    {
                                        //go to the next word until we find a quote at the end
                                        extra = ask;
                                        askExtra = true;
                                        changedData[count] = null;
                                    }
                                }
                                
                                //check for $?*="" // password char
                                if (word.StartsWith("$?*=") && word.Length > 6)
                                {
                                    //check for 2 quotes (single or double)
                                    string ask = word.Substring(4);
                                    //check what kind of a quote it is
                                    char quote = ask[0];
                                    if (quote == ask[ask.Length - 1])
                                    {
                                        //ask the question
                                        extra = ask;
                                        InputBoxDialog i = new InputBoxDialog();
                                        i.PasswordChar = true;
                                        i.FormCaption = "Enter Value";
                                        i.FormPrompt = extra.Substring(1, extra.Length - 2);

                                        i.ShowDialog();
                                        if (i.InputResponse.Length > 0)
                                            changedData[count] = i.InputResponse;
                                        else
                                            changedData[count] = null;
                                        i.Dispose();
                                    }
                                    else
                                    {
                                        //go to the next word until we find a quote at the end
                                        extra = ask;
                                        askExtra = true;
                                        askSecure = true;
                                        changedData[count] = null;
                                    }
                                }


                                if (word.StartsWith("$md5(") && word.IndexOf(')') > word.IndexOf('('))
                                {
                                    string input = ReturnBracketValue(word);
                                    changedData[count] = MD5(input);
                                }

                                if (word.StartsWith("$rand(") && word.IndexOf(')') > word.IndexOf('('))
                                {
                                    string input = ReturnBracketValue(word);
                                    //look for a comma (,)
                                    if (input.Split(',').Length == 2)
                                    {
                                        string lownum = input.Split(',')[0];
                                        string hinum = input.Split(',')[1];
                                        
                                        //string prop = ReturnPropertyValue(word);

                                        int lowNum, hiNum;
                                        if (Int32.TryParse(lownum, out lowNum) && Int32.TryParse(hinum, out hiNum))
                                        {
                                            //valid numbers
                                            Random r = new Random();
                                            int randNumber = r.Next(lowNum, hiNum);

                                            changedData[count] = randNumber.ToString();
                                        }
                                        else
                                            changedData[count] = "$null";

                                    }                                    
                                    else if (input.IndexOf(',') == -1)
                                    {
                                        //make it a value from 1 - value
                                        int hiNum;
                                        if (Int32.TryParse(input, out hiNum))
                                        {
                                            //valid number
                                            Random r = new Random();
                                            int randNumber = r.Next(1, hiNum);

                                            changedData[count]= randNumber.ToString();
                                        }
                                        else
                                            changedData[count] = "$null";

                                    }
                                    else
                                        changedData[count] = "$null";
                                }                                

                                if (word.StartsWith("$read(") && word.IndexOf(')') > word.IndexOf('('))
                                {
                                    string file = ReturnBracketValue(word);
                                    //check if we have passed a path or just a filename
                                    if (file.IndexOf(System.IO.Path.DirectorySeparatorChar) > -1)
                                    {
                                        //its a full folder
                                        if (File.Exists(file))
                                        {
                                            //count the number of lines in the file                                            
                                            //load the file in and read a random line from it
                                            string[] lines = File.ReadAllLines(file);
                                            if (lines.Length > 0)
                                            {
                                                //pick a random line
                                                Random r = new Random();
                                                int line = r.Next(0, lines.Length - 1);
                                                changedData[count] = lines[line];
                                            }
                                            else
                                                changedData[count] = "$null";

                                        }
                                        else
                                        {
                                            changedData[count] = "$null";
                                        }
                                    }
                                    else
                                    {
                                        //just check in the Scripts Folder
                                        if (File.Exists(scriptsFolder + System.IO.Path.DirectorySeparatorChar + file))
                                        {
                                            //load the file in and read a random line from it
                                            string[] lines = File.ReadAllLines(scriptsFolder + System.IO.Path.DirectorySeparatorChar + file);
                                            if (lines.Length > 0)
                                            {
                                                //pick a random line
                                                Random r = new Random();
                                                int line = r.Next(0, lines.Length - 1);
                                                changedData[count] = lines[line];
                                            }
                                            else
                                                changedData[count] = "$null";
                                        }
                                        else
                                        {
                                            changedData[count] = "$null";
                                        }
                                    }
                                }

                                if (word.StartsWith("$plugin(") && word.IndexOf(')') > word.IndexOf('('))
                                {
                                    //get the plugin information
                                    string pluginid = ReturnBracketValue(word);
                                    string prop = ReturnPropertyValue(word);

                                    //tryparse
                                    if (Int32.TryParse(pluginid, out result))
                                    {
                                        //int id = int.Parse(pluginid);
                                        for (int i = 0; i < loadedPlugins.Count; i++)
                                        {
                                            if (i == result)
                                            {
                                                IPluginIceChat ipc = (IPluginIceChat)loadedPlugins[i];

                                                switch (prop.ToLower())
                                                {
                                                    case "id":
                                                        changedData[count] = i.ToString();
                                                        break;
                                                    case "name":
                                                        changedData[count] = ipc.Name;
                                                        break;
                                                    case "version":
                                                        changedData[count] = ipc.Version;
                                                        break;
                                                    case "author":
                                                        changedData[count] = ipc.Author;
                                                        break;
                                                    case "enabled":
                                                        changedData[count] = ipc.Enabled.ToString();
                                                        break;
                                                    case "filename":
                                                        changedData[count] = ipc.FileName;
                                                        break;
                                                    default:
                                                        changedData[count] = ipc.Name;
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //go by plugin filename, not number
                                        for (int i = 0; i < loadedPlugins.Count; i++)
                                        {
                                            if (((IPluginIceChat)loadedPlugins[i]).FileName.ToLower() == pluginid.ToLower())
                                            {
                                                IPluginIceChat ipc = (IPluginIceChat)loadedPlugins[i];

                                                switch (prop.ToLower())
                                                {
                                                    case "id":
                                                        changedData[count] = i.ToString();
                                                        break;
                                                    case "name":
                                                        changedData[count] = ipc.Name;
                                                        break;
                                                    case "version":
                                                        changedData[count] = ipc.Version;
                                                        break;
                                                    case "author":
                                                        changedData[count] = ipc.Author;
                                                        break;
                                                    case "enabled":
                                                        changedData[count] = ipc.Enabled.ToString();
                                                        break;
                                                    case "filename":
                                                        changedData[count] = ipc.FileName;
                                                        break;
                                                    default:
                                                        changedData[count] = ipc.Name;
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }


                                if (connection != null)
                                {
                                    if (word.StartsWith("$ial(") && word.IndexOf(')') > word.IndexOf('('))
                                    {
                                        string nick = ReturnBracketValue(word);
                                        string prop = ReturnPropertyValue(word);

                                        InternalAddressList ial = (InternalAddressList)connection.ServerSetting.IAL[nick];
                                        if (ial != null)
                                        {
                                            if (prop.Length == 0)
                                                changedData[count] = ial.Nick;
                                            else
                                            {
                                                switch (prop.ToLower())
                                                {
                                                    case "nick":
                                                        changedData[count] = ial.Nick;
                                                        break;
                                                    case "host":
                                                        changedData[count] = ial.Host;
                                                        break;
                                                }
                                            }
                                        }
                                        else
                                            changedData[count] = "$null";
                                    }

                                    if (word.StartsWith("$nick(") && word.IndexOf(')') > word.IndexOf('('))
                                    {
                                        //get the value between and after the brackets
                                        string values = ReturnBracketValue(word);
                                        if (values.Split(',').Length == 2)
                                        {
                                            string channel = values.Split(',')[0];
                                            string nickvalue = values.Split(',')[1];

                                            string prop = ReturnPropertyValue(word);

                                            // $nick(#,N)     
                                            //find then channel
                                            IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
                                            if (t != null)
                                            {
                                                User u = null;
                                                if (Int32.TryParse(nickvalue, out result))
                                                {
                                                    if (Convert.ToInt32(nickvalue) == 0)
                                                        changedData[count] = t.Nicks.Count.ToString();
                                                    else
                                                        u = t.GetNick(Convert.ToInt32(nickvalue));
                                                }
                                                else
                                                {
                                                    u = t.GetNick(nickvalue);
                                                }

                                                if (prop.Length == 0 && u != null)
                                                {
                                                    changedData[count] = u.NickName;
                                                }
                                                else if (u != null)
                                                {
                                                    //$nick(#channel,1).op , .voice, .halfop, .admin,.owner. 
                                                    //.mode, .host, .nick,.ident
                                                    InternalAddressList ial = (InternalAddressList)connection.ServerSetting.IAL[u.NickName];
                                                    switch (prop.ToLower())
                                                    {
                                                        case "host":
                                                            if (ial != null && ial.Host != null && ial.Host.Length > 0)
                                                                changedData[count] = ial.Host.Substring(ial.Host.IndexOf('@') + 1);
                                                            break;
                                                        case "ident":
                                                            if (ial != null && ial.Host != null && ial.Host.Length > 0)
                                                                changedData[count] = ial.Host.Substring(0,ial.Host.IndexOf('@'));
                                                            break;
                                                        case "nick":
                                                            changedData[count] = u.NickName;
                                                            break;
                                                        case "mode":
                                                            changedData[count] = u.ToString().Replace(u.NickName, "");
                                                            break;
                                                        case "op":
                                                            for (int i = 0; i < u.Level.Length; i++)
                                                            {
                                                                if (connection.ServerSetting.StatusModes[0][i] == 'o')
                                                                {
                                                                    if (u.Level[i] == true)
                                                                        changedData[count] = "$true";
                                                                    else
                                                                        changedData[count] = "$false";
                                                                }
                                                            }
                                                            break;
                                                        case "voice":
                                                            for (int i = 0; i < u.Level.Length; i++)
                                                            {
                                                                if (connection.ServerSetting.StatusModes[0][i] == 'v')
                                                                {
                                                                    if (u.Level[i] == true)
                                                                        changedData[count] = "$true";
                                                                    else
                                                                        changedData[count] = "$false";
                                                                }
                                                            }
                                                            break;
                                                    }
                                                    ial = null;
                                                }
                                            }
                                        }
                                    }

                                    if (word.StartsWith("$chan(") && word.IndexOf(')') > word.IndexOf('('))
                                    {
                                        //get the value between and after the brackets
                                        string channel = ReturnBracketValue(word);
                                        string prop = ReturnPropertyValue(word);

                                        //find then channel
                                        IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
                                        if (t != null)
                                        {
                                            if (prop.Length == 0)
                                            {
                                                //replace with channel name
                                                changedData[count] = t.TabCaption;
                                            }
                                            else
                                            {
                                                switch (prop.ToLower())
                                                {
                                                    case "mode":
                                                        changedData[count] = t.ChannelModes;
                                                        break;
                                                    case "count":
                                                        changedData[count] = t.Nicks.Count.ToString();
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                    }

                                    if (word.StartsWith("$timer(") && word.IndexOf(')') > word.IndexOf('('))
                                    {
                                        //get the value between and after the brackets
                                        string timerid = ReturnBracketValue(word);
                                        string prop = ReturnPropertyValue(word);

                                        //find the timer
                                        foreach (IrcTimer timer in connection.IRCTimers)
                                        {
                                            if (timer.TimerID == timerid)
                                            {
                                                if (prop.Length == 0)
                                                {
                                                    //replace with timer id
                                                    changedData[count] = timer.TimerID;
                                                }
                                                else
                                                {
                                                    switch (prop.ToLower())
                                                    {
                                                        case "id":
                                                            changedData[count] = timer.TimerID;
                                                            break;
                                                        case "reps":
                                                            changedData[count] = timer.TimerRepetitions.ToString();
                                                            break;
                                                        case "count":
                                                            changedData[count] = timer.TimerCounter.ToString();
                                                            break;
                                                        case "command":
                                                            changedData[count] = timer.TimerCommand;
                                                            break;
                                                        case "interval":
                                                            changedData[count] = timer.TimerInterval.ToString();
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                    }


                                    if (word.StartsWith("$mask(") && word.IndexOf(')') > word.IndexOf('('))
                                    {
                                        //$mask($host,2)
                                        //get the value between and after the brackets
                                        string values = ReturnBracketValue(word);
                                        string prop = ReturnPropertyValue(word);

                                        if (values.Split(',').Length == 2)
                                        {
                                            string full_host = values.Split(',')[0];
                                            string mask_value = values.Split(',')[1];

                                            if (full_host.Length == 0) break;
                                            if (mask_value.Length == 0) break;

                                            if (full_host.IndexOf("@") == -1) break;
                                            if (full_host.IndexOf("!") == -1) break;

                                            switch (mask_value)
                                            {                                                
                                                case "0":   // *!user@host
                                                    changedData[count] = "*!" + full_host.Substring(full_host.IndexOf("!") + 1);
                                                    break;

                                                case "1":   // *!*user@host
                                                    changedData[count] = "*!*" + full_host.Substring(full_host.IndexOf("!") + 1);                                                    
                                                    break;

                                                case "2":   // *!*user@*.host
                                                    changedData[count] = "*!*" + full_host.Substring(full_host.IndexOf("@"));                                                    
                                                    break;

                                                case "3":   // *!*user@*.host
                                                    break;

                                                case "4":   // *!*@*.host
                                                    break;

                                                case "5":   // nick!user@host
                                                    changedData[count] = full_host;
                                                    break;

                                                case "6":   // nick!*user@host
                                                    break;

                                                case "7":   // nick!*@host
                                                    break;

                                                case "8":   // nick!*user@*.host
                                                    break;

                                                case "9":   // nick!*@*.host
                                                    break;

                                                case "10":  // nick!*@*
                                                    changedData[count] = full_host.Substring(0, full_host.IndexOf("!")) + "!*@*";
                                                    break;

                                                case "11":  // *!user@*
                                                    break;
                                            }



                                        }
                                        
                                    }
                                
                                
                                }
                                break;
                        }


                    }

                }
            }
            catch (Exception e)
            {
                WriteErrorFile(connection, "ParseIdentifiers" + data, e);
            }
            //return String.Join(" ", changedData);
            return JoinString(changedData);
        }
        
        //rejoin an arrayed string into a single string, not adding null values
        private string JoinString(string[] joinString)
        {
            string joined = "";
            foreach (string j in joinString)
            {
                if (j != null)
                    joined += j + " ";
            }
            if (joined.Length > 0)
                joined = joined.Substring(0, joined.Length - 1);
            return joined;
        }

        private string ReturnBracketValue(string data)
        {
            //return what is between ( ) brackets
            string d = data.Substring(data.IndexOf('(') + 1);
            return d.Substring(0,d.IndexOf(')'));
        }
        
        private string ReturnPropertyValue(string data)
        {
            if (data.IndexOf('.') == -1)
                return "";
            else
                return data.Substring(data.LastIndexOf('.') + 1);
        }

        //replace 1st occurence of a string inside another string
        private string ReplaceFirst(string haystack, string needle, string replacement)
        {
            int pos = haystack.IndexOf(needle);
            if (pos < 0) return haystack;

            return haystack.Substring(0, pos) + replacement + haystack.Substring(pos + needle.Length);
        }

        private string GetDuration(double seconds)
        {
            TimeSpan t = new TimeSpan(0, 0,(int)seconds);

            string s = t.Seconds.ToString() + " secs";
            if (t.Minutes > 0)
                s = t.Minutes.ToString() + " mins " + s;
            if (t.Hours > 0)
                s = t.Hours.ToString() + " hrs " + s;
            if (t.Days > 0)
                s = t.Days.ToString() + " days " + s;

            return s;
        }

        private string MD5(string password)
        {
            byte[] textBytes = System.Text.Encoding.Default.GetBytes(password);
            try
            {
                System.Security.Cryptography.MD5CryptoServiceProvider cryptHandler;
                cryptHandler = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] hash = cryptHandler.ComputeHash(textBytes);
                string ret = "";
                foreach (byte a in hash)
                {
                    if (a < 16)
                        ret += "0" + a.ToString("x");
                    else
                        ret += a.ToString("x");
                }
                return ret;
            }
            catch
            {
                throw;
            }
        }

        #region Menu and ToolStrip Items

        /// <summary>
        /// Close the Application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //show the about box
            FormAbout fa = new FormAbout();
            fa.Show(this);
        }

        private void minimizeToTrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.previousWindowState = this.WindowState;
            this.Hide();
            this.notifyIcon.Visible = true;
            this.iceChatOptions.IsOnTray = true;

            if (iceChatOptions.AutoAwaySystemTray)
            {
                foreach (IRCConnection c in serverTree.ServerConnections.Values)
                {
                    if (c.IsConnected)
                    {
                        if (c.ServerSetting.Away == false)
                        {
                            string msg = iceChatOptions.AutoAwayMessage;
                            msg = msg.Replace("$autoawaytime", iceChatOptions.AutoAwayTime.ToString());
                            ParseOutGoingCommand(c, "/away " + msg);
                        }
                    }
                }

            }

        }

        private void NotifyIconMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Visible == true)
            {
                this.Activate();
            }
            else
            {
                this.iceChatOptions.IsOnTray = false;
                this.Show();
                this.WindowState = previousWindowState;
                this.notifyIcon.Visible = iceChatOptions.ShowSytemTrayIcon;
            }
        }

        private void iceChatSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //bring up a very basic settings form
            FormSettings fs = new FormSettings(iceChatOptions, iceChatFonts, iceChatEmoticons, iceChatSounds);
            fs.SaveOptions += new FormSettings.SaveOptionsDelegate(fs_SaveOptions);
            fs.Show(this);
        }

        private void fs_SaveOptions()
        {
            SaveOptions();           
            SaveFonts();
            SaveSounds();

            //implement the new Font Settings
            
            //do all the Console Tabs Windows
            foreach (ConsoleTab c in mainChannelBar.GetTabPage("Console").ConsoleTab.TabPages)
            {
                ((TextWindow)c.Controls[0]).Font = new Font(iceChatFonts.FontSettings[0].FontName, iceChatFonts.FontSettings[0].FontSize);
                if (((TextWindow)c.Controls[0]).MaximumTextLines != iceChatOptions.MaximumTextLines)
                    ((TextWindow)c.Controls[0]).MaximumTextLines = iceChatOptions.MaximumTextLines;
            }
            
            //do all the Channel and Query Tabs Windows
            foreach (IceTabPage t in mainChannelBar.TabPages)
            {
                if (t.WindowStyle == IceTabPage.WindowType.Channel)
                {
                    t.TextWindow.Font = new Font(iceChatFonts.FontSettings[1].FontName, iceChatFonts.FontSettings[1].FontSize);
                    t.ResizeTopicFont(iceChatFonts.FontSettings[1].FontName, iceChatFonts.FontSettings[1].FontSize);
                }
                if (t.WindowStyle == IceTabPage.WindowType.Query)
                    t.TextWindow.Font = new Font(iceChatFonts.FontSettings[2].FontName, iceChatFonts.FontSettings[2].FontSize);

                if (t.WindowStyle == IceTabPage.WindowType.DCCChat)
                    t.TextWindow.Font = new Font(iceChatFonts.FontSettings[2].FontName, iceChatFonts.FontSettings[2].FontSize);

                if (t.WindowStyle == IceTabPage.WindowType.Window)
                    t.TextWindow.Font = new Font(iceChatFonts.FontSettings[1].FontName, iceChatFonts.FontSettings[1].FontSize);

                if (t.WindowStyle != IceTabPage.WindowType.Console && t.WindowStyle != IceTabPage.WindowType.DCCFile && t.WindowStyle != IceTabPage.WindowType.ChannelList)
                {
                    //check if value is different
                    if (t.TextWindow.MaximumTextLines != iceChatOptions.MaximumTextLines)
                        t.TextWindow.MaximumTextLines = iceChatOptions.MaximumTextLines;
                }
            }
            
            //change the server list
            serverTree.Font = new Font(iceChatFonts.FontSettings[4].FontName, iceChatFonts.FontSettings[4].FontSize);
            serverTree.ShowServerButtons = iceChatOptions.ShowServerButtons;

            //change the nick list
            nickList.Font = new Font(iceChatFonts.FontSettings[3].FontName, iceChatFonts.FontSettings[3].FontSize);
            nickList.ShowNickButtons = iceChatOptions.ShowNickButtons;

            mainChannelBar.TabFont = new Font(iceChatFonts.FontSettings[8].FontName, iceChatFonts.FontSettings[8].FontSize);

            //change the fonts for the Left and Right Dock Panels
            panelDockLeft.Initialize();
            panelDockRight.Initialize();

            this.notifyIcon.Visible = iceChatOptions.ShowSytemTrayIcon;

            //change the main Menu Bar Font
            //menuMainStrip.Font = new Font(iceChatFonts.FontSettings[7].FontName, iceChatFonts.FontSettings[7].FontSize);

            //change the inputbox font
            inputPanel.InputBoxFont = new Font(iceChatFonts.FontSettings[5].FontName, iceChatFonts.FontSettings[5].FontSize);

            //set if Emoticon Picker/Color Picker is Visible
            inputPanel.ShowEmoticonPicker = iceChatOptions.ShowEmoticonPicker;
            inputPanel.ShowColorPicker = iceChatOptions.ShowColorPicker;
            inputPanel.ShowSendButton = iceChatOptions.ShowSendButton;

            if (iceChatOptions.ShowEmoticons == false)
                inputPanel.ShowEmoticonPicker = false;

            //set if Status Bar is Visible
            statusStripMain.Visible = iceChatOptions.ShowStatusBar;

            foreach (IRCConnection c in serverTree.ServerConnections.Values)
            {
                if (c.IsConnected)
                {
                    if (iceChatOptions.AutoAway == true)
                        c.SetAutoAwayTimer(iceChatOptions.AutoAwayTime);
                    else
                        c.DisableAutoAwayTimer();
                }
            }
        }

        private void serverListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitterLeft.Visible = serverListToolStripMenuItem.Checked;
            panelDockLeft.Visible = serverListToolStripMenuItem.Checked;
            iceChatOptions.ShowServerTree = serverListToolStripMenuItem.Checked;
        }

        private void nickListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitterRight.Visible = nickListToolStripMenuItem.Checked;
            panelDockRight.Visible = nickListToolStripMenuItem.Checked;
            iceChatOptions.ShowNickList = nickListToolStripMenuItem.Checked;
        }

        private void statusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStripMain.Visible = statusBarToolStripMenuItem.Checked;
            iceChatOptions.ShowStatusBar = statusBarToolStripMenuItem.Checked;
        }

        private void channelBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            iceChatOptions.ShowTabBar = channelBarToolStripMenuItem.Checked;
            mainChannelBar.Visible = iceChatOptions.ShowTabBar;
        }

        private void iceChatColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //bring up a very basic settings form
            FormColors fc = new FormColors(iceChatMessages, iceChatColors);
            fc.SaveColors += new FormColors.SaveColorsDelegate(fc_SaveColors);
            fc.StartPosition = FormStartPosition.CenterParent;
            
            fc.Show(this);
        }

        private void fc_SaveColors(IceChatColors colors, IceChatMessageFormat messages)
        {
            this.iceChatColors = colors;
            SaveColors();
            
            this.iceChatMessages = messages;
            SaveMessageFormat();
            menuMainStrip.BackColor = IrcColor.colors[iceChatColors.MenubarBackColor];
            statusStripMain.BackColor = IrcColor.colors[iceChatColors.StatusbarBackColor];
            toolStripStatus.ForeColor = IrcColor.colors[iceChatColors.StatusbarForeColor];

            serverListTab.BackColor = IrcColor.colors[iceChatColors.PanelHeaderBG1];
            serverListTab.ForeColor = IrcColor.colors[iceChatColors.PanelHeaderForeColor];
            nickListTab.BackColor = IrcColor.colors[iceChatColors.PanelHeaderBG1];
            nickListTab.ForeColor = IrcColor.colors[iceChatColors.PanelHeaderForeColor];
            channelListTab.BackColor = IrcColor.colors[iceChatColors.PanelHeaderBG1];
            channelListTab.ForeColor = IrcColor.colors[iceChatColors.PanelHeaderForeColor];
            buddyListTab.BackColor = IrcColor.colors[iceChatColors.PanelHeaderBG1];
            buddyListTab.ForeColor = IrcColor.colors[iceChatColors.PanelHeaderForeColor];

            inputPanel.SetInputBoxColors();
            channelList.SetListColors();
            buddyList.SetListColors();
            nickList.Invalidate();
            serverTree.Invalidate();
            mainChannelBar.Invalidate();
            
            //rebuild the themes menu
            foreach (ToolStripMenuItem t in themesToolStripMenuItem.DropDownItems)
                t.Click -= themeChoice_Click;
            
            themesToolStripMenuItem.DropDownItems.Clear();
            this.themesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defaultToolStripMenuItem});

            defaultToolStripMenuItem.Click +=new EventHandler(defaultToolStripMenuItem_Click);

            foreach (ThemeItem theme in FormMain.Instance.IceChatOptions.Theme)
            {
                if (!theme.ThemeName.Equals("Default"))
                {
                    ToolStripMenuItem t = new ToolStripMenuItem(theme.ThemeName);
                    if (iceChatOptions.CurrentTheme == theme.ThemeName)
                        t.Checked = true;

                    t.Click += new EventHandler(themeChoice_Click);
                    themesToolStripMenuItem.DropDownItems.Add(t);
                }
            }



            //update all the console windows
            foreach (ConsoleTab c in mainChannelBar.GetTabPage("Console").ConsoleTab.TabPages)
            {
                ((TextWindow)c.Controls[0]).IRCBackColor = iceChatColors.ConsoleBackColor;
            }

            //update all the Channel and Query Tabs Windows
            foreach (IceTabPage t in mainChannelBar.TabPages)
            {
                if (t.WindowStyle == IceTabPage.WindowType.Channel)
                {
                    t.TopicWindow.IRCBackColor = iceChatColors.ChannelBackColor;
                    t.TextWindow.IRCBackColor = iceChatColors.ChannelBackColor;
                }
                if (t.WindowStyle == IceTabPage.WindowType.Query)
                    t.TextWindow.IRCBackColor = iceChatColors.QueryBackColor;

                if (t.WindowStyle == IceTabPage.WindowType.DCCChat)
                    t.TextWindow.IRCBackColor = iceChatColors.QueryBackColor;
            }

        }

        private void fontSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings fs = new FormSettings(iceChatOptions, iceChatFonts, iceChatEmoticons, iceChatSounds, true);
            fs.SaveOptions += new FormSettings.SaveOptionsDelegate(fs_SaveOptions);

            fs.Show(this);
        }

        private void OnQuickConnectServer(ServerSetting s)
        {
            s.NickName = iceChatOptions.DefaultNick;
            s.AltNickName = iceChatOptions.DefaultNick + "_";
            s.AwayNickName = iceChatOptions.DefaultNick + "[A]";
            s.FullName = iceChatOptions.DefaultFullName;
            s.QuitMessage = iceChatOptions.DefaultQuitMessage;
            s.IdentName = iceChatOptions.DefaultIdent;
            s.IAL = new Hashtable();

            Random r = new Random();
            s.ID = r.Next(50000, 99999);

            NewServerConnection(s);
        }

        private void toolStripAway_Click(object sender, EventArgs e)
        {
            //check if away or not
            if (FormMain.Instance.InputPanel.CurrentConnection != null)
            {
                if (inputPanel.CurrentConnection.ServerSetting.Away)
                {
                    ParseOutGoingCommand(inputPanel.CurrentConnection, "/away");
                }
                else
                {
                    //ask for an away reason
                    InputBoxDialog i = new InputBoxDialog();
                    i.FormCaption = "Enter your away Reason";
                    i.FormPrompt = "Away Reason";

                    i.ShowDialog();
                    if (i.InputResponse.Length > 0)
                        ParseOutGoingCommand(inputPanel.CurrentConnection, "/away " + i.InputResponse);
                    
                    i.Dispose();
                }
            }
        }

        private void toolStripUpdate_Click(object sender, EventArgs e)
        {
            //update is available, start the updater
            CurrentWindowMessage(inputPanel.CurrentConnection, "There is a newer version of IceChat available", 4, true);
            DialogResult result = MessageBox.Show("Would you like to update to the newer version of IceChat?", "IceChat update available", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.Load(currentFolder + System.IO.Path.DirectorySeparatorChar + "Update" + Path.DirectorySeparatorChar + "updater.xml");
                
                System.Xml.XmlNodeList updaterFile = xmlDoc.GetElementsByTagName("file");
                System.Net.WebClient webClient = new System.Net.WebClient();

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_DownloadFileCompleted);                

                string f = System.IO.Path.GetFileName(updaterFile[0].InnerText);

                if (File.Exists(currentFolder + System.IO.Path.DirectorySeparatorChar + "Update" + Path.DirectorySeparatorChar + f))
                    File.Delete(currentFolder + System.IO.Path.DirectorySeparatorChar + "Update" + Path.DirectorySeparatorChar + f);

                Uri uri = new Uri(updaterFile[0].InnerText);
                
                string localFile = Path.GetFileName(uri.ToString());

                webClient.DownloadFileAsync(uri, currentFolder + System.IO.Path.DirectorySeparatorChar + "Update" + Path.DirectorySeparatorChar + localFile);

            }
        }

        private void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (File.Exists(currentFolder + System.IO.Path.DirectorySeparatorChar + "Update" + Path.DirectorySeparatorChar + "IceChatUpdater.exe"))
            {
                System.Diagnostics.Process process = null;
                System.Diagnostics.ProcessStartInfo processStartInfo;

                processStartInfo = new System.Diagnostics.ProcessStartInfo();

                processStartInfo.FileName = currentFolder + System.IO.Path.DirectorySeparatorChar + "Update" + Path.DirectorySeparatorChar + "IceChatUpdater.exe";
                System.Diagnostics.Debug.WriteLine(processStartInfo.FileName);

                if (System.Environment.OSVersion.Version.Major >= 6)  // Windows Vista or higher
                {
                    processStartInfo.Verb = "runas";
                }
                else
                {
                    // No need to prompt to run as admin
                }

                processStartInfo.Arguments = "\"" + Application.StartupPath + "\"";
                
                processStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                processStartInfo.UseShellExecute = true;
                
                process = System.Diagnostics.Process.Start(processStartInfo);
            }           
        }

        private void iceChatEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormEditor fe = new FormEditor();
            fe.Show(this);
        }

        private void toolStripSystemTray_Click(object sender, EventArgs e)
        {
            minimizeToTrayToolStripMenuItem.PerformClick();
        }

        private void toolStripEditor_Click(object sender, EventArgs e)
        {
            iceChatEditorToolStripMenuItem.PerformClick();
        }

        private void debugWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //add the debug window, if it does not exist
            if (GetWindow(null, "Debug", IceTabPage.WindowType.Debug) == null)
            {
                AddWindow(null, "Debug", IceTabPage.WindowType.Debug);
                mainChannelBar.SelectTab(mainChannelBar.GetTabPage("Debug"));
            }
            else
            {
                //close the tab
                RemoveWindow(null, "Debug", IceTabPage.WindowType.Debug);
            }
            serverTree.Invalidate();
        }

        private void restoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            iceChatOptions.IsOnTray = false;
            this.Show();
            this.notifyIcon.Visible = iceChatOptions.ShowSytemTrayIcon;
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.exitToolStripMenuItem.PerformClick();
        }

        private void OnPluginMenuItemClick(object sender, EventArgs e)
        {
            //show plugin information
            FormPluginInfo pi = new FormPluginInfo((IPluginIceChat)((ToolStripMenuItem)sender).Tag, (ToolStripMenuItem)sender);
            pi.ShowDialog(this);
        }
        
        #endregion

        //http://www.codeproject.com/KB/cs/dynamicpluginmanager.aspx

        private IPluginIceChat loadPlugin(string fileName)
        {
            string args = fileName.Substring(fileName.LastIndexOf("\\") + 1);
            args = args.Substring(0, args.Length - 4);

            Type ObjType = null;
            try
            {
                Assembly ass = null;
                //System.Diagnostics.Debug.WriteLine("load plugin file " + fileName);
                ass = Assembly.LoadFile(fileName);
                
                if (ass != null)
                {
                    ObjType = ass.GetType("IceChatPlugin.Plugin");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("assembly is null::" + ass.GetType("IceChatTheme.Plugin"));                    
                    return null;
                }
                 
            }
            catch (Exception ex)
            {
                WriteErrorFile(inputPanel.CurrentConnection, "LoadPlugin Error ", ex);
                return null;
            }
            try
            {
                // OK Lets create the object as we have the Report Type
                if (ObjType != null)
                {
                    IPluginIceChat ipi = (IPluginIceChat)Activator.CreateInstance(ObjType);

                    ipi.MainForm = this;
                    ipi.MainMenuStrip = menuMainStrip;
                    ipi.CurrentFolder = currentFolder;
                    
                    System.Diagnostics.FileVersionInfo fv = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                    ipi.CurrentVersion = Convert.ToDouble(fv.FileVersion.Replace(".", String.Empty));

                    ipi.FileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                    ipi.LeftPanel = panelDockLeft.TabControl;
                    ipi.RightPanel = panelDockRight.TabControl;

                    //ipi.domain = appDomain;
                    ipi.domain = null;
                    ipi.Enabled = true; //enable it by default

                    WindowMessage(null, "Console", "Loaded Plugin - " + ipi.Name + " v" + ipi.Version + " by " + ipi.Author, 4, true);

                    //add the menu items
                    ToolStripMenuItem t = new ToolStripMenuItem(ipi.Name);
                    t.BackColor = SystemColors.Menu;
                    t.ForeColor = SystemColors.MenuText;
                    t.Tag = ipi;
                    t.ToolTipText = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                    t.Click += new EventHandler(OnPluginMenuItemClick);

                    pluginsToolStripMenuItem.DropDownItems.Add(t);

                    ipi.OnCommand += new OutGoingCommandHandler(Plugin_OnCommand);
                    //ipi.Initialize();

                    loadedPlugins.Add(ipi);
                    return ipi;
                }
                else
                {
                    Assembly ass;
                    ass = Assembly.LoadFile(fileName);
                    ObjType = ass.GetType("IceChatTheme.Theme");
                    if (ObjType != null)
                    {
                        //this is an icechat theme
                        IThemeIceChat iTheme = (IThemeIceChat)Activator.CreateInstance(ObjType);
                        WindowMessage(null, "Console", "Loaded Plugin Theme - " + iTheme.Name + " v" + iTheme.Version + " by " + iTheme.Author, 4, true);
                        iTheme.Enabled = true;
                        iTheme.Initialize();

                        iTheme.FileName = fileName;
                        loadedPluginThemes.Add(iTheme);
                    }
                    else
                        System.Diagnostics.Debug.WriteLine("obj type is null: " + args);
                }
            }
            catch (Exception ex)
            {
                WriteErrorFile(inputPanel.CurrentConnection, "LoadPlugin Error", ex);
            }

            return null;


        }

        private void LoadPlugins()
        {
            string[] pluginFiles = Directory.GetFiles(pluginsFolder, "*.dll");
            
            for (int i = 0; i < pluginFiles.Length; i++)
            {
                loadPlugin(pluginFiles[i]);                
            }
        }

        private void Plugin_OnCommand(PluginArgs e)
        {
            if (e.Command != null)
            {
                if (e.Connection != null)
                    ParseOutGoingCommand(e.Connection, e.Command);
                else
                {
                    if (e.Extra == "current")
                        ParseOutGoingCommand(inputPanel.CurrentConnection, e.Command);
                    else
                        ParseOutGoingCommand(null, e.Command);
                }
            }

        }

        internal void UnloadPlugin(ToolStripMenuItem menuItem)
        {
            ParseOutGoingCommand(null, "/unloadplugin " + menuItem.ToolTipText);
        }
                
        internal void StatusPlugin(ToolStripMenuItem menuItem, bool enable)
        {
            for (int i = 0; i < iceChatPlugins.listPlugins.Count; i++)
            {
                if (((PluginItem)iceChatPlugins.listPlugins[i]).PluginFile.Equals(menuItem.ToolTipText))
                {
                    ((PluginItem)iceChatPlugins.listPlugins[i]).Enabled = enable;
                    SavePluginFiles();
                }
            }

            ParseOutGoingCommand(null, "/statusplugin " + enable.ToString() + " " + menuItem.ToolTipText);
        }
                
        /// <summary>
        /// Write out to the errors file, specific to the Connection
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="method"></param>
        /// <param name="e"></param>
        internal void WriteErrorFile(IRCConnection connection, string method, Exception e)
        {
            //System.Diagnostics.Debug.WriteLine(e.Message + ":" + e.StackTrace);
            //System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(e, true);
            WindowMessage(connection, "Console", "Error:" + method + ":" + e.Message + ":" + e.StackTrace, 4, true);
            
            if (errorFile != null)
            {
                try
                {
                    errorFile.WriteLine(DateTime.Now.ToString("G") + ":" + method);
                }
                catch (Exception ee) { System.Diagnostics.Debug.WriteLine("No error file:" + ee.Message); }
                finally 
                {                     
                    errorFile.Flush(); 
                }
            }
        }

        /// <summary>
        /// Write out to the errors file, not Connection Specific
        /// </summary>
        /// <param name="method"></param>
        /// <param name="e"></param>
        internal void WriteErrorFile(string method, FileNotFoundException e)
        {
            System.Diagnostics.Debug.WriteLine(e.Message + ":" + e.StackTrace);
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(e, true);            
            WindowMessage(inputPanel.CurrentConnection, "Console", "Error:" + method + ":" + e.Message + ":" + e.StackTrace + ":" + trace.GetFrame(0).GetFileLineNumber(), 4, true);

            if (errorFile != null)
            {
                errorFile.WriteLine(DateTime.Now.ToString("G") + ":" + method + ":" + e.Message + ":" + e.StackTrace + ":" + trace.GetFrame(0).GetFileLineNumber());
                errorFile.Flush();
            }
        }

        private void getLocalIPAddress()
        {            
            //find your internet IP Address
            System.Net.WebRequest request = System.Net.WebRequest.Create("http://www.icechat.net/_ipaddress.php");
            try
            {
                System.Net.WebResponse response = request.GetResponse();
                StreamReader stream = new StreamReader(response.GetResponseStream());
                string data = stream.ReadToEnd();
                stream.Close();
                response.Close();

                //remove any linefeeds and such
                data = data.Replace("\n", "");
                iceChatOptions.DCCLocalIP = data.Trim();

                //save the settings
                SaveOptions();
            }
            catch (Exception)
            {
                //error
            }
        }

        private void checkForUpdate()
        {
            //check for newer version
            System.Diagnostics.FileVersionInfo fv = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            double currentVersion = Convert.ToDouble(fv.FileVersion.Replace(".", String.Empty));

            try
            {
                if (File.Exists(currentFolder + System.IO.Path.DirectorySeparatorChar + "Update" + Path.DirectorySeparatorChar + "updater.xml"))
                    File.Delete(currentFolder + System.IO.Path.DirectorySeparatorChar + "Update" + Path.DirectorySeparatorChar + "updater.xml");
                
                System.Net.WebClient webClient = new System.Net.WebClient();
                webClient.DownloadFile("http://www.icechat.net/updater.xml", currentFolder + System.IO.Path.DirectorySeparatorChar + "Update" + Path.DirectorySeparatorChar + "updater.xml");
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.Load(currentFolder + System.IO.Path.DirectorySeparatorChar + "Update" + Path.DirectorySeparatorChar + "updater.xml");
                System.Xml.XmlNodeList version = xmlDoc.GetElementsByTagName("version");
                System.Xml.XmlNodeList versiontext = xmlDoc.GetElementsByTagName("versiontext");

                if (Convert.ToDouble(version[0].InnerText) > currentVersion)
                {
                    this.toolStripUpdate.Visible = true;
                }
                else
                {
                    this.toolStripUpdate.Visible = false;
                    CurrentWindowMessage(inputPanel.CurrentConnection, "You are running the latest version of The PonyChat Client (" + fv.FileVersion + ") -- Version Online = " + versiontext[0].InnerText, 4, true);
                }
            }
            catch (Exception ex)
            {
                CurrentWindowMessage(inputPanel.CurrentConnection, "Error checking for update :" + ex.Message, 4, true);
            }

        }

        private void checkForUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParseOutGoingCommand(null, "/update");
        }
        
        private void tabControl_DoubleClick(object sender, EventArgs e)
        {
            TabControl t = (TabControl)sender;
            if (t.SelectedTab.Controls[0].GetType() == typeof(Panel))
            {
                Panel p = (Panel)t.SelectedTab.Controls[0];
                UnDockPanel(p);
            }
        }
        /// <summary>
        /// Undock the Specified Panel to a Floating Window
        /// </summary>
        /// <param name="p">The panel to remove and add to a Floating Window</param>
        internal void UnDockPanel(Panel p)
        {
            if (p.Parent.GetType() == typeof(TabPage))
            {
                //System.Diagnostics.Debug.WriteLine(panel1.Parent.Name);
                //remove the tab from the tabStrip
                TabControl t = (TabControl)p.Parent.Parent;
                TabPage tp = (TabPage)p.Parent;
                ((TabControl)p.Parent.Parent).TabPages.Remove((TabPage)p.Parent);
                ((TabPage)p.Parent).Controls.Remove(p);

                if (t.TabPages.Count == 0)
                {
                    //hide the splitter bar along with the panel
                    if (t.Parent == panelDockLeft)
                        splitterLeft.Visible = false;
                    else if (t.Parent == panelDockRight)
                        splitterRight.Visible = false;

                    t.Parent.Visible = false;
                }

                FormFloat formFloat = new FormFloat(ref p, this, tp.Text);
                formFloat.Show();
                if (Cursor.Position.X - (formFloat.Width / 2) > 0)
                    formFloat.Left = Cursor.Position.X - (formFloat.Width / 2);
                else
                    formFloat.Left = 0;

                formFloat.Top = Cursor.Position.Y;
            }
        }

        /// <summary>
        /// Re-Dock the Panel checking whether it is closer to the right or left
        /// </summary>
        /// <param name="panel">The panel to re-dock</param>
        /// <param name="formLocation">Current Location of the Floating Form</param>
        /// <param name="tabName">The panels caption</param>
        internal void SetPanel(ref Panel panel, Point formLocation, string tabName)
        {
            if (formLocation.X < (this.Left + 200))
            {
                TabPage p = new TabPage(tabName);
                p.Controls.Add(panel);
                panel.Dock = DockStyle.Fill;
                this.panelDockLeft.TabControl.TabPages.Add(p);
                this.panelDockLeft.TabControl.Visible = true;
                panelDockLeft.Visible = true;
                splitterLeft.Visible = true;
                this.panelDockLeft.TabControl.SelectedTab = p;
            }
            else
            {
                TabPage p = new TabPage(tabName);
                p.Controls.Add(panel);
                panel.Dock = DockStyle.Fill;
                this.panelDockRight.TabControl.TabPages.Add(p);
                this.panelDockRight.TabControl.Visible = true;
                panelDockRight.Visible = true;
                splitterRight.Visible = true;
                this.panelDockRight.TabControl.SelectedTab = p;
            }
        }

        private void browseDataFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParseOutGoingCommand(null, "/run " + currentFolder);
        }

        private void browsePluginsFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParseOutGoingCommand(null, "/run " + pluginsFolder);
        }

        private void closeCurrentWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //close the current window
            mainChannelBar.CloseCurrentTab();
        }
        
        private void serverTreeImageMenu_Click(object sender, EventArgs e)
        {
            ParseOutGoingCommand(null, "/bg serverlist");
        }

        private void nickListImageMenu_Click(object sender, EventArgs e)
        {
            ParseOutGoingCommand(null, "/bg nicklist");
        }

        private void nickListImageRemoveMenu_Click(object sender, EventArgs e)
        {
            ParseOutGoingCommand(null, "/bg nicklist none");
        }

        private void serverTreeImageRemoveMenu_Click(object sender, EventArgs e)
        {
            ParseOutGoingCommand(null, "/bg serverlist none");
        }

        private void muteAllSoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //mute all sounds
            muteAllSounds = !muteAllSounds;
            muteAllSoundsToolStripMenuItem.Checked = muteAllSounds;
        }

        private void loadAPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //bring up a dialog box to open a new Plugin DLL File
            FileDialog fd = new OpenFileDialog();
            fd.DefaultExt = ".dll";
            fd.CheckFileExists = true;
            fd.CheckPathExists = true;
            fd.AddExtension = true;
            fd.AutoUpgradeEnabled = true;
            fd.Filter = "Plugin file (*.dll)|*.dll";
            fd.Title = "Which plugin file do you want to open?";
            fd.InitialDirectory = pluginsFolder;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                //currentScript = fd.FileName;
                //need to make sure the plugin is not already loaded
                foreach (ToolStripItem item in pluginsToolStripMenuItem.DropDownItems)
                {
                    if (item.ToolTipText.ToLower() == System.IO.Path.GetFileName(fd.FileName).ToLower())
                    {
                        return;
                    }
                }
                IPluginIceChat ipc = loadPlugin(fd.FileName);
                //initialize it
                if (ipc != null)
                {
                    System.Threading.Thread initPlugin = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(InitializePlugin));
                    initPlugin.Start(ipc);
                }

            }
        }

        private void multilineEditboxToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            multilineEditboxToolStripMenuItem.Checked = !multilineEditboxToolStripMenuItem.Checked;
            if (multilineEditboxToolStripMenuItem.Checked == true)
                inputPanel.ShowWideTextPanel = true;
            else
                inputPanel.ShowWideTextPanel = false;

            iceChatOptions.ShowMultilineEditbox = multilineEditboxToolStripMenuItem.Checked;
        }

        private void themeChoice_Click(object sender, EventArgs e)
        {

        }


        private void defaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //load the default theme
            
            colorsFile = FormMain.Instance.CurrentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatColors.xml";
            messagesFile = FormMain.Instance.CurrentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatMessages.xml";
            
            //check if the default files exist, if not, an exception error occurs
            LoadMessageFormat();
            
            LoadColors();
            
            //save the current theme
            iceChatOptions.CurrentTheme = "Default";
            SaveOptions();

            //update the colors
            fc_SaveColors(iceChatColors, iceChatMessages);

            foreach (ToolStripMenuItem t in themesToolStripMenuItem.DropDownItems)
                t.Checked = false;
            
            defaultToolStripMenuItem.Checked = true;

        }

        private void channelListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormList formList = new FormList();
            formList.SearchChannelCommand += new FormList.SearchChannelDelegate(formList_SearchChannelCommand);
            formList.ShowDialog(this);
        }

        private void formList_SearchChannelCommand(string command)
        {
            ParseOutGoingCommand(inputPanel.CurrentConnection, command);
        }

        private void VS2008ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.menuRenderer = new VS2008Renderer.MenuStripRenderer();
            this.toolStripRender = new VS2008Renderer.ToolStripRenderer();

            this.menuMainStrip.Renderer = menuRenderer;

            foreach (ToolStripMenuItem t in menuStylesToolStripMenuItem.DropDownItems)
                t.Checked = false;
            ((ToolStripMenuItem)sender).Checked = true;
        }

        private void Office2007ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.menuRenderer = new EasyRenderer.EasyRender();
            this.toolStripRender = new EasyRenderer.EasyRender();
            
            //this.menuRenderer = new ToolStripProfessionalRenderer(new OfficeNormalColorTable());
            //this.toolStripRender = new ToolStripProfessionalRenderer(new OfficeNormalColorTable());

            this.menuMainStrip.RenderMode = ToolStripRenderMode.ManagerRenderMode;   
            
            this.menuMainStrip.Renderer = menuRenderer;

            foreach (ToolStripMenuItem t in menuStylesToolStripMenuItem.DropDownItems)            
                t.Checked = false;            
            ((ToolStripMenuItem)sender).Checked = true;

        }

        private void DefaultRendererToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.menuMainStrip.RenderMode = ToolStripRenderMode.System;
            
            this.menuMainStrip.Renderer = null;

            this.menuRenderer = null;
            this.toolStripRender = null;

            foreach (ToolStripMenuItem t in menuStylesToolStripMenuItem.DropDownItems)
                t.Checked = false;
            ((ToolStripMenuItem)sender).Checked = true;

        }

        private string GetBackgroundImage()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = picturesFolder;
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Title = "Select Background Image";
            dialog.Filter = "Images (*.png;*.jpg)|*.png;*.jpg";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //returns the full path
                return dialog.FileName;
            }
            return "";
        }

        private void resizeWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //switch from tabs to windows
            //get the previous windowed mode
            //use Cascade for Default
            bool haveSize = false;

            for (int i = mainTabControl.Controls.Count - 1; i >= 0; i--)
            {
                if (mainTabControl.Controls[i].GetType() == typeof(IceTabPage))
                {
                    IceTabPage tab = ((IceTabPage)mainTabControl.Controls[i]);
                    tab.DockedForm = true;

                    FormWindow fw = new FormWindow(tab);
                    fw.Text = tab.TabCaption;
                    fw.MdiParent = this;

                    Point location = tab.WindowLocation;

                    fw.Show();

                    if (location != null)
                        fw.Location = location;

                    if (tab.WindowSize != null && tab.WindowSize.Height != 0)
                    {
                        fw.Size = tab.WindowSize;
                        haveSize = true;

                    }
                }
                else if (mainTabControl.Controls[i].GetType() == typeof(IceTabPageDCCFile))
                {
                    
                    IceTabPageDCCFile tab = ((IceTabPageDCCFile)mainTabControl.Controls[i]);
                    tab.DockedForm = true;

                    FormWindow fw = new FormWindow(tab);
                    fw.Text = tab.TabCaption;
                    fw.MdiParent = this;

                    Point location = tab.WindowLocation;

                    fw.Show();

                    if (location != null)
                        fw.Location = location;

                    if (tab.WindowSize != null && tab.WindowSize.Height != 0)
                    {
                        fw.Size = tab.WindowSize;
                        haveSize = true;
                    }

                }
            }
            
            
            //dont set this, if we have actual previous values for windowlocation
            if (!haveSize)
                this.LayoutMdi(mainTabControl.MdiLayout);

            mainTabControl.Visible = false;
            closeWindow.Visible = false;
        }

        internal void ReDockTabs()
        {
            //back to tabbed interface
            //gets called once per form
            try
            {

                IceTabPage selected = null;
                foreach (FormWindow child in this.MdiChildren)
                {
                    child.DisableActivate();
                }

                foreach (FormWindow child in this.MdiChildren)
                {
                    IceTabPage tab = child.DockedControl;
                    tab.DockedForm = false;

                    mainTabControl.AddTabPage(tab);

                    //if tab index == 0, its the current tab
                    if (tab.TabIndex == 0)
                        selected = tab;

                    child.Close();
                }

                mainTabControl.Visible = true;
                closeWindow.Visible = true;

                //what is the active tab??
                if (selected != null)
                    mainChannelBar.SelectTab(selected);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }

        }

        private void serverSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IceChat.Forms.PonyChatEdit me = new Forms.PonyChatEdit();
            me.Show();
        }
    }
}