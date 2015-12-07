﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using System.Data.Odbc;

namespace SQLDep
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeValues();
        }


        private void InitializeValues()
        {
            this.comboBoxDatabase.SelectedIndex = this.GetDatabaseTypeIdx(UIConfig.Get(UIConfig.SQL_DIALECT, "mssql"));
            this.comboBoxAuthType.SelectedIndex = this.GetAuthTypeIdx(UIConfig.Get(UIConfig.AUTH_TYPE, "sql_auth"));
            this.textBoxServerName.Text = UIConfig.Get(UIConfig.SERVER_NAME, "");
            this.textBoxPort.Text = UIConfig.Get(UIConfig.SERVER_PORT, "");
            this.textBoxLoginName.Text = UIConfig.Get(UIConfig.LOGIN_NAME, "");
            this.textBoxLoginPassword.Text = UIConfig.Get(UIConfig.LOGIN_PASSWORD, "");
            this.textBoxUserName.Text = UIConfig.Get(UIConfig.DATA_SET_NAME, "My Data Set Name");
            this.textBoxDatabaseName.Text = UIConfig.Get(UIConfig.DATABASE_NAME, "master");
            this.textBoxKey.Text = UIConfig.Get(UIConfig.SQLDEP_KEY, "12345678-1234-1234-1234-123456789012");
            this.buttonRun.Enabled = false;
            this.EnableAuthSettings();
        }

        public void EnableAuthSettings()
        {
            if ( this.GetAuthTypeName(this.comboBoxAuthType.SelectedIndex) == "sql_auth")
            {
                this.textBoxLoginName.Enabled = true;
                this.textBoxLoginPassword.Enabled = true;
            }
            else
            {
                this.textBoxLoginName.Enabled = false;
                this.textBoxLoginPassword.Enabled = false;
            }
        }

        private int GetDatabaseTypeIdx (string sqlDialect)
        {
            if (sqlDialect == "oracle")
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        private string GetDatabaseTypeName(int idx)
        {
            if (idx == 0)
            {
                return "oracle";
            }
            else
            {
                return "mssql";
            }
        }


        private int GetAuthTypeIdx(string authType)
        {
            if (authType == "sql_auth")
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        private string GetAuthTypeName(int idx)
        {
            if (idx == 0)
            {
                return "sql_auth";
            }
            else
            {
                return "win_auth";
            }
        }

        private string BuildConnectionString (DBExecutor dbExecutor)
        {
            return dbExecutor.BuildConnectionString(this.GetDatabaseTypeName(this.comboBoxDatabase.SelectedIndex),
                                                this.GetAuthTypeName(this.comboBoxAuthType.SelectedIndex),
                                                this.textBoxServerName.Text,
                                                this.textBoxPort.Text, 
                                                this.textBoxDatabaseName.Text,
                                                this.textBoxLoginName.Text,
                                                this.textBoxLoginPassword.Text);
        }

        private void SaveDialogSettings ()
        {
            UIConfig.Set(UIConfig.AUTH_TYPE, this.GetAuthTypeName(this.comboBoxAuthType.SelectedIndex));
            UIConfig.Set(UIConfig.SQL_DIALECT, this.GetDatabaseTypeName(this.comboBoxDatabase.SelectedIndex));
            UIConfig.Set(UIConfig.DATA_SET_NAME, this.textBoxUserName.Text.ToString());
            UIConfig.Set(UIConfig.SQLDEP_KEY, this.textBoxKey.Text.ToString());
            UIConfig.Set(UIConfig.SERVER_NAME, this.textBoxServerName.Text.ToString());
            UIConfig.Set(UIConfig.SERVER_PORT, this.textBoxPort.Text.ToString());
            UIConfig.Set(UIConfig.LOGIN_NAME, this.textBoxLoginName.Text.ToString());
            UIConfig.Set(UIConfig.LOGIN_PASSWORD, this.textBoxLoginPassword.Text.ToString());
            UIConfig.Set(UIConfig.DATABASE_NAME, this.textBoxDatabaseName.Text.ToString());
        }


        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            this.buttonRun.Enabled = false;
            string form1Text = this.Text;
            this.Text = form1Text + " Running - Please Wait ... ";
            try
            {
                this.SaveDialogSettings();
                DBExecutor dbExecutor = new DBExecutor();
                this.BuildConnectionString(dbExecutor);

                string myName = this.textBoxUserName.Text.ToString();
                string myKey = this.textBoxKey.Text.ToString();
                string sqlDialect = this.GetDatabaseTypeName(this.comboBoxDatabase.SelectedIndex);


                // na test lze pouzit: "356d0c42-8717-495d-ad6b-339cd6e530fb"
                // go!
                //Guid myGuid;
                //if (!Guid.TryParse(myKey, out myGuid))
                //{
                //    throw new Exception("Invalid key, it is not possible to handle as quide!");
                //}

                List<string> failedDbs = new List<string>();
                Executor executor = new Executor(dbExecutor);

                string exportFileName = fbd.SelectedPath + "\\DBexport_" + this.textBoxServerName.Text + "_" + DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + ".json";
                executor.Run(myName, myKey, sqlDialect, exportFileName);

                DialogResult answer = MessageBox.Show("Send data to SQLdep?", "Please confirm data sending.", MessageBoxButtons.YesNo);

                if (answer == DialogResult.Yes)
                {
                    executor.SendStructure();
                    MessageBox.Show("Completed succesfully. Data were sent!");
                }
                else
                {
                    MessageBox.Show("Completed succesfully. Data are saved on disk! " + exportFileName);
                }

                executor = null; // zahod vysledky
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                MessageBox.Show(msg);
            }
            this.buttonRun.Enabled = true;
            this.Text = form1Text;
        }

        private void comboBoxAuthType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.EnableAuthSettings();
        }

        private void buttonTestConnection_Click(object sender, EventArgs e)
        {
            DBExecutor dbExecutor = new DBExecutor();
            string connection = this.BuildConnectionString(dbExecutor);

            try
            {

                dbExecutor.Connect();
                dbExecutor.Close();
                this.buttonRun.Enabled = true;
                this.SaveDialogSettings();
                MessageBox.Show("Database connected!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database not connected! \n\nConnection string:\n" + connection + "\n\nError: " + ex.ToString());
            }
        }
    }
}
