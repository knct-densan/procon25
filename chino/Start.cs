using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace chino
{
    /// <summary>
    /// 起動画面。
    /// </summary>
    public partial class Start : Form
    {
        public Start()
        {
            InitializeComponent();
            this.AcceptButton = this.btnStart;
            this.rbtnSorter0.Checked = true;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // こちらのウィンドウの操作を無効化する
            this.Visible = false;

            // Core の 呼び出し
            Core c = new Core(txtServerAddress.Text, txtProblemID.Text, txtToken.Text, (rbtnSorter0.Checked ? 0 : 1));
            c.ShowDialog();
            c.Dispose();

            // こちらのウィンドウを復活させる。
            this.Visible = true;
        }
    }
}
