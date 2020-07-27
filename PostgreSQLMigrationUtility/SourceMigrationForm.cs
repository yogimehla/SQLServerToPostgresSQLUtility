using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Threading;
using System.IO;

namespace PostgreSQLMigrationUtility
{
    public partial class SourceMigrationForm : Form
    {
        public SourceMigrationForm()
        {
            InitializeComponent();
        }

        private void SourceMigrationForm_Load(object sender, EventArgs e)
        {
            var settingData = StaticObjects.GetSettings();
            if (string.IsNullOrEmpty(settingData))
            {
                MessageBox.Show("Configuration is not complete.");
            }
            else {
                var settings = JsonConvert.DeserializeObject<SettingClass>(settingData);
                try
                {
                    using (SqlConnection con = new SqlConnection())
                    {
                        string connectionString = string.Format("Server={0},{4};Database={1};User Id={2};Password={3};", settings.SQLServer, "master", settings.SQLUsername, settings.SQLPassword, settings.SQLPort);
                        con.ConnectionString = connectionString;
                        using (var cmd = new SqlCommand())
                        {
                            cmd.CommandText = "select name from sys.databases";
                            cmd.Connection = con;
                            DataSet ds = new DataSet();
                            using (var da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(ds);
                                cmbDatabases.Items.Clear();
                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    cmbDatabases.Items.Add(dr["name"].ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    StaticObjects.log.Error(ex);
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            StaticObjects.RefMenu.Show();
            this.Close();
        }

        private void cmbDatabases_TextChanged(object sender, EventArgs e)
        {
            var settingData = StaticObjects.GetSettings();
            if (string.IsNullOrEmpty(settingData))
            {
                MessageBox.Show("Configuration is not complete.");
            }
            else
            {
                selectedDatabase = cmbDatabases.Text;
                var settings = JsonConvert.DeserializeObject<SettingClass>(settingData);
                try
                {
                    using (SqlConnection con = new SqlConnection())
                    {
                        string connectionString = string.Format("Server={0},{4};Database={1};User Id={2};Password={3};", settings.SQLServer, "master", settings.SQLUsername, settings.SQLPassword, settings.SQLPort);
                        con.ConnectionString = connectionString;
                        using (var cmd = new SqlCommand())
                        {
                            cmd.CommandText = string.Format( "select QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) as tablename from {0}.INFORMATION_SCHEMA.TABLES order by tablename",cmbDatabases.Text);
                            cmd.Connection = con;
                            DataSet ds = new DataSet();
                            using (var da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(ds);
                                chkListObjects.Items.Clear();
                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    chkListObjects.Items.Add(dr["tablename"].ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    StaticObjects.log.Error(ex);
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnStartMigration_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }
        string selectedDatabase = string.Empty;

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                backgroundWorker1.ReportProgress(1);

                //Step 1 > Setting up task functions
                var script = File.ReadAllText("Scripts/Script1.1.sql");
                var settingData = StaticObjects.GetSettings();
                var settings = JsonConvert.DeserializeObject<SettingClass>(settingData);
                var xml = new StringBuilder();
                if (!Directory.Exists(settings.MigrationLocation + "/" + selectedDatabase))
                {
                    Directory.CreateDirectory(settings.MigrationLocation + "/" + selectedDatabase);
                    Directory.CreateDirectory(settings.MigrationLocation + "/" + selectedDatabase + "/Data/");
                }

                ExecutePatch(script, settings);
                backgroundWorker1.ReportProgress(2);
                script = File.ReadAllText("Scripts/Script1.2.sql");
                ExecutePatch(script, settings);
                backgroundWorker1.ReportProgress(3);
                script = File.ReadAllText("Scripts/Script1.3.sql");
                ExecutePatch(script, settings);
                backgroundWorker1.ReportProgress(4);
                script = File.ReadAllText("Scripts/Script1.4.sql");
                ExecutePatch(script, settings);
                backgroundWorker1.ReportProgress(5);
                script = File.ReadAllText("Scripts/Script1.5.sql");
                ExecutePatch(script, settings);
                backgroundWorker1.ReportProgress(6);
                script = File.ReadAllText("Scripts/Script1.6.sql");
                ExecutePatch(script, settings);
                backgroundWorker1.ReportProgress(7);
                script = File.ReadAllText("Scripts/Script1.7.sql");
                ExecutePatch(script, settings);
                backgroundWorker1.ReportProgress(8);
                script = File.ReadAllText("Scripts/Script1.8.sql");
                ExecutePatch(script, settings);

                bool ischecked = false;
                for (int i = 0; i <= chkListObjects.Items.Count - 1; i++)
                {
                    if (chkListObjects.GetItemChecked(i))
                    {
                        ischecked = true;
                    }
                }
                StringBuilder sb = new StringBuilder();

                if (ischecked)
                {
                    sb.Append("insert into  include_table_list " + Environment.NewLine);

                    for (int i = 0; i <= chkListObjects.Items.Count - 1; i++)
                    {
                        if (chkListObjects.GetItemChecked(i))
                        {
                            sb.Append(string.Format("select '{0}' " + Environment.NewLine, chkListObjects.Items[i].ToString().Replace("[", "").Replace("]", "").ToLower()));
                            sb.Append("union ");
                        }
                    }

                    var querylines = sb.ToString().Split(new[] { Environment.NewLine },
                            StringSplitOptions.None);
                    var querylist = querylines.ToList();
                    querylist.RemoveAt(querylist.Count - 1);
                    var query = string.Join(Environment.NewLine, querylist.ToArray());
                    ExecuteDataset(query, settings);
                }



                backgroundWorker1.ReportProgress(9);
                script = File.ReadAllText("Scripts/Script2.1.sql");
                script = script.Replace("{DBName}", string.Format("'{0}'", selectedDatabase));
                script = script.Replace("{TableNames}", string.Format("{0}", ""));
                var ds = ExecuteDataset(script, settings);
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    xml.Append(dr[0]);
                }
                File.WriteAllText(settings.MigrationLocation + "/" + selectedDatabase + "/V0001__SchemaPatch.sql", xml.ToString());
                backgroundWorker1.ReportProgress(10);

                script = File.ReadAllText("Scripts/Script2.2.sql");
                script = script.Replace("{DBName}", string.Format("'{0}'", selectedDatabase));
                script = script.Replace("{TableNames}", string.Format("{0}", ""));
                var dsxml = ExecuteDataset(script, settings);

                xml = new StringBuilder();
                foreach (DataRow dr in dsxml.Tables[0].Rows)
                {
                    xml.Append(dr[0]);
                }

                File.WriteAllText(settings.MigrationLocation + "/" + selectedDatabase + "/V0002__TablePatch.sql", xml.ToString());
                backgroundWorker1.ReportProgress(11);

                script = File.ReadAllText("Scripts/Script3.sql");
                script = script.Replace("{Path}", string.Format("'{0}'", settings.MigrationLocation + "/" + selectedDatabase + "/Data/"));
                script = script.Replace("{DBName}", string.Format("'{0}'", selectedDatabase));
                ExecutePatch(script, settings);
                backgroundWorker1.ReportProgress(12);


                script = File.ReadAllText("Scripts/Script4.sql");
                script = script.Replace("{Path}", string.Format("'{0}'", settings.MigrationLocation + "/" + selectedDatabase + "/Data/"));
                script = script.Replace("{DBName}", string.Format("'{0}'", selectedDatabase));
                script = script.Replace("{User}", string.Format("{0}", settings.SQLUsername));
                script = script.Replace("{Pass}", string.Format("{0}", settings.SQLPassword));
                script = script.Replace("{DB}", string.Format("{0}", selectedDatabase));
                var dsbcp = ExecuteDataset(script, settings);
                xml = new StringBuilder();
                foreach (DataRow dr in dsbcp.Tables[0].Rows)
                {
                    xml.Append(dr[0]);
                    xml.Append(Environment.NewLine);
                }
                File.WriteAllText(settings.MigrationLocation + "/" + selectedDatabase + "/BCP_Patch.bat", xml.ToString());
                backgroundWorker1.ReportProgress(13);


                script = File.ReadAllText("Scripts/Script5.sql");
                script = script.Replace("{Path}", string.Format("'{0}'", settings.MigrationLocation + "/" + selectedDatabase + "/Data/"));
                script = script.Replace("{TableNames}", string.Format("{0}", ""));
                var dscopy = ExecuteDataset(script, settings);
                xml = new StringBuilder();
                foreach (DataRow dr in dscopy.Tables[0].Rows)
                {
                    xml.Append(dr[1]);
                    xml.Append(Environment.NewLine);
                }
                File.WriteAllText(settings.MigrationLocation + "/" + selectedDatabase + "/V0003__CopyCommand.sql", xml.ToString());
                backgroundWorker1.ReportProgress(14);


                script = File.ReadAllText("Scripts/Script6.sql");
                script = script.Replace("{DBName}", string.Format("'{0}'", selectedDatabase));
                script = script.Replace("{TableNames}", string.Format("{0}", ""));
                var dsconstraints = ExecuteDataset(script, settings);
                xml = new StringBuilder();
                foreach (DataRow dr in dsconstraints.Tables[0].Rows)
                {
                    xml.Append(dr[0]);
                    xml.Append(Environment.NewLine);
                }
                File.WriteAllText(settings.MigrationLocation + "/" + selectedDatabase + "/V0004__Constraints.sql", xml.ToString());
                backgroundWorker1.ReportProgress(15);

                script = File.ReadAllText("Scripts/Script7.sql");
                script = script.Replace("{DBName}", string.Format("'{0}'", selectedDatabase));
                script = script.Replace("{TableNames}", string.Format("{0}", ""));
                var dsindex = ExecuteDataset(script, settings);
                xml = new StringBuilder();
                foreach (DataRow dr in dsindex.Tables[0].Rows)
                {
                    xml.Append(dr[0]);
                    xml.Append(Environment.NewLine);
                }
                File.WriteAllText(settings.MigrationLocation + "/" + selectedDatabase + "/V0005__IndexCommand.sql", xml.ToString());
                backgroundWorker1.ReportProgress(16);


                script = File.ReadAllText("Scripts/Script8.sql");
                script = script.Replace("{DBName}", string.Format("'{0}'", selectedDatabase));
                script = script.Replace("{TableNames}", string.Format("{0}", ""));
                var dsfkconstraints = ExecuteDataset(script, settings);
                xml = new StringBuilder();
                foreach (DataRow dr in dsfkconstraints.Tables[0].Rows)
                {
                    xml.Append(dr[0]);
                    xml.Append(Environment.NewLine);
                }
                File.WriteAllText(settings.MigrationLocation + "/" + selectedDatabase + "/V0006__ForeignKey.sql", xml.ToString());
                backgroundWorker1.ReportProgress(17);

            }
            catch (Exception ex)
            {
                StaticObjects.log.Error(ex);
            }
           
        }

        private void ExecutePatch(string script, SettingClass settings)
        {
            StaticObjects.log.Info("Query");
            StaticObjects.log.Info(script);
            using (var con = new SqlConnection())
            {
                string connectionString = string.Format("Server={0},{4};Database={1};User Id={2};Password={3};", settings.SQLServer, selectedDatabase, settings.SQLUsername, settings.SQLPassword, settings.SQLPort);
                con.ConnectionString = connectionString;
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.CommandText = script;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private object ExecuteScalar(string script, SettingClass settings)
        {
            using (var con = new SqlConnection())
            {
                string connectionString = string.Format("Server={0},{4};Database={1};User Id={2};Password={3};", settings.SQLServer, selectedDatabase, settings.SQLUsername, settings.SQLPassword, settings.SQLPort);
                con.ConnectionString = connectionString;
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.CommandText = script;
                    return cmd.ExecuteScalar();
                }
            }
        }

        private DataSet ExecuteDataset(string script, SettingClass settings)
        {
            DataSet ds = new DataSet();
            using (var con = new SqlConnection())
            {                
                string connectionString = string.Format("Server={0},{4};Database={1};User Id={2};Password={3};", settings.SQLServer, selectedDatabase, settings.SQLUsername, settings.SQLPassword, settings.SQLPort);
                con.ConnectionString = connectionString;
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = script;
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                }
            }
            return ds;
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                txtlogs.Text += "Migration started: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
            else if (e.ProgressPercentage == 2)
            {
                txtlogs.Text += "Dependency 1 delpoyed: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
            else if (e.ProgressPercentage == 3)
            {
                txtlogs.Text += "Dependency 2 delpoyed: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
            else if (e.ProgressPercentage == 4)
            {
                txtlogs.Text += "Dependency 3 delpoyed: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
            else if (e.ProgressPercentage == 5)
            {
                txtlogs.Text += "Dependency 4 delpoyed: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
            else if (e.ProgressPercentage == 6)
            {
                txtlogs.Text += "Dependency 5 delpoyed: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
            else if (e.ProgressPercentage == 7)
            {
                txtlogs.Text += "Dependency 6 delpoyed: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
            else if (e.ProgressPercentage == 8)
            {
                txtlogs.Text += "Schema patch complete: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
            else if (e.ProgressPercentage == 9)
            {
                txtlogs.Text += "Table patch complete: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
            else if (e.ProgressPercentage == 10)
            {
                txtlogs.Text += "Migration process setup complete: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
            else if (e.ProgressPercentage == 11)
            {
                txtlogs.Text += "BCP command build complete: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
            else if (e.ProgressPercentage == 12)
            {
                txtlogs.Text += "COPY patch build complete: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
            else if (e.ProgressPercentage == 13)
            {
                txtlogs.Text += "Constraints patch build complete: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
            else if (e.ProgressPercentage == 14)
            {
                txtlogs.Text += "Index patch build complete: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
            else if (e.ProgressPercentage == 15)
            {
                txtlogs.Text += "Foreign key patch build complete: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSelectAll.Checked)
            {
                for (int i = 0; i <= chkListObjects.Items.Count - 1; i++)
                {
                    chkListObjects.SetItemChecked(i, true);
                }
            }
            else
            {
                for (int i = 0; i <= chkListObjects.Items.Count - 1; i++)
                {
                    chkListObjects.SetItemChecked(i, false);
                }
            }
        }
    }
}
