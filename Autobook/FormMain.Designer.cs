namespace Autobook
{
  partial class FormMain
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Timer timerCheckMarkets;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose (bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose ();
      }
      base.Dispose (disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent ()
    {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.timerCheckMarkets = new System.Windows.Forms.Timer(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonAddMarket = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonAddMarket2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonSettings = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonWalletsRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelUKWallet = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelPoints = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonResults = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripButtonNetworkStatus = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelApiFailCount = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelApiErrorMsg = new System.Windows.Forms.ToolStripLabel();
            this.panelSidebar = new System.Windows.Forms.Panel();
            this.listViewMarkets = new System.Windows.Forms.ListView();
            this.columnHeaderTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMarket = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.labelMarketsHeader = new System.Windows.Forms.Label();
            this.panelContent = new System.Windows.Forms.Panel();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.panelMarketView = new System.Windows.Forms.Panel();
            this.panelBets = new System.Windows.Forms.Panel();
            this.listViewBets = new System.Windows.Forms.ListView();
            this.columnHeaderRunner = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBetType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBetOdds = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBetSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBetTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.labelBetsHeader = new System.Windows.Forms.Label();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.panelSidebar.SuspendLayout();
            this.panelContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panelBets.SuspendLayout();
            this.SuspendLayout();
            //
            // timerCheckMarkets
            //
            this.timerCheckMarkets.Interval = 5000;
            this.timerCheckMarkets.Tick += new System.EventHandler(this.timerCheckMarkets_Tick);
            //
            // toolStrip1
            //
            this.toolStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonAddMarket,
            this.toolStripSeparator1,
            this.toolStripButtonAddMarket2,
            this.toolStripSeparator2,
            this.toolStripButtonSettings,
            this.toolStripSeparator3,
            this.toolStripButtonWalletsRefresh,
            this.toolStripSeparator5,
            this.toolStripLabelUKWallet,
            this.toolStripSeparator8,
            this.toolStripLabelPoints,
            this.toolStripSeparator9,
            this.toolStripButtonResults});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(8, 0, 1, 0);
            this.toolStrip1.Size = new System.Drawing.Size(1400, 35);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            //
            // toolStripButtonAddMarket
            //
            this.toolStripButtonAddMarket.ForeColor = System.Drawing.Color.White;
            this.toolStripButtonAddMarket.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonAddMarket.Image")));
            this.toolStripButtonAddMarket.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonAddMarket.Name = "toolStripButtonAddMarket";
            this.toolStripButtonAddMarket.Size = new System.Drawing.Size(89, 32);
            this.toolStripButtonAddMarket.Text = "Add Market";
            this.toolStripButtonAddMarket.Click += new System.EventHandler(this.toolStripButtonAddMarket_Click);
            //
            // toolStripSeparator1
            //
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 35);
            //
            // toolStripButtonAddMarket2
            //
            this.toolStripButtonAddMarket2.ForeColor = System.Drawing.Color.White;
            this.toolStripButtonAddMarket2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonAddMarket2.Image")));
            this.toolStripButtonAddMarket2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonAddMarket2.Name = "toolStripButtonAddMarket2";
            this.toolStripButtonAddMarket2.Size = new System.Drawing.Size(123, 32);
            this.toolStripButtonAddMarket2.Text = "Add Auto Markets";
            this.toolStripButtonAddMarket2.Click += new System.EventHandler(this.toolStripButtonAddMarket2_Click);
            //
            // toolStripSeparator2
            //
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 35);
            //
            // toolStripButtonSettings
            //
            this.toolStripButtonSettings.ForeColor = System.Drawing.Color.White;
            this.toolStripButtonSettings.Image = global::Autobook.Properties.Resources.Preferences;
            this.toolStripButtonSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSettings.Name = "toolStripButtonSettings";
            this.toolStripButtonSettings.Size = new System.Drawing.Size(69, 32);
            this.toolStripButtonSettings.Text = "Settings";
            this.toolStripButtonSettings.Click += new System.EventHandler(this.toolStripButtonSettings_Click);
            //
            // toolStripSeparator3
            //
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 35);
            //
            // toolStripButtonWalletsRefresh
            //
            this.toolStripButtonWalletsRefresh.ForeColor = System.Drawing.Color.White;
            this.toolStripButtonWalletsRefresh.Image = global::Autobook.Properties.Resources.refresh;
            this.toolStripButtonWalletsRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonWalletsRefresh.Name = "toolStripButtonWalletsRefresh";
            this.toolStripButtonWalletsRefresh.Size = new System.Drawing.Size(107, 32);
            this.toolStripButtonWalletsRefresh.Text = "Refresh Wallets";
            this.toolStripButtonWalletsRefresh.Click += new System.EventHandler(this.toolStripButtonWalletsRefresh_Click);
            //
            // toolStripSeparator5
            //
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 35);
            //
            // toolStripLabelUKWallet
            //
            this.toolStripLabelUKWallet.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripLabelUKWallet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.toolStripLabelUKWallet.Name = "toolStripLabelUKWallet";
            this.toolStripLabelUKWallet.Size = new System.Drawing.Size(64, 32);
            this.toolStripLabelUKWallet.Text = "UK Wallet";
            //
            // toolStripSeparator8
            //
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 35);
            //
            // toolStripLabelPoints
            //
            this.toolStripLabelPoints.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripLabelPoints.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.toolStripLabelPoints.Name = "toolStripLabelPoints";
            this.toolStripLabelPoints.Size = new System.Drawing.Size(43, 32);
            this.toolStripLabelPoints.Text = "Points";
            //
            // toolStripSeparator9
            //
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 35);
            //
            // toolStripButtonResults
            //
            this.toolStripButtonResults.ForeColor = System.Drawing.Color.White;
            this.toolStripButtonResults.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonResults.Name = "toolStripButtonResults";
            this.toolStripButtonResults.Size = new System.Drawing.Size(52, 32);
            this.toolStripButtonResults.Text = "Results";
            this.toolStripButtonResults.Click += new System.EventHandler(this.toolStripButtonResults_Click);
            //
            // statusStrip1
            //
            this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonNetworkStatus,
            this.toolStripSeparator7,
            this.toolStripLabelApiFailCount,
            this.toolStripSeparator6,
            this.toolStripLabelApiErrorMsg});
            this.statusStrip1.Location = new System.Drawing.Point(0, 811);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1400, 28);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            //
            // toolStripButtonNetworkStatus
            //
            this.toolStripButtonNetworkStatus.ForeColor = System.Drawing.Color.White;
            this.toolStripButtonNetworkStatus.Image = global::Autobook.Properties.Resources.Knob_Red;
            this.toolStripButtonNetworkStatus.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonNetworkStatus.Name = "toolStripButtonNetworkStatus";
            this.toolStripButtonNetworkStatus.Size = new System.Drawing.Size(106, 26);
            this.toolStripButtonNetworkStatus.Text = "Network Down";
            //
            // toolStripSeparator7
            //
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 28);
            //
            // toolStripLabelApiFailCount
            //
            this.toolStripLabelApiFailCount.ForeColor = System.Drawing.Color.White;
            this.toolStripLabelApiFailCount.Name = "toolStripLabelApiFailCount";
            this.toolStripLabelApiFailCount.Size = new System.Drawing.Size(89, 26);
            this.toolStripLabelApiFailCount.Text = "API Errors = 0";
            //
            // toolStripSeparator6
            //
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 28);
            //
            // toolStripLabelApiErrorMsg
            //
            this.toolStripLabelApiErrorMsg.ForeColor = System.Drawing.Color.White;
            this.toolStripLabelApiErrorMsg.Name = "toolStripLabelApiErrorMsg";
            this.toolStripLabelApiErrorMsg.Size = new System.Drawing.Size(0, 26);
            //
            // panelSidebar
            //
            this.panelSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panelSidebar.Controls.Add(this.listViewMarkets);
            this.panelSidebar.Controls.Add(this.labelMarketsHeader);
            this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSidebar.Location = new System.Drawing.Point(0, 35);
            this.panelSidebar.Name = "panelSidebar";
            this.panelSidebar.Size = new System.Drawing.Size(280, 776);
            this.panelSidebar.TabIndex = 2;
            //
            // listViewMarkets
            //
            this.listViewMarkets.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.listViewMarkets.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewMarkets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderTime,
            this.columnHeaderMarket});
            this.listViewMarkets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewMarkets.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.listViewMarkets.ForeColor = System.Drawing.Color.White;
            this.listViewMarkets.FullRowSelect = true;
            this.listViewMarkets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewMarkets.HideSelection = false;
            this.listViewMarkets.Location = new System.Drawing.Point(0, 40);
            this.listViewMarkets.MultiSelect = false;
            this.listViewMarkets.Name = "listViewMarkets";
            this.listViewMarkets.OwnerDraw = true;
            this.listViewMarkets.Size = new System.Drawing.Size(280, 736);
            this.listViewMarkets.TabIndex = 1;
            this.listViewMarkets.UseCompatibleStateImageBehavior = false;
            this.listViewMarkets.View = System.Windows.Forms.View.Details;
            this.listViewMarkets.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listViewMarkets_DrawColumnHeader);
            this.listViewMarkets.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listViewMarkets_DrawItem);
            this.listViewMarkets.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listViewMarkets_DrawSubItem);
            this.listViewMarkets.SelectedIndexChanged += new System.EventHandler(this.listViewMarkets_SelectedIndexChanged);
            //
            // columnHeaderTime
            //
            this.columnHeaderTime.Text = "Time";
            this.columnHeaderTime.Width = 55;
            //
            // columnHeaderMarket
            //
            this.columnHeaderMarket.Text = "Market";
            this.columnHeaderMarket.Width = 220;
            //
            // labelMarketsHeader
            //
            this.labelMarketsHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.labelMarketsHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelMarketsHeader.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelMarketsHeader.ForeColor = System.Drawing.Color.White;
            this.labelMarketsHeader.Location = new System.Drawing.Point(0, 0);
            this.labelMarketsHeader.Name = "labelMarketsHeader";
            this.labelMarketsHeader.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.labelMarketsHeader.Size = new System.Drawing.Size(280, 40);
            this.labelMarketsHeader.TabIndex = 0;
            this.labelMarketsHeader.Text = "MARKETS";
            this.labelMarketsHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // panelContent
            //
            this.panelContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.panelContent.Controls.Add(this.splitContainer);
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Location = new System.Drawing.Point(280, 35);
            this.panelContent.Name = "panelContent";
            this.panelContent.Padding = new System.Windows.Forms.Padding(1);
            this.panelContent.Size = new System.Drawing.Size(1120, 776);
            this.panelContent.TabIndex = 3;
            //
            // splitContainer
            //
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer.Location = new System.Drawing.Point(1, 1);
            this.splitContainer.Name = "splitContainer";
            //
            // splitContainer.Panel1
            //
            this.splitContainer.Panel1.BackColor = System.Drawing.Color.White;
            this.splitContainer.Panel1.Controls.Add(this.panelMarketView);
            this.splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(0);
            //
            // splitContainer.Panel2
            //
            this.splitContainer.Panel2.Controls.Add(this.panelBets);
            this.splitContainer.Size = new System.Drawing.Size(1118, 774);
            this.splitContainer.SplitterDistance = 738;
            this.splitContainer.SplitterWidth = 1;
            this.splitContainer.TabIndex = 0;
            //
            // panelMarketView
            //
            this.panelMarketView.BackColor = System.Drawing.Color.White;
            this.panelMarketView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMarketView.Location = new System.Drawing.Point(0, 0);
            this.panelMarketView.Name = "panelMarketView";
            this.panelMarketView.Size = new System.Drawing.Size(738, 774);
            this.panelMarketView.TabIndex = 0;
            //
            // panelBets
            //
            this.panelBets.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panelBets.Controls.Add(this.listViewBets);
            this.panelBets.Controls.Add(this.labelBetsHeader);
            this.panelBets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBets.Location = new System.Drawing.Point(0, 0);
            this.panelBets.Name = "panelBets";
            this.panelBets.Size = new System.Drawing.Size(379, 774);
            this.panelBets.TabIndex = 0;
            //
            // listViewBets
            //
            this.listViewBets.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.listViewBets.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewBets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderRunner,
            this.columnHeaderBetType,
            this.columnHeaderStatus,
            this.columnHeaderBetOdds,
            this.columnHeaderBetSize,
            this.columnHeaderBetTime});
            this.listViewBets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewBets.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.listViewBets.ForeColor = System.Drawing.Color.White;
            this.listViewBets.FullRowSelect = true;
            this.listViewBets.GridLines = true;
            this.listViewBets.HideSelection = false;
            this.listViewBets.Location = new System.Drawing.Point(0, 40);
            this.listViewBets.Name = "listViewBets";
            this.listViewBets.OwnerDraw = true;
            this.listViewBets.Size = new System.Drawing.Size(379, 734);
            this.listViewBets.TabIndex = 1;
            this.listViewBets.UseCompatibleStateImageBehavior = false;
            this.listViewBets.View = System.Windows.Forms.View.Details;
            this.listViewBets.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listViewBets_DrawColumnHeader);
            this.listViewBets.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listViewBets_DrawItem);
            this.listViewBets.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listViewBets_DrawSubItem);
            //
            // columnHeaderRunner
            //
            this.columnHeaderRunner.Text = "Runner";
            this.columnHeaderRunner.Width = 80;
            //
            // columnHeaderBetType
            //
            this.columnHeaderBetType.Text = "Type";
            this.columnHeaderBetType.Width = 40;
            //
            // columnHeaderStatus
            //
            this.columnHeaderStatus.Text = "Status";
            this.columnHeaderStatus.Width = 70;
            //
            // columnHeaderBetOdds
            //
            this.columnHeaderBetOdds.Text = "Odds";
            this.columnHeaderBetOdds.Width = 50;
            //
            // columnHeaderBetSize
            //
            this.columnHeaderBetSize.Text = "Size";
            this.columnHeaderBetSize.Width = 50;
            //
            // columnHeaderBetTime
            //
            this.columnHeaderBetTime.Text = "Time";
            this.columnHeaderBetTime.Width = 85;
            //
            // labelBetsHeader
            //
            this.labelBetsHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.labelBetsHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelBetsHeader.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelBetsHeader.ForeColor = System.Drawing.Color.White;
            this.labelBetsHeader.Location = new System.Drawing.Point(0, 0);
            this.labelBetsHeader.Name = "labelBetsHeader";
            this.labelBetsHeader.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.labelBetsHeader.Size = new System.Drawing.Size(379, 40);
            this.labelBetsHeader.TabIndex = 0;
            this.labelBetsHeader.Text = "BETS";
            this.labelBetsHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // FormMain
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(1400, 839);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panelSidebar);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Autobook - Modern";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panelSidebar.ResumeLayout(false);
            this.panelContent.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.panelBets.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.ToolStripButton toolStripButtonAddMarket;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripButton toolStripButtonAddMarket2;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    private System.Windows.Forms.ToolStripButton toolStripButtonSettings;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    private System.Windows.Forms.ToolStripButton toolStripButtonWalletsRefresh;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
    private System.Windows.Forms.ToolStripLabel toolStripLabelUKWallet;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
    private System.Windows.Forms.ToolStripLabel toolStripLabelPoints;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
    private System.Windows.Forms.ToolStripButton toolStripButtonResults;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripButton toolStripButtonNetworkStatus;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
    private System.Windows.Forms.ToolStripLabel toolStripLabelApiFailCount;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
    private System.Windows.Forms.ToolStripLabel toolStripLabelApiErrorMsg;
    private System.Windows.Forms.Panel panelSidebar;
    private System.Windows.Forms.ListView listViewMarkets;
    private System.Windows.Forms.ColumnHeader columnHeaderTime;
    private System.Windows.Forms.ColumnHeader columnHeaderMarket;
    private System.Windows.Forms.Label labelMarketsHeader;
    private System.Windows.Forms.Panel panelContent;
    private System.Windows.Forms.SplitContainer splitContainer;
    private System.Windows.Forms.Panel panelMarketView;
    private System.Windows.Forms.Panel panelBets;
    private System.Windows.Forms.ListView listViewBets;
    private System.Windows.Forms.ColumnHeader columnHeaderRunner;
    private System.Windows.Forms.ColumnHeader columnHeaderBetType;
    private System.Windows.Forms.ColumnHeader columnHeaderStatus;
    private System.Windows.Forms.ColumnHeader columnHeaderBetOdds;
    private System.Windows.Forms.ColumnHeader columnHeaderBetSize;
    private System.Windows.Forms.ColumnHeader columnHeaderBetTime;
    private System.Windows.Forms.Label labelBetsHeader;
  }
}
