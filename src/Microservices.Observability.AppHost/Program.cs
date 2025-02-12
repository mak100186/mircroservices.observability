using Microservices.Observability.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddKafka(name: "messaging", 9092)
    .WithKafkaUI(kafkaUI => kafkaUI.WithHostPort(9100).WithLifetime(ContainerLifetime.Persistent))
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);

//Aggregator
var postgres = builder.AddPostgres("postgres", port: 5432)
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050).WithLifetime(ContainerLifetime.Persistent))
    .WithPgWeb(pgWeb => pgWeb.WithLifetime(ContainerLifetime.Persistent))
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);

var postgresdb = postgres.AddDatabase("postgresdb", "aggregation-persistence");

var migrationRunner = builder.AddProject<Projects.Microservice_Aggregation_MigrationRunner>("migrationrunner")
    .WithReference(postgres)
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

var aggregator = builder.AddProject<Projects.Microservice_Aggregation>("microservice-aggregation")
    .WithReference(messaging)
    .WithReference(postgres)
    .WithReference(postgresdb)
    .WithReference(migrationRunner)
    .WaitFor(messaging)
    .WaitFor(postgresdb)
    .WaitForCompletion(migrationRunner);

var enricher = builder.AddProject<Projects.Microservice_Enrichment>("microservice-enrichment")
    .WithSwaggerUI()
    .WithReDoc()
    .WithScalar();

builder.AddProject<Projects.Microservice_Presenter>("microservice-presenter")
    .WithReference(postgres)
    .WithReference(postgresdb)
    .WithReference(migrationRunner)
    .WaitFor(enricher)
    .WaitFor(postgresdb)
    .WaitForCompletion(migrationRunner)
    .WithSwaggerUI()
    .WithReDoc()
    .WithScalar();

//Cluster 1
builder.AddProject<Projects.Feed_Generator_One>("feed-generator-one")
    .WithSwaggerUI()
    .WithReDoc()
    .WithScalar();

builder.AddProject<Projects.Microservice_One_Receiver>("microservice-one-receiver")
    .WithReference(messaging)
    .WithReference(aggregator)
    .WaitFor(messaging)
    .WaitFor(aggregator);

builder.AddProject<Projects.Microservice_One_Converter>("microservice-one-converter")
    .WithReference(messaging)
    .WithReference(aggregator)
    .WaitFor(messaging)
    .WaitFor(aggregator);

//Cluster 2
builder.AddProject<Projects.Feed_Generator_Two>("feed-generator-two")
    .WithSwaggerUI()
    .WithReDoc()
    .WithScalar();

builder.AddProject<Projects.Microservice_Two_Receiver>("microservice-two-receiver")
    .WithReference(messaging)
    .WithReference(aggregator)
    .WaitFor(messaging)
    .WaitFor(aggregator);

builder.AddProject<Projects.Microservice_Two_Converter>("microservice-two-converter")
    .WithReference(messaging)
    .WithReference(aggregator)
    .WaitFor(messaging)
    .WaitFor(aggregator);











builder.Build().Run();
