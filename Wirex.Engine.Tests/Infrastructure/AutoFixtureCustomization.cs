using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace Wirex.Engine.Tests.Infrastructure
{
    public class AutoFixtureCustomization : CompositeCustomization
    {
        public AutoFixtureCustomization()
            : base(
                new StableFiniteSequenceCustomization(),
                new MultipleCustomization(),
                new AutoMoqCustomization())
        {
        }
    }
}