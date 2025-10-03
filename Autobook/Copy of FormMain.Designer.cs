namespace Autobook
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
      this.components = new System.ComponentModel.Container ();
      this.panelTop = new System.Windows.Forms.Panel ();
      this.comboBoxRefreshRate = new System.Windows.Forms.ComboBox ();
      this.buttonStartStop = new System.Windows.Forms.Button ();
      this.textBoxUnderOver = new System.Windows.Forms.TextBox ();
      this.label7 = new System.Windows.Forms.Label ();
      this.comboBoxEvent = new System.Windows.Forms.ComboBox ();
      this.label6 = new System.Windows.Forms.Label ();
      this.textBoxLowBack = new System.Windows.Forms.TextBox ();
      this.label5 = new System.Windows.Forms.Label ();
      this.textBoxInRunning = new System.Windows.Forms.TextBox ();
      this.label4 = new System.Windows.Forms.Label ();
      this.textBoxPreRace = new System.Windows.Forms.TextBox ();
      this.label3 = new System.Windows.Forms.Label ();
      this.textBoxLiability = new System.Windows.Forms.TextBox ();
      this.label2 = new System.Windows.Forms.Label ();
      this.textBoxProfit = new System.Windows.Forms.TextBox ();
      this.label1 = new System.Windows.Forms.Label ();
      this.panelBottom = new System.Windows.Forms.Panel ();
      this.panelBets = new System.Windows.Forms.Panel ();
      this.label8 = new System.Windows.Forms.Label ();
      this.listViewBets = new System.Windows.Forms.ListView ();
      this.columnHeaderBetType = new System.Windows.Forms.ColumnHeader ();
      this.columnHeaderBetOdds = new System.Windows.Forms.ColumnHeader ();
      this.columnHeaderBetSize = new System.Windows.Forms.ColumnHeader ();
      this.columnHeaderBetTime = new System.Windows.Forms.ColumnHeader ();
      this.panelRunners = new System.Windows.Forms.Panel ();
      this.label9 = new System.Windows.Forms.Label ();
      this.runnersControl1 = new Autobook.RunnersControl ();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip ();
      this.timerRefresh = new System.Windows.Forms.Timer (this.components);
      this.panelTop.SuspendLayout ();
      this.panelBottom.SuspendLayout ();
      this.panelBets.SuspendLayout ();
      this.panelRunners.SuspendLayout ();
      this.SuspendLayout ();
      // 
      // panelTop
      // 
      this.panelTop.Controls.Add (this.comboBoxRefreshRate);
      this.panelTop.Controls.Add (this.buttonStartStop);
      this.panelTop.Controls.Add (this.textBoxUnderOver);
      this.panelTop.Controls.Add (this.label7);
      this.panelTop.Controls.Add (this.comboBoxEvent);
      this.panelTop.Controls.Add (this.label6);
      this.panelTop.Controls.Add (this.textBoxLowBack);
      this.panelTop.Controls.Add (this.label5);
      this.panelTop.Controls.Add (this.textBoxInRunning);
      this.panelTop.Controls.Add (this.label4);
      this.panelTop.Controls.Add (this.textBoxPreRace);
      this.panelTop.Controls.Add (this.label3);
      this.panelTop.Controls.Add (this.textBoxLiability);
      this.panelTop.Controls.Add (this.label2);
      this.panelTop.Controls.Add (this.textBoxProfit);
      this.panelTop.Controls.Add (this.label1);
      this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
      this.panelTop.Location = new System.Drawing.Point (0, 0);
      this.panelTop.Name = "panelTop";
      this.panelTop.Size = new System.Drawing.Size (1018, 72);
      this.panelTop.TabIndex = 0;
      // 
      // comboBoxRefreshRate
      // 
      this.comboBoxRefreshRate.FormattingEnabled = true;
      this.comboBoxRefreshRate.Items.AddRange (new object[] {
            "0.1 sec",
            "0.2 sec",
            "0.5 sec",
            "1 sec",
            "2 sec",
            "5 sec",
            "10 sec",
            "30 sec"});
      this.comboBoxRefreshRate.Location = new System.Drawing.Point (599, 32);
      this.comboBoxRefreshRate.Name = "comboBoxRefreshRate";
      this.comboBoxRefreshRate.Size = new System.Drawing.Size (75, 21);
      this.comboBoxRefreshRate.TabIndex = 15;
      this.comboBoxRefreshRate.SelectedIndexChanged += new System.EventHandler (this.comboBoxRefreshRate_SelectedIndexChanged);
      // 
      // buttonStartStop
      // 
      this.buttonStartStop.Image = global::Autobook.Properties.Resources.Play1Hot_32;
      this.buttonStartStop.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.buttonStartStop.Location = new System.Drawing.Point (510, 31);
      this.buttonStartStop.Name = "buttonStartStop";
      this.buttonStartStop.Size = new System.Drawing.Size (72, 34);
      this.buttonStartStop.TabIndex = 14;
      this.buttonStartStop.Text = "Start";
      this.buttonStartStop.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.buttonStartStop.UseVisualStyleBackColor = true;
      this.buttonStartStop.Click += new System.EventHandler (this.buttonStartStop_Click);
      // 
      // textBoxUnderOver
      // 
      this.textBoxUnderOver.Location = new System.Drawing.Point (458, 33);
      this.textBoxUnderOver.Name = "textBoxUnderOver";
      this.textBoxUnderOver.Size = new System.Drawing.Size (37, 20);
      this.textBoxUnderOver.TabIndex = 13;
      this.textBoxUnderOver.Text = "0.00";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point (385, 37);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size (67, 13);
      this.label7.TabIndex = 12;
      this.label7.Text = "Under/Over:";
      // 
      // comboBoxEvent
      // 
      this.comboBoxEvent.FormattingEnabled = true;
      this.comboBoxEvent.Location = new System.Drawing.Point (49, 34);
      this.comboBoxEvent.Name = "comboBoxEvent";
      this.comboBoxEvent.Size = new System.Drawing.Size (322, 21);
      this.comboBoxEvent.TabIndex = 11;
      this.comboBoxEvent.MouseClick += new System.Windows.Forms.MouseEventHandler (this.comboBoxEvent_MouseClick);
      this.comboBoxEvent.SelectedIndexChanged += new System.EventHandler (this.comboBoxEvent_SelectedIndexChanged);
      this.comboBoxEvent.DropDownClosed += new System.EventHandler (this.comboBoxEvent_DropDownClosed);
      this.comboBoxEvent.DropDown += new System.EventHandler (this.comboBoxEvent_DropDown);
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point (12, 37);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size (38, 13);
      this.label6.TabIndex = 10;
      this.label6.Text = "Event:";
      // 
      // textBoxLowBack
      // 
      this.textBoxLowBack.Location = new System.Drawing.Point (519, 5);
      this.textBoxLowBack.Name = "textBoxLowBack";
      this.textBoxLowBack.Size = new System.Drawing.Size (37, 20);
      this.textBoxLowBack.TabIndex = 9;
      this.textBoxLowBack.Text = "0.00";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point (455, 8);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size (58, 13);
      this.label5.TabIndex = 8;
      this.label5.Text = "Low Back:";
      // 
      // textBoxInRunning
      // 
      this.textBoxInRunning.Location = new System.Drawing.Point (394, 4);
      this.textBoxInRunning.Name = "textBoxInRunning";
      this.textBoxInRunning.Size = new System.Drawing.Size (45, 20);
      this.textBoxInRunning.TabIndex = 7;
      this.textBoxInRunning.Text = "0.00";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point (326, 8);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size (62, 13);
      this.label4.TabIndex = 6;
      this.label4.Text = "In-Running:";
      // 
      // textBoxPreRace
      // 
      this.textBoxPreRace.Location = new System.Drawing.Point (256, 6);
      this.textBoxPreRace.Name = "textBoxPreRace";
      this.textBoxPreRace.Size = new System.Drawing.Size (47, 20);
      this.textBoxPreRace.TabIndex = 5;
      this.textBoxPreRace.Text = "50.00";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point (195, 9);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size (55, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "Pre-Race:";
      // 
      // textBoxLiability
      // 
      this.textBoxLiability.Location = new System.Drawing.Point (142, 6);
      this.textBoxLiability.Name = "textBoxLiability";
      this.textBoxLiability.Size = new System.Drawing.Size (37, 20);
      this.textBoxLiability.TabIndex = 3;
      this.textBoxLiability.Text = "0.00";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point (92, 9);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size (44, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Liability:";
      // 
      // textBoxProfit
      // 
      this.textBoxProfit.Location = new System.Drawing.Point (49, 6);
      this.textBoxProfit.Name = "textBoxProfit";
      this.textBoxProfit.Size = new System.Drawing.Size (37, 20);
      this.textBoxProfit.TabIndex = 1;
      this.textBoxProfit.Text = "1.00";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point (12, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size (34, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Profit:";
      // 
      // panelBottom
      // 
      this.panelBottom.Controls.Add (this.panelBets);
      this.panelBottom.Controls.Add (this.panelRunners);
      this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelBottom.Location = new System.Drawing.Point (0, 72);
      this.panelBottom.Name = "panelBottom";
      this.panelBottom.Size = new System.Drawing.Size (1018, 672);
      this.panelBottom.TabIndex = 1;
      // 
      // panelBets
      // 
      this.panelBets.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.panelBets.Controls.Add (this.label8);
      this.panelBets.Controls.Add (this.listViewBets);
      this.panelBets.Location = new System.Drawing.Point (671, 6);
      this.panelBets.Name = "panelBets";
      this.panelBets.Size = new System.Drawing.Size (344, 641);
      this.panelBets.TabIndex = 3;
      // 
      // label8
      // 
      this.label8.Dock = System.Windows.Forms.DockStyle.Top;
      this.label8.Location = new System.Drawing.Point (0, 0);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size (340, 13);
      this.label8.TabIndex = 2;
      this.label8.Text = "Bets";
      // 
      // listViewBets
      // 
      this.listViewBets.Columns.AddRange (new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderBetType,
            this.columnHeaderBetOdds,
            this.columnHeaderBetSize,
            this.columnHeaderBetTime});
      this.listViewBets.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.listViewBets.FullRowSelect = true;
      this.listViewBets.GridLines = true;
      this.listViewBets.Location = new System.Drawing.Point (0, 14);
      this.listViewBets.Name = "listViewBets";
      this.listViewBets.Size = new System.Drawing.Size (340, 623);
      this.listViewBets.TabIndex = 1;
      this.listViewBets.UseCompatibleStateImageBehavior = false;
      this.listViewBets.View = System.Windows.Forms.View.Details;
      // 
      // columnHeaderBetType
      // 
      this.columnHeaderBetType.Text = "Type";
      this.columnHeaderBetType.Width = 68;
      // 
      // columnHeaderBetOdds
      // 
      this.columnHeaderBetOdds.Text = "Odds";
      this.columnHeaderBetOdds.Width = 72;
      // 
      // columnHeaderBetSize
      // 
      this.columnHeaderBetSize.Text = "Size";
      this.columnHeaderBetSize.Width = 72;
      // 
      // columnHeaderBetTime
      // 
      this.columnHeaderBetTime.Text = "Time";
      this.columnHeaderBetTime.Width = 124;
      // 
      // panelRunners
      // 
      this.panelRunners.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.panelRunners.Controls.Add (this.label9);
      this.panelRunners.Controls.Add (this.runnersControl1);
      this.panelRunners.Location = new System.Drawing.Point (3, 6);
      this.panelRunners.Name = "panelRunners";
      this.panelRunners.Size = new System.Drawing.Size (662, 641);
      this.panelRunners.TabIndex = 2;
      // 
      // label9
      // 
      this.label9.Dock = System.Windows.Forms.DockStyle.Top;
      this.label9.Location = new System.Drawing.Point (0, 0);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size (658, 13);
      this.label9.TabIndex = 3;
      this.label9.Text = "Runners";
      // 
      // runnersControl1
      // 
      this.runnersControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.runnersControl1.ExchangeId = 0;
      this.runnersControl1.Location = new System.Drawing.Point (0, 14);
      this.runnersControl1.MarketId = 0;
      this.runnersControl1.Name = "runnersControl1";
      this.runnersControl1.Size = new System.Drawing.Size (658, 623);
      this.runnersControl1.TabIndex = 0;
      // 
      // statusStrip1
      // 
      this.statusStrip1.Location = new System.Drawing.Point (0, 722);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size (1018, 22);
      this.statusStrip1.TabIndex = 2;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // timerRefresh
      // 
      this.timerRefresh.Interval = 1000;
      this.timerRefresh.Tick += new System.EventHandler (this.timerRefresh_Tick);
      // 
      // FormMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size (1018, 744);
      this.Controls.Add (this.statusStrip1);
      this.Controls.Add (this.panelBottom);
      this.Controls.Add (this.panelTop);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.Name = "FormMain";
      this.Text = "Autobook";
      this.Load += new System.EventHandler (this.FormMain_Load);
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler (this.FormMain_FormClosing);
      this.panelTop.ResumeLayout (false);
      this.panelTop.PerformLayout ();
      this.panelBottom.ResumeLayout (false);
      this.panelBets.ResumeLayout (false);
      this.panelRunners.ResumeLayout (false);
      this.ResumeLayout (false);
      this.PerformLayout ();

    }

    #endregion

    private System.Windows.Forms.Panel panelTop;
    private System.Windows.Forms.Panel panelBottom;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.TextBox textBoxLowBack;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox textBoxInRunning;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox textBoxPreRace;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox textBoxLiability;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox textBoxProfit;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textBoxUnderOver;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.ComboBox comboBoxEvent;
		private System.Windows.Forms.Label label6;
    private System.Windows.Forms.ComboBox comboBoxRefreshRate;
		private System.Windows.Forms.Button buttonStartStop;
		private System.Windows.Forms.Timer timerRefresh;
		private RunnersControl runnersControl1;
		private System.Windows.Forms.ListView listViewBets;
		private System.Windows.Forms.ColumnHeader columnHeaderBetType;
		private System.Windows.Forms.ColumnHeader columnHeaderBetOdds;
		private System.Windows.Forms.ColumnHeader columnHeaderBetSize;
		private System.Windows.Forms.ColumnHeader columnHeaderBetTime;
		private System.Windows.Forms.Panel panelRunners;
		private System.Windows.Forms.Panel panelBets;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label9;
  }
}