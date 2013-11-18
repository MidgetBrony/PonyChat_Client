using System.Windows.Forms;
using System;
using System.Drawing;

namespace IceChat
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>

        private IceTabControl mainTabControl;
        private InputPanel inputPanel;
        private System.Windows.Forms.MenuStrip menuMainStrip;
        private System.Windows.Forms.ToolStripMenuItem mainToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem iceChatColorsToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem iceChatEditorToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem iceChatSettingsToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuToolBar;
        private System.Windows.Forms.ToolStripMenuItem hideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minimizeToTrayToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.menuMainStrip = new System.Windows.Forms.MenuStrip();
            this.mainToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.minimizeToTrayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeCurrentWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iceChatSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fontSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iceChatColorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iceChatEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.muteAllSoundsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAPluginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.themesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStylesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.vS2008ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.office2007ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serverListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nickListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.channelBarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.multilineEditboxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.channelListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.selectNickListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nickListImageMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.nickListImageRemoveMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.selectServerTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serverTreeImageMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.serverTreeImageRemoveMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.browseDataFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browsePluginsFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.splitterLeft = new System.Windows.Forms.Splitter();
            this.splitterRight = new System.Windows.Forms.Splitter();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuNotify = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.restoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuToolBar = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.hideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.splitterBottom = new System.Windows.Forms.Splitter();
            this.nickListTab = new System.Windows.Forms.TabPage();
            this.nickPanel = new System.Windows.Forms.Panel();
            this.nickList = new IceChat.NickList();
            this.serverListTab = new System.Windows.Forms.TabPage();
            this.serverPanel = new System.Windows.Forms.Panel();
            this.serverTree = new IceChat.ServerTree();
            this.mainTabControl = new IceChat.IceTabControl();
            this.panelDockRight = new IceChat.IceDockPanel();
            this.panelDockLeft = new IceChat.IceDockPanel();
            this.inputPanel = new IceChat.InputPanel();
            this.mainChannelBar = new IceChat.ChannelBar();
            this.editServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuMainStrip.SuspendLayout();
            this.contextMenuNotify.SuspendLayout();
            this.contextMenuToolBar.SuspendLayout();
            this.statusStripMain.SuspendLayout();
            this.nickListTab.SuspendLayout();
            this.nickPanel.SuspendLayout();
            this.serverListTab.SuspendLayout();
            this.serverPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuMainStrip
            // 
            this.menuMainStrip.AccessibleDescription = "Main Menu Bar";
            this.menuMainStrip.AccessibleRole = System.Windows.Forms.AccessibleRole.MenuBar;
            this.menuMainStrip.AllowItemReorder = true;
            this.menuMainStrip.BackColor = System.Drawing.SystemColors.MenuBar;
            this.menuMainStrip.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuMainStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mainToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem,
            this.closeWindow,
            this.toolStripUpdate});
            this.menuMainStrip.Location = new System.Drawing.Point(0, 0);
            this.menuMainStrip.Name = "menuMainStrip";
            this.menuMainStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuMainStrip.Size = new System.Drawing.Size(924, 24);
            this.menuMainStrip.TabIndex = 12;
            this.menuMainStrip.Text = "menuStripMain";
            // 
            // mainToolStripMenuItem
            // 
            this.mainToolStripMenuItem.BackColor = System.Drawing.SystemColors.MenuBar;
            this.mainToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.mainToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.minimizeToTrayToolStripMenuItem,
            this.debugWindowToolStripMenuItem,
            this.closeCurrentWindowToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exitToolStripMenuItem});
            this.mainToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.mainToolStripMenuItem.Name = "mainToolStripMenuItem";
            this.mainToolStripMenuItem.Size = new System.Drawing.Size(42, 20);
            this.mainToolStripMenuItem.Text = "File";
            // 
            // minimizeToTrayToolStripMenuItem
            // 
            this.minimizeToTrayToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.minimizeToTrayToolStripMenuItem.ForeColor = System.Drawing.Color.Black;
            this.minimizeToTrayToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("minimizeToTrayToolStripMenuItem.Image")));
            this.minimizeToTrayToolStripMenuItem.Name = "minimizeToTrayToolStripMenuItem";
            this.minimizeToTrayToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.minimizeToTrayToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.minimizeToTrayToolStripMenuItem.Text = "Minimize to Tray";
            this.minimizeToTrayToolStripMenuItem.Click += new System.EventHandler(this.minimizeToTrayToolStripMenuItem_Click);
            // 
            // debugWindowToolStripMenuItem
            // 
            this.debugWindowToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.debugWindowToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.debugWindowToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("debugWindowToolStripMenuItem.Image")));
            this.debugWindowToolStripMenuItem.Name = "debugWindowToolStripMenuItem";
            this.debugWindowToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.debugWindowToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.debugWindowToolStripMenuItem.Text = "Debug Window";
            this.debugWindowToolStripMenuItem.Click += new System.EventHandler(this.debugWindowToolStripMenuItem_Click);
            // 
            // closeCurrentWindowToolStripMenuItem
            // 
            this.closeCurrentWindowToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.closeCurrentWindowToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.closeCurrentWindowToolStripMenuItem.Name = "closeCurrentWindowToolStripMenuItem";
            this.closeCurrentWindowToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.closeCurrentWindowToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.closeCurrentWindowToolStripMenuItem.Text = "Close Current Window";
            this.closeCurrentWindowToolStripMenuItem.Click += new System.EventHandler(this.closeCurrentWindowToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(271, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.exitToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.BackColor = System.Drawing.SystemColors.MenuBar;
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.iceChatSettingsToolStripMenuItem,
            this.fontSettingsToolStripMenuItem,
            this.iceChatColorsToolStripMenuItem,
            this.iceChatEditorToolStripMenuItem,
            this.muteAllSoundsToolStripMenuItem,
            this.pluginsToolStripMenuItem,
            this.editServerToolStripMenuItem});
            this.optionsToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // iceChatSettingsToolStripMenuItem
            // 
            this.iceChatSettingsToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.iceChatSettingsToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.iceChatSettingsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("iceChatSettingsToolStripMenuItem.Image")));
            this.iceChatSettingsToolStripMenuItem.Name = "iceChatSettingsToolStripMenuItem";
            this.iceChatSettingsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.iceChatSettingsToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.iceChatSettingsToolStripMenuItem.Text = "Program Settings...";
            this.iceChatSettingsToolStripMenuItem.Click += new System.EventHandler(this.iceChatSettingsToolStripMenuItem_Click);
            // 
            // fontSettingsToolStripMenuItem
            // 
            this.fontSettingsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("fontSettingsToolStripMenuItem.Image")));
            this.fontSettingsToolStripMenuItem.Name = "fontSettingsToolStripMenuItem";
            this.fontSettingsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.fontSettingsToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.fontSettingsToolStripMenuItem.Text = "Font Settings...";
            this.fontSettingsToolStripMenuItem.Click += new System.EventHandler(this.fontSettingsToolStripMenuItem_Click);
            // 
            // iceChatColorsToolStripMenuItem
            // 
            this.iceChatColorsToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.iceChatColorsToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.iceChatColorsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("iceChatColorsToolStripMenuItem.Image")));
            this.iceChatColorsToolStripMenuItem.Name = "iceChatColorsToolStripMenuItem";
            this.iceChatColorsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.iceChatColorsToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.iceChatColorsToolStripMenuItem.Text = "Colors Settings...";
            this.iceChatColorsToolStripMenuItem.Click += new System.EventHandler(this.iceChatColorsToolStripMenuItem_Click);
            // 
            // iceChatEditorToolStripMenuItem
            // 
            this.iceChatEditorToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.iceChatEditorToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.iceChatEditorToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("iceChatEditorToolStripMenuItem.Image")));
            this.iceChatEditorToolStripMenuItem.Name = "iceChatEditorToolStripMenuItem";
            this.iceChatEditorToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.iceChatEditorToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.iceChatEditorToolStripMenuItem.Text = "Editor...";
            this.iceChatEditorToolStripMenuItem.Click += new System.EventHandler(this.iceChatEditorToolStripMenuItem_Click);
            // 
            // muteAllSoundsToolStripMenuItem
            // 
            this.muteAllSoundsToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.muteAllSoundsToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.muteAllSoundsToolStripMenuItem.Name = "muteAllSoundsToolStripMenuItem";
            this.muteAllSoundsToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.muteAllSoundsToolStripMenuItem.Text = "Mute all Sounds";
            this.muteAllSoundsToolStripMenuItem.Click += new System.EventHandler(this.muteAllSoundsToolStripMenuItem_Click);
            // 
            // pluginsToolStripMenuItem
            // 
            this.pluginsToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.pluginsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadAPluginToolStripMenuItem});
            this.pluginsToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.pluginsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pluginsToolStripMenuItem.Image")));
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.pluginsToolStripMenuItem.Text = "Loaded Plugins";
            // 
            // loadAPluginToolStripMenuItem
            // 
            this.loadAPluginToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.loadAPluginToolStripMenuItem.Name = "loadAPluginToolStripMenuItem";
            this.loadAPluginToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.loadAPluginToolStripMenuItem.Text = "Load a Plugin...";
            this.loadAPluginToolStripMenuItem.ToolTipText = "Load a new Plugin";
            this.loadAPluginToolStripMenuItem.Click += new System.EventHandler(this.loadAPluginToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.BackColor = System.Drawing.SystemColors.MenuBar;
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.themesToolStripMenuItem,
            this.menuStylesToolStripMenuItem,
            this.serverListToolStripMenuItem,
            this.nickListToolStripMenuItem,
            this.statusBarToolStripMenuItem,
            this.channelBarToolStripMenuItem,
            this.multilineEditboxToolStripMenuItem,
            this.channelListToolStripMenuItem,
            this.toolStripMenuItem3,
            this.selectNickListToolStripMenuItem,
            this.selectServerTreeToolStripMenuItem});
            this.viewToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // themesToolStripMenuItem
            // 
            this.themesToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.themesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defaultToolStripMenuItem});
            this.themesToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.themesToolStripMenuItem.Name = "themesToolStripMenuItem";
            this.themesToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.themesToolStripMenuItem.Text = "Themes";
            // 
            // defaultToolStripMenuItem
            // 
            this.defaultToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.defaultToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.defaultToolStripMenuItem.Name = "defaultToolStripMenuItem";
            this.defaultToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.defaultToolStripMenuItem.Text = "Default";
            this.defaultToolStripMenuItem.Click += new System.EventHandler(this.defaultToolStripMenuItem_Click);
            // 
            // menuStylesToolStripMenuItem
            // 
            this.menuStylesToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.menuStylesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defaultToolStripMenuItem1,
            this.vS2008ToolStripMenuItem,
            this.office2007ToolStripMenuItem});
            this.menuStylesToolStripMenuItem.Name = "menuStylesToolStripMenuItem";
            this.menuStylesToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.menuStylesToolStripMenuItem.Text = "Menu Styles";
            // 
            // defaultToolStripMenuItem1
            // 
            this.defaultToolStripMenuItem1.BackColor = System.Drawing.SystemColors.Menu;
            this.defaultToolStripMenuItem1.Name = "defaultToolStripMenuItem1";
            this.defaultToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.defaultToolStripMenuItem1.Text = "Default";
            this.defaultToolStripMenuItem1.Click += new System.EventHandler(this.DefaultRendererToolStripMenuItem_Click);
            // 
            // vS2008ToolStripMenuItem
            // 
            this.vS2008ToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.vS2008ToolStripMenuItem.Name = "vS2008ToolStripMenuItem";
            this.vS2008ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.vS2008ToolStripMenuItem.Text = "VS 2008";
            this.vS2008ToolStripMenuItem.Click += new System.EventHandler(this.VS2008ToolStripMenuItem_Click);
            // 
            // office2007ToolStripMenuItem
            // 
            this.office2007ToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.office2007ToolStripMenuItem.Name = "office2007ToolStripMenuItem";
            this.office2007ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.office2007ToolStripMenuItem.Text = "Office 2007";
            this.office2007ToolStripMenuItem.Click += new System.EventHandler(this.Office2007ToolStripMenuItem_Click);
            // 
            // serverListToolStripMenuItem
            // 
            this.serverListToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.serverListToolStripMenuItem.Checked = true;
            this.serverListToolStripMenuItem.CheckOnClick = true;
            this.serverListToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.serverListToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.serverListToolStripMenuItem.Name = "serverListToolStripMenuItem";
            this.serverListToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.serverListToolStripMenuItem.Text = "Left Panel";
            this.serverListToolStripMenuItem.Click += new System.EventHandler(this.serverListToolStripMenuItem_Click);
            // 
            // nickListToolStripMenuItem
            // 
            this.nickListToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.nickListToolStripMenuItem.Checked = true;
            this.nickListToolStripMenuItem.CheckOnClick = true;
            this.nickListToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.nickListToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.nickListToolStripMenuItem.Name = "nickListToolStripMenuItem";
            this.nickListToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.nickListToolStripMenuItem.Text = "Right Panel";
            this.nickListToolStripMenuItem.Click += new System.EventHandler(this.nickListToolStripMenuItem_Click);
            // 
            // statusBarToolStripMenuItem
            // 
            this.statusBarToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.statusBarToolStripMenuItem.Checked = true;
            this.statusBarToolStripMenuItem.CheckOnClick = true;
            this.statusBarToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.statusBarToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.statusBarToolStripMenuItem.Name = "statusBarToolStripMenuItem";
            this.statusBarToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.statusBarToolStripMenuItem.Text = "Status Bar";
            this.statusBarToolStripMenuItem.Click += new System.EventHandler(this.statusBarToolStripMenuItem_Click);
            // 
            // channelBarToolStripMenuItem
            // 
            this.channelBarToolStripMenuItem.Checked = true;
            this.channelBarToolStripMenuItem.CheckOnClick = true;
            this.channelBarToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.channelBarToolStripMenuItem.Name = "channelBarToolStripMenuItem";
            this.channelBarToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.channelBarToolStripMenuItem.Text = "Channel Bar";
            this.channelBarToolStripMenuItem.Click += new System.EventHandler(this.channelBarToolStripMenuItem_Click);
            // 
            // multilineEditboxToolStripMenuItem
            // 
            this.multilineEditboxToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.multilineEditboxToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.multilineEditboxToolStripMenuItem.Name = "multilineEditboxToolStripMenuItem";
            this.multilineEditboxToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.multilineEditboxToolStripMenuItem.Text = "Multiline Editbox";
            this.multilineEditboxToolStripMenuItem.Click += new System.EventHandler(this.multilineEditboxToolStripMenuItem_Click);
            // 
            // channelListToolStripMenuItem
            // 
            this.channelListToolStripMenuItem.BackColor = System.Drawing.SystemColors.MenuBar;
            this.channelListToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.channelListToolStripMenuItem.Name = "channelListToolStripMenuItem";
            this.channelListToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.channelListToolStripMenuItem.Text = "Channel List";
            this.channelListToolStripMenuItem.Click += new System.EventHandler(this.channelListToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.BackColor = System.Drawing.SystemColors.Menu;
            this.toolStripMenuItem3.ForeColor = System.Drawing.SystemColors.MenuText;
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(179, 6);
            // 
            // selectNickListToolStripMenuItem
            // 
            this.selectNickListToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.selectNickListToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nickListImageMenu,
            this.nickListImageRemoveMenu});
            this.selectNickListToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.selectNickListToolStripMenuItem.Name = "selectNickListToolStripMenuItem";
            this.selectNickListToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.selectNickListToolStripMenuItem.Text = "Nick List";
            // 
            // nickListImageMenu
            // 
            this.nickListImageMenu.BackColor = System.Drawing.SystemColors.Menu;
            this.nickListImageMenu.ForeColor = System.Drawing.SystemColors.MenuText;
            this.nickListImageMenu.Name = "nickListImageMenu";
            this.nickListImageMenu.Size = new System.Drawing.Size(253, 22);
            this.nickListImageMenu.Text = "Background Image...";
            this.nickListImageMenu.Click += new System.EventHandler(this.nickListImageMenu_Click);
            // 
            // nickListImageRemoveMenu
            // 
            this.nickListImageRemoveMenu.BackColor = System.Drawing.SystemColors.Menu;
            this.nickListImageRemoveMenu.ForeColor = System.Drawing.SystemColors.MenuText;
            this.nickListImageRemoveMenu.Name = "nickListImageRemoveMenu";
            this.nickListImageRemoveMenu.Size = new System.Drawing.Size(253, 22);
            this.nickListImageRemoveMenu.Text = "Remove Background Image";
            this.nickListImageRemoveMenu.Click += new System.EventHandler(this.nickListImageRemoveMenu_Click);
            // 
            // selectServerTreeToolStripMenuItem
            // 
            this.selectServerTreeToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.selectServerTreeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.serverTreeImageMenu,
            this.serverTreeImageRemoveMenu});
            this.selectServerTreeToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.selectServerTreeToolStripMenuItem.Name = "selectServerTreeToolStripMenuItem";
            this.selectServerTreeToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.selectServerTreeToolStripMenuItem.Text = "Server Tree";
            // 
            // serverTreeImageMenu
            // 
            this.serverTreeImageMenu.BackColor = System.Drawing.SystemColors.Menu;
            this.serverTreeImageMenu.ForeColor = System.Drawing.SystemColors.MenuText;
            this.serverTreeImageMenu.Name = "serverTreeImageMenu";
            this.serverTreeImageMenu.Size = new System.Drawing.Size(253, 22);
            this.serverTreeImageMenu.Text = "Background Image...";
            this.serverTreeImageMenu.Click += new System.EventHandler(this.serverTreeImageMenu_Click);
            // 
            // serverTreeImageRemoveMenu
            // 
            this.serverTreeImageRemoveMenu.BackColor = System.Drawing.SystemColors.Menu;
            this.serverTreeImageRemoveMenu.ForeColor = System.Drawing.SystemColors.MenuText;
            this.serverTreeImageRemoveMenu.Name = "serverTreeImageRemoveMenu";
            this.serverTreeImageRemoveMenu.Size = new System.Drawing.Size(253, 22);
            this.serverTreeImageRemoveMenu.Text = "Remove Background Image";
            this.serverTreeImageRemoveMenu.Click += new System.EventHandler(this.serverTreeImageRemoveMenu_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.BackColor = System.Drawing.SystemColors.MenuBar;
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkForUpdateToolStripMenuItem,
            this.toolStripMenuItem1,
            this.browseDataFolderToolStripMenuItem,
            this.browsePluginsFolderToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // checkForUpdateToolStripMenuItem
            // 
            this.checkForUpdateToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.checkForUpdateToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.checkForUpdateToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("checkForUpdateToolStripMenuItem.Image")));
            this.checkForUpdateToolStripMenuItem.Name = "checkForUpdateToolStripMenuItem";
            this.checkForUpdateToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.checkForUpdateToolStripMenuItem.Text = "Check for Update";
            this.checkForUpdateToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdateToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.BackColor = System.Drawing.SystemColors.Menu;
            this.toolStripMenuItem1.ForeColor = System.Drawing.SystemColors.MenuText;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(215, 6);
            // 
            // browseDataFolderToolStripMenuItem
            // 
            this.browseDataFolderToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.browseDataFolderToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.browseDataFolderToolStripMenuItem.Name = "browseDataFolderToolStripMenuItem";
            this.browseDataFolderToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.browseDataFolderToolStripMenuItem.Text = "Browse Data Folder";
            this.browseDataFolderToolStripMenuItem.Click += new System.EventHandler(this.browseDataFolderToolStripMenuItem_Click);
            // 
            // browsePluginsFolderToolStripMenuItem
            // 
            this.browsePluginsFolderToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.browsePluginsFolderToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.browsePluginsFolderToolStripMenuItem.Name = "browsePluginsFolderToolStripMenuItem";
            this.browsePluginsFolderToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.browsePluginsFolderToolStripMenuItem.Text = "Browse Plugins Folder";
            this.browsePluginsFolderToolStripMenuItem.Click += new System.EventHandler(this.browsePluginsFolderToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.aboutToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // closeWindow
            // 
            this.closeWindow.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.closeWindow.BackColor = System.Drawing.SystemColors.MenuBar;
            this.closeWindow.Image = ((System.Drawing.Image)(resources.GetObject("closeWindow.Image")));
            this.closeWindow.Name = "closeWindow";
            this.closeWindow.Size = new System.Drawing.Size(28, 20);
            this.closeWindow.Text = null;
            this.closeWindow.ToolTipText = "Close Current Window";
            this.closeWindow.Click += new System.EventHandler(this.closeWindow_Click);
            // 
            // toolStripUpdate
            // 
            this.toolStripUpdate.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripUpdate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripUpdate.Image = global::IceChat.Properties.Resources.update;
            this.toolStripUpdate.Name = "toolStripUpdate";
            this.toolStripUpdate.Size = new System.Drawing.Size(28, 20);
            this.toolStripUpdate.Text = "Update!";
            this.toolStripUpdate.Visible = false;
            this.toolStripUpdate.Click += new System.EventHandler(this.toolStripUpdate_Click);
            // 
            // splitterLeft
            // 
            this.splitterLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.splitterLeft.Location = new System.Drawing.Point(200, 55);
            this.splitterLeft.Name = "splitterLeft";
            this.splitterLeft.Size = new System.Drawing.Size(3, 470);
            this.splitterLeft.TabIndex = 15;
            this.splitterLeft.TabStop = false;
            // 
            // splitterRight
            // 
            this.splitterRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.splitterRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitterRight.Location = new System.Drawing.Point(701, 55);
            this.splitterRight.Name = "splitterRight";
            this.splitterRight.Size = new System.Drawing.Size(3, 470);
            this.splitterRight.TabIndex = 16;
            this.splitterRight.TabStop = false;
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon.BalloonTipTitle = "IceChat";
            this.notifyIcon.ContextMenuStrip = this.contextMenuNotify;
            this.notifyIcon.Text = "IceChat";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIconMouseDoubleClick);
            // 
            // contextMenuNotify
            // 
            this.contextMenuNotify.BackColor = System.Drawing.SystemColors.MenuBar;
            this.contextMenuNotify.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.restoreToolStripMenuItem,
            this.exitToolStripMenuItem1});
            this.contextMenuNotify.Name = "contextMenuNotify";
            this.contextMenuNotify.Size = new System.Drawing.Size(114, 48);
            // 
            // restoreToolStripMenuItem
            // 
            this.restoreToolStripMenuItem.BackColor = System.Drawing.SystemColors.Menu;
            this.restoreToolStripMenuItem.ForeColor = System.Drawing.SystemColors.MenuText;
            this.restoreToolStripMenuItem.Name = "restoreToolStripMenuItem";
            this.restoreToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.restoreToolStripMenuItem.Text = "Restore";
            this.restoreToolStripMenuItem.Click += new System.EventHandler(this.restoreToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem1
            // 
            this.exitToolStripMenuItem1.BackColor = System.Drawing.SystemColors.Menu;
            this.exitToolStripMenuItem1.ForeColor = System.Drawing.SystemColors.MenuText;
            this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            this.exitToolStripMenuItem1.Size = new System.Drawing.Size(113, 22);
            this.exitToolStripMenuItem1.Text = "Exit";
            this.exitToolStripMenuItem1.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
            // 
            // contextMenuToolBar
            // 
            this.contextMenuToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hideToolStripMenuItem});
            this.contextMenuToolBar.Name = "contextMenuToolBar";
            this.contextMenuToolBar.Size = new System.Drawing.Size(100, 26);
            // 
            // hideToolStripMenuItem
            // 
            this.hideToolStripMenuItem.Name = "hideToolStripMenuItem";
            this.hideToolStripMenuItem.Size = new System.Drawing.Size(99, 22);
            this.hideToolStripMenuItem.Text = "Hide";
            // 
            // toolStripStatus
            // 
            this.toolStripStatus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatus.Name = "toolStripStatus";
            this.toolStripStatus.Size = new System.Drawing.Size(58, 17);
            this.toolStripStatus.Text = "Status:";
            // 
            // statusStripMain
            // 
            this.statusStripMain.AccessibleDescription = "Main status bar";
            this.statusStripMain.AccessibleRole = System.Windows.Forms.AccessibleRole.StatusBar;
            this.statusStripMain.CanOverflow = true;
            this.statusStripMain.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatus});
            this.statusStripMain.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.statusStripMain.Location = new System.Drawing.Point(0, 551);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Padding = new System.Windows.Forms.Padding(1, 0, 15, 0);
            this.statusStripMain.Size = new System.Drawing.Size(924, 22);
            this.statusStripMain.SizingGrip = false;
            this.statusStripMain.TabIndex = 18;
            // 
            // splitterBottom
            // 
            this.splitterBottom.BackColor = System.Drawing.Color.Red;
            this.splitterBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitterBottom.Location = new System.Drawing.Point(0, 573);
            this.splitterBottom.Name = "splitterBottom";
            this.splitterBottom.Size = new System.Drawing.Size(924, 3);
            this.splitterBottom.TabIndex = 0;
            this.splitterBottom.TabStop = false;
            this.splitterBottom.Visible = false;
            // 
            // nickListTab
            // 
            this.nickListTab.Controls.Add(this.nickPanel);
            this.nickListTab.Location = new System.Drawing.Point(4, 4);
            this.nickListTab.Name = "nickListTab";
            this.nickListTab.Size = new System.Drawing.Size(192, 454);
            this.nickListTab.TabIndex = 0;
            this.nickListTab.Text = "Nick List";
            // 
            // nickPanel
            // 
            this.nickPanel.Controls.Add(this.nickList);
            this.nickPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nickPanel.Location = new System.Drawing.Point(0, 0);
            this.nickPanel.Name = "nickPanel";
            this.nickPanel.Size = new System.Drawing.Size(192, 454);
            this.nickPanel.TabIndex = 0;
            // 
            // nickList
            // 
            this.nickList.AccessibleDescription = "List of Nick Names";
            this.nickList.AccessibleRole = System.Windows.Forms.AccessibleRole.Pane;
            this.nickList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nickList.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nickList.Header = "";
            this.nickList.Location = new System.Drawing.Point(0, 0);
            this.nickList.Margin = new System.Windows.Forms.Padding(4);
            this.nickList.Name = "nickList";
            this.nickList.Size = new System.Drawing.Size(192, 454);
            this.nickList.TabIndex = 0;
            // 
            // serverListTab
            // 
            this.serverListTab.Controls.Add(this.serverPanel);
            this.serverListTab.Location = new System.Drawing.Point(24, 4);
            this.serverListTab.Name = "serverListTab";
            this.serverListTab.Size = new System.Drawing.Size(172, 454);
            this.serverListTab.TabIndex = 0;
            this.serverListTab.Text = "Favorite Servers";
            // 
            // serverPanel
            // 
            this.serverPanel.Controls.Add(this.serverTree);
            this.serverPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.serverPanel.Location = new System.Drawing.Point(0, 0);
            this.serverPanel.Name = "serverPanel";
            this.serverPanel.Size = new System.Drawing.Size(172, 454);
            this.serverPanel.TabIndex = 0;
            // 
            // serverTree
            // 
            this.serverTree.AccessibleDescription = "List of servers and channels associated with them once connected";
            this.serverTree.AccessibleRole = System.Windows.Forms.AccessibleRole.Pane;
            this.serverTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.serverTree.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serverTree.Location = new System.Drawing.Point(0, 0);
            this.serverTree.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.serverTree.Name = "serverTree";
            this.serverTree.Size = new System.Drawing.Size(172, 454);
            this.serverTree.TabIndex = 0;
            // 
            // mainTabControl
            // 
            this.mainTabControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Location = new System.Drawing.Point(203, 55);
            this.mainTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.Size = new System.Drawing.Size(498, 470);
            this.mainTabControl.TabIndex = 20;
            // 
            // panelDockRight
            // 
            this.panelDockRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelDockRight.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelDockRight.Location = new System.Drawing.Point(704, 55);
            this.panelDockRight.Name = "panelDockRight";
            this.panelDockRight.Size = new System.Drawing.Size(220, 470);
            this.panelDockRight.TabIndex = 14;
            // 
            // panelDockLeft
            // 
            this.panelDockLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelDockLeft.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelDockLeft.Location = new System.Drawing.Point(0, 55);
            this.panelDockLeft.Name = "panelDockLeft";
            this.panelDockLeft.Size = new System.Drawing.Size(200, 470);
            this.panelDockLeft.TabIndex = 13;
            // 
            // inputPanel
            // 
            this.inputPanel.AccessibleDescription = "";
            this.inputPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.inputPanel.Location = new System.Drawing.Point(0, 525);
            this.inputPanel.Margin = new System.Windows.Forms.Padding(4);
            this.inputPanel.Name = "inputPanel";
            this.inputPanel.Size = new System.Drawing.Size(924, 26);
            this.inputPanel.TabIndex = 0;
            // 
            // mainChannelBar
            // 
            this.mainChannelBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.mainChannelBar.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mainChannelBar.Location = new System.Drawing.Point(0, 24);
            this.mainChannelBar.Name = "mainChannelBar";
            this.mainChannelBar.SelectedIndex = -1;
            this.mainChannelBar.Size = new System.Drawing.Size(924, 31);
            this.mainChannelBar.TabIndex = 24;
            // 
            // editServerToolStripMenuItem
            // 
            this.editServerToolStripMenuItem.Name = "editServerToolStripMenuItem";
            this.editServerToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.editServerToolStripMenuItem.Text = "Edit Server";
            this.editServerToolStripMenuItem.Click += new System.EventHandler(this.editServerToolStripMenuItem_Click);
            // 
            // FormMain
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(924, 576);
            this.Controls.Add(this.mainTabControl);
            this.Controls.Add(this.splitterRight);
            this.Controls.Add(this.splitterLeft);
            this.Controls.Add(this.panelDockRight);
            this.Controls.Add(this.panelDockLeft);
            this.Controls.Add(this.inputPanel);
            this.Controls.Add(this.statusStripMain);
            this.Controls.Add(this.mainChannelBar);
            this.Controls.Add(this.menuMainStrip);
            this.Controls.Add(this.splitterBottom);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuMainStrip;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "PonyChat";
            this.menuMainStrip.ResumeLayout(false);
            this.menuMainStrip.PerformLayout();
            this.contextMenuNotify.ResumeLayout(false);
            this.contextMenuToolBar.ResumeLayout(false);
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.nickListTab.ResumeLayout(false);
            this.nickPanel.ResumeLayout(false);
            this.serverListTab.ResumeLayout(false);
            this.serverPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        internal System.Windows.Forms.ToolStripMenuItem debugWindowToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuNotify;
        private System.Windows.Forms.ToolStripMenuItem restoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem pluginsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem serverListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nickListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem statusBarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdateToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem browseDataFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem browsePluginsFolderToolStripMenuItem;        
        internal System.Windows.Forms.ToolStripMenuItem closeCurrentWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem selectNickListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectServerTreeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem channelBarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem muteAllSoundsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadAPluginToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatus;
        private System.Windows.Forms.StatusStrip statusStripMain;
        private System.Windows.Forms.Splitter splitterBottom;
        public System.Windows.Forms.ToolStripMenuItem multilineEditboxToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem themesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem channelListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuStylesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem vS2008ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem office2007ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultToolStripMenuItem1;

        
        //added
        private System.Windows.Forms.ToolStripMenuItem nickListImageMenu;
        private System.Windows.Forms.ToolStripMenuItem serverTreeImageMenu;
        private System.Windows.Forms.ToolStripMenuItem nickListImageRemoveMenu;
        private System.Windows.Forms.ToolStripMenuItem serverTreeImageRemoveMenu;


        internal System.Windows.Forms.Splitter splitterLeft;
        internal System.Windows.Forms.Splitter splitterRight;

        private IceDockPanel panelDockLeft;
        private IceDockPanel panelDockRight;
        private ServerTree serverTree;
        private ChannelList channelList;
        private BuddyList buddyList;
        private NickList nickList;
        
        private Panel serverPanel;
        private Panel nickPanel;

        private TabPage nickListTab;
        private TabPage serverListTab;
        private TabPage channelListTab;
        private TabPage buddyListTab;
        private ToolStripMenuItem closeWindow;
        private ChannelBar mainChannelBar;
        internal ToolStripMenuItem fontSettingsToolStripMenuItem;
        private ToolStripMenuItem toolStripUpdate;
        private ToolStripMenuItem editServerToolStripMenuItem;

    }

}

