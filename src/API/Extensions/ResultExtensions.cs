using Barbearia.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Barbearia.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToErrorActionResult(this Result result, ControllerBase controller)
    {
        var problema = new { codigo = result.Error.Code, mensagem = result.Error.Message };

        return result.Error.Type switch
        {
            ErrorType.NotFound => controller.NotFound(problema),
            ErrorType.Validation => controller.BadRequest(problema),
            ErrorType.Conflict => controller.Conflict(problema),
            _ => controller.Problem(result.Error.Message)
        };
    }
}
