namespace ImageOptimizerWebJob;

/// <summary>
/// Represents the optimization settings for image processing.
/// </summary>
public class Optimization
{
    /// <summary>
    /// Gets or sets the excludes.
    /// </summary>
    /// <value>The excludes.</value>
    public IEnumerable<string> Excludes { get; set; } = Defaults.Excludes;

    /// <summary>
    /// Gets or sets the includes.
    /// </summary>
    /// <value>The includes.</value>
    public IEnumerable<string> Includes { get; set; } = Defaults.Includes;

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Optimization"/> is lossy.
    /// </summary>
    /// <value><c>true</c> if lossy; otherwise, <c>false</c>.</value>
    public bool Lossy { get; set; }
}