using Craft.Core;

namespace Craft.Domain.Tests.Helpers;

public class PageInfoTests
{
    [Fact]
    public void PaginationInfo_BasicProperties()
    {
        // Arrange
        var info = new PageInfo { CurrentPage = 2, PageSize = 10, TotalCount = 100 };

        // Assert
        Assert.Equal(2, info.CurrentPage);
        Assert.Equal(10, info.PageSize);
        Assert.Equal(100, info.TotalCount);
    }

    [Fact]
    public void PaginationInfo_From_Calculation()
    {
        // Arrange
        var info = new PageInfo { CurrentPage = 2, PageSize = 10, TotalCount = 100 };

        // Assert
        Assert.Equal(11, info.From);
    }

    [Fact]
    public void PaginationInfo_To_Calculation_FullPage()
    {
        // Arrange
        var info = new PageInfo { CurrentPage = 2, PageSize = 10, TotalCount = 100 };

        // Assert
        Assert.Equal(20, info.To);
    }

    [Fact]
    public void PaginationInfo_To_Calculation_PartialPage()
    {
        // Arrange
        var info = new PageInfo { CurrentPage = 9, PageSize = 10, TotalCount = 85 };

        // Assert
        Assert.Equal(85, info.To);
    }

    [Fact]
    public void PaginationInfo_TotalPages_Calculation()
    {
        // Arrange
        var info = new PageInfo { PageSize = 10, TotalCount = 100 };

        // Assert
        Assert.Equal(10, info.TotalPages);
    }

    [Fact]
    public void PaginationInfo_TotalPages_OnePage_Calculation()
    {

        var info = new PageInfo { PageSize = 10, TotalCount = 5 };


        Assert.Equal(1, info.TotalPages);
    }

    [Fact]
    public void PaginationInfo_HasNextPage_True()
    {
        // Arrange
        var info = new PageInfo { CurrentPage = 1, PageSize = 10, TotalCount = 100 };
        // Assert
        Assert.True(info.HasNextPage);
    }

    [Fact]
    public void PaginationInfo_HasNextPage_False()
    {
        // Arrange
        var info = new PageInfo { CurrentPage = 10, PageSize = 10, TotalCount = 100 };

        // Assert
        Assert.False(info.HasNextPage);
    }

    [Fact]
    public void PaginationInfo_HasPreviousPage_True()
    {
        // Arrange
        var info = new PageInfo { CurrentPage = 2, PageSize = 10, TotalCount = 100 };

        // Assert
        Assert.True(info.HasPreviousPage);
    }

    [Fact]
    public void PaginationInfo_HasPreviousPage_False()
    {
        // Arrange
        var info = new PageInfo { CurrentPage = 1, PageSize = 10, TotalCount = 100 };

        // Assert
        Assert.False(info.HasPreviousPage);
    }
}
