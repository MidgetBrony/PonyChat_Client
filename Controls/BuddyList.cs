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
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace IceChat
{
    public partial class BuddyList : UserControl
    {
        private string headerCaption = "Buddy List";
        private int headerHeight = 23;

        private delegate void UpdateBuddyListDelegate(IRCConnection connection, BuddyListItem buddy);
        private delegate void ClearBuddyListDelegate(IRCConnection connection);


        public BuddyList()
        {
            InitializeComponent();

            this.Paint += new PaintEventHandler(OnHeaderPaint);
            //this.DoubleClick += new EventHandler(OnDoubleClick);
            this.panelButtons.Resize += new EventHandler(panelButtons_Resize);
            this.Resize += new EventHandler(OnResize);
            this.MouseDown += new MouseEventHandler(OnMouseDown);

            this.treeBuddies.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(treeBuddies_NodeMouseDoubleClick);
            this.treeBuddies.ShowNodeToolTips = true;

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
            
        }

        private void treeBuddies_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            FormMain.Instance.ParseOutGoingCommand((IRCConnection)e.Node.Tag, "/query " + e.Node.Text);            
        }

        private void OnResize(object sender, EventArgs e)
        {
            treeBuddies.Height = this.Height - (treeBuddies.Top);
            treeBuddies.Width = this.Width;
        }

        private void OnDoubleClick(object sender, EventArgs e)
        {
            /*
            if (this.Parent.Parent.GetType() == typeof(TabPage))
            {
                if (this.Parent.Parent.GetType() != typeof(FormFloat))
                    FormMain.Instance.UnDockPanel((Panel)this.Parent);
                return;
            }
            */ 
        }

        internal void ApplyLanguage()
        {
            IceChatLanguage iceChatLanguage = FormMain.Instance.IceChatLanguage;
            headerCaption = iceChatLanguage.buddyListHeader;
            buttonAdd.Text = iceChatLanguage.favChanbuttonAdd;
            buttonMessage.Text = iceChatLanguage.buddyListbuttonMessage;
            buttonEdit.Text = iceChatLanguage.favChanbuttonEdit;
            buttonRemove.Text = iceChatLanguage.favChanbuttonRemove;
        }

        private void panelButtons_Resize(object sender, EventArgs e)
        {
            this.buttonAdd.Width = (panelButtons.Width / 2) - 4;
            this.buttonMessage.Width = buttonAdd.Width;
            this.buttonEdit.Width = buttonAdd.Width;
            this.buttonRemove.Width = buttonAdd.Width;
            this.buttonMessage.Left = (panelButtons.Width / 2) + 1;
            this.buttonRemove.Left = buttonMessage.Left;
        }

        internal void SetListColors()
        {
            this.treeBuddies.BackColor = IrcColor.colors[FormMain.Instance.IceChatColors.ChannelListBackColor];
            this.treeBuddies.ForeColor = IrcColor.colors[FormMain.Instance.IceChatColors.ChannelListForeColor];
        }

        /// <summary>
        /// Paint the header with a Gradient Background
        /// </summary>
        private void OnHeaderPaint(object sender, PaintEventArgs e)
        {
            Bitmap buffer = new Bitmap(this.Width, this.Height, e.Graphics);
            Graphics g = Graphics.FromImage(buffer);

            //draw the header
            Rectangle headerR = new Rectangle(0, 0, this.Width, headerHeight);
            Brush l = new LinearGradientBrush(headerR, IrcColor.colors[FormMain.Instance.IceChatColors.PanelHeaderBG1], IrcColor.colors[FormMain.Instance.IceChatColors.PanelHeaderBG2], 300);

            g.FillRectangle(l, headerR);
            // http://www.scip.be/index.php?Page=ArticlesNET01&Lang=EN

            //System.Diagnostics.Debug.WriteLine("p:" + this.Parent.GetType());
            //System.Diagnostics.Debug.WriteLine("pp:" + this.Parent.Parent.GetType());
            
            if (this.Parent.Parent.GetType() != typeof(FormFloat))
            {
                if (Application.RenderWithVisualStyles)
                {
                    if (System.Windows.Forms.VisualStyles.VisualStyleRenderer.IsElementDefined(System.Windows.Forms.VisualStyles.VisualStyleElement.ExplorerBar.NormalGroupCollapse.Normal))
                    {
                        System.Windows.Forms.VisualStyles.VisualStyleRenderer renderer = new System.Windows.Forms.VisualStyles.VisualStyleRenderer(System.Windows.Forms.VisualStyles.VisualStyleElement.ExplorerBar.NormalGroupCollapse.Normal);
                        //which side are we docked on
                        Rectangle rect = Rectangle.Empty;
                        if (((IceDockPanel)this.Parent.Parent.Parent.Parent).Dock == DockStyle.Right)
                            rect = new Rectangle(0, 0, 22, 22);
                        else
                            rect = new Rectangle(this.Width - 22, 0, 22, 22);
                        renderer.DrawBackground(g, rect);
                    }
                }
            }
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            
            Font headerFont = new Font("Verdana", 10);

            Rectangle centered = headerR;
            centered.Offset(0, (int)(headerR.Height - g.MeasureString(headerCaption, headerFont).Height) / 2);
            
            g.DrawString(headerCaption, headerFont, new SolidBrush(Color.Black), centered, sf);

            e.Graphics.DrawImageUnscaled(buffer, 0, 0);
            buffer.Dispose();
            g.Dispose();
            l.Dispose();
            headerFont.Dispose();                    

        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (this.Parent.Parent.GetType() != typeof(FormFloat))
            {
                if (e.Y <= headerHeight)
                {
                    //which side are we docked on
                    if (((IceDockPanel)this.Parent.Parent.Parent.Parent).Dock == DockStyle.Right && e.X < 22)
                    {
                        ((IceDockPanel)this.Parent.Parent.Parent.Parent).DockControl();
                        return;
                    }
                    else if (((IceDockPanel)this.Parent.Parent.Parent.Parent).Dock == DockStyle.Left && e.X > (this.Width - 22))
                    {
                        ((IceDockPanel)this.Parent.Parent.Parent.Parent).DockControl();
                        return;
                    }
                }
            }
        }
       
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Edit Buddy List in Server Editor");
        }

       

        private void buttonMessage_Click(object sender, EventArgs e)
        {
            //
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Edit Buddy List in Server Editor");
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Edit Buddy List in Server Editor");
        }

        internal void ClearBuddyList(IRCConnection connection)
        {
            if (this.InvokeRequired)
            {
                ClearBuddyListDelegate cbl = new ClearBuddyListDelegate(ClearBuddyList);
                this.Invoke(cbl, new object[] { connection });
            }
            else
            {
                //remove all items in the buddy list with this connection
                for (int i = treeBuddies.Nodes[1].Nodes.Count; i > 0; i--)
                {
                    if (treeBuddies.Nodes[1].Nodes[i-1].Tag == connection)
                        treeBuddies.Nodes[1].Nodes[i-1].Remove();
                }
                for (int i = treeBuddies.Nodes[0].Nodes.Count; i > 0; i--)
                {
                    if (treeBuddies.Nodes[0].Nodes[i-1].Tag == connection)
                        treeBuddies.Nodes[0].Nodes[i-1].Remove();
                }
                    
            }
        }

        internal void UpdateBuddy(IRCConnection connection,  BuddyListItem buddy)
        {
            if (this.InvokeRequired)
            {
                UpdateBuddyListDelegate ubl = new UpdateBuddyListDelegate(UpdateBuddy);
                this.Invoke(ubl, new object[] { connection, buddy });
            }
            else
            {
                TreeNode t = new TreeNode();
                t.Text = buddy.Nick;
                t.Tag = connection;

                if (buddy.Connected)
                {
                    t.ToolTipText = buddy.Nick + " on " + connection.ServerSetting.RealServerName;
                    this.treeBuddies.Nodes[1].Nodes.Add(t);
                    if (buddy.PreviousState == false)
                        FormMain.Instance.PlaySoundFile("buddy");
                    buddy.PreviousState = true;
                }
                else if (!buddy.Nick.StartsWith(";"))
                {
                    t.ToolTipText = buddy.Nick + " not on " + connection.ServerSetting.RealServerName;
                    this.treeBuddies.Nodes[0].Nodes.Add(t);
                    buddy.PreviousState = false;
                }
                
                this.treeBuddies.ExpandAll();
            }
        }
    }
}
