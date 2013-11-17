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
using IceChatPlugin;

namespace IceChat
{
    public partial class IceTabControl : UserControl 
    {

        //private Font _tabFont = new Font("Verdana", 10F);
        
        //private Dictionary<int, Rectangle> _tabSizeRects = new Dictionary<int, Rectangle>();
        //private Dictionary<int, Rectangle> _tabTextRects = new Dictionary<int, Rectangle>();

        //private List<IceTabPage> _TabPages = new List<IceTabPage>();
        
        private int _selectedIndex = -1;
        //private int _previousSelectedIndex = 0;

        private Point _dragStartPosition = Point.Empty;

        //which tab will be dragged
        /*
        internal delegate void TabEventHandler(object sender, TabEventArgs e);
        internal event TabEventHandler SelectedIndexChanged;
        
        internal delegate void TabClosedDelegate(int nIndex);
        internal event TabClosedDelegate OnTabClosed;

        internal IceTabPage PreviousTab = null;
        internal int PreviousTabIndex = 0;
        */

        private IceTabPage _currentTab = null;
        private IceTabPage _previousTab = null;
        private MdiLayout _layout = MdiLayout.Cascade;

        public IceTabControl()
        {

            InitializeComponent();
            this.ControlRemoved+=new ControlEventHandler(OnControlRemoved);
        
        }

        internal MdiLayout MdiLayout
        {
            get { return _layout; }
            set { this._layout = value; }
        }

        internal void AddTabPage(IceTabPage page)
        {            
            page.Dock = DockStyle.Fill;
            page.Location = new Point(0, 1);

            if (!this.Controls.Contains(page))
                this.Controls.Add(page);

            _previousTab = _currentTab;
            _currentTab = page;
        
        }


        internal void BringFront(IceTabPage page)
        {
            if (page.Parent != null)
            {
                if (page.Parent.GetType() == typeof(FormWindow))
                    page.Parent.BringToFront();
                else
                    page.BringToFront();
            }
            else
            {
                page.BringToFront();
            }

            _selectedIndex = page.TabIndex;
            
            FormMain.Instance.ChannelBar.Invalidate();
            FormMain.Instance.ServerTree.Invalidate();

            _previousTab = _currentTab;
           
            if (_previousTab.WindowStyle != IceTabPage.WindowType.Console)
               if (_previousTab.TextWindow != null)
                   _previousTab.TextWindow.resetUnreadMarker();        

            _currentTab = page;           
        }
        
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //
        }

        protected override void OnPaint(PaintEventArgs e) 
        {
            //
        }

        private void OnControlRemoved(object sender, ControlEventArgs e) 
        {
            if (e.Control is IceTabPage) 
            {
                //System.Diagnostics.Debug.WriteLine("remove tab:" + ((IceTabPage)e.Control).TabCaption + ":" + _currentTab.TabCaption + ":" + _previousTab.TabCaption);
                //((IceTabPage)e.Control).Dispose();
                
                if (((IceTabPage)e.Control).DockedForm == false)
                    ((IceTabPage)e.Control).Dispose();
                
                /*
                //what tab is on top
                foreach(IceTabPage page in this.Controls)
                {
                    //System.Diagnostics.Debug.WriteLine(page.TabCaption + ":" + ((Control)page).Name + ":" + this.Controls.GetChildIndex(page));
                    if (this.Controls.GetChildIndex(page) == 0)
                    {
                        //this is the current 
                        _currentTab = page;
                    }

                    if (this.Controls.GetChildIndex(page) == 1)
                        _previousTab = page;
                }
                
                //redraw
                FormMain.Instance.ChannelBar.SelectTab(_currentTab);
                FormMain.Instance.ServerTree.Invalidate();

                //FormMain.Instance.ChannelBar.Invalidate();
                //FormMain.Instance.NickList.Invalidate();
                */

            }
            else if (e.Control is IceTabPageDCCFile)
            {

                if (((IceTabPageDCCFile)e.Control).DockedForm == false)
                    ((IceTabPageDCCFile)e.Control).Dispose();

            }

            foreach (IceTabPage page in this.Controls)
            {
                if (this.Controls.GetChildIndex(page) == 0)
                {
                    //this is the current 
                    _currentTab = page;
                }

                if (this.Controls.GetChildIndex(page) == 1)
                    _previousTab = page;
            }

            //redraw
            FormMain.Instance.ChannelBar.SelectTab(_currentTab);
            FormMain.Instance.ServerTree.Invalidate();

        }
        
    }

}
