using System;
using Xunit;
using Xunit.Abstractions;
using StarlightDeck_To_Discord;

namespace StarlightDeck_To_Discord.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;
        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }
        [Fact]
        public void Test1()
        {
            var getAPIResult = StarlightAPIController.GetAPI().GetAwaiter().GetResult();
            this.output.WriteLine(getAPIResult);
            Assert.True(true);
        }
    }
}
