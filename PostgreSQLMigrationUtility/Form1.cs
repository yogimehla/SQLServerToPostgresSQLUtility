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
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            //if (DateTime.Now > new DateTime(2020, 02, 15))

            //{
            //    MessageBox.Show("Demo expired");
            //    Application.Exit();
            //}
            //if (string.IsNullOrEmpty(txtPassword.Text) || string.IsNullOrEmpty(txtusername.Text))
            //{
            //    MessageBox.Show("Please provider username and password");
            //}

            if (txtusername.Text == "postgres" && txtPassword.Text == "Feb012020")
            {
                StaticObjects.log.Info("Logged in successful");
                this.Hide();
                var frm = new MenuForm();
                frm.ShowDialog();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            StaticObjects.log.Info("App Start");
        }
    }
}
