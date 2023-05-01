using System;
using System.IO;
using System.Timers;
using System.Threading;

class FolderSync
{
    static string consoleLogs = "";
    static void Main(string[] args)
    {
        logConsoleLine("Enter the source path:");
        string sourceDir = Console.ReadLine();
        consoleLogs += sourceDir + '\n';
        logConsoleLine("Enter the destination path:");
        string destinationDir = Console.ReadLine();
        consoleLogs += destinationDir + '\n';
        logConsoleLine("Enter the log folder path:");
        string logFilePath = Console.ReadLine();
        consoleLogs += logFilePath + '\n';
        logConsoleLine("After how many seconds should the folder auto-sync:");
        string synctime = Console.ReadLine();
        consoleLogs += synctime + '\n';
        bool isLogging = true;
        string LogFileName = $"{logFilePath}\\logs_{DateTime.Now:dd_MM_yyyy-HH_mm_ss}.txt";
        bool recursive = true;
        bool firstLogging = true;

        //Created a timer to periodically trigger the copying operation after how many seconds the user imput from the keyboard.
        int interval = int.Parse(synctime) * 1000;                                                          
        System.Timers.Timer timer = new System.Timers.Timer(interval);
        timer.Elapsed += (sender, e) =>
        {
            CopyDirectory(sourceDir, destinationDir, recursive);
            logConsoleLine($"Directory synchronized at {DateTime.Now}");
        };
        timer.AutoReset = true;
        timer.Enabled = true;

        // Create a FileSystemWatcher to monitor the source directory for changes
        FileSystemWatcher watcher = new FileSystemWatcher(sourceDir);
        watcher.IncludeSubdirectories = recursive;
        watcher.Created += (sender, e) => logConsoleLine($"File created: {e.FullPath} at {DateTime.Now}");
        watcher.Deleted += (sender, e) => logConsoleLine($"File deleted: {e.FullPath} at {DateTime.Now}");
        watcher.EnableRaisingEvents = true;

        //Instructions with the features present in the script for the user
        Console.WriteLine('\n' + $"Logging started at at {DateTime.Now}");
        Console.WriteLine("Type !logs to stop logging");
        Console.WriteLine("Type !resume_logs to resume logging");
        Console.WriteLine("Type !sync to synchronize the directories");
        Console.WriteLine("Type !exit to exit");
        Console.WriteLine("PLEASE NOTE that every time after logs stops, the user has to resume logs manually in order to not lose data");


        using (var stream = new FileStream(LogFileName, FileMode.Create, FileAccess.Write))
        using (var writer = new StreamWriter(stream))

       //Program keeps running in a loop until the user imput any of the following commands
            while (true)
            {
                if (firstLogging)
                {
                    writer.WriteLine(consoleLogs);
                    writer.Flush();
                    firstLogging = false;
                }

                string userImput = Console.ReadLine();
                consoleLogs = userImput + '\n';
                if (userImput == "!sync")                                                                   
                {
                    CopyDirectory(sourceDir, destinationDir, recursive);
                    logConsoleLine($"Directory synchronized at {DateTime.Now}");
                }
                else if (userImput == "!logs")
                {
                    isLogging = false;
                    logConsoleLine($"Logging stopped at {DateTime.Now}. Please resume logs in order to not lose data");
                }
                else if (userImput == "!resume_logs")
                {
                    isLogging = true;
                    logConsoleLine($"Logging resumed at {DateTime.Now}");
                }
                else if (userImput == "!exit")
                {
                    logConsoleLine($"Application stopped at {DateTime.Now}");
                    break;
                }
            }
    }

    static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {  
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);                                                                                               

        foreach (FileSystemInfo item in new DirectoryInfo(destinationDir).GetFileSystemInfos())
        {
        // Delete files and directories in destination directory that do not exist in source directory
            string sourceItemPath = Path.Combine(sourceDir, item.Name);
            if (!item.Exists || (item.Attributes & FileAttributes.Directory) == FileAttributes.Directory && !Directory.Exists(sourceItemPath))
            {
                item.Delete();
            }
            else if (item is FileInfo file && !File.Exists(sourceItemPath))
            {
        // Delete file if it doesn't exist in the source directory
                if (IsFileTypeToDelete(file.Extension))                                     
                {
                    file.Delete();
                }
            }
        }

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }

        // Delete subdirectories in destination directory that do not exist in source directory
            foreach (DirectoryInfo item in new DirectoryInfo(destinationDir).GetDirectories())
            {
                string sourceItemPath = Path.Combine(sourceDir, item.Name);
                if (!item.Exists || !Directory.Exists(sourceItemPath))
                {
                    item.Delete(true);
                }
            }
        }
    }

    // List the file types to delete if they don't exist in the source directory
    static bool IsFileTypeToDelete(string extension)
    {                                                                          
        string[] fileTypesToDelete = new string[]
        { /*Audio file extensions:*/ ".aif", ".cda", ".mid", ".midi", "mp3", ".mpa", ".ogg", ".wav", ".wma", ".wpl",
          /*Compressed file extensions:*/ ".7z", ".arj", ".deb", ".pkg", ".rar", ".rpm", ".tar", ".gz", ".z", ".zip",
          /*Disc and media file extensions:*/ ".bin", ".dmg", ".iso", ".toast", ".vcd",
          /*Data and database file extensions:*/ ".csv", ".dat", ".db", ".dbf", ".log", ".mdb", ".sav", ".sql", ".tar", ".xml", ".accdb",
          /*E-mail file extensions:*/ ".email", ".eml", ".emlx", ".msg", ".oft", ".pst", ".vcf",
          /*Executable file extensions:*/ ".apk", ".bat", ".cgi", ".pl", ".com", ".exe,", ".gadget", ".jar", ".msi", ".py", ".wsf",
          /*Font file extensions:*/ ".fnt", ".fon", ".oft", ".ttf",
          /*Image file extensions:*/ ".ai", ".bmp", ".gif", ".ico", "jpeg", ".jpg", ".png", ".ps", ".psd", ".svg", ".tif", ".tiff", ".webp",
          /*Internet-related file extensions:*/ ".asp", ".aspx", ".cer", ".cfm", ".cgi", ".css", ".htm", ".html", ".js", ".jsp", ".part", ".php", ".py", ".rss", ".xhtml",            
          /*Presentation file extensions:*/ ".key", ".odp", ".pps", ".ppt", ".pptx",
          /*Programming file extensions:*/ ".c", ".cgi", ".pl", ".class", ".cpp", ".cs", ".h", ".java", ".php", ".py", ".sh", ".swift", ".vb",
          /*Spreadsheet file extensions:*/ ".ods", ".xls", ".xlsm", ".xlsx",
          /*System related file extensions:*/ ".bak", ".cab", ".cfg", ".cpl", ".cur", ".dll", ".dmp", ".drv", ".icns", ".ico", ".ini", ".lnk", ".msi", ".sys", ".tmp",
          /*Video file extensions:*/ ".3g2", ".3gp", ".avi", ".flv", ".h264", ".m4v", ".mkv", ".mov", ".mpg", ".mpeg", ".rm", ".swf", ".vob", ".webm", ".wmv",
          /*Word processor and text file extensions:*/ ".doc", ".docx", ".odt", ".pdf", ".rtf", ".tex", ".txt", ".wpd"};


        return Array.Exists(fileTypesToDelete, x => x.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }

    // Every line that is written inside the console app is saved and then written inside the log file.
    static void logConsoleLine(string message)
    {
        Console.WriteLine(message);
        consoleLogs += message;
        consoleLogs += '\n';
    }
}