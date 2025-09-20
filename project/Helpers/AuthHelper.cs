

using BCrypt.Net;
public class AuthHelper
{
    public static string HashContraseña(string contrasena)
    {
        BCrypt.Net.BCrypt.HashPassword(contrasena);
        return contrasena;
    }
    public static bool VerificarContrasena(string contrasena, string hashContrasena)
    {
        return BCrypt.Net.BCrypt.Verify(contrasena, hashContrasena);
    }

}