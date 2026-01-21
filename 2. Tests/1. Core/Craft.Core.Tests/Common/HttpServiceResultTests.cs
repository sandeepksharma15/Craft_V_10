using Xunit;
using Craft.Core.Common;
using System.Collections.Generic;

namespace Craft.Core.Tests.Common
{
    public class HttpServiceResultTests
    {
        [Fact]
        public void HttpServiceResult_Record_PropertyAssignment_WorksCorrectly()
        {
            var result = new HttpServiceResult<string>
            {
                Data = "TestData",
                IsSuccess = true,
                Errors = ["Error1", "Error2"],
                StatusCode = 200
            };

            Assert.Equal("TestData", result.Data);
            Assert.True(result.IsSuccess);
            Assert.Equal(["Error1", "Error2"], result.Errors);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void HttpServiceResult_DefaultValues_AreNullOrFalse()
        {
            var result = new HttpServiceResult<int>();

            Assert.Equal(default, result.Data);
            Assert.False(result.IsSuccess);
            Assert.Null(result.Errors);
            Assert.Null(result.StatusCode);
        }

        [Fact]
        public void SuccessResult_FactoryMethod_SetsPropertiesCorrectly()
        {
            var result = HttpServiceResult<string>.SuccessResult("SuccessData", 201);

            Assert.Equal("SuccessData", result.Data);
            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            Assert.Null(result.Errors);
            Assert.False(result.HasErrors);
            Assert.False(result.IsFailure);
            Assert.Null(result.Message);
        }

        [Fact]
        public void FailureResult_FactoryMethod_SetsPropertiesCorrectly()
        {
            var errors = new List<string> { "Error1", "Error2" };
            var result = HttpServiceResult<string>.FailureResult(errors, 404);

            Assert.Null(result.Data);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal(errors, result.Errors);
            Assert.True(result.HasErrors);
            Assert.True(result.IsFailure);
            Assert.Equal("Error1, Error2", result.Message);
        }

        [Fact]
        public void Message_ReturnsNull_WhenNoErrors()
        {
            var result = new HttpServiceResult<string> { IsSuccess = false };
            Assert.Null(result.Message);
        }

        [Fact]
        public void Message_ReturnsCommaSeparatedErrors_WhenErrorsExist()
        {
            var result = new HttpServiceResult<string> { Errors = ["A", "B", "C"] };
            Assert.Equal("A, B, C", result.Message);
        }
    }
}
