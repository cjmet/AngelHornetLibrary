using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;

namespace AngelHornetLibrary.CLI
{


    public class AhsUtil
    {
        public void GetFilesRef(string path, string searchPattern, ref List<string> result, ref string status, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (!Directory.Exists(path)) return;
            EnumerationOptions opt = new EnumerationOptions();
            opt.IgnoreInaccessible = true;
            opt.RecurseSubdirectories = false;
            opt.MatchCasing = MatchCasing.CaseInsensitive;

            string[] fileList = Directory.GetFiles(path, searchPattern, opt);
            foreach (string file in fileList)
                result.Add(file);

            string[] dirList = Directory.GetDirectories(path, "*", opt);
            foreach (string dir in dirList)
                if (searchOption == SearchOption.AllDirectories)
                {
                    status = Path.Combine(path, dir);
                    GetFilesRef(Path.Combine(path, dir), searchPattern, ref result, ref status, searchOption);
                }
        }
    }



    public static class CliSystem
    {


        // FindFileInVisualStudio("filename", SearchOption, TraverseSideWays, TraverseSolution, TraverseUp);
        // Spider Current Dir, Traverse Sideways and Spider, Traverse Up No Spider, Spider at Destination and Return.
        // continueOption is obsolete, but left in for compatibility.
        public static string? FindFile(string filename) => FindFileInVisualStudio(filename, SearchOption.AllDirectories, SearchOption.TopDirectoryOnly, TraverseSideways: false, TraverseSolution: false, TraverseUp: false);
        static bool CliWarning = false;
        public static string? FindFileInVisualStudio(string filename, SearchOption searchOption = SearchOption.AllDirectories, SearchOption continueOption = SearchOption.AllDirectories, bool TraverseSideways = true, bool TraverseSolution = false, bool TraverseUp = true)
        {
            if (!CliWarning)
            {
                Debug.WriteLine("\nWARNING: FindFileInVisualStudio is only intended for use while debugging COMMAND LINE only apps inside the Visual Studio IDE.\n");
                CliWarning = true;
            }
            string cwd = Directory.GetCurrentDirectory();
            string pushd = cwd;
            string[] files = Array.Empty<string>();
            try
            {
                files = Directory.GetFiles(cwd, filename, searchOption).Where(file => (File.GetAttributes(file) & FileAttributes.ReparsePoint) == 0).ToArray();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"ERROR: FindFileInVisualStudio: Exception");
                Console.Error.WriteLine($"\tDirectory: {cwd}");
                Console.Error.WriteLine($"\tSearchFor: {filename}");
                Console.Error.WriteLine($"\tOptions: {searchOption}");
                Console.Error.WriteLine($"\tException: {e.Message}");
                return null;
            }
            if (files.Length == 0 && TraverseSideways)
            {
                TraverseSideways = false;  // We tried to traverse sideways, but failed, so don't try again
                // get projectdir from env
                string? solutionDir = Environment.GetEnvironmentVariable("SolutionDir");
                if (string.IsNullOrEmpty(solutionDir))
                {
                    solutionDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                if (!string.IsNullOrEmpty(solutionDir))
                {
                    Directory.SetCurrentDirectory(solutionDir);
                    string? value = FindFileInVisualStudio(filename, searchOption, continueOption, TraverseSideways, TraverseSolution, TraverseUp);
                    return value;
                }

            }
            // check for .sln in current directory, if found set TraverseUp to false
            string[] SolutionFiles = Directory.GetFiles(cwd, "*.sln", SearchOption.TopDirectoryOnly);
            if (!TraverseSolution && SolutionFiles.Length > 0)
            {
                if (TraverseUp)
                {
                    TraverseUp = false;
                    string? value = FindFileInVisualStudio(filename, continueOption, continueOption, TraverseSideways, TraverseSolution, TraverseUp);
                    return value;
                }
            }
            if (files.Length == 0 && TraverseUp)
            {
                Directory.SetCurrentDirectory("..");
                string newcwd = Directory.GetCurrentDirectory();
                string? value = "";
                if (newcwd == cwd)
                {
                    TraverseUp = false;
                    value = FindFileInVisualStudio(filename, continueOption, continueOption, TraverseSideways, TraverseSolution, TraverseUp);
                    return value;
                }
                value = FindFileInVisualStudio(filename, SearchOption.TopDirectoryOnly, continueOption, TraverseSideways, TraverseSolution, TraverseUp);  // Don't spider while climbing.
                Directory.SetCurrentDirectory(pushd);
                return value;
            }
            else
            {
                Directory.SetCurrentDirectory(pushd);
                if (files.Length > 0) return files[0];
                else return null;
            }
        }


        public static void Assert([DoesNotReturnIf(false)] bool condition, string? message = null, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            if (!condition)
            {
                Console.WriteLine($"Assert failed: {message} in {file} at {member}:{line}");
                throw new Exception(message);
            }
        }

        // info, Warn, Error, DIE!
        public static void Info(string? message = null, string level = "info") => Die(message, level);
        public static void Warn(string? message = null, string level = "warn") => Die(message, level);
        public static void Error(string? message = null, string level = "error") => Die(message, level);

        public static void Die(string? message = null, string level = "DIE!", [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            Console.WriteLine($"{level}: {message} in {file} at {member}:{line}");
            if (level == "DIE!") throw new Exception(message);
        }

        public enum CliSleepDisplay { None, Spin, Counter }
        public static void CliSleep(int seconds = 0, string? message = null, CliSleepDisplay display = CliSleepDisplay.None)
        {
            for (int i = seconds; i > 0; i--)
            {
                Console.Write($"\r{message}");
                if (display == CliSleepDisplay.Spin) Console.Write($" {(i % 4) switch { 0 => "|", 1 => "/", 2 => "-", 3 => "\\" }}");
                else if (display == CliSleepDisplay.Counter) Console.Write($" {i}");
                Task.Delay(1000).Wait();
            }
            if (message != null || display != CliSleepDisplay.None) Console.WriteLine();
        }

    }

}
