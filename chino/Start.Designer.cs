namespace chino
{
    partial class Start
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.txtToken = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtServerAddress = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtProblemID = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.rbtnSorter0 = new System.Windows.Forms.RadioButton();
            this.rbtnSorter1 = new System.Windows.Forms.RadioButton();
            this.gboxForPractice = new System.Windows.Forms.GroupBox();
            this.lblLine = new System.Windows.Forms.Label();
            this.gboxForPractice.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtToken
            // 
            this.txtToken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtToken.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtToken.Location = new System.Drawing.Point(130, 45);
            this.txtToken.Name = "txtToken";
            this.txtToken.Size = new System.Drawing.Size(329, 20);
            this.txtToken.TabIndex = 1;
            this.txtToken.Text = "1055771881";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label3.Location = new System.Drawing.Point(80, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Token :";
            // 
            // txtServerAddress
            // 
            this.txtServerAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServerAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtServerAddress.Location = new System.Drawing.Point(130, 19);
            this.txtServerAddress.Name = "txtServerAddress";
            this.txtServerAddress.Size = new System.Drawing.Size(329, 20);
            this.txtServerAddress.TabIndex = 0;
            this.txtServerAddress.Text = "172.16.1.2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label2.Location = new System.Drawing.Point(39, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Server Address :";
            // 
            // txtProblemID
            // 
            this.txtProblemID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProblemID.Location = new System.Drawing.Point(100, 19);
            this.txtProblemID.MaxLength = 2;
            this.txtProblemID.Name = "txtProblemID";
            this.txtProblemID.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtProblemID.Size = new System.Drawing.Size(404, 20);
            this.txtProblemID.TabIndex = 0;
            this.txtProblemID.Text = "01";
            this.txtProblemID.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Meiryo UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 14);
            this.label1.TabIndex = 12;
            this.label1.Text = "Problem ID :";
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Location = new System.Drawing.Point(429, 50);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 25);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // rbtnSorter0
            // 
            this.rbtnSorter0.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.rbtnSorter0.AutoSize = true;
            this.rbtnSorter0.Location = new System.Drawing.Point(299, 54);
            this.rbtnSorter0.Name = "rbtnSorter0";
            this.rbtnSorter0.Size = new System.Drawing.Size(59, 17);
            this.rbtnSorter0.TabIndex = 1;
            this.rbtnSorter0.TabStop = true;
            this.rbtnSorter0.Text = "Sorter0";
            this.rbtnSorter0.UseVisualStyleBackColor = true;
            // 
            // rbtnSorter1
            // 
            this.rbtnSorter1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.rbtnSorter1.AutoSize = true;
            this.rbtnSorter1.Location = new System.Drawing.Point(364, 54);
            this.rbtnSorter1.Name = "rbtnSorter1";
            this.rbtnSorter1.Size = new System.Drawing.Size(59, 17);
            this.rbtnSorter1.TabIndex = 2;
            this.rbtnSorter1.TabStop = true;
            this.rbtnSorter1.Text = "Sorter1";
            this.rbtnSorter1.UseVisualStyleBackColor = true;
            // 
            // gboxForPractice
            // 
            this.gboxForPractice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gboxForPractice.Controls.Add(this.label2);
            this.gboxForPractice.Controls.Add(this.txtServerAddress);
            this.gboxForPractice.Controls.Add(this.label3);
            this.gboxForPractice.Controls.Add(this.txtToken);
            this.gboxForPractice.Font = new System.Drawing.Font("Meiryo", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.gboxForPractice.Location = new System.Drawing.Point(24, 100);
            this.gboxForPractice.Name = "gboxForPractice";
            this.gboxForPractice.Size = new System.Drawing.Size(470, 82);
            this.gboxForPractice.TabIndex = 18;
            this.gboxForPractice.TabStop = false;
            this.gboxForPractice.Text = "Settings for practice";
            // 
            // lblLine
            // 
            this.lblLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLine.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblLine.Location = new System.Drawing.Point(10, 87);
            this.lblLine.Name = "lblLine";
            this.lblLine.Size = new System.Drawing.Size(498, 1);
            this.lblLine.TabIndex = 19;
            // 
            // Start
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 192);
            this.Controls.Add(this.lblLine);
            this.Controls.Add(this.gboxForPractice);
            this.Controls.Add(this.rbtnSorter1);
            this.Controls.Add(this.txtProblemID);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rbtnSorter0);
            this.Controls.Add(this.btnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(729, 231);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(532, 231);
            this.Name = "Start";
            this.ShowIcon = false;
            this.Text = "chino";
            this.gboxForPractice.ResumeLayout(false);
            this.gboxForPractice.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtToken;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtServerAddress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtProblemID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.RadioButton rbtnSorter0;
        private System.Windows.Forms.RadioButton rbtnSorter1;
        private System.Windows.Forms.GroupBox gboxForPractice;
        private System.Windows.Forms.Label lblLine;

    }
}

