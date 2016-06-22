using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.Web;

namespace CSharp.Utils.Diagnostics.Performance
{
    public class ExecutionFlow : FlowNode, IHtmlExportable
    {
        #region Static Fields

        private static long sequence;

        #endregion Static Fields

        #region Fields

        private Stack<FlowNode> stack = new Stack<FlowNode>();

        #endregion Fields

        #region Constructors and Finalizers

        public ExecutionFlow(string className, string methodName, IList<Pair<string, object>> arguments)
            : base(className, methodName, arguments)
        {
            this.Id = Interlocked.Increment(ref sequence);
            this.IsExecutionCompleted = false;
            this.stack.Push(this);
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public long Id { get; private set; }

        public bool IsExecutionCompleted { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void ExportAsHtml(TextWriter writer)
        {
            string tableId = "table" + this.Id.ToString(CultureInfo.InvariantCulture);

            writer.Write("<table id=\"");
            writer.Write(tableId.ToString(CultureInfo.InvariantCulture));
            writer.Write("\" class=\"treetable\">");
            writer.Write("<colgroup><col width=\"400\"/><col width=\"0*\"></colgroup>");
            writer.Write("<tr><th>ClassName</th><th>MethodName</th><th>ExecutedOn</th><th>TimeTaken</th><th>Exception</th></tr>");

            writer.Write("<tr id=\"");
            writer.Write(tableId);
            writer.Write("\">");

            for (int i = 0; i < this.Children.Count; i++)
            {
                this.writeChild(this.Children[i], writer, "&nbsp;", tableId + "_" + i.ToString(CultureInfo.InvariantCulture));
            }

            writer.Write("</tr></tablr>");
        }

        public void MethodCalled(string className, string methodName, IList<Pair<string, object>> arguments)
        {
            var node = new FlowNode(className, methodName, arguments);
            this.stack.Push(node);
        }

        public void MethodExecuted(Exception ex)
        {
            FlowNode node = this.stack.Pop();
            node.FinishedExecution(ex);
            if (this.stack.Count > 0)
            {
                FlowNode parent = this.stack.Peek();
                parent.AddChild(node);
            }
            else
            {
                this.IsExecutionCompleted = true;
                this.stack = null;
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        private void writeChild(FlowNode node, TextWriter writer, string prefix, string rowId)
        {
            writer.Write("<td>");
            writer.Write(prefix);
            if (node.Children != null)
            {
                writer.Write("<a href=\"#\" onclick=\"treetable_toggleRow('");
                writer.Write(rowId);
                writer.Write("');\"><img src=\"folder_green_open.png\" class=\"button\" alt=\"\" width=\"16\" height=\"16\" />");
                writer.Write(node.ClassName);
                writer.Write("</a>");
            }
            else
            {
                writer.Write(node.ClassName);
            }

            writer.Write("</td>");

            writer.Write("<td>");
            writer.Write(node.MethodName);
            writer.Write("</td>");

            writer.Write("<td>");
            writer.Write(node.ExecutedOn.ConvertDateTimeToString());
            writer.Write("</td>");

            writer.Write("<td class=\"number\">");
            writer.Write(node.TimeTakenForExecution.ToString(CultureInfo.InvariantCulture));
            writer.Write("</td>");

            writer.Write("<td>");
            if (this.Exception != null)
            {
                writer.Write(node.Exception.ToString());
            }

            writer.Write("</td>");
            if (node.Children != null)
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    this.writeChild(node.Children[i], writer, prefix + "&nbsp;", rowId + "_" + i.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        #endregion Methods
    }
}
