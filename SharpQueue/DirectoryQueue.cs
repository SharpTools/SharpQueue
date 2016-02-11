using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharpQueue.Util;

namespace SharpQueue {
    public class DirectoryQueue : ISharpQueue {
        private readonly string _directoryPath;
        private const string CreatingExtension = ".init";
        private const string OnQueueExtension = ".item";
        private const string WorkingExtension = ".working";
        private const string ErrorExtension = ".error";
        private Random _random = new Random();
        private Mutex _mutex;

        public DirectoryQueue(string directoryPath) {
            _directoryPath = directoryPath;
            Directory.CreateDirectory(directoryPath);
            _mutex = new Mutex(false, @"Local\" + _directoryPath.Replace(Path.DirectorySeparatorChar, '_'));
        }

        public void Enqueue<T>(T item) where T : class {
            var path = Path.Combine(_directoryPath, Guid.NewGuid().ToString()) + CreatingExtension;
            var text = JsonConvert.SerializeObject(item);
            try {
                Try.ThreeTimes(() => File.WriteAllText(path, text, Encoding.UTF8), 100);
                Try.ThreeTimes(() => File.Move(path, Path.ChangeExtension(path, OnQueueExtension)), 100);
            }
            catch (Exception ex){
                var x = ex.Message;
            }
        }

        public T Dequeue<T>() where T : class {
            string item;
            try {
                _mutex.WaitOne();
                item = LockItem();
            }
            finally {
                _mutex.ReleaseMutex();
            }
            item = item ?? "";
            if (!File.Exists(item)) {
                return null;
            }
            try {
                var text = ReadTheFile(item);
                return JsonConvert.DeserializeObject<T>(text);
            }
            catch (Exception) {
                File.Move(item, Path.ChangeExtension(item, ErrorExtension));
                return null;
            }
            finally {
                File.Delete(item);
            }
        }

        private string ReadTheFile(string fullpath) {
            while (true) {
                try {
                    return File.ReadAllText(fullpath, Encoding.UTF8);
                }
                catch (Exception) {
                    Try.Sleep(100);
                }
            }
        }

        public QueueInfo GetQueueInfo() {
            var info = new DirectoryInfo(_directoryPath);
            return new QueueInfo(
                info.GetFiles("*" + OnQueueExtension).Length,
                info.GetFiles("*" + ErrorExtension).Length);
        }

        public void CleanQueue() {
            try {
                _mutex.WaitOne();
                DeleteFiles("*" + OnQueueExtension);
                DeleteFiles("*" + ErrorExtension);
            }
            finally {
                _mutex.ReleaseMutex();
            }
        }

        private void DeleteFiles(string match) {
            var info = new DirectoryInfo(_directoryPath);
            foreach (var file in info.EnumerateFiles(match)) {
                file.Delete();
            }
        }

        private string LockItem() {
            while (true) {
                try {
                    var filename = TryRenameNextOnQueue();
                    return filename;
                }
                catch (Exception) {
                    Try.Sleep(100);
                }
            }
        }

        private string TryRenameNextOnQueue() {
            var info = new DirectoryInfo(_directoryPath);
            var fileInfo = info.GetFiles("*" + OnQueueExtension).OrderBy(p => p.CreationTime).FirstOrDefault();
            if (fileInfo == null) {
                return null;
            }
            var newFilename = Path.Combine(_directoryPath, Path.ChangeExtension(fileInfo.Name, WorkingExtension));
            fileInfo.MoveTo(newFilename);
            return newFilename;
        }

        public void Dispose() {
            _mutex.Dispose();
        }
    }
}