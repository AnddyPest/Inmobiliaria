using Microsoft.AspNetCore.Mvc;

namespace project.Helpers
{
    public static class SweetAlertHelper
    {
        public static void SetSweetAlert(this Controller controller, string title, string message, string type)
        {
            controller.TempData["SwalTitle"] = title;
            controller.TempData["SwalMessage"] = message;
            controller.TempData["SwalType"] = type;
        }

        public static void SweetAlertSuccess(this Controller controller, string message, string title = "¡Éxito!")
        {
            controller.SetSweetAlert(title, message, "success");
        }

        public static void SweetAlertError(this Controller controller, string message, string title = "Error")
        {
            controller.SetSweetAlert(title, message, "error");
        }

        public static void SweetAlertWarning(this Controller controller, string message, string title = "Advertencia")
        {
            controller.SetSweetAlert(title, message, "warning");
        }

        public static void SweetAlertInfo(this Controller controller, string message, string title = "Información")
        {
            controller.SetSweetAlert(title, message, "info");
        }

        public static void SweetAlertQuestion(this Controller controller, string message, string title = "Confirmación")
        {
            controller.SetSweetAlert(title, message, "question");
        }
        // MÉTODOS REDIRECT PARA MISMO CONTROLADOR
        public static IActionResult RedirectToActionWithSuccess(this Controller controller,
            string actionName, string message, string title = "¡Éxito!")
        {
            controller.SweetAlertSuccess(message, title);
            return controller.RedirectToAction(actionName);
        }

        public static IActionResult RedirectToActionWithError(this Controller controller,
            string actionName, string message, string title = "Error")
        {
            controller.SweetAlertError(message, title);
            return controller.RedirectToAction(actionName);
        }

        public static IActionResult RedirectToActionWithWarning(this Controller controller,
            string actionName, string message, string title = "Advertencia")
        {
            controller.SweetAlertWarning(message, title);
            return controller.RedirectToAction(actionName);
        }

        // MÉTODOS REDIRECT PARA OTRO CONTROLADOR
        public static IActionResult RedirectToActionWithSuccess(this Controller controller,
            string actionName, string controllerName, string message, string title = "¡Éxito!")
        {
            controller.SweetAlertSuccess(message, title);
            return controller.RedirectToAction(actionName, controllerName);
        }

        public static IActionResult RedirectToActionWithError(this Controller controller,
            string actionName, string controllerName, string message, string title = "Error")
        {
            controller.SweetAlertError(message, title);
            return controller.RedirectToAction(actionName, controllerName);
        }

        public static IActionResult RedirectToActionWithWarning(this Controller controller,
            string actionName, string controllerName, string message, string title = "Advertencia")
        {
            controller.SweetAlertWarning(message, title);
            return controller.RedirectToAction(actionName, controllerName);
        }

        // VERSIÓN CON PARÁMETROS (opcional)
        public static IActionResult RedirectToActionWithError(this Controller controller,
            string actionName, string message, object routeValues, string title = "Error")
        {
            controller.SweetAlertError(message, title);
            return controller.RedirectToAction(actionName, routeValues);
        }

        public static IActionResult RedirectToActionWithError(this Controller controller,
            string actionName, string controllerName, string message, object routeValues, string title = "Error")
        {
            controller.SweetAlertError(message, title);
            return controller.RedirectToAction(actionName, controllerName, routeValues);
        }
    }
}
