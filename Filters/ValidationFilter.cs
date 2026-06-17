using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerceApp.Filters
{
    /// <summary>
    /// Global action filter that short-circuits POST requests with invalid ModelState,
    /// returning the same view with the model so validation errors display immediately.
    /// </summary>
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Only intercept POST requests with invalid model state
            if (!context.HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                return;

            if (context.ModelState.IsValid)
                return;

            // Return the view with the first action argument (the view model) so
            // validation error messages render correctly in the form.
            var controller = context.Controller as Controller;
            if (controller == null) return;

            var model = context.ActionArguments.Values.FirstOrDefault();
            context.Result = controller.View(model);
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}