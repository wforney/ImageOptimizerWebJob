using Minimatch;
using System.Diagnostics.CodeAnalysis;

namespace ImageOptimizerWebJob
{
    /// <summary>
    /// Represents a queue of images to be processed. Implements the <see cref="Dictionary{String, DateTime}"/>
    /// </summary>
    /// <seealso cref="Dictionary{String, DateTime}"/>
    public class ImageQueue : Dictionary<string, DateTime>
    {
        private static readonly Lock _logRoot = new();

        private readonly FileHashStore _cache;
        private readonly Config _config;
        private readonly Options _matcherOptions = new() { AllowWindowsPaths = true, IgnoreCase = true };

        private FileSystemWatcher? _watcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageQueue"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public ImageQueue(Config config)
        {
            _cache = new FileHashStore(config.CacheFilePath ?? "");
            _config = config;

            string dir = Path.GetDirectoryName(config.FilePath)!;
            StartListening(dir);
        }

        /// <summary>
        /// Process queue as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task ProcessQueueAsync()
        {
            QueueExistingFiles();

            Compressor c = new();

            while (true)
            {
                string[] files = [.. this.Where(e => e.Value < DateTime.Now.AddSeconds(-2)).Select(e => e.Key)];

                foreach (string file in files)
                {
                    if (IsImageOnProbingPath(file, out Optimization? opti) && _cache.HasChangedOrIsNew(file))
                    {
                        CompressionResult result = c.CompressFile(file, opti.Lossy);

                        HandleCompressionResult(result, opti);

                        _cache.Save(file);
                    }

                    _ = Remove(file);
                }

                await Task.Delay(5000);
            }
        }

        private async void FileChanged(object sender, FileSystemEventArgs e)
        {
            string file = e.FullPath;
            string ext = Path.GetExtension(file);

            if (string.IsNullOrWhiteSpace(ext) || ext.Contains('~'))
            {
                return;
            }

            if (Defaults.Extensions.Contains(ext, StringComparer.OrdinalIgnoreCase) && !ContainsKey(file) && _cache.HasChangedOrIsNew(file))
            {
                this[file] = DateTime.Now;
            }
            else if (e.FullPath == _config.FilePath)
            {
                await Task.Delay(2000).ConfigureAwait(false);
                _config.Update();
                QueueExistingFiles();
            }
        }

        private void HandleCompressionResult(CompressionResult result, Optimization opti)
        {
            if (result.Saving > 0)
            {
                DateTime creationTime = File.GetCreationTime(result.OriginalFileName);
                File.Copy(result.ResultFileName, result.OriginalFileName, true);
                File.SetCreationTime(result.OriginalFileName, creationTime);
                File.Delete(result.ResultFileName);
            }

            string dir = Path.GetDirectoryName(_config.FilePath)!;
            string fileName = result.OriginalFileName.Replace(dir, string.Empty);
            double percent = Math.Max(result.Percent, 0);

            lock (_logRoot)
            {
                using StreamWriter writer = new(_config.LogFilePath!, true);
                writer.WriteLine($"{DateTime.UtcNow:s};{fileName};{percent}%;{(opti.Lossy ? "lossy" : "lossless")}");
            }
        }

        private bool IsImageOnProbingPath(string file, [NotNullWhen(true)] out Optimization? optimization)
        {
            optimization = null;

            foreach (Optimization opti in _config.Optimizations)
            {
                optimization = opti;

                bool isIncluded = opti.Includes.Any(pattern => Minimatcher.Check(file, pattern, _matcherOptions));
                if (!isIncluded)
                {
                    continue;
                }

                bool isExcluded = opti.Excludes.Any(pattern => Minimatcher.Check(file, pattern, _matcherOptions));
                if (isExcluded)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private void QueueExistingFiles()
        {
            string dir = Path.GetDirectoryName(_config.FilePath)!;

            foreach (string ext in Defaults.Extensions)
            {
                IEnumerable<string> images = Directory.EnumerateFiles(dir, $"*{ext}", SearchOption.AllDirectories);

                foreach (string image in images)
                {
                    this[image] = DateTime.Now;
                }
            }
        }

        private void StartListening(string folder)
        {
            _watcher = new FileSystemWatcher(folder);
            _watcher.Changed += FileChanged;
            _watcher.IncludeSubdirectories = true;
            _watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.CreationTime;
            _watcher.EnableRaisingEvents = true;
        }
    }
}