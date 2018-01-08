using Ploeh.AutoFixture;

namespace Wirex.Engine.Tests.Infrastructure
{
    public class TestAutoFixture : Fixture
    {
        public TestAutoFixture()
        {
            Behaviors.Add(new OmitOnRecursionBehavior());
            Customize(new AutoFixtureCustomization());
        }
    }
}