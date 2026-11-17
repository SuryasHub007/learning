using System.Reflection;
using Backend.Application;
using Backend.Domain.Entities;

namespace Backend.UnitTests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_DoesNotDependOnOtherLayers()
    {
        Assert.Empty(ProjectReferences(typeof(BaseEntity).Assembly));
    }

    [Fact]
    public void Application_DoesNotDependOnOuterLayers()
    {
        // The compiler drops unused references, so Domain may not be listed here yet.
        Assert.All(
            ProjectReferences(typeof(DependencyInjection).Assembly),
            name => Assert.Equal("Backend.Domain", name));
    }

    private static IEnumerable<string?> ProjectReferences(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name!.StartsWith("Backend."));
}