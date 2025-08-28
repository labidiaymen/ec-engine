using Xunit;

namespace ECEngine.Tests;

[CollectionDefinition("ConsoleTests", DisableParallelization = true)]
public class ConsoleTestCollection
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
