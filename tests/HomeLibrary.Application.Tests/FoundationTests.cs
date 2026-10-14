namespace HomeLibrary.Application.Tests;

public class FoundationTests
{
    [Fact]
    public void Application_assembly_is_loadable()
    {
        var assembly = typeof(HomeLibrary.Application.AssemblyMarker).Assembly;
        Assert.False(string.IsNullOrWhiteSpace(assembly.GetName().Name));
    }
}
