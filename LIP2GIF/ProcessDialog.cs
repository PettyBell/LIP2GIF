using System.Windows.Forms;

namespace RMP {
	/**
	 * 閉じるボタンを無効化したフォーム
	 */
	class ProcessDialog : Form {
		private ProgressBar progressBar1;

		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;
				cp.ClassStyle = cp.ClassStyle | 0x200;
				return cp;
			}
		}

		public ProcessDialog() {
			this.ShowIcon = false;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.InitializeComponent();
		}

		private void InitializeComponent() {
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.SuspendLayout();
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(15, 15);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(290, 20);
			this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.progressBar1.TabIndex = 0;
			// 
			// ProcessDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 15F);
			this.ClientSize = new System.Drawing.Size(340, 90);
			this.Controls.Add(this.progressBar1);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(340, 90);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(340, 90);
			this.Name = "ProcessDialog";
			this.Text = "LIP2GIF | 作業動画をデスクトップに書き出し中";
			this.Load += new System.EventHandler(this.NoControlForm_Load);
			this.ResumeLayout(false);

		}

		private void label1_Click(object sender, System.EventArgs e) {

		}

		private void NoControlForm_Load(object sender, System.EventArgs e) {

		}
	}
}
