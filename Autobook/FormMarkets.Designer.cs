using System.Windows.Forms;

namespace Autobook
{
  partial class FormMarkets
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
			this.treeViewEvents = new System.Windows.Forms.TreeView ();
			this.buttonClose = new System.Windows.Forms.Button ();
			this.SuspendLayout ();
			// 
			// treeViewEvents
			// 
			this.treeViewEvents.Location = new System.Drawing.Point (13, 13);
			this.treeViewEvents.Margin = new System.Windows.Forms.Padding (4);
			this.treeViewEvents.Name = "treeViewEvents";
			this.treeViewEvents.Size = new System.Drawing.Size (489, 726);
			this.treeViewEvents.TabIndex = 0;
			this.treeViewEvents.AfterSelect += new System.Windows.Forms.TreeViewEventHandler (this.treeViewEvents_AfterSelect);
			// 
			// buttonClose
			// 
			this.buttonClose.Location = new System.Drawing.Point (212, 906);
			this.buttonClose.Margin = new System.Windows.Forms.Padding (4);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Size = new System.Drawing.Size (100, 28);
			this.buttonClose.TabIndex = 1;
			this.buttonClose.Text = "Close";
			this.buttonClose.UseVisualStyleBackColor = true;
			this.buttonClose.Click += new System.EventHandler (this.buttonClose_Click);
			// 
			// FormMarkets
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF (8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size (523, 752);
			this.Controls.Add (this.buttonClose);
			this.Controls.Add (this.treeViewEvents);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding (4);
			this.Name = "FormMarkets";
			this.Text = "Select Market";
			this.Load += new System.EventHandler (this.FormMarkets_Load);
			this.ResumeLayout (false);

    }

    #endregion

    private System.Windows.Forms.TreeView treeViewEvents;
    private System.Windows.Forms.Button buttonClose;
  }
}