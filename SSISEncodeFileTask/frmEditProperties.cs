using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.DataTransformationServices.Controls;
using Microsoft.SqlServer.Dts.Runtime;

namespace SSISEncodeFileTask100
{
    public partial class frmEditProperties : Form
    {
        #region Private Properties
        private TaskHost _taskHost;
        private Connections _connections;
        #endregion

        #region Public Properties

        private Connections Connections
        {
            get { return _connections; }
        }

        #endregion

        #region .ctor
        /// <summary>
        /// Initialize form
        /// </summary>
        /// <param name="taskHost"></param>
        /// <param name="connections"></param>
        public frmEditProperties(TaskHost taskHost, Connections connections)
        {
            InitializeComponent();

            _taskHost = taskHost;
            _connections = connections;

            if (taskHost == null)
            {
                //throw new ArgumentNullException("taskHost");
            }

            try
            {
                LoadFileConnections();
                LoadEncodingTypes();

                if (_taskHost.Properties[Keys.FILE_CONNECTOR].GetValue(_taskHost) != null)
                    cmbFile.SelectedIndex = cmbFile.FindString(_taskHost.Properties[Keys.FILE_CONNECTOR].GetValue(_taskHost).ToString());

                string selectedEncodingType = string.Empty;

                if (_taskHost.Properties[Keys.EncodingType].GetValue(_taskHost) != null)
                    foreach (var item in cmbEncoding.Items.Cast<object>().Where(item => ((ComboBoxObjectComboItem)item).ValueMemeber.ToString() == _taskHost.Properties[Keys.EncodingType].GetValue(_taskHost).ToString()))
                    {
                        selectedEncodingType = ((ComboBoxObjectComboItem)item).DisplayMember.ToString();
                        break;
                    }

                cmbEncoding.SelectedIndex = (cmbEncoding.FindString(selectedEncodingType));

                if (_taskHost.Properties[Keys.FileSourcePathInVariable].GetValue(_taskHost) != null)
                    txSourceFile.Text = _taskHost.Properties[Keys.FileSourcePathInVariable].GetValue(_taskHost).ToString();

                if (_taskHost.Properties[Keys.SourceType].GetValue(_taskHost) != null)
                {
                    if (_taskHost.Properties[Keys.SourceType].GetValue(_taskHost).ToString() == SourceFileType.FromFileConnector.ToString())
                        opFileConnector.Checked = true;
                    else
                        opFilePath.Checked = true;
                }

                if (_taskHost.Properties[Keys.AutodetectSourceEncodingType].GetValue(_taskHost) != null)
                {
                    chkAutodetectEncoding.Checked = (bool)_taskHost.Properties[Keys.AutodetectSourceEncodingType].GetValue(_taskHost);
                    cmbEncodingSource.Enabled = !chkAutodetectEncoding.Checked;

                    if (_taskHost.Properties[Keys.SourceEncodingType].GetValue(_taskHost) != null)
                    {
                        foreach (var item in cmbEncodingSource.Items.Cast<object>().Where(item => ((ComboBoxObjectComboItem)item).ValueMemeber.ToString() == _taskHost.Properties[Keys.SourceEncodingType].GetValue(_taskHost).ToString()))
                        {
                            selectedEncodingType = ((ComboBoxObjectComboItem)item).DisplayMember.ToString();
                            break;
                        }

                        cmbEncodingSource.SelectedIndex = (cmbEncodingSource.FindString(selectedEncodingType));
                    }
                    else
                    {
                        cmbEncodingSource.SelectedIndex = -1;
                    }
                }
                else
                {
                    chkAutodetectEncoding.Checked = true;
                    cmbEncodingSource.Enabled = false;
                }

                txReadWriteBuffer.Text = _taskHost.Properties[Keys.ReadWriteBuffer].GetValue(_taskHost) != null
                                            ? _taskHost.Properties[Keys.ReadWriteBuffer].GetValue(_taskHost).ToString()
                                            : "1024";
            }
            catch (Exception)
            {

            }
            finally
            {
                Switcher();
            }

        }
        #endregion

        #region Methods

        /// <summary>
        /// Obtain the list  of File Type connectors
        /// </summary>
        private void LoadFileConnections()
        {
            foreach (var connection in Connections.Cast<ConnectionManager>().Where(connection => connection.CreationName == "FILE"))
            {
                cmbFile.Items.Add(connection.Name);
            }
        }

        /// <summary>
        /// Load encoding types -> FileEncodingTools.cs // FileEncodingTools class
        /// </summary>
        private void LoadEncodingTypes()
        {
            foreach (var comboBoxItem in FileEncodingTools.EncodingList.Select(listItem => new ComboBoxObjectComboItem(listItem[0], string.Format("{0} - {1}", listItem[1], listItem[2]))))
            {
                cmbEncoding.Items.Add(comboBoxItem);
                cmbEncodingSource.Items.Add(comboBoxItem);
            }
        }

        private void Switcher()
        {
            if (opFileConnector.Checked)
            {
                lbFilePath.Enabled = false;
                txSourceFile.Enabled = false;
                btExpressionSource.Enabled = false;
                lbFileConnection.Enabled = true;
                cmbFile.Enabled = true;
            }

            if (opFilePath.Checked)
            {
                lbFilePath.Enabled = true;
                txSourceFile.Enabled = true;
                btExpressionSource.Enabled = true;
                lbFileConnection.Enabled = false;
                cmbFile.Enabled = false;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Expression bulder handlig to obtain an expression
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btExpressionSource_Click(object sender, EventArgs e)
        {
            using (ExpressionBuilder expressionBuilder = ExpressionBuilder.Instantiate(_taskHost.Variables, _taskHost.VariableDispenser, typeof(string), txSourceFile.Text))
            {
                if (expressionBuilder.ShowDialog() == DialogResult.OK)
                {
                    txSourceFile.Text = expressionBuilder.Expression;
                }
            }
        }

        /// <summary>
        /// Save the interface's controls values 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSave_Click(object sender, EventArgs e)
        {
            int readWriteBuffer = 1024;
            Int32.TryParse(txReadWriteBuffer.Text.Trim(), out readWriteBuffer);
            _taskHost.Properties[Keys.ReadWriteBuffer].SetValue(_taskHost, readWriteBuffer);

            _taskHost.Properties[Keys.EncodingType].SetValue(_taskHost, Convert.ToInt32(((ComboBoxObjectComboItem)cmbEncoding.SelectedItem).ValueMemeber.ToString()));

            _taskHost.Properties[Keys.FILE_CONNECTOR].SetValue(_taskHost, cmbFile.Text);
            _taskHost.Properties[Keys.FileSourcePathInVariable].SetValue(_taskHost, txSourceFile.Text);
            _taskHost.Properties[Keys.EncodingType].SetValue(_taskHost, Convert.ToInt32(((ComboBoxObjectComboItem)cmbEncoding.SelectedItem).ValueMemeber.ToString()));
            _taskHost.Properties[Keys.SourceType].SetValue(_taskHost, opFileConnector.Checked
                                                                            ? SourceFileType.FromFileConnector.ToString()
                                                                            : SourceFileType.FromFilePath.ToString());

            _taskHost.Properties[Keys.AutodetectSourceEncodingType].SetValue(_taskHost, chkAutodetectEncoding.Checked);
            _taskHost.Properties[Keys.SourceEncodingType].SetValue(_taskHost, !chkAutodetectEncoding.Checked
                                                                                    ? Convert.ToInt32(((ComboBoxObjectComboItem)cmbEncodingSource.SelectedItem).ValueMemeber.ToString())
                                                                                    : 1);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(linkLabel1.Text);
            }
            catch { }
        }

        private void opFileConnector_Click(object sender, EventArgs e)
        {
            Switcher();
        }

        private void opFilePath_Click(object sender, EventArgs e)
        {
            Switcher();
        }

        private void chkAutodetectEncoding_Click(object sender, EventArgs e)
        {
            cmbEncodingSource.Enabled = !chkAutodetectEncoding.Checked;
        }

        #endregion


    }
}
