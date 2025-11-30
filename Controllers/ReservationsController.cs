using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ProductApi.Application.Common;
using ProductApi.Application.DTOs;
using ProductApi.Application.Services;
using ProductApi.Extensions;
using ProductApi.Resources;

namespace ProductApi.Controllers;

/// <summary>
/// Manages product reservations including reserve, checkout, and cancel operations.
/// Follows DDD principles with thin controller delegating to ReservationService.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationsController> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    /// <summary>
    /// Initializes a new instance of the ReservationsController.
    /// </summary>
    /// <param name="reservationService">The reservation application service.</param>
    /// <param name="logger">The logger for reservation operations.</param>
    /// <param name="localizer">The string localizer for localized messages.</param>
    public ReservationsController(IReservationService reservationService, ILogger<ReservationsController> logger, IStringLocalizer<SharedResource> localizer)
    {
        _reservationService = reservationService;
        _logger = logger;
        _localizer = localizer;
    }

    /// <summary>
    /// Creates a reservation for a product.
    /// </summary>
    /// <param name="request">The reservation request containing productId, quantity, and optional TTL.</param>
    /// <returns>The created reservation details.</returns>
    /// <response code="201">Reservation created successfully.</response>
    /// <response code="400">Invalid request - validation errors or insufficient stock.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Product not found.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/reservations
    ///     {
    ///        "productId": 1,
    ///        "quantity": 2,
    ///        "ttlMinutes": 15
    ///     }
    ///     
    /// This endpoint requires authentication. Include the authentication cookie in the request.
    /// The reservation will automatically expire after the TTL (default 15 minutes).
    /// </remarks>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> Reserve([FromBody] ReserveRequest request)
    {
        _logger.LogInformation("Reserve request received for ProductId: {ProductId}, Quantity: {Quantity}",
            request.ProductId, request.Quantity);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Reserve request validation failed for ProductId: {ProductId}", request.ProductId);
            return BadRequest(ModelState);
        }

        var result = await _reservationService.ReserveAsync(request);

        return result.Match(
            reservation =>
            {
                _logger.LogInformation("Reservation created successfully: {ReservationId}", reservation.ReservationId);
                return CreatedAtAction(
                    nameof(GetReservation),
                    new { id = reservation.ReservationId },
                    reservation);
            },
            error =>
            {
                _logger.LogWarning("Reserve failed for ProductId: {ProductId}, Error: {Error}",
                    request.ProductId, error.Message);
                return error.Type == ErrorType.NotFound
                    ? NotFound(new { message = error.Message })
                    : error.ToProblemDetails();
            });
    }

    /// <summary>
    /// Retrieves a specific reservation by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the reservation.</param>
    /// <returns>The reservation details.</returns>
    /// <response code="200">Returns the requested reservation.</response>
    /// <response code="400">Invalid reservation ID format.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Reservation not found.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/reservations/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     
    /// This endpoint requires authentication.
    /// </remarks>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> GetReservation(string id)
    {
        _logger.LogDebug("GetReservation request for ReservationId: {ReservationId}", id);

        var result = await _reservationService.GetByIdAsync(id);

        return result.Match(
            reservation =>
            {
                _logger.LogDebug("Reservation retrieved: {ReservationId}", id);
                return Ok(reservation);
            },
            error =>
            {
                _logger.LogWarning("GetReservation failed for ReservationId: {ReservationId}, Error: {Error}",
                    id, error.Message);
                return error.Type == ErrorType.NotFound
                    ? NotFound(new { message = error.Message })
                    : error.ToProblemDetails();
            });
    }

    /// <summary>
    /// Checks out a reservation, completing the order.
    /// </summary>
    /// <param name="request">The checkout request containing the reservationId.</param>
    /// <returns>The updated reservation details.</returns>
    /// <response code="200">Checkout completed successfully.</response>
    /// <response code="400">Invalid request - reservation expired or invalid status.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Reservation not found.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/reservations/checkout
    ///     {
    ///        "reservationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    ///     }
    ///     
    /// This endpoint requires authentication.
    /// The reservation must be active and not expired to complete checkout.
    /// </remarks>
    [Authorize]
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> Checkout([FromBody] CheckoutRequest request)
    {
        _logger.LogInformation("Checkout request received for ReservationId: {ReservationId}", request.ReservationId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Checkout request validation failed for ReservationId: {ReservationId}",
                request.ReservationId);
            return BadRequest(ModelState);
        }

        var result = await _reservationService.CheckoutAsync(request);

        return result.Match(
            reservation =>
            {
                _logger.LogInformation("Checkout completed successfully for ReservationId: {ReservationId}",
                    request.ReservationId);
                return Ok(reservation);
            },
            error =>
            {
                _logger.LogWarning("Checkout failed for ReservationId: {ReservationId}, Error: {Error}",
                    request.ReservationId, error.Message);
                return error.Type == ErrorType.NotFound
                    ? NotFound(new { message = error.Message })
                    : error.ToProblemDetails();
            });
    }

    /// <summary>
    /// Cancels an active reservation and returns stock to inventory.
    /// </summary>
    /// <param name="id">The unique identifier of the reservation to cancel.</param>
    /// <returns>The cancelled reservation details.</returns>
    /// <response code="200">Reservation cancelled successfully.</response>
    /// <response code="400">Invalid request - reservation already completed or expired.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Reservation not found.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     DELETE /api/reservations/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     
    /// This endpoint requires authentication.
    /// Only active reservations can be cancelled.
    /// </remarks>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> Cancel(string id)
    {
        _logger.LogInformation("Cancel request received for ReservationId: {ReservationId}", id);

        var result = await _reservationService.CancelAsync(id);

        return result.Match(
            reservation =>
            {
                _logger.LogInformation("Reservation cancelled successfully: {ReservationId}", id);
                return Ok(reservation);
            },
            error =>
            {
                _logger.LogWarning("Cancel failed for ReservationId: {ReservationId}, Error: {Error}",
                    id, error.Message);
                return error.Type == ErrorType.NotFound
                    ? NotFound(new { message = error.Message })
                    : error.ToProblemDetails();
            });
    }
}
