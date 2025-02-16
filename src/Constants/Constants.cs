namespace Constants;

public static class Constants
{
    public const int NumberOfDaysOfWeatherForecast = 5;
    public static class Kafka
    {
        public const string ConnectionName = "kafka-server";
    }

    public static class Postgres
    {
        public const string ConnectionName = "postgresdb";
    }

    public static class TopicNames
    {
        public const string OneReceiverConverter = "One.Receiver-Converter";
        public const string TwoReceiverConverter = "Two.Receiver-Converter";

        public const string OneConverterAggregator = "One.Converter-Aggregator";
        public const string TwoConverterAggregator = "Two.Converter-Aggregator";
    }

    public static class ObservabilityMetrics
    {
        public const string MeterName = "Microservices.Observability";

        public const string PresentationRequestsCount = "presentation.requests.count";
        public const string EnricherRequestCount = "enricher.requests.count";
        public const string PresentationRequestsDuration = "presentation.requests.duration";
        public const string EnricherRequestsDuration = "enricher.requests.duration";
    }

    public static class Cors
    {
        public const string AnyOriginPolicy = "AnyOrigin";
    }
}
