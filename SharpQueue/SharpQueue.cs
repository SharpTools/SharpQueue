using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sharp.Queue {
    public class SharpQueue : ISharpQueue {
        private readonly string _directoryPath;
        private const string OnQueueExtension = ".item";
        private const string WorkingExtension = ".wkg";
        private const string ErrorExtension = ".err";
        private Random _random = new Random();
        private Mutex _mutex;

        public SharpQueue(string directoryPath) {
            _directoryPath = directoryPath;
            Directory.CreateDirectory(directoryPath);
            _mutex = new Mutex(false, @"Local\" + _directoryPath.Replace(Path.DirectorySeparatorChar, '_'));
        }

        public void Enqueue<T>(T item) where T : class {
            var path = Path.Combine(_directoryPath, Guid.NewGuid().ToString());
            var text = JsonConvert.SerializeObject(item);
            File.WriteAllText(path, text, Encoding.UTF8);
            File.Move(path, path + OnQueueExtension);
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
                var text = File.ReadAllText(item, Encoding.UTF8);
                return JsonConvert.DeserializeObject<T>(text);
            }
            catch (Exception) {
                File.Move(item, item.Replace(WorkingExtension, ErrorExtension));
                return null;
            }
            finally {
                File.Delete(item);
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
                    if (filename != null) {
                        return filename;
                    }
                }
                catch (IOException) {
                    Thread.Sleep(_random.Next(0,100));
                }
            }
        }

        private string TryRenameNextOnQueue() {
            var info = new DirectoryInfo(_directoryPath);
            var fileInfo = info.GetFiles("*" + OnQueueExtension).OrderBy(p => p.CreationTime).FirstOrDefault();
            if (fileInfo == null) {
                return null;
            }
            var newFilename = fileInfo.Name.Replace(OnQueueExtension, WorkingExtension);
            fileInfo.MoveTo(newFilename);
            return newFilename;
        }

        public void Dispose() {
            _mutex.Dispose();
        }
    }
}