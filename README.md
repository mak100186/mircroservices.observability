# Overview
Shows how to use observability in microservices using a simple example. Here is what the project structure looks like:

```mermaid
graph TD
    subgraph Cluster1
        A[Feed.Generator.One]
        B[Microservice.One.Receiver]
        T1[[One.Receiver-Converter]]
        C[Microservice.One.Converter]
        T3[[One.Converter-Aggregator]]
    end

    subgraph Cluster2
        D[Feed.Generator.Two]
        E[Microservice.Two.Receiver]
        T2[[Two.Receiver-Converter]]
        F[Microservice.Two.Converter]
        T4[[Two.Converter-Aggregator]]
    end

    G[Microservice.Aggregation]
    DB[(Aggregation.Persistence)]

    A -- Polls every 1 minute --> B
    B --> T1
    T1 --> C
    C --> T3
    D -- Polls every 1 minute --> E
    E --> T2
    T2 --> F
    F --> T4
    T3 --> G
    T4 --> G
    G --> DB
```

Here is a sequence of events involving the cluster:
```mermaid
sequenceDiagram
    participant Feed.Generator.One as Feed Generator One
    participant Microservice.One.Receiver as Receiver
    participant One.Receiver-Converter as {Cluster}.Receiver-Converter
    participant Microservice.One.Converter as Converter
    participant One.Converter-Aggregator as {Cluster}.Converter-Aggregator
    participant Microservice.Aggregation as Aggregator
    participant Aggregation.Persistence as Database
    participant Endpoint as [GET] ~/{city}/{date}

    Microservice.One.Receiver ->> Feed.Generator.One: Poll for Data (every 1 minute)
    Feed.Generator.One -->> Microservice.One.Receiver: Send Data
    loop For each city each day
        Microservice.One.Receiver ->> One.Receiver-Converter: Push Forecast Message
    end
    One.Receiver-Converter ->> Microservice.One.Converter: Consume Data
    Microservice.One.Converter ->> Microservice.One.Converter: Add feedProvider Information
    Microservice.One.Converter ->> One.Converter-Aggregator: Push Processed Data
    One.Converter-Aggregator ->> Microservice.Aggregation: Consume Processed Data
    Microservice.Aggregation ->> Aggregation.Persistence: Add or Update Forecasts
    Endpoint ->> Microservice.Aggregation: Request Forecast Data
    Microservice.Aggregation ->> Aggregation.Persistence: Query Data
    Aggregation.Persistence -->> Microservice.Aggregation: Return Data
    Microservice.Aggregation -->> Endpoint: Return Forecast Data
```
Next tasks:
1. Presenter should use hybrid cache with redis for country data instead of calling enricher. use ResponseCaching
2. Add exception handling and visitor pattern for it
3. Specify DateOnly format on swagger UI as yyyy/mm/dd
4. Add schemas, use the recent short from Nick about it. 
5. Log improvements
6. Update diagrams and documentation
