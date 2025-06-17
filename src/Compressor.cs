using System.Diagnostics;
using System.Globalization;

namespace ImageOptimizerWebJob;

/// <summary>
/// Represents a compressor that uses external tools to compress image files.
/// </summary>
public class Compressor
{
    private readonly string _cwd;

    /// <summary>
    /// Initializes a new instance of the <see cref="Compressor"/> class.
    /// </summary>
    public Compressor() => _cwd = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!, @"Tools\");

    /// <summary>
    /// Compresses the file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="lossy">The lossy.</param>
    /// <returns>ImageOptimizerWebJob.CompressionResult.</returns>
    public CompressionResult CompressFile(string fileName, bool lossy)
    {
        string targetFile = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(fileName));

        ProcessStartInfo start = new("cmd")
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = _cwd,
            Arguments = GetArguments(fileName, targetFile, lossy),
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        Stopwatch stopwatch = Stopwatch.StartNew();

        using Process? process = Process.Start(start);
        process?.WaitForExit();

        stopwatch.Stop();

        return new CompressionResult(fileName, targetFile, stopwatch.Elapsed);
    }

    private static string? GetArguments(string sourceFile, string targetFile, bool lossy)
    {
        if (!Uri.IsWellFormedUriString(sourceFile, UriKind.RelativeOrAbsolute) && !File.Exists(sourceFile))
        {
            return null;
        }

        string ext;
        try
        {
            ext = Path.GetExtension(sourceFile).ToLowerInvariant();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex);
            return null;
        }

        switch (ext)
        {
            case ".png":
                File.Copy(sourceFile, targetFile);

                return lossy
                    ? string.Format(CultureInfo.CurrentCulture, "/c pingo -s8 -q -palette=79 \"{0}\"", targetFile)
                    : string.Format(CultureInfo.CurrentCulture, "/c pingo -s8 -q \"{0}\"", targetFile);

            case ".jpg":
            case ".jpeg":
                if (lossy)
                {
                    return string.Format(CultureInfo.CurrentCulture, "/c cjpeg -quality 80,60 -dct float -smooth 5 -outfile \"{1}\" \"{0}\"", sourceFile, targetFile);
                }

                return string.Format(CultureInfo.CurrentCulture, "/c jpegtran -copy none -optimize -progressive -outfile \"{1}\" \"{0}\"", sourceFile, targetFile);

            case ".gif":
                return string.Format(CultureInfo.CurrentCulture, "/c gifsicle -O3 --batch --colors=256 \"{0}\" --output=\"{1}\"", sourceFile, targetFile);
        }

        return null;
    }
}