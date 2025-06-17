using System.Configuration;

namespace ImageOptimizerWebJob;

/// <summary>
/// Represents the default settings for the image optimizer web job.
/// </summary>
public static class Defaults
{
    /// <summary>
    /// The cache file path
    /// </summary>
    public static readonly string? CacheFilePath = ConfigurationManager.AppSettings.Get("logfile");

    /// <summary>
    /// The configuration file name
    /// </summary>
    public static readonly string? ConfigFileName = ConfigurationManager.AppSettings.Get("configFileName");

    /// <summary>
    /// The folders to exclude from processing
    /// </summary>
    public static readonly List<string> Excludes = ["node_modules", "bower_components", "jspm_packages"];

    /// <summary>
    /// The file extensions to process
    /// </summary>
    public static readonly string[] Extensions = [".jpg", ".jpeg", ".gif", ".png"];

    /// <summary>
    /// The folder to watch
    /// </summary>
    public static readonly string? FolderToWatch = ConfigurationManager.AppSettings.Get("folderToWatch");

    /// <summary>
    /// The folders to include in processing
    /// </summary>
    public static readonly List<string> Includes = [FolderToWatch];
}