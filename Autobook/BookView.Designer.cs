namespace Autobook
{
  partial class BookView
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
      this.panelRunners = new System.Windows.Forms.Panel ();
      this.labelRunnerPL = new System.Windows.Forms.Label ();
      this.labelRunnerName = new System.Windows.Forms.Label ();
      this.panelPrices = new System.Windows.Forms.Panel ();
      this.buttonLay = new System.Windows.Forms.Button ();
      this.buttonBack = new System.Windows.Forms.Button ();
      this.panelRunners.SuspendLayout ();
      this.panelPrices.SuspendLayout ();
      this.SuspendLayout ();
      // 
      // panelRunners
      // 
      this.panelRunners.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
      this.panelRunners.Controls.Add (this.labelRunnerPL);
      this.panelRunners.Controls.Add (this.labelRunnerName);
      this.panelRunners.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelRunners.Location = new System.Drawing.Point (1, 1);
      this.panelRunners.Name = "panelRunners";
      this.panelRunners.Padding = new System.Windows.Forms.Padding(8, 2, 4, 2);
      this.panelRunners.Size = new System.Drawing.Size (548, 36);
      this.panelRunners.TabIndex = 0;
      // 
      // labelRunnerPL
      // 
      this.labelRunnerPL.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.labelRunnerPL.Location = new System.Drawing.Point (0, 16);
      this.labelRunnerPL.Name = "labelRunnerPL";
      this.labelRunnerPL.Size = new System.Drawing.Size (762, 20);
      this.labelRunnerPL.TabIndex = 1;
      // 
      // labelRunnerName
      // 
      this.labelRunnerName.Dock = System.Windows.Forms.DockStyle.Top;
      this.labelRunnerName.Location = new System.Drawing.Point (0, 0);
      this.labelRunnerName.Name = "labelRunnerName";
      this.labelRunnerName.Size = new System.Drawing.Size (762, 20);
      this.labelRunnerName.TabIndex = 0;
      //
      // panelPrices
      //
      this.panelPrices.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.panelPrices.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
      this.panelPrices.Controls.Add (this.buttonLay);
      this.panelPrices.Controls.Add (this.buttonBack);
      this.panelPrices.Location = new System.Drawing.Point (549, 0);
      this.panelPrices.Name = "panelPrices";
      this.panelPrices.Padding = new System.Windows.Forms.Padding(4);
      this.panelPrices.Size = new System.Drawing.Size(370, 38);
      this.panelPrices.TabIndex = 1;
      //
      // buttonLay
      //
      this.buttonLay.BackColor = System.Drawing.Color.FromArgb(250, 176, 215);
      this.buttonLay.FlatAppearance.BorderSize = 0;
      this.buttonLay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.buttonLay.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
      this.buttonLay.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
      this.buttonLay.Location = new System.Drawing.Point (293, 4);
      this.buttonLay.Name = "buttonLay";
      this.buttonLay.Size = new System.Drawing.Size (69, 30);
      this.buttonLay.TabIndex = 1;
      this.buttonLay.UseVisualStyleBackColor = false;
      this.buttonLay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      //
      // buttonBack
      //
      this.buttonBack.BackColor = System.Drawing.Color.FromArgb(91, 192, 222);
      this.buttonBack.FlatAppearance.BorderSize = 0;
      this.buttonBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.buttonBack.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
      this.buttonBack.ForeColor = System.Drawing.Color.White;
      this.buttonBack.Location = new System.Drawing.Point (220, 4);
      this.buttonBack.Name = "buttonBack";
      this.buttonBack.Size = new System.Drawing.Size (69, 30);
      this.buttonBack.TabIndex = 0;
      this.buttonBack.UseVisualStyleBackColor = false;
      this.buttonBack.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // MarketView2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.FromArgb(230, 230, 230);
      this.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.Controls.Add (this.panelPrices);
      this.Controls.Add (this.panelRunners);
      this.Name = "BookView";
      this.Padding = new System.Windows.Forms.Padding (1);
      this.Size = new System.Drawing.Size (920, 38);
      this.panelRunners.ResumeLayout (false);
      this.panelPrices.ResumeLayout (false);
      this.ResumeLayout (false);

    }

    #endregion

    private System.Windows.Forms.Panel panelRunners;
    private System.Windows.Forms.Panel panelPrices;
    private System.Windows.Forms.Button buttonLay;
    private System.Windows.Forms.Button buttonBack;
    private System.Windows.Forms.Label labelRunnerName;
    private System.Windows.Forms.Label labelRunnerPL;
  }
}
