using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ProductApi.Application.Common;
using ProductApi.Application.DTOs;
using ProductApi.Application.Services;
using ProductApi.Controllers;
using Xunit;

namespace ProductApi.Tests;

/// <summary>
/// Tests for the ReservationsController.
/// </summary>
public class ReservationsControllerTests : TestBase
{
    private readonly IReservationService _mockReservationService;
    private readonly ILogger<ReservationsController> _mockLogger;
    private readonly ReservationsController _controller;

    public ReservationsControllerTests()
    {
        _mockReservationService = Substitute.For<IReservationService>();
        _mockLogger = Substitute.For<ILogger<ReservationsController>>();
        _controller = new ReservationsController(_mockReservationService, _mockLogger, GetMockLocalizer());
        
        // Set up HttpContext
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region Reserve Tests

    [Fact]
    public async Task Reserve_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new ReserveRequest
        {
            ProductId = 1,
            Quantity = 5,
            TtlMinutes = 15
        };

        var reservationDto = new ReservationDto
        {
            ReservationId = Guid.NewGuid().ToString(),
            ProductId = 1,
            Quantity = 5,
            Status = "Reserved",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        _mockReservationService.ReserveAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Success(reservationDto));

        // Act
        var result = await _controller.Reserve(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(_controller.GetReservation), createdResult.ActionName);
        var returnedDto = Assert.IsType<ReservationDto>(createdResult.Value);
        Assert.Equal(reservationDto.ReservationId, returnedDto.ReservationId);
        Assert.Equal("Reserved", returnedDto.Status);
    }

    [Fact]
    public async Task Reserve_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var request = new ReserveRequest
        {
            ProductId = -1, // Invalid
            Quantity = 0    // Invalid
        };

        _controller.ModelState.AddModelError("ProductId", "ProductId must be positive");
        _controller.ModelState.AddModelError("Quantity", "Quantity must be positive");

        // Act
        var result = await _controller.Reserve(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Reserve_WithNonExistentProduct_ReturnsNotFound()
    {
        // Arrange
        var request = new ReserveRequest
        {
            ProductId = 999,
            Quantity = 1
        };

        _mockReservationService.ReserveAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ReservationDto>(Error.NotFound("Product with ID 999 not found")));

        // Act
        var result = await _controller.Reserve(request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Reserve_WithInsufficientStock_ReturnsBadRequest()
    {
        // Arrange
        var request = new ReserveRequest
        {
            ProductId = 1,
            Quantity = 1000
        };

        _mockReservationService.ReserveAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ReservationDto>(Error.Validation("Insufficient stock available")));

        // Act
        var result = await _controller.Reserve(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    #endregion

    #region GetReservation Tests

    [Fact]
    public async Task GetReservation_WithValidId_ReturnsReservation()
    {
        // Arrange
        var reservationId = Guid.NewGuid().ToString();
        var reservationDto = new ReservationDto
        {
            ReservationId = reservationId,
            ProductId = 1,
            Quantity = 3,
            Status = "Reserved",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        _mockReservationService.GetByIdAsync(reservationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(reservationDto));

        // Act
        var result = await _controller.GetReservation(reservationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ReservationDto>(okResult.Value);
        Assert.Equal(reservationId, returnedDto.ReservationId);
    }

    [Fact]
    public async Task GetReservation_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var reservationId = Guid.NewGuid().ToString();

        _mockReservationService.GetByIdAsync(reservationId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ReservationDto>(Error.NotFound("Reservation not found")));

        // Act
        var result = await _controller.GetReservation(reservationId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetReservation_WithInvalidIdFormat_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = "not-a-valid-guid";

        _mockReservationService.GetByIdAsync(invalidId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ReservationDto>(Error.Validation("Invalid reservation ID format")));

        // Act
        var result = await _controller.GetReservation(invalidId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    #endregion

    #region Checkout Tests

    [Fact]
    public async Task Checkout_WithValidReservation_ReturnsCompletedReservation()
    {
        // Arrange
        var reservationId = Guid.NewGuid().ToString();
        var request = new CheckoutRequest { ReservationId = reservationId };

        var reservationDto = new ReservationDto
        {
            ReservationId = reservationId,
            ProductId = 1,
            Quantity = 2,
            Status = "Completed",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CompletedAt = DateTime.UtcNow
        };

        _mockReservationService.CheckoutAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Success(reservationDto));

        // Act
        var result = await _controller.Checkout(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ReservationDto>(okResult.Value);
        Assert.Equal("Completed", returnedDto.Status);
        Assert.NotNull(returnedDto.CompletedAt);
    }

    [Fact]
    public async Task Checkout_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var request = new CheckoutRequest { ReservationId = "" };
        _controller.ModelState.AddModelError("ReservationId", "ReservationId is required");

        // Act
        var result = await _controller.Checkout(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Checkout_WithNonExistentReservation_ReturnsNotFound()
    {
        // Arrange
        var request = new CheckoutRequest { ReservationId = Guid.NewGuid().ToString() };

        _mockReservationService.CheckoutAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ReservationDto>(Error.NotFound("Reservation not found")));

        // Act
        var result = await _controller.Checkout(request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Checkout_WithExpiredReservation_ReturnsBadRequest()
    {
        // Arrange
        var request = new CheckoutRequest { ReservationId = Guid.NewGuid().ToString() };

        _mockReservationService.CheckoutAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ReservationDto>(Error.Validation("Reservation has expired")));

        // Act
        var result = await _controller.Checkout(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    #endregion

    #region Cancel Tests

    [Fact]
    public async Task Cancel_WithValidReservation_ReturnsCancelledReservation()
    {
        // Arrange
        var reservationId = Guid.NewGuid().ToString();

        var reservationDto = new ReservationDto
        {
            ReservationId = reservationId,
            ProductId = 1,
            Quantity = 3,
            Status = "Cancelled",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CompletedAt = DateTime.UtcNow
        };

        _mockReservationService.CancelAsync(reservationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(reservationDto));

        // Act
        var result = await _controller.Cancel(reservationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ReservationDto>(okResult.Value);
        Assert.Equal("Cancelled", returnedDto.Status);
    }

    [Fact]
    public async Task Cancel_WithNonExistentReservation_ReturnsNotFound()
    {
        // Arrange
        var reservationId = Guid.NewGuid().ToString();

        _mockReservationService.CancelAsync(reservationId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ReservationDto>(Error.NotFound("Reservation not found")));

        // Act
        var result = await _controller.Cancel(reservationId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Cancel_WithAlreadyCompletedReservation_ReturnsBadRequest()
    {
        // Arrange
        var reservationId = Guid.NewGuid().ToString();

        _mockReservationService.CancelAsync(reservationId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ReservationDto>(Error.Validation("Cannot cancel reservation with status: Completed")));

        // Act
        var result = await _controller.Cancel(reservationId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    #endregion
}
