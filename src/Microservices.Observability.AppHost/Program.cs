using Microservices.Observability.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddKafka(name: "messaging", 9092)
    .WithKafkaUI(kafkaUI => kafkaUI.WithHostPort(9100).WithLifetime(ContainerLifetime.Persistent))
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);

var postgres = builder.AddPostgres("postgres", port: 5432)
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050).WithLifetime(ContainerLifetime.Persistent))
    .WithPgWeb(pgWeb => pgWeb.WithLifetime(ContainerLifetime.Persistent))
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
