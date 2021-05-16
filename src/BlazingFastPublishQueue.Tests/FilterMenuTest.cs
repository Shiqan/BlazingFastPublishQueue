using BlazingFastPublishQueue.Models;
using Bunit;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BlazingFastPublishQueue.Tests
{
    public class FilterMenuTest
    {
        [Fact]
        public void ShouldRender()
        {
            using var ctx = new TestContext();
            ctx.Services.AddMudServices();

            var c = ctx.RenderComponent<Server.Shared.FilterMenu>(parameters => parameters
                .Add(p => p.Filter, new Filter())
                .Add(p => p.OnFilterChanged, args => { /* handle callback */ })
                .Add(p => p.PublishTargets, new List<string> { "Hello", "World" })
                .Add(p => p.UserSearch, UserSearch)
                .Add(p => p.Publications, new List<string> { "Publication01", "Publication02" })
                .Add(p => p.Servers, new List<string> { "Server01" })
            );

            Assert.Equal(1, c.RenderCount);
        }

        [Fact]
        public void ShouldUpdateAfterFilterChange()
        {
            using var ctx = new TestContext();
            ctx.Services.AddMudServices();

            var c = ctx.RenderComponent<Server.Shared.FilterMenu>(parameters => parameters
                .Add(p => p.Filter, new Filter())
                .Add(p => p.OnFilterChanged, args => { /* handle callback */ })
                .Add(p => p.PublishTargets, new List<string> { "Hello", "World" })
                .Add(p => p.UserSearch, UserSearch)
                .Add(p => p.Publications, new List<string> { "Publication01", "Publication02" })
                .Add(p => p.Servers, new List<string> { "Server01" })
            );

            c.Find(".mud-list-item").Click();

            Assert.Equal(2, c.RenderCount);
        }

        private Task<IEnumerable<string>> Search(string query)
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        private Func<string, Task<IEnumerable<string>>> UserSearch => Search;
    }
}
