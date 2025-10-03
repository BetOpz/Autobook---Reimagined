namespace Autobook
{
  partial class RunnersControl
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
			this.listViewRunners = new System.Windows.Forms.ListView ();
			this.columnHeaderSelection = new System.Windows.Forms.ColumnHeader ();
			this.columnHeaderBack3 = new System.Windows.Forms.ColumnHeader ();
			this.columnHeaderBack2 = new System.Windows.Forms.ColumnHeader ();
			this.columnHeaderBack = new System.Windows.Forms.ColumnHeader ();
			this.columnHeaderLay = new System.Windows.Forms.ColumnHeader ();
			this.columnHeaderLay2 = new System.Windows.Forms.ColumnHeader ();
			this.columnHeaderLay3 = new System.Windows.Forms.ColumnHeader ();
			this.SuspendLayout ();
			// 
			// listViewRunners
			// 
			this.listViewRunners.Columns.AddRange (new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderSelection,
            this.columnHeaderBack3,
            this.columnHeaderBack2,
            this.columnHeaderBack,
            this.columnHeaderLay,
            this.columnHeaderLay2,
            this.columnHeaderLay3});
			this.listViewRunners.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewRunners.FullRowSelect = true;
			this.listViewRunners.GridLines = true;
			this.listViewRunners.HideSelection = false;
			this.listViewRunners.Location = new System.Drawing.Point (0, 0);
			this.listViewRunners.Name = "listViewRunners";
			this.listViewRunners.Size = new System.Drawing.Size (564, 150);
			this.listViewRunners.TabIndex = 0;
			this.listViewRunners.UseCompatibleStateImageBehavior = false;
			this.listViewRunners.View = System.Windows.Forms.View.Details;
			// 
			// columnHeaderSelection
			// 
			this.columnHeaderSelection.Text = "Selection";
			this.columnHeaderSelection.Width = 200;
			// 
			// columnHeaderBack3
			// 
			this.columnHeaderBack3.Text = "";
			// 
			// columnHeaderBack2
			// 
			this.columnHeaderBack2.Text = "";
			// 
			// columnHeaderBack
			// 
			this.columnHeaderBack.Text = "Back";
			// 
			// columnHeaderLay
			// 
			this.columnHeaderLay.Text = "Lay";
			// 
			// columnHeaderLay2
			// 
			this.columnHeaderLay2.Text = "";
			// 
			// columnHeaderLay3
			// 
			this.columnHeaderLay3.Text = "";
			// 
			// RunnersControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add (this.listViewRunners);
			this.Name = "RunnersControl";
			this.Size = new System.Drawing.Size (564, 150);
			this.ResumeLayout (false);

    }

    #endregion

    private System.Windows.Forms.ListView listViewRunners;
    private System.Windows.Forms.ColumnHeader columnHeaderSelection;
    private System.Windows.Forms.ColumnHeader columnHeaderBack3;
    private System.Windows.Forms.ColumnHeader columnHeaderBack2;
    private System.Windows.Forms.ColumnHeader columnHeaderBack;
    private System.Windows.Forms.ColumnHeader columnHeaderLay;
    private System.Windows.Forms.ColumnHeader columnHeaderLay2;
    private System.Windows.Forms.ColumnHeader columnHeaderLay3;
  }
}
