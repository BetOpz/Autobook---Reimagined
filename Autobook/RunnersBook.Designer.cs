namespace Autobook
{
	partial class RunnersBook
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
      this.labelName = new System.Windows.Forms.Label ();
      this.labelBack = new System.Windows.Forms.Label ();
      this.SuspendLayout ();
      // 
      // labelName
      // 
      this.labelName.Dock = System.Windows.Forms.DockStyle.Left;
      this.labelName.Location = new System.Drawing.Point (0, 0);
      this.labelName.Name = "labelName";
      this.labelName.Size = new System.Drawing.Size (264, 20);
      this.labelName.TabIndex = 1;
      this.labelName.Text = "Runner";
      // 
      // labelBack
      // 
      this.labelBack.Dock = System.Windows.Forms.DockStyle.Left;
      this.labelBack.Location = new System.Drawing.Point (264, 0);
      this.labelBack.Name = "labelBack";
      this.labelBack.Size = new System.Drawing.Size (69, 20);
      this.labelBack.TabIndex = 2;
      this.labelBack.Text = "Back";
      // 
      // RunnersBook
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add (this.labelBack);
      this.Controls.Add (this.labelName);
      this.Name = "RunnersBook";
      this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.Size = new System.Drawing.Size (630, 20);
      this.ResumeLayout (false);

		}

		#endregion

    private System.Windows.Forms.Label labelName;
    private System.Windows.Forms.Label labelBack;
	}
}
