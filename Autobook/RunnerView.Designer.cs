namespace Autobook
{
	partial class RunnerView
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
			this.labelLay = new System.Windows.Forms.Label ();
			this.labelStake = new System.Windows.Forms.Label ();
			this.labelPL = new System.Windows.Forms.Label ();
			this.labelUnmatched = new System.Windows.Forms.Label ();
			this.SuspendLayout ();
			// 
			// labelName
			// 
			this.labelName.Dock = System.Windows.Forms.DockStyle.Left;
			this.labelName.Location = new System.Drawing.Point (0, 0);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size (264, 20);
			this.labelName.TabIndex = 0;
			this.labelName.Text = "Runner";
			// 
			// labelBack
			// 
			this.labelBack.Dock = System.Windows.Forms.DockStyle.Left;
			this.labelBack.Location = new System.Drawing.Point (264, 0);
			this.labelBack.Name = "labelBack";
			this.labelBack.Size = new System.Drawing.Size (69, 20);
			this.labelBack.TabIndex = 1;
			this.labelBack.Text = "Back";
			// 
			// labelLay
			// 
			this.labelLay.Dock = System.Windows.Forms.DockStyle.Left;
			this.labelLay.Location = new System.Drawing.Point (333, 0);
			this.labelLay.Name = "labelLay";
			this.labelLay.Size = new System.Drawing.Size (69, 20);
			this.labelLay.TabIndex = 2;
			this.labelLay.Text = "Lay";
			// 
			// labelStake
			// 
			this.labelStake.Dock = System.Windows.Forms.DockStyle.Left;
			this.labelStake.Location = new System.Drawing.Point (402, 0);
			this.labelStake.Name = "labelStake";
			this.labelStake.Size = new System.Drawing.Size (69, 20);
			this.labelStake.TabIndex = 3;
			this.labelStake.Text = "Stake";
			// 
			// labelPL
			// 
			this.labelPL.Dock = System.Windows.Forms.DockStyle.Left;
			this.labelPL.Location = new System.Drawing.Point (471, 0);
			this.labelPL.Name = "labelPL";
			this.labelPL.Size = new System.Drawing.Size (69, 20);
			this.labelPL.TabIndex = 4;
			this.labelPL.Text = "P/L";
			// 
			// labelUnmatched
			// 
			this.labelUnmatched.Dock = System.Windows.Forms.DockStyle.Left;
			this.labelUnmatched.Location = new System.Drawing.Point (540, 0);
			this.labelUnmatched.Name = "labelUnmatched";
			this.labelUnmatched.Size = new System.Drawing.Size (69, 20);
			this.labelUnmatched.TabIndex = 5;
			this.labelUnmatched.Text = "Unmatched";
			// 
			// RunnerView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add (this.labelUnmatched);
			this.Controls.Add (this.labelPL);
			this.Controls.Add (this.labelStake);
			this.Controls.Add (this.labelLay);
			this.Controls.Add (this.labelBack);
			this.Controls.Add (this.labelName);
			this.Name = "RunnerView";
			this.Size = new System.Drawing.Size (630, 20);
			this.ResumeLayout (false);

		}

		#endregion

		private System.Windows.Forms.Label labelName;
		private System.Windows.Forms.Label labelBack;
		private System.Windows.Forms.Label labelLay;
		private System.Windows.Forms.Label labelStake;
		private System.Windows.Forms.Label labelPL;
		private System.Windows.Forms.Label labelUnmatched;
	}
}
