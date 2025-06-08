namespace TinyAnimations.Tests;

public sealed class DotAnimatorTests
{
    [Fact]
    public async Task ScopedDotAnimator_UpdatesTextInLoop()
    {
        var updates = new List<string>();

        await using (ScopedDotAnimator.Start(text =>
                     {
                         lock (updates)
                         {
                             updates.Add(text);
                         }

                         return Task.CompletedTask;
                     }, "Loading", TimeSpan.FromMilliseconds(100)))
        {
            await Task.Delay(450);
        }

        Assert.True(updates.Count >= 3);
        Assert.Contains("Loading", updates[0]);
    }

    [Fact]
    public async Task DotAnimator_StopsCorrectly()
    {
        var updates = new List<string>();
        var controller = DotAnimator.Start(text =>
        {
            lock (updates)
            {
                updates.Add(text);
            }
        }, "Processing", TimeSpan.FromMilliseconds(100));

        await Task.Delay(300);
        controller.Stop();
        await controller.WaitForCompletionAsync();

        int countAfterStop = updates.Count;
        await Task.Delay(300);

        Assert.True(countAfterStop >= 2);
        Assert.Equal(countAfterStop, updates.Count);
    }

    [Fact]
    public void ScopedDotAnimator_ThrowsOnEmptyBaseText()
    {
        Assert.Throws<ArgumentException>(() =>
            ScopedDotAnimator.Start(_ => Task.CompletedTask, ""));
    }

    [Fact]
    public void DotAnimator_ThrowsOnInvalidMaxDots()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DotAnimator.Start(_ => Task.CompletedTask, "Text", maxDots: 0));
    }
}