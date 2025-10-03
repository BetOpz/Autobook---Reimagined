namespace Autobook
{
  partial class MarketView2
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent ()
    {
            this.components = new System.ComponentModel.Container();
            this.panelRunners = new System.Windows.Forms.Panel();
            this.labelRunnerPL = new System.Windows.Forms.Label();
            this.labelRunnerName = new System.Windows.Forms.Label();
            this.panelPrices = new System.Windows.Forms.Panel();
            this.buttonLay = new System.Windows.Forms.Button();
            this.buttonBack = new System.Windows.Forms.Button();
            this.labelExc = new System.Windows.Forms.Label();
            this.labelBets = new System.Windows.Forms.Label();
            this.labelReps = new System.Windows.Forms.Label();
            this.labelMatched = new System.Windows.Forms.Label();
            this.labelUnmatched = new System.Windows.Forms.Label();
            this.contextMenuStripDisable = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.disableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelRunners.SuspendLayout();
            this.panelPrices.SuspendLayout();
            this.contextMenuStripDisable.SuspendLayout();
            this.SuspendLayout();
            //
            // panelRunners
            //
            this.panelRunners.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
            this.panelRunners.Controls.Add(this.labelRunnerPL);
            this.panelRunners.Controls.Add(this.labelRunnerName);
            this.panelRunners.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRunners.Location = new System.Drawing.Point(1, 1);
            this.panelRunners.Name = "panelRunners";
            this.panelRunners.Padding = new System.Windows.Forms.Padding(8, 4, 4, 4);
            this.panelRunners.Size = new System.Drawing.Size(548, 48);
            this.panelRunners.TabIndex = 0;
            //
            // labelRunnerPL
            //
            this.labelRunnerPL.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelRunnerPL.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelRunnerPL.Location = new System.Drawing.Point(8, 26);
            this.labelRunnerPL.Name = "labelRunnerPL";
            this.labelRunnerPL.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.labelRunnerPL.Size = new System.Drawing.Size(536, 18);
            this.labelRunnerPL.TabIndex = 1;
            //
            // labelRunnerName
            //
            this.labelRunnerName.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelRunnerName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.labelRunnerName.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.labelRunnerName.Location = new System.Drawing.Point(8, 4);
            this.labelRunnerName.Name = "labelRunnerName";
            this.labelRunnerName.Size = new System.Drawing.Size(536, 22);
            this.labelRunnerName.TabIndex = 0;
            //
            // panelPrices
            //
            this.panelPrices.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.panelPrices.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
            this.panelPrices.Controls.Add(this.labelExc);
            this.panelPrices.Controls.Add(this.labelBets);
            this.panelPrices.Controls.Add(this.labelReps);
            this.panelPrices.Controls.Add(this.labelMatched);
            this.panelPrices.Controls.Add(this.labelUnmatched);
            this.panelPrices.Controls.Add(this.buttonBack);
            this.panelPrices.Controls.Add(this.buttonLay);
            this.panelPrices.Location = new System.Drawing.Point(549, 1);
            this.panelPrices.Name = "panelPrices";
            this.panelPrices.Padding = new System.Windows.Forms.Padding(4);
            this.panelPrices.Size = new System.Drawing.Size(370, 48);
            this.panelPrices.TabIndex = 1;
            //
            // labelExc
            //
            this.labelExc.BackColor = System.Drawing.Color.Transparent;
            this.labelExc.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.labelExc.ForeColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this.labelExc.Location = new System.Drawing.Point(4, 4);
            this.labelExc.Name = "labelExc";
            this.labelExc.Size = new System.Drawing.Size(36, 40);
            this.labelExc.TabIndex = 5;
            this.labelExc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // labelBets
            //
            this.labelBets.BackColor = System.Drawing.Color.FromArgb(13, 110, 253);
            this.labelBets.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelBets.ForeColor = System.Drawing.Color.White;
            this.labelBets.Location = new System.Drawing.Point(44, 4);
            this.labelBets.Name = "labelBets";
            this.labelBets.Size = new System.Drawing.Size(40, 40);
            this.labelBets.TabIndex = 4;
            this.labelBets.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // labelReps
            //
            this.labelReps.BackColor = System.Drawing.Color.FromArgb(108, 117, 125);
            this.labelReps.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelReps.ForeColor = System.Drawing.Color.White;
            this.labelReps.Location = new System.Drawing.Point(88, 4);
            this.labelReps.Name = "labelReps";
            this.labelReps.Size = new System.Drawing.Size(40, 40);
            this.labelReps.TabIndex = 3;
            this.labelReps.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // labelMatched
            //
            this.labelMatched.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            this.labelMatched.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelMatched.ForeColor = System.Drawing.Color.White;
            this.labelMatched.Location = new System.Drawing.Point(132, 4);
            this.labelMatched.Name = "labelMatched";
            this.labelMatched.Size = new System.Drawing.Size(40, 40);
            this.labelMatched.TabIndex = 6;
            this.labelMatched.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // labelUnmatched
            //
            this.labelUnmatched.BackColor = System.Drawing.Color.FromArgb(255, 165, 0);
            this.labelUnmatched.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelUnmatched.ForeColor = System.Drawing.Color.White;
            this.labelUnmatched.Location = new System.Drawing.Point(176, 4);
            this.labelUnmatched.Name = "labelUnmatched";
            this.labelUnmatched.Size = new System.Drawing.Size(40, 40);
            this.labelUnmatched.TabIndex = 7;
            this.labelUnmatched.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // buttonBack
            //
            this.buttonBack.BackColor = System.Drawing.Color.FromArgb(91, 192, 222);
            this.buttonBack.FlatAppearance.BorderSize = 0;
            this.buttonBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBack.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonBack.ForeColor = System.Drawing.Color.White;
            this.buttonBack.Location = new System.Drawing.Point(220, 4);
            this.buttonBack.Name = "buttonBack";
            this.buttonBack.Size = new System.Drawing.Size(69, 40);
            this.buttonBack.TabIndex = 0;
            this.buttonBack.UseVisualStyleBackColor = false;
            //
            // buttonLay
            //
            this.buttonLay.BackColor = System.Drawing.Color.FromArgb(250, 176, 215);
            this.buttonLay.FlatAppearance.BorderSize = 0;
            this.buttonLay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonLay.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonLay.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.buttonLay.Location = new System.Drawing.Point(293, 4);
            this.buttonLay.Name = "buttonLay";
            this.buttonLay.Size = new System.Drawing.Size(69, 40);
            this.buttonLay.TabIndex = 1;
            this.buttonLay.UseVisualStyleBackColor = false;
            // 
            // contextMenuStripDisable
            // 
            this.contextMenuStripDisable.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.disableToolStripMenuItem,
            this.enableToolStripMenuItem});
            this.contextMenuStripDisable.Name = "contextMenuStripDisable";
            this.contextMenuStripDisable.Size = new System.Drawing.Size(113, 48);
            // 
            // disableToolStripMenuItem
            // 
            this.disableToolStripMenuItem.Name = "disableToolStripMenuItem";
            this.disableToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.disableToolStripMenuItem.Text = "Disable";
            this.disableToolStripMenuItem.Click += new System.EventHandler(this.disableToolStripMenuItem_Click);
            // 
            // enableToolStripMenuItem
            // 
            this.enableToolStripMenuItem.Enabled = false;
            this.enableToolStripMenuItem.Name = "enableToolStripMenuItem";
            this.enableToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.enableToolStripMenuItem.Text = "Enable";
            this.enableToolStripMenuItem.Click += new System.EventHandler(this.enableToolStripMenuItem_Click);
            //
            // MarketView2
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(230, 230, 230);
            this.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ContextMenuStrip = this.contextMenuStripDisable;
            this.Controls.Add(this.panelPrices);
            this.Controls.Add(this.panelRunners);
            this.Name = "MarketView2";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Size = new System.Drawing.Size(920, 50);
            this.panelRunners.ResumeLayout(false);
            this.panelPrices.ResumeLayout(false);
            this.contextMenuStripDisable.ResumeLayout(false);
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panelRunners;
    private System.Windows.Forms.Panel panelPrices;
    private System.Windows.Forms.Button buttonLay;
    private System.Windows.Forms.Button buttonBack;
    private System.Windows.Forms.Label labelRunnerName;
    private System.Windows.Forms.Label labelRunnerPL;
    private System.Windows.Forms.Label labelExc;
    private System.Windows.Forms.Label labelBets;
    private System.Windows.Forms.Label labelReps;
    private System.Windows.Forms.Label labelMatched;
    private System.Windows.Forms.Label labelUnmatched;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripDisable;
    private System.Windows.Forms.ToolStripMenuItem disableToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem enableToolStripMenuItem;
  }
}
