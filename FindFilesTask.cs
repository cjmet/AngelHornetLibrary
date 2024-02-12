using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.IO;

namespace AngelHornetLibrary
{
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


























    public class FindFilesConcurrentQueue
    {
        public Task _task;
        static public bool IsRunningOrWaiting() {
            foreach (Task _t in Tasks) { if ( false
                    || _t.Status == TaskStatus.Created
                    || _t.Status == TaskStatus.WaitingForActivation
                    || _t.Status == TaskStatus.WaitingToRun
                    || _t.Status == TaskStatus.Running 
                    || _t.Status == TaskStatus.WaitingForChildrenToComplete
                    ) return true; }
            return false;
        }
        static public bool IsCompletedOrDefault() { 
            
            return !IsRunningOrWaiting();
        }

        public FindFilesConcurrentQueue(
             Action<ConcurrentQueue<string>,bool> callBack,
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

            Debug.Write($"\n*** Creating Task ... ");
            Task _task = new Task(() =>
            {
                FindFilesQueueFunc(callBack, paths, searchPattern, searchOption, _token, fileOptions, true);
            }, taskOptions);
            _task.Start();
            Tasks.Add(_task);
            while (_task != null && !_task.IsCanceled && !_task.IsCompleted && !_task.IsFaulted && _task.Status != TaskStatus.Running) { _task.Wait(25); }
            Debug.WriteLine($"Task Started: {_task.Status}. ***");
            return;
        }

        static private readonly object _tasksLock = new object();
        static private List<Task> _tasks = new List<Task>();
        static private List<Task> Tasks
        {
            get { lock (_tasksLock) { return _tasks; } }
            set { lock (_tasksLock) { _tasks = value; } }
        }


        void FindFilesQueueFunc(
            Action<ConcurrentQueue<string>,bool> callBack,
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
                if (_fileList.Length > 0) callBack(_queue,false); // Call-Back, the callback function to pass the list back to the main thread, and notify it that the ConCurrentQueue is ready.

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



    public class FindFilesTask
    {
        private int spinner = 0;
        // lockable 
        private readonly object _resultsLock = new object();
        List<string> _results = new List<string>();
        public List<string> ResultsList
        {
            get
            {
                lock (_resultsLock)
                {
                    return _results;
                }
            }

            private set
            {
                lock (_resultsLock)
                {
                    _results = value;
                }
            }
        }

        // lockable 
        private readonly object _statusLock = new object();
        public string _status = "";
        public string StatusString
        {
            get
            {
                lock (_statusLock)
                {
                    return _status;
                }
            }

            private set
            {
                lock (_statusLock)
                {
                    _status = value;
                }
            }
        }

        public Task<List<string>> task = null;

        public TaskStatus Status { get => task != null ? task.Status : TaskStatus.WaitingForActivation; }


        public FindFilesTask(List<string> paths, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly, Action<List<string>>? listAction = null, CancellationToken? token = null, TaskCreationOptions? taskOptions = null)
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


            Action<List<string>> _listAction; // cjm
            _listAction = listAction;
            //if (listAction == null) _listAction = Action<List<string>> (List<string> list) => { return list; };
            //else _listAction = listAction;


            CancellationToken _token;
            //if (token == null) _token = new CancellationToken(true);
            if (token == null) _token = new CancellationTokenSource(TimeSpan.FromSeconds(3600)).Token;  // 45m is enough to scan my 2TB SMB Drive
            else _token = (CancellationToken)token;


            TaskCreationOptions _taskOptions;
            if (taskOptions == null) _taskOptions = TaskCreationOptions.LongRunning;
            else _taskOptions = (TaskCreationOptions)taskOptions;


            task = new Task<List<string>>(() =>
            {
                // *** TASK FUNCTION ***
                StatusString = $"Task Started {paths}";
                Debug.WriteLine(StatusString);
                return FindFilesFunction(paths, searchPattern, searchOption, _listAction, _token, _getFilesOptions);

            },  // Task<TResult> function
                _token,                                                         // CancellationToken token
                _taskOptions                                                    // TaskCreationOptions taskOptions
                );
            task.Start();
            StatusString = $"Task Started {paths}";
            Debug.WriteLine(StatusString);
            if (task.Status == TaskStatus.Canceled)
            {
                StatusString = "Task Canceled";
                Debug.WriteLine(StatusString);
                return;
            }
            if (task.Status == TaskStatus.Faulted)
            {
                StatusString = "Task Faulted";
                Debug.WriteLine(StatusString);
                return;
            }
            while (task.Status == TaskStatus.Created) { task.Wait(25); }
            while (task.Status == TaskStatus.WaitingForActivation) { task.Wait(25); }
            while (task.Status == TaskStatus.WaitingToRun) { task.Wait(25); }
            Debug.WriteLine($"Task Started: {task.Status}");

            return;
        }




        // *****************************************************************************************************


        // FindFilesFunction(paths, searchPattern, searchOption, _listAction, _token, getFilesOptions);
        List<String> FindFilesFunction(
            List<string> paths,
            string searchPattern,
            SearchOption searchOption,
            Action<List<string>> listAction,
            CancellationToken token,
            EnumerationOptions getFilesOptions)
        {

            foreach (string path in paths)
            {
                StatusString = path;
                token.ThrowIfCancellationRequested();
                if (token.IsCancellationRequested)
                {
                    return ResultsList;
                }
                if (!Directory.Exists(path))
                {
                    continue;
                }

                string[] fileList = Directory.GetFiles(path, searchPattern, getFilesOptions);
                AddFilesToList(fileList);

                if (searchOption == SearchOption.AllDirectories)
                {
                    string[] dirList = Directory.GetDirectories(path, "*", getFilesOptions);
                    foreach (string dir in dirList)
                    {
                        // FindFilesFunction(paths, searchPattern, searchOption, _listAction, _token, getFilesOptions);
                        FindFilesFunction(new List<string> { Path.Combine(path, dir) }, searchPattern, searchOption, listAction, token, getFilesOptions);
                    }
                }

            }
            return ResultsList;
        }



        void AddFilesToList(string[] fileList)
        {
            lock (_resultsLock)
            {
                _results.AddRange(fileList);
            }
        }



    }
}


