using Xunit;
using Craft.Core.Common;
using System.Collections.Generic;

namespace Craft.Core.Tests.Common
{
    public class HttpServiceResultTests
    {
        [Fact]
        public void HttpServiceResult_PropertyAssignment_WorksCorrectly()
        {
            var result = new HttpServiceResult<string>
            {
                Data = "TestData",
                Success = true,
                Errors = new List<string> { "Error1", "Error2" },
                StatusCode = 200
            };

            Assert.Equal("TestData", result.Data);
            Assert.True(result.Success);
            Assert.Equal(new List<string> { "Error1", "Error2" }, result.Errors);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void HttpServiceResult_DefaultValues_AreNullOrFalse()
        {
            var result = new HttpServiceResult<int>();

            Assert.Null(result.Data);
            Assert.False(result.Success);
            Assert.Null(result.Errors);
            Assert.Null(result.StatusCode);
        }
    }
}
