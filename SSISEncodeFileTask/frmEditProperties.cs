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
                cmbFile.SelectedIndex = cmbFile.FindString(_taskHost.Properties[Keys.FILE_CONNECTOR].GetValue(_taskHost).ToString());

                string selectedEncodingType = string.Empty;

                foreach (var item in cmbEncoding.Items.Cast<object>().Where(item => ((ComboBoxObjectComboItem)item).ValueMemeber.ToString() == _taskHost.Properties[Keys.EncodingType].GetValue(_taskHost).ToString()))
                {
                    selectedEncodingType = ((ComboBoxObjectComboItem)item).DisplayMember.ToString();
                    break;
                }

                cmbEncoding.SelectedIndex = (cmbEncoding.FindString(selectedEncodingType));

                txSourceFile.Text = _taskHost.Properties[Keys.FileSourceFile].GetValue(_taskHost).ToString();
                if (_taskHost.Properties[Keys.SourceType].GetValue(_taskHost).ToString() == SourceFileType.FromFileConnector.ToString())
                {
                    opFileConnector.Checked = true;
                }
                else
                {
                    opFilePath.Checked = true;
                }

                Switcher();
            }
            catch (Exception)
            {

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
            foreach (var listItem in FileEncodingTools.EncodingList)
            {
                cmbEncoding.Items.Add(new ComboBoxObjectComboItem(listItem[0], string.Format("{0} - {1}", listItem[1], listItem[2])));
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
            _taskHost.Properties[Keys.FILE_CONNECTOR].SetValue(_taskHost, cmbFile.Text);
            _taskHost.Properties[Keys.FileSourceFile].SetValue(_taskHost, txSourceFile.Text);
            _taskHost.Properties[Keys.EncodingType].SetValue(_taskHost, ((ComboBoxObjectComboItem)cmbEncoding.SelectedItem).ValueMemeber);
            _taskHost.Properties[Keys.SourceType].SetValue(_taskHost, (opFileConnector.Checked)
                                                                                        ? SourceFileType.FromFileConnector.ToString()
                                                                                        : SourceFileType.FromFilePath.ToString());
            DialogResult = DialogResult.OK;
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel1.Text);
        }

        private void opFileConnector_Click(object sender, EventArgs e)
        {
            Switcher();
        }

        private void opFilePath_Click(object sender, EventArgs e)
        {
            Switcher();
        }
        #endregion
    }
}
