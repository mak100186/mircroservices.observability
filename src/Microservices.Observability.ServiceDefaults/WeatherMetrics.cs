using System.Diagnostics.Metrics;

using static Constants.Constants.ObservabilityMetrics;

namespace Microservices.Observability.ServiceDefaults;
public class WeatherMetrics
{
    private readonly TimeProvider _timeProvider;

    private readonly Counter<long> _presentationRequestCounter;
    private readonly Counter<long> _enricherRequestCounter;

    private readonly Histogram<double> _presentationRequestDuration;
    private readonly Histogram<double> _enricherRequestDuration;

    public WeatherMetrics(TimeProvider timeProvider, IMeterFactory meterFactory)
    {
        _timeProvider = timeProvider;

        var meter = meterFactory.Create(MeterName);

        _presentationRequestCounter = meter.CreateCounter<long>(PresentationRequestsCount);
        _enricherRequestCounter = meter.CreateCounter<long>(EnricherRequestCount);

        _presentationRequestDuration = meter.CreateHistogram<double>(PresentationRequestsDuration, "ms");
        _enricherRequestDuration = meter.CreateHistogram<double>(EnricherRequestsDuration, "ms");
    }

    public void IncrementPresentationRequestCounter() => _presentationRequestCounter.Add(1);
    public void IncrementEnricherRequestCounter() => _enricherRequestCounter.Add(1);

    public TrackedRequestDuration MeasurePresentationRequestDuration() =>
        new(_timeProvider, _presentationRequestDuration);

    public TrackedRequestDuration MeasureEnricherRequestDuration() =>
        new(_timeProvider, _enricherRequestDuration);
}

public sealed class TrackedRequestDuration(TimeProvider timeProvider, Histogram<double> histogram) : IDisposable
{
    private readonly long _requestStartTime = timeProvider.GetTimestamp();
    private readonly Histogram<double> _histogram = histogram;
    public void Dispose()
    {
        var elapsedTimeSpan = timeProvider.GetElapsedTime(_requestStartTime);
        _histogram.Record(elapsedTimeSpan.TotalMilliseconds);
    }
}
