using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace MergeAndSortLogs
{
    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                Stopwatch totalTime = Stopwatch.StartNew();
                // Validate and convert args into well named variables
                (string outputLogFile, string[] inputLogFiles) = ValidateArgsAndPopulateVariables(args);

                // I have opted for a LinkedList data structure here to benefit from fast inserts and improve efficiency when handling large datasets.
                // Memory footprint will be increased, but that was not considered to be a limiting factor in normal use cases for a tool like this.
                LinkedList<LogEntry> outputLogEntries = new LinkedList<LogEntry>();

                // Define Regex pattern to match the date/time of the entry.
                string pattern = "([0-9]{4}-[0-9]{2}-[0-9]{2}[ ][0-9]{2}:[0-9]{2}:[0-9]{2}[.][0-9]{3})";

                foreach (string inputLogFile in inputLogFiles)
                {
                    string[] logSections = ConvertLogFileToSections(inputLogFile, pattern);

                    Console.WriteLine("\nCreating LogEntry objects and inserting to the linked list in chronological order...\n");
                    for (int i = 0; i < logSections.Length; i++)
                    {
                        // Create a LogEntry object, only when a datetime section is found
                        if (!Regex.IsMatch(logSections[i], pattern))
                        {
                            continue;
                        }

                        Stopwatch createObjectTimer = Stopwatch.StartNew();
                        DateTime dateTime = DateTime.Parse(logSections[i]);

                        LogEntry logEntry;

                        // If the section following the current one is not another dateTime, let it be the LogEntry message.
                        if (!DateTime.TryParse(logSections[i + 1], out _))
                        {
                            logEntry = new LogEntry()
                            {
                                EntryDateTime = dateTime,
                                Message = logSections[i + 1]
                            };
                            i++; // Increment the index by an extra 1, since we know the next section doesn't contain another dateTime.
                        }
                        //Otherwise, set the message as an empty string.
                        else
                        {
                            logEntry = new LogEntry()
                            {
                                EntryDateTime = dateTime,
                                Message = String.Empty
                            };
                        }

                        Stopwatch insertTimer = Stopwatch.StartNew();
                        // The first log entry examined should be added to our linked list without comparing against anything (since there is nothing to compare against)
                        if (outputLogEntries.Count == 0)
                        {
                            outputLogEntries.AddLast(logEntry);
                            continue;
                        }

                        // All other log entries should be added before the first log entry that has a later date than the current log entry.
                        InsertLogEntryIntoLinkedList(outputLogEntries, logEntry);
                    }
                    Console.WriteLine($"\nSuccessfully merged log entries from {inputLogFile} to the linked list.\n");
                }

                // Once outputLogEntries is populated with sorted entries from all input files ->
                WriteLogEntriesToFile(outputLogFile, outputLogEntries);
                Console.WriteLine($"Inserted {outputLogEntries.Count} log entries in {totalTime.ElapsedMilliseconds}ms.");
            }
            catch (FileNotFoundException ex)
            {
                // Opportunity to do logging and if using an interactive UI could redirect the user to a helpful place.
                Console.WriteLine(ex.Message);
                return;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                //Log as appropriate.
                Console.WriteLine(ex.Message + ex.StackTrace);
                return;
            }

        }


        /// <summary>
        /// Examines the linked list and inserts the LogEntry object in the correct chronological place.
        /// </summary>
        /// <param name="outputLogEntries"></param>
        /// <param name="logEntry"></param>
        static void InsertLogEntryIntoLinkedList(LinkedList<LogEntry> outputLogEntries, LogEntry logEntry)
        {
            int lengthOfLinkedList = outputLogEntries.Count;

            var currentNode = outputLogEntries.First;

            for (int j = 0; j < lengthOfLinkedList; j++)
            {
                // If the entry has an earlier datetime than the current item in the list, we add it to the list before that item.
                if (currentNode.Value.EntryDateTime.CompareTo(logEntry.EntryDateTime) > 0)
                {
                    outputLogEntries.AddBefore(currentNode, logEntry);
                    break;
                }
                // If we have reached the end of the list and all other items have an earlier datetime than the current entry, we add it to the end.
                else if (currentNode.Next == null)
                {
                    outputLogEntries.AddLast(logEntry);
                }
                // Otherwise we should move onto the next item in the list and repeat. 
                else
                {
                    currentNode = currentNode.Next;
                }
            }
        }


        /// <summary>
        /// Takes a log file and splits it into sections based upon the Regex pattern provided
        /// </summary>
        /// <param name="inputLogFile"></param>
        /// <returns></returns>
        static string[] ConvertLogFileToSections(string inputLogFile, string regexPattern)
        {
            using StreamReader inputReader = new StreamReader(inputLogFile);

            // Read the log file into one long string.
            Console.WriteLine($"\nReading {inputLogFile} into memory as a string...");
            Stopwatch readToStringTimer = Stopwatch.StartNew();
            string logFileAsString = inputReader.ReadToEnd();
            Console.WriteLine($"\n\tRead log file to string in {readToStringTimer.ElapsedMilliseconds}ms.\n");

            // Split this string into log sections containing entry times and messages.
            Console.WriteLine("\nSplitting the log string into sections containing entry times and messages...");
            Stopwatch splitStringTimer = Stopwatch.StartNew();
            string[] logSections = Regex.Split(logFileAsString, regexPattern, RegexOptions.Multiline);
            Console.WriteLine($"\n\tSections were identified in {splitStringTimer.ElapsedMilliseconds}ms.\n");
            return logSections;
        }

        /// <summary>
        /// Writes IEnumerable<LogEntry> to the specified filePath.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="enumerable"></param>
        static void WriteLogEntriesToFile(string filePath, IEnumerable<LogEntry> enumerable)
        {
            Console.WriteLine("Writing linked list to file.");
            Stopwatch outputWritingTimer = Stopwatch.StartNew();
            using StreamWriter sr = new StreamWriter(filePath);
            foreach (var item in enumerable)
            {
                sr.WriteLine(item.ToString());
            }
            Console.WriteLine($"Write to file completed in {outputWritingTimer.ElapsedMilliseconds}ms.");
        }

        /// <summary>
        /// Validates command line arguments and populates output/input log file variables.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static (string outputLogFile, string[] inputLogFiles) ValidateArgsAndPopulateVariables(string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("Invalid inputs. Usage: [.exe name] {inputlogfile1} ... {inputlogfilen} {outputlogfile}");
            }

            int numberOfInputLogFiles = args.Length - 1;

            string[] inputLogFiles = new string[numberOfInputLogFiles];

            for (int i = 0; i < numberOfInputLogFiles; i++)
            {
                inputLogFiles[i] = args[i];

                if (!File.Exists(inputLogFiles[i]))
                {
                    throw new FileNotFoundException($"\nThe specified input file '{inputLogFiles[i]}' does not exist. Please try again.");
                }
                Console.WriteLine($"\nInput log file path {i + 1}: {inputLogFiles[i]}");
            }


            // Point of extensibility, we could add a warning to users if the output file already exists "This file already exists, are you sure you want to overwrite it?"
            string outputLogFile = args[^1];
            string outputFileName = outputLogFile.Split('\\')[^1];
            string outputDirectory = outputLogFile.Remove(outputLogFile.LastIndexOf(outputFileName));

            if (!Directory.Exists(outputDirectory))
            {
                throw new DirectoryNotFoundException($"\nThe output log file directory specified '{outputDirectory}' does not exist. Please try again.");
            }

            Console.WriteLine($"\nOutput log file path: {outputLogFile}");

            return (outputLogFile, inputLogFiles);

        }
    }
}
