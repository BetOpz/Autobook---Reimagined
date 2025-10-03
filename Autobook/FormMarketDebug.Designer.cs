namespace Autobook
{
    partial class FormMarketDebug
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMarketDebug));
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.listViewInternals = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageInternals = new System.Windows.Forms.TabPage();
            this.tabPagePAndL = new System.Windows.Forms.TabPage();
            this.listViewPAndL = new System.Windows.Forms.ListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPageSmallLay = new System.Windows.Forms.TabPage();
            this.listViewSmallLays = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPageSmallBacks = new System.Windows.Forms.TabPage();
            this.listViewSmallBacks = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.comboBoxRefreshRate = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonStartRefresh = new System.Windows.Forms.Button();
            this.buttonStopRefresh = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPageInternals.SuspendLayout();
            this.tabPagePAndL.SuspendLayout();
            this.tabPageSmallLay.SuspendLayout();
            this.tabPageSmallBacks.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(265, 537);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 2;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Location = new System.Drawing.Point(697, 9);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(75, 23);
            this.buttonRefresh.TabIndex = 3;
            this.buttonRefresh.Text = "&Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // listViewInternals
            // 
            this.listViewInternals.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderValue});
            this.listViewInternals.GridLines = true;
            this.listViewInternals.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewInternals.Location = new System.Drawing.Point(6, 6);
            this.listViewInternals.Name = "listViewInternals";
            this.listViewInternals.Size = new System.Drawing.Size(750, 455);
            this.listViewInternals.TabIndex = 4;
            this.listViewInternals.UseCompatibleStateImageBehavior = false;
            this.listViewInternals.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 372;
            // 
            // columnHeaderValue
            // 
            this.columnHeaderValue.Text = "Value";
            this.columnHeaderValue.Width = 372;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageInternals);
            this.tabControl1.Controls.Add(this.tabPagePAndL);
            this.tabControl1.Controls.Add(this.tabPageSmallLay);
            this.tabControl1.Controls.Add(this.tabPageSmallBacks);
            this.tabControl1.Location = new System.Drawing.Point(12, 41);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(770, 490);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPageInternals
            // 
            this.tabPageInternals.Controls.Add(this.listViewInternals);
            this.tabPageInternals.Location = new System.Drawing.Point(4, 22);
            this.tabPageInternals.Name = "tabPageInternals";
            this.tabPageInternals.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInternals.Size = new System.Drawing.Size(762, 464);
            this.tabPageInternals.TabIndex = 0;
            this.tabPageInternals.Text = "Internals";
            this.tabPageInternals.UseVisualStyleBackColor = true;
            // 
            // tabPagePAndL
            // 
            this.tabPagePAndL.Controls.Add(this.listViewPAndL);
            this.tabPagePAndL.Location = new System.Drawing.Point(4, 22);
            this.tabPagePAndL.Name = "tabPagePAndL";
            this.tabPagePAndL.Size = new System.Drawing.Size(762, 464);
            this.tabPagePAndL.TabIndex = 3;
            this.tabPagePAndL.Text = "Profits & Loss";
            this.tabPagePAndL.UseVisualStyleBackColor = true;
            // 
            // listViewPAndL
            // 
            this.listViewPAndL.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6});
            this.listViewPAndL.GridLines = true;
            this.listViewPAndL.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewPAndL.Location = new System.Drawing.Point(6, 6);
            this.listViewPAndL.Name = "listViewPAndL";
            this.listViewPAndL.Size = new System.Drawing.Size(750, 481);
            this.listViewPAndL.TabIndex = 6;
            this.listViewPAndL.UseCompatibleStateImageBehavior = false;
            this.listViewPAndL.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Selection Name";
            this.columnHeader5.Width = 372;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Amount";
            this.columnHeader6.Width = 372;
            // 
            // tabPageSmallLay
            // 
            this.tabPageSmallLay.Controls.Add(this.listViewSmallLays);
            this.tabPageSmallLay.Location = new System.Drawing.Point(4, 22);
            this.tabPageSmallLay.Name = "tabPageSmallLay";
            this.tabPageSmallLay.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSmallLay.Size = new System.Drawing.Size(762, 464);
            this.tabPageSmallLay.TabIndex = 1;
            this.tabPageSmallLay.Text = "Small Lays";
            this.tabPageSmallLay.UseVisualStyleBackColor = true;
            // 
            // listViewSmallLays
            // 
            this.listViewSmallLays.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewSmallLays.GridLines = true;
            this.listViewSmallLays.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewSmallLays.Location = new System.Drawing.Point(6, 6);
            this.listViewSmallLays.Name = "listViewSmallLays";
            this.listViewSmallLays.Size = new System.Drawing.Size(750, 481);
            this.listViewSmallLays.TabIndex = 5;
            this.listViewSmallLays.UseCompatibleStateImageBehavior = false;
            this.listViewSmallLays.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Selection Id";
            this.columnHeader1.Width = 372;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Amount";
            this.columnHeader2.Width = 372;
            // 
            // tabPageSmallBacks
            // 
            this.tabPageSmallBacks.Controls.Add(this.listViewSmallBacks);
            this.tabPageSmallBacks.Location = new System.Drawing.Point(4, 22);
            this.tabPageSmallBacks.Name = "tabPageSmallBacks";
            this.tabPageSmallBacks.Size = new System.Drawing.Size(762, 464);
            this.tabPageSmallBacks.TabIndex = 2;
            this.tabPageSmallBacks.Text = "Small Backs";
            this.tabPageSmallBacks.UseVisualStyleBackColor = true;
            // 
            // listViewSmallBacks
            // 
            this.listViewSmallBacks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
            this.listViewSmallBacks.GridLines = true;
            this.listViewSmallBacks.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewSmallBacks.Location = new System.Drawing.Point(6, 6);
            this.listViewSmallBacks.Name = "listViewSmallBacks";
            this.listViewSmallBacks.Size = new System.Drawing.Size(750, 481);
            this.listViewSmallBacks.TabIndex = 6;
            this.listViewSmallBacks.UseCompatibleStateImageBehavior = false;
            this.listViewSmallBacks.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Selection Id";
            this.columnHeader3.Width = 372;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Amount";
            this.columnHeader4.Width = 372;
            // 
            // comboBoxRefreshRate
            // 
            this.comboBoxRefreshRate.FormattingEnabled = true;
            this.comboBoxRefreshRate.Items.AddRange(new object[] {
            "0.1 sec",
            "0.2 sec",
            "0.5 sec",
            "1 sec",
            "2 sec",
            "5 sec",
            "10 sec",
            "30 sec"});
            this.comboBoxRefreshRate.Location = new System.Drawing.Point(88, 6);
            this.comboBoxRefreshRate.Name = "comboBoxRefreshRate";
            this.comboBoxRefreshRate.Size = new System.Drawing.Size(109, 21);
            this.comboBoxRefreshRate.TabIndex = 33;
            this.comboBoxRefreshRate.SelectedIndexChanged += new System.EventHandler(this.comboBoxRefreshRate_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 13);
            this.label6.TabIndex = 34;
            this.label6.Text = "Refresh Rate:";
            // 
            // buttonStartRefresh
            // 
            this.buttonStartRefresh.Location = new System.Drawing.Point(214, 4);
            this.buttonStartRefresh.Name = "buttonStartRefresh";
            this.buttonStartRefresh.Size = new System.Drawing.Size(75, 23);
            this.buttonStartRefresh.TabIndex = 36;
            this.buttonStartRefresh.Text = "St&art";
            this.buttonStartRefresh.UseVisualStyleBackColor = true;
            this.buttonStartRefresh.Click += new System.EventHandler(this.buttonStartRefresh_Click);
            // 
            // buttonStopRefresh
            // 
            this.buttonStopRefresh.Enabled = false;
            this.buttonStopRefresh.Location = new System.Drawing.Point(295, 4);
            this.buttonStopRefresh.Name = "buttonStopRefresh";
            this.buttonStopRefresh.Size = new System.Drawing.Size(75, 23);
            this.buttonStopRefresh.TabIndex = 37;
            this.buttonStopRefresh.Text = "St&op";
            this.buttonStopRefresh.UseVisualStyleBackColor = true;
            this.buttonStopRefresh.Click += new System.EventHandler(this.buttonStopRefresh_Click);
            // 
            // FormMarketDebug
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 572);
            this.Controls.Add(this.buttonStopRefresh);
            this.Controls.Add(this.buttonStartRefresh);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.comboBoxRefreshRate);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.buttonClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMarketDebug";
            this.Text = "Debug";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMarketDebug_FormClosed);
            this.tabControl1.ResumeLayout(false);
            this.tabPageInternals.ResumeLayout(false);
            this.tabPagePAndL.ResumeLayout(false);
            this.tabPageSmallLay.ResumeLayout(false);
            this.tabPageSmallBacks.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.ListView listViewInternals;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderValue;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageInternals;
        private System.Windows.Forms.TabPage tabPageSmallLay;
        private System.Windows.Forms.TabPage tabPageSmallBacks;
        private System.Windows.Forms.TabPage tabPagePAndL;
        private System.Windows.Forms.ListView listViewSmallLays;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ListView listViewSmallBacks;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ListView listViewPAndL;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ComboBox comboBoxRefreshRate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonStartRefresh;
        private System.Windows.Forms.Button buttonStopRefresh;
    }
}