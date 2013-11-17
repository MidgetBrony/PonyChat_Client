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
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;

using IceChatPlugin;
using IceChatTheme;

namespace IceChat
{
    public partial class FormColors : Form
    {
        internal delegate void SaveColorsDelegate(IceChatColors colors, IceChatMessageFormat messages);        
        internal event SaveColorsDelegate SaveColors;

        private ColorButtonArray colorPicker;

        private Hashtable messageIdentifiers;

        private IceChatMessageFormat iceChatMessages;
        private IceChatColors iceChatColors;

        private object currentColorPick;
        
        private const char newColorChar = '\xFF03';

        private class ComboItem
        {
            public string ThemeName;
            public string ThemeType;
            public string FileName;
            public override string ToString()
            {
                return this.ThemeName;
            }
        }

        public FormColors(IceChatMessageFormat MessageFormat, IceChatColors IceChatColors)
        {
            InitializeComponent();

            this.Load += new EventHandler(FormColors_Load);

            //add the events for the Tab Bar Color Picker
            this.pictureTabCurrent.Click += new EventHandler(OnColor_Click);
            this.pictureTabMessage.Click += new EventHandler(OnColor_Click);
            this.pictureTabAction.Click += new EventHandler(OnColor_Click);
            
            this.pictureTabJoin.Click += new EventHandler(OnColor_Click);
            this.pictureTabPart.Click += new EventHandler(OnColor_Click);
            this.pictureTabQuit.Click += new EventHandler(OnColor_Click);
            this.pictureTabServer.Click += new EventHandler(OnColor_Click);
            this.pictureTabServerNotice.Click += new EventHandler(OnColor_Click);
            
            this.pictureTabOther.Click += new EventHandler(OnColor_Click);
            this.pictureTabDefault.Click += new EventHandler(OnColor_Click);

            //add the events for the Nick Color Picker
            this.pictureAdmin.Click += new EventHandler(OnColor_Click);
            this.pictureOwner.Click += new EventHandler(OnColor_Click);
            this.pictureOperator.Click += new EventHandler(OnColor_Click);
            this.pictureHalfOperator.Click += new EventHandler(OnColor_Click);
            this.pictureVoice.Click += new EventHandler(OnColor_Click);
            this.pictureDefault.Click += new EventHandler(OnColor_Click);

            this.pictureConsole.Click += new EventHandler(OnColor_Click);
            this.pictureChannel.Click += new EventHandler(OnColor_Click);
            this.pictureQuery.Click += new EventHandler(OnColor_Click);
            this.pictureNickList.Click += new EventHandler(OnColor_Click);
            this.pictureServerList.Click += new EventHandler(OnColor_Click);
            this.pictureTabBarCurrent1.Click += new EventHandler(OnColor_Click);
            this.pictureTabBarCurrent2.Click += new EventHandler(OnColor_Click);
            this.pictureTabBarOther1.Click += new EventHandler(OnColor_Click);
            this.pictureTabBarOther2.Click += new EventHandler(OnColor_Click);
            this.pictureTabBackground.Click += new EventHandler(OnColor_Click);

            this.pictureTabBarHover1.Click += new EventHandler(OnColor_Click);
            this.pictureTabBarHover2.Click += new EventHandler(OnColor_Click);

            this.picturePanelHeaderBG1.Click += new EventHandler(OnColor_Click);
            this.picturePanelHeaderBG2.Click += new EventHandler(OnColor_Click);
            this.picturePanelHeaderForeColor.Click += new EventHandler(OnColor_Click);
            this.pictureUnreadTextMarkerColor.Click += new EventHandler(OnColor_Click);

            this.pictureToolBar.Click += new EventHandler(OnColor_Click);
            this.pictureMenuBar.Click += new EventHandler(OnColor_Click);
            this.pictureInputBox.Click += new EventHandler(OnColor_Click);
            this.pictureInputBoxFore.Click += new EventHandler(OnColor_Click);
            this.pictureChannelList.Click += new EventHandler(OnColor_Click);
            this.pictureChannelListFore.Click += new EventHandler(OnColor_Click);
            this.pictureStatusBar.Click += new EventHandler(OnColor_Click);
            this.pictureStatusFore.Click += new EventHandler(OnColor_Click);
            this.pictureHyperlink.Click += new EventHandler(OnColor_Click);

            this.iceChatColors = IceChatColors;

            UpdateColorSettings();

            messageIdentifiers = new Hashtable();
            AddMessageIdentifiers();

            colorPicker = new ColorButtonArray(panelColorPicker);
            colorPicker.OnClick += new ColorButtonArray.ColorSelected(colorPicker_OnClick);
            
            treeMessages.AfterSelect += new TreeViewEventHandler(treeMessages_AfterSelect);
            textRawMessage.TextChanged+=new EventHandler(textRawMessage_TextChanged);
            textRawMessage.KeyDown += new KeyEventHandler(textRawMessage_KeyDown);
            listIdentifiers.DoubleClick += new EventHandler(listIdentifiers_DoubleClick);

            treeBasicMessages.AfterSelect += new TreeViewEventHandler(treeBasicMessages_AfterSelect);

            tabMessages.SelectedTab = tabBasic;

            iceChatMessages = MessageFormat;

            textFormattedText.SingleLine = true;
            textFormattedText.NoEmoticons = true;
            textFormattedBasic.SingleLine = true;
            textFormattedBasic.NoEmoticons = true;

            //populate Message Settings            

            UpdateMessageSettings();

            //load any plugin addons
            foreach (IPluginIceChat ipc in FormMain.Instance.IceChatPlugins)
            {
                if (ipc.Enabled == true)
                    ipc.LoadColorsForm(this.tabControlColors);
            }

            ApplyLanguage();

            if (FormMain.Instance.IceChatOptions.Theme != null)
            {
                foreach (ThemeItem theme in FormMain.Instance.IceChatOptions.Theme)
                {
                    ComboItem item = new ComboItem();
                    item.ThemeName = theme.ThemeName;
                    item.ThemeType = theme.ThemeType;
                    comboTheme.Items.Add(item);
                }
            }

            if (FormMain.Instance.IceChatPluginThemes != null)
            {
                foreach (IThemeIceChat theme in FormMain.Instance.IceChatPluginThemes)
                {
                    ComboItem item = new ComboItem();
                    item.ThemeName = theme.Name;
                    item.ThemeType = "Plugin";
                    item.FileName = theme.FileName;
                    comboTheme.Items.Add(item);
                }

            }

            comboTheme.Text = FormMain.Instance.IceChatOptions.CurrentTheme;

            this.comboTheme.SelectedIndexChanged += new System.EventHandler(this.comboTheme_SelectedIndexChanged);

        }

        private void FormColors_Load(object sender, EventArgs e)
        {
            if (this.Owner != null)
                this.Location = new Point(this.Owner.Location.X + this.Owner.Width / 2 - this.Width / 2,
                    this.Owner.Location.Y + this.Owner.Height / 2 - this.Height / 2);
        }

        private void ApplyLanguage()
        {
            
        }

        private void OnColor_Click(object sender, EventArgs e)
        {
            currentColorPick = sender;
            colorPicker.SelectedColor = (int)((PictureBox)sender).Tag;
            labelCurrent.Text = "Current Selected:" + Environment.NewLine + GetLabelText((PictureBox)sender);            
        }

        private string GetLabelText(PictureBox sender)
        {
            try
            {
                Label l = this.GetType().GetField("label" + sender.Name.Substring(7).ToString(), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(this) as Label;
                return l.Text;
            }
            catch (NullReferenceException)
            {
                return "";
            }
        }

        private void textRawMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.K)
                {
                    textRawMessage.SelectedText = ((char)3).ToString();
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.B)
                {
                    textRawMessage.SelectedText = ((char)2).ToString();
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.U)
                {
                    textRawMessage.SelectedText = ((char)31).ToString();
                    e.Handled = true;
                }
            }
        }

        private void listIdentifiers_DoubleClick(object sender, EventArgs e)
        {
            int s = listIdentifiers.SelectedIndex;

            if (s == -1) return;

            string t = listIdentifiers.Items[s].ToString();
            t = t.Substring(0,t.IndexOf(" "));
            textRawMessage.SelectedText = t;
        }

        #region AddMessageIdentifiers

        private void AddMsgIdent(string MessageName, string Identifier)
        {
            if (messageIdentifiers.Contains(MessageName))
            {
                ArrayList idents = (ArrayList)messageIdentifiers[MessageName];
                bool Found = false;
                int i = 0;

                IEnumerator myEnum = idents.GetEnumerator();
                while (myEnum.MoveNext())
                {
                    if (myEnum.Current.ToString().IndexOf(Identifier.Substring(0, Identifier.IndexOf(' '))) > -1)
                    {
                        Found = true;
                        break;
                    }
                    i++;
                }

                if (!Found)
                    idents.Add(Identifier);
                else
                    idents[i] = Identifier;

                messageIdentifiers[MessageName] = idents;
            }
            else
            {
                ArrayList a = new ArrayList();
                a.Add(Identifier);
                messageIdentifiers.Add(MessageName, a);
            }

        }

        private void AddMessageIdentifiers()
        {
            AddMsgIdent("Server Connect", "$server - actual server name");
            AddMsgIdent("Server Connect", "$serverip - server IP Address");
            AddMsgIdent("Server Connect", "$port - server port");

            AddMsgIdent("Server Disconnect", "$server - server name");
            AddMsgIdent("Server Disconnect", "$serverip - server IP Address");
            AddMsgIdent("Server Disconnect", "$port - server port");

            AddMsgIdent("Server Reconnect", "$server - server name");
            AddMsgIdent("Server Reconnect", "$serverip - server IP Address");
            AddMsgIdent("Server Reconnect", "$port - server port");

            AddMsgIdent("Channel Invite", "$nick - nickname who invited you");
            AddMsgIdent("Channel Invite", "$host - hostname of nick");
            AddMsgIdent("Channel Invite", "$channel - channel you were invited to");

            AddMsgIdent("Channel Message", "$nick - nickname who messaged");
            AddMsgIdent("Channel Message", "$host - hostname of nick");
            AddMsgIdent("Channel Message", "$status - op/voice status of nick");
            AddMsgIdent("Channel Message", "$channel - channel name");
            AddMsgIdent("Channel Message", "$message - channel message");
            AddMsgIdent("Channel Message", "$color - nickname nicklist color");

            AddMsgIdent("Channel Action", "$nick - nickname who performed action");
            AddMsgIdent("Channel Action", "$host - hostname of nick");
            AddMsgIdent("Channel Action", "$status - op/voice status of nick");
            AddMsgIdent("Channel Action", "$channel - channel name");
            AddMsgIdent("Channel Action", "$message - channel action");

            AddMsgIdent("Channel Join", "$nick - nickname who joined");
            AddMsgIdent("Channel Join", "$host - hostname of nick");
            AddMsgIdent("Channel Join", "$channel - channel name");
            AddMsgIdent("Channel Join", "$account - account name");

            AddMsgIdent("Channel Part", "$nick - nickname who parted");
            AddMsgIdent("Channel Part", "$host - hostname of nick");
            AddMsgIdent("Channel Part", "$status - op/voice status of nick");
            AddMsgIdent("Channel Part", "$channel - channel name");
            AddMsgIdent("Channel Part", "$reason - part reason");

            AddMsgIdent("Channel Kick", "$nick - nickname who performed kick");
            AddMsgIdent("Channel Kick", "$host - hostname of nick who performed");
            //AddMsgIdent("Channel Kick", "$status - op/voice status of nick");
            AddMsgIdent("Channel Kick", "$channel - channel name");
            AddMsgIdent("Channel Kick", "$kickee - the nick who was kicked");
            AddMsgIdent("Channel Kick", "$reason - kick reason");

            AddMsgIdent("Channel Mode", "$nick - who performed the mode change");
            AddMsgIdent("Channel Mode", "$host - host of who performed the mode change");
            AddMsgIdent("Channel Mode", "$channel - what channel had the mode change");
            AddMsgIdent("Channel Mode", "$mode - the channel mode that changed");
            AddMsgIdent("Channel Mode", "$modeparam - the parameters for the mode");

            AddMsgIdent("Channel Notice", "$nick - nickname who noticed");
            AddMsgIdent("Channel Notice", "$host - host of nick");
            AddMsgIdent("Channel Notice", "$status - the status level in the channel the notice was sent to");
            AddMsgIdent("Channel Notice", "$channel - the channel the notice was sent to");
            AddMsgIdent("Channel Notice", "$message - the notice message sent");

            AddMsgIdent("Channel Other", "$message - the message");

            AddMsgIdent("Channel Nick Change", "$nick - the old nick name");
            AddMsgIdent("Channel Nick Change", "$host - hostname");
            AddMsgIdent("Channel Nick Change", "$newnick - the new nick name");

            AddMsgIdent("Self Channel Join", "$nick - yourself");
            AddMsgIdent("Self Channel Join", "$host - your hostname");
            AddMsgIdent("Self Channel Join", "$channel - channel name");

            AddMsgIdent("Self Channel Part", "$nick - yourself");
            AddMsgIdent("Self Channel Part", "$host - your hostname");
            AddMsgIdent("Self Channel Part", "$status - your op/voice status");
            AddMsgIdent("Self Channel Part", "$channel - channel name");
            AddMsgIdent("Self Channel Part", "$reason - part reason");

            AddMsgIdent("Self Channel Kick", "$nick - yourself");
            //AddMsgIdent("Self Channel Kick", "$status - your op/voice status");
            AddMsgIdent("Self Channel Kick", "$channel - channel name");
            AddMsgIdent("Self Channel Kick", "$kicker - the nick who performed the kick");
            AddMsgIdent("Self Channel Kick", "$host - the host of who performed the kick");
            AddMsgIdent("Self Channel Kick", "$reason - kick reason");

            AddMsgIdent("Self Channel Message", "$nick - yourself");
            //AddMsgIdent("Self Channel Message", "$host - your hostname ");
            AddMsgIdent("Self Channel Message", "$status - your op/voice status");
            AddMsgIdent("Self Channel Message", "$channel - channel name");
            AddMsgIdent("Self Channel Message", "$message - channel message");
            AddMsgIdent("Self Channel Message", "$color - nickname nicklist color");

            AddMsgIdent("Self Channel Action", "$nick - yourself");
            //AddMsgIdent("Self Channel Action", "$host - your hostname");
            //AddMsgIdent("Self Channel Action", "$status - your op/voice status");
            AddMsgIdent("Self Channel Action", "$channel - channel name");
            AddMsgIdent("Self Channel Action", "$message - channel action");

            AddMsgIdent("Self Nick Change", "$nick - your old nick name");
            AddMsgIdent("Self Nick Change", "$host - your hostname");
            AddMsgIdent("Self Nick Change", "$newnick - your new nick name");

            AddMsgIdent("Self Notice", "$nick - who you are sending notice to");
            AddMsgIdent("Self Notice", "$message - the notice message");

            AddMsgIdent("Private Message", "$nick - nickname who messaged");
            AddMsgIdent("Private Message", "$host - hostname of nick");
            AddMsgIdent("Private Message", "$message - channel message");

            AddMsgIdent("Self Private Message", "$nick - yourself");
            AddMsgIdent("Self Private Message", "$host - your hostname");
            AddMsgIdent("Self Private Message", "$message - private message");

            AddMsgIdent("Private Action", "$nick - nickname who performed action");
            AddMsgIdent("Private Action", "$host - hostname of nick");
            AddMsgIdent("Private Action", "$message - private action");

            AddMsgIdent("Self Private Action", "$nick - yourself");
            AddMsgIdent("Self Private Action", "$host - your hostname");
            AddMsgIdent("Self Private Action", "$message - private action");

            AddMsgIdent("Server Mode", "$mode - the mode the server changed for you");
            AddMsgIdent("Server Mode", "$nick - your nickname");
            AddMsgIdent("Server Mode", "$server - the server name");

            AddMsgIdent("User Notice", "$nick - who sent the notice");
            AddMsgIdent("User Notice", "$message - the notice");

            AddMsgIdent("User Echo", "$message - the message to echo");

            AddMsgIdent("User Whois", "$nick - the nick for the whois");
            AddMsgIdent("User Whois", "$data - the whois information");

            AddMsgIdent("User Error", "$message - the error message");

            AddMsgIdent("Server Notice", "$server - the server name");
            AddMsgIdent("Server Notice", "$message - the notice");
            AddMsgIdent("Server Notice", "$nick - your nickname");

            AddMsgIdent("Server MOTD", "$message - the MOTD message");

            AddMsgIdent("Server Quit", "$nick - nickname who quit");
            AddMsgIdent("Server Quit", "$host - hostname of nick");
            AddMsgIdent("Server Quit", "$reason - quit reason");

            AddMsgIdent("Server Message", "$server - the server name");
            AddMsgIdent("Server Message", "$message - the message");

            AddMsgIdent("Server Error", "$server - the server name");
            AddMsgIdent("Server Error", "$message - the error message");

            AddMsgIdent("CTCP Request", "$nick - the nick the Ctcp request is for");
            AddMsgIdent("CTCP Request", "$ctcp - the Ctcp you wish to request for");

            AddMsgIdent("CTCP Reply", "$nick - who you send the Ctcp Request to");
            AddMsgIdent("CTCP Reply", "$host - the host of the nick");
            AddMsgIdent("CTCP Reply", "$ctcp - which Ctcp was requested");
            AddMsgIdent("CTCP Reply", "$reply - the Ctcp was reply");

            AddMsgIdent("CTCP Send", "$nick - the nick you want to send a Ctcp Request to");
            AddMsgIdent("CTCP Send", "$ctcp - the Ctcp you wish to request");

            AddMsgIdent("Channel Topic Change", "$topic - channel topic");
            AddMsgIdent("Channel Topic Change", "$channel - channel name");
            AddMsgIdent("Channel Topic Change", "$nick - who changed topic");
            AddMsgIdent("Channel Topic Change", "$host - who changed topic host");

            AddMsgIdent("Channel Topic Text", "$channel - channel name");
            AddMsgIdent("Channel Topic Text", "$topic - channel topic");

            AddMsgIdent("DCC Chat Request", "$nick - nickname of person who requests chat");
            AddMsgIdent("DCC Chat Request", "$host - host of nickname");

            AddMsgIdent("DCC File Send", "$nick - nickname of person who is sending file");
            AddMsgIdent("DCC File Send", "$host - host of nickname");
            AddMsgIdent("DCC File Send", "$file - name of the file trying to be sent");
            AddMsgIdent("DCC File Send", "$filesize - the size in bytes of file");

            AddMsgIdent("DCC Chat Message", "$nick - nickname who messaged");
            AddMsgIdent("DCC Chat Message", "$message - chat message");

            AddMsgIdent("Self DCC Chat Message", "$nick - yourself");
            AddMsgIdent("Self DCC Chat Message", "$host - your hostname");
            AddMsgIdent("Self DCC Chat Message", "$message - chat message");

            AddMsgIdent("DCC Chat Action", "$nick - nickname who performed action");
            AddMsgIdent("DCC Chat Action", "$message - the action performed");

            AddMsgIdent("Self DCC Chat Action", "$nick - yourself");
            AddMsgIdent("Self DCC Chat Action", "$host - your hostname");
            AddMsgIdent("Self DCC Chat Action", "$message - the action performed");

            AddMsgIdent("DCC Chat Request", "$nick - nickname who performed request");
            AddMsgIdent("DCC Chat Request", "$host- nickname host");

            AddMsgIdent("DCC Chat Outgoing", "$nick - who you want to chat with");
            AddMsgIdent("DCC Chat Connect", "$nick - who you are chatting with");
            AddMsgIdent("DCC Chat Disconnect", "$nick - who you are chatting with");
            AddMsgIdent("DCC Chat Timeout", "$nick - who you want to chat with");

        }

        #endregion

        private void colorPicker_OnClick(int colorSelected)
        {
            if (tabControlColors.SelectedTab.Text == "Messages")
            {
                //check if we are in basic or advanced
                if (tabMessages.SelectedTab.Text == "Advanced")
                {

                    if (treeMessages.SelectedNode == null)
                        return;

                    //add in the color code in the current place in the textbox
                    if (this.checkBGColor.Checked == true)
                    {
                        textFormattedText.IRCBackColor = colorSelected;

                        if (textRawMessage.Text.StartsWith(""))
                        {
                            //find the comma if it exists
                            if (textRawMessage.SelectionStart == 0)
                            {
                                int result;
                                if (int.TryParse(textRawMessage.Text.Substring(1, 2), out result))
                                {
                                    if (textRawMessage.Text.Substring(3, 1) == ",")
                                    {
                                        System.Diagnostics.Debug.WriteLine(textRawMessage.Text.Substring(4, 2));
                                        int result2;
                                        if (int.TryParse(textRawMessage.Text.Substring(4, 2), out result2))
                                        {
                                            System.Diagnostics.Debug.WriteLine("01,01");

                                        }
                                        else
                                        {
                                            System.Diagnostics.Debug.WriteLine("01,1");
                                        }
                                    }
                                    else
                                    {
                                        //comma is not after the color


                                    }
                                    
                                    System.Diagnostics.Debug.WriteLine(textRawMessage.Text.Substring(3,1));
                                    System.Diagnostics.Debug.WriteLine("2");
                                    //textRawMessage.Text = "" + textRawMessage.Text.Substring(1,2) +"," + colorSelected.ToString("00")
                                    textRawMessage.Text = "" + colorSelected.ToString("00") + textRawMessage.Text.Substring(3);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("1");
                                    if (textRawMessage.Text.Substring(2, 1) == ",")
                                    {
                                    


                                    }
                                    
                                    
                                    textRawMessage.Text = "" + colorSelected.ToString("00") + textRawMessage.Text.Substring(2);
                                }
                            }
                            
                            //else
                            //    this.textRawMessage.SelectedText = "" + colorSelected.ToString("00");
                            
                        }


                    }
                    else
                    {
                        if (textRawMessage.Text.StartsWith(""))
                        {
                            if (textRawMessage.SelectionStart == 0)
                            {
                                int result;
                                if (int.TryParse(textRawMessage.Text.Substring(1, 2), out result))
                                    textRawMessage.Text = "" + colorSelected.ToString("00") + textRawMessage.Text.Substring(3);
                                else
                                    textRawMessage.Text = "" + colorSelected.ToString("00") + textRawMessage.Text.Substring(2);
                            }
                            else
                                this.textRawMessage.SelectedText = "" + colorSelected.ToString("00");
                        }
                        else
                            this.textRawMessage.Text = "" + colorSelected.ToString("00") + textRawMessage.Text;
                    }
                }
                else
                {
                    //basic settings
                    if (treeBasicMessages.SelectedNode == null)
                    {
                        MessageBox.Show("Please select a message to edit","Edit Colors");                        
                        return;
                    }
                    else if (treeBasicMessages.SelectedNode.Tag == null)
                    {
                        MessageBox.Show("Please select a message to edit", "Edit Colors");                        
                        return;
                    }
                    if (this.checkChangeBGBasic.Checked == true)
                    {
                        textFormattedBasic.IRCBackColor = colorSelected;
                    }
                    else
                    {                        
                        string message = treeBasicMessages.SelectedNode.Tag.ToString();
                        message = message.Replace("&#x3;", ((char)3).ToString());
                        message = RemoveColorCodes(message);
                        
                        message = "" + colorSelected.ToString("00") + message;
                        message = message.Replace(((char)3).ToString(),"&#x3;");

                        treeBasicMessages.SelectedNode.Tag = message;
                        
                        UpdateBasicText();
                    }
                }
            }

            if (tabControlColors.SelectedTab.Text == "Nick List")
            {
                if (currentColorPick != null)
                {
                    ((PictureBox)currentColorPick).BackColor = IrcColor.colors[colorSelected];
                    ((PictureBox)currentColorPick).Tag = colorSelected;
                }
            }
            
            if (tabControlColors.SelectedTab.Text == "Server Tree")
            {
                if (currentColorPick != null)
                {
                    ((PictureBox)currentColorPick).BackColor = IrcColor.colors[colorSelected];
                    ((PictureBox)currentColorPick).Tag = colorSelected;
                }
            }
            
            if (tabControlColors.SelectedTab.Text == "Other")
            {
                if (currentColorPick != null)
                {
                    ((PictureBox)currentColorPick).BackColor = IrcColor.colors[colorSelected];
                    ((PictureBox)currentColorPick).Tag = colorSelected;
                }
            }
        }

        private void treeBasicMessages_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent == null)
            {
                textFormattedBasic.ClearTextWindow();
                return;
            }

            if (treeBasicMessages.SelectedNode == null)
                return;

            if (treeBasicMessages.SelectedNode.Parent == null)
                return;

            string type = e.Node.Text.Split(' ').GetValue(0).ToString();
            if (type == "Server" || type == "Ctcp")
                textFormattedBasic.IRCBackColor = iceChatColors.ConsoleBackColor;
            else if (type == "User")
                textFormattedBasic.IRCBackColor = iceChatColors.ConsoleBackColor;
            else if (type == "Channel")
                textFormattedBasic.IRCBackColor = iceChatColors.ChannelBackColor;
            else if (type == "Private")
                textFormattedBasic.IRCBackColor = iceChatColors.QueryBackColor;
            else if (type == "DCC")
                textFormattedBasic.IRCBackColor = iceChatColors.QueryBackColor;
            else if (type == "Self")
            {
                type = e.Node.Text.Split(' ').GetValue(1).ToString();
                if (type == "Server" || type == "Notice")
                    textFormattedBasic.IRCBackColor = iceChatColors.ConsoleBackColor;
                else if (type == "Channel")
                    textFormattedBasic.IRCBackColor = iceChatColors.ChannelBackColor;
                else if (type == "Nick")
                    textFormattedBasic.IRCBackColor = iceChatColors.ChannelBackColor;
                else if (type == "DCC")
                    textFormattedBasic.IRCBackColor = iceChatColors.QueryBackColor;
                else if (type == "Private")
                    textFormattedBasic.IRCBackColor = iceChatColors.QueryBackColor;
                else
                    textFormattedBasic.IRCBackColor = 0;
            }
            else
                textFormattedBasic.IRCBackColor = 0;

            this.listIdentifiers.Items.Clear();

            IDictionaryEnumerator msgIdent = messageIdentifiers.GetEnumerator();
            while (msgIdent.MoveNext())
            {
                if (msgIdent.Key.ToString().ToLower() == e.Node.Text.ToLower())
                {
                    ArrayList idents = (ArrayList)msgIdent.Value;
                    foreach (object ident in idents)
                        this.listIdentifiers.Items.Add(ident);
                }
            }

            UpdateBasicText();
        
        }

        private void treeMessages_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent == null)
            {
                textRawMessage.Text = null;
                textFormattedText.ClearTextWindow();
                return;
            }

            //get the window type and set the background color
            string type = e.Node.Text.Split(' ').GetValue(0).ToString();
            if (type == "Server" || type == "Ctcp")
                textFormattedText.IRCBackColor = iceChatColors.ConsoleBackColor;
            else if (type == "User")
                textFormattedText.IRCBackColor = iceChatColors.ConsoleBackColor;
            else if (type == "Channel")
                textFormattedText.IRCBackColor = iceChatColors.ChannelBackColor;
            else if (type == "Private")
                textFormattedText.IRCBackColor = iceChatColors.QueryBackColor;
            else if (type == "DCC")
                textFormattedText.IRCBackColor = iceChatColors.QueryBackColor;
            else if (type == "Self")
            {
                type = e.Node.Text.Split(' ').GetValue(1).ToString();
                if (type == "Server" || type == "Notice")
                    textFormattedText.IRCBackColor = iceChatColors.ConsoleBackColor;
                else if (type == "Channel")
                    textFormattedText.IRCBackColor = iceChatColors.ChannelBackColor;
                else if (type == "Nick")
                    textFormattedText.IRCBackColor = iceChatColors.ChannelBackColor;
                else if (type == "DCC")
                    textFormattedText.IRCBackColor = iceChatColors.QueryBackColor;
                else if (type == "Private")
                    textFormattedText.IRCBackColor = iceChatColors.QueryBackColor;
                else
                    textFormattedText.IRCBackColor = 0;
            }
            else
                textFormattedText.IRCBackColor = 0;

            textRawMessage.Text = e.Node.Tag.ToString();
            //replace the color code
            textRawMessage.Text = textRawMessage.Text.Replace("&#x3;", ((char)3).ToString());
            //replace the bold code
            textRawMessage.Text = textRawMessage.Text.Replace("&#x2;", ((char)2).ToString());


            this.listIdentifiers.Items.Clear();

            IDictionaryEnumerator msgIdent = messageIdentifiers.GetEnumerator();
            while (msgIdent.MoveNext())
            {
                if (msgIdent.Key.ToString().ToLower() == e.Node.Text.ToLower())
                {
                    ArrayList idents = (ArrayList)msgIdent.Value;
                    foreach (object ident in idents)
                        this.listIdentifiers.Items.Add(ident);
                }
            }

            UpdateFormattedText();

        }

        private void UpdateBasicText()
        {
            this.textFormattedBasic.ClearTextWindow();

            string message = treeBasicMessages.SelectedNode.Tag.ToString();

            SetMessageFormat(treeBasicMessages.SelectedNode.Text, message);

            //replace some of the basic identifiers to make it look right            
            if (CheckIdentifier("$status"))
                message = message.Replace("$status", "@");

            if (CheckIdentifier("$nick"))
                message = message.Replace("$nick", "Nick");

            if (CheckIdentifier("$newnick"))
                message = message.Replace("$newnick", "NewNick");

            if (CheckIdentifier("$kickee"))
                message = message.Replace("$kickee", "ThisNick");

            if (CheckIdentifier("$kicker"))
                message = message.Replace("$kicker", "WhoKicked");

            if (CheckIdentifier("$channel"))
                message = message.Replace("$channel", "#channel");

            if (CheckIdentifier("$host"))
                message = message.Replace("$host", "ident@host.com");

            if (CheckIdentifier("$reason"))
                message = message.Replace("$reason", "(Reason)");

            if (CheckIdentifier("$account"))
                message = message.Replace("$account", "[account]");
            
            if (CheckIdentifier("$message"))
                message = message.Replace("$message", "message");

            if (CheckIdentifier("$modeparam"))
                message = message.Replace("$modeparam", "nick!ident@host");

            if (CheckIdentifier("$mode"))
                message = message.Replace("$mode", "+o");

            if (CheckIdentifier("$ctcp"))
                message = message.Replace("$ctcp", "VERSION");

            if (CheckIdentifier("$reply"))
                message = message.Replace("$reply", "CTCP Reply");

            if (CheckIdentifier("$serverip"))
                message = message.Replace("$serverip", "192.168.1.101");

            if (CheckIdentifier("$server"))
                message = message.Replace("$server", "irc.server.com");

            if (CheckIdentifier("$port"))
                message = message.Replace("$port", "6667");

            if (CheckIdentifier("$topic"))
                message = message.Replace("$topic", "The Channel Topic");

            if (CheckIdentifier("$filesize"))
                message = message.Replace("$filesize", "512");

            if (CheckIdentifier("$file"))
                message = message.Replace("$file", "file.ext");

            message = message.Replace("$color", ((char)3).ToString() + "12");

            this.textFormattedBasic.AppendText(message, 1);

        }


        private void UpdateFormattedText()
        {
            if (treeMessages.SelectedNode == null)
                return;

            if (treeMessages.SelectedNode.Parent == null)
                return;

            this.textFormattedText.ClearTextWindow();
            string message = this.textRawMessage.Text;

            treeMessages.SelectedNode.Tag = message;

            SetMessageFormat(treeMessages.SelectedNode.Text, message);

            //replace some of the basic identifiers to make it look right            
            if (CheckIdentifier("$status"))
                message = message.Replace("$status", "@");
            
            if (CheckIdentifier("$nick"))            
                message = message.Replace("$nick", "Nick");
            
            if (CheckIdentifier("$newnick"))
                message = message.Replace("$newnick", "NewNick");
            
            if (CheckIdentifier("$kickee"))
                message = message.Replace("$kickee", "ThisNick");

            if (CheckIdentifier("$kicker"))
                message = message.Replace("$kicker", "WhoKicked");

            if (CheckIdentifier("$channel"))
                message = message.Replace("$channel", "#channel");
            
            if (CheckIdentifier("$host"))
                message = message.Replace("$host", "ident@host.com");
            
            if (CheckIdentifier("$reason"))
                message = message.Replace("$reason", "(Reason)");

            if (CheckIdentifier("$account"))
                message = message.Replace("$account", " [account]");

            if (CheckIdentifier("$message"))
                message = message.Replace("$message", "message");

            if (CheckIdentifier("$modeparam"))
                message = message.Replace("$modeparam", "nick!ident@host");
            
            if (CheckIdentifier("$mode"))
                message = message.Replace("$mode", "+o");
            
            if (CheckIdentifier("$ctcp"))
                message = message.Replace("$ctcp", "VERSION");
            
            if (CheckIdentifier("$reply"))
                message = message.Replace("$reply", "CTCP Reply");
            
            if (CheckIdentifier("$serverip"))
                message = message.Replace("$serverip", "192.168.1.101");
            
            if (CheckIdentifier("$server"))
                message = message.Replace("$server", "irc.server.com");
            
            if (CheckIdentifier("$port"))
                message = message.Replace("$port", "6667");
            
            if (CheckIdentifier("$topic"))
                message = message.Replace("$topic", "The Channel Topic");
            
            if (CheckIdentifier("$filesize"))
                message = message.Replace("$filesize", "512");
            
            if (CheckIdentifier("$file"))
                message = message.Replace("$file", "file.ext");

            message = message.Replace("$color", ((char)3).ToString() + "12");

            this.textFormattedText.AppendText(message, 1);

        }

        private bool CheckIdentifier(string identifier)
        {
            foreach (string m in listIdentifiers.Items)
            {                
                if (m.StartsWith(identifier))
                    return true;
            }
            return false;
        }

        private void SetMessageFormat(string MessageName, string MessageFormat)
        {
            foreach (ServerMessageFormatItem msg in iceChatMessages.MessageSettings)
            {
                if (msg.MessageName.ToLower() == MessageName.ToLower())
                {
                    msg.FormattedMessage = MessageFormat.Replace(((char)3).ToString(), "&#x3;").Replace(((char)2).ToString(), "&#x2;");
                    return;
                }
            }
        }

        private void textRawMessage_TextChanged(object sender, EventArgs e)
        {
            UpdateFormattedText();
        }

        private void UpdateMessageSettings()
        {
            treeMessages.Nodes.Clear();
            treeBasicMessages.Nodes.Clear();

            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Channel Messages");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Server Messages");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Private Messages");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Self Messages");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Ctcp");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("DCC");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Other");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Channel Messages");
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Server Messages");
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("Private Messages");
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("Self Messages");
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("Ctcp");
            System.Windows.Forms.TreeNode treeNode13 = new System.Windows.Forms.TreeNode("DCC");
            System.Windows.Forms.TreeNode treeNode14 = new System.Windows.Forms.TreeNode("Other");

            this.treeBasicMessages.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4,
            treeNode5,
            treeNode6,
            treeNode7});

            this.treeMessages.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode8,
            treeNode9,
            treeNode10,
            treeNode11,
            treeNode12,
            treeNode13,
            treeNode14});

            if (iceChatMessages.MessageSettings != null)
            {
                foreach (ServerMessageFormatItem msg in iceChatMessages.MessageSettings)
                {
                    if (msg.MessageName.StartsWith("Channel"))
                    {
                        TreeNode t = new TreeNode(msg.MessageName);
                        t.Tag = msg.FormattedMessage;
                        treeMessages.Nodes[0].Nodes.Add(t);

                        TreeNode t2 = new TreeNode(msg.MessageName);
                        t2.Tag = msg.FormattedMessage;
                        treeBasicMessages.Nodes[0].Nodes.Add(t2);
                    }
                    else if (msg.MessageName.StartsWith("Server"))
                    {
                        TreeNode t = new TreeNode(msg.MessageName);
                        t.Tag = msg.FormattedMessage;
                        treeMessages.Nodes[1].Nodes.Add(t);

                        TreeNode t2 = new TreeNode(msg.MessageName);
                        t2.Tag = msg.FormattedMessage;
                        treeBasicMessages.Nodes[1].Nodes.Add(t2);
                    }
                    else if (msg.MessageName.StartsWith("Private"))
                    {
                        TreeNode t = new TreeNode(msg.MessageName);
                        t.Tag = msg.FormattedMessage;
                        treeMessages.Nodes[2].Nodes.Add(t);

                        TreeNode t2 = new TreeNode(msg.MessageName);
                        t2.Tag = msg.FormattedMessage;
                        treeBasicMessages.Nodes[2].Nodes.Add(t2);

                    }
                    else if (msg.MessageName.StartsWith("Self"))
                    {
                        TreeNode t = new TreeNode(msg.MessageName);
                        t.Tag = msg.FormattedMessage;
                        treeMessages.Nodes[3].Nodes.Add(t);

                        TreeNode t2 = new TreeNode(msg.MessageName);
                        t2.Tag = msg.FormattedMessage;
                        treeBasicMessages.Nodes[3].Nodes.Add(t2);
                    }
                    else if (msg.MessageName.StartsWith("Ctcp"))
                    {
                        TreeNode t = new TreeNode(msg.MessageName);
                        t.Tag = msg.FormattedMessage;
                        treeMessages.Nodes[4].Nodes.Add(t);

                        TreeNode t2 = new TreeNode(msg.MessageName);
                        t2.Tag = msg.FormattedMessage;
                        treeBasicMessages.Nodes[4].Nodes.Add(t2);
                    }
                    else if (msg.MessageName.StartsWith("DCC"))
                    {
                        TreeNode t = new TreeNode(msg.MessageName);
                        t.Tag = msg.FormattedMessage;
                        treeMessages.Nodes[5].Nodes.Add(t);

                        TreeNode t2 = new TreeNode(msg.MessageName);
                        t2.Tag = msg.FormattedMessage;
                        treeBasicMessages.Nodes[5].Nodes.Add(t2);
                    }
                    else
                    {
                        TreeNode t = new TreeNode(msg.MessageName);
                        t.Tag = msg.FormattedMessage;
                        treeMessages.Nodes[6].Nodes.Add(t);

                        TreeNode t2 = new TreeNode(msg.MessageName);
                        t2.Tag = msg.FormattedMessage;
                        treeBasicMessages.Nodes[6].Nodes.Add(t2);
                    }
                }
            }

            treeMessages.ExpandAll();
            treeBasicMessages.ExpandAll();    

        }

        private void UpdateColorSettings()
        {
            this.pictureOwner.BackColor = IrcColor.colors[iceChatColors.ChannelOwnerColor];
            this.pictureOwner.Tag = iceChatColors.ChannelOwnerColor;

            this.pictureAdmin.BackColor = IrcColor.colors[iceChatColors.ChannelAdminColor];
            this.pictureAdmin.Tag = iceChatColors.ChannelAdminColor;
            
            this.pictureOperator.BackColor = IrcColor.colors[iceChatColors.ChannelOpColor];
            this.pictureOperator.Tag = iceChatColors.ChannelOpColor;

            this.pictureHalfOperator.BackColor = IrcColor.colors[iceChatColors.ChannelHalfOpColor];
            this.pictureHalfOperator.Tag = iceChatColors.ChannelHalfOpColor;

            this.pictureVoice.BackColor = IrcColor.colors[iceChatColors.ChannelVoiceColor];
            this.pictureVoice.Tag = iceChatColors.ChannelVoiceColor;

            this.pictureDefault.BackColor = IrcColor.colors[iceChatColors.ChannelRegularColor];
            this.pictureDefault.Tag = iceChatColors.ChannelRegularColor;

            this.pictureTabCurrent.BackColor = IrcColor.colors[iceChatColors.TabBarCurrent];
            this.pictureTabCurrent.Tag = iceChatColors.TabBarCurrent;

            this.pictureTabMessage.BackColor = IrcColor.colors[iceChatColors.TabBarNewMessage];
            this.pictureTabMessage.Tag = iceChatColors.TabBarNewMessage;

            this.pictureTabJoin.BackColor = IrcColor.colors[iceChatColors.TabBarChannelJoin];
            this.pictureTabJoin.Tag = iceChatColors.TabBarChannelJoin;

            this.pictureTabPart.BackColor = IrcColor.colors[iceChatColors.TabBarChannelPart];
            this.pictureTabPart.Tag = iceChatColors.TabBarChannelPart;

            this.pictureTabQuit.BackColor = IrcColor.colors[iceChatColors.TabBarServerQuit];
            this.pictureTabQuit.Tag = iceChatColors.TabBarServerQuit;

            this.pictureTabServer.BackColor = IrcColor.colors[iceChatColors.TabBarServerMessage];
            this.pictureTabServer.Tag = iceChatColors.TabBarServerMessage;

            this.pictureTabOther.BackColor = IrcColor.colors[iceChatColors.TabBarOtherMessage];
            this.pictureTabOther.Tag = iceChatColors.TabBarOtherMessage;

            this.pictureTabAction.BackColor = IrcColor.colors[iceChatColors.TabBarNewAction];
            this.pictureTabAction.Tag = iceChatColors.TabBarNewAction;

            this.pictureTabServerNotice.BackColor = IrcColor.colors[iceChatColors.TabBarServerNotice];
            this.pictureTabServerNotice.Tag = iceChatColors.TabBarServerNotice;

            this.pictureTabDefault.BackColor = IrcColor.colors[iceChatColors.TabBarDefault];
            this.pictureTabDefault.Tag = iceChatColors.TabBarDefault;

            this.pictureTabBarCurrent1.BackColor = IrcColor.colors[iceChatColors.TabBarCurrentBG1];
            this.pictureTabBarCurrent1.Tag = iceChatColors.TabBarCurrentBG1;

            this.pictureTabBarCurrent2.BackColor = IrcColor.colors[iceChatColors.TabBarCurrentBG2];
            this.pictureTabBarCurrent2.Tag = iceChatColors.TabBarCurrentBG2;

            this.pictureTabBarOther1.BackColor = IrcColor.colors[iceChatColors.TabBarOtherBG1];
            this.pictureTabBarOther1.Tag = iceChatColors.TabBarOtherBG1;

            this.pictureTabBarOther2.BackColor = IrcColor.colors[iceChatColors.TabBarOtherBG2];
            this.pictureTabBarOther2.Tag = iceChatColors.TabBarOtherBG2;

            this.pictureTabBarHover1.BackColor = IrcColor.colors[iceChatColors.TabBarHoverBG1];
            this.pictureTabBarHover1.Tag = iceChatColors.TabBarHoverBG1;

            this.pictureTabBarHover2.BackColor = IrcColor.colors[iceChatColors.TabBarHoverBG2];
            this.pictureTabBarHover2.Tag = iceChatColors.TabBarHoverBG2;

            this.pictureTabBackground.BackColor = IrcColor.colors[iceChatColors.TabbarBackColor];
            this.pictureTabBackground.Tag = iceChatColors.TabbarBackColor;


            this.picturePanelHeaderBG1.BackColor = IrcColor.colors[iceChatColors.PanelHeaderBG1];
            this.picturePanelHeaderBG1.Tag = iceChatColors.PanelHeaderBG1;

            this.picturePanelHeaderBG2.BackColor = IrcColor.colors[iceChatColors.PanelHeaderBG2];
            this.picturePanelHeaderBG2.Tag = iceChatColors.PanelHeaderBG2;

            this.picturePanelHeaderForeColor.BackColor = IrcColor.colors[iceChatColors.PanelHeaderForeColor];
            this.picturePanelHeaderForeColor.Tag = iceChatColors.PanelHeaderForeColor;

            this.pictureUnreadTextMarkerColor.BackColor = IrcColor.colors[iceChatColors.UnreadTextMarkerColor];
            this.pictureUnreadTextMarkerColor.Tag = iceChatColors.UnreadTextMarkerColor;

            this.pictureMenuBar.BackColor = IrcColor.colors[iceChatColors.MenubarBackColor];
            this.pictureMenuBar.Tag = iceChatColors.MenubarBackColor;

            this.pictureToolBar.BackColor = IrcColor.colors[iceChatColors.ToolbarBackColor];
            this.pictureToolBar.Tag = iceChatColors.ToolbarBackColor;

            this.pictureConsole.BackColor = IrcColor.colors[iceChatColors.ConsoleBackColor];
            this.pictureConsole.Tag = iceChatColors.ConsoleBackColor;

            this.pictureChannel.BackColor = IrcColor.colors[iceChatColors.ChannelBackColor];
            this.pictureChannel.Tag = iceChatColors.ChannelBackColor;

            this.pictureQuery.BackColor = IrcColor.colors[iceChatColors.QueryBackColor];
            this.pictureQuery.Tag = iceChatColors.QueryBackColor;

            this.pictureNickList.BackColor = IrcColor.colors[iceChatColors.NickListBackColor];
            this.pictureNickList.Tag = iceChatColors.NickListBackColor;

            this.pictureServerList.BackColor = IrcColor.colors[iceChatColors.ServerListBackColor];
            this.pictureServerList.Tag = iceChatColors.ServerListBackColor;

            this.pictureChannelList.BackColor = IrcColor.colors[iceChatColors.ChannelListBackColor];
            this.pictureChannelList.Tag = iceChatColors.ChannelListBackColor;

            this.pictureInputBox.BackColor = IrcColor.colors[iceChatColors.InputboxBackColor];
            this.pictureInputBox.Tag = iceChatColors.InputboxBackColor;

            this.pictureInputBoxFore.BackColor = IrcColor.colors[iceChatColors.InputboxForeColor];
            this.pictureInputBoxFore.Tag = iceChatColors.InputboxForeColor;

            this.pictureChannelListFore.BackColor = IrcColor.colors[iceChatColors.ChannelListForeColor];
            this.pictureChannelListFore.Tag = iceChatColors.ChannelListForeColor;

            this.pictureStatusBar.BackColor = IrcColor.colors[iceChatColors.StatusbarBackColor];
            this.pictureStatusBar.Tag = iceChatColors.StatusbarBackColor;

            this.pictureStatusFore.BackColor = IrcColor.colors[iceChatColors.StatusbarForeColor];
            this.pictureStatusFore.Tag = iceChatColors.StatusbarForeColor;

            this.pictureHyperlink.BackColor = IrcColor.colors[iceChatColors.HyperlinkColor];
            this.pictureHyperlink.Tag = iceChatColors.HyperlinkColor;

            this.checkRandomNickColors.Checked = iceChatColors.RandomizeNickColors;
        }

        private void UpdateColors()
        {
            iceChatColors.ChannelOwnerColor = (int)pictureOwner.Tag;
            iceChatColors.ChannelAdminColor = (int)pictureAdmin.Tag;
            iceChatColors.ChannelOpColor = (int)pictureOperator.Tag;
            iceChatColors.ChannelHalfOpColor = (int)pictureHalfOperator.Tag;
            iceChatColors.ChannelVoiceColor = (int)pictureVoice.Tag;
            iceChatColors.ChannelRegularColor = (int)pictureDefault.Tag;

            iceChatColors.TabBarCurrent = (int)pictureTabCurrent.Tag;
            iceChatColors.TabBarChannelJoin = (int)pictureTabJoin.Tag;
            iceChatColors.TabBarChannelPart = (int)pictureTabPart.Tag;
            iceChatColors.TabBarNewMessage = (int)pictureTabMessage.Tag;
            iceChatColors.TabBarServerMessage = (int)pictureTabServer.Tag;
            iceChatColors.TabBarServerQuit = (int)pictureTabQuit.Tag;
            iceChatColors.TabBarOtherMessage = (int)pictureTabOther.Tag;
            iceChatColors.TabBarDefault = (int)pictureTabDefault.Tag;
            iceChatColors.TabBarNewAction = (int)pictureTabAction.Tag;
            iceChatColors.TabBarServerNotice = (int)pictureTabServerNotice.Tag;

            iceChatColors.ConsoleBackColor = (int)pictureConsole.Tag;
            iceChatColors.ChannelBackColor = (int)pictureChannel.Tag;
            iceChatColors.QueryBackColor = (int)pictureQuery.Tag;
            iceChatColors.NickListBackColor = (int)pictureNickList.Tag;
            iceChatColors.ServerListBackColor = (int)pictureServerList.Tag;
            iceChatColors.ChannelListBackColor = (int)pictureChannelList.Tag;
            iceChatColors.InputboxBackColor = (int)pictureInputBox.Tag;

            iceChatColors.TabBarCurrentBG1 = (int)pictureTabBarCurrent1.Tag;
            iceChatColors.TabBarCurrentBG2 = (int)pictureTabBarCurrent2.Tag;
            iceChatColors.TabBarOtherBG1 = (int)pictureTabBarOther1.Tag;
            iceChatColors.TabBarOtherBG2 = (int)pictureTabBarOther2.Tag;
            iceChatColors.TabBarHoverBG1 = (int)pictureTabBarHover1.Tag;
            iceChatColors.TabBarHoverBG2 = (int)pictureTabBarHover2.Tag;
            iceChatColors.TabbarBackColor = (int)pictureTabBackground.Tag;

            iceChatColors.PanelHeaderBG1 = (int)picturePanelHeaderBG1.Tag;
            iceChatColors.PanelHeaderBG2 = (int)picturePanelHeaderBG2.Tag;
            iceChatColors.PanelHeaderForeColor = (int)picturePanelHeaderForeColor.Tag;
            iceChatColors.UnreadTextMarkerColor = (int)pictureUnreadTextMarkerColor.Tag;

            iceChatColors.MenubarBackColor = (int)pictureMenuBar.Tag;
            iceChatColors.ToolbarBackColor = (int)pictureToolBar.Tag;
            iceChatColors.ChannelListForeColor = (int)pictureChannelListFore.Tag;
            iceChatColors.InputboxForeColor = (int)pictureInputBoxFore.Tag;
            iceChatColors.HyperlinkColor = (int)pictureHyperlink.Tag;

            iceChatColors.StatusbarBackColor = (int)pictureStatusBar.Tag;
            iceChatColors.StatusbarForeColor = (int)pictureStatusFore.Tag;


            iceChatColors.RandomizeNickColors = checkRandomNickColors.Checked;
        }

        private void saveSettings()
        {
            UpdateColors();

            //save the icechat themes
            if (comboTheme.Items.Count > 0)
            {
                int count = 0;
                foreach (ComboItem theme in comboTheme.Items)
                    if (theme.ThemeType == "XML")
                        count++;

                FormMain.Instance.IceChatOptions.Theme = new ThemeItem[count];
                int i = 0;

                foreach (ComboItem theme in comboTheme.Items)
                {
                    if (theme.ThemeType == "XML")
                    {
                        //System.Diagnostics.Debug.WriteLine("saving theme: " + theme.ThemeName);
                        FormMain.Instance.IceChatOptions.Theme[i] = new ThemeItem();
                        FormMain.Instance.IceChatOptions.Theme[i].ThemeName = theme.ThemeName;
                        FormMain.Instance.IceChatOptions.Theme[i].ThemeType = "XML";
                        i++;
                    }
                }
                FormMain.Instance.IceChatOptions.CurrentTheme = comboTheme.Text;
            }
            else
            {
                FormMain.Instance.IceChatOptions.Theme = new ThemeItem[1];
                FormMain.Instance.IceChatOptions.Theme[0] = new ThemeItem();
                FormMain.Instance.IceChatOptions.Theme[0].ThemeName = "Default";
                FormMain.Instance.IceChatOptions.Theme[0].ThemeType = "XML";

                FormMain.Instance.IceChatOptions.CurrentTheme = "Default";
            }

            foreach (IPluginIceChat ipc in FormMain.Instance.IceChatPlugins)
            {
                if (ipc.Enabled)
                    ipc.SaveColorsForm();
            }

            if (SaveColors != null)
                SaveColors(this.iceChatColors, this.iceChatMessages);

        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            saveSettings();

            this.Close();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            saveSettings();
        }
        
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private string RemoveColorCodes(string line)
        {
            string ParseBackColor = @"\x03([0-9]{1,2}),([0-9]{1,2})";
            string ParseForeColor = @"\x03[0-9]{1,2}";
            string ParseColorChar = @"\x03";

            Regex ParseIRCCodes = new Regex(ParseBackColor + "|" + ParseForeColor + "|" + ParseColorChar);

            return ParseIRCCodes.Replace(line, "");
        }

        private string GetDefaultMessage(string MessageName)
        {
            string newMessage = "";

            switch (MessageName)
            {
                case "Server Connect":
                    newMessage = "&#x3;1*** Attempting to connect to $server ($serverip) on port $port";
                    break;
                case "Server Disconnect":
                    newMessage = "&#x3;1*** Server disconnected on $server";
                    break;
                case "Server Reconnect":
                    newMessage = "&#x3;1*** Attempting to re-connect to $server";
                    break;
                case "Channel Invite":
                    newMessage = "&#x3;3* $nick invites you to $channel";
                    break;
                case "Ctcp Reply":
                    newMessage = "&#x3;12[$nick $ctcp Reply] : $reply";
                    break;
                case "Ctcp Send":
                    newMessage =  "&#x3;10--> [$nick] $ctcp";
                    break;
                case "Ctcp Request":
                    newMessage = "&#x3;7[$nick] $ctcp";
                    break;
                case "Channel Mode":
                    newMessage = "&#x3;9* $nick sets mode $mode $modeparam for $channel";
                    break;
                case "Server Mode":
                    newMessage = "&#x3;6* Your mode is now $mode";
                    break;
                case "Server Notice":
                    newMessage = "&#x3;4*** $server $message";
                    break;
                case "Server Message":
                    newMessage = "&#x3;4-$server- $message";
                    break;                
                case "User Notice":
                    newMessage = "&#x3;4--$nick-- $message";
                    break;
                case "Channel Message":
                    newMessage =  "&#x3;1<$nick&#x3;> $message";
                    break;
                case "Self Channel Message":
                    newMessage = "&#x3;1<$nick&#x3;> $message";
                    break;
                case "Channel Action":
                    newMessage = "&#x3;5* $nick $message";
                    break;
                case "Self Channel Action":
                    newMessage = "&#x3;4* $nick $message";
                    break;
                case "Channel Join":
                    newMessage = "&#x3;7* $nick ($host)$account has joined channel $channel";
                    break;
                case "Self Channel Join":
                    newMessage = "&#x3;4* You have joined $channel";
                    break;
                case "Channel Part":
                    newMessage = "&#x3;3* $nick ($host) has left $channel $reason";
                    break;
                case "Self Channel Part":
                    newMessage = "&#x3;4* You have left $channel - You will be missed &#x3;10 $reason";
                    break;
                case "Server Quit":
                    newMessage = "&#x3;2* $nick ($host) Quit ($reason)";
                    break;
                case "Channel Nick Change":
                    newMessage = "&#x3;3* $nick is now known as $newnick";
                    break;
                case "Self Nick Change":
                    newMessage = "&#x3;4* You are now known as $newnick";
                    break;
                case "Channel Kick":
                    newMessage = "&#x3;4* $kickee was kicked by $nick($host) &#x3;3 - Reason ($reason)" ;
                    break;
                case "Self Channel Kick":
                    newMessage = "&#x3;4* You were kicked from $channel by $kicker (&#x3;3$reason)";
                    break;
                case "Private Message":
                    newMessage = "&#x3;1<$nick> $message";
                    break;
                case "Self Private Message":
                    newMessage = "&#x3;4<$nick>&#x3;1 $message";
                    break;
                case "Private Action":
                    newMessage = "&#x3;13* $nick $message";
                    break;
                case "Self Private Action":
                    newMessage = "&#x3;12* $nick $message";
                    break;
                case "DCC Chat Action":
                    newMessage = "&#x3;5* $nick $message";
                    break;
                case "Self DCC Chat Action":
                    newMessage = "&#x3;5* $nick $message";
                    break;
                case "DCC Chat Message":
                    newMessage = "&#x3;1<$nick> $message";
                    break;
                case "Self DCC Chat Message":
                    newMessage = "&#x3;4<$nick> $message";
                    break;
                case "DCC Chat Request":
                    newMessage = "&#x3;4* $nick ($host) is requesting a DCC Chat";
                    break;
                case "DCC File Send":
                    newMessage = "&#x3;4* $nick ($host) is trying to send you a file ($file) [$filesize bytes]";
                    break;
                case "Channel Topic Change":
                    newMessage = "&#x3;3* $nick changes topic to: $topic";
                    break;
                case "Channel Topic Text":
                    newMessage =  "&#x3;3Topic: $topic";
                    break;
                case "Server MOTD":
                    newMessage = "&#x3;3$message";
                    break;
                case "Channel Notice":
                    newMessage = "&#x3;5-$nick:$status$channel- $message";
                    break;
                case "Channel Other":
                    newMessage = "&#x3;1$message";
                    break;
                case "User Echo":
                    newMessage = "&#x3;7$message";
                    break;
                case "Server Error":
                    newMessage = "&#x3;4ERROR: $message";
                    break;
                case "User Whois":
                    newMessage = "&#x3;12->> $nick $data";
                    break;
                case "User Error":
                    newMessage = "&#x3;4ERROR: $message";
                    break;
                case "DCC Chat Connect":
                    newMessage = "&#x3;1* DCC Chat Connection Established with $nick";
                    break;
                case "DCC Chat Disconnect":
                    newMessage = "&#x3;1* DCC Chat Disconnected from $nick";
                    break;
                case "DCC Chat Outgoing":
                    newMessage = "&#x3;1* DCC Chat Requested with $nick";
                    break;
                case "DCC Chat Timeout":
                    newMessage = "&#x3;1* DCC Chat with $nick timed out";
                    break;
                case "Self Notice":
                    newMessage = "&#x3;1--> $nick - $message";
                    break;
            }

            return newMessage;
        }

        private void buttonResetBasic_Click(object sender, EventArgs e)
        {
            //reset the selected color setting back to the default color
            if (treeBasicMessages.SelectedNode == null)
                return;

            if (treeBasicMessages.SelectedNode.Parent == null)
                return;

            string newMessage = GetDefaultMessage(treeBasicMessages.SelectedNode.Text);
            if (newMessage.Length > 0)
            {
                this.textFormattedBasic.ClearTextWindow();
                this.textFormattedBasic.AppendText(newMessage, 1);
                this.treeBasicMessages.SelectedNode.Tag = newMessage;
            }
            treeBasicMessages.Select();
        }

        private void buttonResetAdvanced_Click(object sender, EventArgs e)
        {
            if (treeMessages.SelectedNode == null)
                return;

            if (treeMessages.SelectedNode.Parent == null)
                return;

            string newMessage = GetDefaultMessage(treeMessages.SelectedNode.Text);
            if (newMessage.Length > 0)
            {
                textRawMessage.Text = newMessage;
                //replace the color code
                textRawMessage.Text = textRawMessage.Text.Replace("&#x3;", ((char)3).ToString());
                //replace the bold code
                textRawMessage.Text = textRawMessage.Text.Replace("&#x2;", ((char)2).ToString());

                this.textFormattedText.ClearTextWindow();
                this.textFormattedText.AppendText(newMessage, 1);
                this.treeMessages.SelectedNode.Tag = newMessage;
            }
            treeMessages.Select();

        }

        private void buttonAddTheme_Click(object sender, EventArgs e)
        {
            //add the current color settings as a new theme
            //ask for the new theme
            InputBoxDialog i = new InputBoxDialog();
            i.FormPrompt = "New IceChat Theme Name";
            i.FormCaption = "Add IceChat Theme";
            if (i.ShowDialog() == DialogResult.OK)
            {
                if (i.InputResponse.Length > 0)
                {
                    //make file name theme-name.xml
                    string colorsFile = FormMain.Instance.CurrentFolder + System.IO.Path.DirectorySeparatorChar + "Colors-" + i.InputResponse + ".xml";

                    UpdateColors();

                    XmlSerializer serializer = new XmlSerializer(typeof(IceChatColors));
                    TextWriter textWriter = new StreamWriter(colorsFile);
                    serializer.Serialize(textWriter, iceChatColors);
                    textWriter.Close();
                    textWriter.Dispose();

                    string messageFile = FormMain.Instance.CurrentFolder + System.IO.Path.DirectorySeparatorChar + "Messages-" + i.InputResponse + ".xml";

                    XmlSerializer serializer2 = new XmlSerializer(typeof(IceChatMessageFormat));
                    TextWriter textWriter2 = new StreamWriter(messageFile);
                    serializer2.Serialize(textWriter2, iceChatMessages);
                    textWriter2.Close();
                    textWriter2.Dispose();

                    //add in the theme name into IceChatOptions
                    
                    
                    //comboTheme.Items.Add(i.InputResponse);
                    //make it the current theme

                    ComboItem item = new ComboItem();
                    item.ThemeName = i.InputResponse;
                    item.ThemeType = "XML";
                    comboTheme.Items.Add(item);
                    
                    comboTheme.Text = i.InputResponse;
                    
                }

            }
        }

        private void buttonRemoveTheme_Click(object sender, EventArgs e)
        {
            //remove the current selected theme
            if (comboTheme.Text == "Default")
                return;

            //theme files are not deleted

            //remove the theme
            comboTheme.Items.Remove(comboTheme.SelectedItem);

            comboTheme.SelectedIndex = 0;
        }

        private void comboTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            //change the current theme
            //ComboItem item = (ComboItem)comboTheme.SelectedItem;

            ComboItem item = (ComboItem)comboTheme.Items[comboTheme.SelectedIndex];
            
            System.Diagnostics.Debug.WriteLine("Change theme to " + item.ThemeName);
            
            string themeColorsFile = "";
            string themeMessagesFile = "";

            if (item.ThemeName == "Default" && item.ThemeType == "XML")
            {
                themeColorsFile = FormMain.Instance.CurrentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatColors.xml";
                themeMessagesFile = FormMain.Instance.CurrentFolder + System.IO.Path.DirectorySeparatorChar + "IceChatMessages.xml";
            }
            else if (item.ThemeType == "XML")
            {
                themeColorsFile = FormMain.Instance.CurrentFolder + System.IO.Path.DirectorySeparatorChar + "Colors-" + item.ThemeName + ".xml";
                themeMessagesFile = FormMain.Instance.CurrentFolder + System.IO.Path.DirectorySeparatorChar + "Messages-" + item.ThemeName + ".xml";
            }
            else if (item.ThemeType == "Plugin")
            {
                //update the stuff accordingly
                foreach (IThemeIceChat theme in FormMain.Instance.IceChatPluginThemes)
                {
                    if (theme.FileName == item.FileName)
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
                        iceChatColors.TabBarNewAction = theme.TabBarNewAction;
                        iceChatColors.TabBarServerNotice = theme.TabBarServerNotice;
                        iceChatColors.ToolbarBackColor = theme.ToolbarBackColor;
                        iceChatColors.UnreadTextMarkerColor = theme.UnreadTextMarkerColor;
                        

                    }
                }

            }
            
            if (themeColorsFile.Length > 0 && File.Exists(themeColorsFile) && item.ThemeType == "XML")
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(IceChatColors));
                TextReader textReader = new StreamReader(themeColorsFile);
                this.iceChatColors = (IceChatColors)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
                
                FormMain.Instance.IceChatOptions.CurrentTheme = item.ThemeName;
                FormMain.Instance.ColorsFile = themeColorsFile;
                UpdateColorSettings();
            }

            if (themeMessagesFile.Length > 0 && File.Exists(themeMessagesFile) && item.ThemeType == "XML")
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(IceChatMessageFormat));
                TextReader textReader = new StreamReader(themeMessagesFile);
                this.iceChatMessages = (IceChatMessageFormat)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
                
                FormMain.Instance.MessagesFile = themeMessagesFile;
                UpdateMessageSettings();

            }
        }

        private void buttonLoadTheme_Click(object sender, EventArgs e)
        {
            //load a new theme file(s)
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".xml";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.AddExtension = true;
            ofd.AutoUpgradeEnabled = true;
            ofd.Filter = "XML file (*.xml)|*.xml";
            ofd.Title = "Which Theme File do you want to load?";
            ofd.InitialDirectory = FormMain.Instance.CurrentFolder;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string themeName = "";
                if (System.IO.Path.GetFileName(ofd.FileName).StartsWith("Colors-"))
                {
                    //get the theme name
                    string f = System.IO.Path.GetFileName(ofd.FileName).Substring(7);
                    themeName = f.Substring(0, f.Length - 4);
                }

                if (System.IO.Path.GetFileName(ofd.FileName).StartsWith("Messages-"))
                {
                    //get the theme name
                    string f = System.IO.Path.GetFileName(ofd.FileName).Substring(9);
                    themeName = f.Substring(0, f.Length - 4);
                }

                if (themeName.Length > 0)
                {
                    //add the new theme name
                    ComboItem item = new ComboItem();
                    item.ThemeName = themeName;
                    item.ThemeType = "XML";
                    comboTheme.Items.Add(item);

                }
            }
        }

        private ServerMessageFormatItem NewMessageFormat(string messageName, string message)
        {
            ServerMessageFormatItem m = new ServerMessageFormatItem();
            m.MessageName = messageName;
            m.FormattedMessage = message;
            return m;
        }

        private void buttonImportTheme_Click(object sender, EventArgs e)
        {
            //this will load an icechat 7 theme
            //do it the same way as icechat 7 did, from the clipboard

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".ini";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.AddExtension = true;
            ofd.AutoUpgradeEnabled = true;
            ofd.Filter = "IceChat INI Setting (*.ini)|*.ini";
            ofd.Title = "Locate the IceChat.ini settings file?";

            string directory = Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + "IceChat";

            //check if the folder exists
            if (File.Exists(directory + Path.DirectorySeparatorChar + "icechat.ini"))
                ofd.InitialDirectory = directory;
            else
                ofd.InitialDirectory = Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //make sure it is icechat.ini                
                System.Diagnostics.Debug.WriteLine(ofd.FileName);

                if (Path.GetFileName(ofd.FileName).ToLower().Equals("icechat.ini"))
                {
                    IniParser parser = new IniParser(ofd.FileName);

                    string[] themes = parser.EnumSectionTheme("Color Themes");
                    foreach (string theme in themes)
                    {
                        string name = theme.Substring(0, theme.IndexOf((char)255));
                        string value = theme.Substring(theme.IndexOf((char)255) + 1);
                        string[] colors = value.Split(',');

                        if (name == "Default")
                            name = "DefaultIce7";

                        IceChatColors ic = new IceChatColors();

                        ic.ChannelAdminColor = Convert.ToInt32(colors[38]);
                        ic.ChannelBackColor = Convert.ToInt32(colors[43]);
                        ic.ChannelListBackColor = Convert.ToInt32(colors[63]);
                        ic.ChannelListForeColor = Convert.ToInt32(colors[86]);
                        ic.ChannelOwnerColor = Convert.ToInt32(colors[39]);
                        ic.ChannelRegularColor = Convert.ToInt32(colors[34]);
                        ic.ChannelVoiceColor = Convert.ToInt32(colors[35]);
                        ic.ChannelHalfOpColor = Convert.ToInt32(colors[36]);
                        ic.ChannelOpColor = Convert.ToInt32(colors[37]);
                        ic.ConsoleBackColor = Convert.ToInt32(colors[42]);
                        ic.InputboxBackColor = Convert.ToInt32(colors[52]);
                        ic.InputboxForeColor = Convert.ToInt32(colors[25]);
                        ic.NickListBackColor = Convert.ToInt32(colors[53]);
                        ic.PanelHeaderBG1 = Convert.ToInt32(colors[70]);
                        ic.PanelHeaderBG2 = Convert.ToInt32(colors[71]);
                        ic.PanelHeaderForeColor = Convert.ToInt32(colors[88]);
                        ic.QueryBackColor = Convert.ToInt32(colors[44]);
                        ic.RandomizeNickColors = false;
                        ic.ServerListBackColor = Convert.ToInt32(colors[54]);
                        ic.StatusbarBackColor = Convert.ToInt32(colors[90]);
                        ic.StatusbarForeColor = Convert.ToInt32(colors[89]);
                        ic.TabBarChannelJoin = Convert.ToInt32(colors[30]);
                        ic.TabBarChannelPart = Convert.ToInt32(colors[31]);
                        ic.TabBarCurrent = Convert.ToInt32(colors[28]);
                        ic.TabBarDefault = Convert.ToInt32(colors[28]);
                        ic.TabBarNewMessage = Convert.ToInt32(colors[29]);
                        ic.TabBarOtherMessage = Convert.ToInt32(colors[32]);
                        ic.TabBarServerMessage = Convert.ToInt32(colors[32]);
                        ic.TabBarServerQuit = Convert.ToInt32(colors[32]);
                        ic.TabBarNewAction = Convert.ToInt32(colors[29]);
                        ic.TabBarServerNotice = Convert.ToInt32(colors[32]);
                        ic.ToolbarBackColor = Convert.ToInt32(colors[68]);
                        ic.UnreadTextMarkerColor = Convert.ToInt32(colors[67]);

                        XmlSerializer serializerC = new XmlSerializer(typeof(IceChatColors));
                        string themeFile = FormMain.Instance.CurrentFolder + System.IO.Path.DirectorySeparatorChar + "Colors-" + name + ".xml";
                        TextWriter textWriterC = new StreamWriter(themeFile);
                        serializerC.Serialize(textWriterC, ic);
                        textWriterC.Close();
                        textWriterC.Dispose();

                        IceChatMessageFormat im = new IceChatMessageFormat();
                        im.MessageSettings = new ServerMessageFormatItem[49];

                        im.MessageSettings[0] = NewMessageFormat("Server Connect", "&#x3;" + colors[18] + "*** Attempting to connect to $server ($serverip) on port $port");
                        im.MessageSettings[1] = NewMessageFormat("Server Disconnect", "&#x3;" + colors[19] + "*** Server disconnected on $server");
                        im.MessageSettings[2] = NewMessageFormat("Server Reconnect", "&#x3;" + colors[11] + "*** Attempting to re-connect to $server");
                        im.MessageSettings[3] = NewMessageFormat("Channel Invite", "&#x3;" + colors[22] + "* $nick invites you to $channel");
                        im.MessageSettings[4] = NewMessageFormat("Ctcp Reply", "&#x3;" + colors[27] + "[$nick $ctcp Reply] : $reply");
                        im.MessageSettings[5] = NewMessageFormat("Ctcp Send", "&#x3;" + colors[26] + "--> [$nick] $ctcp");
                        im.MessageSettings[6] = NewMessageFormat("Ctcp Request", "&#x3;" + colors[11] + "[$nick] $ctcp");
                        im.MessageSettings[7] = NewMessageFormat("Channel Mode", "&#x3;" + colors[7] + "* $nick sets mode $mode $modeparam for $channel");
                        im.MessageSettings[8] = NewMessageFormat("Server Mode", "&#x3;" + colors[16] + "* Your mode is now $mode");
                        im.MessageSettings[9] = NewMessageFormat("Server Notice", "&#x3;" + colors[24] + "*** $server $message");
                        im.MessageSettings[10] = NewMessageFormat("Server Message", "&#x3;" + colors[11] + "-$server- $message");
                        im.MessageSettings[11] = NewMessageFormat("User Notice", "&#x3;" + colors[3] + "--$nick-- $message");
                        im.MessageSettings[12] = NewMessageFormat("Channel Message", "&#x3;" + colors[0] + "<$color$status$nick&#x3;> $message");
                        im.MessageSettings[13] = NewMessageFormat("Self Channel Message", "&#x3;" + colors[2] + "<$nick&#x3;> $message");
                        im.MessageSettings[14] = NewMessageFormat("Channel Action", "&#x3;" + colors[1] + "* $nick $message");
                        im.MessageSettings[15] = NewMessageFormat("Self Channel Action", "&#x3;" + colors[2] + "* $nick $message");
                        im.MessageSettings[16] = NewMessageFormat("Channel Join", "&#x3;" + colors[5] + "* $nick ($host)$account has joined channel $channel");
                        im.MessageSettings[17] = NewMessageFormat("Self Channel Join", "&#x3;" + colors[5] + "* You have joined $channel");
                        im.MessageSettings[18] = NewMessageFormat("Channel Part", "&#x3;" + colors[4] + "* $nick ($host) has left $channel $reason");
                        im.MessageSettings[19] = NewMessageFormat("Self Channel Part", "&#x3;" + colors[4] + "* You have left $channel - You will be missed &#x3;10 $reason");
                        im.MessageSettings[20] = NewMessageFormat("Server Quit", "&#x3;" + colors[21] + "* $nick ($host) Quit ($reason)");
                        im.MessageSettings[21] = NewMessageFormat("Channel Nick Change", "&#x3;" + colors[20] + "* $nick is now known as $newnick");
                        im.MessageSettings[22] = NewMessageFormat("Self Nick Change", "&#x3;" + colors[20] + "* You are now known as $newnick");
                        im.MessageSettings[23] = NewMessageFormat("Channel Kick", "&#x3;" + colors[6] + "* $kickee was kicked by $nick($host) &#x3;3 - Reason ($reason)");
                        im.MessageSettings[24] = NewMessageFormat("Self Channel Kick", "&#x3;" + colors[6] + "* You were kicked from $channel by $kicker (&#x3;3$reason)");
                        im.MessageSettings[25] = NewMessageFormat("Private Message", "&#x3;" + colors[0] + "<$nick> $message");
                        im.MessageSettings[26] = NewMessageFormat("Self Private Message", "&#x3;" + colors[2] + "<$nick>&#x3;1 $message");
                        im.MessageSettings[27] = NewMessageFormat("Private Action", "&#x3;" + colors[1] + "* $nick $message");
                        im.MessageSettings[28] = NewMessageFormat("Self Private Action", "&#x3;" + colors[1] + "* $nick $message");
                        im.MessageSettings[29] = NewMessageFormat("DCC Chat Action", "&#x3;" + colors[1] + "* $nick $message");
                        im.MessageSettings[30] = NewMessageFormat("Self DCC Chat Action", "&#x3;" + colors[1] + "* $nick $message");
                        im.MessageSettings[31] = NewMessageFormat("DCC Chat Message", "&#x3;" + colors[0] + "<$nick> $message");
                        im.MessageSettings[32] = NewMessageFormat("Self DCC Chat Message", "&#x3;" + colors[2] + "<$nick> $message");
                        im.MessageSettings[33] = NewMessageFormat("DCC Chat Request", "&#x3;" + colors[11] + "* $nick ($host) is requesting a DCC Chat");
                        im.MessageSettings[34] = NewMessageFormat("DCC File Send", "&#x3;" + colors[11] + "* $nick ($host) is trying to send you a file ($file) [$filesize bytes]");
                        im.MessageSettings[35] = NewMessageFormat("Channel Topic Change", "&#x3;" + colors[8] + "* $nick changes topic to: $topic");
                        im.MessageSettings[36] = NewMessageFormat("Channel Topic Text", "&#x3;" + colors[8] + " Topic: $topic");
                        im.MessageSettings[37] = NewMessageFormat("Server MOTD", "&#x3;" + colors[10] + "$message");
                        im.MessageSettings[38] = NewMessageFormat("Channel Notice", "&#x3;" + colors[3] + "-$nick:$status$channel- $message");
                        im.MessageSettings[39] = NewMessageFormat("Channel Other", "&#x3;" + colors[9] + "$message");
                        im.MessageSettings[40] = NewMessageFormat("User Echo", "&#x3;" + colors[15] + "$message");
                        im.MessageSettings[41] = NewMessageFormat("Server Error", "&#x3;" + colors[17] + "ERROR: $message");
                        im.MessageSettings[42] = NewMessageFormat("User Whois", "&#x3;" + colors[14] + "->> $nick $data");
                        im.MessageSettings[43] = NewMessageFormat("User Error", "&#x3;" + colors[12] + "ERROR: $message");
                        im.MessageSettings[44] = NewMessageFormat("DCC Chat Connect", "&#x3;" + colors[18] + "* DCC Chat Connection Established with $nick");
                        im.MessageSettings[45] = NewMessageFormat("DCC Chat Disconnect", "&#x3;" + colors[19] + "* DCC Chat Disconnected from $nick");
                        im.MessageSettings[46] = NewMessageFormat("DCC Chat Outgoing", "&#x3;" + colors[11] + "* DCC Chat Requested with $nick");
                        im.MessageSettings[47] = NewMessageFormat("DCC Chat Timeout", "&#x3;" + colors[11] + "* DCC Chat with $nick timed out");
                        im.MessageSettings[48] = NewMessageFormat("Self Notice", "&#x3;" + colors[24] + "--> $nick - $message");

                        
                        //check if we have this theme already...
                        bool themeFound = false;
                        foreach (ComboItem checkTheme in comboTheme.Items)
                        {
                            if (checkTheme.ThemeName.ToLower().Equals(name.ToLower()))
                                themeFound = true;
                        }

                        if (themeFound == false)
                        {
                            XmlSerializer serializerIM = new XmlSerializer(typeof(IceChatMessageFormat));
                            string messFile = FormMain.Instance.CurrentFolder + System.IO.Path.DirectorySeparatorChar + "Messages-" + name + ".xml";
                            TextWriter textWriterIM = new StreamWriter(messFile);
                            serializerIM.Serialize(textWriterIM, im);
                            textWriterIM.Close();
                            textWriterIM.Dispose();

                            ComboItem item = new ComboItem();
                            item.ThemeName = name;
                            item.ThemeType = "XML";
                            comboTheme.Items.Add(item);
                        }
                    }
                
                }

            }            
        }


    }
}
