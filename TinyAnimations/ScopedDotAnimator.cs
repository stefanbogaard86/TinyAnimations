namespace TinyAnimations;

/// <summary>
/// Animates a text string by appending dot characters cyclically (e.g., "Loading", "Loading.", "Loading..", etc.).
/// Automatically starts on construction and stops on disposal.
/// </summary>
public sealed class ScopedDotAnimator : IAsyncDisposable
{
    private readonly Task _animationTask;
    private readonly CancellationTokenSource _cts = new();


    public ScopedDotAnimator(
        Func<string, Task> updateTextAsync,
        string baseText,
        TimeSpan? interval = null,
        int maxDots = 3,
        bool padToMaxLength = true)
    {
        if (string.IsNullOrWhiteSpace(baseText))
            throw new ArgumentException("Base text cannot be null or empty.", nameof(baseText));

        if (maxDots < 1)
            throw new ArgumentOutOfRangeException(nameof(maxDots), "Must be at least 1");

        interval ??= TimeSpan.FromMilliseconds(500);

        _animationTask = Task.Run(async () =>
        {
            var dotCount = 0;
            while (!_cts.Token.IsCancellationRequested)
            {
                var dots = new string('.', dotCount);
                if (padToMaxLength)
                    dots = dots.PadRight(maxDots, ' ');

                string text = baseText + dots;
                await updateTextAsync(text);

                dotCount = (dotCount + 1) % (maxDots + 1);
                await Task.Delay(interval.Value, _cts.Token).ContinueWith(_ => { });
            }
        }, _cts.Token);
    }

    /// <summary>
    /// Stops the animation and cleans up resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        try
        {
            await _animationTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        _cts.Dispose();
    }

    /// <summary>
    /// Starts a new scoped dot animator that will run until disposed.
    /// </summary>
    /// <param name="updateTextAsync">An async function to receive the animated string updates.</param>
    /// <param name="baseText">The base text to which dots will be appended.</param>
    /// <param name="interval">The delay between updates. Default is 500ms.</param>
    /// <param name="maxDots">Maximum number of dots to cycle through.</param>
    /// <param name="padToMaxLength">If true, pads the text to ensure consistent length (avoids UI shifting).</param>
    public static ScopedDotAnimator Start(
        Func<string, Task> updateTextAsync,
        string baseText,
        TimeSpan? interval = null,
        int maxDots = 3,
        bool padToMaxLength = true)
    {
        return new ScopedDotAnimator(updateTextAsync, baseText, interval, maxDots, padToMaxLength);
    }

    /// <summary>
    /// Synchronous version of Start for use with simple action callbacks.
    /// </summary>
    public static ScopedDotAnimator Start(
        Action<string> updateText,
        string baseText,
        TimeSpan? interval = null,
        int maxDots = 3,
        bool padToMaxLength = true)
    {
        return new ScopedDotAnimator(text =>
        {
            updateText(text);
            return Task.CompletedTask;
        }, baseText, interval, maxDots, padToMaxLength);
    }
}