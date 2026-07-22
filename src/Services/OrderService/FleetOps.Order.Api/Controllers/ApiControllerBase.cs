using FleetOps.Order.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FleetOps.Order.Api.Controllers
{
  
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected ActionResult HandleResult(Result result)
           => result.IsSuccess? NoContent(): HandleProblem(result.Errors);
       

        protected ActionResult<TValue> HandleResult<TValue>(Result<TValue> result)
          =>result.IsSuccess?Ok(result.Value):HandleProblem(result.Errors);



        private ActionResult HandleProblem(IReadOnlyList<Error> errors)
        {
            if (errors.Count == 0) return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.");

            if (errors.All(e => e.Type == ErrorType.Validation))
                return HandleValidationErrors(errors);

            return HandleSingleError(errors[0]);
        }



        private ActionResult HandleSingleError(Error error)
        {
            return Problem(title: error.Code, detail: error.Description, statusCode: MapErrorTypeIntoStatusCode(error.Type));
        }

        private ActionResult HandleValidationErrors(IReadOnlyList<Error> errors)
        {
            var modelState = new ModelStateDictionary();
            foreach (var error in errors)
            {
                modelState.AddModelError(error.Code, error.Description);

            }
            return ValidationProblem(modelState);
        }
        private static int MapErrorTypeIntoStatusCode(ErrorType errorType) => errorType switch
        {

            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.InvalidCredentials => StatusCodes.Status401Unauthorized,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError

        };


    }
}
