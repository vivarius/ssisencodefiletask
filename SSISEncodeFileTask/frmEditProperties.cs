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
        private Variables Variables
        {
            get { return _taskHost.Variables; }
        }

        private Connections Connections
        {
            get { return _connections; }
        }

        #endregion

        #region .ctor
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
                cmbFile.SelectedIndex = cmbFile.FindString(_taskHost.Properties[NamedStringMembers.FILE_CONNECTOR].GetValue(_taskHost).ToString());

                string selectedEncodingType = string.Empty;

                foreach (var item in cmbEncoding.Items.Cast<object>().Where(item => ((ComboBoxObjectComboItem)item).ValueMemeber.ToString() == _taskHost.Properties[NamedStringMembers.EncodingType].GetValue(_taskHost).ToString()))
                {
                    selectedEncodingType = ((ComboBoxObjectComboItem)item).DisplayMember.ToString();
                    break;
                }

                cmbEncoding.SelectedIndex = (cmbEncoding.FindString(selectedEncodingType));

                txSourceFile.Text = _taskHost.Properties[NamedStringMembers.FileSourceFile].GetValue(_taskHost).ToString();
                if (_taskHost.Properties[NamedStringMembers.SourceType].GetValue(_taskHost).ToString() == SourceFileType.FromFileConnector.ToString())
                {
                    opFileConnector.Checked = true;
                }
                else
                {
                    opFilePath.Checked = true;
                }

            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region Methods

        private void LoadFileConnections()
        {
            foreach (var connection in Connections.Cast<ConnectionManager>().Where(connection => connection.CreationName == "FILE"))
            {
                cmbFile.Items.Add(connection.Name);
            }
        }

        private void LoadEncodingTypes()
        {
            foreach (var listItem in FileEncodingTools.EncodingList)
            {
                cmbEncoding.Items.Add(new ComboBoxObjectComboItem(listItem[0], string.Format("{0} - {1}", listItem[1], listItem[2])));
            }
        }

        #endregion

        #region Events
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

        private void btSave_Click(object sender, EventArgs e)
        {
            _taskHost.Properties[NamedStringMembers.FILE_CONNECTOR].SetValue(_taskHost, cmbFile.Text);
            _taskHost.Properties[NamedStringMembers.FileSourceFile].SetValue(_taskHost, txSourceFile.Text);
            _taskHost.Properties[NamedStringMembers.EncodingType].SetValue(_taskHost, ((ComboBoxObjectComboItem)cmbEncoding.SelectedItem).ValueMemeber);
            _taskHost.Properties[NamedStringMembers.SourceType].SetValue(_taskHost, (opFileConnector.Checked)
                                                                                        ? SourceFileType.FromFileConnector.ToString()
                                                                                        : SourceFileType.FromFilePath.ToString());
            DialogResult = DialogResult.OK;
            Close();
        }
        #endregion
    }
}
