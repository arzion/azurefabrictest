using Ploeh.AutoFixture.Xunit2;

namespace Wirex.Engine.Tests.Infrastructure
{
    public class AutoMockDataAttribute : AutoDataAttribute
    {
        public AutoMockDataAttribute()
            : base(new TestAutoFixture())
        {
        }
    }
}