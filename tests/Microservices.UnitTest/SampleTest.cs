using Microservices.UnitTest.Fixtures;

namespace Microservices.UnitTest;

public class SampleTest(ITestOutputHelper output, DatabaseFixture databaseFixture)
{
    [Fact]
    public void TestOutput()
    {
        output.WriteLine("This is a test output");

        Assert.True(true);
    }

    [Fact]
    public void Skip()
    {
        output.WriteLine("Skip");

        Assert.Skip("Cuz i said so");
    }

    [Fact]
    public void SkipWhen()
    {
        output.WriteLine("Assert.SkipWhen");

        Assert.SkipWhen(true, "condinally skipping");
    }

    [Fact(SkipExceptions = [typeof(ArgumentException)])]
    public void SkipExceptions()
    {
        output.WriteLine("Assert.SkipExceptions");
        output.WriteLine("Wont fail if ArgumentException occurs. ");
    }

    [Fact(Explicit = true)]
    public void TestExplicit()
    {
        output.WriteLine("Only run when explicitly executed");

        Assert.True(true);
    }

}
