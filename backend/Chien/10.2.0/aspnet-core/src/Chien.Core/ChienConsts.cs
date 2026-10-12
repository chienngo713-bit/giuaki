using Chien.Debugging;

namespace Chien;

public class ChienConsts
{
    public const string LocalizationSourceName = "Chien";

    public const string ConnectionStringName = "Default";

    public const bool MultiTenancyEnabled = true;


    /// <summary>
    /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
    /// </summary>
    public static readonly string DefaultPassPhrase =
        DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "d6df54c626fb4af8a33669ed8f8a00ab";
}
