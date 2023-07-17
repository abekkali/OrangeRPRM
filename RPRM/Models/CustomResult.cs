using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace RPRM.Models
{
    public class CustomResult : ViewResult
    {
        public string Message { get; set; }

        public CustomResult(string message )
        {
            Message = message;
            ViewName = "~/Views/Home/PageInformation.cshtml";
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.HttpContext.Response.ContentType = "text/html";
            ViewData["ErrorAccessMessage"] = Message; // Passez le message d'erreur à la vue
            return base.ExecuteResultAsync(context);
        }
    }
}
