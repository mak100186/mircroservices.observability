using Aspire.Hosting.Lifecycle;
using Microservices.Observability.AppHost;

var builder = DistributedApplication.CreateBuilder(args);
builder.Services.AddLifecycleHook<LifecycleLogger>();

var cache = builder.AddRedis("cache")
    .WithDataVolume(isReadOnly: false)
    .WithPersistence(interval: TimeSpan.FromMinutes(5), keysChangedThreshold: 100)
    .WithRedisInsight(insight => insight.WithLifetime(ContainerLifetime.Persistent))
    .WithRedisCommander(commander => commander.WithLifetime(ContainerLifetime.Persistent));

var messaging = builder.AddKafka(name: "kafka-server")
    .WithKafkaUI(kafkaUI => kafkaUI.WithLifetime(ContainerLifetime.Persistent))
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin(pgAdmin => pgAdmin.WithLifetime(ContainerLifetime.Persistent))
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);

//Aggregator
var postgresdb = postgres.AddDatabase("postgresdb", "aggregation-persistence");

var migrationRunner = builder.AddProject<Projects.Microservice_Aggregation_MigrationRunner>("migrationrunner")
    .WithReference(postgres)
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

var aggregator = builder.AddProject<Projects.Microservice_Aggregation>("microservice-aggregation")
    .WithReference(messaging)
    .WithReference(postgres)
    .WithReference(postgresdb)
    .WaitFor(messaging)
    .WaitFor(postgresdb)
    .WaitForCompletion(migrationRunner);

//Enrichment
var enricher = builder.AddProject<Projects.Microservice_Enrichment>("microservice-enrichment")
    .WithReference(cache)
    .WaitFor(cache)
    .WithSwaggerUI()
    .WithReDoc()
    .WithScalar();

//Presenter
builder.AddProject<Projects.Microservice_Presenter>("microservice-presenter")
    .WithReference(postgres)
    .WithReference(postgresdb)
    .WaitFor(enricher)
    .WaitFor(postgresdb)
    .WaitForCompletion(migrationRunner)
    .WithSwaggerUI()
    .WithReDoc()
    .WithScalar();

//Cluster 1
{
    var feedGenOne = builder.AddProject<Projects.Feed_Generator_One>("feed-generator-one")
    .WithSwaggerUI()
    .WithReDoc()
    .WithScalar();

    var oneConverter = builder.AddProject<Projects.Microservice_One_Converter>("microservice-one-converter")
        .WithReference(messaging)
        .WaitFor(messaging)
        .WaitFor(aggregator);

    builder.AddProject<Projects.Microservice_One_Receiver>("microservice-one-receiver")
        .WithReference(messaging)
        .WaitFor(messaging)
        .WithReference(feedGenOne)
        .WaitFor(feedGenOne)
        .WaitFor(aggregator)
        .WaitFor(oneConverter);
}

//Cluster 2
{
    var feedGenTwo = builder.AddProject<Projects.Feed_Generator_Two>("feed-generator-two")
    .WithSwaggerUI()
    .WithReDoc()
    .WithScalar();

    var twoConverter = builder.AddProject<Projects.Microservice_Two_Converter>("microservice-two-converter")
        .WithReference(messaging)
        .WaitFor(messaging)
        .WaitFor(aggregator);

    builder.AddProject<Projects.Microservice_Two_Receiver>("microservice-two-receiver")
        .WithReference(messaging)
        .WaitFor(messaging)
        .WithReference(feedGenTwo)
        .WaitFor(feedGenTwo)
        .WaitFor(aggregator)
        .WaitFor(twoConverter);
}

builder.Build().Run();
