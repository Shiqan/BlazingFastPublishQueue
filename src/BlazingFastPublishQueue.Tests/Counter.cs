using Bunit;
using Xunit;

namespace BlazingFastPublishQueue.Tests
{
    public class Counter
    {
        [Fact]
        public void CounterShouldIncrementWhenSelected()
        {
            // Arrange
            using var ctx = new TestContext();
            var c = ctx.RenderComponent<Server.Pages.Counter>();
            var paragraph = c.Find("p");

            // Act
            c.Find("button").Click();
            var paraElmText = paragraph.TextContent;

            // Assert
            paraElmText.MarkupMatches("Current count: 1");
        }
    }
}
