using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MergeAndSortLogs
{

    // This is a first step towards the Template design pattern, which will allow different implementations of the algorithm to merge and sort log files to be cleanly called and easily compared.
    // Certain optimisations are sacrificed when using this method, although the extensibility of the code is greatly increased.
    // Classes which inherit from this class will implement methods such as MergeAndSoftLogEntries with whichever merge/sort logic they choose. 
    // This allows for a clean and simple entry point to the solution and very simple benchmarking.

    abstract class MergeAndSortLogs
    {
        protected string OutputLogFile { get; set; }
        protected IEnumerable<string> InputLogFiles { get; set; }
        protected IEnumerable<LogEntry> MergedAndSortedLogEntries{get;set;}

        /// <summary>
        /// Parse the command line arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract (string outputLogFile, IEnumerable<string> inputLogFiles) ParseInputs(string[] args);

        /// <summary>
        /// Parse the input log files into a collection of LogEntries
        /// </summary>
        /// <param name="inputLogFiles"></param>
        /// <returns></returns>
        public abstract IEnumerable<LogEntry> ParseLogFiles(IEnumerable<string> inputLogFiles);

        /// <summary>
        /// Asynchronously merge and sort the log entries into a collection.
        /// </summary>
        /// <param name="logEntries"></param>
        /// <returns></returns>
        public abstract Task<IEnumerable<LogEntry>> MergeAndSortLogEntries(IEnumerable<LogEntry> logEntries);

        /// <summary>
        /// Write the merged and sorted collection of log entries to a file.
        /// </summary>
        /// <param name="mergedAndSortedLogEntries"></param>
        /// <returns></returns>
        public virtual async Task WriteToFile(IEnumerable<LogEntry> mergedAndSortedLogEntries)
        {

            using StreamWriter sr = new StreamWriter(OutputLogFile);

            foreach (var entry in mergedAndSortedLogEntries)
            {

                await sr.WriteLineAsync(entry.ToString());
            }
        }

        public async Task Run(string[] args)
        {
            ParseInputs(args);
            IEnumerable<LogEntry> logEntries = ParseLogFiles(InputLogFiles);
            IEnumerable<LogEntry> mergedAndSortedLogEntries = await MergeAndSortLogEntries(logEntries);
            await WriteToFile(mergedAndSortedLogEntries);
        }
    }
}
