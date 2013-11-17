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
using System.Drawing.Drawing2D;
using System.Drawing;
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
        private void LoadDefaultMessageSettings()
        {
            IceChatMessageFormat oldMessage = new IceChatMessageFormat();
            oldMessage.MessageSettings = new ServerMessageFormatItem[49];

            if (iceChatMessages.MessageSettings != null)
                iceChatMessages.MessageSettings.CopyTo(oldMessage.MessageSettings, 0);
            
            iceChatMessages.MessageSettings = new ServerMessageFormatItem[49];

            if (oldMessage.MessageSettings[0] == null || oldMessage.MessageSettings[0].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[0] = NewMessageFormat("Server Connect", "&#x3;0*** Attempting to connect to $server ($serverip) on port $port");
            else
                iceChatMessages.MessageSettings[0] = oldMessage.MessageSettings[0];

            if (oldMessage.MessageSettings[1] == null || oldMessage.MessageSettings[1].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[1] = NewMessageFormat("Server Disconnect", "&#x3;4*** Server disconnected on $server");
            else
                iceChatMessages.MessageSettings[1] = oldMessage.MessageSettings[1];

            if (oldMessage.MessageSettings[2] == null || oldMessage.MessageSettings[2].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[2] = NewMessageFormat("Server Reconnect", "&#x3;0*** Attempting to re-connect to $server");
            else
                iceChatMessages.MessageSettings[2] = oldMessage.MessageSettings[2];

            if (oldMessage.MessageSettings[3] == null || oldMessage.MessageSettings[3].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[3] = NewMessageFormat("Channel Invite", "&#x3;0* $nick invites you to $channel");
            else
                iceChatMessages.MessageSettings[3] = oldMessage.MessageSettings[3];

            if (oldMessage.MessageSettings[7] == null || oldMessage.MessageSettings[7].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[7] = NewMessageFormat("Channel Mode", "&#x3;4* $nick sets mode $mode $modeparam for $channel");
            else
                iceChatMessages.MessageSettings[7] = oldMessage.MessageSettings[7];

            if (oldMessage.MessageSettings[8] == null || oldMessage.MessageSettings[8].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[8] = NewMessageFormat("Server Mode", "&#x3;9* Your mode is now $mode");
            else
                iceChatMessages.MessageSettings[8] = oldMessage.MessageSettings[8];

            if (oldMessage.MessageSettings[9] == null || oldMessage.MessageSettings[9].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[9] = NewMessageFormat("Server Notice", "&#x3;9*** $server $message");
            else
                iceChatMessages.MessageSettings[9] = oldMessage.MessageSettings[9];

            if (oldMessage.MessageSettings[10] == null || oldMessage.MessageSettings[10].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[10] = NewMessageFormat("Server Message", "&#x3;0-$server- $message");
            else
                iceChatMessages.MessageSettings[10] = oldMessage.MessageSettings[10];

            if (oldMessage.MessageSettings[11] == null || oldMessage.MessageSettings[11].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[11] = NewMessageFormat("User Notice", "&#x3;0--$nick-- $message");
            else
                iceChatMessages.MessageSettings[11] = oldMessage.MessageSettings[11];

            if (oldMessage.MessageSettings[12] == null || oldMessage.MessageSettings[12].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[12] = NewMessageFormat("Channel Message", "&#x3;0<$color$status$nick&#x3;> $message");
            else
                iceChatMessages.MessageSettings[12] = oldMessage.MessageSettings[12];

            if (oldMessage.MessageSettings[13] == null || oldMessage.MessageSettings[13].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[13] = NewMessageFormat("Self Channel Message", "&#x3;8<$nick&#x3;> $message");
            else
                iceChatMessages.MessageSettings[13] = oldMessage.MessageSettings[13];

            if (oldMessage.MessageSettings[14] == null || oldMessage.MessageSettings[14].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[14] = NewMessageFormat("Channel Action", "&#x3;13* $nick $message");
            else
                iceChatMessages.MessageSettings[14] = oldMessage.MessageSettings[14];

            if (oldMessage.MessageSettings[15] == null || oldMessage.MessageSettings[15].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[15] = NewMessageFormat("Self Channel Action", "&#x3;13* $nick $message");
            else
                iceChatMessages.MessageSettings[15] = oldMessage.MessageSettings[15];

            if (oldMessage.MessageSettings[16] == null || oldMessage.MessageSettings[16].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[16] = NewMessageFormat("Channel Join", "&#x3;7* $nick ($host) has joined channel $channel");
            else
                iceChatMessages.MessageSettings[16] = oldMessage.MessageSettings[16];

            if (oldMessage.MessageSettings[17] == null || oldMessage.MessageSettings[17].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[17] = NewMessageFormat("Self Channel Join", "&#x3;4* You have joined $channel");
            else
                iceChatMessages.MessageSettings[17] = oldMessage.MessageSettings[17];

            if (oldMessage.MessageSettings[18] == null || oldMessage.MessageSettings[18].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[18] = NewMessageFormat("Channel Part", "&#x3;3* $nick ($host) has left $channel ($reason)");
            else
                iceChatMessages.MessageSettings[18] = oldMessage.MessageSettings[18];

            if (oldMessage.MessageSettings[19] == null || oldMessage.MessageSettings[19].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[19] = NewMessageFormat("Self Channel Part", "&#x3;4* You have left $channel - You will be missed &#x3;10($reason)");
            else
                iceChatMessages.MessageSettings[19] = oldMessage.MessageSettings[19];

            if (oldMessage.MessageSettings[20] == null || oldMessage.MessageSettings[20].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[20] = NewMessageFormat("Server Quit", "&#x3;9* $nick ($host) Quit ($reason)");
            else
                iceChatMessages.MessageSettings[20] = oldMessage.MessageSettings[20];

            if (oldMessage.MessageSettings[21] == null || oldMessage.MessageSettings[21].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[21] = NewMessageFormat("Channel Nick Change", "&#x3;7* $nick is now known as $newnick");
            else
                iceChatMessages.MessageSettings[21] = oldMessage.MessageSettings[21];

            if (oldMessage.MessageSettings[22] == null || oldMessage.MessageSettings[22].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[22] = NewMessageFormat("Self Nick Change", "&#x3;4* You are now known as $newnick");
            else
                iceChatMessages.MessageSettings[22] = oldMessage.MessageSettings[22];

            if (oldMessage.MessageSettings[23] == null || oldMessage.MessageSettings[23].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[23] = NewMessageFormat("Channel Kick", "&#x3;8* $kickee was kicked by $nick($host) &#x3;3 - Reason ($reason)");
            else
                iceChatMessages.MessageSettings[23] = oldMessage.MessageSettings[23];

            if (oldMessage.MessageSettings[24] == null || oldMessage.MessageSettings[24].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[24] = NewMessageFormat("Self Channel Kick", "&#x3;4* You were kicked from $channel by $kicker (&#x3;3$reason)");
            else
                iceChatMessages.MessageSettings[24] = oldMessage.MessageSettings[24];

            if (oldMessage.MessageSettings[25] == null || oldMessage.MessageSettings[25].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[25] = NewMessageFormat("Private Message", "&#x3;0<$nick> $message");
            else
                iceChatMessages.MessageSettings[25] = oldMessage.MessageSettings[25];

            if (oldMessage.MessageSettings[26] == null || oldMessage.MessageSettings[26].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[26] = NewMessageFormat("Self Private Message", "&#x3;4<$nick>&#x3;4 $message");
            else
                iceChatMessages.MessageSettings[26] = oldMessage.MessageSettings[26];

            if (oldMessage.MessageSettings[27] == null || oldMessage.MessageSettings[27].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[27] = NewMessageFormat("Private Action", "&#x3;13* $nick $message");
            else
                iceChatMessages.MessageSettings[27] = oldMessage.MessageSettings[27];

            if (oldMessage.MessageSettings[28] == null || oldMessage.MessageSettings[28].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[28] = NewMessageFormat("Self Private Action", "&#x3;13* $nick $message");
            else
                iceChatMessages.MessageSettings[28] = oldMessage.MessageSettings[28];

            if (oldMessage.MessageSettings[35] == null || oldMessage.MessageSettings[35].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[35] = NewMessageFormat("Channel Topic Change", "&#x3;3* $nick changes topic to: $topic");
            else
                iceChatMessages.MessageSettings[35] = oldMessage.MessageSettings[35];

            if (oldMessage.MessageSettings[36] == null || oldMessage.MessageSettings[36].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[36] = NewMessageFormat("Channel Topic Text", "&#x3;0Topic: $topic");
            else
                iceChatMessages.MessageSettings[36] = oldMessage.MessageSettings[36];

            if (oldMessage.MessageSettings[37] == null || oldMessage.MessageSettings[37].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[37] = NewMessageFormat("Server MOTD", "&#x3;0$message");
            else
                iceChatMessages.MessageSettings[37] = oldMessage.MessageSettings[37];

            if (oldMessage.MessageSettings[38] == null || oldMessage.MessageSettings[38].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[38] = NewMessageFormat("Channel Notice", "&#x3;4-$nick:$status$channel- $message");
            else
                iceChatMessages.MessageSettings[38] = oldMessage.MessageSettings[38];

            if (oldMessage.MessageSettings[39] == null || oldMessage.MessageSettings[39].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[39] = NewMessageFormat("Channel Other", "&#x3;0$message");
            else
                iceChatMessages.MessageSettings[39] = oldMessage.MessageSettings[39];

            if (oldMessage.MessageSettings[40] == null || oldMessage.MessageSettings[40].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[40] = NewMessageFormat("User Echo", "&#x3;7$message");
            else
                iceChatMessages.MessageSettings[40] = oldMessage.MessageSettings[40];

            if (oldMessage.MessageSettings[41] == null || oldMessage.MessageSettings[41].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[41] = NewMessageFormat("Server Error", "&#x3;4ERROR: $message");
            else
                iceChatMessages.MessageSettings[41] = oldMessage.MessageSettings[41];

            if (oldMessage.MessageSettings[42] == null || oldMessage.MessageSettings[42].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[42] = NewMessageFormat("User Whois", "&#x3;12->> $nick $data");
            else
                iceChatMessages.MessageSettings[42] = oldMessage.MessageSettings[42];

            if (oldMessage.MessageSettings[43] == null || oldMessage.MessageSettings[43].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[43] = NewMessageFormat("User Error", "&#x3;4ERROR: $message");
            else
                iceChatMessages.MessageSettings[43] = oldMessage.MessageSettings[43];

            if (oldMessage.MessageSettings[44] == null || oldMessage.MessageSettings[44].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[44] = NewMessageFormat("DCC Chat Connect", "&#x3;0* DCC Chat Connection Established with $nick");
            else
                iceChatMessages.MessageSettings[44] = oldMessage.MessageSettings[44];
            
            if (oldMessage.MessageSettings[45] == null || oldMessage.MessageSettings[45].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[45] = NewMessageFormat("DCC Chat Disconnect", "&#x3;4* DCC Chat Disconnected from $nick");
            else
                iceChatMessages.MessageSettings[45] = oldMessage.MessageSettings[45];

            if (oldMessage.MessageSettings[48] == null || oldMessage.MessageSettings[13].FormattedMessage.Length == 0)
                iceChatMessages.MessageSettings[48] = NewMessageFormat("Self Notice", "&#x3;4--> $nick - $message");
            else
                iceChatMessages.MessageSettings[48] = oldMessage.MessageSettings[48];

            //still do customize these messages
            iceChatMessages.MessageSettings[4] = NewMessageFormat("Ctcp Reply", "&#x3;12[$nick $ctcp Reply] : $reply");
            iceChatMessages.MessageSettings[5] = NewMessageFormat("Ctcp Send", "&#x3;10--> [$nick] $ctcp");
            iceChatMessages.MessageSettings[6] = NewMessageFormat("Ctcp Request", "&#x3;7[$nick] $ctcp");
            
            iceChatMessages.MessageSettings[29] = NewMessageFormat("DCC Chat Action", "&#x3;13* $nick $message");
            iceChatMessages.MessageSettings[30] = NewMessageFormat("Self DCC Chat Action", "&#x3;13* $nick $message");
            iceChatMessages.MessageSettings[31] = NewMessageFormat("DCC Chat Message", "&#x3;0<$nick> $message");
            iceChatMessages.MessageSettings[32] = NewMessageFormat("Self DCC Chat Message", "&#x3;4<$nick> $message");
            
            iceChatMessages.MessageSettings[33] = NewMessageFormat("DCC Chat Request", "&#x3;4* $nick ($host) is requesting a DCC Chat");
            iceChatMessages.MessageSettings[34] = NewMessageFormat("DCC File Send", "&#x3;4* $nick ($host) is trying to send you a file ($file) [$filesize bytes]");
            
            iceChatMessages.MessageSettings[46] = NewMessageFormat("DCC Chat Outgoing", "&#x3;0* DCC Chat Requested with $nick");
            iceChatMessages.MessageSettings[47] = NewMessageFormat("DCC Chat Timeout", "&#x3;0* DCC Chat with $nick timed out");

            SaveMessageFormat();

        }

        private void LoadDefaultFontSettings()
        {
            IceChatFontSetting oldFonts = new IceChatFontSetting();
            oldFonts.FontSettings = new FontSettingItem[9];

            if (iceChatFonts.FontSettings != null)
            {
                iceChatFonts.FontSettings.CopyTo(oldFonts.FontSettings, 0);
                iceChatFonts.FontSettings = new FontSettingItem[9];
                oldFonts.FontSettings.CopyTo(iceChatFonts.FontSettings, 0);
            }
            else
                iceChatFonts.FontSettings = new FontSettingItem[9];

            if (oldFonts.FontSettings[0] == null || iceChatFonts.FontSettings[0].FontName.Length == 0)
                iceChatFonts.FontSettings[0] = NewFontSetting("Console", "Verdana", 10);
            else
                iceChatFonts.FontSettings[0] = oldFonts.FontSettings[0];

            if (oldFonts.FontSettings[1] == null || iceChatFonts.FontSettings[1].FontName.Length == 0)
                iceChatFonts.FontSettings[1] = NewFontSetting("Channel", "Verdana", 10);
            else
                iceChatFonts.FontSettings[1] = oldFonts.FontSettings[1];

            if (oldFonts.FontSettings[2] == null || iceChatFonts.FontSettings[2].FontName.Length == 0)
                iceChatFonts.FontSettings[2] = NewFontSetting("Query", "Verdana", 10);
            else
                iceChatFonts.FontSettings[2] = oldFonts.FontSettings[2];

            if (oldFonts.FontSettings[3] == null || iceChatFonts.FontSettings[3].FontName.Length == 0)
                iceChatFonts.FontSettings[3] = NewFontSetting("Nicklist", "Verdana", 10);
            else
                iceChatFonts.FontSettings[3] = oldFonts.FontSettings[3];

            if (oldFonts.FontSettings[4] == null || iceChatFonts.FontSettings[4].FontName.Length == 0)
                iceChatFonts.FontSettings[4] = NewFontSetting("Serverlist", "Verdana", 10);
            else
                iceChatFonts.FontSettings[4] = oldFonts.FontSettings[4];

            if (oldFonts.FontSettings[5] == null || iceChatFonts.FontSettings[5].FontName.Length == 0)
                iceChatFonts.FontSettings[5] = NewFontSetting("InputBox", "Verdana", 10);
            else
                iceChatFonts.FontSettings[5] = oldFonts.FontSettings[5];

            if (oldFonts.FontSettings[6] == null || iceChatFonts.FontSettings[6].FontName.Length == 0)
                iceChatFonts.FontSettings[6] = NewFontSetting("DockTabs", "Verdana", 10);
            else
                iceChatFonts.FontSettings[6] = oldFonts.FontSettings[6];

            if (oldFonts.FontSettings[7] == null || iceChatFonts.FontSettings[7].FontName.Length == 0)
                iceChatFonts.FontSettings[7] = NewFontSetting("MenuBar", "Verdana", 10);
            else
                iceChatFonts.FontSettings[7] = oldFonts.FontSettings[7];

            if (oldFonts.FontSettings[8] == null || iceChatFonts.FontSettings[8].FontName.Length == 0)
                iceChatFonts.FontSettings[8] = NewFontSetting("ChannelBar", "Verdana", 10);
            else
                iceChatFonts.FontSettings[8] = oldFonts.FontSettings[8];

            
            oldFonts = null;

            SaveFonts();
        }

        private ServerMessageFormatItem NewMessageFormat(string messageName, string message)
        {
            ServerMessageFormatItem m = new ServerMessageFormatItem();
            m.MessageName = messageName;
            m.FormattedMessage = message;
            return m;
        }

        private FontSettingItem NewFontSetting(string windowType, string fontName, int fontSize)
        {
            FontSettingItem f = new FontSettingItem();
            f.WindowType = windowType;
            f.FontName = fontName;
            f.FontSize = fontSize;
            return f;
        }

        private void LoadOptions()
        {
            if (File.Exists(optionsFile))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(IceChatOptions));
                TextReader textReader = new StreamReader(optionsFile);
                iceChatOptions = (IceChatOptions)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
            }
            else
            {
                //create default settings
                iceChatOptions = new IceChatOptions();                
                SaveOptions();
            }
        }

        private void SaveOptions()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(IceChatOptions));
            TextWriter textWriter = new StreamWriter(optionsFile);
            serializer.Serialize(textWriter, iceChatOptions);
            textWriter.Close();
            textWriter.Dispose();
        }

        private void LoadMessageFormat()
        {
            if (File.Exists(messagesFile))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(IceChatMessageFormat));
                TextReader textReader = new StreamReader(messagesFile);
                iceChatMessages = (IceChatMessageFormat)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
                if (iceChatMessages.MessageSettings.Length != 49)
                    LoadDefaultMessageSettings();
            }
            else
            {
                iceChatMessages = new IceChatMessageFormat();
                LoadDefaultMessageSettings();
            }
        }

        private void SaveMessageFormat()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(IceChatMessageFormat));
            TextWriter textWriter = new StreamWriter(messagesFile);
            serializer.Serialize(textWriter, iceChatMessages);
            textWriter.Close();
            textWriter.Dispose();
        }

        private void LoadChannelSettings()
        {
            if (File.Exists(channelSettingsFile))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(ChannelSettings));
                TextReader textReader = new StreamReader(channelSettingsFile);
                channelSettings = (ChannelSettings)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
            }
            else
            {
                channelSettings = new ChannelSettings();
            }
        }

        internal void SaveChannelSettings()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ChannelSettings));
            TextWriter textWriter = new StreamWriter(channelSettingsFile);
            serializer.Serialize(textWriter, channelSettings);
            textWriter.Close();
            textWriter.Dispose();
        }

        private void LoadAliases()
        {
            if (File.Exists(aliasesFile))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(IceChatAliases));
                TextReader textReader = new StreamReader(aliasesFile);
                iceChatAliases = (IceChatAliases)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
            }
            else
            {
                iceChatAliases = new IceChatAliases();
                SaveAliases();
            }
        }

        private void SaveAliases()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(IceChatAliases));
            TextWriter textWriter = new StreamWriter(aliasesFile);
            serializer.Serialize(textWriter, iceChatAliases);
            textWriter.Close();
            textWriter.Dispose();
        }

        private void LoadPluginFiles()
        {
            if (File.Exists(pluginsFile))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(IceChatPluginFile));
                TextReader textReader = new StreamReader(pluginsFile);
                iceChatPlugins = (IceChatPluginFile)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
            }
            else
            {
                iceChatPlugins = new IceChatPluginFile();
                SavePluginFiles();
            }
        }

        private void SavePluginFiles()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(IceChatPluginFile));
            TextWriter textWriter = new StreamWriter(pluginsFile);
            serializer.Serialize(textWriter, iceChatPlugins);
            textWriter.Close();
            textWriter.Dispose();
        }


        private void LoadEmoticons()
        {
            if (File.Exists(emoticonsFile))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(IceChatEmoticon));
                TextReader textReader = new StreamReader(emoticonsFile);
                iceChatEmoticons = (IceChatEmoticon)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
            }
            else
            {
                iceChatEmoticons = new IceChatEmoticon();
                SaveEmoticons();
            }
        }

        private void SaveEmoticons()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(IceChatEmoticon));
            //check if emoticons Folder Exists
            if (!System.IO.File.Exists(EmoticonsFolder))
                System.IO.Directory.CreateDirectory(EmoticonsFolder);

            TextWriter textWriter = new StreamWriter(emoticonsFile);
            serializer.Serialize(textWriter, iceChatEmoticons);
            textWriter.Close();
            textWriter.Dispose();
        }

        private void LoadPopups()
        {
            if (File.Exists(popupsFile))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(IceChatPopupMenus));
                TextReader textReader = new StreamReader(popupsFile);
                iceChatPopups = (IceChatPopupMenus)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
            }
            else
                iceChatPopups = new IceChatPopupMenus();

        }

        private void SavePopups()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(IceChatPopupMenus));
            TextWriter textWriter = new StreamWriter(popupsFile);
            serializer.Serialize(textWriter, iceChatPopups);
            textWriter.Close();
            textWriter.Dispose();
        }

        private void LoadFonts()
        {
            if (File.Exists(fontsFile))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(IceChatFontSetting));
                TextReader textReader = new StreamReader(fontsFile);
                iceChatFonts = (IceChatFontSetting)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
                if (iceChatFonts.FontSettings.Length < 9)
                    LoadDefaultFontSettings();
            }
            else
            {
                iceChatFonts = new IceChatFontSetting();
                LoadDefaultFontSettings();
            }
        }

        private void SaveFonts()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(IceChatFontSetting));
            TextWriter textWriter = new StreamWriter(fontsFile);
            serializer.Serialize(textWriter, iceChatFonts);
            textWriter.Close();
            textWriter.Dispose();
        }

        private void LoadColors()
        {
            if (File.Exists(colorsFile))
            {                
                XmlSerializer deserializer = new XmlSerializer(typeof(IceChatColors));
                TextReader textReader = new StreamReader(colorsFile);
                iceChatColors = (IceChatColors)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
            }
            else
                iceChatColors = new IceChatColors();
        }

        private void SaveColors()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(IceChatColors));
            TextWriter textWriter = new StreamWriter(colorsFile);
            serializer.Serialize(textWriter, iceChatColors);
            textWriter.Close();
            textWriter.Dispose();
        }
    
        public void LoadSounds()
        {
            if (File.Exists(soundsFile))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(IceChatSounds));
                TextReader textReader = new StreamReader(soundsFile);
                iceChatSounds = (IceChatSounds)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
            }
            else
            {
                iceChatSounds = new IceChatSounds();
                iceChatSounds.AddDefaultSounds();
            }
        }

        private void SaveSounds()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(IceChatSounds));
            TextWriter textWriter = new StreamWriter(soundsFile);
            serializer.Serialize(textWriter, iceChatSounds);
            textWriter.Close();
            textWriter.Dispose();
        }
    }
}
