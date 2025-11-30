using System.ComponentModel.DataAnnotations;
using ProductApi.Application.DTOs;
using Xunit;

namespace ProductApi.Tests;

public class ValidationTests
{
    private IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void LoginRequest_WithValidData_PassesValidation()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        // Act
        var results = ValidateModel(loginRequest);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void LoginRequest_WithEmptyUsername_FailsValidation()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "",
            Password = "password123"
        };

        // Act
        var results = ValidateModel(loginRequest);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains("Username"));
    }

    [Fact]
    public void LoginRequest_WithShortUsername_FailsValidation()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "ab",
            Password = "password123"
        };

        // Act
        var results = ValidateModel(loginRequest);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains("Username"));
    }

    [Fact]
    public void LoginRequest_WithShortPassword_FailsValidation()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "12345"
        };

        // Act
        var results = ValidateModel(loginRequest);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains("Password"));
    }

    [Fact]
    public void ProductCreateDto_WithValidData_PassesValidation()
    {
        // Arrange
        var productDto = new ProductCreateDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 29.99m,
            Stock = 50
        };

        // Act
        var results = ValidateModel(productDto);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void ProductCreateDto_WithEmptyName_FailsValidation()
    {
        // Arrange
        var productDto = new ProductCreateDto
        {
            Name = "",
            Description = "Test Description",
            Price = 29.99m,
            Stock = 50
        };

        // Act
        var results = ValidateModel(productDto);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void ProductCreateDto_WithNegativePrice_FailsValidation()
    {
        // Arrange
        var productDto = new ProductCreateDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = -10.0m,
            Stock = 50
        };

        // Act
        var results = ValidateModel(productDto);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains("Price"));
    }

    [Fact]
    public void ProductCreateDto_WithZeroPrice_FailsValidation()
    {
        // Arrange
        var productDto = new ProductCreateDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 0m,
            Stock = 50
        };

        // Act
        var results = ValidateModel(productDto);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains("Price"));
    }

    [Fact]
    public void ProductCreateDto_WithNegativeStock_FailsValidation()
    {
        // Arrange
        var productDto = new ProductCreateDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 29.99m,
            Stock = -5
        };

        // Act
        var results = ValidateModel(productDto);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains("Stock"));
    }

    [Fact]
    public void ProductUpdateDto_WithValidData_PassesValidation()
    {
        // Arrange
        var productDto = new ProductUpdateDto
        {
            Name = "Updated Product",
            Price = 49.99m
        };

        // Act
        var results = ValidateModel(productDto);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void ProductUpdateDto_WithNegativePrice_FailsValidation()
    {
        // Arrange
        var productDto = new ProductUpdateDto
        {
            Price = -10.0m
        };

        // Act
        var results = ValidateModel(productDto);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains("Price"));
    }

    [Fact]
    public void ProductUpdateDto_WithNegativeStock_FailsValidation()
    {
        // Arrange
        var productDto = new ProductUpdateDto
        {
            Stock = -5
        };

        // Act
        var results = ValidateModel(productDto);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains("Stock"));
    }
}
