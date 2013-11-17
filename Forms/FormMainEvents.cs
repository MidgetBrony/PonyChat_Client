﻿/******************************************************************************\
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
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

using IceChat.Properties;
using IceChatPlugin;

namespace IceChat
{
    public partial class FormMain
    {
        private delegate void ShowDCCFileAcceptDelegate(IRCConnection connection, string nick, string host, string port, string ip, string file, uint fileSize, uint filePos, bool resume);
        private delegate void ShowDCCPassiveAcceptDelegate(IRCConnection connection, string nick, string host, string ip, string file, uint fileSize, uint filePos, bool resume, string id);

        private void OnRefreshServerTree(IRCConnection connection)
        {
            serverTree.Invalidate();
        }

        private void OnWriteErrorFile(IRCConnection connection, string method, Exception e)
        {
            WriteErrorFile(connection, method, e);
        }

        private void ShowTrayNotification(string message)
        {
            if (iceChatOptions.ShowSytemTrayNotifications)
            {
                if (this.notifyIcon.Visible)
                {
                    //check if we are in the foreground or not
                    if (!this.IsForeGround)
                    {
                        this.notifyIcon.BalloonTipText = message;
                        this.notifyIcon.ShowBalloonTip(5000);
                    }
                }
            }
        }

        private void OnServerReconnect(IRCConnection connection)
        {
            string msg = GetMessageFormat("Server Reconnect");
            msg = msg.Replace("$serverip", connection.ServerSetting.ServerIP).Replace("$server",connection.ServerSetting.ServerName).Replace("$port", connection.ServerSetting.ServerPort);
            
            OnServerMessage(connection, msg);
        }

        private void OnServerPreConnect(IRCConnection connection)
        {
            if (loadedPlugins.Count > 0)
            {
                PluginArgs args = new PluginArgs(connection);

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        ipc.ServerPreConnect(args);
                }
            }
        }


        private void OnServerForceDisconnect(IRCConnection connection)
        {
            if (IceChatOptions.ReconnectServer && connection.AttemptReconnect)
            {                
                OnServerMessage(connection, "Waiting 60 seconds to re-connect to (" + connection.ServerSetting.ServerName + ")");
                try
                {
                    if (connection.ReconnectTimer != null)
                    {
                        connection.ReconnectTimer.Stop();
                        connection.ReconnectTimer.Start();
                    } 
                    connection.DisconnectError = false;

                }
                catch (Exception)
                {
                    //do nada
                }
            }
            
            serverTree.Invalidate();

        }

        private void OnServerConnect(IRCConnection connection, string address)
        {
            string msg = FormMain.Instance.GetMessageFormat("Server Connect");

            if (connection.ServerSetting.UseBNC)
                msg = msg.Replace("$serverip", address).Replace("$server", connection.ServerSetting.BNCIP).Replace("$port", connection.ServerSetting.BNCPort) + " (BNC Connection)";
            else
            {
                if (connection.ServerSetting.UseProxy)
                {
                    string strProxyType = "HTTP";
                    if (connection.ServerSetting.ProxyType == 3)
                        strProxyType = "Socks 5";
                    if (connection.ServerSetting.ProxyType == 2)
                        strProxyType = "Socks 4";
                    
                    msg = msg.Replace("$serverip", address).Replace("$server", connection.ServerSetting.ServerName).Replace("$port", connection.ServerSetting.ServerPort) + " using " + strProxyType + " proxy " + connection.ServerSetting.ProxyIP + " on port " + connection.ServerSetting.ProxyPort;
                }
                else
                    msg = msg.Replace("$serverip", address).Replace("$server", connection.ServerSetting.ServerName).Replace("$port", connection.ServerSetting.ServerPort);
            }

            OnServerMessage(connection, msg);

            if (connection.ServerSetting.UseBNC)
                OnStatusText(connection, connection.ServerSetting.CurrentNickName + " connecting to " + connection.ServerSetting.BNCIP);
            else
            {
                if (connection.ServerSetting.UseProxy)
                    OnStatusText(connection, connection.ServerSetting.CurrentNickName + " connecting to " + connection.ServerSetting.ServerName + " using a proxy");
                else
                    OnStatusText(connection, connection.ServerSetting.CurrentNickName + " connecting to " + connection.ServerSetting.ServerName);
            }
            if (loadedPlugins.Count > 0)
            {
                PluginArgs args = new PluginArgs(mainChannelBar.GetTabPage("Console").TextWindow, "", connection.ServerSetting.CurrentNickName, "", msg);
                args.Connection = connection;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        ipc.ServerConnect(args);
                }
            }
        }

        private void OnServerDisconnect(IRCConnection connection)
        {
            string msg = GetMessageFormat("Server Disconnect");
            
            msg = msg.Replace("$serverip", connection.ServerSetting.ServerIP);
            msg = msg.Replace("$port", connection.ServerSetting.ServerPort);
            
            if (connection.ServerSetting.RealServerName.Length > 0)
                msg = msg.Replace("$server",connection.ServerSetting.RealServerName);
            else
                msg = msg.Replace("$server", connection.ServerSetting.ServerName);


            foreach (IceTabPage t in mainChannelBar.TabPages)
            {
                if (t.WindowStyle == IceTabPage.WindowType.Channel || t.WindowStyle == IceTabPage.WindowType.Query)
                {
                    if (t.Connection == connection)
                    {
                        t.ClearNicks();
                        t.IsFullyJoined = false;
                        t.GotNamesList = false;
                        t.GotWhoList = false;

                        t.TextWindow.AppendText(msg, 1);
                        t.LastMessageType = ServerMessageType.ServerMessage;

                        if (CurrentWindow == t)
                            nickList.Header = t.TabCaption + ":0";
                    }
                }
            }

            OnServerMessage(connection, msg);

            PlaySoundFile("dropped");

            if (loadedPlugins.Count > 0)
            {
                PluginArgs args = new PluginArgs(mainChannelBar.GetTabPage("Console").TextWindow, "", connection.ServerSetting.CurrentNickName, "", msg);
                args.Connection = connection;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        ipc.ServerDisconnect(args);
                }
            }
            
            ShowTrayNotification("You have disconnected from " + connection.ServerSetting.ServerName);

        }


        private void OnChannelInfoTopicSet(IRCConnection connection, string channel, string nick, string time)
        {
            IceTabPage t = FormMain.Instance.GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                if (t.HasChannelInfo)
                {
                    t.ChannelInfoForm.ChannelTopicSetBy(nick, time);
                }
            }                            
        }

        private void OnChannelInfoAddException(IRCConnection connection, string channel, string host, string bannedBy)
        {
            IceTabPage t = FormMain.Instance.GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                if (t.HasChannelInfo)
                {
                    t.ChannelInfoForm.AddChannelException(host, bannedBy);
                }
            }
        }

        private void OnChannelInfoAddBan(IRCConnection connection, string channel, string host, string bannedBy)
        {
            IceTabPage t = FormMain.Instance.GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                if (t.HasChannelInfo)
                {
                    t.ChannelInfoForm.AddChannelBan(host, bannedBy);
                }
            }        
        }


        private bool OnUserInfoWindowExists(IRCConnection connection, string nick)
        {
            if (connection.UserInfoWindow != null)
            {
                if (connection.UserInfoWindow.Text.ToLower() == "user information: " + nick.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        private void OnUserInfoIdleLogon(IRCConnection connection, string nick, string idleTime, string logonTime)
        {
            if (connection.UserInfoWindow != null)
            {
                ((FormUserInfo)connection.UserInfoWindow).IdleTime(idleTime);
                ((FormUserInfo)connection.UserInfoWindow).LogonTime(logonTime);
            }
        }

        private void OnUserInfoHostFullName(IRCConnection connection, string nick, string host, string full)
        {
            if (connection.UserInfoWindow != null)
            {
                ((FormUserInfo)connection.UserInfoWindow).HostName(host);
                ((FormUserInfo)connection.UserInfoWindow).FullName(full);
            }
        }

        private void OnUserInfoAddChannels(IRCConnection connection, string nick, string[] channels)
        {
            if (connection.UserInfoWindow != null)
            {
                foreach (string chan in channels)
                    ((FormUserInfo)connection.UserInfoWindow).Channel(chan);
            }
        }

        private void OnUserInfoAwayStatus(IRCConnection connection, string nick, string awayMessage)
        {
            if (connection.UserInfoWindow != null)
            {
                ((FormUserInfo)connection.UserInfoWindow).AwayStatus(awayMessage);
            }
        }

        private void OnUserInfoServer(IRCConnection connection, string nick, string server)
        {
            if (connection.UserInfoWindow != null)
            {
                ((FormUserInfo)connection.UserInfoWindow).Server(server);
            }
        }

        private bool OnChannelInfoWindowExists(IRCConnection connection, string channel)
        {
            IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                return t.HasChannelInfo;
            }
            return false;
        }

        private void OnStatusText(IRCConnection connection, string statusText)
        {
            //update the status bar
            if (CurrentWindowType == IceTabPage.WindowType.Console)
            {
                if (InputPanel.CurrentConnection == connection)
                {
                    StatusText(statusText);
                }
            }
        }

        private void OnAutoPerform(IRCConnection connection, string[] commands)
        {
            string autoCommand;

            PluginArgs args = new PluginArgs(connection);

            for (int x = 0; x < commands.Length; x++)
            {
                autoCommand = commands[x].Replace("\r", String.Empty);
                if (!autoCommand.StartsWith(";"))
                {
                    args.Command = autoCommand;
                    foreach (IPluginIceChat ipc in loadedPlugins)
                    {
                        if (ipc.Enabled == true)
                            args = ipc.AutoPerformCommand(args);
                    }
                    if (args.Command.Length > 0)                    
                        ParseOutGoingCommand(connection, args.Command);
                }
            }            
        }

        private void OnAutoRejoin(IRCConnection connection)
        {
            int x = 0;
            
            PluginArgs args = new PluginArgs(connection);
            
            foreach (IceTabPage tw in mainChannelBar.TabPages)
            {
                if (tw.WindowStyle == IceTabPage.WindowType.Channel)
                {
                    if (tw.Connection == connection)
                    {
                        
                        if (connection.ServerSetting.AutoJoinDelay)
                        {
                            if (connection.ServerSetting.AutoJoinDelayBetween)
                            {
                                args.Channel = tw.TabCaption;
                                foreach (IPluginIceChat ipc in loadedPlugins)
                                {
                                    if (ipc.Enabled == true)
                                        args = ipc.AutoJoinChannel(args);
                                }
                                if (args.Channel.Length > 0)
                                    ParseOutGoingCommand(connection, "/timer rejoin 1 " + (x + 6) + " /join " + args.Channel);
                            }
                            else
                            {
                                args.Channel = tw.TabCaption;
                                foreach (IPluginIceChat ipc in loadedPlugins)
                                {
                                    if (ipc.Enabled == true)
                                        args = ipc.AutoJoinChannel(args);
                                }
                                if (args.Channel.Length > 0)
                                    ParseOutGoingCommand(connection, "/timer rejoin 1 6 /join " + args.Channel);
                            }
                        }
                        else
                        {
                            if (connection.ServerSetting.AutoJoinDelayBetween)
                            {
                                args.Channel = tw.TabCaption;
                                foreach (IPluginIceChat ipc in loadedPlugins)
                                {
                                    if (ipc.Enabled == true)
                                        args = ipc.RejoinChannel(args);
                                }
                                if (args.Channel.Length > 0)
                                    ParseOutGoingCommand(connection, "/timer autojoin 1 " + (x + 1) + " /join " + args.Channel);

                            }
                            else
                            {
                                args.Channel = tw.TabCaption;
                                foreach (IPluginIceChat ipc in loadedPlugins)
                                {
                                    if (ipc.Enabled == true)
                                        args = ipc.RejoinChannel(args);
                                }
                                if (args.Channel.Length > 0)
                                    connection.SendData("JOIN " + args.Channel);
                            }
                        }
                    }
                    x++;
                }
            }
        }

        private void OnAutoJoin(IRCConnection connection, string[] channels)
        {
            PluginArgs args = new PluginArgs(connection);

            for (int x = 0; x < channels.Length; x++)
            {
                if (!channels[x].StartsWith(";"))
                {
                    if (connection.ServerSetting.AutoJoinDelay)
                    {
                        if (connection.ServerSetting.AutoJoinDelayBetween)
                        {
                            args.Channel = channels[x];
                            foreach (IPluginIceChat ipc in loadedPlugins)
                            {
                                if (ipc.Enabled == true)
                                    args = ipc.AutoJoinChannel(args);
                            }
                            if (args.Channel.Length > 0)
                                ParseOutGoingCommand(connection, "/timer autojoin 1 " + (x + 6) + " /join " + args.Channel);
                        }
                        else 
                        {
                            args.Channel = channels[x];
                            foreach (IPluginIceChat ipc in loadedPlugins)
                            {
                                if (ipc.Enabled == true)
                                    args = ipc.AutoJoinChannel(args);
                            }
                            if (args.Channel.Length > 0)
                                ParseOutGoingCommand(connection, "/timer autojoin 1 6 /join " + args.Channel);
                        }
                    }
                    else
                    {
                        if (connection.ServerSetting.AutoJoinDelayBetween)
                        {
                            args.Channel = channels[x];
                            foreach (IPluginIceChat ipc in loadedPlugins)
                            {
                                if (ipc.Enabled == true)
                                    args = ipc.AutoJoinChannel(args);
                            }
                            if (args.Channel.Length > 0)
                                ParseOutGoingCommand(connection, "/timer autojoin 1 " + (x + 1) + " /join " + args.Channel);
                        }
                        else
                        {
                            args.Channel = channels[x];
                            foreach (IPluginIceChat ipc in loadedPlugins)
                            {
                                if (ipc.Enabled == true)
                                    args = ipc.AutoJoinChannel(args);
                            }
                            if (args.Channel.Length > 0)
                                connection.SendData("JOIN " + args.Channel);
                        }
                    }
                }

            }
        }

        private void OnEndofNames(IRCConnection connection, string channel)
        {
            IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                t.GotNamesList = true;
                //send a WHO command to get all the hosts
                if (t.Nicks.Count < 200)
                {
                    t.GotWhoList = false;
                    connection.SendData("WHO " + t.TabCaption);
                }
                else
                    t.GotWhoList = true;

                t.IsFullyJoined = true;
                
                if (nickList.CurrentWindow == t)
                    nickList.RefreshList(t);
            }

        }

        private void OnBuddyListClear(IRCConnection connection)
        {
            buddyList.ClearBuddyList(connection);
        }

        private void OnBuddyList(IRCConnection connection, string[] buddies)
        {
            PluginArgs args = new PluginArgs(connection);
            
            foreach (BuddyListItem b in connection.ServerSetting.BuddyList)
            {
                if (b.IsOnSent && !b.IsOnReceived)
                {
                    bool isFound = false;
                    foreach (string buddy in buddies)
                    {
                        //this nick is connected
                        if (b.Nick.ToLower() == buddy.ToLower())
                        {
                            b.Connected = true;
                            b.IsOnReceived = true;
                            isFound = true;

                            if (loadedPlugins.Count > 0)
                            {
                                args.Nick = b.Nick;
                                args.Extra = "online";
                                foreach (IPluginIceChat ipc in loadedPlugins)
                                {
                                    if (ipc.Enabled == true)
                                        ipc.BuddyList(args);
                                }
                            }
                        }
                    }
                    if (!isFound)
                    {
                        b.Connected = false;
                        b.IsOnReceived = true;

                        args.Nick = b.Nick;
                        args.Extra = "offline";
                        if (loadedPlugins.Count > 0)
                        {
                            foreach (IPluginIceChat ipc in loadedPlugins)
                            {
                                if (ipc.Enabled == true)
                                    ipc.BuddyList(args);
                            }
                        }
                    }
                }
            }

            if (connection.buddiesIsOnSent == connection.ServerSetting.BuddyList.Length)
            {
                //reset all the isonsent values
                foreach (BuddyListItem buddy in connection.ServerSetting.BuddyList)
                {
                    buddy.IsOnSent = false;
                    buddy.IsOnReceived = false;
                }
                connection.buddiesIsOnSent = 0;

                //send a user event to refresh the buddy list for this server
                this.buddyList.ClearBuddyList(connection);

                foreach (BuddyListItem buddy in connection.ServerSetting.BuddyList)
                {
                    this.buddyList.UpdateBuddy(connection, buddy);
                }

            }

        }

        private void OnEndofWhoReply(IRCConnection connection, string channel)
        {
            IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                //end of who reply, do a channel refresh
                t.GotWhoList = true;
                if (nickList.CurrentWindow == t)
                    nickList.RefreshList(t);

            }
        }
        
        private void OnWhoReply(IRCConnection connection, string channel, string nick, string host, string flags, string message)
        {
            IceTabPage t = FormMain.Instance.GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                //user flags in ircData[8] H-here G-gone(away) *-irc operator x-hiddenhost d-deaf
                
                bool away = false;
                //bool ircop = false;
                
                if (flags.Contains("G")) away = true;
                //if (flags.Contains("*")) ircop = true;

                OnIALUserDataAway(connection, nick, host, channel, away, "");
                
                if (t.GotWhoList)
                    OnServerMessage(connection, message);

                if (loadedPlugins.Count > 0)
                {
                    PluginArgs args = new PluginArgs(t, channel, nick, host, message);
                    args.Connection = connection;

                    foreach (IPluginIceChat ipc in loadedPlugins)
                    {
                        if (ipc.Enabled == true)
                            ipc.WhoReply(args);
                    }
                }
            }
        }

        private void OnChannelUserList(IRCConnection connection, string channel, string[] nicks, string message)
        {
            IceTabPage t = FormMain.Instance.GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                if (t.IsFullyJoined)
                {
                    //just show the message to the console
                    OnServerMessage(connection, message);
                    return;
                }
                
                if (!t.GotNamesList)
                {
                    foreach (string nickName in nicks)
                    {
                        if (nickName.Length > 0)
                        {
                            OnChannelJoin(connection, channel, nickName, "", "", false);
                            OnIALUserData(connection, nickName, "", channel);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Show a reply to a CTCP Message we have sent out
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="nick"></param>
        /// <param name="ctcp"></param>
        /// <param name="message"></param>
        private void OnCtcpReply(IRCConnection connection, string nick, string ctcp, string message)
        {
            //we got a ctcp reply
            string msg = GetMessageFormat("Ctcp Reply");
            msg = msg.Replace("$nick", nick);
            msg = msg.Replace("$ctcp", ctcp);
            msg = msg.Replace("$reply", message);

            if (connection.UserInfoWindow != null)
            {
                if (connection.UserInfoWindow.Text.ToLower() == "user information: " + nick.ToLower())
                {
                    ((FormUserInfo)connection.UserInfoWindow).CtcpReply(message);
                    return;
                }
            }

            if (loadedPlugins.Count > 0)
            {
                PluginArgs args = new PluginArgs(mainChannelBar.GetTabPage("Console").TextWindow, "", nick, "", msg);
                args.Extra = ctcp;
                args.Connection = connection;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        args = ipc.CtcpReply(args); ;
                }
                CurrentWindowMessage(connection, args.Message, 7, false);
            }
            else
                CurrentWindowMessage(connection, msg, 7, false);

            
        }

        /// <summary>
        /// Received a CTCP Message
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="nick">The Nick who sent the CTCP Message</param>
        /// <param name="ctcp">The CTCP Message</param>
        private void OnCtcpMessage(IRCConnection connection, string nick, string ctcp, string message)
        {
            //we need to send a ctcp reply
            string msg = GetMessageFormat("Ctcp Request");
            msg = msg.Replace("$nick", nick);
            msg = msg.Replace("$ctcp", ctcp);

            PluginArgs args = new PluginArgs(mainChannelBar.GetTabPage("Console").TextWindow, "", nick, "", msg);
            args.Extra = ctcp;
            args.Connection = connection;

            foreach (IPluginIceChat ipc in loadedPlugins)
            {
                if (ipc.Enabled == true)
                    args = ipc.CtcpMessage(args);
            }
            //check if CTCP's are enabled
            if ((connection.ServerSetting.DisableCTCP == true) && (nick != connection.ServerSetting.CurrentNickName))
                return;
                        
            CurrentWindowMessage(connection, args.Message, 7, false);
            
            switch (ctcp)
            {
                case "VERSION":
                    SendData(connection, "NOTICE " + nick + " :" + ((char)1).ToString() + "VERSION " + ProgramID + " " + VersionID + " : " + GetOperatingSystemName() + ((char)1).ToString());
                    break;
                case "ICECHAT":
                    SendData(connection, "NOTICE " + nick + " :" + ((char)1).ToString() + "VERSION " + ProgramID + " " + VersionID + " : " + GetOperatingSystemName() + ((char)1).ToString());
                    //see if we have any plugins
                    if (loadedPlugins.Count > 0)
                    {
                        string plugins = "";
                        foreach (IPluginIceChat ipc in loadedPlugins)
                        {
                            if (ipc.Enabled == true)
                                plugins += ipc.Name + " " + ipc.Version + " : ";
                        }
                        if (plugins.EndsWith(" : "))
                            plugins = plugins.Substring(0, plugins.Length - 3);

                        SendData(connection, "NOTICE " + nick + " :" + ((char)1).ToString() + "VERSION " + plugins + ((char)1).ToString());
                    }
                    break;
                case "PING":
                    SendData(connection, "NOTICE " + nick + " :" + ((char)1).ToString() + "PING " + message + ((char)1).ToString());
                    break;
                case "TIME":
                    SendData(connection, "NOTICE " + nick + " :" + ((char)1).ToString() + "TIME " + System.DateTime.Now.ToString() + ((char)1).ToString());
                    break;
                case "USERINFO":
                    SendData(connection, "NOTICE " + nick + " :" + ((char)1).ToString() + "USERINFO IceChat IRC Client : Download at http://www.icechat.net" + ((char)1).ToString());
                    break;
                case "CLIENTINFO":
                    SendData(connection, "NOTICE " + nick + " :" + ((char)1).ToString() + "CLIENTINFO This client supports: UserInfo, Finger, Version, Source, Ping, Time and ClientInfo" + ((char)1).ToString());
                    break;
                case "SOURCE":
                    SendData(connection, "NOTICE " + nick + " :" + ((char)1).ToString() + "SOURCE " + FormMain.ProgramID + " " + FormMain.VersionID + " http://www.icechat.net" + ((char)1).ToString());
                    break;
                case "FINGER":
                    SendData(connection, "NOTICE " + nick + " :" + ((char)1).ToString() + "FINGER Stop fingering me" + ((char)1).ToString());
                    break;

            }
        }

        /// <summary>
        /// Received a User Notice
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="nick">The Nick who sent the Notice</param>
        /// <param name="message">The Notice message</param>
        private void OnUserNotice(IRCConnection connection, string nick, string message)
        {
            if (message.ToUpper().StartsWith("DCC CHAT"))
            {
                if (FormMain.Instance.IceChatOptions.DCCChatIgnore)
                    return;
            }
            
            string msg = GetMessageFormat("User Notice");
            if (connection.ServerSetting.RealServerName.Length > 0)
                msg = msg.Replace("$server", connection.ServerSetting.RealServerName);
            else
                msg = msg.Replace("$server", connection.ServerSetting.ServerName);
            msg = msg.Replace("$nick", nick);
            msg = msg.Replace("$message", message);

            PluginArgs args = new PluginArgs(CurrentWindow.TextWindow, "", nick, "", msg);
            args.Extra = message;
            args.Connection = connection;

            foreach (IPluginIceChat ipc in loadedPlugins)
            {
                if (ipc.Enabled == true)
                    args = ipc.UserNotice(args);
            }

            if (iceChatOptions.UserNoticeEventLocation == 0)
            {
                mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);
                mainChannelBar.GetTabPage("Console").LastMessageType = ServerMessageType.Other;
            }
            else if (iceChatOptions.UserNoticeEventLocation == 1)
            {
                CurrentWindowMessage(connection, args.Message, 1, false);
                CurrentWindow.LastMessageType = ServerMessageType.Other;
            }
            
            if (iceChatOptions.UserNoticeEventLocation < 2)
                if (!connection.ServerSetting.DisableSounds)
                    PlaySoundFile("notice");

        }
        /// <summary>
        /// Received the full host for a userreply
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="fullhost">The full user host Nick+=Ident@Host</param>
        private void OnUserHostReply(IRCConnection connection, string fullhost)
        {
            string host = fullhost.Substring(fullhost.IndexOf('+') + 1);
            string nick = "";
            if (fullhost.IndexOf('*') > -1)
                nick = fullhost.Substring(0, fullhost.IndexOf('*'));
            else
                nick = fullhost.Substring(0, fullhost.IndexOf('='));

            //update the internal addresslist and check for user in all channels
            InternalAddressList ial = new InternalAddressList(nick, host, "");

            if (!connection.ServerSetting.IAL.ContainsKey(nick))
                connection.ServerSetting.IAL.Add(nick, ial);
            else
                ((InternalAddressList)connection.ServerSetting.IAL[nick]).Host = host;

        }

        /// <summary>
        /// Received a Server Notice 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        private void OnServerNotice(IRCConnection connection, string message)
        {
            string msg = GetMessageFormat("Server Notice");
            if (connection.ServerSetting.RealServerName.Length > 0)
                msg = msg.Replace("$server", connection.ServerSetting.RealServerName);
            else
                msg = msg.Replace("$server", connection.ServerSetting.ServerName);
            msg = msg.Replace("$message", message);

            PluginArgs args = new PluginArgs(mainChannelBar.GetTabPage("Console").TextWindow, "", "", connection.ServerSetting.RealServerName, msg);
            args.Extra = message;
            args.Connection = connection;

            foreach (IPluginIceChat ipc in loadedPlugins)
            {
                if (ipc.Enabled == true)
                    args = ipc.ServerNotice(args);
            }

            if (iceChatOptions.ServerNoticeEventLocation == 0)
            {
                //send data to console
                mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);
                mainChannelBar.GetTabPage("Console").LastMessageType = ServerMessageType.ServerNotice;
            }
            else if (iceChatOptions.ServerNoticeEventLocation == 1)
            {
                //current window
                CurrentWindowMessage(connection, args.Message, 1, false);
                CurrentWindow.LastMessageType = ServerMessageType.ServerNotice;

            }
            
            if (iceChatOptions.ServerNoticeEventLocation < 2)
                if (!connection.ServerSetting.DisableSounds)
                    PlaySoundFile("conmsg");
        


        }

        /// <summary>
        /// Send out a message to be parsed
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="data">The message to be parsed</param>
        private void OutGoingCommand(IRCConnection connection, string data)
        {
            ParseOutGoingCommand(connection, data);
        }

        /// <summary>
        /// Recieved a Standard Server Message
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="message">The Server message</param>
        private void OnServerMessage(IRCConnection connection, string message)
        {
            //goes to the console            
            string msg = GetMessageFormat("Server Message");
            if (connection.ServerSetting.RealServerName.Length > 0)
                msg = msg.Replace("$server", connection.ServerSetting.RealServerName);
            else
                msg = msg.Replace("$server", connection.ServerSetting.ServerName);
            msg = msg.Replace("$message", message);

            PluginArgs args = new PluginArgs(mainChannelBar.GetTabPage("Console").TextWindow, "", "", connection.ServerSetting.RealServerName, msg);
            args.Extra = message;
            args.Connection = connection;

            foreach (IPluginIceChat ipc in loadedPlugins)
            {
                if (ipc.Enabled == true)
                    args = ipc.ServerMessage(args);
            }

            if (iceChatOptions.ServerMessageEventLocation == 0)
            {
                //send data to console
                mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);
                mainChannelBar.GetTabPage("Console").LastMessageType = ServerMessageType.ServerMessage;
            }
            else if (iceChatOptions.ServerMessageEventLocation == 1)
            {
                //current window
                CurrentWindowMessage(connection, args.Message, 1, false);
                CurrentWindow.LastMessageType = ServerMessageType.ServerMessage;
            }

            if (iceChatOptions.ServerMessageEventLocation < 2)
                if (!connection.ServerSetting.DisableSounds)
                    PlaySoundFile("conmsg");
        }

        /// <summary>
        /// Received Server Message of the Day
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="message">Message</param>
        private void OnServerMOTD(IRCConnection connection, string message)
        {
            string msg = GetMessageFormat("Server MOTD");
            if (connection.ServerSetting.RealServerName.Length > 0)
                msg = msg.Replace("$server", connection.ServerSetting.RealServerName);
            else
                msg = msg.Replace("$server", connection.ServerSetting.ServerName);
            msg = msg.Replace("$message", message);

            mainChannelBar.GetTabPage("Console").AddText(connection, msg, 1, false);
            mainChannelBar.GetTabPage("Console").LastMessageType = ServerMessageType.ServerMessage;
            
            if (!connection.ServerSetting.DisableSounds)
                PlaySoundFile("conmsg");

        }

        /// <summary>
        /// Clear the Channel List Window if it is Already Open
        /// </summary>
        /// <param name="connection"></param>
        private void OnChannelListStart(IRCConnection connection)
        {
            IceTabPage t = GetWindow(connection, "Channels", IceTabPage.WindowType.ChannelList);
            if (t != null)
            {
                t.ClearChannelList();
                t.ChannelListComplete = false;
            }
        }

        private void OnChannelListEnd(IRCConnection connection)
        {
            IceTabPage t = GetWindow(connection, "Channels", IceTabPage.WindowType.ChannelList);
            if (t != null)
                t.ChannelListComplete = true;
        }


        /// <summary>
        /// Received a Channel for the Server Channel List
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="channel">Channel Name</param>
        /// <param name="users">Total Users in Channel</param>
        /// <param name="topic">Channel Topic</param>
        private void OnChannelList(IRCConnection connection, string channel, string users, string topic)
        {
            //will make a seperate window for this eventually
            if (!mainChannelBar.WindowExists(connection, "Channels", IceTabPage.WindowType.ChannelList))
                AddWindow(connection, "Channels", IceTabPage.WindowType.ChannelList);

            IceTabPage t = GetWindow(connection, "Channels", IceTabPage.WindowType.ChannelList);
            if (t != null)
                t.AddChannelList(channel, Convert.ToInt32(users), StripColorCodes(topic));
        }

        /// <summary>
        /// Received a Server/Connection Error
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="message">Error Message</param>
        private void OnServerError(IRCConnection connection, string message, bool current)
        {
            string[] msgs = message.Split('\n');
            foreach (string msg in msgs)
            {
                if (msg.Length > 0)
                {
                    //goes to the console                        
                    string error = GetMessageFormat("Server Error");
                    if (connection.ServerSetting.RealServerName.Length > 0)
                        error = msg.Replace("$server", connection.ServerSetting.RealServerName);
                    else
                        error = msg.Replace("$server", connection.ServerSetting.ServerName);
                    error = error.Replace("$message", msg);

                    if (loadedPlugins.Count > 0)
                    {
                        PluginArgs args = new PluginArgs(mainChannelBar.GetTabPage("Console").TextWindow, "", "", connection.ServerSetting.RealServerName, error);
                        args.Extra = message;
                        args.Connection = mainChannelBar.GetTabPage("Console").Connection;

                        foreach (IPluginIceChat ipc in loadedPlugins)
                        {
                            if (ipc.Enabled == true)
                                ipc.ServerError(args);
                        }
                    }
                    
                    if (current == true)
                    {
                        System.Diagnostics.Debug.WriteLine("true:" + error);
                        // if (CurrentWindowType != IceTabPage.WindowType.Console)
                       //     CurrentWindowMessage(connection, error, 4, false);
                    }

                    

                    if (iceChatOptions.ServerErrorEventLocation == 0)
                    {
                        //send data to console
                        mainChannelBar.GetTabPage("Console").AddText(connection, error, 4, false);
                        mainChannelBar.GetTabPage("Console").LastMessageType = ServerMessageType.ServerMessage;
                    }
                    else if (iceChatOptions.ServerErrorEventLocation == 1)
                    {
                        //current window
                        CurrentWindowMessage(connection, error, 4, false);
                        CurrentWindow.LastMessageType = ServerMessageType.ServerMessage;
                    }

                    if (!connection.ServerSetting.DisableSounds)
                        PlaySoundFile("conmsg");
                }
            }

        }

        /// <summary>
        /// Received Whois Data on a Nick
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="nick">The nick whois data is from</param>
        /// <param name="data">The Whois data</param>
        private void OnWhoisData(IRCConnection connection, string nick, string data)
        {
            string msg = GetMessageFormat("User Whois");
            msg = msg.Replace("$nick", nick);
            msg = msg.Replace("$data", data);

            if (loadedPlugins.Count > 0)
            {
                PluginArgs args = new PluginArgs(mainChannelBar.GetTabPage("Console").TextWindow, "", nick, "", msg);
                args.Connection = connection;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        ipc.WhoisUser(args);
                }
            }

            if (iceChatOptions.WhoisEventLocation == 2) //hide the event
                return;

            //check if there is a query window open
            IceTabPage t = GetWindow(connection, nick, IceTabPage.WindowType.Query);
            if (t != null)
            {
                if (iceChatOptions.WhoisEventLocation == 0)
                {
                    t.TextWindow.AppendText(msg, 1);
                    t.LastMessageType = ServerMessageType.Other;
                }
                else
                {
                    mainChannelBar.GetTabPage("Console").AddText(connection, msg, 1, false);
                }
            }
            else
            {                
                if (iceChatOptions.WhoisEventLocation == 0)
                    //send whois data to the current window
                    CurrentWindowMessage(connection, msg, 1, false);
                else
                    mainChannelBar.GetTabPage("Console").AddText(connection, msg, 1, false);

            }
        }

        /// <summary>
        /// Received a Query/Private Message action
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="nick">Nick who sent the action</param>
        /// <param name="message">Query Action Message</param>
        private void OnQueryAction(IRCConnection connection, string nick, string host, string message)
        {
            if (!mainChannelBar.WindowExists(connection, nick, IceTabPage.WindowType.Query) && iceChatOptions.DisableQueries)
                return;

            if (!mainChannelBar.WindowExists(connection, nick, IceTabPage.WindowType.Query))
                AddWindow(connection, nick, IceTabPage.WindowType.Query);

            IceTabPage t = GetWindow(connection, nick, IceTabPage.WindowType.Query);
            if (t != null)
            {
                string msg = GetMessageFormat("Private Action");
                msg = msg.Replace("$nick", nick).Replace("$host", host);
                msg = msg.Replace("$message", message);

                if (loadedPlugins.Count > 0)
                {
                    PluginArgs args = new PluginArgs(t.TextWindow, "", nick, host, msg);
                    args.Extra = message;
                    args.Connection = connection;

                    foreach (IPluginIceChat ipc in loadedPlugins)
                    {
                        if (ipc.Enabled == true)
                            args = ipc.QueryAction(args);
                    }
                    t.TextWindow.AppendText(args.Message, 1);
                }
                else
                    t.TextWindow.AppendText(msg, 1);


                if (iceChatOptions.FlashTaskBar == true)
                {
                    //check if we are minimized or program is not in foreground
                    if (this.WindowState == FormWindowState.Minimized || this.IsForeGround == false)
                    {
                        if (t.EventOverLoad == false)
                            FlashTaskBar();
                    }
                }

                
                //make the tabcaption proper case
                if (t.TabCaption != nick)
                    t.TabCaption = nick;

                t.LastMessageType = ServerMessageType.Action;

                if (connection.ServerSetting.Away && iceChatOptions.SendAwayPrivateMessage)
                {
                    //send an away message
                    TimeSpan ts = DateTime.Now.Subtract(connection.ServerSetting.AwayStart);

                    string s = ts.Seconds.ToString() + " secs";
                    if (ts.Minutes > 0)
                        s = ts.Minutes.ToString() + " mins " + s;
                    if (ts.Hours > 0)
                        s = ts.Hours.ToString() + " hrs " + s;
                    if (ts.Days > 0)
                        s = ts.Days.ToString() + " days " + s;


                    string msgAway = iceChatOptions.PrivateAwayMessage;
                    msgAway = msgAway.Replace("$awaytime", s);

                    ParseOutGoingCommand(connection, "//msg " + nick + " " + msgAway);
                }

            }
        }
        /// <summary>
        /// Received a Query/Private Message 
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="nick">Nick who sent the message</param>
        /// <param name="message">Query Message</param>
        private void OnQueryMessage(IRCConnection connection, string nick, string host, string message)
        {
            if (!mainChannelBar.WindowExists(connection, nick, IceTabPage.WindowType.Query) && iceChatOptions.DisableQueries)
                return;

            if (!mainChannelBar.WindowExists(connection, nick, IceTabPage.WindowType.Query))
                AddWindow(connection, nick, IceTabPage.WindowType.Query);

            IceTabPage t = GetWindow(connection, nick, IceTabPage.WindowType.Query);
            if (t != null)
            {
                string msg = GetMessageFormat("Private Message");
                msg = msg.Replace("$nick", nick).Replace("$host", host);
                msg = msg.Replace("$message", message);

                PluginArgs args = new PluginArgs(t.TextWindow, "", nick, host, msg);
                args.Extra = message;
                args.Connection = connection;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        args = ipc.QueryMessage(args);
                }

                if (args.Message.Contains(connection.ServerSetting.CurrentNickName))
                {
                    //check if sounds are disabled for this window
                    if (!t.DisableSounds)
                    {
                        if ((CurrentWindow != t || iceChatOptions.SoundPlayActive) || this.notifyIcon.Visible == true)
                            PlaySoundFile("nickchan");
                    }
                }

                if (iceChatOptions.FlashTaskBar == true)
                {
                    //check if we are minimized or program is not in foreground
                    if (this.WindowState == FormWindowState.Minimized || this.IsForeGround == false)
                    {
                        if (t.EventOverLoad == false)
                            FlashTaskBar();
                    }
                }

                t.TextWindow.AppendText(msg, 1);

                //make the tabcaption proper case
                if (t.TabCaption != nick)
                    t.TabCaption = nick;

                t.LastMessageType = ServerMessageType.Message;

                if (!t.DisableSounds)
                {
                    //only play sound if out of focus , or on tray
                    if ((CurrentWindow != t || iceChatOptions.SoundPlayActive) || this.notifyIcon.Visible == true)
                        PlaySoundFile("privmsg");
                }
                
                if (connection.ServerSetting.Away && iceChatOptions.SendAwayPrivateMessage)
                {
                    //send an away message
                    TimeSpan ts = DateTime.Now.Subtract(connection.ServerSetting.AwayStart);

                    string s = ts.Seconds.ToString() + " secs";
                    if (ts.Minutes > 0)
                        s = ts.Minutes.ToString() + " mins " + s;
                    if (ts.Hours > 0)
                        s = ts.Hours.ToString() + " hrs " + s;
                    if (ts.Days > 0)
                        s = ts.Days.ToString() + " days " + s;


                    string msgAway = iceChatOptions.PrivateAwayMessage;
                    msgAway = msgAway.Replace("$awaytime", s);

                    ParseOutGoingCommand(connection, "//msg " + nick + " " + msgAway);
                }
            }
        }

        /// <summary>
        /// Received a Channel Action
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="channel">Which Channel it is from</param>
        /// <param name="nick">Nick who sent the action</param>
        /// <param name="message">Channel action</param>
        private void OnChannelAction(IRCConnection connection, string channel, string nick, string host, string message)
        {
            IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                string msg = GetMessageFormat("Channel Action");
                msg = msg.Replace("$nick", nick).Replace("$channel", channel);
                msg = msg.Replace("$color", "");
                msg = msg.Replace("$message", message);

                PluginArgs args = new PluginArgs(t.TextWindow, channel, nick, host, msg);
                args.Extra = message;
                args.Connection = connection;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        args = ipc.ChannelAction(args);
                }

                if (iceChatOptions.FlashTaskBar == true)
                {
                    //check if we are minimized or program is not in foreground
                    if (this.WindowState == FormWindowState.Minimized || this.IsForeGround == false)
                    {
                        if (t.EventOverLoad == false)
                            FlashTaskBar();
                    }
                }

                if (iceChatOptions.ChannelActionEventLocation == 0)
                {
                    //send it to the channel
                    t.TextWindow.AppendText(args.Message, 1);
                    t.LastMessageType = ServerMessageType.Action;
                }
                else if (iceChatOptions.ChannelActionEventLocation == 1)
                {
                    //send it to the console
                    mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);
                }

            }
        }

        /// <summary>
        /// Received a Channel Message
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="channel">Which Channel it is from</param>
        /// <param name="nick">Nick who sent the message</param>
        /// <param name="message">Channel Message</param>
        private void OnChannelMessage(IRCConnection connection, string channel, string nick, string host, string message)
        {
            IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                string msg = GetMessageFormat("Channel Message");
                msg = msg.Replace("$nick", nick).Replace("$channel", channel).Replace("$host", host);
                
                //assign $color to the nickname color
                //get the user mode for the nickname                
                if (msg.Contains("$color") && t.NickExists(nick))
                {
                    User u = t.GetNick(nick);

                    //get the nick color
                    if (u.nickColor == -1)
                    {
                        if (FormMain.Instance.IceChatColors.RandomizeNickColors == true)
                        {
                            int randColor = new Random().Next(0,71);
                            if (randColor == FormMain.Instance.IceChatColors.NickListBackColor)
                                randColor = new Random().Next(0,71);

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

                            if (u.nickColor == -1)
                                u.nickColor = FormMain.Instance.IceChatColors.ChannelRegularColor;
                        }

                    }
                    
                    msg = msg.Replace("$color", ((char)3).ToString() + u.nickColor.ToString("00"));
                }
                else
                {
                    msg = msg.Replace("$color", string.Empty);
                }
                
                //check if the nickname exists
                if (t.NickExists(nick))
                    msg = msg.Replace("$status", t.GetNick(nick).ToString().Replace(nick, ""));
                else
                    msg = msg.Replace("$status", "");

                msg = msg.Replace("$message", message);

                PluginArgs args = new PluginArgs(t.TextWindow, channel, nick, host, msg);
                args.Extra = message;
                args.Connection = connection;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        args = ipc.ChannelMessage(args);
                }

                args.Message = args.Message.Replace("$message", message);

                if (args.Message.Contains(connection.ServerSetting.CurrentNickName))
                {                    
                    //check if sounds are disabled for this window
                    if (!t.DisableSounds)
                    {
                        if ((CurrentWindow != t || iceChatOptions.SoundPlayActive)  || this.notifyIcon.Visible == true)
                            PlaySoundFile("nickchan");
                    }
                }

                if (iceChatOptions.FlashTaskBar == true)
                {
                    //check if we are minimized or program is not in foreground
                    if (this.WindowState == FormWindowState.Minimized || this.IsForeGround == false)
                    {
                        if (t.EventOverLoad == false)
                            FlashTaskBar();
                    }
                }

                if (iceChatOptions.ChannelMessageEventLocation == 0)
                {
                    //send it to the channel
                    t.TextWindow.AppendText(args.Message, 1);
                    t.LastMessageType = ServerMessageType.Message;

                    if (!t.DisableSounds)
                    {
                        if ((CurrentWindow != t || iceChatOptions.SoundPlayActive) || this.notifyIcon.Visible == true)
                            PlaySoundFile("chanmsg");
                    }
                }
                else if (iceChatOptions.ChannelMessageEventLocation == 1)
                {
                    //send it to the console
                    mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);
                }
            }
        }

        /// <summary>
        /// Received a Standard/generic Channel Message
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="channel">Which Channel it is from</param>
        /// <param name="message">Channel Message</param>
        private void OnGenericChannelMessage(IRCConnection connection, string channel, string message)
        {
            IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                string msg = GetMessageFormat("Channel Other");
                msg = msg.Replace("$message", message);

                t.TextWindow.AppendText(msg, 1);
                t.LastMessageType = ServerMessageType.Other;
            }
        }

        /// <summary>
        /// A User Quit the Server
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="user">Which Nick quit the Server</param>
        /// <param name="reason">Quit Reason</param>
        private void OnServerQuit(IRCConnection connection, string nick, string host, string reason)
        {
            PluginArgs args = null;

            string msg = GetMessageFormat("Server Quit");
            msg = msg.Replace("$nick", nick);
            msg = msg.Replace("$host", host);
            msg = msg.Replace("$reason", reason);

            args = new PluginArgs(mainChannelBar.GetTabPage("Console").TextWindow, "", nick, host, msg);
            args.Extra = reason;
            args.Connection = connection;

            foreach (IPluginIceChat ipc in loadedPlugins)
            {
                if (ipc.Enabled == true)
                    args = ipc.ServerQuit(args);
            }

            foreach (IceTabPage t in mainChannelBar.TabPages)
            {
                if (t.Connection == connection)
                {
                    if (t.WindowStyle == IceTabPage.WindowType.Channel)
                    {
                        if (t.NickExists(nick) == true)
                        {
                            if (iceChatOptions.QuitEventLocation == 0)
                            {
                                //send it to the channel
                                t.TextWindow.AppendText(args.Message, 1);
                                t.LastMessageType = ServerMessageType.QuitServer;
                            }
                            t.RemoveNick(nick);
                        }
                    }
                    if (t.WindowStyle == IceTabPage.WindowType.Query)
                    {
                        if (t.TabCaption == nick)
                        {
                            t.TextWindow.AppendText(args.Message, 1);
                            t.LastMessageType = ServerMessageType.QuitServer;
                        }
                    }
                }
            }
            if (iceChatOptions.QuitEventLocation == 1)
            {
                //send the message to the Console
                if (args != null)
                    mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);
            }
        }

        /// <summary>
        /// A User Joined a Channel
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="channel">Which Channel was Joined</param>
        /// <param name="user">Full User Host of who Joined</param>
        /// <param name="refresh">Whether to Refresh the Nick List</param>
        private void OnChannelJoin(IRCConnection connection, string channel, string nick, string host, string account, bool refresh)
        {
            IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                if (refresh)
                {
                    string msg = GetMessageFormat("Channel Join");
                    msg = msg.Replace("$nick", nick).Replace("$channel", channel).Replace("$host", host);
                    if (account.Length > 0)
                        msg = msg.Replace("$account", " [" + account + "]");
                    else
                        msg = msg.Replace("$account", "");


                    PluginArgs args = new PluginArgs(t.TextWindow, channel, nick, host, msg);
                    args.Connection = connection;

                    foreach (IPluginIceChat ipc in loadedPlugins)
                    {
                        if (ipc.Enabled == true)
                            args = ipc.ChannelJoin(args);
                    }

                    if (iceChatOptions.JoinEventLocation == 0)
                    {
                        //send to the channel window
                        t.TextWindow.AppendText(args.Message, 1);
                        t.LastMessageType = ServerMessageType.JoinChannel;
                    }
                    else if (iceChatOptions.JoinEventLocation == 1)
                    {
                        //send to the console
                        mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);
                    }
                }

                t.AddNick(nick, refresh);
            }
        }

        /// <summary>
        /// A User Parted/Left a Channel
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="channel">Which Channel was Parted</param>
        /// <param name="user">Full User Host of who Parted</param>
        /// <param name="reason">Part Reason (if any)</param>
        private void OnChannelPart(IRCConnection connection, string channel, string nick, string host, string reason)
        {
            IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                string msg = GetMessageFormat("Channel Part");
                msg = msg.Replace("$channel", channel).Replace("$nick", nick).Replace("$host", host);
                if(reason.Length > 0)
                    msg = msg.Replace("$reason", "(" + reason + ")");
                else
                    msg = msg.Replace("$reason", "");

                PluginArgs args = new PluginArgs(t.TextWindow, channel, nick, host, msg);
                args.Extra = reason;
                args.Connection = connection;
                
                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        args = ipc.ChannelPart(args);
                }

                if (iceChatOptions.PartEventLocation == 0)
                {
                    //send it to the channel
                    t.TextWindow.AppendText(args.Message, 1);
                    t.LastMessageType = ServerMessageType.PartChannel;
                }
                else if (iceChatOptions.PartEventLocation == 1)
                {
                    //send to the console
                    mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);
                }

                t.RemoveNick(nick);
            }
        }

        /// <summary>
        /// A User was kicked from a Channel
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="channel">Which Channel the User was Kicked from</param>
        /// <param name="nick">Nickname of who was Kicked</param>
        /// <param name="reason">Kick Reason</param>
        /// <param name="kickUser">Full User Host of Who kicked the User</param>
        private void OnChannelKick(IRCConnection connection, string channel, string nick, string reason, string kickUser)
        {
            IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                string kickNick = NickFromFullHost(kickUser);
                string kickHost = HostFromFullHost(kickUser);

                string msg = GetMessageFormat("Channel Kick");
                msg = msg.Replace("$nick", kickNick);
                msg = msg.Replace("$host", kickHost);
                msg = msg.Replace("$kickee", nick);
                msg = msg.Replace("$channel", channel);
                msg = msg.Replace("$reason", reason);

                PluginArgs args = new PluginArgs(iceChatOptions.KickEventLocation == 0 ? t.TextWindow : mainChannelBar.GetTabPage("Console").TextWindow, channel, nick, "", msg);
                args.Extra = reason;
                args.Connection = connection;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        args = ipc.ChannelKick(args);
                }

                if (iceChatOptions.KickEventLocation == 0)
                {
                    //send it to the channel
                    t.TextWindow.AppendText(args.Message, 1);
                    t.LastMessageType = ServerMessageType.Other;
                }
                else if (iceChatOptions.KickEventLocation == 1)
                {
                    //send to the console
                    mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);
                }

                t.RemoveNick(nick);
            }
        }

        /// <summary>
        /// You have Joined a Channel
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="channel">Which Channel you joined</param>
        private void OnChannelJoinSelf(IRCConnection connection, string channel, string host, string account)
        {
            //check if channel window already exists
            IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t == null)
            {
                t = AddWindow(connection, channel, IceTabPage.WindowType.Channel);

                PluginArgs args = new PluginArgs(t.TextWindow, channel, connection.ServerSetting.CurrentNickName, host, "");
                args.Connection = connection;
                args.Extra = "self";

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        ipc.ChannelJoin(args);
                }

                serverTree.Invalidate();
            }
            else
            {
                if (loadedPlugins.Count > 0)
                {
                    PluginArgs args = new PluginArgs(t.TextWindow, channel, connection.ServerSetting.CurrentNickName, host, "");
                    args.Connection = connection;

                    foreach (IPluginIceChat ipc in loadedPlugins)
                    {
                        if (ipc.Enabled == true)
                            ipc.ChannelJoin(args);
                    }
                }

                serverTree.Invalidate();

                string msg = GetMessageFormat("Self Channel Join");
                msg = msg.Replace("$nick", connection.ServerSetting.CurrentNickName).Replace("$channel", t.TabCaption);

                t.TextWindow.AppendText(msg, 1);

            }
        }

        /// <summary>
        /// You have Parted/Left a Channel
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="channel">Which Channel you parted</param>
        private void OnChannelPartSelf(IRCConnection connection, string channel)
        {
            string reason = "";
            string msg = GetMessageFormat("Self Channel Part");
            msg = msg.Replace("$nick", connection.ServerSetting.CurrentNickName).Replace("$channel", channel);
            if (reason.Length > 0)
                msg = msg.Replace("$reason", "("+ reason+")");
            else
                msg = msg.Replace("$reason", "");


            IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                t.IsFullyJoined = false;
                t.GotNamesList = false;
                t.GotWhoList = false;
                t.ClearNicks();
            }

            PluginArgs args = new PluginArgs(mainChannelBar.GetTabPage("Console").TextWindow, channel, connection.ServerSetting.CurrentNickName , "", msg);
            args.Connection = connection;
            args.Extra = "self";

            foreach (IPluginIceChat ipc in loadedPlugins)
            {
                if (ipc.Enabled == true)
                    args = ipc.ChannelPart(args);
            }

            if (t != null)
            {
                if (!t.ChannelHop)
                {
                    RemoveWindow(connection, channel, IceTabPage.WindowType.Channel);
                    mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);
                }
                else
                {
                    t.TextWindow.AppendText(args.Message, 1);                    
                    t.ChannelHop = false;
                }
            }
            else
            {
                   mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);
            }
        }

        /// <summary>
        /// You where Kicked from a Channel
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="channel">Which Channel you were kicked from</param>
        /// <param name="reason">Kick Reason</param>
        /// <param name="kickUser">Full User Host of who kicked you</param>
        private void OnChannelKickSelf(IRCConnection connection, string channel, string reason, string kickUser)
        {
            try
            {
                IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
                if (iceChatOptions.ChannelOpenKick)
                {
                    if (t != null)
                    {
                        t.ClearNicks();
                        t.IsFullyJoined = false;
                        t.GotNamesList = false;
                        t.GotWhoList = false;

                        if (CurrentWindow == t)
                            nickList.Header = t.TabCaption + ":0";

                        FormMain.Instance.NickList.Invalidate();
                    }
                }
                else
                {
                    RemoveWindow(connection, channel, IceTabPage.WindowType.Channel);
                }

                string nick = NickFromFullHost(kickUser);
                string host = HostFromFullHost(kickUser);

                string msg = GetMessageFormat("Self Channel Kick");
                msg = msg.Replace("$nick", connection.ServerSetting.CurrentNickName);
                msg = msg.Replace("$kicker", nick);
                msg = msg.Replace("$host", host);
                msg = msg.Replace("$channel", channel);
                msg = msg.Replace("$reason", reason);

                PluginArgs args = new PluginArgs(mainChannelBar.GetTabPage("Console").TextWindow, channel, nick, connection.ServerSetting.CurrentNickName, msg);
                args.Extra = reason;
                args.Connection = connection;
                
                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                    {
                        args = ipc.ChannelKick(args);
                    }
                }

                if (iceChatOptions.ChannelOpenKick)
                {
                    if (t != null)
                        t.TextWindow.AppendText(args.Message, 1);
                    else
                        mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);
                }
                else
                    mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);

            }
            catch (Exception e)
            {
                WriteErrorFile(connection, "OnKickSelf", e);
            }
        }

        /// <summary>
        /// A User changed their Nick Name
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="oldnick">Original Nick Name</param>
        /// <param name="newnick">New Nick Name</param>
        private void OnChangeNick(IRCConnection connection, string oldnick, string newnick, string host)
        {
            try
            {
                string network = "";
                if (connection.ServerSetting.NetworkName.Length > 0)
                    network = " (" + connection.ServerSetting.NetworkName + ")";


                if (CurrentWindowType == IceTabPage.WindowType.Console)
                {
                    if (inputPanel.CurrentConnection == connection)
                    {
                        StatusText(inputPanel.CurrentConnection.ServerSetting.CurrentNickName + " connected to " + inputPanel.CurrentConnection.ServerSetting.RealServerName + network);

                        if (connection.ServerSetting.CurrentNickName == newnick)
                        {
                            string msg = GetMessageFormat("Self Nick Change");
                            msg = msg.Replace("$nick", oldnick).Replace("$newnick", newnick).Replace("$host", host); ;

                            mainChannelBar.GetTabPage("Console").AddText(connection, msg, 1, false);

                        }
                    }
                }
                
                string cmsg = "";
                if (connection.ServerSetting.CurrentNickName == newnick)
                    cmsg = GetMessageFormat("Self Nick Change");
                else
                    cmsg = GetMessageFormat("Channel Nick Change");

                cmsg = cmsg.Replace("$nick", oldnick).Replace("$newnick", newnick).Replace("$host", host); ;

                string consoleMessage = "";

                if (loadedPlugins.Count > 0)
                {
                    PluginArgs args = new PluginArgs(mainChannelBar.GetTabPage("Console").TextWindow, "", oldnick, newnick, cmsg);
                    args.Connection = connection;

                    foreach (IPluginIceChat ipc in loadedPlugins)
                    {
                        if (ipc.Enabled == true)
                            ipc.NickChange(args);
                    }
                }

                foreach (IceTabPage t in mainChannelBar.TabPages)
                {
                    if (t.Connection == connection)
                    {
                        if (t.WindowStyle == IceTabPage.WindowType.Channel)
                        {
                            if (t.NickExists(oldnick))
                            {
                                if (connection.ServerSetting.CurrentNickName == newnick)
                                {
                                    string msg = GetMessageFormat("Self Nick Change");
                                    msg = msg.Replace("$nick", oldnick).Replace("$newnick", newnick).Replace("$host", host); ;
                                    consoleMessage = msg;

                                    if (iceChatOptions.NickChangeEventLocation == 0)
                                        t.TextWindow.AppendText(msg, 1);

                                    //update status bar as well if current channel
                                    if ((inputPanel.CurrentConnection == connection) && (CurrentWindowType == IceTabPage.WindowType.Channel))
                                    {
                                        if (CurrentWindow == t)
                                            StatusText(t.Connection.ServerSetting.CurrentNickName + " in " + t.TabCaption + " [" + t.ChannelModes + "] {" + t.Connection.ServerSetting.RealServerName + "}" + network);
                                    }
                                }
                                else
                                {
                                    string msg = GetMessageFormat("Channel Nick Change");
                                    msg = msg.Replace("$nick", oldnick).Replace("$newnick", newnick).Replace("$host", host);
                                    consoleMessage = msg;

                                    if (iceChatOptions.NickChangeEventLocation == 0)
                                    {
                                        t.TextWindow.AppendText(msg, 1);
                                        t.LastMessageType = ServerMessageType.Other;
                                    }
                                }

                                User u = t.GetNick(oldnick);
                                string nick = newnick;
                                if (u != null)
                                {
                                    for (int i = 0; i < u.Level.Length; i++)
                                    {
                                        if (u.Level[i])
                                        {
                                            if (!nick.StartsWith(connection.ServerSetting.StatusModes[1][i].ToString()))
                                                nick = connection.ServerSetting.StatusModes[1][i] + nick;
                                            break;
                                        }
                                    }

                                    t.RemoveNick(oldnick);
                                    t.AddNick(nick, true);                                    
                                }
                                
                                if (nickList.CurrentWindow == t)
                                    nickList.RefreshList(t);
                            
                            }
                        }
                        else if (t.WindowStyle == IceTabPage.WindowType.Query)
                        {
                            if (t.TabCaption == oldnick)
                            {
                                t.TabCaption = newnick;

                                string msg = GetMessageFormat("Channel Nick Change");
                                msg = msg.Replace("$nick", oldnick);
                                msg = msg.Replace("$newnick", newnick);
                                msg = msg.Replace("$host", host);

                                if (iceChatOptions.NickChangeEventLocation == 0)
                                {
                                    t.TextWindow.AppendText(msg, 1);
                                    t.LastMessageType = ServerMessageType.Other;
                                }

                                if ((inputPanel.CurrentConnection == connection) && (CurrentWindowType == IceTabPage.WindowType.Query))
                                {
                                    if (CurrentWindow == t)
                                        StatusText(t.Connection.ServerSetting.CurrentNickName + " in private chat with " + t.TabCaption + " on {" + t.Connection.ServerSetting.RealServerName + "}" + network);
                                }

                                if (nickList.CurrentWindow == t)
                                    nickList.RefreshList(t);

                                this.serverTree.Invalidate();
                                this.mainChannelBar.Invalidate();
                            }
                            else if (connection.ServerSetting.CurrentNickName == newnick)
                            {
                                string msg = GetMessageFormat("Self Nick Change");
                                msg = msg.Replace("$nick", oldnick).Replace("$newnick", newnick).Replace("$host", host); ;
                                
                                if (iceChatOptions.NickChangeEventLocation == 0)
                                    t.TextWindow.AppendText(msg, 1);

                                if (nickList.CurrentWindow == t)
                                    nickList.RefreshList(t);

                                this.serverTree.Invalidate();
                                this.mainChannelBar.Invalidate();
                            
                            }
                        }

                    }
                }
                if (iceChatOptions.NickChangeEventLocation == 1)
                {
                    if (consoleMessage.Length > 0)
                        mainChannelBar.GetTabPage("Console").AddText(connection, consoleMessage, 1, false);
                }

            }
            catch (Exception e)
            {
                WriteErrorFile(connection, "OnChangeNick Error:" + oldnick + ":" + newnick ,e);
            }
        }

        /// <summary>
        /// Channel Topic Changed
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="channel">Which Channel the Topic changed for</param>
        /// <param name="nick">Nick who changed the Topic</param>
        /// <param name="topic">New Channel Topic</param>
        private void OnChannelTopic(IRCConnection connection, string channel, string nick, string host, string topic)
        {
            IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {                
                
                //have ability                 
                PluginArgs ar = new PluginArgs();
                ar.Message = topic;
                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        ar = ipc.ChannelTopic(ar);
                }
                t.ChannelTopic = ar.Message;

                if (nick.Length > 0)
                {
                    string msg = GetMessageFormat("Channel Topic Change");
                    msg = msg.Replace("$nick", nick);
                    msg = msg.Replace("$host", host);
                    msg = msg.Replace("$channel", channel);
                    msg = msg.Replace("$topic", topic);

                    PluginArgs args = new PluginArgs(t, channel, nick, host, msg);
                    args.Connection = connection;

                    foreach (IPluginIceChat ipc in loadedPlugins)
                    {
                        if (ipc.Enabled == true)
                            args = ipc.ChannelTopic(args);
                    }
                    
                    if (iceChatOptions.TopicEventLocation == 0)
                    {
                        //send it to the channel
                        t.TextWindow.AppendText(args.Message, 1);
                        t.LastMessageType = ServerMessageType.Other;
                    }
                    else if (iceChatOptions.TopicEventLocation == 1)
                    {
                        //send it to the console
                        mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);
                    }

                }
                else
                {
                    string msgt = GetMessageFormat("Channel Topic Text");
                    msgt = msgt.Replace("$channel", channel);
                    msgt = msgt.Replace("$topic", topic);

                    PluginArgs args = new PluginArgs(t, channel, nick, host, msgt);
                    args.Connection = connection;

                    foreach (IPluginIceChat ipc in loadedPlugins)
                    {
                        if (ipc.Enabled == true)
                            args = ipc.ChannelTopic(args);
                    }

                    t.TextWindow.AppendText(args.Message, 1);
                    t.LastMessageType = ServerMessageType.Other;

                
                }

            }
        }

        /// <summary>
        /// Your User Mode for the Server has Changed
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="nick">Your Nick Name</param>
        /// <param name="mode">New User Mode(s)</param>
        private void OnUserMode(IRCConnection connection, string nick, string mode)
        {
            string msg = GetMessageFormat("Server Mode");
            if (connection.ServerSetting.RealServerName.Length > 0)
                msg = msg.Replace("$server", connection.ServerSetting.RealServerName);
            else
                msg = msg.Replace("$server", connection.ServerSetting.ServerName);
            msg = msg.Replace("$mode", mode);
            msg = msg.Replace("$nick", nick);

            mainChannelBar.GetTabPage("Console").AddText(connection, msg, 1, false);
            if (!connection.ServerSetting.DisableSounds)
                PlaySoundFile("conmsg");

            //parse out the user modes
            //set the mode in Server Setting

        }

        /// <summary>
        /// Channel Mode Changed
        /// </summary>
        /// <param name="connection">Which Connection it came from</param>
        /// <param name="modeSetter">Who set the mode(s)</param>
        /// <param name="channel">Channel which mode change is for</param>
        /// <param name="fullmode">All the modes and parameters</param>
        private void OnChannelMode(IRCConnection connection, string modeSetter, string modeSetterHost, string channel, string fullmode)
        {
            try
            {
                string mode = "";
                string parameter = "";

                if (fullmode.IndexOf(' ') == -1)
                {
                    mode = fullmode;
                }
                else
                {
                    mode = fullmode.Substring(0, fullmode.IndexOf(' '));
                    parameter = fullmode.Substring(fullmode.IndexOf(' ') + 1);
                }

                string msg = GetMessageFormat("Channel Mode");
                msg = msg.Replace("$modeparam", parameter);
                msg = msg.Replace("$mode", mode);
                msg = msg.Replace("$nick", modeSetter);
                msg = msg.Replace("$host", modeSetterHost);
                msg = msg.Replace("$channel", channel);

                IceTabPage chan = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
                if (chan != null)
                {
                    PluginArgs args = new PluginArgs(chan, channel, modeSetter, modeSetterHost, msg);
                    args.Extra = fullmode;
                    args.Connection = connection;

                    foreach (IPluginIceChat ipc in loadedPlugins)
                    {
                        if (ipc.Enabled == true)
                            ipc.ChannelMode(args);
                    }                    
                    
                    if (modeSetter != channel)
                    {
                        if (iceChatOptions.ModeEventLocation == 0)
                        {
                            chan.TextWindow.AppendText(msg, 1);
                            chan.LastMessageType = ServerMessageType.Other;
                        }
                        else if (iceChatOptions.ModeEventLocation == 1)
                        {
                            //send it to the console
                            mainChannelBar.GetTabPage("Console").AddText(connection, msg, 1, false);
                        }
                    }
                    else
                    {
                        chan.ChannelModes = fullmode.Trim();
                    }


                    string[] parameters = parameter.Split(new char[] { ' ' });

                    bool addMode = false;
                    int modelength = mode.Length;
                    string temp;
                    
                    IEnumerator parametersEnumerator = parameters.GetEnumerator();
                    parametersEnumerator.MoveNext();
                    for (int i = 0; i < modelength; i++)
                    {
                        switch (mode[i])
                        {
                            case '-':
                                addMode = false;
                                break;
                            case '+':
                                addMode = true;
                                break;
                            case 'b':
                                //handle bans seperately
                                temp = (string)parametersEnumerator.Current;
                                parametersEnumerator.MoveNext();
                                break;
                            default:
                                //check if it's a status mode which can vary by server
                                //temp = (string)parametersEnumerator.Current;
                                bool isChecked = false;

                                for (int j = 0; j < connection.ServerSetting.StatusModes[0].Length; j++)
                                {
                                    if (mode[i] == connection.ServerSetting.StatusModes[0][j])
                                    {
                                        temp = (string)parametersEnumerator.Current;
                                        //make sure its not an address
                                        if (temp.IndexOf("@") == -1)
                                        {
                                            chan.UpdateNick(temp, connection.ServerSetting.StatusModes[1][j].ToString(), addMode);
                                            parametersEnumerator.MoveNext();
                                            isChecked = true;
                                        }
                                        break;
                                    }
                                }

                                if (!isChecked)
                                {
                                    for (int j = 0; j < connection.ServerSetting.ChannelModeAddress.Length ; j++)
                                    {
                                        if (mode[i] == connection.ServerSetting.ChannelModeAddress[j])
                                        {
                                            temp = (string)parametersEnumerator.Current;
                                            chan.UpdateNick(temp, connection.ServerSetting.ChannelModeAddress[j].ToString(), addMode);
                                            parametersEnumerator.MoveNext();
                                            isChecked = true;
                                            break;
                                        }
                                    }
                                }                                
                                
                                if (!isChecked)
                                {
                                    for (int j = 0; j < connection.ServerSetting.ChannelModeParam.Length; j++)
                                    {
                                        if (mode[i] == connection.ServerSetting.ChannelModeParam[j])
                                        {
                                            temp = (string)parametersEnumerator.Current;
                                            chan.UpdateChannelMode(mode[i], temp, addMode);
                                            parametersEnumerator.MoveNext();
                                            isChecked = true;
                                            break;                                            
                                        }
                                    }
                                }
                                
                                if (!isChecked)
                                {
                                    for (int j = 0; j < connection.ServerSetting.ChannelModeParamNotRemove.Length; j++)
                                    {
                                        if (mode[i] == connection.ServerSetting.ChannelModeParamNotRemove[j])
                                        {
                                            if (addMode)
                                            {
                                                temp = (string)parametersEnumerator.Current;
                                                chan.UpdateChannelMode(mode[i], temp, addMode);
                                                parametersEnumerator.MoveNext();
                                            }
                                            else
                                            {
                                                chan.UpdateChannelMode(mode[i], addMode);
                                            }
                                            isChecked = true;
                                            break;
                                        }
                                    }
                                }
                                
                                if (!isChecked)
                                {
                                    for (int j = 0; j < connection.ServerSetting.ChannelModeNoParam.Length; j++)
                                    {
                                        if (mode[i] == connection.ServerSetting.ChannelModeNoParam[j])
                                        {
                                            chan.UpdateChannelMode(mode[i], addMode);
                                            break;
                                        }
                                    }
                                }

                                /*
                                //check if the mode has a parameter (CHANMODES= from 005)
                                if (connection.ServerSetting.ChannelModeParam.Contains(mode[i].ToString()))
                                {
                                    //even though mode l requires a param to add it, it does not to remove it
                                    //if (!addMode && mode[i] != 'l')
                                    {
                                        //mode has parameter
                                        temp = (string)parametersEnumerator.Current;
                                        parametersEnumerator.MoveNext();
                                    }
                                    chan.UpdateChannelMode(mode[i], temp, addMode);
                                }
                                else
                                    //check if it is an actual channel mode, and not a user mode
                                    if (connection.ServerSetting.ChannelModeNoParam.Contains(mode[i].ToString()))
                                        chan.UpdateChannelMode(mode[i], addMode);
                                */
                                break;

                        }
                    }

                    if (inputPanel.CurrentConnection == connection)
                    {
                        string network = "";
                        if (connection.ServerSetting.NetworkName.Length > 0)
                            network = " (" + connection.ServerSetting.NetworkName + ")";

                        if (mainChannelBar.CurrentTab == chan)
                            StatusText(connection.ServerSetting.CurrentNickName + " in " + chan.TabCaption + " [" + chan.ChannelModes + "] {" + chan.Connection.ServerSetting.RealServerName + "}" + network);
                    }
                }
            }
            catch (Exception e)
            {
                WriteErrorFile(connection, "OnChannelMode", e);
            }
        }

        /// <summary>
        /// When a User Invites you to a Channel
        /// </summary>
        /// <param name="connection">Which connection it came from</param>
        /// <param name="channel">The channel you are being invited to</param>
        /// <param name="nick">The nick who invited you</param>
        /// <param name="host">The host of the nick who invited you</param>
        private void OnChannelInvite(IRCConnection connection, string channel, string nick, string host)
        {
            string msg = GetMessageFormat("Channel Invite");
            msg = msg.Replace("$channel", channel).Replace("$nick", nick).Replace("$host", host);
            
            mainChannelBar.GetTabPage("Console").AddText(connection, msg, 1, false);

            if (!connection.ServerSetting.DisableSounds)
                PlaySoundFile("conmsg");

            if (loadedPlugins.Count > 0)
            {
                PluginArgs args = new PluginArgs(mainChannelBar.GetTabPage("Console").TextWindow, channel, nick, host, msg);
                args.Connection = connection;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        ipc.ChannelInvite(args);
                }
            }
        }

        /// <summary>
        /// Received a Channel Notice
        /// </summary>
        /// <param name="connection">The connection the notice was received on</param>
        /// <param name="nick">The nick who sent the notice</param>
        /// <param name="host">The host of the nick who sent the notice</param>
        /// <param name="status">The status char that the notice was sent to</param>
        /// <param name="channel">The channel the notice was sent to</param>
        /// <param name="notice">The notice message</param>
        private void OnChannelNotice(IRCConnection connection, string nick, string host, char status, string channel, string message)
        {
            IceTabPage t = GetWindow(connection, channel, IceTabPage.WindowType.Channel);
            if (t != null)
            {
                string msg = GetMessageFormat("Channel Notice");
                msg = msg.Replace("$nick", nick);
                msg = msg.Replace("$host", host);
                if (status == '0')
                    msg = msg.Replace("$status", "");
                else
                    msg = msg.Replace("$status", status.ToString());

                msg = msg.Replace("$channel", channel);
                msg = msg.Replace("$message", message);

                PluginArgs args = new PluginArgs(t, channel, nick, host, msg);
                args.Connection = connection;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        args = ipc.ChannelNotice(args);
                }

                if (iceChatOptions.ChannelNoticeEventLocation == 0)
                {
                    //send it to the channel
                    t.TextWindow.AppendText(args.Message, 1);
                    t.LastMessageType = ServerMessageType.Other;
                }
                else if (iceChatOptions.ChannelNoticeEventLocation == 1)
                {
                    //send it to the console
                    mainChannelBar.GetTabPage("Console").AddText(connection, args.Message, 1, false);
                    mainChannelBar.GetTabPage("Console").LastMessageType = ServerMessageType.Other;
                }
            }
        }

        /// <summary>
        /// Shows raw Server Data in a Debug Window
        /// </summary>
        /// <param name="connection">The connection the notice was received on</param>
        /// <param name="data">The Raw Server Data</param>
        private void OnRawServerData(IRCConnection connection, string data)
        {
            //check if a Debug Window is open
            IceTabPage t = GetWindow(null, "Debug", IceTabPage.WindowType.Debug);
            if (t != null)
                t.TextWindow.AppendText(connection.ServerSetting.ID + ":" + data, 1);

            if (loadedPlugins.Count > 0)
            {
                PluginArgs args = new PluginArgs(connection);
                args.Message = data;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        ipc.ServerRaw(args);
                }
            }
        }


        private string OnRawServerIncomingDataOverRide(IRCConnection connection, string data)
        {

            if (loadedPlugins.Count > 0)
            {
                PluginArgs args = new PluginArgs(connection);
                args.Message = data;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        args = ipc.ServerRawOverride(args);
                }

                IceTabPage t = GetWindow(null, "Debug", IceTabPage.WindowType.Debug);
                if (t != null)
                {
                    if (args.Message != data)
                    {
                        t.TextWindow.AppendText("C:" + connection.ServerSetting.ID + ":" + args.Message, 1);
                    }
                }

                data = args.Message;
            }

            return data;
        }

        private void OnRawServerOutgoingData(IRCConnection connection, string data)
        {
            //check if a Debug Window is open
            IceTabPage t = GetWindow(null, "Debug", IceTabPage.WindowType.Debug);
            if (t != null)
                t.TextWindow.AppendText("-" + connection.ServerSetting.ID + ":" + data, 1);

            if (loadedPlugins.Count > 0)
            {

                PluginArgs args = new PluginArgs(connection);
                args.Message = data;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    if (ipc.Enabled == true)
                        ipc.ServerRaw(args);
                }

            }
        }

        private void OnIALUserData(IRCConnection connection, string nick, string host, string channel)
        {
            //internal addresslist userdata            
            if (!connection.IsFullyConnected) return;
            if (connection.ServerSetting.StatusModes == null) return;

            for (int i = 0; i < connection.ServerSetting.StatusModes[1].Length; i++)
                nick = nick.Replace(connection.ServerSetting.StatusModes[1][i].ToString(), string.Empty);

            InternalAddressList ial = new InternalAddressList(nick, host, channel);
            if (!connection.ServerSetting.IAL.ContainsKey(nick))
            {
                connection.ServerSetting.IAL.Add(nick, ial);
            }
            else
            {
                if (channel.Length > 0)
                    ((InternalAddressList)connection.ServerSetting.IAL[nick]).AddChannel(channel);
                if (host.Length > 0)
                    ((InternalAddressList)connection.ServerSetting.IAL[nick]).Host = host;
            }

        }

        private void OnIALUserDataAway(IRCConnection connection, string nick, string host, string channel, bool away, string awayMessage)
        {
            //internal addresslist userdata            
            if (!connection.IsFullyConnected) return;
            if (connection.ServerSetting.StatusModes == null) return;

            for (int i = 0; i < connection.ServerSetting.StatusModes[1].Length; i++)
                nick = nick.Replace(connection.ServerSetting.StatusModes[1][i].ToString(), string.Empty);

            InternalAddressList ial = new InternalAddressList(nick, host, channel);
            if (!connection.ServerSetting.IAL.ContainsKey(nick))
            {
                connection.ServerSetting.IAL.Add(nick, ial);
            }
            else
            {
                if (channel.Length > 0)
                    ((InternalAddressList)connection.ServerSetting.IAL[nick]).AddChannel(channel);
                if (host.Length > 0)
                    ((InternalAddressList)connection.ServerSetting.IAL[nick]).Host = host;
                ((InternalAddressList)connection.ServerSetting.IAL[nick]).AwayStatus = away;
                ((InternalAddressList)connection.ServerSetting.IAL[nick]).AwayMessage = awayMessage;
            }

        }

        private void OnIALUserDataAwayOnly(IRCConnection connection, string nick, bool away, string awayMessage)
        {
            if (!connection.IsFullyConnected) return;
            if (connection.ServerSetting.StatusModes == null) return;

            for (int i = 0; i < connection.ServerSetting.StatusModes[1].Length; i++)
                nick = nick.Replace(connection.ServerSetting.StatusModes[1][i].ToString(), string.Empty);

            InternalAddressList ial = new InternalAddressList(nick, away, awayMessage);
            if (!connection.ServerSetting.IAL.ContainsKey(nick))
            {
                connection.ServerSetting.IAL.Add(nick, ial);
            }
            else
            {
                ((InternalAddressList)connection.ServerSetting.IAL[nick]).AwayStatus = away;
                ((InternalAddressList)connection.ServerSetting.IAL[nick]).AwayMessage = awayMessage;
            }

            //redraw the current window
            if (CurrentWindowType == IceTabPage.WindowType.Channel)
            {
                if (CurrentWindow.NickExists(nick))
                    if (FormMain.Instance.CurrentWindow == CurrentWindow)
                        FormMain.Instance.NickList.RefreshList(CurrentWindow);
            }

        }

        private void OnIALUserChange(IRCConnection connection, string oldnick, string newnick)
        {
            //change a nickname in the IAL list
            if (connection.ServerSetting.IAL.ContainsKey(oldnick))
            {                
                InternalAddressList ial = (InternalAddressList)connection.ServerSetting.IAL[oldnick];
                connection.ServerSetting.IAL.Remove(oldnick);
                
                if (connection.ServerSetting.IAL.ContainsKey(newnick))
                {
                    connection.ServerSetting.IAL.Remove(newnick);
                }
                ial.Nick = newnick;
                connection.ServerSetting.IAL.Add(newnick, ial);
            }
        }

        private void OnIALUserQuit(IRCConnection connection, string nick)
        {
            //user has quit, remove from IAL
            if (connection.ServerSetting.IAL.ContainsKey(nick))
                connection.ServerSetting.IAL.Remove(nick);

        }
        private void OnIALUserPart(IRCConnection connection, string nick, string channel)
        {
            //user left a channel, remove from channel list
            InternalAddressList ial = (InternalAddressList)connection.ServerSetting.IAL[nick];
            if (ial != null)
            {
                ial.RemoveChannel(channel);
                //if channels count is 0, remove the nick from the ial
                if (ial.Channels.Count == 0)
                    connection.ServerSetting.IAL.Remove(nick);
            }
        }

        private void OnDCCChat(IRCConnection connection, string nick, string host, string port, string ip)
        {
            //check if we have disabled DCC Chats, do we auto-accept or ask to allow
            if (iceChatOptions.DCCChatIgnore)
                return;

            if (!iceChatOptions.DCCChatAutoAccept)
            {
                //check if on System Tray
                if (!this.Visible)
                    return;

                //ask for the dcc chat
                DialogResult askDCC = MessageBox.Show(nick + "@" + host + " wants a DCC Chat, will you accept?", "DCC Chat Request", MessageBoxButtons.YesNo);
                if (askDCC == DialogResult.No)
                    return;

            }

            if (iceChatOptions.DCCChatAutoAcceptBuddyOnly && iceChatOptions.DCCChatAutoAccept)
            {
                //check if nick is in buddylist
                bool found = false;
                foreach (BuddyListItem b in connection.ServerSetting.BuddyList)
                {
                    if (b.Nick.CompareTo(nick) == 0)
                        found = true;
                }
                if (!found)
                    return;
            }


            if (!mainChannelBar.WindowExists(connection, nick, IceTabPage.WindowType.DCCChat))
                AddWindow(connection, nick, IceTabPage.WindowType.DCCChat);

            IceTabPage t = GetWindow(connection, nick, IceTabPage.WindowType.DCCChat);
            if (t != null)
            {
                string msg = GetMessageFormat("DCC Chat Request");
                msg = msg.Replace("$nick", nick).Replace("$host", host);
                msg = msg.Replace("$port", port).Replace("$ip", ip);
                
                PluginArgs args = new PluginArgs(t.TextWindow, "", nick, host, msg);
                args.Connection = connection;

                foreach (IPluginIceChat ipc in loadedPlugins)
                {
                    //new dcc chat started
                    if (ipc.Enabled == true)
                        args = ipc.DCCChatOpen(args);
                }
                
                t.TextWindow.AppendText(args.Message, 1);
                t.StartDCCChat(nick, ip, port);
                t.LastMessageType = ServerMessageType.Other;
            }

        }

        private void OnDCCFile(IRCConnection connection, string nick, string host, string port, string ip, string file, uint fileSize, uint filePos, bool resume)
        {
            if (iceChatOptions.DCCFileIgnore)
                return;
            
            if (this.InvokeRequired)
            {
                //System.Diagnostics.Debug.WriteLine("Show DCC Accept");
                ShowDCCFileAcceptDelegate s = new ShowDCCFileAcceptDelegate(OnDCCFile);
                this.Invoke(s, new object[] { connection, nick, host, port, ip, file, fileSize, filePos, resume });
            }
            else
            {
                //check if we have disabled DCC Files, do we auto-accept or ask to allow


                if (!iceChatOptions.DCCFileAutoAccept && !resume)
                {
                    //check if on System Tray
                    if (!this.Visible)
                        return;


                    //ask for the dcc file receive
                    FormDCCFileAccept dccAccept = new FormDCCFileAccept(connection, nick, host, port, ip, file, fileSize, resume, filePos);
                    dccAccept.DCCFileAcceptResult += new FormDCCFileAccept.DCCFileAcceptDelegate(OnDCCFileAcceptResult);
                    dccAccept.StartPosition = FormStartPosition.CenterParent;
                    dccAccept.Show(FormMain.Instance);

                }
                else if (iceChatOptions.DCCFileAutoAccept || resume == true)
                {
                    if (!mainChannelBar.WindowExists(null, "DCC Files", IceTabPage.WindowType.DCCFile))
                        AddWindow(null, "DCC Files", IceTabPage.WindowType.DCCFile);

                    IceTabPage t = GetWindow(null, "DCC Files", IceTabPage.WindowType.DCCFile);
                    if (t != null)
                    {
                        if (!resume)
                        {
                            ((IceTabPageDCCFile)t).StartDCCFile(connection, nick, host, ip, port, file, fileSize);

                            PluginArgs args = new PluginArgs(connection);
                            args.Nick = nick;
                            args.Host = host;
                            args.fileName = file;
                            args.filePos = 0;
                            args.fileSize = fileSize;
                            args.dccPort = port;
                            args.dccIP = ip;

                            foreach (IPluginIceChat ipc in loadedPlugins)
                            {
                                //new dcc file started
                                if (ipc.Enabled == true)
                                    ipc.DCCFileStart(args);
                            }

                        }
                        else
                        {
                            ((IceTabPageDCCFile)t).ResumeDCCFile(connection, port, filePos);
                            
                            PluginArgs args = new PluginArgs(connection);
                            args.Nick = nick;
                            args.Host = host;
                            args.fileName = file;
                            args.filePos = filePos;
                            args.fileSize = fileSize;
                            args.dccPort = port;
                            args.dccIP = ip;

                            foreach (IPluginIceChat ipc in loadedPlugins)
                            {
                                //dcc file resume started
                                if (ipc.Enabled == true)
                                    ipc.DCCFileResume(args);
                            }
                        }
                    }
                }
                else if (resume)
                {
                    System.Diagnostics.Debug.WriteLine("resume accept");


                }

            }
        }

        private void OnDCCPassive(IRCConnection connection, string nick, string host, string ip, string file, uint fileSize, uint filePos, bool resume, string id)
        {
            //passive dcc, open a listening socket and send out back to socket
            if (iceChatOptions.DCCFileIgnore)
                return;
            
            if (this.InvokeRequired)
            {
                ShowDCCPassiveAcceptDelegate s = new ShowDCCPassiveAcceptDelegate(OnDCCPassive);
                this.Invoke(s, new object[] { connection, nick, host, ip, file, fileSize, filePos, resume, id });
            }
            else
            {
                //check if on System Tray
                if (!this.Visible)
                    return;

                //ask for the dcc file receive
                FormDCCFileAccept dccAccept = new FormDCCFileAccept(connection, nick, host, "", ip, file, fileSize, filePos, resume, id);
                dccAccept.DCCFileAcceptResult += new FormDCCFileAccept.DCCFileAcceptDelegate(OnDCCPassiveAcceptResult);
                dccAccept.StartPosition = FormStartPosition.CenterParent;
                dccAccept.Show(FormMain.Instance);
            
            }
        }
        
        private void OnDCCPassiveAcceptResult(DialogResult result, IRCConnection connection, string nick, string host, string port, string ip, string file, uint fileSize, uint filePos, bool resume, string id)
        {
            if (result == DialogResult.Ignore)
            {
                //ignore the nick
                ParseOutGoingCommand(connection, "/ignore " + nick);
                return;
            }
            if (result == DialogResult.No)
            {
                //dcc was rejected
                return;
            }

            if (!mainChannelBar.WindowExists(null, "DCC Files", IceTabPage.WindowType.DCCFile))
                AddWindow(null, "DCC Files", IceTabPage.WindowType.DCCFile);

            IceTabPage t = GetWindow(null, "DCC Files", IceTabPage.WindowType.DCCFile);
            if (t != null)
            {
                if (!resume)
                {
                    ((IceTabPageDCCFile)t).StartDCCPassive(connection, nick, host, ip, file, fileSize, id);

                    PluginArgs args = new PluginArgs(connection);
                    args.Nick = nick;
                    args.Host = host;
                    args.fileName = file;
                    args.filePos = 0;
                    args.fileSize = fileSize;
                    args.dccPort = port;
                    args.dccIP = ip;
                    
                    foreach (IPluginIceChat ipc in loadedPlugins)
                    {
                        //new dcc file started
                        if (ipc.Enabled == true)
                            ipc.DCCFileStart(args);
                    }

                }
                else
                {
                    ((IceTabPageDCCFile)t).ResumeDCCFile(connection, port, filePos);

                    PluginArgs args = new PluginArgs(connection);
                    args.Nick = nick;
                    args.Host = host;
                    args.fileName = file;
                    args.filePos = filePos;
                    args.fileSize = fileSize;
                    args.dccPort = port;
                    args.dccIP = ip;

                    foreach (IPluginIceChat ipc in loadedPlugins)
                    {
                        //dcc file resume started
                        if (ipc.Enabled == true)
                            ipc.DCCFileResume(args);
                    }
                }
            }


        }

        private void OnServerFullyConnected(IRCConnection connection)
        {
            if (iceChatOptions.AutoAway)
                connection.SetAutoAwayTimer(iceChatOptions.AutoAwayTime);
            else
                connection.DisableAutoAwayTimer();

            if (connection.ServerSetting.RealServerName.Length > 0)
                ShowTrayNotification("You have connected to " + connection.ServerSetting.RealServerName + " as " + connection.ServerSetting.CurrentNickName);
            else
                ShowTrayNotification("You have connected to " + connection.ServerSetting.ServerName + " as " + connection.ServerSetting.CurrentNickName);

        }


        private void OnAutoAwayTrigger(IRCConnection connection)
        {
            string msg = iceChatOptions.AutoAwayMessage;
            msg = msg.Replace("$autoawaytime", iceChatOptions.AutoAwayTime.ToString());
            ParseOutGoingCommand(connection, "/away " + msg);
        }

        private void OnDCCFileAcceptResult(DialogResult result, IRCConnection connection, string nick, string host, string port, string ip, string file, uint fileSize, uint filePos, bool resume, string id)
        {
            if (result == DialogResult.Ignore)
            {
                //ignore the nick
                ParseOutGoingCommand(connection, "/ignore " + nick);
                return;
            }
            if (result == DialogResult.No)
            {
                //dcc was rejected
                return;
            }

            if (!mainChannelBar.WindowExists(null, "DCC Files", IceTabPage.WindowType.DCCFile))
                AddWindow(null, "DCC Files", IceTabPage.WindowType.DCCFile);

            IceTabPage t = GetWindow(null, "DCC Files", IceTabPage.WindowType.DCCFile);
            if (t != null)
            {
                if (!resume)
                {
                    ((IceTabPageDCCFile)t).StartDCCFile(connection, nick, host, ip, port, file, fileSize);

                    PluginArgs args = new PluginArgs(connection);
                    args.Nick = nick;
                    args.Host = host;
                    args.fileName = file;
                    args.filePos = 0;
                    args.fileSize = fileSize;
                    args.dccPort = port;
                    args.dccIP = ip;

                    foreach (IPluginIceChat ipc in loadedPlugins)
                    {
                        //new dcc file started
                        if (ipc.Enabled == true)
                            ipc.DCCFileStart(args);
                    }

                }
                else
                {
                    ((IceTabPageDCCFile)t).ResumeDCCFile(connection, port, filePos);

                    PluginArgs args = new PluginArgs(connection);
                    args.Nick = nick;
                    args.Host = host;
                    args.fileName = file;
                    args.filePos = filePos;
                    args.fileSize = fileSize;
                    args.dccPort = port;
                    args.dccIP = ip;

                    foreach (IPluginIceChat ipc in loadedPlugins)
                    {
                        //dcc file resume started
                        if (ipc.Enabled == true)
                            ipc.DCCFileResume(args);
                    }

                }
            }
        }

        private string StripColorCodes(string line)
        {
            //strip out all the color codes, bold , underline and reverse codes
            string ParseBackColor = @"\x03([0-9]{1,2}),([0-9]{1,2})";
            string ParseForeColor = @"\x03[0-9]{1,2}";
            string ParseColorChar = @"\x03";
            string ParseBoldChar = @"\x02";
            string ParseUnderlineChar = @"\x1F";    //code 31
            string ParseReverseChar = @"\x16";      //code 22
            string ParseItalicChar = @"\x1D";      //code 29

            StringBuilder sLine = new StringBuilder();
            sLine.Append(line);

            Regex ParseIRCCodes = new Regex(ParseBackColor + "|" + ParseForeColor + "|" + ParseColorChar + "|" + ParseBoldChar + "|" + ParseUnderlineChar + "|" + ParseReverseChar + "|" + ParseItalicChar);

            Match m = ParseIRCCodes.Match(sLine.ToString());

            while (m.Success)
            {
                sLine.Remove(m.Index, m.Length);
                m = ParseIRCCodes.Match(sLine.ToString(), m.Index);
            }

            return sLine.ToString();
        }

    }
}
