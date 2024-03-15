using System.Collections.Concurrent;
using static AngelHornetLibrary.AhLog;
using System.Diagnostics;

namespace AngelHornetLibrary
{

    public class AhFileIO
    {
        // Both these over-ride some file locks: FileShare.ReadWrite
        public async IAsyncEnumerable<string> AhReadLinesAsync(string path, CancellationToken? token = null)
        {
            await foreach (var line in PollLinesAsync(path, 0, token)) yield return line;
        }

        // Both these over-ride some file locks: FileShare.ReadWrite
        public async IAsyncEnumerable<string> PollLinesAsync(string path, int pollIntervalSeconds = 1, CancellationToken? token = null)
        {
            CancellationToken _token;
            if (token == null) _token = new CancellationToken();
            else _token = (CancellationToken)token;
            int filePos = 0;
            int fileSize = 0;

            do
            {
                using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileStream.Seek(filePos, SeekOrigin.Begin);
                    using (var streamReader = new System.IO.StreamReader(fileStream))
                    {
                        string line;
                        while ((line = await streamReader.ReadLineAsync()) != null)
                        {
                            if (_token.IsCancellationRequested) yield break;
                            yield return line;
                        }
                        filePos = (int)fileStream.Position;
                        fileSize = (int)fileStream.Length;
                        if (pollIntervalSeconds > 0) await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds));
                    }
                }
            } while (_token.IsCancellationRequested == false && pollIntervalSeconds > 0);
        }
    }
}




public class AhGetFiles
{
    private readonly TimeSpan GetFilesAsyncCancel = TimeSpan.FromMinutes(60);     //  Default Cancellation Token Timeout.


    // convert string path -> List<string> paths
    public async IAsyncEnumerable<string> GetFilesAsync(
        // standard options and syntax                                  // --- Standard Options ---
        string path,                                                    // string path or List<string> paths
        string searchPattern = "*",                                     // File Search Pattern
        SearchOption searchOption = SearchOption.AllDirectories,        // Search Option
        EnumerationOptions? fileOptions = null,                         // Enumeration Options, change these at your own peril.
                                                                        // async options                                                // --- Async Options ---
        IProgress<string>? iprogress = null,                             // Reports directory search progress every 250ms
        CancellationToken? token = null)                                // *** WARNING *** If you pass in a CancellationToken, you'll need
                                                                        // to cancel it when finished so the IProgress Task will exit as well.
                                                                        // Cancellation Defaults to GetFilesAsyncCancel, 1 hour,
                                                                        // which is about how long it takes to scan my largest SMB Drive.
    {
        await foreach (var result in GetFilesAsync([path], searchPattern, searchOption, fileOptions, iprogress, token))
        {
            yield return result;
        }
    }



    // Setup and Data Validation
    public async IAsyncEnumerable<string> GetFilesAsync(
        // standard options and syntax
        List<string> paths,
        string searchPattern = "*",
        SearchOption searchOption = SearchOption.AllDirectories,
        EnumerationOptions? fileOptions = null,
        // async options
        IProgress<string>? iprogress = null,
        CancellationToken? token = null)
    {
        foreach (var path in paths)
        {
            if (!Directory.Exists(path))
            {
                string _error = $"ERROR[95] Directory [{path}] Not Found! \nVerify that you are using proper UPPER and LOWER case filenames.  C:\\DirName does NOT equal C:\\dirname.  Verify that you are using UPPERCASE Drive Letters on Windows Systems.\n";
                Console.WriteLine(_error);
                Debug.WriteLine(_error);
                LogError("ERROR[95] Use Case Sensitive Path Names");
                LogError($"ERROR[95] Directory [{path}] Not Found!");
                throw new DirectoryNotFoundException(_error);
            }
        }

        if (fileOptions == null) fileOptions = EnumerationOptionsDefaults();
        CancellationTokenSource _cts = new CancellationTokenSource(GetFilesAsyncCancel);
        CancellationToken _token;
        if (token == null) _token = _cts.Token;
        else _token = (CancellationToken)token;


        // IProgress Report Task here ... 
        // Using a task to reduce the number of times the progress.Report is called.
        // *** WARNING *** If you pass in a CancellationToken, you'll need to cancel it when finished so the IProgress Task will exit as well.
        string _lastReport = "Searching ... ";
        ConcurrentBag<string> _bag = new ConcurrentBag<string>();
        if (iprogress != null)
        {
            Task progressTask = new Task(() =>
            {
                while (_bag != null)
                {
                    if (_token.IsCancellationRequested) break;
                    if (_bag.Count > 0)
                    {
                        string _path;
                        if (_bag.TryTake(out _path))
                        {
                            iprogress.Report(_path);
                            _lastReport = _path;
                        }
                    }
                    else
                        iprogress.Report(_lastReport);
                    _bag.Clear();
                    Task.Delay(1000).Wait();
                }
            }, _token);
            progressTask.Start();
        }
        // /Progress Task

        // Ready, Set, Go!
        await foreach (var result in GetFilesAsync(paths, searchPattern, searchOption, fileOptions, iprogress, _token, _bag))
        {
            yield return result;
        }
        _cts.Cancel();
    }



    // Primary Private Function
    private async IAsyncEnumerable<string> GetFilesAsync(
        // standard options and syntax
        List<string> paths,
        string searchPattern,
        SearchOption searchOption,
        EnumerationOptions? fileOptions,
        // async options
        IProgress<string>? progress,
        CancellationToken token,
        // private options
        ConcurrentBag<string>? bag)
    {

        foreach (var path in paths)
        {
            token.ThrowIfCancellationRequested();
            if (token.IsCancellationRequested) yield break;
            else
            {
                if (progress != null) bag.Add(path);          // 'true': changed my mind to report all.
                await foreach (var s in LambdaGetFiles(path, searchPattern, fileOptions))
                    yield return s;
                if (searchOption == SearchOption.AllDirectories)
                    await foreach (var s in LambdaGetDirectories(path, fileOptions))
                        await foreach (var f in GetFilesAsync([Path.Combine(path, s)], searchPattern, searchOption, fileOptions, progress, token, bag)) { yield return f; }
            }
        }
    }



    private async IAsyncEnumerable<string> LambdaGetFiles(string path, string searchPattern, EnumerationOptions fileOptions)
    {
        Task<string[]> _task = new Task<string[]>(() => { return Directory.GetFiles(path, searchPattern, fileOptions); }, TaskCreationOptions.LongRunning);
        _task.Start();
        string[] _fileList = await _task;
        foreach (var _file in _fileList) { yield return _file; }
    }
    private async IAsyncEnumerable<string> LambdaGetDirectories(string path, EnumerationOptions fileOptions)
    {
        Task<string[]> _task = new Task<string[]>(() =>
        {
            return Directory.GetDirectories(path, "*", fileOptions);
        }, TaskCreationOptions.LongRunning);
        _task.Start();
        string[] _dirList = await _task;
        foreach (var _dir in _dirList) { yield return _dir; }
    }



    // Default Options, change these at your own peril.
    EnumerationOptions EnumerationOptionsDefaults()
    {
        EnumerationOptions _getFilesOptions = new EnumerationOptions();
        _getFilesOptions.MatchCasing = MatchCasing.CaseInsensitive;           // You definitely want the top 3.
        _getFilesOptions.IgnoreInaccessible = true;
        _getFilesOptions.RecurseSubdirectories = false;

        _getFilesOptions.ReturnSpecialDirectories = false;   // probably want to add these two as well
        _getFilesOptions.AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.Device | FileAttributes.SparseFile | FileAttributes.Temporary | FileAttributes.ReparsePoint;
        // ReparsePoint is a tricky one.  Reparse points can cause infinite loops.
        // Given /users/user/Music is a reparse point, you'll have to explicity search it,
        // as you can't search into it.
        return _getFilesOptions;
    }

    // /Class
}


