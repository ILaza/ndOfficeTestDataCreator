using System;
using System.Diagnostics;

namespace NetDocuments.Automation.Helpers.Extensions
{
    public static class EventLogEntryExtensions
    {
        public static string ToText(this EventLogEntry logEntry)
            => $"EntryType: {logEntry.EntryType}" +
               $"{Environment.NewLine}" +
               $"EventId: {logEntry.InstanceId}" +
               $"{Environment.NewLine}" +
               $"MachineName: {logEntry.MachineName}" +
               $"{Environment.NewLine}" +
               $"Source: {logEntry.Source}" +
               $"{Environment.NewLine}" +
               $"LogTime: {logEntry.TimeWritten}" +
               $"{Environment.NewLine}" +
               $"Message: {logEntry.Message}" +
               $"{Environment.NewLine}" +
               $"-----------------------------------------";
    }
}
