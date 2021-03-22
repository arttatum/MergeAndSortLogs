using System;
using System.Collections.Generic;
using System.Text;

namespace MergeAndSortLogs
{
    // Define a LogEntry object with two properties of type DateTime String
    // If this was an application built on scale, this should be separated from the console app. For a console app with a single purpose, a nested class is acceptable.
    class LogEntry
    {
        public DateTime EntryDateTime { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{EntryDateTime.Year}-{EntryDateTime.Month}-{EntryDateTime.Day} {EntryDateTime.Hour}:{EntryDateTime.Minute}:{EntryDateTime.Second}.{EntryDateTime.Millisecond} {Message.Trim()}";
        }
    }
}
