﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AngelHornetLibrary.CLI
{
	public static class CliSystem
	{
		public static string FindFile(string filename) => FindFileInVisualStudio(filename, SearchOption.AllDirectories, SearchOption.AllDirectories);
		public static string FindFileInVisualStudio(string filename, SearchOption searchOption = SearchOption.AllDirectories, SearchOption continueOption = SearchOption.TopDirectoryOnly)
		{
			string pushd = Directory.GetCurrentDirectory();
			string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), filename, searchOption);
			if (files.Length == 0)
			{
				Directory.SetCurrentDirectory("..");
				// string value = directory.FindFileInVisualStudio(directory, filename, SearchOption.TopDirectoryOnly);
				string value = FindFileInVisualStudio(filename, continueOption);
				Directory.SetCurrentDirectory(pushd);
				return value;
			}
			else
			{
				Directory.SetCurrentDirectory(pushd);
				return files[0];
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

		public enum CliSleepDisplay{ None, Spin, Counter}
		public static void CliSleep(int seconds = 0, string? message = null, CliSleepDisplay display = CliSleepDisplay.None) 
		{
            for (int i = seconds; i > 0; i--)
            {
                Console.Write($"\r{message}");
				if (display == CliSleepDisplay.Spin) Console.Write($" {(i%4) switch { 0 => "|", 1 => "/", 2 => "-", 3 => "\\"}}");
				else if (display == CliSleepDisplay.Counter) Console.Write($" {i}");
                Task.Delay(1000).Wait();
            }
            if (message != null || display != CliSleepDisplay.None ) Console.WriteLine();
        }

    }

}
