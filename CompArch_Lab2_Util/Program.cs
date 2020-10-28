using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

// Вариант 10. Find a subdirectory with the given name within the other directory

namespace CompArch_Lab2_Util
{
    class Program
    {
        private static readonly string exeDirPath = Environment.CurrentDirectory;

        private static string toSearch = null;

        private static string specifiedPath = null;

        private static List<DirectoryInfo> satisfyingDirs = new List<DirectoryInfo>();

        private static bool shouldIncludeHidden = false;
        private static bool shouldSearchOnlyHidden = false;

        private static StringCollection log = new StringCollection();
        
        // Values, returned by Main
        // 0 - all is good
        // 1 - bad arguments
        // 2 - access error
        public static int Main(string[] args)
        {
            int argsLen = args.Length;

            if (argsLen == 0 || args[0] == @"/?")
            {
                Console.WriteLine("\n---\tFind a subdirectory with the given name in the directory.\t---\n");
                Console.WriteLine("\t - Use /H to include hidden directories");
                Console.WriteLine("\t - Use /h to search for ONLY hidden directories");
                Console.WriteLine("\t - Then write subdirectory name to search in current directory as root");
                Console.WriteLine("\t - To search in an arbitrary directory, specify its full path after the name of the subdirectory.\n");
                Console.WriteLine(@"Example: /H target C:\All_Files\UNIVER\Архитектура компьютера\CompArch_Lab2_Util\CompArch_Lab2_Util\bin\Debug");
                Console.WriteLine(@"Example: *target* C:\All_Files\UNIVER\Архитектура компьютера\CompArch_Lab2_Util\CompArch_Lab2_Util\bin\Debug");
                Console.WriteLine(@"Example: *target");
                return 0;
            }
            if (argsLen > 3)
            {
                Console.WriteLine("Bad arguments! Try /?\n");
                return 1;
            }

            if (!TryParseParameters(args))
            {
                return 1;
            }

            // detect where to search
            string path = null;
            if (specifiedPath != null)
            {
                path = specifiedPath;
            }
            else
            {
                path = exeDirPath;
            }


            try
            {
                WalkDirectoryTree(new DirectoryInfo(path), toSearch);
            }
            catch (NotSupportedException e)
            {
                log.Add(e.Message);
            }
            catch (ArgumentException e)
            {
                log.Add(e.Message);
            }
            catch (Exception e)
            {
                log.Add(e.Message);
            }

            if (log.Count > 0)
            {
                Console.WriteLine($"{log.Count} errors occurred:");
                foreach (string err in log)
                {
                    Console.WriteLine(err);
                }

                return 2;
            }
            
            if (satisfyingDirs.Count > 0)
            {
                foreach (var satisfyingDir in satisfyingDirs)
                {
                    Console.WriteLine(satisfyingDir.FullName);
                }
                Console.WriteLine($"\nTotal: {satisfyingDirs.Count} directories found.");
            }
            else
            {
                Console.WriteLine($"There is no any directory in '{path}' that matches our input '{toSearch}'");
            }

            return 0;
        }

        private static void WalkDirectoryTree(DirectoryInfo root, string subDirName)
        {
            DirectoryInfo[] subDirs = null;
            DirectoryInfo[] despiteAtributesDirs = null;

            try
            {
                // all dirs
                subDirs = root.GetDirectories();
                // dirs that satisfy our target folder name
                despiteAtributesDirs = root.GetDirectories(subDirName);

                if (shouldIncludeHidden)
                {
                    satisfyingDirs.AddRange(despiteAtributesDirs);
                }
                else if (shouldSearchOnlyHidden)
                {
                    foreach (var dir in despiteAtributesDirs)
                    {
                        //if (dir.Attributes == FileAttributes.Hidden)
                        if ((dir.Attributes & FileAttributes.Hidden) != 0)
                        {
                            satisfyingDirs.Add(dir);
                        }
                    }
                }
                else
                {
                    foreach (var dir in despiteAtributesDirs)
                    {
                        //if (dir.Attributes != FileAttributes.Hidden)
                        if ((dir.Attributes & FileAttributes.Hidden) == 0)
                        {
                            satisfyingDirs.Add(dir);
                        }
                    }
                }

                foreach (var subDir in subDirs)
                {
                    WalkDirectoryTree(subDir, subDirName);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                log.Add(e.Message);
            }
            catch (Exception e)
            {
                log.Add(e.Message);
            }
        }

        private static bool TryParseParameters(string[] args)
        {
            // check for options
            int i = 0;
            while (i < args.Length && args[i].StartsWith("/"))
            {
                if (args[i].Equals("/H"))
                {
                    shouldIncludeHidden = true;
                }
                else if (args[i].Equals("/h"))
                {
                    shouldSearchOnlyHidden = true;
                }
                ++i;
            }

            // if we have both parameters - bad
            if (shouldIncludeHidden && shouldSearchOnlyHidden)
            {
                return false;
            }
            // if there's nothing left - bad
            if (i >= args.Length)
            {
                return false;
            }

            // name for folder to search
            toSearch = args[i];
            ++i;

            // name of the directory to search into (if was specified)
            if (i < args.Length)
            {
                specifiedPath = args[i];
            }

            return true;
        }
    }
}
