using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Npgsql;

namespace PostgreSQLMigrationUtility
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }
        public SettingClass Settings { get; set; }
        private void btnSave_Click(object sender, EventArgs e)
        {
            Settings.MigrationLocation = txtMigrationLocation.Text;
            Settings.SQLServer = txtSQLServer.Text;
            Settings.SQLUsername = txtSQLUsername.Text;
            Settings.SQLPassword = txtSQLPassword.Text;
            Settings.SQLPort = txtSQLPort.Text;
            Settings.Postgres = txtPostgresSQL.Text;
            Settings.PostgresUsername = txtPostgreSQLUsername.Text;
            Settings.PostgresPassword = txtPostgreSQLPassword.Text;
            Settings.PostgresPort = txtPostgresPort.Text;
            File.WriteAllText(Environment.ExpandEnvironmentVariables("%allusersprofile%") + "/PostgreSQLMigrationUtility/Configuration.config", JsonConvert.SerializeObject(Settings));
            this.Close();
        }
  
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(Environment.ExpandEnvironmentVariables("%allusersprofile%") + "/PostgreSQLMigrationUtility/Configuration.config"))
            {
                Settings = JsonConvert.DeserializeObject<SettingClass>(File.ReadAllText(Environment.ExpandEnvironmentVariables("%allusersprofile%") + "/PostgreSQLMigrationUtility/Configuration.config"));
                txtMigrationLocation.Text = Settings.MigrationLocation;
                txtPostgresPort.Text = Settings.PostgresPort;
                txtPostgreSQLPassword.Text = Settings.PostgresPassword;
                txtPostgreSQLUsername.Text = Settings.PostgresUsername;
                txtPostgresSQL.Text = Settings.Postgres;
                txtSQLPassword.Text = Settings.SQLPassword;
                txtSQLPort.Text = Settings.SQLPort;
                txtSQLServer.Text = Settings.SQLServer;
                txtSQLUsername.Text = Settings.SQLUsername;
            }
            else {
                Settings = new SettingClass();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StaticObjects.RefMenu.Show();
        }

        private void btnFolderBrowser_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            if (!string.IsNullOrEmpty(folderBrowserDialog1.SelectedPath))
            {
                txtMigrationLocation.Text = folderBrowserDialog1.SelectedPath;
         
            }
        }

        private void btnSQLTest_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection())
                {
                    string connectionString = string.Format( "Server={0},{4};Database={1};User Id={2};Password={3};", txtSQLServer.Text, "master", txtSQLUsername.Text, txtSQLPassword.Text, txtSQLPort.Text);
                    con.ConnectionString = connectionString;
                    con.Open();
                    MessageBox.Show("Test connection successful");
                }
            }
            catch (Exception ex)
            {
                StaticObjects.log.Error(ex);
                MessageBox.Show(ex.Message);
            }
        }

        private void btnPostgresCon_Click(object sender, EventArgs e)
        {
            try
            {
                using (var con = new NpgsqlConnection())
                {
                    string connectionString = string.Format("User ID={2};Password={3};Host={0};Port={4};Database={1};", txtPostgresSQL.Text, "postgres", txtPostgreSQLUsername.Text, txtPostgreSQLPassword.Text, txtPostgresPort.Text);
                    con.ConnectionString = connectionString;
                    con.Open();
                    MessageBox.Show("Test connection successful");
                }
            }
            catch (Exception ex)
            {
                StaticObjects.log.Error(ex);
                MessageBox.Show(ex.Message);
            }
        }
    }
}
