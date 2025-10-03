using System.Windows.Forms;

namespace Autobook
{
	partial class FormMarket2
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
			this.SuspendLayout ();
			// 
			// treeViewEvents
			// 
			this.treeViewEvents.Location = new System.Drawing.Point (12, 12);
			this.treeViewEvents.Name = "treeViewEvents";
			this.treeViewEvents.Size = new System.Drawing.Size (525, 297);
			this.treeViewEvents.TabIndex = 0;
			this.treeViewEvents.AfterSelect += new System.Windows.Forms.TreeViewEventHandler (this.treeViewEvents_AfterSelect);
			// 
			// FormMarket2
			// 
			this.ClientSize = new System.Drawing.Size (549, 321);
			this.Controls.Add (this.treeViewEvents);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FormMarket2";
			this.Text = "Select Market";
			this.Load += new System.EventHandler (this.FormMarket2_Load);
			this.ResumeLayout (false);

		}

		#endregion

		private System.Windows.Forms.TreeView treeViewEvents;
	}
}