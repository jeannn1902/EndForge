namespace EndForge.App;

public class AppState
{
    /// <summary>
    /// True si la aplicación detectó conexión con la base en la nube (Azure SQL), false si está en fallback local.
    /// </summary>
    public bool IsCloudConnected { get; set; }
}
