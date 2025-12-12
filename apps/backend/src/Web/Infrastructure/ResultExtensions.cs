using backend.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace backend.Web.Infrastructure;

/// <summary>
/// Các phương thức mở rộng để chuyển đổi đối tượng Result sang IActionResult.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Chuyển đổi một đối tượng Result (không có giá trị) thành IActionResult.
    /// </summary>
    /// <param name="result">Đối tượng Result.</param>
    /// <param name="controller">Controller hiện tại.</param>
    /// <returns>IActionResult tương ứng.</returns>
    public static IActionResult ToActionResult(
        this Result result,
        ControllerBase controller,
        int successStatusCode = 204, // Default to 204 No Content for successful non-value operations
        string? createdAtActionName = null, // These parameters are typically not used for non-generic Result, but included for signature consistency
        object? createdAtRouteValues = null)
    {
        if (result.IsSuccess)
        {
            return successStatusCode switch
            {
                200 => controller.Ok(), // Ok with no content
                204 => controller.NoContent(),
                _ => controller.StatusCode(successStatusCode)
            };
        }

        return HandleFailure(result, controller);
    }

    /// <summary>
    /// Chuyển đổi một đối tượng Result<T> (có giá trị) thành IActionResult.
    /// </summary>
    /// <typeparam name="T">Kiểu của giá trị trong Result.</typeparam>
    /// <param name="result">Đối tượng Result<T>.</param>
    /// <param name="controller">Controller hiện tại.</param>
    /// <param name="successStatusCode">Mã trạng thái HTTP khi thành công (mặc định là 200 OK).</param>
    /// <param name="createdAtActionName">Tên hành động cho CreatedAtAction nếu successStatusCode là 201.</param>
    /// <param name="createdAtRouteValues">Giá trị định tuyến cho CreatedAtAction nếu successStatusCode là 201.</param>
    /// <returns>IActionResult tương ứng.</returns>
    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        ControllerBase controller,
        int successStatusCode = 200,
        string? createdAtActionName = null,
        object? createdAtRouteValues = null)
    {
        if (result.IsSuccess)
        {
            return successStatusCode switch
            {
                200 => controller.Ok(result.Value),
                201 when createdAtActionName != null && createdAtRouteValues != null => controller.CreatedAtAction(createdAtActionName, createdAtRouteValues, result.Value),
                201 => controller.StatusCode(201, result.Value), // Fallback if CreatedAtAction parameters are missing
                204 => controller.NoContent(),
                _ => controller.StatusCode(successStatusCode, result.Value)
            };
        }

        return HandleFailure(result, controller);
    }

    private static IActionResult HandleFailure(Result result, ControllerBase controller)
    {
        if (result.ErrorSource == "Validation" && result.ValidationErrors != null && result.ValidationErrors.Any())
        {
            return CreateValidationProblemDetails(result, controller);
        }

        // Use controller.StatusCode with ProblemDetails for more consistent error responses
        return result.StatusCode switch
        {
            400 => controller.BadRequest(new ProblemDetails { Title = "Bad Request", Detail = result.Error, Status = 400 }),
            403 => controller.StatusCode(403, new ProblemDetails { Title = "Forbidden", Detail = result.Error, Status = 403 }),
            404 => controller.NotFound(new ProblemDetails { Title = "Not Found", Detail = result.Error, Status = 404 }),
            409 => controller.Conflict(new ProblemDetails { Title = "Conflict", Detail = result.Error, Status = 409 }),
            _ => controller.StatusCode(result.StatusCode, new ProblemDetails { Title = "Error", Detail = result.Error, Status = result.StatusCode })
        };
    }

    private static IActionResult HandleFailure<TValue>(Result<TValue> result, ControllerBase controller)
    {
        if (result.ErrorSource == "Validation" && result.ValidationErrors != null && result.ValidationErrors.Any())
        {
            return CreateValidationProblemDetails(result, controller);
        }

        // Use controller.StatusCode with ProblemDetails for more consistent error responses
        return result.StatusCode switch
        {
            400 => controller.BadRequest(new ProblemDetails { Title = "Bad Request", Detail = result.Error, Status = 400 }),
            403 => controller.StatusCode(403, new ProblemDetails { Title = "Forbidden", Detail = result.Error, Status = 403 }),
            404 => controller.NotFound(new ProblemDetails { Title = "Not Found", Detail = result.Error, Status = 404 }),
            409 => controller.Conflict(new ProblemDetails { Title = "Conflict", Detail = result.Error, Status = 409 }),
            _ => controller.StatusCode(result.StatusCode, new ProblemDetails { Title = "Error", Detail = result.Error, Status = result.StatusCode })
        };
    }

    private static IActionResult CreateValidationProblemDetails(Result result, ControllerBase controller)
    {
        var modelStateDictionary = new ModelStateDictionary();
        foreach (var error in result.ValidationErrors!)
        {
            foreach (var message in error.Value)
            {
                modelStateDictionary.AddModelError(error.Key, message);
            }
        }

        return controller.ValidationProblem(modelStateDictionary);
    }

    private static IActionResult CreateValidationProblemDetails<TValue>(Result<TValue> result, ControllerBase controller)
    {
        var modelStateDictionary = new ModelStateDictionary();
        foreach (var error in result.ValidationErrors!)
        {
            foreach (var message in error.Value)
            {
                modelStateDictionary.AddModelError(error.Key, message);
            }
        }

        return controller.ValidationProblem(modelStateDictionary);
    }
}
