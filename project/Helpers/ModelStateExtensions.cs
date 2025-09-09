using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace project.Helpers
{
    public static class ModelStateExtensions
    {
        public static string GetErrorMessages(this ModelStateDictionary modelState)
        {
            var errors = modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(error => !string.IsNullOrEmpty(error))
                .ToList();

            if (!errors.Any())
            {
                return "Por favor, verifique los datos ingresados.";
            }

            // Formatear con saltos de línea para mejor visualización
            return "Errores de validación:<br>• " + string.Join("<br>• ", errors);
        }
    }
}
