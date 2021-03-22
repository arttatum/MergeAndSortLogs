# Merge-and-sort-log-files
A .NET Core console app that merges multiple log files into one output log, where entries are sorted in chronological order.

Author: William Loughney

To package the console application into a single .exe file, navigate to the solution folder and run the command: dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true

This will create an .exe in the folder ...\Merge and sort log files\bin\Release\netcoreapp3.1\win-x64\publish that you can use by calling:

"Merge and sort log files..exe" {inputLogFilePath1} ... {inputLogFilePathN} {outputLogfilePath} from the directory containing the .exe

This console application will expect log entries in the format: 

2018-06-29 14:14:46.675 Hello User!2018-06-29 14:15:00.123 Goodbye User!
2018-06-29 14:15:00.123 Goodbye User!

It does, however, handle entries such as this:

2018-06-29 14:14:46.675 Hello User!2018-06-29 14:15:00.123 Goodbye User!2018-06-29 14:14:46.675 Hello User!2018-06-29 14:15:00.123 Goodbye User!

or 

2018-06-29 14:14:46.675 Hello User!2018-06-29 14:15:00.123 Goodbye User!2018-06-29 14:15:00.1232018-06-29 14:15:00.1232018-06-29 14:15:00.123

Try it out and see for yourself :)



Considerations for further development:

- adding temp LogEntry objects. Allows comparison of several datetimes in the sorted list vs current LogEntry.EntryDateTime and speed up insert speed by reducing length of linked list scanned.
- comparison of different approaches to improve efficiency, https://en.wikipedia.org/wiki/Sorting_algorithm#Efficient_sorts
- https://dofactory.com/net/template-method-design-pattern This design pattern may be appropriate so the steps of the algorithm remain consistent, but we can easily benchmark the performance of different approaches.
