using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PostgreSQLMigrationUtility
{
    public partial class MenuForm : Form
    {
        public MenuForm()
        {
            InitializeComponent();
        }

        private void MenuForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void btnConfiguration_Click(object sender, EventArgs e)
        {
            StaticObjects.RefMenu = this;
            this.Hide();
            var frm = new SettingsForm();
            frm.ShowDialog();
        }

        private void btnSourceMigration_Click(object sender, EventArgs e)
        {
            StaticObjects.RefMenu = this;
            this.Hide();
            var frm = new SourceMigrationForm();
            frm.ShowDialog();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.techsapphire.net/");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText("+91-9360223756");
            MessageBox.Show("Phone No. Copied");
            
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText("contact@techsapphire.net");
            MessageBox.Show("Email Copied");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.me/bimlamehla");
        }
    }
}
