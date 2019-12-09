using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekin.Log
{
    public class DataTableHelper
    {
        public bool IncludeErrors { get; set; } = true;
        public bool IncludeWarnings { get; set; } = true;
        public bool IncludeAudits { get; set; } = true;

        public string DateFormat { get; set; } = "yyyy/MM/dd HH:mm:ss";

        public string LogTypeLabel { get; set; } = "Log Type";
        public string DateTimeLabel { get; set; } = "Date & Time";
        public string ClassLabel { get; set; } = "Class";
        public string FunctionLabel { get; set; } = "Function";
        public string MessageLabel { get; set; } = "Message";
        public string StackTraceLabel { get; set; } = "Stack Trace";

        public string ErrorText { get; set; } = "Error";
        public string WarningText { get; set; } = "Warning";
        public string AuditText { get; set; } = "Audit";

        private LogFactory Logs { get; set; }

        public DataTableHelper(LogFactory Logs)
        {
            this.Logs = Logs;
        }

        public DataTable GetNew()
        {
            System.Data.DataTable table = new System.Data.DataTable();
            table.Clear();
            table.Columns.Add(LogTypeLabel);
            table.Columns.Add(DateTimeLabel);
            table.Columns.Add(ClassLabel);
            table.Columns.Add(FunctionLabel);
            table.Columns.Add(MessageLabel);
            table.Columns.Add(StackTraceLabel);

            if (IncludeErrors && Logs.HasErrors())
            {
                foreach (Error error in Logs.Errors)
                {
                    DataRow row = table.NewRow();
                    row[LogTypeLabel] = ErrorText;
                    row[DateTimeLabel] = error.Time.ToString(DateFormat);
                    row[ClassLabel] = error.Class;
                    row[FunctionLabel] = error.Function;
                    row[MessageLabel] = error.Message;
                    row[StackTraceLabel] = error.StackTrace;
                    table.Rows.Add(row);
                }
            }

            if (IncludeWarnings && Logs.HasWarnings())
            {
                foreach (Warning warning in Logs.Warnings)
                {
                    DataRow row = table.NewRow();
                    row[LogTypeLabel] = WarningText;
                    row[DateTimeLabel] = warning.Time.ToString(DateFormat);
                    row[ClassLabel] = warning.Class;
                    row[FunctionLabel] = warning.Function;
                    row[MessageLabel] = warning.Message;
                    row[StackTraceLabel] = warning.StackTrace;
                    table.Rows.Add(row);
                }
            }

            if (IncludeAudits && Logs.HasAudits())
            {
                foreach (Audit audit in Logs.Audits)
                {
                    DataRow row = table.NewRow();
                    row[LogTypeLabel] = AuditText;
                    row[DateTimeLabel] = audit.Time.ToString(DateFormat);
                    row[ClassLabel] = audit.Class;
                    row[FunctionLabel] = audit.Function;
                    row[MessageLabel] = audit.Message;
                    row[StackTraceLabel] = audit.StackTrace;
                    table.Rows.Add(row);
                }
            }

            return table;
        }
    }
}
