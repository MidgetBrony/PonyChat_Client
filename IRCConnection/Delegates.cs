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

namespace IceChat
{
    public delegate void OutGoingCommandDelegate(IRCConnection connection, string data);
    public delegate void RawServerIncomingDataDelegate(IRCConnection connection, string data);
    public delegate void RawServerOutgoingDataDelegate(IRCConnection connection, string data);

    public delegate string RawServerIncomingDataOverRideDelegate(IRCConnection connection, string data);

    public delegate void ChannelMessageDelegate(IRCConnection connection, string channel, string nick, string host, string message);
    public delegate void ChannelActionDelegate(IRCConnection connection, string channel, string nick, string host, string message);
    public delegate void QueryMessageDelegate(IRCConnection connection, string nick, string host, string message);
    public delegate void QueryActionDelegate(IRCConnection connection, string nick, string host, string message);
    public delegate void GenericChannelMessageDelegate(IRCConnection connection, string channel, string message);
    public delegate void ChannelNoticeDelegate(IRCConnection connection, string nick, string host, char status, string channel, string message);

    public delegate void JoinChannelDelegate(IRCConnection connection, string channel, string nick, string host, string account, bool refresh);
    public delegate void PartChannelDelegate(IRCConnection connection, string channel, string nick, string host, string reason);
    public delegate void QuitServerDelegate(IRCConnection connection, string nick, string host, string reason);

    public delegate void AddNickNameDelegate(IRCConnection connection, string channel, string nick);
    public delegate void RemoveNickNameDelegate(IRCConnection connection, string channel, string nick);
    public delegate void ClearNickListDelegate(IRCConnection connection, string channel);
    public delegate void UserHostReplyDelegate(IRCConnection connection, string fullhost);

    public delegate void ChannelKickDelegate(IRCConnection connection, string channel, string nick, string reason, string kickUser);
    public delegate void ChannelKickSelfDelegate(IRCConnection connection, string channel, string reason, string kickUser);
    public delegate void ChangeNickDelegate(IRCConnection connection, string oldnick, string newnick, string host);
    public delegate void UserNoticeDelegate(IRCConnection connection, string nick, string message);
    public delegate void ServerNoticeDelegate(IRCConnection connection, string message);

    public delegate void JoinChannelMyselfDelegate(IRCConnection connection, string channel, string host, string account);
    public delegate void PartChannelMyselfDelegate(IRCConnection connection, string channel);

    public delegate void ChannelTopicDelegate(IRCConnection connection, string channel, string nick, string host, string topic);

    public delegate void UserModeChangeDelegate(IRCConnection connection, string nick, string mode);
    public delegate void ChannelModeChangeDelegate(IRCConnection connection, string modeSetter, string modeSetterHost, string channel, string fullmode);

    public delegate void ServerMessageDelegate(IRCConnection connection, string message);
    public delegate void ServerMOTDDelegate(IRCConnection connection, string message);
    public delegate void ServerErrorDelegate(IRCConnection connection, string message, bool current);
    public delegate void WhoisDataDelegate(IRCConnection connection, string nick, string data);
    public delegate void CtcpMessageDelegate(IRCConnection connection, string nick, string ctcp, string message);
    public delegate void CtcpReplyDelegate(IRCConnection connection, string nick, string ctcp, string message);

    public delegate void ChannelListStartDelegate(IRCConnection connection);
    public delegate void ChannelListEndDelegate(IRCConnection connection);
    public delegate void ChannelListDelegate(IRCConnection connection, string channel, string users, string topic);
    public delegate void ChannelInviteDelegate(IRCConnection connection, string channel, string nick, string host);

    public delegate void DCCChatDelegate(IRCConnection connection, string nick, string host, string port, string ip);
    public delegate void DCCFileDelegate(IRCConnection connection, string nick, string host, string port, string ip, string file, uint fileSize, uint filePos, bool resume);
    public delegate void DCCPassiveDelegate(IRCConnection connection, string nick, string host, string ip, string file, uint fileSize, uint filePos, bool resume, string id);

    public delegate void ServerFullyConnectedDelegate(IRCConnection connection);
    public delegate void AutoAwayDelegate(IRCConnection connection);

    //for the Server Tree
    public delegate void NewServerConnectionDelegate(ServerSetting serverSetting);

    //for the Buddy List
    public delegate void BuddyListDelegate(IRCConnection connection, string[] buddies);

    //for the IAL (internal address list)
    public delegate void IALUserDataDelegate(IRCConnection connection, string nick, string host, string channel);
    public delegate void IALUserDataAwayOnlyDelegate(IRCConnection connection, string nick, bool away, string awayMessage);
    public delegate void IALUserChangeDelegate(IRCConnection connection, string oldnick, string newnick);
    public delegate void IALUserPartDelegate(IRCConnection connection, string nick, string channel);
    public delegate void IALUserQuitDelegate(IRCConnection connection, string nick);


    public delegate void AutoPerformDelegate(IRCConnection connection, string[] commands);
    public delegate void AutoJoinDelegate(IRCConnection connection, string[] channels);
    public delegate void AutoRejoinDelegate(IRCConnection connection);

    public delegate void EndofNamesDelegate(IRCConnection connection, string channel);
    public delegate void EndofWhoReplyDelegate(IRCConnection connection, string channel);
    public delegate void WhoReplyDelegate(IRCConnection connection, string channel, string nick, string host, string flags, string message);
    public delegate void ChannelUserListDelegate(IRCConnection connection, string channel, string[] nicks, string message);

    public delegate void StatusTextDelegate(IRCConnection connection, string statusText);

    public delegate bool UserInfoWindowExistsDelegate(IRCConnection connection, string nick);

    public delegate void UserInfoHostFullnameDelegate(IRCConnection connection, string nick, string host, string full);
    public delegate void UserInfoIdleLogonDelegate(IRCConnection connection, string nick, string idleTime, string logonTime);
    public delegate void UserInfoAddChannelsDelegate(IRCConnection connection, string nick, string[] channels);
    public delegate void UserInfoAwayStatusDelegate(IRCConnection connection, string nick, string awayMessage);
    public delegate void UserInfoServerDelegate(IRCConnection connection, string nick, string server);

    public delegate bool ChannelInfoWindowExistsDelegate(IRCConnection connection, string channel);
    public delegate void ChannelInfoAddBanDelegate(IRCConnection connection, string channel, string host, string bannedBy);
    public delegate void ChannelInfoAddExceptionDelegate(IRCConnection connection, string channel, string host, string bannedBy);
    public delegate void ChannelInfoTopicSetDelegate(IRCConnection connection, string channel, string nick, string time);

    public delegate void RefreshServerTreeDelegate(IRCConnection connection);
    public delegate void BuddyListClearDelegate(IRCConnection connection);
    public delegate void ServerReconnectDelegate(IRCConnection connection);
    public delegate void ServerDisconnectDelegate(IRCConnection connection);
    public delegate void ServerConnectDelegate(IRCConnection connection, string address);
    public delegate void ServerForceDisconnectDelegate(IRCConnection connection);
    public delegate void ServerPreConnectDelegate(IRCConnection connection);

    public delegate void WriteErrorFileDelegate(IRCConnection connection, string method, Exception e);

}
