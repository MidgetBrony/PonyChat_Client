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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

using IceChatPlugin;

namespace IceChat
{
    public partial class FormEditor : Form
    {
        private IceChatAliases aliasList;
        private IceChatPopupMenus popupList;

        private string[] nickListPopup;
        private string[] consolePopup;
        private string[] channelPopup;
        private string[] queryPopup;

        private string currentPopup;
        private ToolStripMenuItem currentPopupMenu;

        public FormEditor()
        {            
            InitializeComponent();
            this.Load += new EventHandler(FormEditor_Load);
            popupTypeToolStripMenuItem.Visible = false;

            tabControlEditor.SelectedIndexChanged += new EventHandler(tabControlEditor_SelectedIndexChanged);
            textAliases.KeyDown += new KeyEventHandler(OnKeyDown);
            textPopups.KeyDown += new KeyEventHandler(OnKeyDown);

            //load the aliases
            aliasList = FormMain.Instance.IceChatAliases;
            LoadAliases();

            //load the popups
            popupList = FormMain.Instance.IceChatPopupMenus;
            nickListPopup = LoadPopupMenu("NickList");
            consolePopup = LoadPopupMenu("Console");
            channelPopup = LoadPopupMenu("Channel");
            queryPopup = LoadPopupMenu("Query");

            //load the nicklist by default into popup editor
            LoadPopups(nickListPopup);
            nickListToolStripMenuItem.Checked = true;
            currentPopup = "NickList";
            currentPopupMenu = nickListToolStripMenuItem;

            this.Activated += new EventHandler(FormEditor_Activated);

            ApplyLanguage();
        }

        private void FormEditor_Load(object sender, EventArgs e)
        {
            if (this.Owner != null)
                this.Location = new Point(this.Owner.Location.X + this.Owner.Width / 2 - this.Width / 2,
                    this.Owner.Location.Y + this.Owner.Height / 2 - this.Height / 2);
        }

        private void FormEditor_Activated(object sender, EventArgs e)
        {
            //load any plugin addons
            foreach (IPluginIceChat ipc in FormMain.Instance.IceChatPlugins)
            {
                if (ipc.Enabled == true)
                    ipc.LoadEditorForm(this.tabControlEditor, this.menuStripMain);
            }

            this.Activated -= FormEditor_Activated;
        }


        private void ApplyLanguage()
        {

        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.K)
                {
                    //((TextBox)sender).SelectedText = ((char)3).ToString();
                    ((TextBox)sender).SelectedText = @"\%C";
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                if (e.KeyCode == Keys.B)
                {
                    //((TextBox)sender).SelectedText = ((char)2).ToString();
                    ((TextBox)sender).SelectedText = @"\%B";
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                if (e.KeyCode == Keys.R)
                {
                    //((TextBox)sender).SelectedText = ((char)2).ToString();
                    ((TextBox)sender).SelectedText = @"\%R";
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                if (e.KeyCode == Keys.O)
                {
                    //((TextBox)sender).SelectedText = ((char)2).ToString();
                    ((TextBox)sender).SelectedText = @"\%O";
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                if (e.KeyCode == Keys.U)
                {
                    ((TextBox)sender).SelectedText = @"\%U"; //31 (1F) normally //219    //db
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                
                if (e.KeyCode == Keys.I)
                {
                    //((TextBox)sender).SelectedText = ((char)206).ToString();     //29 1D for normal 29 //ce
                    ((TextBox)sender).SelectedText = @"\%I";     //29 1D for normal 29 //ce
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                 
                //219 - DB
                //206 - CE
            }
        }


        private void tabControlEditor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlEditor.SelectedTab.Text != "PopupMenus")
            {
                popupTypeToolStripMenuItem.Visible = false;
            }
            else
                popupTypeToolStripMenuItem.Visible = true;

        }

        private void ReLoadAliases()
        {
            if (File.Exists(FormMain.Instance.AliasesFile))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(IceChatAliases));
                TextReader textReader = new StreamReader(FormMain.Instance.AliasesFile);
                aliasList = (IceChatAliases)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
            }
            else
            {
                aliasList = new IceChatAliases();
            }
        }

        private void LoadAliases()
        {
            textAliases.Clear();
            
            //reload the aliases from the actual file
            ReLoadAliases();

            foreach (AliasItem alias in aliasList.listAliases)
            {
                if (alias.Command.Length == 1)
                    textAliases.AppendText(alias.AliasName + " " + alias.Command[0].Replace("&#x3;", ((char)3).ToString()).Replace("&#x2;", ((char)2).ToString()).Replace("&#x1F;", ((char)219).ToString()).Replace("&#x1D;", ((char)206).ToString()) + Environment.NewLine);
                else
                {
                    //multiline alias
                    textAliases.AppendText(alias.AliasName + " {" + Environment.NewLine);
                    foreach (string command in alias.Command)
                    {
                        textAliases.AppendText(command + Environment.NewLine);
                    }
                    textAliases.AppendText("}" + Environment.NewLine);
                }
            }
        }

        private void LoadPopups(string[] menu)
        {
            textPopups.Clear();
            
            if (menu == null) return;
            
            foreach (string m in menu)
            {
                textPopups.AppendText(m.Replace("&#x3;", ((char)3).ToString()).Replace("&#x2;", ((char)2).ToString()).Replace("&#x1F;", ((char)219).ToString()).Replace("&#x1D;", ((char)206).ToString()) + Environment.NewLine);
            }

            textPopups.SelectionStart = 0;
            textPopups.SelectionLength = 0;

        }

        private void ReLoadPopups()
        {
            if (File.Exists(FormMain.Instance.PopupsFile))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(IceChatPopupMenus));
                TextReader textReader = new StreamReader(FormMain.Instance.PopupsFile);
                popupList = (IceChatPopupMenus)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
            }
            else
                popupList = new IceChatPopupMenus();

        }

        private string[] LoadPopupMenu(string popupType)
        {
            //reload the popupmenu's file
            ReLoadPopups();

            foreach (PopupMenuItem p in popupList.listPopups)
            {
                if (p.PopupType == popupType)
                    return p.Menu;
            }
            return null;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            this.saveToolStripMenuItem.PerformClick();
        }

        private void closeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
		
        private void UpdateCurrentPopupMenus()
        {
            try
            {
                string[] popups = textPopups.Text.Replace(((char)3).ToString(), "&#x3;").Replace(((char)2).ToString(), "&#x2;").Replace(((char)219).ToString(), "&#x1F;").Replace(((char)206).ToString(), "&#x1D;").Trim().Split(new String[] { Environment.NewLine }, StringSplitOptions.None);

                if (currentPopup == "NickList")
                    nickListPopup = popups;
                if (currentPopup == "Console")
                    consolePopup = popups;
                if (currentPopup == "Channel")
                    channelPopup = popups;
                if (currentPopup == "Query")
                    queryPopup = popups;


                PopupMenuItem p = new PopupMenuItem();
                p.PopupType = currentPopup;
                p.Menu = popups;
                popupList.ReplacePopup(p.PopupType, p);

                FormMain.Instance.IceChatPopupMenus = popupList;

                currentPopupMenu.Checked = false;
            }
            catch (Exception ex)
            {
                FormMain.Instance.WindowMessage(FormMain.Instance.InputPanel.CurrentConnection, "Console", "UpdatePopupMenus Error:" + ex.Message + ":" + ex.Source, 4, true);
            }

        }
        private void nickListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //save the current popup
            if (currentPopup == "NickList") return;

            UpdateCurrentPopupMenus();
            
            currentPopupMenu = nickListToolStripMenuItem;
            currentPopup = "NickList";
            currentPopupMenu.Checked = true;
            LoadPopups(nickListPopup);
        }

        private void consoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentPopup == "Console") return;

            UpdateCurrentPopupMenus();

            currentPopupMenu = consoleToolStripMenuItem;
            currentPopup = "Console";
            currentPopupMenu.Checked = true;
            LoadPopups(consolePopup);
        }

        private void channelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentPopup == "Channel") return;

            UpdateCurrentPopupMenus();

            currentPopupMenu = channelToolStripMenuItem;
            currentPopup = "Channel";
            currentPopupMenu.Checked = true;
            LoadPopups(channelPopup);
        }

        private void queryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentPopup == "Query") return;

            UpdateCurrentPopupMenus();

            currentPopupMenu = queryToolStripMenuItem;
            currentPopup = "Query";
            currentPopupMenu.Checked = true;
            LoadPopups(queryPopup);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //save all the settings
            try
            {
                //parse out all the aliases
                textAliases.Text = textAliases.Text.Replace(((char)3).ToString(), "&#x3;").Replace(((char)2).ToString(), "&#x2;").Replace(((char)219).ToString(), "&#x1F;").Replace(((char)206).ToString(), "&#x1D;");

                aliasList.listAliases.Clear();

                string[] aliases = textAliases.Text.Trim().Split(new String[] { Environment.NewLine }, StringSplitOptions.None);
                bool isMultiLine = false;
                AliasItem multiLineAlias = null;
                string aliasCommands = "";

                foreach (string alias in aliases)
                {
                    if (alias.Length > 0)
                    {
                        //check if it is a multilined alias
                        if (alias.EndsWith("{") && !isMultiLine)
                        {
                            //start of a multilined alias
                            isMultiLine = true;
                            multiLineAlias = new AliasItem();
                            multiLineAlias.AliasName = alias.Substring(0, alias.IndexOf(' '));
                            aliasCommands = "";
                        }
                        else if (alias == "}")
                        {
                            //end of multiline alias
                            isMultiLine = false;
                            multiLineAlias.Command = aliasCommands.Split(new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                            aliasList.AddAlias(multiLineAlias);
                            multiLineAlias = null;
                        }
                        else if (!isMultiLine)
                        {
                            //just a normal alias
                            AliasItem a = new AliasItem();
                            a.AliasName = alias.Substring(0, alias.IndexOf(' '));
                            a.Command = new String[] { alias.Substring(alias.IndexOf(' ') + 1) };
                            aliasList.AddAlias(a);
                            a = null;
                        }
                        else
                        {
                            //add a line to the multiline alias
                            aliasCommands += alias + Environment.NewLine;
                        }
                    }
                }

                FormMain.Instance.IceChatAliases = aliasList;

                //save the current popup menu
                UpdateCurrentPopupMenus();

                //save any plugin addons for the Script Editor
                foreach (IPluginIceChat ipc in FormMain.Instance.IceChatPlugins)
                {
                    if (ipc.Enabled)
                        ipc.SaveEditorForm();
                }
            }
            catch (Exception ex)
            {
                FormMain.Instance.WindowMessage(FormMain.Instance.InputPanel.CurrentConnection, "Console", "SaveEditor Error:" + ex.Message + ":" + ex.Source, 4, true);
            }
            finally
            {
                this.Close();
            }
        }

    }
}
