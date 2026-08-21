using Xunit;

namespace Polyglossy.Interactive.App.Tests;

public class PublicNamespaceRenameTests
{
    [Fact]
    public void core_kernel_type_is_available_from_polyglossy_namespace()
    {
        Assert.Equal("Polyglossy.Interactive.Kernel", typeof(Polyglossy.Interactive.Kernel).FullName);
    }

    [Fact]
    public void kernel_info_type_is_available_from_polyglossy_namespace()
    {
        Assert.Equal("Polyglossy.Interactive.KernelInfo", typeof(Polyglossy.Interactive.KernelInfo).FullName);
    }

    [Fact]
    public void kernel_invocation_context_type_is_available_from_polyglossy_namespace()
    {
        Assert.Equal("Polyglossy.Interactive.KernelInvocationContext", typeof(Polyglossy.Interactive.KernelInvocationContext).FullName);
    }
}
