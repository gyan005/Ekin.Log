using Ekin.Log.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Ekin.Log
{
    public class LogFactory
    {
        #region Public properties

        public string Id { get; set; }

        public List<ILogItem> Errors { get; set; }
        public List<ILogItem> Warnings { get; set; }
        public List<ILogItem> Audits { get; set; }
        public List<string> TimeAudit { get; set; }

        public DataTableHelper DataTable { get; set; }

        public bool AllowDuplicateEntries { get; set; }
        public StringComparison DuplicateEntryComparisonType { get; set; }

        #endregion

        #region Events

        public delegate void ItemAdded(ILogItem item);
        public event ItemAdded OnItemAdded;

        public delegate void FileSaveFailed(Exception ex);
        public event FileSaveFailed OnFileSaveFailed;

        public delegate void EventLogSaveFailed(Exception ex);
        public event EventLogSaveFailed OnEventLogSaveFailed;

        #endregion

        #region Constructors

        public LogFactory()
        {
            Id = Guid.NewGuid().ToString("N");
            Errors = new List<ILogItem> { };
            Warnings = new List<ILogItem> { };
            Audits = new List<ILogItem> { };
            TimeAudit = new List<string> { };
            AllowDuplicateEntries = true;
            DuplicateEntryComparisonType = StringComparison.InvariantCultureIgnoreCase;
            DataTable = new DataTableHelper(this);
        }

        public LogFactory(FileInfo file) : this()
        {
            this.LoadFromFile(file);
        }

        public LogFactory(string json) : this()
        {
            this.LoadFromJson(json);
        }

        #endregion

        #region Add items or merge another log

        public void AddTimeAudit(string text)
        {
            TimeAudit.Add(string.Format("{0:yyyy-MM-dd hh:mm:ss} - {1}", DateTime.UtcNow, text));
        }

        public void AddError(string Class, string Function, string Message, string StackTrace, object Data)
        {
            if (AllowNewItem(Errors, Class, Function, Message, StackTrace))
            {
                Error error = new Error() { Id = Guid.NewGuid().ToString("N"), Time = DateTime.Now, Class = Class, Function = Function, Message = Message, StackTrace = StackTrace, Data = Data };
                Errors.Add(error);
                OnItemAdded?.Invoke(error);
            }
        }

        public void AddError(string Class, string Function, string Message, string StackTrace)
        {
            AddError(Class, Function, Message, StackTrace, null);
        }

        public void AddError(string Class, string Function, string Message, object Data)
        {
            AddError(Class, Function, Message, string.Empty, Data);
        }

        public void AddError(string Class, string Function, string Message)
        {
            AddError(Class, Function, Message, string.Empty, null);
        }

        public void AddError(string Class, string Function, Exception ex, object Data)
        {
            AddError(Class, Function, ex.ToString(), string.Empty, Data);
        }

        public void AddError(string Class, string Function, Exception ex)
        {
            AddError(Class, Function, ex.ToString(), string.Empty, null);
        }

        public void AddWarning(string Class, string Function, string Message, string StackTrace, object Data)
        {
            if (AllowNewItem(Warnings, Class, Function, Message, StackTrace))
            {
                Warning warning = new Warning() { Id = Guid.NewGuid().ToString("N"), Time = DateTime.Now, Class = Class, Function = Function, Message = Message, StackTrace = StackTrace, Data = Data };
                Warnings.Add(warning);
                OnItemAdded?.Invoke(warning);
            }
        }

        public void AddWarning(string Class, string Function, string Message, string StackTrace)
        {
            AddWarning(Class, Function, Message, StackTrace, null);
        }

        public void AddWarning(string Class, string Function, string Message, object Data)
        {
            AddWarning(Class, Function, Message, string.Empty, Data);
        }

        public void AddWarning(string Class, string Function, string Message)
        {
            AddWarning(Class, Function, Message, string.Empty, null);
        }

        public void AddAudit(string Class, string Function, string Message, string StackTrace, object Data)
        {
            if (AllowNewItem(Audits, Class, Function, Message, StackTrace))
            {
                Audit audit = new Audit() { Id = Guid.NewGuid().ToString("N"), Time = DateTime.Now, Class = Class, Function = Function, Message = Message, StackTrace = StackTrace, Data = Data };
                Audits.Add(audit);
                OnItemAdded?.Invoke(audit);
            }
        }

        public void AddAudit(string Class, string Function, string Message, string StackTrace)
        {
            AddAudit(Class, Function, Message, StackTrace, null);
        }

        public void AddAudit(string Class, string Function, string Message, object Data)
        {
            AddAudit(Class, Function, Message, string.Empty, Data);
        }

        public void AddAudit(string Class, string Function, string Message)
        {
            AddAudit(Class, Function, Message, string.Empty, null);
        }

        public void AddLog(LogFactory Log)
        {
            if (Log.HasErrors())
                Errors.AddRange(Log.Errors);

            if (Log.HasWarnings())
                Warnings.AddRange(Log.Warnings);

            if (Log.HasAudits())
                Audits.AddRange(Log.Audits);

            if (Log.HasTimeAudit())
                TimeAudit.AddRange(Log.TimeAudit);
        }

        public void MergeLogs(LogFactory Log)
        {
            AddLog(Log);
        }

        public void Assert(bool Condition, string Class, string Function, string Message, string DetailMessage = "", object Data = null)
        {
            if (!Condition)
            {
                AddError(Class, Function, Message, DetailMessage, Data);
            }
        }

        #endregion

        #region Helpers

        public bool HasErrors()
        {
            return (Errors?.Any()).GetValueOrDefault(false);
        }

        public bool HasWarnings()
        {
            return (Warnings?.Any()).GetValueOrDefault(false);
        }

        public bool HasAudits()
        {
            return (Audits?.Any()).GetValueOrDefault(false);
        }

        public bool HasTimeAudit()
        {
            return (TimeAudit?.Any()).GetValueOrDefault(false);
        }

        public bool HasItems()
        {
            return HasErrors() || HasWarnings() || HasAudits();
        }

        public string ToHtmlString()
        {
            string result = string.Empty;

            if (Errors?.Count > 0)
            {
                result += "<br><br>" + Environment.NewLine + "Errors:<br>" + Environment.NewLine;
                result += string.Join("<br>" + Environment.NewLine, Errors.Select(i => i.Message).ToList());
            }

            if (Warnings?.Count > 0)
            {
                result += "<br><br>" + Environment.NewLine + "Warnings:<br>" + Environment.NewLine;
                result += string.Join("<br>" + Environment.NewLine, Warnings.Select(i => i.Message).ToList());
            }

            if (Audits?.Count > 0)
            {
                result += "<br><br>" + Environment.NewLine + "Audits:<br>" + Environment.NewLine;
                result += string.Join("<br>" + Environment.NewLine, Audits.Select(i => i.Message).ToList());
            }

            if (TimeAudit?.Count > 0)
            {
                result += "<br><br>" + Environment.NewLine + "Time audit:<br>" + Environment.NewLine;
                result += string.Join("<br>", TimeAudit.Where(s => !String.IsNullOrWhiteSpace(s)));
            }

            return result;
        }

        #endregion

        #region File operations

        public void Save(string Filepath)
        {
            try
            {
                this.SaveToFile(Filepath);
            }
            catch (Exception ex)
            {
                OnFileSaveFailed?.Invoke(ex);
            }
        }

        public static string[] GetLogFiles(string Folder)
        {
            // May throw a permission error
            return new DirectoryInfo(Folder).GetFiles().Select(o => o.Name).Where(i => i.EndsWith(".log", StringComparison.InvariantCultureIgnoreCase)).ToArray();
        }

        #endregion

        #region Event Log

        public bool WriteToEventLog(string AppName, bool CreateOneEntryPerLogItem = false)
        {
            try
            {
                if (!EventLog.SourceExists(AppName)) EventLog.CreateEventSource(AppName, AppName);
                if (Errors.Any())
                {
                    WriteEventLogEntries(AppName, EventLogEntryType.Error, Errors, CreateOneEntryPerLogItem);
                }
                if (Warnings.Any())
                {
                    WriteEventLogEntries(AppName, EventLogEntryType.Warning, Warnings, CreateOneEntryPerLogItem);
                }
                if (Audits.Any())
                {
                    WriteEventLogEntries(AppName, EventLogEntryType.SuccessAudit, Audits, CreateOneEntryPerLogItem);
                }
                if (TimeAudit.Any())
                {
                    WriteEventLogEntries(AppName, EventLogEntryType.Error, TimeAudit, false);
                }
                return true;
            }
            catch (Exception ex)
            {
                OnEventLogSaveFailed?.Invoke(ex);
                return false;
            }
        }

        private void WriteEventLogEntries(string appName, EventLogEntryType entryType, List<ILogItem> items, bool createOneEntryPerLogItem = false)
        {
            string messages = string.Empty;
            foreach (ILogItem item in items)
            {
                string message = string.Format("{0} {1}: {2}", item.Class, item.Function, item.Message);
                if (createOneEntryPerLogItem)
                {
                    EventLog.WriteEntry(appName, message, entryType);
                }
                else
                {
                    messages += message + Environment.NewLine;
                }
            }
            if (!createOneEntryPerLogItem)
            {
                EventLog.WriteEntry(appName, messages, entryType);
            }
        }

        private void WriteEventLogEntries(string appName, EventLogEntryType entryType, List<string> items, bool createOneEntryPerLogItem = false)
        {
            string messages = string.Empty;
            foreach (string item in items)
            {
                if (createOneEntryPerLogItem)
                {
                    EventLog.WriteEntry(appName, item, entryType);
                }
                else
                {
                    messages += item + Environment.NewLine;
                }
            }
            if (!createOneEntryPerLogItem)
            {
                EventLog.WriteEntry(appName, messages, entryType);
            }
        }

        #endregion

        #region Private Helpers

        private bool AllowNewItem(List<ILogItem> items, string Class, string Function, string Message, string StackTrace = "")
        {
            return AllowDuplicateEntries || !items.Any(i => (string.IsNullOrWhiteSpace(Class) || Class.Equals(i.Class, DuplicateEntryComparisonType)) &&
                                                            (string.IsNullOrWhiteSpace(Function) || Function.Equals(i.Function, DuplicateEntryComparisonType)) &&
                                                            (string.IsNullOrWhiteSpace(Message) || Message.Equals(i.Message, DuplicateEntryComparisonType)) &&
                                                            (string.IsNullOrWhiteSpace(StackTrace) || StackTrace.Equals(i.StackTrace, DuplicateEntryComparisonType)));
        }

        #endregion
    }
}
