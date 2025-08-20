namespace project.Helpers
{
    public class HelperFor
    {
        public static void imprimirMensajeDeError(string mensaje, string clase, string metodo)
        {
            Console.WriteLine($"Error en la clase {clase}\nMetodo {metodo}\nMensaje:{mensaje}");
        }

    }
}
