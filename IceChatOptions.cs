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
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace IceChat
{
    [XmlRoot("IceChatColors")]
    public class IceChatColors
    {
        //default color values
        private int _channelOwnerColor = 4;
        private int _channelAdminColor = 4;
        private int _channelOpColor = 4;
        private int _channelHalfOpColor = 7;
        private int _channelVoiceColor = 9;
        private int _channelRegularColor = 0;
        private int _channelAwayColor = 8;

        private int _tabBarCurrent = 0;
        private int _tabBarNewMessage = 4;
        private int _tabBarChannelJoin = 29;
        private int _tabBarChannelPart = 46;
        private int _tabBarServerQuit = 9;
        private int _tabBarServerMessage = 13;
        private int _tabBarOtherMessage = 6;
        private int _tabBarDefault = 0;

        private int _tabBarNewAction = 4;
        private int _tabBarServerNotice = 13;

        private int _tabBarCurrentBG1 = 29;
        private int _tabBarCurrentBG2 = 0;
        private int _tabBarOtherBG1 = 14;
        private int _tabBarOtherBG2 = 0;
        private int _tabBarHoverBG1 = 0;
        private int _tabBarHoverBG2 = 7;
        private int _tabbarBack = 20;

        private int _panelHeaderBGColor1 = 15;
        private int _panelHeaderBGColor2 = 0;
        private int _panelHeaderForeColor = 1;

        private int _consoleBack = 1;
        private int _channelBack = 1;
        private int _queryBack = 1;
        private int _nicklistBack = 1;
        private int _serverlistBack = 1;
        private int _inputboxBack = 1;
        private int _inputboxFore = 0;
        private int _channellistBack = 1;
        private int _channellistFore = 0;
        private int _menubarBack = 20;
        private int _toolbarBack = 20;
        private int _statusbarBack = 20;
        private int _statusbarFore = 1;
        private int _unreadTextMarker = 4;        
        private int _hyperLinkColor = 4;
        private int _textSelectFore = 0;
        private int _textSelectBack = 2;


        private bool _randomizeNickColors = false;
        private bool _newMessageEnabled = true;
        private bool _channelJoinEnabled = true;
        private bool _channelPartEnabled = true;
        private bool _serverQuitEnabled = true;
        private bool _serverMessageEnabled = true;
        private bool _otherMessageEnabled = true;

        [XmlElement("ConsoleBackColor")]
        public int ConsoleBackColor
        { get { return _consoleBack; }  set { _consoleBack = value; } }

        [XmlElement("ChannelBackColor")]
        public int ChannelBackColor
        { get { return _channelBack; }  set { _channelBack = value; } }

        [XmlElement("QueryBackColor")]
        public int QueryBackColor
        { get { return _queryBack; }  set { _queryBack = value; } }

        [XmlElement("NickListBackColor")]
        public int NickListBackColor
        { get { return _nicklistBack; }  set { _nicklistBack = value; } }

        [XmlElement("ServerListBackColor")]
        public int ServerListBackColor
        { get { return _serverlistBack; }  set { _serverlistBack = value; } }

        [XmlElement("ChannelListBackColor")]
        public int ChannelListBackColor
        { get { return _channellistBack; } set { _channellistBack = value; } }

        [XmlElement("ChannelListForeColor")]
        public int ChannelListForeColor
        { get { return _channellistFore; } set { _channellistFore = value; } }

        [XmlElement("InputboxBackColor")]
        public int InputboxBackColor
        { get { return _inputboxBack; } set { _inputboxBack = value; } }

        [XmlElement("InputboxForeColor")]
        public int InputboxForeColor
        { get { return _inputboxFore; } set { _inputboxFore = value; } }

        [XmlElement("TabbarBackColor")]
        public int TabbarBackColor
        { get { return _tabbarBack; } set { _tabbarBack = value; } }

        [XmlElement("MenubarBackColor")]
        public int MenubarBackColor
        { get { return _menubarBack; } set { _menubarBack = value; } }

        [XmlElement("ToolbarBackColor")]
        public int ToolbarBackColor
        { get { return _toolbarBack; } set { _toolbarBack = value; } }

        [XmlElement("StatusbarBackColor")]
        public int StatusbarBackColor
        { get { return _statusbarBack; } set { _statusbarBack = value; } }

        [XmlElement("StatusbarForeColor")]
        public int StatusbarForeColor
        { get { return _statusbarFore; } set { _statusbarFore = value; } }

        [XmlElement("ChannelOwnerColor")]
        public int ChannelOwnerColor
        { get { return _channelOwnerColor; }  set { _channelOwnerColor = value; } }

        [XmlElement("ChannelAdminColor")]
        public int ChannelAdminColor
        { get { return this._channelAdminColor; } set { this._channelAdminColor = value; } }

        [XmlElement("ChannelOpColor")]
        public int ChannelOpColor
        { get { return this._channelOpColor; } set { this._channelOpColor = value; } }

        [XmlElement("ChannelHalfOpColor")]
        public int ChannelHalfOpColor
        { get { return this._channelHalfOpColor; } set { this._channelHalfOpColor = value; } }

        [XmlElement("ChannelVoiceColor")]
        public int ChannelVoiceColor
        { get { return this._channelVoiceColor; } set { this._channelVoiceColor = value; } }

        [XmlElement("ChannelRegularColor")]
        public int ChannelRegularColor
        { get { return this._channelRegularColor; } set { this._channelRegularColor = value; } }

        [XmlElement("ChannelAwayColor")]
        public int ChannelAwayColor
        { get { return this._channelAwayColor; } set { this._channelAwayColor = value; } }

        [XmlElement("TabBarCurrent")]
        public int TabBarCurrent
        { get { return _tabBarCurrent; } set { _tabBarCurrent = value; } }
        
        //new messages and actions
        [XmlElement("TabBarNewMessage")]
        public int TabBarNewMessage
        { get { return _tabBarNewMessage; } set { _tabBarNewMessage = value; } }

        [XmlElement("TabBarChannelJoin")]
        public int TabBarChannelJoin
        { get { return _tabBarChannelJoin; } set { _tabBarChannelJoin = value; } }

        [XmlElement("TabBarChannelPart")]
        public int TabBarChannelPart
        { get { return _tabBarChannelPart; } set { _tabBarChannelPart = value; } }

        [XmlElement("TabBarServerQuit")]
        public int TabBarServerQuit
        { get { return _tabBarServerQuit; } set { _tabBarServerQuit = value; } }

        [XmlElement("TabBarServerMessage")]
        public int TabBarServerMessage
        { get { return _tabBarServerMessage; } set { _tabBarServerMessage = value; } }

        [XmlElement("TabBarOtherMessage")]
        public int TabBarOtherMessage
        { get { return _tabBarOtherMessage; } set { _tabBarOtherMessage = value; } }

        [XmlElement("TabBarNewAction")]
        public int TabBarNewAction
        { get { return _tabBarNewAction; } set { _tabBarNewAction = value; } }

        [XmlElement("TabBarServerNotice")]
        public int TabBarServerNotice
        { get { return _tabBarServerNotice; } set { _tabBarServerNotice = value; } }

        [XmlElement("TabBarDefault")]
        public int TabBarDefault
        { get { return _tabBarDefault; } set { _tabBarDefault = value; } }

        [XmlElement("TabBarCurrentBG1")]
        public int TabBarCurrentBG1
        { get { return _tabBarCurrentBG1; } set { _tabBarCurrentBG1 = value; } }

        [XmlElement("TabBarCurrentBG2")]
        public int TabBarCurrentBG2
        { get { return _tabBarCurrentBG2; } set { _tabBarCurrentBG2 = value; } }

        [XmlElement("TabBarOtherBG1")]
        public int TabBarOtherBG1
        { get { return _tabBarOtherBG1; } set { _tabBarOtherBG1 = value; } }

        [XmlElement("TabBarOtherBG2")]
        public int TabBarOtherBG2
        { get { return _tabBarOtherBG2; } set { _tabBarOtherBG2 = value; } }

        [XmlElement("TabBarHoverBG1")]
        public int TabBarHoverBG1
        { get { return _tabBarHoverBG1; } set { _tabBarHoverBG1 = value; } }

        [XmlElement("TabBarHoverBG2")]
        public int TabBarHoverBG2
        { get { return _tabBarHoverBG2; } set { _tabBarHoverBG2 = value; } }

        [XmlElement("PanelHeaderBG1")]
        public int PanelHeaderBG1
        { get { return _panelHeaderBGColor1; } set { _panelHeaderBGColor1 = value; } }

        [XmlElement("PanelHeaderBG2")]
        public int PanelHeaderBG2
        { get { return _panelHeaderBGColor2; } set { _panelHeaderBGColor2 = value; } }

        [XmlElement("PanelHeaderForeColor")]
        public int PanelHeaderForeColor
        { get { return _panelHeaderForeColor; } set { _panelHeaderForeColor = value; } }

        [XmlElement("UnreadTextMarker")]
        public int UnreadTextMarkerColor
        { get { return this._unreadTextMarker; } set { this._unreadTextMarker = value; } }

        [XmlElement("HyperlinkColor")]
        public int HyperlinkColor
        { get { return this._hyperLinkColor; } set { this._hyperLinkColor = value; } }

        [XmlElement("TextSelectForeColor")]
        public int TextSelectForeColor
        { get { return this._textSelectFore; } set { this._textSelectFore = value; } }

        [XmlElement("TextSelectBackColor")]
        public int TextSelectBackColor
        { get { return this._textSelectBack; } set { this._textSelectBack = value; } }

        [XmlElement("RandomizeNickColors")]
        public bool RandomizeNickColors
        { get { return this._randomizeNickColors; } set { this._randomizeNickColors = value; } }

        [XmlElement("NewMessageColorChange")]
        public bool NewMessageColorChange
        { get { return this._newMessageEnabled; } set { this._newMessageEnabled = value; } }

        [XmlElement("ChannelJoinColorChange")]
        public bool ChannelJoinColorChange
        { get { return this._channelJoinEnabled; } set { this._channelJoinEnabled = value; } }

        [XmlElement("ChannePartColorChange")]
        public bool ChannelPartColorChange
        { get { return this._channelPartEnabled; } set { this._channelPartEnabled = value; } }

        [XmlElement("ServerQuitColorChange")]
        public bool ServerQuitColorChange
        { get { return this._serverQuitEnabled; } set { this._serverQuitEnabled = value; } }

        [XmlElement("ServerMessageColorChange")]
        public bool ServerMessageColorChange
        { get { return this._serverMessageEnabled; } set { this._serverMessageEnabled = value; } }

        [XmlElement("OtherMessageColorChange")]
        public bool OtherMessageColorChange
        { get { return this._otherMessageEnabled; } set { this._otherMessageEnabled = value; } }

    }
   
    [XmlRoot("IceChatOptions")]
    public class IceChatOptions
    {
        //set the default values
        private string _timeStamp = "[hh:mm.ss] ";
        private bool _showTimeStamp = true;
        private bool _saveWindow = true;
        private bool _identServer = true;
        private bool _reconnectServer = true;
        private bool _askQuit = true;
        private bool _logConsole = true;
        private bool _logChannel = true;
        private bool _logQuery = true;
        private bool _logWindow = true;
        private bool _seperateLogs = true;
        private string _logFormat = "Plain Text";

        private bool _showEmoticons = false;
        private bool _showEmoticonPicker = true;
        private bool _emoticonsFixedSize = true;

        private bool _showColorPicker = true;
        private bool _showStatusBar = true;
        private bool _showServerTree = true;
        private bool _showSendButton = true;
        private bool _showNickList = true;
        private bool _showToolBar = true;
        private bool _showTabBar = true;
        private bool _showTopic = true;
        private bool _disableQueries = false;
        private bool _showQueryForegound = true;
        private bool _whoisNewQuery = true;
        private bool _showUnreadLine = false;
        private bool _minimizeTray = false;
        private bool _isOnTray = false;
        private System.Windows.Forms.FormWindowState _windowState = System.Windows.Forms.FormWindowState.Normal;
        private bool _showSystemTrayIcon = true;
        private bool _showSystemTrayNotifications = true;
        private int _maxTextLines = 500;
        private bool _showNickHost = false;
        private bool _showNickButtons = true;
        private bool _showServerButtons = true;

        private string[] _leftPanels = { "Favorite Servers" };
        private string[] _rightPanels = { "Nick List", "Favorite Channels", "Buddy List" };

        private int _panelRightWidth = 230;
        private int _panelLeftWidth = 200;
        private int _dccChatTimeOut = 60;
        private int _dccPortLower = 5000;
        private int _dccPortUpper = 10000;
        private string _dccReceiveFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string _dccSendFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private int _dccBufferSize = 1024;
        private bool _autoGetRouterIP = true;

        private string _language = "English";
        private string _identName = "PonyChat";
        private string _fullName = "PonyChat User";
        private string _quitMessage = "$randquit";
        private string _afterNickComplete = "";

        private string _awayCommand = "/ame is now away - Reason : $awayreason";
        private string _returnCommand = "/ame is no longer away - Gone for $awaytime";
        private string _privateAwayMessage = "I am away at the moment, your message has been logged ( away for $awaytime )";
        private string _autoAwayMessage = "Auto-Away (Away from Keyboard for $autoawaytime minutes)";
        private int _autoAwayTime = 30;
        private bool _sendAwayCommands = true;
        private bool _sendAwayPrivateMessage = true;
        private int userNoticeEvent = 1;

        [XmlElement("TimeStamp")]
        public string TimeStamp
        { 
            get { return this._timeStamp; }
            set { this._timeStamp = value; }
        }

        [XmlElement("ShowTimeStamp")]
        public bool ShowTimeStamp
        {
            get { return this._showTimeStamp; }
            set { this._showTimeStamp = value; }
        }

        [XmlElement("AskQuit")]
        public bool AskQuit
        {
            get { return this._askQuit; }
            set { this._askQuit = value; }
        }

        [XmlElement("SaveWindowPosition")]
        public bool SaveWindowPosition
        {
            get { return this._saveWindow; }
            set { this._saveWindow = value; }
        }

        [XmlElement("WindowSize")]
        public System.Drawing.Size WindowSize
        { get; set; }

        [XmlElement("WindowLocation")]
        public System.Drawing.Point WindowLocation
        { get; set; }

        [XmlElement("WindowState")]
        public System.Windows.Forms.FormWindowState WindowState
        {
            get { return this._windowState; }
            set { this._windowState = value; }
        }

        [XmlElement("RightPanelWidth")]
        public int RightPanelWidth
        {
            get { return this._panelRightWidth; }
            set { this._panelRightWidth = value; }
        }

        [XmlElement("LeftPanelWidth")]
        public int LeftPanelWidth
        {
            get { return this._panelLeftWidth; }
            set { this._panelLeftWidth = value; }
        }

        [XmlElement("LeftPanels")]
        public string[] LeftPanels
        {
            get { return this._leftPanels; }
            set { this._leftPanels = value; }
        }

        [XmlElement("RightPanels")]
        public string[] RightPanels
        {
            get { return this._rightPanels; }
            set { this._rightPanels = value; }
        }

        [XmlElement("DefaultNick")]
        public string DefaultNick
        { get; set; }

        [XmlElement("DefaultIdent")]
        public string DefaultIdent
        {
            get { return this._identName; }
            set { this._identName = value; }
        }

        [XmlElement("DefaultFullName")]
        public string DefaultFullName
        {
            get { return this._fullName; }
            set { this._fullName = value; }
        }

        [XmlElement("DefaultQuitMessage")]
        public string DefaultQuitMessage
        {
            get { return this._quitMessage; }
            set { this._quitMessage = value; }
        }

        [XmlElement("IdentServer")]
        public bool IdentServer
        {
            get { return this._identServer; }
            set { this._identServer = value; }
        }

        [XmlElement("ReconnectServer")]
        public bool ReconnectServer
        {
            get { return this._reconnectServer; }
            set { this._reconnectServer = value; }
        }

        [XmlElement("LogConsole")]
        public bool LogConsole
        {
            get { return this._logConsole; }
            set { this._logConsole = value; }
        }

        [XmlElement("LogChannel")]
        public bool LogChannel
        {
            get { return this._logChannel; }
            set { this._logChannel = value; }
        }

        [XmlElement("LogWindow")]
        public bool LogWindow
        {
            get { return this._logWindow; }
            set { this._logWindow = value; }
        }

        [XmlElement("LogQuery")]
        public bool LogQuery
        {
            get { return this._logQuery; }
            set { this._logQuery = value; }
        }

        [XmlElement("SeperateLogs")]
        public bool SeperateLogs
        {
            get { return this._seperateLogs; }
            set { this._seperateLogs = value; }
        }

        [XmlElement("LogFormat")]
        public string LogFormat
        {
            get { return this._logFormat; }
            set { this._logFormat = value; }
        }

        [XmlElement("ShowEmoticons")]
        public bool ShowEmoticons
        {
            get { return this._showEmoticons; }
            set { this._showEmoticons = value; }
        }

        [XmlElement("ShowEmoticonPicker")]
        public bool ShowEmoticonPicker
        {
            get { return this._showEmoticonPicker; }
            set { this._showEmoticonPicker = value; }
        }

        [XmlElement("EmoticonsFixedSize")]
        public bool EmoticonsFixedSize
        {
            get { return this._emoticonsFixedSize; }
            set { this._emoticonsFixedSize = value; }
        }

        [XmlElement("ShowColorPicker")]
        public bool ShowColorPicker
        {
            get { return this._showColorPicker; }
            set { this._showColorPicker = value; }
        }
        
        [XmlElement("ShowStatusBar")]
        public bool ShowStatusBar
        {
            get { return this._showStatusBar; }
            set { this._showStatusBar = value; }
        }

        [XmlElement("ShowSendButton")]
        public bool ShowSendButton
        {
            get { return this._showSendButton; }
            set { this._showSendButton = value; }
        }

        [XmlElement("ShowToolBar")]
        public bool ShowToolBar
        {
            get { return this._showToolBar; }
            set { this._showToolBar = value; }
        }

        [XmlElement("ShowTabBar")]
        public bool ShowTabBar
        {
            get { return this._showTabBar; }
            set { this._showTabBar = value; }
        }
        
        [XmlElement("ShowServerTree")]
        public bool ShowServerTree
        {
            get { return this._showServerTree; }
            set { this._showServerTree = value; }
        }

        [XmlElement("ShowNickList")]
        public bool ShowNickList
        {
            get { return this._showNickList; }
            set { this._showNickList = value; }
        }

        [XmlElement("ShowNickHost")]
        public bool ShowNickHost
        {
            get { return this._showNickHost; }
            set { this._showNickHost = value; }
        }

        [XmlElement("ShowNickButtons")]
        public bool ShowNickButtons
        {
            get { return this._showNickButtons; }
            set { this._showNickButtons = value; }
        }

        [XmlElement("ShowServerButtons")]
        public bool ShowServerButtons
        {
            get { return this._showServerButtons; }
            set { this._showServerButtons = value; }
        }

        [XmlElement("ShowTopic")]
        public bool ShowTopic
        {
            get { return this._showTopic; }
            set { this._showTopic = value; }
        }

        [XmlElement("DockLeftPanel")]
        public bool DockLeftPanel
        {
            get;
            set;
        }

        [XmlElement("DockRightPanel")]
        public bool DockRightPanel
        {
            get;
            set;
        }

        [XmlElement("FlashTaskBar")]
        public bool FlashTaskBar
        {
            get;
            set;
        }

        [XmlElement("FlashTaskBarNumber")]
        public int FlashTaskBarNumber
        {
            get;
            set;
        }

        [XmlElement("FlashServerTreeIcons")]
        public bool FlashServerTreeIcons
        {
            get;
            set;
        }

        [XmlElement("DisableQueries")]
        public bool DisableQueries
        {
            get { return this._disableQueries; }
            set { this._disableQueries = value; }
        }

        [XmlElement("NewQueryForegound")]
        public bool NewQueryForegound
        {
            get { return this._showQueryForegound; }
            set { this._showQueryForegound = value; }
        }

        [XmlElement("WhoisNewQuery")]
        public bool WhoisNewQuery
        {
            get { return this._whoisNewQuery; }
            set { this._whoisNewQuery = value; }
        }

        [XmlElement("ChannelOpenKick")]
        public bool ChannelOpenKick
        { get; set; }


        [XmlElement("ShowUnreadLine")]
        public bool ShowUnreadLine
        {
            get { return this._showUnreadLine; }
            set { this._showUnreadLine = value; }
        }

        [XmlElement("MinimizeToTray")]
        public bool MinimizeToTray
        {
            get { return this._minimizeTray; }
            set { this._minimizeTray = value; }
        }

        [XmlElement("IsOnTray")]
        public bool IsOnTray
        {
            get { return this._isOnTray; }
            set { this._isOnTray = value; }
        }

        [XmlElement("ShowSytemTrayIcon")]
        public bool ShowSytemTrayIcon
        {
            get { return this._showSystemTrayIcon; }
            set { this._showSystemTrayIcon = value; }
        }

        [XmlElement("ShowSytemTrayNotifications")]
        public bool ShowSytemTrayNotifications
        {
            get { return this._showSystemTrayNotifications; }
            set { this._showSystemTrayNotifications = value; }
        }

        [XmlElement("MaximumTextLines")]
        public int MaximumTextLines
        {
            get { return this._maxTextLines; }
            set { this._maxTextLines = value; }
        }

        [XmlElement("NickCompleteAfter")]
        public string NickCompleteAfter
        {
            get { return this._afterNickComplete; }
            set { this._afterNickComplete = value; }
        }

        [XmlElement("Language")]
        public string Language
        {
            get { return this._language; }
            set { this._language = value; }
        }

        [XmlElement("DCCChatAutoAccept")]
        public bool DCCChatAutoAccept
        { get; set; }

        [XmlElement("DCCFileAutoAccept")]
        public bool DCCFileAutoAccept
        { get; set; }

        [XmlElement("DCCChatAutoAcceptBuddyOnly")]
        public bool DCCChatAutoAcceptBuddyOnly
        { get; set; }

        [XmlElement("DCCFileAutoAcceptBuddyOnly")]
        public bool DCCFileAutoAcceptBuddyOnly
        { get; set; }

        [XmlElement("DCCChatIgnore")]
        public bool DCCChatIgnore
        { get; set; }

        [XmlElement("DCCFileIgnore")]
        public bool DCCFileIgnore
        { get; set; }

        [XmlElement("DCCChatTimeOut")]
        public int DCCChatTimeOut
        {
            get { return this._dccChatTimeOut; }
            set { this._dccChatTimeOut = value; }
        }

        [XmlElement("DCCPortLower")]
        public int DCCPortLower
        {
            get { return this._dccPortLower; }
            set { this._dccPortLower = value; }
        }

        [XmlElement("DCCPortUpper")]
        public int DCCPortUpper
        {
            get { return this._dccPortUpper; }
            set { this._dccPortUpper = value; }
        }

        [XmlElement("DCCReceiveFolder")]
        public string DCCReceiveFolder
        {
            get { return this._dccReceiveFolder; }
            set { this._dccReceiveFolder = value; }
        }

        [XmlElement("DCCSendFolder")]
        public string DCCSendFolder
        {
            get { return this._dccSendFolder; }
            set { this._dccSendFolder = value; }
        }

        [XmlElement("DCCLocalIP")]
        public string DCCLocalIP
        { get; set; }

        [XmlElement("DCCAutoGetRouterIP")]
        public bool DCCAutogetRouterIP
        {
            get { return this._autoGetRouterIP; }
            set { this._autoGetRouterIP = value; }
        }

        [XmlElement("DCCBufferSize")]
        public int DCCBufferSize
        {
            get { return this._dccBufferSize; }
            set { this._dccBufferSize = value; }
        }

        [XmlArray("ScriptFiles")]
        [XmlArrayItem("Item")]
        public string[] ScriptFiles
        { get; set; }

        [XmlElement("JoinEventLocation")]
        public int JoinEventLocation
        { get; set; }

        [XmlElement("PartEventLocation")]
        public int PartEventLocation
        { get; set; }

        [XmlElement("QuitEventLocation")]
        public int QuitEventLocation
        { get; set; }

        [XmlElement("ModeEventLocation")]
        public int ModeEventLocation
        { get; set; }

        [XmlElement("TopicEventLocation")]
        public int TopicEventLocation
        { get; set; }

        [XmlElement("KickEventLocation")]
        public int KickEventLocation
        { get; set; }

        [XmlElement("ChannelMessageEventLocation")]
        public int ChannelMessageEventLocation
        { get; set; }

        [XmlElement("ChannelActionEventLocation")]
        public int ChannelActionEventLocation
        { get; set; }

        [XmlElement("ChannelNoticeEventLocation")]
        public int ChannelNoticeEventLocation
        { get; set; }

        [XmlElement("NickChangeEventLocation")]
        public int NickChangeEventLocation
        { get; set; }

        [XmlElement("ServerNoticeEventLocation")]
        public int ServerNoticeEventLocation
        { get; set; }

        [XmlElement("ServerMessageEventLocation")]
        public int ServerMessageEventLocation
        { get; set; }

        [XmlElement("UserNoticeEventLocation")]
        public int UserNoticeEventLocation
        { get { return userNoticeEvent; } set { userNoticeEvent = value; } }

        [XmlElement("WhoisEventLocation")]
        public int WhoisEventLocation
        { get; set; }

        [XmlElement("CtcpEventLocation")]
        public int CtcpEventLocation
        { get; set; }

        [XmlElement("ServerErrorEventLocation")]
        public int ServerErrorEventLocation
        { get; set; }

        [XmlElement("BuddyEventLocation")]
        public int BuddyEventLocation
        { get; set; }

        [XmlElement("DccEventLocation")]
        public int DccEventLocation
        { get; set; }

        [XmlElement("CurrentTheme")]
        public string CurrentTheme
        { get; set; }

        [XmlArray("Theme")]
        [XmlArrayItem("Item", typeof(ThemeItem))]
        public ThemeItem[] Theme
        { get; set; }

        [XmlElement("SoundUseExternalCommand")]
        public bool SoundUseExternalCommand
        { get; set; }

        [XmlElement("SoundExternalCommand")]
        public string SoundExternalCommand
        { get; set; }

        [XmlElement("SoundPlayActive")]
        public bool SoundPlayActive
        { get; set; }

        [XmlElement("ShowMultilineEditbox")]
        public bool ShowMultilineEditbox
        { get; set; }

        [XmlElement("NickListImage")]
        public string NickListImage
        { get; set; }

        [XmlElement("ServerTreeImage")]
        public string ServerTreeImage
        { get; set; }

        [XmlElement("AwayCommand")]
        public string AwayCommand
        {
            get { return this._awayCommand; }
            set { this._awayCommand = value; }
        }
        [XmlElement("ReturnCommand")]
        public string ReturnCommand
        {
            get { return this._returnCommand; }
            set { this._returnCommand = value; }
        }

      [XmlElement("PrivateAwayMessage")]
      public string PrivateAwayMessage
      {
          get { return this._privateAwayMessage; }
          set { this._privateAwayMessage = value; }
      }

      [XmlElement("AutoAwayMessage")]
      public string AutoAwayMessage
      {
          get { return this._autoAwayMessage; }
          set { this._autoAwayMessage = value; }
      }

      [XmlElement("AutoAwayTime")]
      public int AutoAwayTime
      {
          get { return this._autoAwayTime; }
          set { this._autoAwayTime = value; }
      }

      [XmlElement("SendAwayCommands")]
      public bool SendAwayCommands
      {
          get { return this._sendAwayCommands; }
          set { this._sendAwayCommands = value; }
      }

      [XmlElement("SendAwayPrivateMessage")]
      public bool SendAwayPrivateMessage
      {
          get { return this._sendAwayPrivateMessage; }
          set { this._sendAwayPrivateMessage = value; }
      }

      [XmlElement("AutoAway")]
      public bool AutoAway
      { get; set; }

      [XmlElement("AutoReturn")]
      public bool AutoReturn
      { get; set; }

      [XmlElement("AutoAwaySystemTray")]
      public bool AutoAwaySystemTray
      { get; set; }

    }
    
    public class ThemeItem
    {
        public string ThemeName;
        public string ThemeType;
    }

    [XmlRoot("IceChatMessageFormat")]
    public class IceChatMessageFormat
    {
        [XmlArray("MessageSettings")]
        [XmlArrayItem("Item", typeof(ServerMessageFormatItem))]
        public ServerMessageFormatItem[] MessageSettings
        { get; set; }

    }

    public class ServerMessageFormatItem
    {
        public string MessageName;
        public string FormattedMessage;
    }



    [XmlRoot("IceChatFonts")]
    public class IceChatFontSetting
    {
        [XmlArray("FontSettings")]
        [XmlArrayItem("Item", typeof(FontSettingItem))]
        public FontSettingItem[] FontSettings
        { get; set; }    
    }

    public class FontSettingItem
    {
        public string WindowType;
        public string FontName;
        public float FontSize;

    }

    public class IceChatPluginFile
    {
        [XmlArray("Plugins")]
        [XmlArrayItem("Item", typeof(PluginItem))]
        public List<PluginItem> listPlugins;

        public IceChatPluginFile()
        {
            listPlugins = new List<PluginItem>();
        }
        public void AddPlugin(PluginItem plugin)
        {
            listPlugins.Add(plugin);
        }
    }

    public class PluginItem
    {
        [XmlElement("PluginFile")]
        public string PluginFile
        { get; set; }

        [XmlElement("Enabled")]
        public bool Enabled
        { get; set; }
    }

    //seperate file(s) for all the aliases
    public class IceChatAliases
    {
        [XmlArray("Aliases")]
        [XmlArrayItem("Item", typeof(AliasItem))]
        public List<AliasItem> listAliases;

        public IceChatAliases()
        {
            listAliases = new List<AliasItem>();
        }
        public void AddAlias(AliasItem alias)
        {
            listAliases.Add(alias);
        }
    }

    public class AliasItem
    {
        [XmlElement("AliasName")]
        public string AliasName
        { get; set; }

        [XmlArray("Command")]
        [XmlArrayItem("Item")]
        public string[] Command
        { get; set; }        
    }

    public class IceChatEmoticon
    {
        [XmlArray("Emoticons")]
        [XmlArrayItem("Item", typeof(EmoticonItem))]
        public List<EmoticonItem> listEmoticons;
        public IceChatEmoticon()
        {
            listEmoticons = new List<EmoticonItem>();
        }
        public void AddEmoticon(EmoticonItem item)
        {
            listEmoticons.Add(item);
        }
    }

    public class EmoticonItem
    {
        [XmlAttribute("ID")]
        public int ID
        { get; set; }
        
        [XmlElement("EmoticonImage")]
        public string EmoticonImage
        { get; set; }

        [XmlElement("Trigger")]
        public string Trigger
        { get; set; }        

    }

    public class IceChatPopupMenus
    {
        [XmlArray("Popups")]
        [XmlArrayItem("Item",typeof(PopupMenuItem))]
        public List<PopupMenuItem> listPopups;

        public IceChatPopupMenus() 
        {
            listPopups = new List<PopupMenuItem>();
        }
        
        public void ReplacePopup(string popupType, PopupMenuItem menu)
        {
            foreach (PopupMenuItem p in listPopups)
            {
                if (p.PopupType == popupType)
                {
                    listPopups.Remove(p);
                    break;
                }
            }
            listPopups.Add(menu);
        }

        public void AddPopup(PopupMenuItem menu) 
        {
            listPopups.Add(menu);
        }

    }
    public class PopupMenuItem
    {
        [XmlAttribute("PopupType")]
        public string PopupType
        { get; set; }

        [XmlArray("Menu")]
        [XmlArrayItem("Item")]
        public string[] Menu
        { get; set; }        

    }

    [XmlRoot("IceChatSounds")]
    public class IceChatSounds
    {
        public class SoundEntry
        {
            public SoundEntry() { }
            public SoundEntry(string k, string d)
            {
                key = k; desc = d;
            }

            public SoundEntry(string k, string d, string f)
            {
                key=k; desc=d; file=f;
            }

            public SoundEntry(string k, string d, SoundEntry p)
            {
                key = k; desc = d; parent = p;
            }

            private string key;
            private string desc;
            private string file;
            SoundEntry parent;

            [XmlElement("Key")]
            public string Key
            {
                get { return key; }
                set
                {                    
                    if (key != null)
                        throw new InvalidOperationException("Cannot modify a key after it was assigned a value.");
                    key = value;
                }
            }
            [XmlElement("Description")]
            public string Description
            {
                get { return desc; }
                set { desc = value; }
            }
            [XmlElement("File")]
            public string File
            {
                get { return file; }
                set { file = value; }
            }

            public SoundEntry Parent
            {
                get {return parent;}
                set { parent = value; }
            }

            public string getSoundFile()
            {
                if (file!=null && file.Length>0) return file;
                if (parent!=null) return parent.getSoundFile();
                return null;
            }

        }

        [XmlArray("soundList")]
        [XmlArrayItem("soundEntry", typeof(SoundEntry))]
        public List<SoundEntry> soundList;

        public IceChatSounds()
        {
            soundList = new List<SoundEntry>();
        }

        public void AddDefaultSounds()
        {
            Add(new SoundEntry("conmsg", "New Message in Console"));
            Add(new SoundEntry("chanmsg", "New Channel Message"));
            Add(new SoundEntry("privmsg", "New Private Message"));
            Add(new SoundEntry("notice", "New User Notice"));
            Add(new SoundEntry("nickchan", "Nickname said in channel"));
            Add(new SoundEntry("nickpriv", "Nickname said in private message"));
            Add(new SoundEntry("buddy", "A buddy has come online"));
            Add(new SoundEntry("dropped", "Server Disconnection"));
            Add(new SoundEntry("operping", "Request for operator"));
        }

        public void Add(SoundEntry s)
        {
            foreach (SoundEntry x in soundList)
            {
                if (x.Key.Equals(s.Key))
                {
                    x.Parent = s.Parent;
                    return;
                }
            }
            soundList.Add(s);
        }

        public SoundEntry getSound(string key)
        {
            foreach (SoundEntry x in soundList)
            {
                if (x.Key.Equals(key))
                {
                    return x;
                }
            }
            return null;
        }

        public SoundEntry getSound(int index)
        {
            return soundList[index];
        }

    }
    
    [XmlRoot("ChannelSetting")]
    public class ChannelSetting
    {
        //remember certain individual channel settings
        [XmlElement("ChannelName")]
        public string ChannelName
        {
            get;
            set;
        }

        [XmlElement("NetworkName")]
        public string NetworkName
        {
            get;
            set;
        }

        [XmlElement("EventsDisable")]
        public bool EventsDisable
        {
            get;
            set;
        }

        [XmlElement("NoColorMode")]
        public bool NoColorMode
        {
            get;
            set;
        }

        [XmlElement("SoundsDisable")]
        public bool SoundsDisable
        {
            get;
            set;
        }
        
        [XmlElement("LoggingDisable")]
        public bool LoggingDisable
        {
            get;
            set;
        }
         
    }

    [XmlRoot("ChannelSettings")]
    public class ChannelSettings
    {
        [XmlArray("Channels")]
        [XmlArrayItem("Item", typeof(ChannelSetting))]
        public List<ChannelSetting> listChannels;

        public ChannelSettings()
        {
            listChannels = new List<ChannelSetting>();
        }

        public void AddChannel(ChannelSetting channel)
        {
            listChannels.Add(channel);
        }

        public void RemoveChannel(ChannelSetting channel)
        {
            listChannels.Remove(channel);
        }
        
        public ChannelSetting FindChannel(string name)
        {
            foreach (ChannelSetting x in listChannels)
            {
                if (x.ChannelName.ToLower().Equals(name.ToLower()))
                {
                    return x;
                }
            }
            return null;
        }
    }
}
