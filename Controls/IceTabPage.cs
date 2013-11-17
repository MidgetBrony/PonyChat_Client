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
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

using IceChatPlugin;

namespace IceChat
{
    public class IceTabPage : Panel, IDisposable
    {
        private IRCConnection connection = null;

        private Hashtable nicks;

        //channel settings
        private string channelTopic = "";
        private string fullChannelMode = "";

        internal struct channelMode
        {
            public char mode;
            public bool set;
            public string param;
        }
        private Hashtable channelModes;

        private bool isFullyJoined = false;
        private bool hasChannelInfo = false;
        private FormChannelInfo channelInfoForm = null;

        private delegate TextWindow CurrentWindowDelegate();
        private delegate void ChangeTopicDelegate(string topic);
        private delegate void ChangeTextDelegate(string text);
        private delegate void AddDccChatDelegate(string message);
        private delegate void AddConsoleTabDelegate(IRCConnection connection);
        
        private delegate void AddChannelListDelegate(string channel, int users, string topic);
        private delegate void ClearChannelListDelegate();
        private delegate void AddConsoleDelegate(ConsoleTab tab, TextWindow window);

        private Panel panelTopic;
        
        private TextWindow textWindow;
        private TextWindow textTopic;
        private WindowType windowType;
        private FlickerFreeListView channelList;
        private TextBox searchText;
        private List<ListViewItem> listItems;
        private TabControl consoleTab;

        private FormMain.ServerMessageType lastMessageType;

        private bool _disableConsoleSelectChangedEvent = false;
        private bool _disableSounds = false;

        private bool gotWhoList = false;
        private bool gotNamesList = false;
        private bool channelHop = false;
        private bool channelListComplete = false;

        private TcpClient dccSocket;
        private TcpListener dccSocketListener;
        private Thread dccThread;
        private Thread listenThread;
        private bool keepListening;

        private System.Timers.Timer dccTimeOutTimer;

        private bool _flashTab;
        private int _flashValue;
        //private bool _soundOverLoad;
        private bool _eventOverLoad;
        private bool _dockControl;
        
        private System.Drawing.Size windowSize;
        private System.Drawing.Point windowLocation;

        public enum WindowType
        {
            Console = 1,
            Channel = 2,
            Query = 3,
            ChannelList = 4,
            DCCChat = 5,
            DCCFile = 6,
            Window = 7,
            Debug = 99
        }

        //private Image imgIcon;
        //private int iTabIndex;
        private string _tabCaption;

        public IceTabPage(WindowType windowType, string sCaption) 
        {
            if (windowType == WindowType.Channel)
            {
                InitializeChannel();
                textTopic.NoEmoticons = true;
                textTopic.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                this.textTopic.AppendText("Topic:", 3);
            }
            else if (windowType == WindowType.Query)
            {
                InitializeChannel();
                panelTopic.Visible = false;
            }
            else if (windowType == WindowType.Console)
            {
                InitializeConsole();
            }
            else if (windowType == WindowType.ChannelList)
            {
                InitializeChannelList();
            }
            else if (windowType == WindowType.DCCChat)
            {
                InitializeChannel();
                panelTopic.Visible = false;
            }
            else if (windowType == WindowType.Window)
            {
                InitializeChannel();
                panelTopic.Visible = false;
                textWindow.NoEmoticons = true;
            }
            else if (windowType == WindowType.Debug)
            {
                InitializeChannel();
                panelTopic.Visible = false;
                textWindow.NoEmoticons = true;
            }

            _tabCaption = sCaption;
            this.WindowStyle = windowType;
            
            nicks = new Hashtable();
            channelModes = new Hashtable();
            
            _flashTab = false;
            _flashValue = 0;
            _eventOverLoad = false;
            
            lastMessageType = FormMain.ServerMessageType.Default;
        }

        protected override void Dispose(bool disposing)
        {
            //this will dispose the TextWindow, making it close the log file
            if (this.windowType == WindowType.Channel || this.windowType == WindowType.Query)
                textWindow.Dispose();

            if (this.windowType == WindowType.DCCChat)
            {
                System.Diagnostics.Debug.WriteLine("disposing dcc chat");
                if (dccSocket != null)
                {
                    if (dccSocket.Connected)
                        dccSocket.Close();
                }
                
                if (dccSocketListener != null)
                    dccSocketListener.Stop();

                if (dccThread != null)
                {
                    if (dccThread.IsAlive)
                        dccThread.Abort();
                }
                if (listenThread != null)
                {
                    System.Diagnostics.Debug.WriteLine("abort listen thread");                    
                    listenThread.Abort();
                    if (listenThread.IsAlive)
                    {
                        System.Diagnostics.Debug.WriteLine("abort listen thread JOIN");
                        listenThread.Join();
                        System.Diagnostics.Debug.WriteLine("abort listen thread JOIN DONE");
                    }
                }
            }
        }

        /// <summary>
        /// Add a message to the Text Window for Selected Console Tab Connection
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="color"></param>
        internal void AddText(IRCConnection connection, string data, int color, bool scrollToBottom)
        {
            foreach (ConsoleTab t in consoleTab.TabPages)
            {
                if (t.Connection == connection)
                {
                    ((TextWindow)t.Controls[0]).AppendText(data, color);
                    if (scrollToBottom)
                        ((TextWindow)t.Controls[0]).ScrollToBottom();
                    return;
                }
            }
        }

        /// <summary>
        /// Add a new Tab/Connection to the Console Tab Control
        /// </summary>
        /// <param name="connection"></param>
        internal void AddConsoleTab(IRCConnection connection)
        {
            if (this.InvokeRequired)
            {
                AddConsoleTabDelegate act = new AddConsoleTabDelegate(AddConsoleTab);
                this.Invoke(act, new object[] { connection });
            }
            else            
            {
                ConsoleTab t;
                if (connection.ServerSetting.UseBNC)                
                    t = new ConsoleTab(connection.ServerSetting.BNCIP);
                else
                    t = new ConsoleTab(connection.ServerSetting.ServerName);

                t.Connection = connection;

                TextWindow w = new TextWindow();
                w.Dock = DockStyle.Fill;
                w.Font = new System.Drawing.Font(FormMain.Instance.IceChatFonts.FontSettings[0].FontName, FormMain.Instance.IceChatFonts.FontSettings[0].FontSize);
                w.IRCBackColor = FormMain.Instance.IceChatColors.ConsoleBackColor;
                w.NoEmoticons = true;
                
                AddConsole(t, w);
            }
        }

        private void AddConsole(ConsoleTab tab, TextWindow window)
        {
            if (this.InvokeRequired)
            {
                AddConsoleDelegate add = new AddConsoleDelegate(AddConsole);
                this.Invoke(add, new object[] { tab, window });
            }
            else
            {
                tab.Controls.Add(window);
                if (FormMain.Instance.IceChatOptions.LogConsole)
                    window.SetLogFile();

                consoleTab.TabPages.Add(tab);
                consoleTab.SelectedTab = tab;
            }
        }

        /// <summary>
        /// Temporary Method to create a NULL Connection for the Welcome Tab in the Console
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="serverName"></param>
        internal void AddConsoleTab(string serverName)
        {
            //this is only used for now, to show the "Welcome" Tab
            ConsoleTab t = new ConsoleTab(serverName);

            TextWindow w = new TextWindow();
            w.Dock = DockStyle.Fill;
            w.Font = new System.Drawing.Font(FormMain.Instance.IceChatFonts.FontSettings[0].FontName, FormMain.Instance.IceChatFonts.FontSettings[0].FontSize);
            w.IRCBackColor = FormMain.Instance.IceChatColors.ConsoleBackColor;

            t.Controls.Add(w);
            consoleTab.TabPages.Add(t);
            consoleTab.SelectedTab = t;

        }

        internal Size WindowSize
        {
            get { return windowSize; }
            set { windowSize = value; }
        }

        internal System.Drawing.Point WindowLocation
        {
            get { return windowLocation; }
            set { windowLocation = value; }
        }

        internal bool DockedForm
        {
            get { return _dockControl; }
            set { _dockControl = value; }
        }

        internal bool NickExists(string nick)
        {
            return nicks.ContainsKey(nick);
        }

        internal void UpdateChannelMode(char mode, bool addMode)
        {
            try
            {
                channelMode c = new channelMode();
                c.mode = mode;
                c.set = addMode;
                c.param = null;

                if (channelModes.Contains(mode))
                {
                    if (addMode)
                        channelModes[mode] = c;
                    else
                        channelModes.Remove(mode);
                }
                else
                {
                    channelModes.Add(mode, c);
                }

                string modes = "";
                string prms = " ";
                foreach (channelMode cm in channelModes.Values)
                {
                    modes += cm.mode.ToString();
                    if (cm.param != null)
                        prms += cm.param + " ";
                }

                if (modes.Trim().Length > 0)
                    ChannelModes = "+" + modes.Trim() + prms.TrimEnd();
                else
                    ChannelModes = "";
            }
            catch (Exception e)
            {
                FormMain.Instance.WriteErrorFile(FormMain.Instance.InputPanel.CurrentConnection,"IceTabPage UpdateChannelMode", e);
            }
        }

        internal void UpdateChannelMode(char mode, string param, bool addMode)
        {
            try
            {
                channelMode c = new channelMode();
                c.mode = mode;
                c.set = addMode;
                c.param = param;

                if (channelModes.Contains(mode))
                {
                    if (addMode)
                        channelModes[mode] = c;
                    else
                        channelModes.Remove(mode);
                }
                else
                {
                    if (addMode)
                        channelModes.Add(mode, c);
                }

                string modes = "";
                string prms = " ";
                foreach (channelMode cm in channelModes.Values)
                {
                    modes += cm.mode.ToString();
                    if (cm.param != null)
                        prms += cm.param + " ";
                }
                if (modes.Trim().Length > 0)
                    ChannelModes = "+" + modes.Trim() + prms.TrimEnd();
                else
                    ChannelModes = "";
            }
            catch (Exception e)
            {
                FormMain.Instance.WriteErrorFile(FormMain.Instance.InputPanel.CurrentConnection,"IceTabPage UpdateChannelMode2", e);
            }
        }

        internal void UpdateNick(string nick, string mode, bool addMode)
        {
            string justNick = nick;

            for (int i = 0; i < connection.ServerSetting.StatusModes[1].Length; i++)
                justNick = justNick.Replace(connection.ServerSetting.StatusModes[1][i].ToString(), string.Empty);

            if (nicks.ContainsKey(justNick))
            {
                User u = (User)nicks[justNick];

                for (int i = 0; i < connection.ServerSetting.StatusModes[1].Length; i++)
                    if (mode == connection.ServerSetting.StatusModes[1][i].ToString())
                        u.Level[i] = addMode;

                if (FormMain.Instance.CurrentWindow == this)
                    FormMain.Instance.NickList.RefreshList(this);
            }
        }


        internal User GetNick(string nick)
        {
            for (int i = 0; i < connection.ServerSetting.StatusModes[1].Length; i++)
                nick = nick.Replace(connection.ServerSetting.StatusModes[1][i].ToString(), string.Empty);

            if (nicks.ContainsKey(nick))
                return (User)nicks[nick];

            return null;
        }

        internal User GetNick(int nickNumber)
        {
            if (nickNumber <= nicks.Count)
            {
                int i = 1;
                foreach (User u in nicks.Values)
                {
                    if (nickNumber == i)
                        return u;
                    i++;
                }
            }
            return null;
        }

        internal void AddNick(string nick, bool refresh)
        {
            //replace any user modes from the nick
            string justNick = nick;
            if (connection != null)
            for (int i = 0; i < connection.ServerSetting.StatusModes[1].Length; i++)
                justNick = justNick.Replace(connection.ServerSetting.StatusModes[1][i].ToString(), string.Empty);

            if (nicks.ContainsKey(justNick))
                return;

            User u = new User(nick, connection);

            nicks.Add(justNick, u);
            if (refresh)
            {
                if (FormMain.Instance.CurrentWindow == this)
                    FormMain.Instance.NickList.RefreshList(this);
            }
            
        }

        internal void RemoveNick(string nick)
        {
            nicks.Remove(nick);
            if (FormMain.Instance.CurrentWindow == this)
                FormMain.Instance.NickList.RefreshList(this);
            
        }

        internal void ClearNicks()
        {
            nicks.Clear();
        }

        internal Hashtable Nicks
        {
            get { return nicks; }
        }

        internal TextWindow TextWindow
        {
            get { return this.textWindow; }
        }

        internal TextWindow TopicWindow
        {
            get { return this.textTopic; }
        }

        internal bool EventOverLoad
        {
            get { return _eventOverLoad; }
            set { 
                _eventOverLoad = value;
                if (this.windowType == WindowType.Channel)
                {
                    ChannelSetting cs = FormMain.Instance.ChannelSettings.FindChannel(this.TabCaption);
                    if (cs != null)
                    {
                        cs.EventsDisable = value;
                    }
                    else
                    {
                        ChannelSetting cs1 = new ChannelSetting();
                        cs1.EventsDisable = value;
                        cs1.ChannelName = this.TabCaption;
                        FormMain.Instance.ChannelSettings.AddChannel(cs1);
                    }
                    FormMain.Instance.SaveChannelSettings();
                }
            }
        }

        //whether to play sound events for this window
        internal bool DisableSounds
        {
            get { return _disableSounds; }
            set
            {
                _disableSounds = value;
                if (this.windowType == WindowType.Channel)
                {
                    ChannelSetting cs = FormMain.Instance.ChannelSettings.FindChannel(this.TabCaption);
                    if (cs != null)
                    {
                        cs.SoundsDisable = value;
                    }
                    else
                    {
                        ChannelSetting cs1 = new ChannelSetting();
                        cs1.SoundsDisable = value;
                        cs1.ChannelName = this.TabCaption;
                        cs1.NetworkName = this.connection.ServerSetting.NetworkName;
                        FormMain.Instance.ChannelSettings.AddChannel(cs1);
                    }
                    FormMain.Instance.SaveChannelSettings();

                }

            }
        }

        /*
        internal bool SoundOverLoad
        {
            get { return _soundOverLoad; }
            set { _soundOverLoad = value; }
        }
        */
        internal bool IsConnected
        {
            get
            {
                if (dccSocket != null)
                    return dccSocket.Connected;
                else
                    return false;
            }
        }

        internal bool GotWhoList
        {
            get { return gotWhoList; }
            set { gotWhoList = value; }
        }

        internal bool ChannelHop
        {
            get { return channelHop; }
            set { channelHop = value; }
        }

        internal bool GotNamesList
        {
            get { return gotNamesList; }
            set { gotNamesList = value; }
        }

        internal bool FlashTab
        {
            get { return _flashTab; }
            set { _flashTab = value; }
        }

        internal int FlashValue
        {
            get
            {
                _flashValue++;
                if (_flashValue == 2)
                    _flashValue = 0;

                return _flashValue;
            }
        }
        /*
        internal int CheckFlashValue
        {
            get
            {
                return _flashValue;
            }
        }
        */
        internal void DisconnectDCC()
        {
            if (dccSocket != null)
                dccSocket.Close();
            else if (dccSocketListener != null)
                dccSocketListener.Stop();
        }

        internal void ChannelSettings(string network)
        {
            //System.Diagnostics.Debug.WriteLine("find:" + network);
            if (windowType == WindowType.Channel)
            {
                ChannelSetting cs = FormMain.Instance.ChannelSettings.FindChannel(_tabCaption);
                if (cs != null)
                {
                    _eventOverLoad = cs.EventsDisable;
                    _disableSounds = cs.SoundsDisable;
                    textWindow.NoColorMode = cs.NoColorMode;
                }
            }
        }

        internal void dccTimeOutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            string msg = FormMain.Instance.GetMessageFormat("DCC Chat Timeout");
            msg = msg.Replace("$nick", _tabCaption);

            PluginArgs args = new PluginArgs(this.textWindow, "", _tabCaption, "", msg);
            args.Connection = this.connection;

            foreach (IPluginIceChat ipc in FormMain.Instance.IceChatPlugins)
            {
                if (ipc.Enabled == true)
                    args = ipc.DCCChatTimeOut(args);
            }

            textWindow.AppendText(args.Message, 1);
            this.LastMessageType = FormMain.ServerMessageType.ServerMessage;

            System.Diagnostics.Debug.WriteLine("timed out");

            dccTimeOutTimer.Stop();
            
            System.Diagnostics.Debug.WriteLine("timed out 1");
            dccSocketListener.Stop();
            
            System.Diagnostics.Debug.WriteLine("timed out 2");
            keepListening = false;

            System.Diagnostics.Debug.WriteLine("timed out 3");

            //listenThread.Abort();
        }


        internal void RequestDCCChat()
        {
            //send out a dcc chat request
            string localIP = "";
            if (FormMain.Instance.IceChatOptions.DCCLocalIP != null && FormMain.Instance.IceChatOptions.DCCLocalIP.Length > 0)
            {
                localIP = IPAddressToLong(IPAddress.Parse(FormMain.Instance.IceChatOptions.DCCLocalIP)).ToString();
            }
            else
            {
                if (connection.ServerSetting.LocalIP == null || connection.ServerSetting.LocalIP.ToString().Length == 0)
                {
                    //error. no local IP found
                    FormMain.Instance.WindowMessage(connection, _tabCaption, "DCC ERROR, no Router/Firewall IP Address specified in DCC Settings", 4, true);
                    this.LastMessageType = FormMain.ServerMessageType.ServerMessage; 
                    return;
                }
                else
                {
                    localIP = IPAddressToLong(connection.ServerSetting.LocalIP).ToString();
                }
            }
            
            
            Random port = new Random();
            int p = port.Next(FormMain.Instance.IceChatOptions.DCCPortLower, FormMain.Instance.IceChatOptions.DCCPortUpper);

            dccSocketListener = new TcpListener(new IPEndPoint(IPAddress.Any, Convert.ToInt32(p)));

            object args = new object[2] { localIP, p };

            listenThread = new Thread(new ParameterizedThreadStart(ListenForConnection));
            listenThread.Name = "DCCListenThread";
            listenThread.Start(args);
            
            System.Diagnostics.Debug.WriteLine("dcc chat outgoing :" + localIP.ToString() + ":port:" + p.ToString());
            //connection.SendData("PRIVMSG " + _tabCaption + " :DCC CHAT chat " + localIP + " " + p.ToString() + "");
            dccTimeOutTimer = new System.Timers.Timer();
            dccTimeOutTimer.Interval = 1000 * FormMain.Instance.IceChatOptions.DCCChatTimeOut;
            dccTimeOutTimer.Elapsed += new System.Timers.ElapsedEventHandler(dccTimeOutTimer_Elapsed);
            dccTimeOutTimer.Start();
        }

        private void ListenForConnection(object portIP)
        {
            this.dccSocketListener.Start();

            Array argArray = new object[2];
            argArray = (Array)portIP;

            string localIP = (string)argArray.GetValue(0);
            int port = (int)argArray.GetValue(1);

            connection.SendData("PRIVMSG " + _tabCaption + " :\x0001DCC CHAT chat " + localIP + " " + port.ToString() + "\x0001");
            System.Diagnostics.Debug.WriteLine("PRIVMSG " + _tabCaption + " :\x0001DCC CHAT chat " + localIP + " " + port.ToString() + "\x0001");
            System.Diagnostics.Debug.WriteLine("start listening:" + dccSocketListener.Pending());
            keepListening = true;

            while (keepListening)
            {
                try
                {
                    dccSocket = dccSocketListener.AcceptTcpClient();
                    
                    keepListening = false;
                    System.Diagnostics.Debug.WriteLine("accepted");
                    dccSocketListener.Stop();
                    dccTimeOutTimer.Stop();
                    

                    string msg = FormMain.Instance.GetMessageFormat("DCC Chat Connect");
                    msg = msg.Replace("$nick", _tabCaption);

                    PluginArgs args = new PluginArgs(this.textWindow, "", this.connection.ServerSetting.CurrentNickName, "", msg);
                    args.Connection = this.connection;

                    foreach (IPluginIceChat ipc in FormMain.Instance.IceChatPlugins)
                    {
                        if (ipc.Enabled == true)
                            args = ipc.DCCChatConnected(args);
                    }

                    textWindow.AppendText(args.Message, 1);
                    this.LastMessageType = FormMain.ServerMessageType.ServerMessage;

                    dccThread = new Thread(new ThreadStart(GetDCCData));
                    dccThread.Name = "DCCDataThread";
                    dccThread.Start();
                    break;

                }
                catch (ThreadAbortException tx)
                {
                    System.Diagnostics.Debug.WriteLine("listen thread exception:" + tx.Message + ":" + tx.StackTrace);
                }
                catch (SocketException se)
                {
                    System.Diagnostics.Debug.WriteLine("listen thread socket exception:" + se.Message + ":" + se.StackTrace);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("listen exception:" + ex.Message + ":" + ex.StackTrace);
                }
            }
            System.Diagnostics.Debug.WriteLine("finished listening on port " + port + ":" + keepListening);
        }

        internal void StartDCCChat(string nick, string ip, string port)
        {
            dccSocket = new TcpClient();
            System.Diagnostics.Debug.WriteLine("start dcc chat " + ip + " on port " + port);
            try
            {
                IPAddress ipAddr = LongToIPAddress(ip);
                IPEndPoint ep = new IPEndPoint(ipAddr, Convert.ToInt32(port));
                System.Diagnostics.Debug.WriteLine("attempting to connect on port " + port + " from address " + ipAddr.ToString() + ":" + ipAddr);

                dccSocket.Connect(ep);
                if (dccSocket.Connected)
                {
                    string msg = FormMain.Instance.GetMessageFormat("DCC Chat Connect");
                    msg = msg.Replace("$nick", nick).Replace("$ip", ip).Replace("$port", port);

                    PluginArgs args = new PluginArgs(this.textWindow, "", nick, ip, msg);
                    args.Extra = port;
                    args.Connection = this.connection;

                    foreach (IPluginIceChat ipc in FormMain.Instance.IceChatPlugins)
                    {
                        if (ipc.Enabled == true)
                            args = ipc.DCCChatConnected(args);
                    }
                    
                    if (dccTimeOutTimer != null)
                        dccTimeOutTimer.Stop();

                    textWindow.AppendText(args.Message, 1);
                    this.LastMessageType = FormMain.ServerMessageType.ServerMessage;

                    dccThread = new Thread(new ThreadStart(GetDCCData));
                    dccThread.Name = "DCCChatThread_" + nick;
                    dccThread.Start();
                }
            }
            catch (SocketException se)
            {
                System.Diagnostics.Debug.WriteLine(se.Message + ":" + se.StackTrace);
                textWindow.AppendText(se.Message, 4);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + ":" + e.StackTrace);
                textWindow.AppendText(e.Message + ":" + e.StackTrace, 4);
            }
        }

        internal void SendDCCData(string message)
        {
            if (dccSocket != null)
            {
                if (dccSocket.Connected)
                {
                    NetworkStream ns = dccSocket.GetStream();
                    ASCIIEncoding encoder = new ASCIIEncoding();
                    byte[] buffer = encoder.GetBytes(message + "\n");
                    try
                    {
                        ns.Write(buffer, 0, buffer.Length);
                        ns.Flush();
                    }
                    catch { }
                }
            }
        }

        private void GetDCCData()
        {
            while (true)
            {
                try
                {
                    int buffSize = 0;
                    byte[] buffer = new byte[8192];
                    NetworkStream ns = dccSocket.GetStream();
                    buffSize = dccSocket.ReceiveBufferSize;
                    int bytesRead = ns.Read(buffer, 0, buffSize);
                    Decoder d = Encoding.GetEncoding(this.connection.ServerSetting.Encoding).GetDecoder();
                    char[] chars = new char[buffSize];
                    int charLen = d.GetChars(buffer, 0, buffSize, chars, 0);

                    System.String strData = new System.String(chars);
                    if (bytesRead == 0)
                    {
                        //we have a disconnection
                        break;
                    }
                    //cut off the null chars
                    strData = strData.Substring(0, strData.IndexOf(Convert.ToChar(0x0).ToString()));

                    AddDCCMessage(strData);
                }
                catch (Exception)
                {
                    //we have an error
                    break;
                }
            }

            string msg = FormMain.Instance.GetMessageFormat("DCC Chat Disconnect");
            msg = msg.Replace("$nick", _tabCaption);

            PluginArgs args = new PluginArgs(this.textWindow, "", _tabCaption, "", msg);
            args.Connection = this.connection;

            foreach (IPluginIceChat ipc in FormMain.Instance.IceChatPlugins)
            {
                if (ipc.Enabled == true)
                    args = ipc.DCCChatClosed(args);
            }
            
            textWindow.AppendText(msg, 1);
            this.LastMessageType = FormMain.ServerMessageType.ServerMessage;

            System.Diagnostics.Debug.WriteLine("dcc chat disconnect");
            dccSocket.Close();
        }

        private void AddDCCMessage(string message)
        {
            if (this.InvokeRequired)
            {
                AddDccChatDelegate a = new AddDccChatDelegate(AddDCCMessage);
                this.Invoke(a, new object[] { message });
            }
            else
            {
                string[] lines = message.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    if (line[0] != (char)0 && line[0] != (char)1)
                    {
                        string msg = FormMain.Instance.GetMessageFormat("DCC Chat Message");
                        msg = msg.Replace("$nick", _tabCaption);
                        msg = msg.Replace("$message", line);

                        PluginArgs args = new PluginArgs(this.textWindow, "", _tabCaption, "", msg);
                        args.Connection = this.connection;

                        foreach (IPluginIceChat ipc in FormMain.Instance.IceChatPlugins)
                        {
                            if (ipc.Enabled == true)
                                args = ipc.DCCChatMessage(args);
                        }

                        textWindow.AppendText(args.Message, 1);
                        this.LastMessageType = FormMain.ServerMessageType.Message;

                    }
                    else if (line[0] == (char)1)
                    {
                        //action
                        string action = line.Substring(8);
                        action = action.Substring(0, action.Length - 1);
                        
                        string msg = FormMain.Instance.GetMessageFormat("DCC Chat Action");
                        msg = msg.Replace("$nick", _tabCaption);
                        msg = msg.Replace("$message", action);

                        PluginArgs args = new PluginArgs(this.textWindow, "", _tabCaption, "", msg);
                        args.Connection = this.connection;

                        foreach (IPluginIceChat ipc in FormMain.Instance.IceChatPlugins)
                        {
                            if (ipc.Enabled == true)
                                args = ipc.DCCChatMessage(args);
                        }

                        textWindow.AppendText(args.Message, 1);
                        this.LastMessageType = FormMain.ServerMessageType.Action;

                    }
                }
            }
        }

        private long IPAddressToLong(IPAddress ip)
        {
            byte[] bytes = ip.GetAddressBytes();
            return (long)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]); 
        }

        private IPAddress LongToIPAddress(string longIP)
        {
            byte[] quads = BitConverter.GetBytes(long.Parse(longIP, System.Globalization.CultureInfo.InvariantCulture));
            return IPAddress.Parse(quads[3] + "." + quads[2] + "." + quads[1] + "." + quads[0]);
        }

        private long NetworkUnsignedLong(long hostOrderLong)
        {
            long networkLong = IPAddress.HostToNetworkOrder(hostOrderLong);
            //Network order has the octets in reverse order starting with byte 7
            //To get the correct string simply shift them down 4 bytes
            //and zero out the first 4 bytes.
            return (networkLong >> 32) & 0x00000000ffffffff;
        }

        /// <summary>
        /// Return the Connection for the Current Selected in the Console Tab Control
        /// </summary>
        internal IRCConnection CurrentConnection
        {
            get
            {
                return ((ConsoleTab)consoleTab.SelectedTab).Connection;
            }
        }

        internal IRCConnection Connection
        {
            get
            {
                return this.connection;
            }
            set
            {
                this.connection = value;
            }
        }

        internal WindowType WindowStyle
        {
            get
            {
                return windowType;
            }
            set
            {
                windowType = value;
                if (windowType == WindowType.Console)
                {
                    //nada
                }
                else if (windowType == WindowType.Channel)
                {
                    panelTopic.Visible = true;
                    textWindow.IRCBackColor = FormMain.Instance.IceChatColors.ChannelBackColor;
                    textTopic.IRCBackColor = FormMain.Instance.IceChatColors.ChannelBackColor;
                }
                else if (windowType == WindowType.Query)
                {
                    textWindow.IRCBackColor = FormMain.Instance.IceChatColors.QueryBackColor;
                }
                else if (windowType == WindowType.ChannelList)
                {
                    //nada
                }
                else if (windowType == WindowType.DCCChat)
                {
                    textWindow.IRCBackColor = FormMain.Instance.IceChatColors.QueryBackColor;
                }
                else if (windowType == WindowType.DCCFile)
                {
                    //nada
                }
                else if (windowType == WindowType.Window)
                {
                    //nada
                }
                else if (windowType == WindowType.Debug)
                {
                    //nada
                }

            }
        }

        internal string ChannelModes
        {
            get
            {
                return fullChannelMode;
            }
            set
            {
                fullChannelMode = value;
            }
        }

        internal Hashtable ChannelModesHash
        {
            get
            {
                return channelModes;
            }
        }

        internal string ChannelTopic
        {
            get
            {
                return channelTopic;
            }
            set
            {
                channelTopic = value;
                UpdateTopic(value);
            }
        }

        internal bool IsFullyJoined
        {
            get
            {
                return isFullyJoined;
            }
            set
            {
                isFullyJoined = value;
            }
        }

        internal bool HasChannelInfo
        {
            get
            {
                return hasChannelInfo;
            }
            set
            {
                hasChannelInfo = value;
            }
        }

        internal FormChannelInfo ChannelInfoForm
        {
            get
            {
                return channelInfoForm;
            }
            set
            {
                channelInfoForm = value;
            }
        }

        internal bool ChannelListComplete
        {
            get
            {
                return channelListComplete;
            }
            set
            {
                channelListComplete = value;
            }
        }

        internal bool ShowTopicBar
        {
            get
            {
                return this.panelTopic.Visible;
            }
            set
            {
                this.panelTopic.Visible = value;
            }
        }


        internal TabControl ConsoleTab
        {
            get { return consoleTab; }
        }

        internal FormMain.ServerMessageType LastMessageType
        {
            get
            {
                return lastMessageType;
            }
            set
            {
                if (lastMessageType != value)
                {
                    //check if we are the current window or not
                    if (this == FormMain.Instance.CurrentWindow)
                    {
                        lastMessageType = FormMain.ServerMessageType.Default;
                        return;
                    }
                    
                    // do not change if already a New Message
                    if (lastMessageType != FormMain.ServerMessageType.Message)
                    {
                        if (this._eventOverLoad == false)
                        {
                            lastMessageType = value;
                            FormMain.Instance.ChannelBar.Invalidate();
                            FormMain.Instance.ServerTree.Invalidate();
                        }
                    }
                }
            }
        }

        internal TextWindow CurrentConsoleWindow()
        {
            if (this.InvokeRequired)
            {
                CurrentWindowDelegate cwd = new CurrentWindowDelegate(CurrentConsoleWindow);
                return (TextWindow)this.Invoke(cwd, new object[] { });
            }
            else
            {
                return (TextWindow)consoleTab.SelectedTab.Controls[0];
            }
        }

        internal int TotalChannels
        {
            get { return this.channelList.Items.Count; }
        }



        internal void ClearChannelList()
        {
            if (this.InvokeRequired)
            {
                ClearChannelListDelegate c = new ClearChannelListDelegate(ClearChannelList);
                this.Invoke(c, new object[] { });
            }
            else
                this.channelList.Items.Clear();        
        }

        private void UpdateText(string text)
        {
            if (this.InvokeRequired)
            {
                ChangeTextDelegate c = new ChangeTextDelegate(UpdateText);
                this.Invoke(c, new object[] { text });
            }
            else
            {
                this.Text = text;
                this.Update();
            }
        }

        private void UpdateTopic(string topic)
        {
            if (this.InvokeRequired)
            {
                ChangeTopicDelegate c = new ChangeTopicDelegate(UpdateTopic);
                this.Invoke(c, new object[] { topic });
            }
            else
            {
                channelTopic = topic;
                textTopic.ClearTextWindow();
                string msgt = FormMain.Instance.GetMessageFormat("Channel Topic Text");
                msgt = msgt.Replace("$channel", this.TabCaption);
                msgt = msgt.Replace("$topic", topic);
                textTopic.AppendText(msgt, 1);
            }   
        }

        /// <summary>
        /// Add the specified channel list data to the ListView
        /// </summary>
        /// <param name="channel">Channel Name</param>
        /// <param name="users">The number of users</param>
        /// <param name="topic">The channel topic</param>
        internal void AddChannelList(string channel, int users, string topic)
        {
            if (this.InvokeRequired)
            {
                AddChannelListDelegate a = new AddChannelListDelegate(AddChannelList);
                this.Invoke(a, new object[] { channel, users, topic });
            }
            else
            {
                ListViewItem lvi = new ListViewItem(channel);
                lvi.ToolTipText = topic;
                lvi.SubItems.Add(users.ToString());
                lvi.SubItems.Add(topic);
                channelList.Items.Add(lvi);
            }  
        }

        private void OnControlAdded(object sender, ControlEventArgs e)
        {
            e.Control.Dock = DockStyle.Fill;
        }

        public string TabCaption
        {
            get { return _tabCaption; }
            set { this._tabCaption = value; }
        }

        internal void SelectConsoleTab(ConsoleTab c)
        {
            _disableConsoleSelectChangedEvent = true;
            consoleTab.SelectedTab = c;
            StatusChange();
            _disableConsoleSelectChangedEvent = false;
        }

        internal void ResizeTopicFont(string fontName, float fontSize)
        {
            //resize the font for the topic, and make the box size accordingly
            textTopic.Font = new Font(fontName, fontSize);
            this.panelTopic.Size = new System.Drawing.Size(panelTopic.Width,(int) fontSize * 2);
            this.panelTopic.Visible = FormMain.Instance.IceChatOptions.ShowTopic;
        }

        private void StatusChange()
        {
            FormMain.Instance.InputPanel.CurrentConnection = ((ConsoleTab)consoleTab.SelectedTab).Connection;

            string network = "";
            if (((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting.NetworkName.Length > 0)
                network = " (" + ((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting.NetworkName + ")";

            if (((ConsoleTab)consoleTab.SelectedTab).Connection.IsConnected)
            {
                if (((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting.UseBNC)
                    FormMain.Instance.StatusText(((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting.CurrentNickName + " connected to " + ((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting.BNCIP);
                else
                {
                    if (((ConsoleTab)consoleTab.SelectedTab).Connection.IsFullyConnected == true)
                        FormMain.Instance.StatusText(((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting.CurrentNickName + " connected to " + ((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting.RealServerName + network);
                    else
                        FormMain.Instance.StatusText(((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting.CurrentNickName + " connecting to " + ((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting.ServerName);
                }
            }
            else
            {
                if (((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting.UseBNC)
                    FormMain.Instance.StatusText(((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting.NickName + " disconnected from " + ((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting.BNCIP);
                else
                    FormMain.Instance.StatusText(((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting.NickName + " disconnected from " + ((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting.ServerName + network);
            }
        }

        private void OnTabConsoleSelectedIndexChanged(object sender, EventArgs e)
        {
            ((TextWindow)(consoleTab.SelectedTab.Controls[0])).resetUnreadMarker(); 
            
            if (consoleTab.TabPages.IndexOf(consoleTab.SelectedTab) != 0 && !_disableConsoleSelectChangedEvent)
            {
                StatusChange();
                
                //highlite the proper item in the server tree
                FormMain.Instance.ServerTree.SelectTab(((ConsoleTab)consoleTab.SelectedTab).Connection.ServerSetting, false);                
            }
            else
            {
                FormMain.Instance.InputPanel.CurrentConnection = null;
                FormMain.Instance.StatusText("Welcome to " + FormMain.ProgramID + " " + FormMain.VersionID);
            }            
        }

        private void OnTabConsoleMouseUp(object sender, MouseEventArgs e)
        {
            FormMain.Instance.FocusInputBox();
        }

        /// <summary>
        /// Checks if Left Mouse Button is Pressed by the "X" button
        /// Quits Server if Server is Connected
        /// Closes Server Tab if Server is Disconnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTabConsoleMouseDown(object sender, MouseEventArgs e)
        {                        
            if (e.Button == MouseButtons.Left)
            {
                for (int i = consoleTab.TabPages.Count - 1; i >= 0; i--)
                {
                    if (consoleTab.GetTabRect(i).Contains(e.Location) && i == consoleTab.SelectedIndex)
                    {
                        if (((ConsoleTab)consoleTab.TabPages[i]).Connection != null)
                        {
                            if (e.Location.X > consoleTab.GetTabRect(i).Right - 14)
                            {
                                if (((ConsoleTab)consoleTab.TabPages[i]).Connection.IsConnected)
                                {
                                    if (((ConsoleTab)consoleTab.TabPages[i]).Connection.IsFullyConnected)
                                    {
                                        ((ConsoleTab)consoleTab.TabPages[i]).Connection.AttemptReconnect = false;
                                        FormMain.Instance.ParseOutGoingCommand(((ConsoleTab)consoleTab.TabPages[i]).Connection, "//quit " + ((ConsoleTab)consoleTab.TabPages[i]).Connection.ServerSetting.QuitMessage);
                                        return;
                                    }
                                }
                                //close all the windows related to this tab
                                System.Diagnostics.Debug.WriteLine("removing:" + i);
                                RemoveConsoleTab(i);
                                return;
                            }
                        }
                    }
                }
            }
        }

        internal void RemoveConsoleTab(int index)
        {
            FormMain.Instance.CloseAllWindows(((ConsoleTab)consoleTab.TabPages[index]).Connection);
            //remove the server connection from the collection
            ((ConsoleTab)consoleTab.TabPages[index]).Connection.Dispose();
            FormMain.Instance.ServerTree.ServerConnections.Remove(((ConsoleTab)consoleTab.TabPages[index]).Connection.ServerSetting.ID);
            consoleTab.TabPages.RemoveAt(consoleTab.TabPages.IndexOf(consoleTab.TabPages[index]));
        }


        private void OnControlRemoved(object sender, ControlEventArgs e)
        {
            //this will close the log file for the particular server tab closed
            try
            {
                if (e.Control.GetType() == typeof(ConsoleTab))
                {
                    if (((ConsoleTab)e.Control).Connection.ServerSetting.ID > 50000)
                    {
                        //temporary server, remove it from ServerTree
                        FormMain.Instance.ServerTree.ServersCollection.RemoveServer(((ConsoleTab)e.Control).Connection.ServerSetting);
                    }
                    ((TextWindow)((ConsoleTab)e.Control).Controls[0]).Dispose();
                    FormMain.Instance.ServerTree.Invalidate();
                }
            }
            catch { }
        }

        private void OnTabConsoleDrawItem(object sender, DrawItemEventArgs e)
        {
            string name = consoleTab.TabPages[e.Index].Text;
            Rectangle bounds = e.Bounds;
            e.Graphics.FillRectangle(new SolidBrush(Color.White), bounds);

            if (e.Index == consoleTab.SelectedIndex)
            {
                bounds.Offset(4, 2);
                e.Graphics.DrawString(name, this.Font, new SolidBrush(Color.Red), bounds);
                bounds.Offset(0, -1);
            }
            else
            {
                bounds.Offset(2, 3);
                e.Graphics.DrawString(name, this.Font, new SolidBrush(Color.Black), bounds);
                bounds.Offset(4, -2);
            }
            if (e.Index != 0 && e.Index == consoleTab.SelectedIndex)
            {
                System.Drawing.Image icon = StaticMethods.LoadResourceImage("CloseButton.png");
                e.Graphics.DrawImage(icon, bounds.Right - 20, bounds.Top + 4, 12, 12);
                icon.Dispose();
            }

        }

        private void OnTabConsoleSelectingTab(object sender, TabControlCancelEventArgs e)
        {
            if (consoleTab.GetTabRect(e.TabPageIndex).Contains(consoleTab.PointToClient(Cursor.Position)) && e.TabPageIndex != 0)
            {
                if (this.PointToClient(Cursor.Position).X > consoleTab.GetTabRect(e.TabPageIndex).Right - 14)
                    e.Cancel = true;
            }
        }
        /// <summary>
        /// Create the Console Tab
        /// </summary>
        private void InitializeConsole()
        {
            this.SuspendLayout();
            // 
            // consoleTab
            // 
            this.consoleTab = new TabControl();
            this.consoleTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.consoleTab.Font = new System.Drawing.Font("Verdana", 10F);
            this.consoleTab.Location = new System.Drawing.Point(0, 0);
            this.consoleTab.Name = "consoleTab";
            this.consoleTab.SelectedIndex = 0;
            this.consoleTab.Size = new System.Drawing.Size(200, 100);
            this.consoleTab.TabIndex = 0;
            
            // 
            // ConsoleTabWindow
            // 
            this.Controls.Add(this.consoleTab);
            this.ResumeLayout(false);
            
            consoleTab.DrawMode = TabDrawMode.OwnerDrawFixed;
            consoleTab.SizeMode = TabSizeMode.Normal;
            consoleTab.SelectedIndexChanged += new EventHandler(OnTabConsoleSelectedIndexChanged);
            consoleTab.MouseUp += new MouseEventHandler(OnTabConsoleMouseUp);
            consoleTab.MouseDown += new MouseEventHandler(OnTabConsoleMouseDown);
            consoleTab.DrawItem += new DrawItemEventHandler(OnTabConsoleDrawItem);
            consoleTab.Selecting += new TabControlCancelEventHandler(OnTabConsoleSelectingTab);

            consoleTab.ControlRemoved += new ControlEventHandler(OnControlRemoved);

        }

        /// <summary>
        /// Create the Channel List
        /// </summary>
        private void InitializeChannelList()
        {
            this.channelList = new FlickerFreeListView();
            this.channelList.SuspendLayout();
            this.SuspendLayout();

            this.channelList.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.channelList.Location = new System.Drawing.Point(0, 0);
            this.channelList.Name = "channelList";
            this.channelList.DoubleClick += new EventHandler(channelList_DoubleClick);
            this.channelList.ColumnClick += new ColumnClickEventHandler(channelList_ColumnClick);
            this.channelList.MouseDown += new MouseEventHandler(channelList_MouseDown);
            
            this.channelList.ShowItemToolTips = true;

            ColumnHeader c = new ColumnHeader();
            c.Text = "Channel";
            c.Width = 200;
            this.channelList.Columns.Add(c);
            
            this.channelList.Columns.Add("Users");
            ColumnHeader t = new ColumnHeader();
            t.Text = "Topic";
            t.Width = 2000;
            this.channelList.Columns.Add(t);

            this.channelList.View = View.Details;
            this.channelList.MultiSelect = false;
            this.channelList.FullRowSelect = true;

            Panel searchPanel = new Panel();
            searchPanel.Height = 30;

            Label searchLabel = new Label();
            searchLabel.Text = "Search Channels:";
            searchLabel.Dock = DockStyle.Left;
            searchLabel.AutoSize = true;
            searchLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            
            searchText = new TextBox();
            searchText.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            searchText.Dock = DockStyle.Fill;
            searchText.BorderStyle = BorderStyle.Fixed3D;

            Button searchButton = new Button();
            searchButton.Text = "Search";
            searchButton.Dock = DockStyle.Right;
            searchButton.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            searchButton.Click += new EventHandler(OnSearchButtonClick);
            
            searchPanel.Controls.Add(searchText);
            searchPanel.Controls.Add(searchLabel);
            searchPanel.Controls.Add(searchButton);
            
            searchPanel.Dock =  DockStyle.Bottom;
            this.channelList.Dock = DockStyle.Fill;

            this.Controls.Add(channelList);
            this.Controls.Add(searchPanel);
            this.channelList.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void OnSearchButtonClick(object sender, EventArgs e)
        {
            //filter the channel list according to the specified text
            if (searchText.Text.Length == 0)
            {
                //restore the original list
                if (listItems != null)
                {                    
                    channelList.Items.Clear();
                    ListViewItem[] items = new ListViewItem[listItems.Count];
                    listItems.CopyTo(items);
                    channelList.Items.AddRange(items);
                }
                listItems = null;
            }
            else
            {
                if (listItems == null)
                {
                    listItems = new List<ListViewItem>();
                    ListViewItem[] items = new ListViewItem[channelList.Items.Count];
                    channelList.Items.CopyTo(items, 0);
                    listItems.AddRange(items);
                }
                
                channelList.Items.Clear();
                
                foreach (ListViewItem item in listItems)
                {
                    if (item.Text.Contains(searchText.Text))
                        channelList.Items.Add(item);        
                    else if (item.SubItems[2].Text.Contains(searchText.Text))
                        channelList.Items.Add(item);        
                }

            }


        }

        private void channelList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewSorter Sorter = new ListViewSorter();
            channelList.ListViewItemSorter = Sorter;
            if (!(channelList.ListViewItemSorter is ListViewSorter))
                return;

            Sorter = (ListViewSorter)channelList.ListViewItemSorter;
            if (Sorter.LastSort == e.Column)
            {
                if (channelList.Sorting == SortOrder.Descending)
                    channelList.Sorting = SortOrder.Ascending;
                else
                    channelList.Sorting = SortOrder.Descending;
            }
            else
            {
                channelList.Sorting = SortOrder.Ascending;
            }
            Sorter.ByColumn = e.Column;
            
            channelList.Sort();
        }

        private void channelList_DoubleClick(object sender, EventArgs e)
        {
            foreach (ListViewItem eachItem in channelList.SelectedItems)
                FormMain.Instance.ParseOutGoingCommand(this.connection, "/join " + eachItem.Text);
        }

        private void channelList_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {                
                //show a popup menu
                ContextMenuStrip menu = new ContextMenuStrip();
                ToolStripMenuItem joinChannel = new System.Windows.Forms.ToolStripMenuItem();
                ToolStripMenuItem autoJoinChannel = new System.Windows.Forms.ToolStripMenuItem();
                
                menu.BackColor = SystemColors.Menu;
                joinChannel.ForeColor = SystemColors.MenuText;
                autoJoinChannel.ForeColor = SystemColors.MenuText;

                joinChannel.Text = "Join Channel";
                autoJoinChannel.Text = "Autojoin Channel";
                
                joinChannel.Size = new System.Drawing.Size(165, 22);
                autoJoinChannel.Size = new System.Drawing.Size(165, 22);

                joinChannel.Click += new EventHandler(joinChannel_Click);
                autoJoinChannel.Click += new EventHandler(autoJoinChannel_Click);
                menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    joinChannel,
                    autoJoinChannel});

                menu.Show(channelList, new Point(e.X, e.Y));
            }
        }

        private void autoJoinChannel_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem eachItem in channelList.SelectedItems)
                FormMain.Instance.ParseOutGoingCommand(this.connection, "/autojoin " + eachItem.Text);
            
        }

        private void joinChannel_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem eachItem in channelList.SelectedItems)
                FormMain.Instance.ParseOutGoingCommand(this.connection, "/join " + eachItem.Text);            
        }


        /// <summary>
        /// Create the Channel Window and items needed
        /// </summary>
        private void InitializeChannel()
        {
            this.panelTopic = new System.Windows.Forms.Panel();
            this.textTopic = new IceChat.TextWindow();
            this.textWindow = new IceChat.TextWindow();
            this.panelTopic.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTopic
            // 
            this.panelTopic.Controls.Add(this.textTopic);
            this.panelTopic.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTopic.Location = new System.Drawing.Point(0, 0);
            this.panelTopic.Name = "panelTopic";
            this.panelTopic.Size = new System.Drawing.Size(304, 22);
            this.panelTopic.TabIndex = 1;
            this.panelTopic.Visible = false;
            // 
            // textTopic
            // 
            this.textTopic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textTopic.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textTopic.IRCBackColor = 0;
            this.textTopic.IRCForeColor = 1;
            this.textTopic.Location = new System.Drawing.Point(0, 0);
            this.textTopic.Name = "textTopic";
            this.textTopic.NoColorMode = false;
            this.textTopic.ShowTimeStamp = true;
            this.textTopic.SingleLine = true;
            this.textTopic.Size = new System.Drawing.Size(304, 22);
            this.textTopic.TabIndex = 0;
            // 
            // textWindow
            // 
            this.textWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textWindow.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textWindow.IRCBackColor = 0;
            this.textWindow.IRCForeColor = 0;
            this.textWindow.Location = new System.Drawing.Point(0, 22);
            this.textWindow.Name = "textWindow";
            this.textWindow.NoColorMode = false;
            this.textWindow.ShowTimeStamp = true;
            this.textWindow.SingleLine = false;
            this.textWindow.Size = new System.Drawing.Size(304, 166);
            this.textWindow.TabIndex = 0;
            // 
            // TabWindow
            // 
            this.Controls.Add(this.textWindow);
            this.Controls.Add(this.panelTopic);
            this.Size = new System.Drawing.Size(304, 188);
            this.panelTopic.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }

    public class ConsoleTab : TabPage
    {
        public IRCConnection Connection;

        public ConsoleTab(string serverName)
        {
            base.Text = serverName;
        }
    }

    //http://www.daniweb.com/forums/thread86620.html

    //flicker free listview for channel list/dcc file list
    internal class FlickerFreeListView : ListView
    {

        private ToolTip toolTip = new ToolTip();
        
        public FlickerFreeListView()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
            
            toolTip.AutoPopDelay = 7000;
            toolTip.InitialDelay = 450;
            toolTip.ReshowDelay = 450;
        }

        protected override void OnNotifyMessage(Message m)
        {
            // filter WM_ERASEBKGND
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            ListViewItem item = this.GetItemAt(e.X, e.Y);
            ListViewHitTestInfo info = this.HitTest(e.X, e.Y);

            if ((item != null) && (info.SubItem != null))
            {
                toolTip.SetToolTip(this.Parent, info.Item.ToolTipText);
            }
            else
            {
                toolTip.SetToolTip(this.Parent, null);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pea)
        {
            // do nothing here since this event is now handled by OnPaint
        }

        protected override void OnPaint(PaintEventArgs pea)
        {
            base.OnPaint(pea);
        }


    }

    internal class ListViewSorter : System.Collections.IComparer
    {
        public int Compare(object o1, object o2)
        {
            if (!(o1 is ListViewItem))
                return (0);
            if (!(o2 is ListViewItem))
                return (0);

            ListViewItem lvi1 = (ListViewItem)o2;
            string str1 = lvi1.SubItems[ByColumn].Text;
            ListViewItem lvi2 = (ListViewItem)o1;
            string str2 = lvi2.SubItems[ByColumn].Text;

            int result;
            if (lvi1.ListView.Sorting == SortOrder.Ascending)
            {
                int r1;
                int r2;
                if (int.TryParse(str1, out r1) && int.TryParse(str2, out r2))
                {
                    //check if numeric
                    if (Convert.ToInt32(str1) > Convert.ToInt32(str2))
                        result = 1;
                    else
                        result = -1;
                }
                else
                    result = String.Compare(str1, str2);
            }
            else
            {
                int r3;
                int r4;
                if (int.TryParse(str1, out r3) && int.TryParse(str2, out r4))
                {
                    //check if numeric
                    if (Convert.ToInt32(str1) < Convert.ToInt32(str2))
                        result = 1;
                    else
                        result = -1;
                }
                else
                    result = String.Compare(str2, str1);
            }
            LastSort = ByColumn;

            return (result);
        }


        public int ByColumn
        {
            get { return Column; }
            set { Column = value; }
        }
        int Column = 0;

        public int LastSort
        {
            get { return LastColumn; }
            set { LastColumn = value; }
        }
        int LastColumn = 0;
    }
}
