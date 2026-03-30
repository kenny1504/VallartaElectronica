using System.Security.Cryptography;

namespace ElectronicaVallarta.Servicios;

public static class ServicioHashClave
{
    private const int Iteraciones = 100_000;
    private const int TamanioSalt = 16;
    private const int TamanioClave = 32;

    public static string GenerarHash(string clave)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanioSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(clave, salt, Iteraciones, HashAlgorithmName.SHA256, TamanioClave);

        return $"{Iteraciones}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool VerificarHash(string clave, string hashPersistido)
    {
        var partes = hashPersistido.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length != 3 || !int.TryParse(partes[0], out var iteraciones))
        {
            return false;
        }

        var salt = Convert.FromBase64String(partes[1]);
        var hashEsperado = Convert.FromBase64String(partes[2]);
        var hashActual = Rfc2898DeriveBytes.Pbkdf2(clave, salt, iteraciones, HashAlgorithmName.SHA256, hashEsperado.Length);
        return CryptographicOperations.FixedTimeEquals(hashActual, hashEsperado);
    }
}
