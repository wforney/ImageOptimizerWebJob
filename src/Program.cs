using ImageOptimizerWebJob;

string? basePath = Defaults.FolderToWatch;
string? logFilePath = Defaults.CacheFilePath;

if (!(basePath?.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ?? false))
{
    throw new Exception("The folder MUST end with a backslash");
}

if (!Directory.Exists(basePath))
{
    basePath = "./";
}

if (!Directory.Exists(Path.GetDirectoryName(logFilePath)))
{
    logFilePath = "log.cache";
}

Config options = Config.FromPath(basePath, logFilePath!);
ImageQueue queue = new(options);

Console.WriteLine("Image Optimizer started");
Console.WriteLine($"Watching {new DirectoryInfo(basePath).FullName}");

await queue.ProcessQueueAsync();
