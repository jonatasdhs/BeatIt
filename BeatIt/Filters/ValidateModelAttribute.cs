using BeatIt.Errors;
using BeatIt.Extensions;
using BeatIt.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BeatIt.Filters;

public class ValidateModelAttribute : ActionFilterAttribute
{
    private readonly ILogger<ValidateModelAttribute> _logger;

    public ValidateModelAttribute(ILogger<ValidateModelAttribute> logger)
    {
        _logger = logger;
    }

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            _logger.LogWarning(AppLogEvents.InvalidValidation,
                "Validation failed for {Action}. Errors: {Errors}",
                context.ActionDescriptor.DisplayName,
                string.Join("; ", errors));
        }
    }
}