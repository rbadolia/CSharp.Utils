using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.Xml;

namespace CSharp.Utils.Diagnostics.Performance
{
    [Serializable]
    public class FlowNode : IXmlExportable
    {
        #region Fields

        private List<FlowNode> children;

        private long ticksTaken;

        #endregion Fields

        #region Constructors and Finalizers

        public FlowNode(string className, string methodName, IList<Pair<string, object>> arguments)
        {
            this.ClassName = className;
            this.MethodName = methodName;
            this.Arguments = arguments;
            this.ExecutedOn = GlobalSettings.Instance.CurrentDateTime;
            this.ticksTaken = SharedStopWatch.ElapsedTicks;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public IList<Pair<string, object>> Arguments { get; private set; }

        public IList<FlowNode> Children
        {
            get
            {
                return this.children;
            }
        }

        public string ClassName { get; private set; }

        public Exception Exception { get; private set; }

        public DateTime ExecutedOn { get; private set; }

        public string MethodName { get; private set; }

        public long TimeTakenForExecution
        {
            get
            {
                return this.ticksTaken;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public void AddChild(FlowNode childNode)
        {
            if (this.children == null)
            {
                this.children = new List<FlowNode>();
            }

            this.children.Add(childNode);
        }

        public void WriteXml(TextWriter writer, string alignmentPrefix)
        {
            writer.Write(alignmentPrefix);
            writer.Write("<Method ");
            writer.Write("ClassName=\"");
            writer.Write(this.ClassName);
            writer.Write("\" MethodName=\"");
            writer.Write(this.MethodName);
            writer.Write("\" ExecutedOn=\"");
            writer.Write(this.ExecutedOn.ConvertDateTimeToString());
            writer.Write("\" TicksTaken=\"");
            writer.Write(this.TimeTakenForExecution.ToString(CultureInfo.InvariantCulture));
            if (!(this.Exception == null && this.children == null))
            {
                writer.Write("\">\r\n");
                string prefix = alignmentPrefix + "\t";
                if (this.Exception != null)
                {
                    this.exportExceptionToXml(writer, this.Exception, prefix);
                }

                if (this.children != null)
                {
                    foreach (FlowNode node in this.children)
                    {
                        node.WriteXml(writer, prefix);
                    }
                }

                writer.Write(alignmentPrefix);
                writer.Write("</Method>\r\n");
            }
            else
            {
                writer.Write("\"/>\r\n");
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        internal void FinishedExecution(Exception ex)
        {
            this.ticksTaken = SharedStopWatch.ElapsedTicks - this.ticksTaken;
            this.Exception = ex;
        }

        private void exportExceptionToXml(TextWriter writer, Exception ex, string alignmentPrefix)
        {
            writer.Write(alignmentPrefix);
            writer.Write("<Exception type=\"");
            writer.Write(ex.GetType().ToString());
            writer.Write("\" Event=\"");
            writer.Write(ex.StackTrace);
            writer.Write("\" StackTrace=\"");
            writer.Write(ex.StackTrace);
            writer.Write("\"");
            if (ex.InnerException != null)
            {
                writer.Write(">");
                this.exportExceptionToXml(writer, ex.InnerException, alignmentPrefix + "\t");
                writer.Write(alignmentPrefix);
                writer.Write("</Exception>\r\n");
            }
            else
            {
                writer.Write("\">\r\n");
            }
        }

        #endregion Methods
    }
}
