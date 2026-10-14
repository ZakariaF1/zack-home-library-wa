namespace HomeLibrary.Domain.Tests;

public class FoundationTests
{
    [Fact]
    public void Domain_assembly_is_loadable()
    {
        var assembly = typeof(HomeLibrary.Domain.AssemblyMarker).Assembly;
        Assert.False(string.IsNullOrWhiteSpace(assembly.GetName().Name));
    }
}
