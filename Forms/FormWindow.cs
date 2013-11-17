using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace IceChat
{
    public partial class FormWindow : Form
    {
        private IceTabPage dockedControl;
        private bool kickedChannel;

        public FormWindow(IceTabPage tab)
        {
            //used to hold each tab in MDI
            InitializeComponent();

            this.Controls.Add(tab);
            tab.Dock = DockStyle.Fill;
            dockedControl = tab;

            this.Activated += new EventHandler(OnActivated);
            this.Resize += new EventHandler(OnResize);
            this.Move += new EventHandler(OnMove);

            //this.ControlRemoved += new ControlEventHandler(OnControlRemoved);            
            this.FormClosing += new FormClosingEventHandler(OnFormClosing);
            
            if (tab.WindowStyle == IceTabPage.WindowType.Console)
                this.Icon = System.Drawing.Icon.FromHandle(StaticMethods.LoadResourceImage("console.png").GetHicon());
            else if (tab.WindowStyle == IceTabPage.WindowType.Channel)
                this.Icon = System.Drawing.Icon.FromHandle(StaticMethods.LoadResourceImage("channel.png").GetHicon());
            else if (tab.WindowStyle == IceTabPage.WindowType.Query)
                this.Icon = System.Drawing.Icon.FromHandle(StaticMethods.LoadResourceImage("new-query.ico").GetHicon());
            else if (tab.WindowStyle == IceTabPage.WindowType.ChannelList)
                this.Icon = System.Drawing.Icon.FromHandle(StaticMethods.LoadResourceImage("channellist.png").GetHicon());
            else //for the rest
                this.Icon = System.Drawing.Icon.FromHandle(StaticMethods.LoadResourceImage("window-icon.ico").GetHicon());

        }

        internal void DisableActivate()
        {
            this.Activated -= OnActivated;
        }

        internal void DisableResize()
        {
            this.Resize -= OnResize;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            //need to send the part message
            try
            {
                System.Diagnostics.Debug.WriteLine("closing form:" + this.Text);
                if (this.Controls.Count == 1 && kickedChannel == false)
                {
                    if (dockedControl.WindowStyle == IceTabPage.WindowType.Console)
                    {
                        //System.Diagnostics.Debug.WriteLine(e.CloseReason);
                        if (e.CloseReason == CloseReason.UserClosing)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }

                    if (dockedControl.WindowStyle == IceTabPage.WindowType.Channel)
                        FormMain.Instance.ParseOutGoingCommand(dockedControl.Connection, "/part " + dockedControl.TabCaption);
                    else if (dockedControl.WindowStyle == IceTabPage.WindowType.Query)
                        FormMain.Instance.ParseOutGoingCommand(dockedControl.Connection, "/close " + dockedControl.TabCaption);
                    else
                    {                        
                        //remove this from the channel bar
                        FormMain.Instance.RemoveWindow(dockedControl.Connection, dockedControl.TabCaption, dockedControl.WindowStyle);
                    }
                    
                }
            }
            catch (Exception ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                System.Diagnostics.Debug.WriteLine(ee.StackTrace);
            }
        }

        private void OnControlRemoved(object sender, ControlEventArgs e)
        {
            if (e.Control is IceTabPage)
            {
                System.Diagnostics.Debug.WriteLine("remove tab (form):" + ((IceTabPage)e.Control).TabCaption + ":" + this.Controls.Count);
            }
        }

        private void OnMove(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                dockedControl.WindowLocation = this.Location;
            }
        }


        private void OnResize(object sender, EventArgs e)
        {
            //change back to tabbed view if maximized
            if (this.WindowState == FormWindowState.Maximized)
            {
                //thsi gets called for all forms, disable them all
                foreach (FormWindow child in FormMain.Instance.MdiChildren)
                {
                    child.DisableResize();
                }

                FormMain.Instance.ReDockTabs();
            }        
            else
                dockedControl.WindowSize = this.Size;

        }

        private void OnActivated(object sender, EventArgs e)
        {
            //disable this if we are un-docking
            
            FormMain.Instance.ChannelBar.SelectTab(dockedControl);
            FormMain.Instance.ServerTree.SelectTab(dockedControl, false);
            
            //set the tabindex to 0
            dockedControl.TabIndex = 0;
            
            //set the rest to 1
            foreach (FormWindow child in FormMain.Instance.MdiChildren)
            {
                if (child != this)
                {
                    IceTabPage tab = child.DockedControl;
                    tab.TabIndex = 1;                    
                }
            }
        }

        internal IceTabPage DockedControl
        {
            get { return dockedControl; }
        }

        internal bool KickedChannel
        {
            get { return kickedChannel; }
            set { kickedChannel = value; }
        }

    }
}
