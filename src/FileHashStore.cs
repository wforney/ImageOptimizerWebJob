using System.Text;

namespace ImageOptimizerWebJob;

/// <summary>
/// Represents a simple file hash store that keeps track of files and their hashes.
/// </summary>
public class FileHashStore
{
    private static readonly Lock _syncRoot = new();
    private readonly string _filePath;
    private readonly Dictionary<string, string> _store = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FileHashStore"/> class.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public FileHashStore(string fileName)
    {
        _filePath = fileName;

        string dir = Path.GetDirectoryName(_filePath)!;

        if (!Directory.Exists(dir))
        {
            _ = Directory.CreateDirectory(dir);
        }

        Load();
    }

    /// <summary>
    /// Determines whether the specified <paramref name="file"/> has changed or is new.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>
    /// <c>true</c> if the specified <paramref name="file"/> has changed or is new; otherwise, <c>false</c>.
    /// </returns>
    public bool HasChangedOrIsNew(string file)
    {
        if (!_store.TryGetValue(file, out string? value))
        {
            return true;
        }

        string? currentHash = GetHash(file);

        return string.IsNullOrEmpty(currentHash) || currentHash != value;
    }

    /// <summary>
    /// Saves the specified file.
    /// </summary>
    /// <param name="file">The file.</param>
    public void Save(string file)
    {
        bool exist = _store.ContainsKey(file);

        try
        {
            lock (_syncRoot)
            {
                _store[file] = GetHash(file) ?? "";

                if (!exist)
                {
                    // If the file is new to the azure job, just append it to the existing file
                    File.AppendAllLines(_filePath, [$"{file}|{_store[file]}"]);
                }
                else
                {
                    // If the file is known we must avoid duplicates, so this just writes the entire store
                    StringBuilder sb = new();

                    foreach (string key in _store.Keys)
                    {
                        _ = sb.AppendLine($"{key}|{_store[key]}");
                    }

                    File.WriteAllText(_filePath, sb.ToString());
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static string? GetHash(string file)
    {
        try
        {
            return File.Exists(file) ? new FileInfo(file).Length.ToString() : null;

            //using (var md5 = MD5.Create())
            //using (var stream = File.OpenRead(file))
            //{
            //    byte[] hash = md5.ComputeHash(stream);
            //    return BitConverter.ToString(hash);
            //}
        }
        catch
        {
            return null;
        }
    }

    private void Load()
    {
        try
        {
            // If the file hasn't been created yet, just ignore it.
            if (!File.Exists(_filePath))
            {
                return;
            }

            foreach (string line in File.ReadAllLines(_filePath))
            {
                string[] args = line.Split('|');

                if (args.Length == 2 && !_store.ContainsKey(args[0]))
                {
                    _store.Add(args[0], args[1]);
                }
            }
        }
        catch
        {
            // Do nothing. The file format has changed and will be overwritten next time Save() is called.
        }
    }
}