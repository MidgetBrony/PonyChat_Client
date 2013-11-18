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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace IceChat
{
    public partial class IRCConnection
    {

        public event OutGoingCommandDelegate OutGoingCommand;

        public event ChannelMessageDelegate ChannelMessage;
        public event ChannelActionDelegate ChannelAction;
        public event QueryMessageDelegate QueryMessage;
        public event QueryActionDelegate QueryAction;

        public event GenericChannelMessageDelegate GenericChannelMessage;

        public event ChangeNickDelegate ChangeNick;
        public event JoinChannelDelegate JoinChannel;
        public event PartChannelDelegate PartChannel;
        public event QuitServerDelegate QuitServer;
        public event ChannelNoticeDelegate ChannelNotice;

        public event ChannelKickDelegate ChannelKick;
        public event ChannelKickSelfDelegate ChannelKickSelf;

        public event ChannelTopicDelegate ChannelTopic;

        public event ChannelModeChangeDelegate ChannelMode;
        public event UserModeChangeDelegate UserMode;

        public event ChannelInviteDelegate ChannelInvite;
        public event UserHostReplyDelegate UserHostReply;
        public event JoinChannelMyselfDelegate JoinChannelMyself;
        public event PartChannelMyselfDelegate PartChannelMyself;

        public event ServerMessageDelegate ServerMessage;
        public event ServerMOTDDelegate ServerMOTD;
        public event ServerErrorDelegate ServerError;
        public event WhoisDataDelegate WhoisData;
        public event CtcpMessageDelegate CtcpMessage;
        public event CtcpReplyDelegate CtcpReply;
        public event UserNoticeDelegate UserNotice;

        public event ServerNoticeDelegate ServerNotice;

        public event ChannelListStartDelegate ChannelListStart;
        public event ChannelListEndDelegate ChannelListEnd;
        public event ChannelListDelegate ChannelList;

        public event DCCChatDelegate DCCChat;
        public event DCCFileDelegate DCCFile;
        public event DCCPassiveDelegate DCCPassive;

        public event RawServerIncomingDataDelegate RawServerIncomingData;
        public event RawServerOutgoingDataDelegate RawServerOutgoingData;

        public event RawServerIncomingDataOverRideDelegate RawServerIncomingDataOverRide;

        public event IALUserDataDelegate IALUserData;
        public event IALUserDataAwayOnlyDelegate IALUserDataAwayOnly;
        public event IALUserChangeDelegate IALUserChange;
        public event IALUserPartDelegate IALUserPart;
        public event IALUserQuitDelegate IALUserQuit;

        public event BuddyListDelegate BuddyListData;
        public event BuddyListClearDelegate BuddyListClear;

        public event AutoJoinDelegate AutoJoin;
        public event AutoRejoinDelegate AutoRejoin;
        public event AutoPerformDelegate AutoPerform;
        public event EndofNamesDelegate EndofNames;
        public event EndofWhoReplyDelegate EndofWhoReply;
        public event WhoReplyDelegate WhoReply;
        public event ChannelUserListDelegate ChannelUserList;

        public event StatusTextDelegate StatusText;

        public event ChannelInfoWindowExistsDelegate ChannelInfoWindowExists;
        public event ChannelInfoAddBanDelegate ChannelInfoAddBan;
        public event ChannelInfoAddExceptionDelegate ChannelInfoAddException;
        public event ChannelInfoTopicSetDelegate ChannelInfoTopicSet;

        public event UserInfoWindowExistsDelegate UserInfoWindowExists;
        public event UserInfoHostFullnameDelegate UserInfoHostFullName;
        public event UserInfoIdleLogonDelegate UserInfoIdleLogon;
        public event UserInfoAddChannelsDelegate UserInfoAddChannels;
        public event UserInfoAwayStatusDelegate UserInfoAwayStatus;
        public event UserInfoServerDelegate UserInfoServer;

        public event RefreshServerTreeDelegate RefreshServerTree;
        public event WriteErrorFileDelegate WriteErrorFile;
        public event ServerReconnectDelegate ServerReconnect;
        public event ServerReconnectDelegate ServerDisconnect;
        public event ServerConnectDelegate ServerConnect;
        public event ServerForceDisconnectDelegate ServerForceDisconnect;
        public event ServerPreConnectDelegate ServerPreConnect;

        public event AutoAwayDelegate AutoAwayTrigger;
        public event ServerForceDisconnectDelegate ServerFullyConnected;

        private bool triedAltNickName = false;
        private bool initialLogon = false;

        public Form UserInfoWindow = null;

        private void ParseData(string data)
        {
            try
            {
                string[] ircData = data.Split(' ');
                string channel;
                string nick;
                string host;
                string msg;
                string tempValue;
                bool check;

                if (RawServerIncomingDataOverRide != null)
                    data = RawServerIncomingDataOverRide(this, data);

                if (RawServerIncomingData != null)
                    RawServerIncomingData(this, data);

                if (data.Length > 4)
                {
                    if (data.Substring(0, 4).ToUpper() == "PING")
                    {
                        SendData("PONG " + ircData[1]);
                        //System.Diagnostics.Debug.WriteLine("PONG " + ircData[1] + "\r\n");
                        pongTimer.Stop();
                        pongTimer.Start();

                        if (serverSetting.ShowPingPong == true)
                            ServerMessage(this, "Ping? Pong!");
                        return;
                    }

                    if (data.Substring(0,12) == "AUTHENTICATE")
                    {
                        //need USERNAME and PASSWORD for SASL Auth
                        if (serverSetting.UseSASL)
                        {
                            if (serverSetting.SASLPass.Length > 0 && serverSetting.SASLUser.Length > 0)
                            {
                                string a = serverSetting.SASLUser + "\0" + serverSetting.SASLUser + "\0" + serverSetting.SASLPass + "\0";
                                byte[] toEncodeAsBytes = System.Text.Encoding.UTF8.GetBytes(a);
                                string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

                                SendData("AUTHENTICATE " + returnValue);
                            }
                            else
                                SendData("AUTHENTICATE *");
                        }                            
                    }                            
                }

                //parse then 2nd word
                string IrcNumeric;

                if (data.IndexOf(' ') > -1)
                {

                    IrcNumeric = ircData[1];

                    nick = NickFromFullHost(RemoveColon(ircData[0]));
                    host = HostFromFullHost(RemoveColon(ircData[0]));

                    // A great list of IRC Numerics http://www.mirc.net/raws/

                    switch (IrcNumeric)
                    {
                        case "001":
                            //get the real server name
                            serverSetting.RealServerName = RemoveColon(ircData[0]);

                            if (serverSetting.NickName != ircData[2])
                            {
                                ChangeNick(this, serverSetting.NickName, ircData[2], HostFromFullHost(ircData[0]));
                                serverSetting.NickName = ircData[2];
                                serverSetting.CurrentNickName = ircData[2];
                            }
                            else
                                serverSetting.CurrentNickName = serverSetting.NickName;

                            StatusText(this, serverSetting.CurrentNickName + " connected to " + serverSetting.RealServerName);

                            ServerMessage(this, JoinString(ircData, 3, true));

                            SendData("USERHOST " + serverSetting.CurrentNickName);

                            initialLogon = true;
                            break;
                        case "002":
                        case "003":
                            ServerMessage(this, JoinString(ircData, 3, true));
                            break;

                        case "004":
                        case "005":
                            if (ServerMessage != null)
                                ServerMessage(this, JoinString(ircData, 3, false));

                            //parse out all the 005 data
                            for (int i = 0; i < ircData.Length; i++)
                            {
                                //parse out all the status modes for user prefixes
                                if (ircData[i].Length >= 7)
                                {
                                    if (ircData[i].StartsWith("PREFIX="))
                                    {
                                        string[] modes = ircData[i].Substring(8).Split(')');
                                        serverSetting.StatusModes = new char[2][];
                                        serverSetting.StatusModes[0] = modes[0].ToCharArray();
                                        serverSetting.StatusModes[1] = modes[1].ToCharArray();                                        
                                    }
                                }

                                //add all the channel modes that have parameters into a variable
                                if (ircData[i].Length >= 10)
                                {
                                    if (ircData[i].Substring(0, 10) == "CHANMODES=")
                                    {
                                        //CHANMODES=b,k,l,imnpstrDducCNMT
                                        /*
                                        CHANMODES=A,B,C,D

                                        The CHANMODES token specifies the modes that may be set on a channel.
                                        These modes are split into four categories, as follows:

                                        Type A: Modes that add or remove an address to or from a list.
                                        These modes always take a parameter when sent by the server to a
                                        client; when sent by a client, they may be specified without a
                                        parameter, which requests the server to display the current
                                        contents of the corresponding list on the channel to the client.
                                        Type B: Modes that change a setting on the channel. These modes
                                        always take a parameter.
                                        Type C: Modes that change a setting on the channel. These modes
                                        take a parameter only when set; the parameter is absent when the
                                        mode is removed both in the client's and server's MODE command.
                                        Type D: Modes that change a setting on the channel. These modes
                                        never take a parameter.
                                        */

                                        //CHANMODES=b,k,l,imnpstrDducCNMT
                                        //CHANMODES=bouv,k,lOMN,cdejimnpqrstzAJLRU
                                        string[] modes = ircData[i].Substring(ircData[i].IndexOf("=") + 1).Split(',');
                                        if (modes.Length == 4)
                                        {
                                            serverSetting.ChannelModeAddress = modes[0];
                                            serverSetting.ChannelModeParam = modes[1];
                                            serverSetting.ChannelModeParamNotRemove = modes[2];
                                            serverSetting.ChannelModeNoParam = modes[3];
                                        }
                                    }
                                }
                                //parse MAX MODES set
                                if (ircData[i].Length > 6)
                                {
                                    if (ircData[i].StartsWith("MODES="))
                                        serverSetting.MaxModes = Convert.ToInt32(ircData[i].Substring(6));
                                }

                                //parse STATUSMSG symbols
                                if (ircData[i].Length > 10)
                                {
                                    if (ircData[i].StartsWith("STATUSMSG="))
                                        serverSetting.StatusMSG = ircData[i].Substring(10).ToCharArray();
                                }

                                //extract the network name                            
                                if (ircData[i].Length > 8)
                                {
                                    if (ircData[i].Substring(0, 8) == "NETWORK=")
                                        serverSetting.NetworkName = ircData[i].Substring(8);
                                }

                                //parse CHANTYPES symbols
                                if (ircData[i].Length > 10)
                                {
                                    if (ircData[i].StartsWith("CHANTYPES="))
                                        serverSetting.ChannelTypes = ircData[i].Substring(10).ToCharArray();
                                }

                                if (ircData[i].Length > 8)
                                {
                                    if (ircData[i].Substring(0, 8) == "CHARSET=")
                                    {
                                        //do something about character sets
                                    }
                                }

                                //check max nick length
                                if (ircData[i].Length > 8)
                                {
                                    if (ircData[i].Substring(0, 8) == "NICKLEN=")
                                    {
                                        serverSetting.MaxNickLength = Convert.ToInt32(ircData[i].Substring(8));
                                    }
                                }
                                if (ircData[i].Length > 11)
                                {
                                    if (ircData[i].Substring(0, 11) == "MAXNICKLEN=")
                                    {
                                        serverSetting.MaxNickLength = Convert.ToInt32(ircData[i].Substring(11));
                                    }
                                }

                                //tell server this client supports NAMESX
                                if (ircData[i] == "NAMESX")
                                {
                                    SendData("PROTOCTL NAMESX");
                                }
                                //CLIENTVER=3.0
                                if (ircData[i].Length > 10)
                                {
                                    if (ircData[i].Substring(0, 10) == "CLIENTVER=")
                                    {
                                        //ircv3 or 3.1, etc
                                        serverSetting.IRCV3 = true;
                                    }
                                }
                            }
                            break;
                        case "006": // map data
                            ServerMessage(this, JoinString(ircData, 3, true));                            
                            break;
                        case "007": //could be end of map
                            if (ircData[3] == ":End")
                            {
                                ServerMessage(this, JoinString(ircData, 3, true));
                            }
                            else
                            {
                                DateTime date5 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                                date5 = date5.AddSeconds(Convert.ToDouble(ircData[4]));

                                msg = ircData[3] + " " + date5.ToShortTimeString() + " " + JoinString(ircData, 5, true);
                                ServerMessage(this, msg);
                            }
                            break;
                        case "014":
                            ServerMessage(this, JoinString(ircData, 3, false));
                            break;
                        case "020": //IRCnet message
                            ServerMessage(this, JoinString(ircData, 3, true));
                            break;
                        case "042":
                            msg = JoinString(ircData, 4, true) + " " + ircData[3];
                            ServerMessage(this, msg);
                            break;
                        case "219": //end of stats
                            ServerMessage(this, JoinString(ircData, 4, true));
                            break;
                        case "221": //:port80b.se.quakenet.org 221 Snerf +i
                            ServerMessage(this, RemoveColon(ircData[0]) + " sets mode for " + ircData[2] + " " + ircData[3]);
                            break;
                        case "222":
                            //ircData[3] == new encoding
                            //auto switch to the new encoding style
                            foreach (EncodingInfo ei in System.Text.Encoding.GetEncodings())
                                if (ei.Name.ToLower() == ircData[3].ToLower())
                                    this.serverSetting.Encoding = ircData[3].ToLower();
                            
                            ServerMessage(this, ircData[3] + " " + JoinString(ircData, 4, true));
                            break;
                        case "251": //there are x users on x servers
                        case "255": //I have x users and x servers
                            ServerMessage(this, JoinString(ircData, 3, true));
                            break;
                        case "250": //highest connection count
                            msg = JoinString(ircData, 3, true);
                            ServerMessage(this, msg);
                            break;
                        case "252": //operators online
                        case "253": //unknown connections
                        case "254": //channels formed
                            msg = "There are " + ircData[3] + " " + JoinString(ircData, 4, true);
                            ServerMessage(this, msg);
                            break;
                        case "265": //current local users / max
                        case "266": //current global users / max
                            if (ircData[5].StartsWith(":"))
                                msg = JoinString(ircData, 5, true);
                            else
                                msg = JoinString(ircData, 3, true);
                            ServerMessage(this, msg);
                            break;

                        case "302": //parse out a userhost
                            msg = JoinString(ircData, 3, true).TrimEnd();
                            if (msg.Length == 0) return;                            
                            if (msg.IndexOf(' ') == -1)
                            {
                                //single host
                                host = msg.Substring(msg.IndexOf('@') + 1);
                                if (msg.IndexOf('*') > -1)
                                    nick = msg.Substring(0, msg.IndexOf('*'));
                                else
                                    nick = msg.Substring(0, msg.IndexOf('='));

                                try
                                {
                                    System.Net.IPAddress[] addresslist = System.Net.Dns.GetHostAddresses(host);
                                    foreach (System.Net.IPAddress address in addresslist)
                                    {
                                        OutGoingCommand(this, "/echo " + nick + " resolved to " + address.ToString());
                                        UserHostReply(this, msg);
                                        if (nick == serverSetting.CurrentNickName)
                                        {
                                            serverSetting.LocalIP = address;
                                        }
                                    }
                                }
                                catch
                                {
                                    //this can cause a Socket Exception Error
                                    OutGoingCommand(this, "/echo " + nick + " (" + host + ") can not be resolved");
                                }
                            }
                            else
                            {
                                //multiple hosts
                                string[] hosts = msg.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string h in hosts)
                                    UserHostReply(this, h);
                            }
                            break;
                        case "303": //parse out ISON information (Buddy List)
                            msg = JoinString(ircData, 3, true);

                            //queue up next batch to send
                            buddyListTimer.Start();

                            //if (msg.Length == 0) return;

                            string[] buddies = msg.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            BuddyListData(this, buddies);
                            break;
                        case "311":     //whois information username address
                            nick = ircData[3];
                            check = UserInfoWindowExists(this, ircData[3]);
                            if (check)
                            {
                                UserInfoHostFullName(this, nick, ircData[4] + "@" + ircData[5], JoinString(ircData, 7, true));
                            }
                            else
                            {
                                msg = "is " + ircData[4] + "@" + ircData[5] + " (" + JoinString(ircData, 7, true) + ")";
                                WhoisData(this, ircData[3], msg);
                                IALUserData(this, nick, ircData[4] + "@" + ircData[5], "");
                            }
                            break;
                        case "312":     //whois information server info
                            nick = ircData[3];
                            check = UserInfoWindowExists(this, nick);
                            if (check)
                            {
                                UserInfoServer(this, nick, ircData[4] + " (" + JoinString(ircData, 5, true) + ")");
                            }
                            else
                            {
                                msg = "using " + ircData[4] + " (" + JoinString(ircData, 5, true) + ")";
                                WhoisData(this, ircData[3], msg);
                            }
                            break;
                        case "223":     //whois charset is UTF-8
                        case "264":     //whois using encrypted connection
                        case "307":     //whois information nick ips
                        case "310":     //whois is available for help
                        case "313":     //whois information is an IRC operator
                            nick = ircData[3];
                            check = UserInfoWindowExists(this, nick);
                            if (check)
                                return;
                            msg = JoinString(ircData, 4, true);
                            WhoisData(this, ircData[3], msg);
                            break;
                        case "317":     //whois information signon time
                            DateTime date1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                            date1 = date1.AddSeconds(Convert.ToDouble(ircData[5]));
                            nick = ircData[3];
                            check = UserInfoWindowExists(this, nick);
                            if (check)
                            {
                                //UserInfoIdleLogon(this, nick, GetDuration(Convert.ToInt32(ircData[4])) + " " + JoinString(ircData, 6, true), date1.ToShortTimeString() + " " + date1.ToShortDateString());
                                UserInfoIdleLogon(this, nick, GetDuration(Convert.ToInt32(ircData[4])), date1.ToShortTimeString() + " " + date1.ToShortDateString());
                            }
                            else
                            {
                                msg = GetDuration(Convert.ToInt32(ircData[4])) + " " + JoinString(ircData, 6, true) + " " + date1.ToShortTimeString() + " " + date1.ToShortDateString();
                                WhoisData(this, ircData[3], msg);
                            }
                            break;
                        case "318":     //whois information end of whois
                            nick = ircData[3];
                            check = UserInfoWindowExists(this, nick);
                            if (check)
                                return;
                            msg = JoinString(ircData, 4, false);
                            WhoisData(this, ircData[3], msg);
                            break;
                        case "319":     //whois information channels
                            nick = ircData[3];
                            check = UserInfoWindowExists(this, nick);
                            if (check)
                            {
                                string[] chans = JoinString(ircData, 4, true).Split(' ');
                                UserInfoAddChannels(this, nick, chans);
                            }
                            else
                            {
                                msg = "is on: " + JoinString(ircData, 4, true);
                                WhoisData(this, ircData[3], msg);
                            }
                            break;
                        case "320":     //whois information
                            nick = ircData[3];
                            check = UserInfoWindowExists(this, nick);
                            if (check)
                                return;
                            msg = JoinString(ircData, 4, true);
                            WhoisData(this, ircData[3], msg);
                            break;
                        case "330":     //whois information
                            nick = ircData[3];
                            check = UserInfoWindowExists(this, nick);
                            if (check)
                                return;
                            msg = JoinString(ircData, 5, true) + " " + ircData[4];
                            WhoisData(this, ircData[3], msg);
                            break;
                        case "334":
                            nick = ircData[3];
                            check = UserInfoWindowExists(this, nick);
                            if (check)
                                return;
                            msg = JoinString(ircData, 4, false);
                            WhoisData(this, ircData[3], msg);
                            break;
                        case "335":     //whois information
                            nick = ircData[3];
                            check = UserInfoWindowExists(this, nick);
                            if (check)
                                return;
                            msg = ircData[3] + " " + JoinString(ircData, 4, true);
                            WhoisData(this, ircData[3], msg);
                            break;
                        case "338":     //whois information
                            nick = ircData[3];
                            check = UserInfoWindowExists(this, nick);
                            if (check)
                                return;
                            if (ircData[6].StartsWith(":"))
                                msg = JoinString(ircData, 6, true) + " " + ircData[4] + " " + ircData[5];
                            else
                                msg = JoinString(ircData, 5, true) + " " + ircData[4];
                            WhoisData(this, ircData[3], msg);
                            break;
                        case "378":     //whois information
                            nick = ircData[3];
                            check = UserInfoWindowExists(this, nick);
                            if (check)
                                return;
                            msg = RemoveColon(ircData[4]) + " " + JoinString(ircData, 5, true);
                            WhoisData(this, ircData[3], msg);
                            break;
                        case "379":     //whois information
                            nick = ircData[3];
                            check = UserInfoWindowExists(this, nick);
                            if (check)
                                return;
                            msg = RemoveColon(ircData[4]) + " " + JoinString(ircData, 5, true);
                            WhoisData(this, ircData[3], msg);
                            break;
                        case "314":  //whowas information
                            msg = ircData[4] + "!" + ircData[5] + " " + ircData[6] + " " + JoinString(ircData,7,true);
                            WhoisData(this, ircData[3], msg);
                            break;
                        case "369":
                        case "406": //whowas information
                            msg = JoinString(ircData, 4, false);
                            WhoisData(this, ircData[3], msg);
                            break;
                        case "275":
                        case "671":     //using secure connection
                            nick = ircData[3];
                            check = UserInfoWindowExists(this, nick);
                            if (check)
                                return;
                            msg = JoinString(ircData, 4, true);
                            WhoisData(this, ircData[3], msg);
                            break;
                        case "321":     //start channel list
                            ChannelListStart(this);
                            break;
                        case "322":     //channel list
                            //3 4 rc(5+)
                            ChannelList(this, ircData[3], ircData[4], RemoveColon(JoinString(ircData, 5, true)));
                            break;
                        case "323": //end channel list
                            ChannelListEnd(this);
                            ServerMessage(this, "End of Channel List");
                            break;
                        case "324":     //channel modes
                            channel = ircData[3];
                            msg = "Channel modes for " + channel + " are :" + JoinString(ircData, 4, false);
                            ChannelMode(this, channel, "", channel, JoinString(ircData, 4, false));
                            GenericChannelMessage(this, channel, msg);
                            break;
                        case "328":     //channel url
                            channel = ircData[3];
                            msg = "Channel URL is " + JoinString(ircData, 4, true);
                            GenericChannelMessage(this, channel, msg);
                            break;
                        case "329":     //channel creation time
                            channel = ircData[3];
                            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                            date = date.AddSeconds(Convert.ToDouble(ircData[4]));
                            msg = "Channel Created on: " + date.ToShortTimeString() + " " + date.ToShortDateString();
                            GenericChannelMessage(this, channel, msg);
                            break;
                        case "331":     //no topic is set
                            channel = ircData[3];
                            check = ChannelInfoWindowExists(this, channel);
                            if (!check)
                                GenericChannelMessage(this, channel, "No Topic Set");
                            break;
                        case "332":     //channel topic
                            channel = ircData[3];
                            check = ChannelInfoWindowExists(this, channel);
                            if (!check)
                                ChannelTopic(this, channel, "", "", JoinString(ircData, 4, true));
                            break;
                        case "333":     //channel time
                            channel = ircData[3];
                            nick = ircData[4];
                            DateTime date2 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                            date2 = date2.AddSeconds(Convert.ToDouble(ircData[5]));

                            check = ChannelInfoWindowExists(this, channel);
                            if (check)
                            {
                                ChannelInfoTopicSet(this, channel, nick, date2.ToShortTimeString() + " " + date2.ToShortDateString());
                            }
                            else
                            {
                                msg = "Channel Topic Set by: " + nick + " on " + date2.ToShortTimeString() + " " + date2.ToShortDateString();
                                GenericChannelMessage(this, channel, msg);
                            }

                            break;
                        case "343":
                            ServerMessage(this, JoinString(ircData, 3, false));
                            break;
                        case "348": //channel exception list
                            channel = ircData[3];
                            //3 is channel
                            //4 is host
                            //5 added by
                            //6 added time
                            check = ChannelInfoWindowExists(this, channel);
                            if (check)
                            {
                                DateTime date4 = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Convert.ToDouble(ircData[6]));
                                ChannelInfoAddException(this, channel, ircData[4], NickFromFullHost(ircData[5]) + " on " + date4.ToShortTimeString() + " " + date4.ToShortDateString());
                            }
                            else
                            {
                                ServerMessage(this, JoinString(ircData, 3, false));
                            }
                            break;
                        case "349": //end of channel exception list
                            break;
                        case "315": //end of who reply
                            channel = ircData[3];
                            EndofWhoReply(this, channel);
                            break;
                        case "352": //who reply
                            channel = ircData[3];
                            //user flags in ircData[8] H-here G-gone(away) * - irc operator x-hiddenhost d-deaf
                            //server hops ircData[9]
                            WhoReply(this, channel, ircData[7], ircData[4] + "@" + ircData[5], ircData[8], JoinString(ircData, 3, false));
                            break;
                        case "353": //channel user list
                            channel = ircData[4];
                            ChannelUserList(this, channel, JoinString(ircData, 5, true).Split(' '), JoinString(ircData, 4, true));
                            break;
                        case "365":  //End of Links
                            ServerMessage(this, JoinString(ircData, 4, true));
                            break;
                        case "366":     //end of names
                            channel = ircData[3];
                            //channel is fully joined                            
                            EndofNames(this, channel);
                            break;
                        case "367": //channel ban list
                            channel = ircData[3];
                            //3 is channel
                            //4 is host
                            //5 banned by
                            //6 ban time
                            check = ChannelInfoWindowExists(this, channel);
                            if (check)
                            {
                                DateTime date3 = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Convert.ToDouble(ircData[6]));
                                ChannelInfoAddBan(this, channel, ircData[4], ircData[5] + " on " + date3.ToShortTimeString() + " " + date3.ToShortDateString());
                            }
                            else
                            {
                                ServerMessage(this, JoinString(ircData, 3, false));
                            }
                            break;
                        case "368": //end of channel ban list
                            break;
                        case "377":
                            ServerMessage(this, JoinString(ircData, 6, true));
                            break;
                        case "372": //motd
                        case "375":
                            msg = JoinString(ircData, 3, true);
                            if (serverSetting.ShowMOTD || serverSetting.ForceMOTD)
                                ServerMOTD(this, msg);
                            break;
                        case "376": //end of motd
                        case "422": //missing motd
                            if (serverSetting.ForceMOTD)
                            {
                                serverSetting.ForceMOTD = false;
                                return;
                            }

                            if (fullyConnected)
                                return;

                            ServerMessage(this, "You have successfully connected to " + serverSetting.RealServerName);

                            //create default 005 responses if they are blank
                            if (serverSetting.StatusModes == null)
                            {
                                serverSetting.StatusModes = new char[2][];
                                serverSetting.StatusModes[0] = "ov".ToCharArray();
                                serverSetting.StatusModes[1] = "@+".ToCharArray();
                            }

                            if (serverSetting.ChannelTypes == null)
                                serverSetting.ChannelTypes = "#".ToCharArray();

                            if (serverSetting.ChannelModeAddress == null)
                            {
                                string[] modes = "b,k,l,imnpstrDducCNMT".Split(',');
                                serverSetting.ChannelModeAddress = modes[0];
                                serverSetting.ChannelModeParam = modes[1];
                                serverSetting.ChannelModeParamNotRemove = modes[2];
                                serverSetting.ChannelModeNoParam = modes[3];
                            }

                            if (serverSetting.SetModeI)
                                SendData("MODE " + serverSetting.CurrentNickName + " +i");

                            //run autoperform
                            if (serverSetting.AutoPerformEnable && serverSetting.AutoPerform != null)
                            {
                                ServerMessage(this, "Running AutoPerform command(s)..");
                                AutoPerform(this, serverSetting.AutoPerform);
                            }

                            // Nickserv password
                            if (serverSetting.NickservPassword != null && serverSetting.NickservPassword.Length > 0)
                            {
                                if (serverSetting.NickservMask)
                                    OutGoingCommand(this, "/msgsec NickServ identify " + serverSetting.NickservPassword);
                                else
                                    OutGoingCommand(this, "/msg NickServ identify " + serverSetting.NickservPassword);
                            }

                            if (serverSetting.RejoinChannels)
                            {
                                //rejoin any channels that are open
                                AutoRejoin(this);
                            }

                            //run autojoins
                            if (serverSetting.AutoJoinEnable && serverSetting.AutoJoinChannels != null)
                            {
                                ServerMessage(this, "Auto-joining Channels");
                                AutoJoin(this, serverSetting.AutoJoinChannels);
                            }

                            fullyConnected = true;

                            RefreshServerTree(this);

                            //read the command queue
                            if (commandQueue.Count > 0)
                            {
                                foreach (string command in commandQueue)
                                    SendData(command);
                            }
                            commandQueue.Clear();

                            BuddyListCheck();
                            buddyListTimer.Start();

                            if (ServerFullyConnected != null)
                                ServerFullyConnected(this);
                            
                            break;
                        case "396":     //mode X message                            
                            msg = ircData[3] + " " + JoinString(ircData, 4, true);
                            ServerMessage(this, msg);
                            break;
                        case "439":
                        case "931":
                            ServerMessage(this, JoinString(ircData, 3, true));
                            break;
                        case "901":
                            ServerMessage(this, JoinString(ircData, 6, true));
                            break;
                        case "PRIVMSG":
                            channel = ircData[2];
                            msg = JoinString(ircData, 3, true);

                            if (CheckIgnoreList(nick, host)) return;

                            if (channel.ToLower() == serverSetting.CurrentNickName.ToLower())
                            {
                                //this is a private message to you
                                //check if it was an notice/action
                                if (msg[0] == (char)1)
                                {
                                    //drop the 1st and last CTCP Character
                                    msg = msg.Trim(new char[] { (char)1 });
                                    //check for action
                                    switch (msg.Split(' ')[0].ToUpper())
                                    {
                                        case "ACTION":
                                            msg = msg.Substring(6);
                                            QueryAction(this, nick, host, msg);
                                            IALUserData(this, nick, host, "");
                                            break;
                                        case "VERSION":
                                        case "ICECHAT":
                                        case "TIME":
                                        case "PING":
                                        case "USERINFO":
                                        case "CLIENTINFO":
                                        case "SOURCE":
                                        case "FINGER":
                                            CtcpMessage(this, nick, msg.Split(' ')[0].ToUpper(), msg.Substring(msg.IndexOf(" ") + 1));
                                            break;
                                        default:
                                            //check for DCC SEND, DCC CHAT, DCC ACCEPT, DCC RESUME
                                            if (msg.ToUpper().StartsWith("DCC SEND"))
                                            {
                                                msg = msg.Substring(8).Trim();
                                                System.Diagnostics.Debug.WriteLine("PRIVMSG:" + msg);

                                                string[] dccData = msg.Split(' ');
                                                //sometimes the filenames can be include in quotes
                                                System.Diagnostics.Debug.WriteLine("length:" + dccData.Length);
                                                //PRIVMSG Snerf :DCC SEND serial.txt 1614209982 20052 71

                                                if (dccData.Length > 4)
                                                {
                                                    uint uresult;
                                                    if (!uint.TryParse(dccData[dccData.Length - 1], out uresult))
                                                    {
                                                        return;
                                                    }

                                                    //there can be a passive dcc request sent
                                                    //PegMan-default(2010-05-07)-OS.zip 4294967295 0 176016 960
                                                    //960 is the passive DCC ID
                                                    //length:5
                                                    //960:176016:0:
                                                    if (dccData[dccData.Length - 3] == "0")
                                                    {
                                                        //passive DCC
                                                        string id = dccData[dccData.Length - 1];
                                                        uint fileSize = uint.Parse(dccData[dccData.Length - 2]);
                                                        string port = dccData[dccData.Length - 3];
                                                        string ip = dccData[dccData.Length - 4];
                                                        string file = "";
                                                        if (msg.Contains("\""))
                                                        {
                                                            string[] words = msg.Split('"');
                                                            if (words.Length == 3)
                                                                file = words[1];
                                                            System.Diagnostics.Debug.WriteLine(words.Length);
                                                            foreach (string w in words)
                                                            {
                                                                System.Diagnostics.Debug.WriteLine(w);
                                                            }
                                                        }
                                                        else
                                                            file = dccData[dccData.Length - 5];

                                                        //start up a listening socket on a specific port and send back to ip
                                                        //http://trout.snt.utwente.nl/ubbthreads/ubbthreads.php?ubb=showflat&Number=139329&site_id=1#import

                                                        System.Diagnostics.Debug.WriteLine("PASSIVE DCC " + id + ":" + fileSize + ":" + port + ":" + ip + ":" + file);
                                                        if (DCCPassive != null)
                                                            DCCPassive(this, nick, host, ip, file, fileSize, 0, false, id);

                                                        return;
                                                    }
                                                    else
                                                    {
                                                        uint fileSize = uint.Parse(dccData[dccData.Length - 1]);
                                                        string port = dccData[dccData.Length - 2];
                                                        string ip = dccData[dccData.Length - 3];
                                                        string file = "";
                                                        if (msg.Contains("\""))
                                                        {
                                                            string[] words = msg.Split('"');
                                                            if (words.Length == 3)
                                                                file = words[1];
                                                            System.Diagnostics.Debug.WriteLine(words.Length);
                                                            foreach (string w in words)
                                                            {
                                                                System.Diagnostics.Debug.WriteLine(w);
                                                            }
                                                        }

                                                        System.Diagnostics.Debug.WriteLine(fileSize + ":" + port + ":" + ip + ":" + file);
                                                        //check that values are numbers

                                                        if (DCCFile != null && file.Length > 0)
                                                            DCCFile(this, nick, host, port, ip, file, fileSize, 0, false);
                                                        return;
                                                    }
                                                }
                                                //string fileName = dccData[0];
                                                //string ip = dccData[1];
                                                //string port = dccData[2];
                                                //string fileSize = dccData[3];
                                                System.Diagnostics.Debug.WriteLine("DCC SEND:" + dccData[0] + "::" + dccData[1] + "::" + dccData[2] + "::" + dccData[3]);

                                                //check if filesize is a valid number
                                                uint result;
                                                if (!uint.TryParse(dccData[3], out result))
                                                    return;

                                                //check if quotes around file name
                                                if (dccData[0].StartsWith("\"") && dccData[0].EndsWith("\""))
                                                {
                                                    dccData[0] = dccData[0].Substring(1, dccData[0].Length - 2);
                                                }

                                                if (DCCFile != null)
                                                    DCCFile(this, nick, host, dccData[2], dccData[1], dccData[0], uint.Parse(dccData[3]), 0, false);
                                                else
                                                    ServerError(this, "Invalid DCC File send from " + nick, true);
                                            }
                                            else if (msg.ToUpper().StartsWith("DCC RESUME"))
                                            {
                                                //dcc resume, other client requests resuming a file
                                                //PRIVMSG User1 :DCC RESUME "filename" port position
                                                System.Diagnostics.Debug.WriteLine("DCC RESUME:" + data);
                                                //send back a DCC ACCEPT MESSAGE

                                            }
                                            else if (msg.ToUpper().StartsWith("DCC ACCEPT"))
                                            {
                                                //dcc accept, other client accepts the dcc resume
                                                //PRIVMSG User2 :DCC ACCEPT file.ext port position
                                                //System.Diagnostics.Debug.WriteLine("DCC ACCEPT:" + data);
                                                msg = msg.Substring(10).Trim();
                                                System.Diagnostics.Debug.WriteLine("ACCEPT:" + msg);
                                                string[] dccData = msg.Split(' ');
                                                System.Diagnostics.Debug.WriteLine("length:" + dccData.Length);
                                                
                                                //ACCEPT:"epson13792.exe" 5010 68513792
                                                //length:3

                                                if (DCCFile != null)
                                                    DCCFile(this, nick, host, dccData[dccData.Length - 2], "ip", "file", 0, uint.Parse(dccData[dccData.Length - 1]), true);


                                            }
                                            else if (msg.ToUpper().StartsWith("DCC CHAT"))
                                            {
                                                string ip = ircData[6];
                                                string port = ircData[7].TrimEnd(new char[] { (char)1 });
                                                if (DCCChat != null)
                                                    DCCChat(this, nick, host, port, ip);
                                            }
                                            else
                                                UserNotice(this, nick, msg);
                                            break;
                                    }
                                }
                                else
                                {
                                    QueryMessage(this, nick, host, msg);
                                    IALUserData(this, nick, host, "");
                                }
                            }
                            else
                            {
                                if (msg[0] == (char)1)
                                {
                                    //some clients dont end with a CTCP character
                                    if (msg.EndsWith( ((char)1).ToString()))
                                        msg = msg.Substring(1, msg.Length - 2);
                                    else
                                        msg = msg.Substring(1);
                                    
                                    switch (msg.Split(' ')[0].ToUpper())
                                    {
                                        case "ACTION":
                                            msg = msg.Substring(7);
                                            ChannelAction(this, channel, nick, host, msg);
                                            IALUserData(this, nick, host, channel);
                                            break;
                                        case "VERSION":
                                        case "TIME":
                                        case "PING":
                                        case "USERINFO":
                                        case "CLIENTINFO":
                                        case "SOURCE":
                                        case "FINGER":
                                            //we need to send a reply
                                            CtcpMessage(this, nick, msg.Split(' ')[0].ToUpper(), msg);
                                            break;
                                        default:
                                            if (msg.ToUpper().StartsWith("ACTION "))
                                            {
                                                msg = msg.Substring(7);
                                                ChannelAction(this, channel, nick, host, msg);
                                                IALUserData(this, nick, host, channel);
                                            }
                                            else
                                            {
                                                ChannelNotice(this, nick, host, (char)32, channel, msg);
                                                IALUserData(this, nick, host, channel);
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    if (ChannelMessage != null)
                                        ChannelMessage(this, channel, nick, host, msg);
                                    IALUserData(this, nick, host, channel);

                                }
                            }
                            break;
                        case "INVITE":      //channel invite
                            channel = ircData[3];
                            ChannelInvite(this, channel, nick, host);
                            break;

                        case "NOTICE":
                            msg = JoinString(ircData, 3, true);
                            //check if its a user notice or a server notice
                            if (nick.ToLower() == serverSetting.RealServerName.ToLower())
                                ServerNotice(this, msg);
                            else
                            {
                                if (CheckIgnoreList(nick, host)) return;

                                if (initialLogon && serverSetting.StatusMSG == null && serverSetting.StatusModes != null)
                                {
                                    serverSetting.StatusMSG = new char[serverSetting.StatusModes[1].Length];
                                    for (int j = 0; j < serverSetting.StatusModes[1].Length; j++)
                                        serverSetting.StatusMSG[j] = serverSetting.StatusModes[1][j];
                                }
                                if (initialLogon && serverSetting.ChannelTypes != null && Array.IndexOf(serverSetting.ChannelTypes, ircData[2][0]) != -1)
                                {
                                    ChannelNotice(this, nick, host, '0', ircData[2], msg);
                                    IALUserData(this, nick, host, ircData[2]);
                                }
                                else if (initialLogon && serverSetting.StatusMSG != null && Array.IndexOf(serverSetting.StatusMSG, ircData[2][0]) != -1 && Array.IndexOf(serverSetting.ChannelTypes, ircData[2][1]) != -1)
                                {
                                    ChannelNotice(this, nick, host, ircData[2][0], ircData[2].Substring(1), msg);
                                    IALUserData(this, nick, host, ircData[2]);
                                }
                                else
                                {
                                    //System.Diagnostics.Debug.WriteLine("NOTICE:" + msg);
                                    if (msg.ToUpper().StartsWith("DCC SEND"))
                                    {
                                        System.Diagnostics.Debug.WriteLine("NOTICE DCC SEND:" + nick + ":" + msg);
                                        UserNotice(this, nick, msg);
                                    }
                                    else if (msg.ToUpper().StartsWith("DCC CHAT"))
                                    {
                                        UserNotice(this, nick, msg);
                                    }
                                    else
                                    {
                                        if (msg[0] == (char)1)
                                        {
                                            msg = msg.Substring(1, msg.Length - 2);
                                            string ctcp = msg.Split(' ')[0].ToUpper();
                                            msg = msg.Substring(msg.IndexOf(" ") + 1);
                                            switch (ctcp)
                                            {
                                                case "PING":
                                                    int result;
                                                    if (Int32.TryParse(msg, out result))
                                                    {
                                                        int diff = System.Environment.TickCount - Convert.ToInt32(msg);
                                                        
                                                        System.Diagnostics.Debug.WriteLine(msg + ":" + System.Environment.TickCount + ":" + diff);
                                                        
                                                        msg = GetDurationMS(diff);
                                                    }
                                                    if (CtcpReply != null)
                                                        CtcpReply(this, nick, ctcp, msg);
                                                    break;
                                                default:
                                                    if (CtcpReply != null)
                                                        CtcpReply(this, nick, ctcp, msg);
                                                    break;
                                            }
                                        }
                                        else
                                            UserNotice(this, nick, msg);
                                    }
                                }
                            }
                            break;

                        case "MODE":
                            channel = ircData[2];

                            if (channel.ToLower() == serverSetting.CurrentNickName.ToLower())
                            {
                                if (host.IndexOf('@') > -1 && this.serverSetting.LocalIP == null)
                                {
                                    this.serverSetting.LocalHost = host;
                                    /*
                                    try
                                    {
                                        host = host.Substring(host.IndexOf('@') + 1);
                                        System.Net.IPAddress[] addresslist = System.Net.Dns.GetHostAddresses(host);
                                        foreach (System.Net.IPAddress address in addresslist)
                                            this.serverSetting.LocalIP = address;
                                    }
                                    catch (Exception)
                                    {
                                        //can not parse the mode
                                    }
                                    */
                                }
                                //user mode
                                tempValue = JoinString(ircData, 3, true);
                                UserMode(this, channel, tempValue);
                            }
                            else
                            {
                                //channel mode
                                tempValue = JoinString(ircData, 3, false);

                                ChannelMode(this, nick, HostFromFullHost(ircData[0]), channel, tempValue);
                            }
                            break;

                        case "JOIN":
                            channel = RemoveColon(ircData[2]);
                            //extended-join is below
                            string account = "";
                            if (ircData.Length > 3)
                            {
                                //extended join
                                account = ircData[3];
                            }
                                
                            //this is normal
                            //check if it is our own nickname
                            if (nick.ToLower() == serverSetting.CurrentNickName.ToLower())
                            {
                                JoinChannelMyself(this, channel, host, account);
                                SendData("MODE " + channel);
                            }
                            else
                            {
                                IALUserData(this, nick, host, channel);
                                JoinChannel(this, channel, nick, host, account, true);
                            }
                            break;

                        case "PART":
                            channel = RemoveColon(ircData[2]);
                            tempValue = JoinString(ircData, 3, true); //part reason
                            //check if it is our own nickname
                            if (nick.ToLower() == serverSetting.CurrentNickName.ToLower())
                            {
                                //part self
                                PartChannelMyself(this, channel);
                            }
                            else
                            {
                                tempValue = JoinString(ircData, 3, true);
                                IALUserPart(this, nick, channel);
                                PartChannel(this, channel, nick, host, tempValue);
                            }
                            break;

                        case "QUIT":
                            nick = NickFromFullHost(RemoveColon(ircData[0]));
                            host = HostFromFullHost(RemoveColon(ircData[0]));
                            tempValue = JoinString(ircData, 2, true);

                            QuitServer(this, nick, host, tempValue);
                            IALUserQuit(this, nick);
                            break;

                        case "NICK":
                            //old nickname
                            nick = NickFromFullHost(RemoveColon(ircData[0]));
                            host = HostFromFullHost(RemoveColon(ircData[0]));

                            //new nickname
                            tempValue = RemoveColon(ircData[2]);

                            if (nick.ToLower() == serverSetting.CurrentNickName.ToLower())
                            {
                                //if it is your own nickname, update it
                                serverSetting.CurrentNickName = tempValue;
                            }

                            IALUserChange(this, nick, tempValue);
                            ChangeNick(this, nick, tempValue, HostFromFullHost(ircData[0]));

                            break;

                        case "KICK":
                            msg = JoinString(ircData, 4, true);  //kick message                        
                            channel = ircData[2];
                            //this is WHO got kicked
                            nick = ircData[3];
                            //check if it is our own nickname who got kicked
                            if (nick.ToLower() == serverSetting.CurrentNickName.ToLower())
                            {
                                //we got kicked
                                ChannelKickSelf(this, channel, msg, ircData[0]);
                            }
                            else
                            {
                                ChannelKick(this, channel, nick, msg, ircData[0]);
                                IALUserPart(this, nick, channel);
                            }
                            break;
                        case "PONG":
                            pongTimer.Stop();
                            pongTimer.Start();
                            //:servercentral.il.us.quakenet.org PONG servercentral.il.us.quakenet.org :servercentral.il.us.quakenet.org
                            
                            break;
                        case "TOPIC":   //channel topic change
                            channel = ircData[2];
                            msg = JoinString(ircData, 3, true);
                            ChannelTopic(this, channel, nick, host, msg);
                            break;
                        case "AUTH":    //NOTICE AUTH
                            ServerMessage(this, JoinString(ircData, 2, true));
                            break;

                        /*************  IRCV3 extras *******************/
                        case "AWAY": //IRC v3
                            msg = JoinString(ircData, 2, true);
                            if (msg.Length == 0)
                            {
                                //nick is no longer away
                                ServerMessage(this, nick + " is no longer away");
                                IALUserDataAwayOnly(this, nick, false, "");
                            }
                            else
                            {
                                ServerMessage(this, nick + " is set as away ("+msg+")");
                                IALUserDataAwayOnly(this, nick, true, msg);                            
                            }
                            break;

                        case "301": //whois reply, away reason
                            nick = ircData[3];                            
                            check = UserInfoWindowExists(this, nick);
                            msg = JoinString(ircData, 4, true);
                            if (check)
                                UserInfoAwayStatus(this, nick, msg);
                            else
                            {
                                if (msg.Length > 0)
                                    IALUserDataAwayOnly(this, nick, true, msg);
                                else
                                    IALUserDataAwayOnly(this, nick, false, "");
                                
                                WhoisData(this, nick, "is away: " + msg);
                            }
                            break;
                        case "305": //no longer marked away
                            msg = JoinString(ircData, 3, true);
                            ServerMessage(this, msg);
                            break;
                        case "306": //marked as away
                            msg = JoinString(ircData, 3, true);
                            ServerMessage(this, msg);
                            break;
                        
                        case "ACCOUNT":
                            //:nick!user@host ACCOUNT accountname
                            //:nick!user@host ACCOUNT *
                            if (ircData[2] == "*")
                            {
                                //nick has logged out
                                UserNotice(this, nick, "logged out of account");
                            }
                            else
                            {
                                UserNotice(this, nick, "logged in with account (" + ircData[2] + ")");
                            }
                            break;    
                        case "CAP": //ircv3 
                            //:sendak.freenode.net CAP * LS :account-notify extended-join identify-msg multi-prefix sasl
                        
                            //<- :totoro.staticbox.net CAP * LS :account-notify away-notify extended-join multi-prefix sasl tls
                            //-> irc.atheme.org CAP REQ :multi-prefix
                            //<- :totoro.staticbox.net CAP Snerf8 ACK :multi-prefix 
                            //-> irc.atheme.org CAP END
                            //<- :totoro.staticbox.net CAP Snerf8 NAK :multi-prefix 
                            
                            tempValue = JoinString(ircData, 4, true); //Capabilities
                            //System.Diagnostics.Debug.WriteLine(ircData.Length + ":" + tempValue);
                            if (ircData[3] == "LS")
                            {
                                string sendREQ = "";
                                if (tempValue.IndexOf("multi-prefix") > -1)
                                    sendREQ += "multi-prefix ";
                                                               
                                //check if option is enabled
                                if (this.serverSetting.UseIdentifyMsg)
                                    if (tempValue.IndexOf("identify-msg") > -1)
                                        sendREQ += "identify-msg ";
                                
                                if (tempValue.IndexOf("extended-join") > -1)
                                    sendREQ += "extended-join ";
                                
                                if (tempValue.IndexOf("account-notify") > -1)
                                    sendREQ += "account-notify ";

                                if (tempValue.IndexOf("away-notify") > -1)
                                    sendREQ += "away-notify ";

                                if (tempValue.IndexOf("sasl") > -1)
                                    if (serverSetting.UseSASL)
                                        sendREQ += "sasl ";

                                //if (tempValue.IndexOf("tls") > -1)
                                //    sendREQ += "tls ";

                                if (sendREQ.Length > 0)
                                    SendData("CAP REQ :" + sendREQ.Trim());                                    
                            }
                            else if (ircData[3] == "NAK")
                            {
                                //
                                SendData("CAP END");
                            }
                            else if (ircData[3] == "ACK")
                            {
                                if (tempValue.IndexOf("extended-join") > -1)
                                {
                                    //extended join is enabled
                                    //:nick!user@host JOIN #channelname accountname :Real Name
                                    //:nick!user@host JOIN #channelname * :Real Name
                                    serverSetting.ExtendedJoin = true;
                                }

                                if (tempValue.IndexOf("account-notify") > -1)
                                {
                                    //account-notify                                    
                                    //System.Diagnostics.Debug.WriteLine("account notify enabled");
                                }

                                if (tempValue.IndexOf("away-notify") > -1)
                                {
                                    //extended away is turned on
                                    serverSetting.AwayNotify = true;
                                }

                                if (tempValue.IndexOf("tls") > -1)
                                {
                                    //SendData("STARTTLS");
                                }

                                //::sendak.freenode.net CAP Snerf_ ACK :multi-prefix 
                                if (tempValue.IndexOf("sasl") > -1)
                                {
                                    //sasl is enabled
                                    //send PLAIN auth
                                    if (serverSetting.UseSASL)
                                        SendData("AUTHENTICATE PLAIN");
                                }
                                else
                                    SendData("CAP END");
                            }
                            break;
                        case "670":
                            //totoro.staticbox.net 670 Snerf :STARTTLS successful, proceed with TLS handshake
                            //http://ircv3.org/extensions/tls-3.1
                            ServerMessage(this, JoinString(ircData, 3, true));                            
                            break;

                        case "900": //SASL logged in as ..
                            ServerMessage(this, JoinString(ircData, 5, true));
                            break;
                        case "903": //SASL authentication successful
                            ServerMessage(this, JoinString(ircData, 3, true));
                            SendData("CAP END");
                            break;
                        case "904": //SASL authentication failed
                        case "905":
                            //::sendak.freenode.net 904 Snerfus :SASL authentication failed
                            ServerMessage(this, JoinString(ircData, 3, true));
                            SendData("CAP END");
                            break;
                        case "906": //SASL aborted
                        case "907": //SASL Already authenticated error
                            ServerMessage(this, JoinString(ircData, 3, true));
                            break;

                        /*************  IRCV3 extras *******************/


                        case "501": //unknown MODE
                            ServerMessage(this, JoinString(ircData, 3, true));
                            break;
                        //errors
                        case "404": //can not send to channel
                        case "467": //channel key already set
                        case "482": //not a channel operator
                            msg = JoinString(ircData, 4, true);
                            GenericChannelMessage(this, ircData[3], msg);
                            break;
                        case "432": //erroneus nickname
                        case "438": //nick change too fast
                        case "468": //only servers can change mode
                        case "485": //cant join channel
                        case "493": //user doesnt want message
                            msg = ircData[3] + " " + JoinString(ircData, 4, true);                            
                            ServerError(this, msg, true);
                            break;
                        case "401": //no such nick
                        case "402": //no such server
                        case "403": //no such channel
                        case "405": //joined too many channels
                        case "407": //no message delivered
                        case "411": //no recipient given
                        case "412": //no text to send
                        case "421": //unknown command
                        case "431": //no nickname given
                        case "470": //forward to other channel
                        case "471": //can not join channel (limit enforced)
                        case "472": //unknown char to me (channel mode)
                            ServerError(this, JoinString(ircData, 3, false), true);
                            break;
                        case "473": //can not join channel invite only
                            msg = ircData[3] + " " + JoinString(ircData, 4, true);
                            ServerError(this, msg, true);
                            break;
                        case "442": //you're not in that channel
                        case "474": //Cannot join channel (+b)
                        case "475": //Cannot join channel (+k)
                            msg = ircData[3] + " " + JoinString(ircData, 4, true);
                            ServerError(this, msg, true);
                            break;

                        case "433": //nickname in use
                            if (fullyConnected == false)
                                serverSetting.RealServerName = ircData[0];

                            ServerMessage(this, JoinString(ircData, 4, true));

                            if (!initialLogon)
                            {
                                if (serverSetting.NickName == serverSetting.CurrentNickName)
                                {
                                    SendData("NICK " + serverSetting.AltNickName);
                                    ChangeNick(this, serverSetting.CurrentNickName, serverSetting.AltNickName, HostFromFullHost(RemoveColon(ircData[0])));
                                    triedAltNickName = true;

                                    //serverSetting.NickName = serverSetting.AltNickName;
                                    //serverSetting.AltNickName = nick;
                                }
                                else if (serverSetting.CurrentNickName != serverSetting.AltNickName)
                                {
                                    if (!triedAltNickName)
                                    {
                                        SendData("NICK " + serverSetting.AltNickName);
                                        ChangeNick(this, serverSetting.CurrentNickName, serverSetting.AltNickName, HostFromFullHost(RemoveColon(ircData[0])));
                                        triedAltNickName = true;
                                    }
                                    else
                                    {
                                        OutGoingCommand(this, "/addtext /nick "); 
                                        ServerMessage(this,"Choose a new nickname");


                                    }
                                    //SendData("NICK " + serverSetting.AltNickName);
                                }
                            }
                            else
                            {
                                //pick a random nick
                                /*
                                Random r = new Random();
                                string randNick = r.Next(10, 99).ToString();
                                if (serverSetting.NickName.Length + 2 <= serverSetting.MaxNickLength)
                                {
                                    serverSetting.NickName = serverSetting.NickName + randNick;
                                    SendData("NICK " + serverSetting.NickName);
                                }
                                else
                                {
                                    serverSetting.NickName = serverSetting.NickName.Substring(0, serverSetting.MaxNickLength - 2);
                                    serverSetting.NickName = serverSetting.NickName + randNick;
                                    SendData("NICK " + serverSetting.NickName);
                                }
                                */

                                //OutGoingCommand(this, "/addtext /nick "); 
                                //ServerMessage(this,"Choose a new nickname");
                            }

                            break;
                        case "465": //no open proxies
                        case "513": //if you can not connect, type /quote PONG ...
                            ServerError(this, JoinString(ircData, 3, true), true);
                            break;                        
                        
                        case "742": //freenode
                            //742 wanfu #icechat s nts :MODE cannot be set due to channel having－ an active MLOCK restriction policy
                            ChannelNotice(this, ircData[2], "", (char)32 ,ircData[3], JoinString(ircData,6,true));
                            //ServerNotice(this, JoinString(ircData, 3, false));
                            break;
                        default:
                            ServerMessage(this, JoinString(ircData, 3, false));
                            break;
                        //                            
                    }
                }
            }
            catch (Exception e)
            {
                WriteErrorFile(this, "ParseData", e);
            }
        }
        
        private bool CheckIgnoreList(string nick, string host)
        {
            if (!this.serverSetting.IgnoreListEnable) return false; //if ignore list is disabled, no match
            if (this.serverSetting.IgnoreList.Length == 0) return false;    //if no items in list, no match

            foreach (string ignore in serverSetting.IgnoreList)
            {
                if (!ignore.StartsWith(";"))    //check to make sure its not disabled
                {
                    //check for an exact match
                    if (nick.ToLower() == ignore.ToLower()) return true;

                    //check if we are looking for a host match
                    if (ignore.Contains("!") && ignore.Contains("@"))
                    {

                    }
                    else
                    {
                        //check for wildcard/regex match for nick name
                        if (Regex.Match(nick, ignore, RegexOptions.IgnoreCase).Success) return true;
                    }
                }
            }


            return false;
        }


        #region Parsing Methods

        private string GetDurationMS(int milliSseconds)
        {
            TimeSpan t = new TimeSpan(0, 0, 0, 0, milliSseconds);

            string s = t.Seconds.ToString() + "." + t.Milliseconds.ToString() + " secs";
            if (t.Minutes > 0)
                s = t.Minutes.ToString() + " mins " + s;
            if (t.Hours > 0)
                s = t.Hours.ToString() + " hrs " + s;
            if (t.Days > 0)
                s = t.Days.ToString() + " days " + s;

            return s;
        }

        private string GetDuration(int seconds)
        {
            TimeSpan t = new TimeSpan(0, 0, seconds);

            string s = t.Seconds.ToString() + " secs";
            if (t.Minutes > 0)
                s = t.Minutes.ToString() + " mins " + s;
            if (t.Hours > 0)
                s = t.Hours.ToString() + " hrs " + s;
            if (t.Days > 0)
                s = t.Days.ToString() + " days " + s;

            return s;
        }

        private string HostFromFullHost(string host)
        {
            if (host.IndexOf("!") > -1)
                return host.Substring(host.IndexOf("!") + 1);
            else
                return host;
        }

        private string NickFromFullHost(string host)
        {
            if (host.StartsWith(":"))
                host = host.Substring(1);

            if (host.IndexOf("!") > -1)
                return host.Substring(0, host.IndexOf("!"));
            else
                return host;
        }

        private string RemoveColon(string data)
        {
            if (data.StartsWith(":"))
                return data.Substring(1);
            else
                return data;
        }

        private string JoinString(string[] strData, int startIndex, bool removeColon)
        {
            if (startIndex > strData.GetUpperBound(0)) return "";

            string tempString = String.Join(" ", strData, startIndex, strData.GetUpperBound(0) + 1 - startIndex);
            if (removeColon)
            {
                tempString = RemoveColon(tempString);
            }
            return tempString;
        }

        #endregion

    }
}
