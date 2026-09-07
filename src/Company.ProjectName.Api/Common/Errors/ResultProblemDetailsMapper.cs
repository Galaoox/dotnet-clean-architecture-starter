using Company.ProjectName.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Company.ProjectName.Api.Common.Errors;

public static class ResultProblemDetailsMapper
{
    public static ActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return controller.Problem(
            statusCode: result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError,
            },
            title: result.Error.Type.ToString(),
            detail: result.Error.Description,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = result.Error.Code,
            });
    }

    public static ActionResult ToActionResult(this ControllerBase controller, Result result)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        return controller.Problem(
            statusCode: result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError,
            },
            title: result.Error.Type.ToString(),
            detail: result.Error.Description,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = result.Error.Code,
            });
    }
}
