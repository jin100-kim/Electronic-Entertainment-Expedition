using Unity.Netcode;

public static class NetworkSession
{
    public static bool IsActive => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    public static bool IsServer => NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
    public static bool IsClient => NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient;
}
