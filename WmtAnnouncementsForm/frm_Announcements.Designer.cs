namespace WmtAnnouncementsForm
{
    partial class frm_Announcements
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
            this.components = new System.ComponentModel.Container();
            this.tmr_UpdateElements = new System.Windows.Forms.Timer(this.components);
            this.tmr_thirtySeconds = new System.Windows.Forms.Timer(this.components);
            this.tmr_PaintRefresh = new System.Windows.Forms.Timer(this.components);
            this.tmr_slideshow = new System.Windows.Forms.Timer(this.components);
            this.bodyTextBox = new System.Windows.Forms.TextBox();
            this.titleTextBox = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tmr_UpdateElements
            // 
            this.tmr_UpdateElements.Enabled = true;
            this.tmr_UpdateElements.Interval = 300000;
            this.tmr_UpdateElements.Tick += new System.EventHandler(this.tmr_UpdateElements_Tick);
            // 
            // tmr_thirtySeconds
            // 
            this.tmr_thirtySeconds.Enabled = true;
            this.tmr_thirtySeconds.Interval = 30000;
            this.tmr_thirtySeconds.Tick += new System.EventHandler(this.tmr_thirtySeconds_Tick);
            // 
            // tmr_PaintRefresh
            // 
            this.tmr_PaintRefresh.Enabled = true;
            this.tmr_PaintRefresh.Tick += new System.EventHandler(this.tmr_PaintRefresh_Tick);
            // 
            // tmr_slideshow
            // 
            this.tmr_slideshow.Enabled = true;
            this.tmr_slideshow.Interval = 5000;
            this.tmr_slideshow.Tick += new System.EventHandler(this.tmr_slideshow_Tick);
            // 
            // bodyTextBox
            // 
            this.bodyTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.bodyTextBox.Cursor = System.Windows.Forms.Cursors.No;
            this.bodyTextBox.Location = new System.Drawing.Point(151, 31);
            this.bodyTextBox.Multiline = true;
            this.bodyTextBox.Name = "bodyTextBox";
            this.bodyTextBox.ReadOnly = true;
            this.bodyTextBox.Size = new System.Drawing.Size(100, 20);
            this.bodyTextBox.TabIndex = 1;
            // 
            // titleTextBox
            // 
            this.titleTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.titleTextBox.Cursor = System.Windows.Forms.Cursors.No;
            this.titleTextBox.Location = new System.Drawing.Point(151, 100);
            this.titleTextBox.Multiline = true;
            this.titleTextBox.Name = "titleTextBox";
            this.titleTextBox.ReadOnly = true;
            this.titleTextBox.Size = new System.Drawing.Size(100, 20);
            this.titleTextBox.TabIndex = 2;
            // 
            // frm_Announcements
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.ControlBox = false;
            this.Controls.Add(this.titleTextBox);
            this.Controls.Add(this.bodyTextBox);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.Name = "frm_Announcements";
            this.Text = "Westmount Announcements";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.frm_Announcements_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frm_Announcements_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer tmr_UpdateElements;
        private System.Windows.Forms.Timer tmr_thirtySeconds;
        private System.Windows.Forms.Timer tmr_PaintRefresh;
        private System.Windows.Forms.Timer tmr_slideshow;
        private System.Windows.Forms.TextBox bodyTextBox;
        private System.Windows.Forms.TextBox titleTextBox;
        private System.Windows.Forms.TextBox textBox1;
    }
}

