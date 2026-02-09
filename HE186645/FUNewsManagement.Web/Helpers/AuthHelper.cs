using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FUNewsManagement.DataAccess.Models;

namespace FUNewsManagement.WebApp.Helpers
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var account = session.GetObjectFromJson<SystemAccount>("Account");

            if (account == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_roles.Length > 0)
            {
                bool hasAccess = false;

                foreach (var role in _roles)
                {
                    if (role == "Admin")
                    {
                        var adminEmail = context.HttpContext.RequestServices
                            .GetService<IConfiguration>()?
                            .GetValue<string>("AdminAccount:Email");

                        if (account.AccountEmail == adminEmail)
                        {
                            hasAccess = true;
                            break;
                        }
                    }
                    else if (role == "Staff" && account.AccountRole == 1)
                    {
                        hasAccess = true;
                        break;
                    }
                    else if (role == "Lecturer" && account.AccountRole == 2)
                    {
                        hasAccess = true;
                        break;
                    }
                }

                if (!hasAccess)
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}