using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using Serilog;
 

namespace AngelHornetLibrary
{



    public class AhGetFiles
    {
        private readonly TimeSpan GetFilesAsyncCancel = TimeSpan.FromSeconds(3600);

        //private readonly Serilog.ILogger _logger;
        //public AhGetFiles()
        //{   
        //}
        //public AhGetFiles(Serilog.ILogger logger)
        //{
        //    _logger = logger;
        //    if (_logger != null) _logger.Information("Hello, Dependency Injected Serilog!");
        //    else Debug.WriteLine("Dependency Injected Failed!");
        //    Environment.Exit(1);

        //}

        // convert string path -> List<string> paths
        public async IAsyncEnumerable<string> GetFilesAsync(
            // standard options and syntax
            string path,                                                    // string path or List<string> paths
            string searchPattern = "*",                                     // File Search Pattern
            SearchOption searchOption = SearchOption.AllDirectories,        // Search Option
            EnumerationOptions? fileOptions = null,                         // Enumeration Options, change these at your own peril.
            // async options
            IProgress<string>? progress = null,                             // Reports directory search progress every 250ms
            CancellationToken? token = null)                                // If you use this, make sure you cancel it when finished so the IProgress Task will exit as well.
                                                                            // Cancellation Defaults to GetFilesAsyncCancel, which is about how long it takes to scan my largest SMB Drive.
        {
            await foreach (var result in GetFilesAsync([path], searchPattern, searchOption, fileOptions, progress, token))
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
            IProgress<string>? progress = null,
            CancellationToken? token = null)
        {
            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                {
                    string _error = $"\n*** ERROR ***   Directory [{path}] Not Found! \nVerify that you are using proper UPPER and LOWER case filenames.  C:\\DirName does NOT equal C:\\dirname.  Verify that you are using UPPERCASE Drive Letters on Windows Systems.\n";
                    Console.WriteLine(_error);
                    Debug.WriteLine(_error);
                    //var log = new ILogger<Program>();  // cjm - work on Dependency Injection Logging later.
                    throw new DirectoryNotFoundException(_error);
                }
            }

            if (fileOptions == null) fileOptions = EnumerationOptionsDefaults();
            CancellationTokenSource _cts = new CancellationTokenSource(GetFilesAsyncCancel);
            CancellationToken _token;
            if (token == null) _token = _cts.Token;
            else _token = (CancellationToken)token;


            // IProgress Report Task here ... console.writeline for testing for now
            // Using a task to reduce the number of times the progress.Report is called.
            ConcurrentBag<string> _bag = new ConcurrentBag<string>();
            if (progress != null)
            {
                Task progressTask = new Task(() =>
                {
                    while (_bag != null)
                    {
                        if (_token.IsCancellationRequested) break;
                        if (_bag.Count > 0)
                        {
                            string _path;
                            if (_bag.TryTake(out _path)) { progress.Report(_path); }
                        }
                        _bag.Clear();
                        Task.Delay(250).Wait();
                    }
                }, _token);
                progressTask.Start();
            }
            // /Progress Task

            // Ready, Set, Go!
            await foreach (var result in GetFilesAsync(paths, searchPattern, searchOption, fileOptions, progress, _token, _bag))
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
                    if ((progress != null && bag.Count <= 0) || true) bag.Add(path);
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
            // cjm - ReparsePoint is a tricky one.  Reparse points can cause infinite loops.
            // Given /users/user/Music is a reparse point, you'll have to explicity search it,
            // as you can't search into it.
            return _getFilesOptions;
        }

        // /Class
    }





    // *** OBSOLETE CODE BELOW ***






    // *** OBSOLETE CODE BELOW ***





    // *** OBSOLETE CODE BELOW ***





    // *** OBSOLETE CODE BELOW ***





    // *** OBSOLETE CODE BELOW ***






    // ===================================================================
    // Create a callback function to pass the list back to the main thread,
    // and notify it that the ConCurrentQueue is ready.
    //
    //
    // Example:
    //
    // Action<ConcurrentQueue<string>, bool> _callback = (ConcurrentQueue<string> q, bool d) => { string r; while (q.TryDequeue(out r)) { Console.WriteLine(r); } };
    // new FindFilesConcurrentQueue(_callback, paths, "*.mp3", SearchOption.AllDirectories);



    // Example:
    //
    // public static void FindFilesQueueFunc(List<string> paths)
    // {
    //    Action<ConcurrentQueue<string>, bool> _callback = callback;
    //    if (FindFilesConcurrentQueue.IsRunningOrWaiting()) { Console.WriteLine("A FindFiles Task is already running."); return; }
    //    new FindFilesConcurrentQueue(_callback, paths, "*.mp3", SearchOption.AllDirectories);
    //    return;
    // }
    //
    // public static void callback(ConcurrentQueue<string> queue, bool done = false)
    // {
    //    string result = "result";
    //    while (queue.TryDequeue(out result)) { Console.WriteLine(result);}
    //    if (done) Console.WriteLine("FindFiles Completed.");
    //    return;
    // }


    // *** OBSOLETE CODE BELOW ***
    public class FindFilesConcurrentQueue
    {
        public Task _task;
        static public bool IsRunningOrWaiting()
        {
            foreach (Task _t in Tasks)
            {
                if (false
                    || _t.Status == TaskStatus.Created
                    || _t.Status == TaskStatus.WaitingForActivation
                    || _t.Status == TaskStatus.WaitingToRun
                    || _t.Status == TaskStatus.Running
                    || _t.Status == TaskStatus.WaitingForChildrenToComplete
                    ) return true;
            }
            return false;
        }
        static public bool IsCompletedOrDefault()
        {

            return !IsRunningOrWaiting();
        }
        
        // *** OBSOLETE CODE BELOW ***
        public FindFilesConcurrentQueue(
             Action<ConcurrentQueue<string>, bool> callBack,
             List<string> paths,
             string searchPattern = "*",
             SearchOption searchOption = SearchOption.TopDirectoryOnly,
             CancellationToken? token = null,
             EnumerationOptions? fileOptions = null,
             TaskCreationOptions taskOptions = TaskCreationOptions.LongRunning
             )
        {
            // ~45m is enough to scan my largest SMB Drive, the drive with 17,000 MP3s only takes about 5m
            // CancellationToken will not convert easily.
            CancellationToken _token;
            if (token == null) _token = new CancellationTokenSource(TimeSpan.FromSeconds(3600)).Token;
            else _token = (CancellationToken)token;

            if (fileOptions == null) fileOptions = EnumerationOptionsDefaults();

            Task _task = new Task(() =>
            {
                FindFilesQueueFunc(callBack, paths, searchPattern, searchOption, _token, fileOptions, true);
            }, taskOptions);
            _task.Start();
            Tasks.Add(_task);
            while (_task != null && !_task.IsCanceled && !_task.IsCompleted && !_task.IsFaulted && _task.Status != TaskStatus.Running) { _task.Wait(25); }
            return;
        }

        // *** OBSOLETE CODE BELOW ***
        static private readonly object _tasksLock = new object();
        static private List<Task> _tasks = new List<Task>();
        static private List<Task> Tasks
        {
            get { lock (_tasksLock) { return _tasks; } }
            set { lock (_tasksLock) { _tasks = value; } }
        }

        // *** OBSOLETE CODE BELOW ***
        void FindFilesQueueFunc(
            Action<ConcurrentQueue<string>, bool> callBack,
            List<string> paths,
            string searchPattern,
            SearchOption searchOption,
            CancellationToken token,
            EnumerationOptions fileOptions,
            bool Parent = false
            )
        {
            ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();

            foreach (string path in paths)
            {
                token.ThrowIfCancellationRequested();
                if (token.IsCancellationRequested) return;
                if (!Directory.Exists(path))
                {
                    // cjm - may not want to do this. If an exception is thrown the "done" flag will never be sent.
                    //throw new DirectoryNotFoundException(path); 
                    continue;
                }

                string[] _fileList = Directory.GetFiles(path, searchPattern, fileOptions);
                foreach (var _file in _fileList) { if (_file != null) _queue.Enqueue(_file); }
                if (_fileList.Length > 0) callBack(_queue, false); // Call-Back, the callback function to pass the list back to the main thread, and notify it that the ConCurrentQueue is ready.

                if (searchOption == SearchOption.AllDirectories)
                {
                    string[] dirList = Directory.GetDirectories(path, "*", fileOptions);
                    foreach (string dir in dirList)
                    {
                        // FindFilesFunction(paths, searchPattern, searchOption, _listAction, _token, getFilesOptions);
                        FindFilesQueueFunc(callBack, new List<string> { Path.Combine(path, dir) }, searchPattern, searchOption, token, fileOptions);
                    }
                }

            }
            // cjm - this is a signal to the callback function that the queue is complete.
            // but I'm not sure if I want to do this, as it could cause confusion with consumer callback functions.
            // and that could inject garbage into any number of things.
            if (Parent) callBack(_queue, true);
            return;
        }


        // *** OBSOLETE CODE BELOW ***
        EnumerationOptions EnumerationOptionsDefaults()
        {
            EnumerationOptions _getFilesOptions = new EnumerationOptions();
            _getFilesOptions.MatchCasing = MatchCasing.CaseInsensitive;           // You definitely want the top 3.
            _getFilesOptions.IgnoreInaccessible = true;
            _getFilesOptions.RecurseSubdirectories = false;

            _getFilesOptions.ReturnSpecialDirectories = false;   // probably want to add these two as well
            _getFilesOptions.AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.Device | FileAttributes.SparseFile | FileAttributes.Temporary | FileAttributes.ReparsePoint;
            // cjm - ReparsePoint is a tricky one.  Reparse points can cause infinite loops.
            // Given /users/user/Music is a reparse point, you'll have to explicity search it,
            // as you can't search into it.
            return _getFilesOptions;
        }



    }



    // ================================================================
    // ================================================================
    // ================================================================


}


