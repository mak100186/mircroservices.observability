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
}
