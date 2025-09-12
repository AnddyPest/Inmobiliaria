namespace project.Helpers
{
    public class HelperFor
    {
        public static void imprimirMensajeDeError(string mensaje, string clase, string metodo)
        {
            Console.WriteLine($"Error en la clase {clase}\nMetodo {metodo}\nMensaje:{mensaje}");
        }
        public static string construirSqlWhereAnd(List<string> parametros)
        {
            if(parametros.Count == 0) return "";
            string sql = "WHERE ";
            sql += string.Join("\n AND ", parametros);
            return sql;
        }

    }
}
