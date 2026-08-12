using Aetherphone.Core;
using Xunit;

namespace Aetherphone.Tests;

public sealed class PollCadenceStretchTests
{
    private static PhoneVisibility Visible()
    {
        var visibility = new PhoneVisibility();
        visibility.Bind(static () => true);
        return visibility;
    }

    [Fact]
    public void TheStretchedIntervalWinsWhileTheConditionHolds()
    {
        var cadence = new PollCadence(Visible(), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(120));
        var stretched = false;
        cadence.StretchWhen(() => stretched, TimeSpan.FromSeconds(300));

        Assert.Equal(TimeSpan.FromSeconds(60), cadence.CurrentInterval);

        stretched = true;
        Assert.Equal(TimeSpan.FromSeconds(300), cadence.CurrentInterval);

        stretched = false;
        Assert.Equal(TimeSpan.FromSeconds(60), cadence.CurrentInterval);
    }

    [Fact]
    public void AStretchedCadenceStillHonorsAnImmediateRequest()
    {
        var cadence = new PollCadence(Visible(), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(120));
        cadence.StretchWhen(static () => true, TimeSpan.FromSeconds(300));
        var now = DateTime.UtcNow;

        Assert.True(cadence.Due(now));
        Assert.False(cadence.Due(now + TimeSpan.FromSeconds(120)));

        cadence.RequestImmediate();
        Assert.True(cadence.Due(now + TimeSpan.FromSeconds(121)));
    }

    [Fact]
    public void TheStretchedIntervalElapsesEventually()
    {
        var cadence = new PollCadence(Visible(), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(120));
        cadence.StretchWhen(static () => true, TimeSpan.FromSeconds(300));
        var now = DateTime.UtcNow;

        Assert.True(cadence.Due(now));
        Assert.False(cadence.Due(now + TimeSpan.FromSeconds(299)));
        Assert.True(cadence.Due(now + TimeSpan.FromSeconds(301)));
    }
}
