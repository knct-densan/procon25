namespace chino
{
    partial class Core
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
            this.txtSortStatus = new System.Windows.Forms.Label();
            this.btnResort = new System.Windows.Forms.Button();
            this.btnPost = new System.Windows.Forms.Button();
            this.txtPostStatus = new System.Windows.Forms.Label();
            this.lbl1 = new System.Windows.Forms.Label();
            this.lbl2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtSortStatus
            // 
            this.txtSortStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtSortStatus.AutoSize = true;
            this.txtSortStatus.Enabled = false;
            this.txtSortStatus.Location = new System.Drawing.Point(94, 261);
            this.txtSortStatus.Name = "txtSortStatus";
            this.txtSortStatus.Size = new System.Drawing.Size(19, 13);
            this.txtSortStatus.TabIndex = 0;
            this.txtSortStatus.Text = "....";
            this.txtSortStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnResort
            // 
            this.btnResort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResort.Location = new System.Drawing.Point(407, 273);
            this.btnResort.Name = "btnResort";
            this.btnResort.Size = new System.Drawing.Size(75, 23);
            this.btnResort.TabIndex = 1;
            this.btnResort.TabStop = false;
            this.btnResort.Text = "Resort";
            this.btnResort.UseVisualStyleBackColor = true;
            this.btnResort.Click += new System.EventHandler(this.btnResort_Click);
            // 
            // btnPost
            // 
            this.btnPost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPost.Location = new System.Drawing.Point(488, 273);
            this.btnPost.Name = "btnPost";
            this.btnPost.Size = new System.Drawing.Size(75, 23);
            this.btnPost.TabIndex = 2;
            this.btnPost.TabStop = false;
            this.btnPost.Text = "Post";
            this.btnPost.UseVisualStyleBackColor = true;
            this.btnPost.Click += new System.EventHandler(this.btnPost_Click);
            // 
            // txtPostStatus
            // 
            this.txtPostStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtPostStatus.AutoSize = true;
            this.txtPostStatus.Enabled = false;
            this.txtPostStatus.Location = new System.Drawing.Point(94, 286);
            this.txtPostStatus.Name = "txtPostStatus";
            this.txtPostStatus.Size = new System.Drawing.Size(19, 13);
            this.txtPostStatus.TabIndex = 3;
            this.txtPostStatus.Text = "....";
            this.txtPostStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbl1
            // 
            this.lbl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbl1.AutoSize = true;
            this.lbl1.Enabled = false;
            this.lbl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lbl1.Location = new System.Drawing.Point(12, 261);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(74, 13);
            this.lbl1.TabIndex = 4;
            this.lbl1.Text = "Sort Status:";
            this.lbl1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl2
            // 
            this.lbl2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbl2.AutoSize = true;
            this.lbl2.Enabled = false;
            this.lbl2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lbl2.Location = new System.Drawing.Point(12, 286);
            this.lbl2.Name = "lbl2";
            this.lbl2.Size = new System.Drawing.Size(76, 13);
            this.lbl2.TabIndex = 5;
            this.lbl2.Text = "Post Status:";
            this.lbl2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Core
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 308);
            this.Controls.Add(this.lbl2);
            this.Controls.Add(this.lbl1);
            this.Controls.Add(this.txtPostStatus);
            this.Controls.Add(this.btnPost);
            this.Controls.Add(this.btnResort);
            this.Controls.Add(this.txtSortStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Core";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "chino";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Core_FormClosing);
            this.Shown += new System.EventHandler(this.Core_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Core_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Core_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Core_MouseUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label txtSortStatus;
        private System.Windows.Forms.Button btnResort;
        private System.Windows.Forms.Button btnPost;
        private System.Windows.Forms.Label txtPostStatus;
        private System.Windows.Forms.Label lbl1;
        private System.Windows.Forms.Label lbl2;
    }
}