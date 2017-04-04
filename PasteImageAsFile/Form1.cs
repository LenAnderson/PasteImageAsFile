using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PasteImageAsFile
{
    public partial class Preferences : Form
    {
        public Preferences()
        {
            InitializeComponent();
        }

        private void Preferences_Load(object sender, EventArgs e)
        {
            this.Hide();
            cbxFormat.Text = Properties.Settings.Default.format;
            ImagePaster ip = new ImagePaster();
        }

        private void systray_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.format = cbxFormat.Text;
            Properties.Settings.Default.Save();
            this.Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cbxFormat.Text = Properties.Settings.Default.format;
            this.Hide();
        }
    }
}
