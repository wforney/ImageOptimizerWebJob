using System.Text;

namespace ImageOptimizerWebJob
{
    /// <summary>
    /// Represents the result of a compression operation.
    /// </summary>
    public class CompressionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompressionResult"/> class.
        /// </summary>
        /// <param name="originalFileName">Name of the original file.</param>
        /// <param name="resultFileName">Name of the result file.</param>
        /// <param name="elapsed">The elapsed.</param>
        public CompressionResult(string originalFileName, string resultFileName, TimeSpan elapsed)
        {
            Elapsed = elapsed;
            FileInfo original = new(originalFileName);
            FileInfo result = new(resultFileName);

            if (original.Exists)
            {
                OriginalFileName = original.FullName;
                OriginalFileSize = original.Length;
            }

            if (result.Exists)
            {
                ResultFileName = result.FullName;
                ResultFileSize = result.Length;
            }

            Processed = true;
        }

        /// <summary>
        /// Gets or sets the elapsed time.
        /// </summary>
        /// <value>The elapsed time.</value>
        public TimeSpan Elapsed { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Gets or sets the name of the original file.
        /// </summary>
        /// <value>The name of the original file.</value>
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size of the original file.
        /// </summary>
        /// <value>The size of the original file.</value>
        public long OriginalFileSize { get; set; } = 0;

        /// <summary>
        /// Gets the percent of file size reduction.
        /// </summary>
        /// <value>The percent of file size reduction.</value>
        public double Percent => Math.Round(100 - (ResultFileSize / (double)OriginalFileSize * 100), 1);

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CompressionResult"/> is processed.
        /// </summary>
        /// <value><c>true</c> if processed; otherwise, <c>false</c>.</value>
        public bool Processed { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the result file.
        /// </summary>
        /// <value>The name of the result file.</value>
        public string ResultFileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size of the result file.
        /// </summary>
        /// <value>The size of the result file.</value>
        public long ResultFileSize { get; set; } = 0;

        /// <summary>
        /// Gets the number of bytes saved.
        /// </summary>
        /// <value>The number of bytes saved.</value>
        public long Saving => Math.Max(OriginalFileSize - ResultFileSize, 0);

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = new();

            _ = sb.Append("Optimized ")
                .Append(Path.GetFileName(OriginalFileName))
                .Append(" in ")
                .Append(Math.Round(Elapsed.TotalMilliseconds / 1000, 2))
                .AppendLine(" seconds");
            _ = sb.Append("Before: ")
                .Append(OriginalFileSize)
                .AppendLine(" bytes");
            _ = sb.Append("After: ")
                .Append(ResultFileSize)
                .AppendLine(" bytes");
            _ = sb.Append("Saving: ")
                .Append(Saving)
                .Append(" bytes / ")
                .Append(Percent)
                .AppendLine("%");

            return sb.ToString();
        }
    }
}