using Microservices.UnitTest.Fixtures;

[assembly: AssemblyFixture(typeof(DatabaseFixture))]

namespace Microservices.UnitTest.Fixtures;
public class DatabaseFixture : IDisposable
{
    public DatabaseFixture()
    {
        // initialize database
    }
    public void Dispose()
    {
        // cleanup database
    }
}
