

using BCrypt.Net;
public class AuthHelper
{
    public static string HashContraseña(string contrasena)
    {
        
        return BCrypt.Net.BCrypt.HashPassword(contrasena);
    }
    public static bool VerificarContrasena(string contrasena, string hashContrasena)
    {
        return BCrypt.Net.BCrypt.Verify(contrasena, hashContrasena);
    }

}