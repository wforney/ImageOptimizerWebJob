using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImageOptimizerWebJob;

/// <summary>
/// Represents the configuration for the image optimization job.
/// </summary>
public class Config
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Config"/> class.
    /// </summary>
    public Config()
    {
        Optimizations = [new()];
        CacheFilePath = Defaults.CacheFilePath;
        FilePath = Path.Combine(Defaults.FolderToWatch ?? "", Defaults.ConfigFileName ?? "");
    }

    /// <summary>
    /// Gets or sets the cache file path.
    /// </summary>
    /// <value>The cache file path.</value>
    [JsonIgnore]
    public string? CacheFilePath { get; set; }

    /// <summary>
    /// Gets the file path.
    /// </summary>
    /// <value>The file path.</value>
    [JsonIgnore]
    public string? FilePath { get; private set; }

    /// <summary>
    /// Gets or sets the log file path.
    /// </summary>
    /// <value>The log file path.</value>
    [JsonIgnore]
    public string? LogFilePath { get; set; }

    /// <summary>
    /// Gets or sets the optimizations.
    /// </summary>
    /// <value>The optimizations.</value>
    public List<Optimization> Optimizations { get; set; }

    /// <summary>
    /// Loads the configuration from the specified folder and cache file path.
    /// </summary>
    /// <param name="folder">The folder.</param>
    /// <param name="cacheFilePath">The cache file path.</param>
    /// <returns>The configuration.</returns>
    public static Config FromPath(string folder, string cacheFilePath)
    {
        DirectoryInfo dir = new(folder);

        Config config = new()
        {
            CacheFilePath = cacheFilePath,
            FilePath = Path.Combine(dir.FullName, Defaults.ConfigFileName ?? "")
        };

        config.Update();

        return config;
    }

    /// <summary>
    /// Updates this instance.
    /// </summary>
    public void Update()
    {
        if (File.Exists(FilePath))
        {
            Console.WriteLine($"Read config from {FilePath}");

            using StreamReader reader = new(FilePath);
            Config? options = JsonSerializer.Deserialize<Config>(reader.ReadToEnd());
            Optimizations = options?.Optimizations ?? [];
        }
        else
        {
            Console.WriteLine($"No config file present. Using default configuration");

            Config options = new();
            Optimizations = options.Optimizations;
        }

        NormalizePaths();
    }

    private string CleanGlobbingPattern(string pattern)
    {
        string dir = Path.GetDirectoryName(FilePath)!;
        string path = Path.Combine(dir, pattern).Replace('/', '\\').Replace(".\\", "");

        if (path.EndsWith('\\'))
        {
            path += "**\\*";
        }

        return path;
    }

    private void NormalizePaths()
    {
        foreach (Optimization opti in Optimizations)
        {
            opti.Includes = opti.Includes.Select(CleanGlobbingPattern);
            opti.Excludes = opti.Excludes.Select(CleanGlobbingPattern);
        }

        CacheFilePath = new FileInfo(CacheFilePath ?? "").FullName;
        LogFilePath = Path.ChangeExtension(CacheFilePath, ".log");
    }
}
