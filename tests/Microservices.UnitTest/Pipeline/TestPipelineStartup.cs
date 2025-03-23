using Xunit.Sdk;
using Xunit.v3;

namespace Microservices.UnitTest.Pipeline;
public class TestPipelineStartup(ITestOutputHelper output) : ITestPipelineStartup
{
    public ValueTask StartAsync(IMessageSink diagnosticMessageSink)
    {
        output.WriteLine("Starting test pipeline");

        //use this to bring up infrastructure for the test

        return ValueTask.CompletedTask;
    }
    public ValueTask StopAsync()
    {
        output.WriteLine("Stopping test pipeline");

        //use this to break down infrastructure for the test

        return ValueTask.CompletedTask;
    }
}
