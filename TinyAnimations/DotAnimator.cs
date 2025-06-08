namespace TinyAnimations;

/// <summary>
/// A manually controlled dot animator that updates a bound string periodically.
/// </summary>
public sealed class DotAnimator
{
    private readonly Task _animationTask;
    private readonly CancellationTokenSource _cts = new();

    private DotAnimator(
        Func<string, Task> updateTextAsync,
        string baseText,
        TimeSpan interval,
        int maxDots,
        bool padToMaxLength)
    {
        if (string.IsNullOrWhiteSpace(baseText))
            throw new ArgumentException("Base text cannot be null or empty.", nameof(baseText));

        if (maxDots < 1)
            throw new ArgumentOutOfRangeException(nameof(maxDots), "Must be at least 1");

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
                await Task.Delay(interval, _cts.Token).ContinueWith(_ => { });
            }
        }, _cts.Token);
    }

    /// <summary>
    /// Starts a manually controlled animator that must be stopped explicitly.
    /// </summary>
    /// <param name="updateTextAsync">An async function to receive the animated string updates.</param>
    /// <param name="baseText">The base text to which dots will be appended.</param>
    /// <param name="interval">The delay between updates. Default is 500ms.</param>
    /// <param name="maxDots">Maximum number of dots to cycle through.</param>
    /// <param name="padToMaxLength">If true, pads the text to ensure consistent length (avoids UI shifting).</param>
    public static DotAnimator Start(
        Func<string, Task> updateTextAsync,
        string baseText,
        TimeSpan? interval = null,
        int maxDots = 3,
        bool padToMaxLength = true)
    {
        return new DotAnimator(updateTextAsync, baseText, interval ?? TimeSpan.FromMilliseconds(500), maxDots,
            padToMaxLength);
    }

    /// <summary>
    /// Synchronous version of Start for use with simple action callbacks.
    /// </summary>
    public static DotAnimator Start(
        Action<string> updateText,
        string baseText,
        TimeSpan? interval = null,
        int maxDots = 3,
        bool padToMaxLength = true)
    {
        return Start(
            text =>
            {
                updateText(text);
                return Task.CompletedTask;
            },
            baseText, interval, maxDots, padToMaxLength);
    }

    /// <summary>
    /// Stops the animation loop.
    /// </summary>
    public void Stop()
    {
        _cts.Cancel();
    }

    /// <summary>
    /// Waits for the animation task to complete after stopping.
    /// </summary>
    public async Task WaitForCompletionAsync()
    {
        try
        {
            await _animationTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }
}