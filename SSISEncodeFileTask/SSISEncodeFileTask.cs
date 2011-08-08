using System;
using System.ComponentModel;
using System.Xml;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using DTSExecResult = Microsoft.SqlServer.Dts.Runtime.DTSExecResult;
using DTSProductLevel = Microsoft.SqlServer.Dts.Runtime.DTSProductLevel;
using VariableDispenser = Microsoft.SqlServer.Dts.Runtime.VariableDispenser;

namespace SSISEncodeFileTask100.SSIS
{
    [DtsTask(
        DisplayName = "Encoding File Task",
        UITypeName = "SSISEncodeFileTask100.SSISEncodeFileTaskUIInterface" +
        ",SSISEncodeFileTask100," +
        "Version=1.2.0.0," +
        "Culture=Neutral," +
        "PublicKeyToken=236ec97d37527d44",
        IconResource = "SSISEncodeFileTask100.FileEncodeIcon.ico",
        TaskContact = "cosmin.vlasiu@gmail.com",
        RequiredProductLevel = DTSProductLevel.Standard
        )]
    public class SSISEncodeFileTask : Task, IDTSComponentPersist
    {

        #region Constructor
        public SSISEncodeFileTask()
        {
        }

        #endregion

        #region Public Properties
        [Category("Component specifics"), Description("The File connector")]
        public string FileConnector { get; set; }
        [Category("Component specifics"), Description("Source File Path")]
        public string FileSourcePathInVariable { get; set; }
        [Category("Component specifics"), Description("Source Type")]
        public string SourceType { get; set; }
        [Category("Component specifics"), Description("Encoding")]
        public string EncodingType { get; set; }
        #endregion

        #region Private Properties

        Variables _vars = null;

        #endregion

        #region Validate

        /// <summary>
        /// Validate properties
        /// </summary>
        public override DTSExecResult Validate(Connections connections, VariableDispenser variableDispenser,
                                               IDTSComponentEvents componentEvents, IDTSLogging log)
        {
            bool isBaseValid = true;

            if (base.Validate(connections, variableDispenser, componentEvents, log) != DTSExecResult.Success)
            {
                componentEvents.FireError(0, "SSISEncodeFileTask", "Base validation failed", "", 0);
                isBaseValid = false;
            }

            if (string.IsNullOrEmpty(EncodingType))
            {
                componentEvents.FireError(0, "SSISEncodeFileTask", "An encode type is required.", "", 0);
                isBaseValid = false;
            }

            if (string.IsNullOrEmpty(SourceType))
            {
                componentEvents.FireError(0, "SSISEncodeFileTask", "A source type is required.", "", 0);
                isBaseValid = false;
            }

            if (SourceFileType.FromFileConnector.ToString() == SourceType)
            {
                if (string.IsNullOrEmpty(FileConnector))
                {
                    componentEvents.FireError(0, "SSISEncodeFileTask", "A FILE connector is required.", "", 0);
                    isBaseValid = false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(FileSourcePathInVariable))
                {
                    componentEvents.FireError(0, "SSISEncodeFileTask", "A file path is required.", "", 0);
                    isBaseValid = false;
                }
            }

            return isBaseValid ? DTSExecResult.Success : DTSExecResult.Failure;
        }

        #endregion

        #region Execute

        /// <summary>
        /// This method is a run-time method executed dtsexec.exe
        /// </summary>
        /// <param name="connections"></param>
        /// <param name="variableDispenser"></param>
        /// <param name="componentEvents"></param>
        /// <param name="log"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public override DTSExecResult Execute(Connections connections, VariableDispenser variableDispenser, IDTSComponentEvents componentEvents, IDTSLogging log, object transaction)
        {
            bool refire = false;

            componentEvents.FireInformation(0,
                                                "SSISEncodeFileTask",
                                                "Prepare variables",
                                                string.Empty,
                                                0,
                                                ref refire);


            if (!string.IsNullOrEmpty(FileSourcePathInVariable))
                GetNeededVariables(variableDispenser, FileSourcePathInVariable);

            componentEvents.FireInformation(0,
                                            "SSISEncodeFileTask",
                                            string.Format("Source File: \"{0}\"", EvaluateExpression(FileSourcePathInVariable, variableDispenser)),
                                            string.Empty,
                                            0,
                                            ref refire);

            try
            {
                
                componentEvents.FireInformation(0,
                                                "SSISEncodeFileTask",
                                                EncodeFile(connections, variableDispenser, componentEvents)
                                                    ? "The file has been encoded successfully."
                                                    : "The file has NOT been encoded.",
                                                string.Empty,
                                                0,
                                                ref refire);
            }
            catch (Exception ex)
            {
                componentEvents.FireError(0,
                                          "SSISEncodeFileTask",
                                          string.Format("Problem: {0}", ex.Message),
                                          string.Empty,
                                          0);

                return DTSExecResult.Failure;
            }
            finally
            {
                if (_vars.Locked)
                {
                    _vars.Unlock();
                }
            }

            return base.Execute(connections, variableDispenser, componentEvents, log, transaction);
        }

        /// <summary>
        /// Method use to encode the flat file
        /// </summary>
        /// <param name="connections"></param>
        /// <param name="variableDispenser"></param>
        /// <param name="componentEvents"></param>
        /// <returns></returns>
        private bool EncodeFile(Connections connections, VariableDispenser variableDispenser, IDTSComponentEvents componentEvents)
        {
            bool retval = false;
            bool refire = false;

            try
            {
                string filePath = SourceType == SourceFileType.FromFileConnector.ToString()
                                        ? connections[FileConnector].ConnectionString
                                        : EvaluateExpression(FileSourcePathInVariable, variableDispenser).ToString();

                componentEvents.FireInformation(0,
                                                "SSISEncodeFileTask",
                                                string.Format("Encode file {0} || Encode in {1}", filePath, EncodingType),
                                                string.Empty,
                                                0,
                                                ref refire);


                retval = new FileEncodingTools(filePath, Convert.ToInt32(EncodingType)).Encode(componentEvents);
            }
            catch (Exception exception)
            {
                componentEvents.FireError(0,
                                        "SSISEncodeFileTask",
                                        string.Format("EncodeFile Error :{0} {1} {2}", exception.Message, exception.Source, exception.StackTrace),
                                        string.Empty,
                                        0);
            }

            return retval;
        }

        #endregion

        #region Methods
        /// <summary>
        /// This method evaluate expressions like @([System::TaskName] + [System::TaskID]) or any other operation created using 
        /// ExpressionBuilder
        /// </summary>
        /// <param name="mappedParam"></param>
        /// <param name="variableDispenser"></param>
        /// <returns></returns>
        private static object EvaluateExpression(string mappedParam, VariableDispenser variableDispenser)
        {
            object variableObject = null;
            try
            {
                var expressionEvaluatorClass = new ExpressionEvaluatorClass
                {
                    Expression = mappedParam
                };

                expressionEvaluatorClass.Evaluate(DtsConvert.GetExtendedInterface(variableDispenser), out variableObject, false);
            }
            catch
            {
                variableObject = mappedParam;
            }
            return variableObject;
        }

        /// <summary>
        /// Unlock the "File Path Variable" variable
        /// </summary>
        /// <param name="variableDispenser"></param>
        /// <param name="variableExpression"></param>
        private void GetNeededVariables(VariableDispenser variableDispenser, string variableExpression)
        {
            try
            {
                var mappedParams = variableExpression.Split(new[] { "@" }, StringSplitOptions.RemoveEmptyEntries);

                for (int index = 0; index < mappedParams.Length - 1; index++)
                {
                    var param = mappedParams[index].Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[1];
                    variableDispenser.LockForRead(param.Substring(0, param.IndexOf(']')));
                }
            }
            catch
            {
                //We will continue...
            }

            variableDispenser.GetVariables(ref _vars);
        }

        #endregion

        #region Implementation of IDTSComponentPersist

        /// <summary>
        /// Save task's properties into the internal serializing xml model
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="infoEvents"></param>
        void IDTSComponentPersist.SaveToXML(XmlDocument doc, IDTSInfoEvents infoEvents)
        {
            XmlElement taskElement = doc.CreateElement(string.Empty, "SSISEncodeFileTask", string.Empty);

            XmlAttribute fileConnector = doc.CreateAttribute(string.Empty, Keys.FILE_CONNECTOR, string.Empty);
            fileConnector.Value = FileConnector;

            XmlAttribute fileSourceFile = doc.CreateAttribute(string.Empty, Keys.FileSourcePathInVariable, string.Empty);
            fileSourceFile.Value = FileSourcePathInVariable;

            XmlAttribute sourceType = doc.CreateAttribute(string.Empty, Keys.SourceType, string.Empty);
            sourceType.Value = SourceType;

            XmlAttribute encodingType = doc.CreateAttribute(string.Empty, Keys.EncodingType, string.Empty);
            encodingType.Value = EncodingType;

            taskElement.Attributes.Append(fileConnector);
            taskElement.Attributes.Append(fileSourceFile);
            taskElement.Attributes.Append(sourceType);
            taskElement.Attributes.Append(encodingType);

            doc.AppendChild(taskElement);
        }

        /// <summary>
        /// Load properties from the internal serializing xml model
        /// </summary>
        /// <param name="node"></param>
        /// <param name="infoEvents"></param>
        void IDTSComponentPersist.LoadFromXML(XmlElement node, IDTSInfoEvents infoEvents)
        {
            if (node.Name != "SSISEncodeFileTask")
            {
                throw new Exception("Unexpected task element when loading task.");
            }

            try
            {
                FileConnector = node.Attributes.GetNamedItem(Keys.FILE_CONNECTOR).Value;
                FileSourcePathInVariable = node.Attributes.GetNamedItem(Keys.FileSourcePathInVariable).Value;
                SourceType = node.Attributes.GetNamedItem(Keys.SourceType).Value;
                EncodingType = node.Attributes.GetNamedItem(Keys.EncodingType).Value;
            }
            catch
            {

            }
        }

        #endregion
    }
}
