using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Oclock.Filters
{

    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly int[] _allowedRoles;

        public AuthorizeRoleAttribute(params int[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            // Verificar si el usuario está autenticado
            var usuarioId = session.GetInt32("UsuarioId");

            if (!usuarioId.HasValue)
            {
                // No hay sesión activa, redirigir al login
                context.Result = new RedirectToActionResult("Index", "Usuario", null);
                return;
            }

            // Verificar el rol del usuario
            var usuarioRol = session.GetInt32("UsuarioRol");

            if (!usuarioRol.HasValue || !_allowedRoles.Contains(usuarioRol.Value))
            {
                // Redirigir a la vista correspondiente según su rol
                if (usuarioRol == 1) // Administrador
                {
                    context.Result = new RedirectToActionResult("AdminHome", "Home", null);
                }
                else if (usuarioRol == 2)
                {
                    context.Result = new RedirectToActionResult("ColaboradorHome", "Home", null);
                }
                else
                {
                    session.Clear();
                    context.Result = new RedirectToActionResult("Index", "Usuario", null);
                }
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}