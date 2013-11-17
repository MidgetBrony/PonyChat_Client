﻿/***********************************\
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

using IceChatPlugin;

namespace IceChat
{
    public partial class ChannelBar : UserControl
    {
        private Font _tabFont = new Font("Verdana", 10F);
        private Dictionary<int, Rectangle> _tabSizeRects = new Dictionary<int, Rectangle>();
        private Dictionary<int, Rectangle> _tabTextRects = new Dictionary<int, Rectangle>();

        private List<IceTabPage> _TabPages = new List<IceTabPage>();

        private int _selectedIndex = -1;
        private int _hoveredIndex = -1;

        private int _TabRowHeight = 30;
        private int _TotalTabRows = 1;

        private int _tabStartXPos = 0;

        private Point _dragStartPosition = Point.Empty;

        //which tab will be dragged
        private IceTabPage _dragTab;
        private ContextMenuStrip _popupMenu;

        //private bool showTabs;

        private System.Timers.Timer flashTabTimer;

        internal delegate void TabEventHandler(object sender, TabEventArgs e);
        internal event TabEventHandler SelectedIndexChanged;

        internal delegate void TabClosedDelegate(int nIndex);
        internal event TabClosedDelegate OnTabClosed;

        public ChannelBar()
        {
            InitializeComponent();

            this.MouseDown += new MouseEventHandler(OnMouseDown);
            this.MouseMove += new MouseEventHandler(OnMouseMove);
            this.MouseLeave += new EventHandler(OnMouseLeave);
            this.MouseUp += new MouseEventHandler(OnMouseUp);

            this.AutoSize = false;

            this.DoubleBuffered = true;

            this.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            _popupMenu = ConsolePopupMenu();
            _popupMenu.ItemClicked += new ToolStripItemClickedEventHandler(OnPopupMenu_ItemClicked);

            flashTabTimer = new System.Timers.Timer();
            flashTabTimer.Interval = 1000;
            flashTabTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnFlashTabTimerElapsed);

        }

        internal int TabCount
        {
            get
            {
                return this._TabPages.Count;
            }
        }

        internal Font TabFont
        {
            set
            {
                this._tabFont = value;
                this.Invalidate();
            }
        }

        public int SelectedIndex
        {
            set
            {
                this._selectedIndex = value;

                if (this.SelectedIndexChanged != null)
                {
                    TabEventArgs e = new TabEventArgs();
                    e.IsHandled = true;
                    SelectedIndexChanged(this, e);
                }
            }
            get
            {
                return this._selectedIndex;
            }
        }

        internal IceTabPage CurrentTab
        {
            get
            {
                if (_selectedIndex == -1) _selectedIndex = 0;
                if (_selectedIndex > (_TabPages.Count - 1)) _selectedIndex = 0;
                return _TabPages[_selectedIndex];
            }
        }

        internal List<IceTabPage> TabPages
        {
            get
            {
                return this._TabPages;
            }
        }

        internal void AddTabPage(ref IceTabPage page)
        {
            this._TabPages.Add(page);
            //set the tab as active

        }
        internal void InsertTabPage(int index, ref IceTabPage page)
        {
            this._TabPages.Insert(index, page);
        }
        
        private IceTabPage HoverTab(Point pClickLocation)
        {
            for (int i = 0; i < _tabSizeRects.Count; i++)
            {
                Rectangle rectTab = _tabSizeRects[i];
                if ((pClickLocation.X > rectTab.X && pClickLocation.X < rectTab.X + rectTab.Width) && (pClickLocation.Y > rectTab.Y && pClickLocation.Y < rectTab.Bottom))
                    return GetTabPage(i);
            }
            return null;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (FormMain.Instance != null)
                DrawControl(e.Graphics);
        }


        internal void DrawControl(Graphics g)
        {
            try
            {
                //draw the background
                g.Clear(IrcColor.colors[FormMain.Instance.IceChatColors.TabbarBackColor]);

                if (this._TabPages.Count == 0) return;

                if (this._TabPages.Count != 0 && _selectedIndex == -1)
                    SelectedIndex = 0;

                if (this._selectedIndex > (_TabPages.Count - 1))
                    SelectedIndex = 0;

                CalculateTabSizes(g);

                bool flashTabs = false;

                //check if we have any flashing tabs
                for (int i = 0; i < _TabPages.Count; i++)
                {
                    if (_TabPages[i].FlashTab == true)
                    {
                        flashTabs = true;
                        break;
                    }
                }

                if (flashTabs == true)
                {
                    //enable the flash tab timer
                    flashTabTimer.Enabled = true;
                    flashTabTimer.Start();
                }
                else
                {
                    //disable the flash tab timer
                    flashTabTimer.Stop();
                    flashTabTimer.Enabled = false;
                }

                for (int i = 0; i < _TabPages.Count; i++)
                    DrawTab(g, _TabPages[i], i);

            }
            catch (Exception)
            {
                //System.Diagnostics.Debug.WriteLine("IceChatControl DrawControl Error:" + e.Message);
                //FormMain.Instance.WriteErrorFile(FormMain.Instance.InputPanel.CurrentConnection, "IceTabControl DrawControl", e);
            }
        }

        internal void DrawTab(Graphics g, IceTabPage tabPage, int nIndex)
        {
            try
            {
                Rectangle recBounds = _tabSizeRects[nIndex];
                Rectangle tabTextArea = _tabTextRects[nIndex];

                Brush br;
                Point[] pt;

                bool bSelected = (this._selectedIndex == nIndex);
                bool bHovered = (this._hoveredIndex == nIndex);

                if (bSelected)
                    br = new LinearGradientBrush(recBounds, IrcColor.colors[FormMain.Instance.IceChatColors.TabBarCurrentBG1], IrcColor.colors[FormMain.Instance.IceChatColors.TabBarCurrentBG2], 90);
                else if (bHovered)
                    br = new LinearGradientBrush(recBounds, IrcColor.colors[FormMain.Instance.IceChatColors.TabBarHoverBG1], IrcColor.colors[FormMain.Instance.IceChatColors.TabBarHoverBG2], 90);
                else
                    br = new LinearGradientBrush(recBounds, IrcColor.colors[FormMain.Instance.IceChatColors.TabBarOtherBG1], IrcColor.colors[FormMain.Instance.IceChatColors.TabBarOtherBG2], 90);

                pt = new Point[7];
                pt[0] = new Point(recBounds.Left + 1, recBounds.Bottom);
                if (bSelected)
                {
                    pt[1] = new Point(recBounds.Left + 1, recBounds.Top + 3);
                    pt[2] = new Point(recBounds.Left + 4, recBounds.Top);
                    pt[3] = new Point(recBounds.Right - 4, recBounds.Top);
                    pt[4] = new Point(recBounds.Right - 1, recBounds.Top + 3);
                }
                else
                {
                    pt[1] = new Point(recBounds.Left + 1, recBounds.Top + 6);
                    pt[2] = new Point(recBounds.Left + 4, recBounds.Top + 3);
                    pt[3] = new Point(recBounds.Right - 4, recBounds.Top + 3);
                    pt[4] = new Point(recBounds.Right - 1, recBounds.Top + 6);
                }
                pt[5] = new Point(recBounds.Right - 1, recBounds.Bottom);
                pt[6] = new Point(recBounds.Left + 1, recBounds.Bottom);


                g.FillPolygon(br, pt);
                // draw the border around the control
                g.DrawPolygon(new Pen(Color.Black, 1), pt);

                br.Dispose();
                Image img = null;

                switch (tabPage.WindowStyle)
                {
                    case IceTabPage.WindowType.Console:
                        img = StaticMethods.LoadResourceImage("console.png");
                        break;
                    case IceTabPage.WindowType.Channel:
                        img = StaticMethods.LoadResourceImage("channel.png");
                        break;
                    case IceTabPage.WindowType.Query:
                    case IceTabPage.WindowType.DCCChat:
                        img = StaticMethods.LoadResourceImage("new-query.ico");
                        break;
                    case IceTabPage.WindowType.ChannelList:
                        img = StaticMethods.LoadResourceImage("channellist.png");
                        break;
                    case IceTabPage.WindowType.Window:
                    case IceTabPage.WindowType.Debug:
                        img = StaticMethods.LoadResourceImage("window-icon.ico");
                        break;
                    default:
                        img = StaticMethods.LoadResourceImage("window-icon.ico");
                        break;

                }

                Rectangle rimage = new Rectangle(recBounds.X, recBounds.Y, img.Width, img.Height);
                if (bSelected)
                {
                    rimage.Offset(4, 4);
                    g.DrawImage(img, rimage);

                    //disable flashing, since it is the current page
                    tabPage.FlashTab = false;
                }
                else
                {
                    rimage.Offset(4, 6);
                    if (tabPage.FlashTab == true)
                    {
                        if (tabPage.FlashValue == 1)
                            g.DrawImage(img, rimage);
                    }
                    else
                        g.DrawImage(img, rimage);

                }

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Near;
                stringFormat.LineAlignment = StringAlignment.Center;

                //get the tab text color
                if (bSelected)
                {
                    br = new SolidBrush(IrcColor.colors[FormMain.Instance.IceChatColors.TabBarCurrent]);
                    tabPage.LastMessageType = FormMain.ServerMessageType.Default;
                }
                //else if (bHovered)
                //{
                //   br = new SolidBrush(IrcColor.colors[FormMain.Instance.IceChatColors.TabBarCurrent]);
                //}
                else
                {
                    switch (tabPage.LastMessageType)
                    {
                        case FormMain.ServerMessageType.JoinChannel:
                            br = new SolidBrush(IrcColor.colors[FormMain.Instance.IceChatColors.TabBarChannelJoin]);
                            break;
                        case FormMain.ServerMessageType.PartChannel:
                            br = new SolidBrush(IrcColor.colors[FormMain.Instance.IceChatColors.TabBarChannelPart]);
                            break;
                        case FormMain.ServerMessageType.Message:
                            br = new SolidBrush(IrcColor.colors[FormMain.Instance.IceChatColors.TabBarNewMessage]);
                            break;
                        case FormMain.ServerMessageType.Action:
                            br = new SolidBrush(IrcColor.colors[FormMain.Instance.IceChatColors.TabBarNewAction]);
                            break;
                        case FormMain.ServerMessageType.QuitServer:
                            br = new SolidBrush(IrcColor.colors[FormMain.Instance.IceChatColors.TabBarServerQuit]);
                            break;
                        case FormMain.ServerMessageType.ServerMessage:
                            br = new SolidBrush(IrcColor.colors[FormMain.Instance.IceChatColors.TabBarServerMessage]);
                            break;
                        case FormMain.ServerMessageType.ServerNotice:
                            br = new SolidBrush(IrcColor.colors[FormMain.Instance.IceChatColors.TabBarServerNotice]);
                            break;
                        case FormMain.ServerMessageType.Other:
                            br = new SolidBrush(IrcColor.colors[FormMain.Instance.IceChatColors.TabBarOtherMessage]);
                            break;
                        default:
                            br = new SolidBrush(IrcColor.colors[FormMain.Instance.IceChatColors.TabBarDefault]);
                            break;
                    }
                }
                if (tabPage.WindowStyle != IceTabPage.WindowType.Console)
                {
                    if (tabPage.WindowStyle != IceTabPage.WindowType.ChannelList)
                        g.DrawString(tabPage.TabCaption, _tabFont, br, tabTextArea, stringFormat);
                    else
                        g.DrawString(tabPage.TabCaption + "(" + tabPage.TotalChannels + ")", _tabFont, br, tabTextArea, stringFormat);

                }
                else
                {
                    g.DrawString(FormMain.Instance.IceChatLanguage.consoleTabTitle, _tabFont, br, tabTextArea, stringFormat);
                }
            }
            catch (Exception)
            {
                //FormMain.Instance.WriteErrorFile(FormMain.Instance.InputPanel.CurrentConnection, "IceTabControl DrawTab", e);
            }
        }


        private void CalculateTabSizes(Graphics g)
        {
            try
            {
                _tabSizeRects.Clear();
                _tabTextRects.Clear();

                _TotalTabRows = 1;

                int totalWidth = 0;
                int xPos = _tabStartXPos;
                int yPos = 0;

                _TabRowHeight = (int)g.MeasureString("0", _tabFont).Height + 4;

                if ((_TabRowHeight / 2) * 2 == _TabRowHeight)
                    _TabRowHeight++;

                for (int i = 0; i < _TabPages.Count; i++)
                {

                    Rectangle recBounds = new Rectangle();
                    Rectangle recTextArea = new Rectangle();

                    //caclulate the width of the text
                    int textWidth = (int)g.MeasureString(_TabPages[i].TabCaption, _tabFont).Width;
                    if (_TabPages[i].WindowStyle == IceTabPage.WindowType.ChannelList)
                    {
                        textWidth += (int)g.MeasureString(" (" + _TabPages[i].TotalChannels + ") ", _tabFont).Width;
                    }
                    recBounds.Width = textWidth + 26;
                    recBounds.Height = _TabRowHeight + 5;

                    recTextArea.Width = textWidth + 1;
                    recTextArea.Height = (int)g.MeasureString(_TabPages[i].TabCaption, _tabFont).Height + 10;

                    if (totalWidth > 0 && ((totalWidth + recBounds.Width) > (this.Width - 10)))
                    {
                        _TotalTabRows++;
                        totalWidth = 0;
                        xPos = _tabStartXPos;
                        yPos = yPos + _TabRowHeight + 5;
                    }

                    recBounds.X = xPos;
                    recBounds.Y = yPos;

                    recTextArea.X = xPos + 21;  //add area for image and a little extra
                    recTextArea.Y = yPos;

                    _tabSizeRects.Add(i, recBounds);
                    _tabTextRects.Add(i, recTextArea);

                    xPos = xPos + recBounds.Width;
                    totalWidth = totalWidth + recBounds.Width;
                }

                this.Height = (_TabRowHeight + 6) * _TotalTabRows;

            }
            catch (Exception)
            {
                //FormMain.Instance.WriteErrorFile(FormMain.Instance.InputPanel.CurrentConnection, "CalculateTabSizes", e);
            }
        }

        internal void SelectTab(IceTabPage page)
        {
            FormMain.Instance.TabMain.BringFront(page);

            for (int i = 0; i < _TabPages.Count; i++)
            {
                if (_TabPages[i] == page)
                {
                    SelectedIndex = i;
                    this.Invalidate();
                    if (this.SelectedIndexChanged != null)
                    {
                        TabEventArgs e = new TabEventArgs();
                        e.IsHandled = true;
                        SelectedIndexChanged(this, e);
                    }
                    
                    break;                
                }
            }
        }
        private IceTabPage setSelectedByClickLocation(Point pClickLocation)
        {
            if (_tabSizeRects.Count == 0) return null;

            //if (PreviousTab != null)
            //{
                //System.Diagnostics.Debug.WriteLine("previous:" + PreviousTab.TabCaption + ":" + PreviousTabIndex);
            //}
            //PreviousTab = _TabPages[_selectedIndex];
            //PreviousTabIndex = _selectedIndex;
            //System.Diagnostics.Debug.WriteLine("change tab index:" + _selectedIndex);

            for (int i = 0; i < _tabSizeRects.Count; i++)
            {
                Rectangle rectTab = _tabSizeRects[i];
                if ((pClickLocation.X > rectTab.X && pClickLocation.X < rectTab.X + rectTab.Width) && (pClickLocation.Y > rectTab.Y && pClickLocation.Y < rectTab.Bottom))
                {
                    if (this._selectedIndex != i)
                        this._selectedIndex = i;
                    break;
                }
            }

            if (GetTabPage(_selectedIndex) != null)
            {
                if (this.SelectedIndexChanged != null)
                {
                    FormMain.Instance.TabMain.BringFront(_TabPages[_selectedIndex]);

                    TabEventArgs e = new TabEventArgs();
                    SelectedIndexChanged(this, e);
                }
            }

            this.Invalidate();

            return GetTabPage(_selectedIndex);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_tabSizeRects.Count == 0)
                return;

            if (e.Button == MouseButtons.Left)
            {
                Rectangle r = new Rectangle(_dragStartPosition, Size.Empty);

                r.Inflate(SystemInformation.DragSize);

                if (_dragTab != null)
                {
                    if (!r.Contains(e.X, e.Y))
                    {
                        IceTabPage hover_Tab = HoverTab(e.Location);
                        if (hover_Tab != null)
                        {
                            SwapTabPages(_dragTab, hover_Tab);
                            _dragTab = setSelectedByClickLocation(e.Location);
                            this.Invalidate();
                        }
                    }
                }

                _dragStartPosition = Point.Empty;
                return;
            }


            if (e.Y < _tabSizeRects[0].Y + 3 || e.Y > _tabSizeRects[_tabSizeRects.Count - 1].Y + _tabSizeRects[0].Height)
            {
                _hoveredIndex = -1;
                this.Invalidate();
                return;
            }

            int iHoveredIndexBeforeClick = _hoveredIndex;

            for (int i = 0; i < _tabSizeRects.Count; i++)
            {
                Rectangle rectTab = _tabSizeRects[i];
                if ((e.X > rectTab.X && e.X < (rectTab.X + rectTab.Width)) && (e.Y > rectTab.Y && e.Y < (rectTab.Y + rectTab.Height)))
                {
                    if (this._hoveredIndex != i)
                        this._hoveredIndex = i;
                    break;
                }
            }

            if (_hoveredIndex == iHoveredIndexBeforeClick)
                return;

            this.Invalidate();

        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            _hoveredIndex = -1;
            this.Invalidate();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            _dragStartPosition = new Point(e.X, e.Y);
            _dragTab = setSelectedByClickLocation(e.Location);

            if (e.Button == MouseButtons.Middle)
            {
                if (_dragTab != null)
                {
                    if (this.OnTabClosed != null)
                        OnTabClosed(SelectedIndex);
                }
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //show the proper popup menu according to what kind of tab
                if (GetTabPage(_selectedIndex).WindowStyle == IceTabPage.WindowType.Console)
                {
                    //console tab
                    _popupMenu.ItemClicked -= new ToolStripItemClickedEventHandler(OnPopupMenu_ItemClicked);
                    _popupMenu.Items.Clear();
                    _popupMenu = ConsolePopupMenu();
                    _popupMenu.ItemClicked += new ToolStripItemClickedEventHandler(OnPopupMenu_ItemClicked);
                    _popupMenu.Show(this, e.Location);
                }
                else
                {

                    if (GetTabPage(_selectedIndex).WindowStyle == IceTabPage.WindowType.Channel)
                    {
                        _popupMenu.ItemClicked -= new ToolStripItemClickedEventHandler(OnPopupMenu_ItemClicked);
                        _popupMenu.Items.Clear();
                        _popupMenu = ChannelPopupMenu();
                        _popupMenu.ItemClicked += new ToolStripItemClickedEventHandler(OnPopupMenu_ItemClicked);
                        _popupMenu.Show(this, e.Location);

                    }
                    else if (GetTabPage(_selectedIndex).WindowStyle == IceTabPage.WindowType.Query)
                    {
                        _popupMenu.ItemClicked -= new ToolStripItemClickedEventHandler(OnPopupMenu_ItemClicked);
                        _popupMenu.Items.Clear();
                        _popupMenu = QueryPopupMenu();
                        _popupMenu.ItemClicked += new ToolStripItemClickedEventHandler(OnPopupMenu_ItemClicked);
                        _popupMenu.Show(this, e.Location);
                    }
                    else if (GetTabPage(_selectedIndex).WindowStyle == IceTabPage.WindowType.ChannelList)
                    {
                        _popupMenu.ItemClicked -= new ToolStripItemClickedEventHandler(OnPopupMenu_ItemClicked);
                        _popupMenu.Items.Clear();
                        _popupMenu = ChannelListPopupMenu();
                        _popupMenu.ItemClicked += new ToolStripItemClickedEventHandler(OnPopupMenu_ItemClicked);
                        _popupMenu.Show(this, e.Location);
                    }
                    else if (GetTabPage(_selectedIndex).WindowStyle == IceTabPage.WindowType.DCCChat)
                    {
                        _popupMenu.ItemClicked -= new ToolStripItemClickedEventHandler(OnPopupMenu_ItemClicked);
                        _popupMenu.Items.Clear();
                        _popupMenu = DCCChatPopupMenu();
                        _popupMenu.ItemClicked += new ToolStripItemClickedEventHandler(OnPopupMenu_ItemClicked);
                        _popupMenu.Show(this, e.Location);
                    }
                    else if (GetTabPage(_selectedIndex).WindowStyle == IceTabPage.WindowType.Window || GetTabPage(_selectedIndex).WindowStyle == IceTabPage.WindowType.Debug)
                    {
                        _popupMenu.ItemClicked -= new ToolStripItemClickedEventHandler(OnPopupMenu_ItemClicked);
                        _popupMenu.Items.Clear();
                        _popupMenu = WindowPopupMenu();
                        _popupMenu.ItemClicked += new ToolStripItemClickedEventHandler(OnPopupMenu_ItemClicked);
                        _popupMenu.Show(this, e.Location);
                    }
                }
            }

            _dragTab = null;
            FormMain.Instance.FocusInputBox();
        }

        private void SwapTabPages(IceTabPage drag, IceTabPage hover)
        {
            int Index1 = _TabPages.IndexOf(drag);
            int Index2 = _TabPages.IndexOf(hover);

            if (Index1 == Index2) return;

            _TabPages.Remove(drag);
            _TabPages.Insert(Index2, drag);

        }


        private void OnPopupMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //send the command to the proper window
            if (e.ClickedItem.Tag == null) return;

            string command = e.ClickedItem.Tag.ToString();

            ((ContextMenuStrip)(sender)).Close();

            if (GetTabPage(_selectedIndex).WindowStyle == IceTabPage.WindowType.Console)
            {
                //a console command, find out which is the current tab
                command = command.Replace("$1", "Console");
                FormMain.Instance.ParseOutGoingCommand(GetTabPage("Console").CurrentConnection, command);
            }
            else
            {

                IceTabPage t = FormMain.Instance.ChannelBar.TabPages[_selectedIndex];
                if (t != null)
                {
                    command = command.Replace("$1", t.TabCaption);
                    FormMain.Instance.ParseOutGoingCommand(t.Connection, command);
                }
            }
        }


        private ContextMenuStrip ConsolePopupMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            menu.Items.Add(NewMenuItem("Clear", "/clear $1", StaticMethods.LoadResourceImage("clear.png")));
            menu.Items.Add(NewMenuItem("Clear All", "/clear all console", StaticMethods.LoadResourceImage("clear.png")));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(NewMenuItem("Quit Server", "//quit", StaticMethods.LoadResourceImage("disconected.png")));
            menu.Items.Add(NewMenuItem("Auto Join", "/autojoin", null));
            menu.Items.Add(NewMenuItem("Auto Perform", "/autoperform", null));

            if (FormMain.Instance != null && FormMain.Instance.IceChatPlugins != null)
            {
                foreach (IPluginIceChat ipc in FormMain.Instance.IceChatPlugins)
                {
                    ToolStripItem[] popServer = ipc.AddServerPopups();
                    if (popServer != null && popServer.Length > 0)
                    {
                        menu.Items.AddRange(popServer);
                    }
                }
            }

            //add the console popup menu
            AddPopupMenu("Console", menu);
            return menu;
        }

        private ContextMenuStrip ChannelPopupMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            menu.Items.Add(NewMenuItem("Clear Window", "/clear $1", StaticMethods.LoadResourceImage("clear.png")));
            menu.Items.Add(NewMenuItem("Close Channel", "/part $1", StaticMethods.LoadResourceImage("CloseButton.png")));
            menu.Items.Add(NewMenuItem("Rejoin Channel", "/hop $1", StaticMethods.LoadResourceImage("refresh.png")));
            menu.Items.Add(NewMenuItem("Add to Autojoin Channel", "/autojoin $1", StaticMethods.LoadResourceImage("info.png")));
            menu.Items.Add(NewMenuItem("Channel Information", "/chaninfo $1", StaticMethods.LoadResourceImage("info.png")));
            menu.Items.Add(NewMenuItem("Change Font", "/font $1", StaticMethods.LoadResourceImage("fonts.png")));

            //menu.Items.Add(NewMenuItem("Disable Sounds", "/sound $1", StaticMethods.LoadResourceImage("mute.png")));

            if (FormMain.Instance != null && FormMain.Instance.IceChatPlugins != null)
            {
                foreach (IPluginIceChat ipc in FormMain.Instance.IceChatPlugins)
                {
                    ToolStripItem[] popServer = ipc.AddChannelPopups();
                    if (popServer != null && popServer.Length > 0)
                    {
                        menu.Items.AddRange(popServer);
                    }
                }
            }

            //add then channel popup menu
            AddPopupMenu("Channel", menu);
            return menu;
        }

        private ContextMenuStrip QueryPopupMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            menu.Items.Add(NewMenuItem("Clear Window", "/clear $1", StaticMethods.LoadResourceImage("clear.png")));
            menu.Items.Add(NewMenuItem("Close Window", "/part $1", StaticMethods.LoadResourceImage("CloseButton.png")));
            menu.Items.Add(NewMenuItem("User Information", "/userinfo $1", StaticMethods.LoadResourceImage("refresh.png")));
            menu.Items.Add(NewMenuItem("Silence User", "/silence +$1", StaticMethods.LoadResourceImage("info.png")));

            if (FormMain.Instance != null && FormMain.Instance.IceChatPlugins != null)
            {
                foreach (IPluginIceChat ipc in FormMain.Instance.IceChatPlugins)
                {
                    ToolStripItem[] popServer = ipc.AddQueryPopups();
                    if (popServer != null && popServer.Length > 0)
                    {
                        menu.Items.AddRange(popServer);
                    }
                }
            }

            //add then channel popup menu
            AddPopupMenu("Query", menu);
            return menu;

        }

        private ContextMenuStrip DCCChatPopupMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add(NewMenuItem("Clear Window", "/clear $1", StaticMethods.LoadResourceImage("clear.png")));
            menu.Items.Add(NewMenuItem("Close Window", "/close $1", StaticMethods.LoadResourceImage("CloseButton.png")));
            menu.Items.Add(NewMenuItem("Disconnect", "/disconnect $1", StaticMethods.LoadResourceImage("clear.png")));

            //AddPopupMenu("DCCChat", menu);
            return menu;

        }

        private ContextMenuStrip WindowPopupMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add(NewMenuItem("Clear Window", "/clear $1", StaticMethods.LoadResourceImage("clear.png")));
            menu.Items.Add(NewMenuItem("Close Window", "/close $1", StaticMethods.LoadResourceImage("CloseButton.png")));

            return menu;

        }

        private ContextMenuStrip ChannelListPopupMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add(NewMenuItem("Close Window", "/close $1", StaticMethods.LoadResourceImage("CloseButton.png")));

            AddPopupMenu("ChannelList", menu);
            return menu;

        }

        private ToolStripMenuItem NewMenuItem(string caption, string command, Bitmap icon)
        {
            ToolStripMenuItem t = new ToolStripMenuItem(caption);
            t.Tag = command;
            return t;
        }

        internal IceTabPage GetTabPage(string sCaption)
        {
            for (int i = 0; i < _TabPages.Count; i++)
            {
                if (_TabPages[i].TabCaption.Equals(sCaption))
                    return _TabPages[i];
            }
            return null;
        }
        //just return for channel - tab order
        internal int GetTabPage(string sCaption, IRCConnection connection)
        {
            for (int i = 0; i < _TabPages.Count; i++)
            {
                if (_TabPages[i].TabCaption.ToLower() == sCaption && _TabPages[i].Connection == connection && _TabPages[i].WindowStyle == IceTabPage.WindowType.Channel)
                    return i;
            }
            return 0;
        }

        internal IceTabPage GetTabPage(int iTabIndex)
        {
            if (iTabIndex < _TabPages.Count)
                return _TabPages[iTabIndex];
            return null;
        }

        internal void CloseCurrentTab()
        {
            IceTabPage current = GetTabPage(_selectedIndex);
            if (current != null)
            {

                if (current.WindowStyle == IceTabPage.WindowType.Console)
                {
                    return;
                }
                
                if (this.OnTabClosed != null)
                    OnTabClosed(SelectedIndex);

                _TabPages.Remove(current);

                this.Invalidate();
            }
        }


        private void AddPopupMenu(string PopupType, ContextMenuStrip mainMenu)
        {
            //add the console menu popup
            if (FormMain.Instance == null) return;

            if (FormMain.Instance.IceChatPopupMenus == null) return;

            foreach (PopupMenuItem p in FormMain.Instance.IceChatPopupMenus.listPopups)
            {
                if (p.PopupType == PopupType && p.Menu.Length > 0)
                {
                    //add a break
                    ToolStripItem sep = new ToolStripSeparator();
                    mainMenu.Items.Add(sep);

                    string[] menuItems = p.Menu;
                    List<string> list = new List<string>();
                    for (int i = 0; i < menuItems.Length; i++)
                    {
                        if (menuItems[i] != "")
                            list.Add(menuItems[i]);
                    }
                    menuItems = list.ToArray();

                    //build the menu
                    ToolStripItem t;
                    int subMenu = 0;

                    foreach (string menu in menuItems)
                    {
                        string caption;
                        string command;
                        string menuItem = menu;
                        int menuDepth = 0;

                        //get the menu depth
                        while (menuItem.StartsWith("."))
                        {
                            menuItem = menuItem.Substring(1);
                            menuDepth++;
                        }

                        if (menu.IndexOf(':') > 0)
                        {
                            caption = menuItem.Substring(0, menuItem.IndexOf(':'));
                            command = menuItem.Substring(menuItem.IndexOf(':') + 1);
                        }
                        else
                        {
                            caption = menuItem;
                            command = "";
                        }


                        if (caption.Length > 0)
                        {
                            //parse out $identifiers
                            IceTabPage tw = null;
                            if (GetTabPage(_selectedIndex).WindowStyle != IceTabPage.WindowType.Console)
                            {
                                tw = GetTabPage(_selectedIndex);
                            }

                            if (p.PopupType == "Channel")
                            {
                                if (tw != null)
                                {
                                    command = command.Replace("$chanlogdir", tw.TextWindow.LogFileLocation);
                                    caption = caption.Replace("$chan", tw.TabCaption);
                                    command = command.Replace("$chan", tw.TabCaption);
                                    caption = caption.Replace("$1", tw.TabCaption);
                                    command = command.Replace("$1", tw.TabCaption);

                                }
                            }

                            if (p.PopupType == "Query")
                            {
                                if (tw != null)
                                {
                                    caption = caption.Replace("$nick", tw.TabCaption);
                                    command = command.Replace("$nick", tw.TabCaption);
                                    caption = caption.Replace("$1", tw.TabCaption);
                                    command = command.Replace("$1", tw.TabCaption);
                                    command = command.Replace("$querylogdir", tw.TextWindow.LogFileLocation);
                                }
                            }

                            if (p.PopupType == "ChannelList")
                            {
                                if (tw != null)
                                {
                                    caption = caption.Replace("$1", tw.WindowStyle.ToString());
                                    command = command.Replace("$1", tw.WindowStyle.ToString());
                                }
                            }

                            if (caption == "-")
                                t = new ToolStripSeparator();
                            else
                            {
                                t = new ToolStripMenuItem(caption);

                                t.Click += new EventHandler(OnPopupExtraMenuClick);
                                t.Tag = command;
                            }

                            if (menuDepth == 0)
                                subMenu = mainMenu.Items.Add(t);
                            else
                            {
                                if (mainMenu.Items[subMenu].GetType() == typeof(ToolStripMenuItem))
                                    ((ToolStripMenuItem)mainMenu.Items[subMenu]).DropDownItems.Add(t);
                            }

                            t = null;
                        }
                    }
                }
            }
        }

        private void OnPopupExtraMenuClick(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Tag == null) return;

            string command = ((ToolStripMenuItem)sender).Tag.ToString();

            if (GetTabPage(_selectedIndex).WindowStyle == IceTabPage.WindowType.Console)
            {
                //a console command, find out which is the current tab
                command = command.Replace("$1", "Console");
                FormMain.Instance.ParseOutGoingCommand(GetTabPage("Console").CurrentConnection, command);
            }
            else
            {

                IceTabPage t = GetTabPage(_selectedIndex);
                if (t != null)
                {
                    command = command.Replace("$1", t.TabCaption);
                    FormMain.Instance.ParseOutGoingCommand(t.Connection, command);
                }
            }
        }

        private void OnFlashTabTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Invalidate();
            FormMain.Instance.ServerTree.Invalidate();
        }

        internal bool WindowExists(IRCConnection connection, string windowName, IceTabPage.WindowType windowType)
        {
            foreach (IceTabPage t in this._TabPages)
            {
                if (t.Connection == null)
                {
                    if (t.WindowStyle == IceTabPage.WindowType.DCCFile)
                    {
                        if (t.TabCaption.ToLower() == windowName.ToLower())
                            return true;
                    }
                }
                else if (t.Connection == connection)
                {
                    if (t.WindowStyle == windowType)
                    {
                        if (t.TabCaption.ToLower() == windowName.ToLower())
                            return true;
                    }
                }
            }
            return false;
        }
    }

    internal class TabEventArgs : System.EventArgs
    {
        public bool IsHandled;
    }
}