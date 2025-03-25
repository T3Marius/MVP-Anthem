using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using Clientprefs.API;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API;
namespace MVPAnthem;

public partial class MVPAnthem
{
    public int MVPCookie = -1;
    public int VolumeCookie = -1;

    public Dictionary<CCSPlayerController, string> playerMVPCookies { get; set; } = new();
    public Dictionary<CCSPlayerController, string> playerVolumeCookies { get; set; } = new();

    public readonly PluginCapability<IClientprefsApi> cookieCapabilty = new("Clientprefs");
    public IClientprefsApi? CLIENT_PREFS_API;

    void LoadClientPrefs()
    {
        try
        {
            CLIENT_PREFS_API = cookieCapabilty.Get() ?? throw new Exception("Clientprefs api not found");
            CLIENT_PREFS_API.OnDatabaseLoaded += OnClientprefDatabaseReady;
            CLIENT_PREFS_API.OnPlayerCookiesCached += OnPlayerCookiesCached;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load CleintprefsApi! | {ex.Message}");
        }
    }
    void UnloadClientprefis()
    {
        if (CLIENT_PREFS_API == null)
            return;

        CLIENT_PREFS_API.OnDatabaseLoaded -= OnClientprefDatabaseReady;
        CLIENT_PREFS_API.OnPlayerCookiesCached -= OnPlayerCookiesCached;
    }
    void ReloadClientprefs()
    {
        if (CLIENT_PREFS_API == null || MVPCookie == -1 || VolumeCookie == -1) return;

        foreach (CCSPlayerController player in Utilities.GetPlayers().Where(p => !p.IsBot))
        {
            if (!CLIENT_PREFS_API.ArePlayerCookiesCached(player))
                continue;


            string mvpValue = CLIENT_PREFS_API.GetPlayerCookie(player, MVPCookie);
            if (!string.IsNullOrEmpty(mvpValue))
                playerMVPCookies[player] = mvpValue;

            string volValue = CLIENT_PREFS_API.GetPlayerCookie(player, VolumeCookie);
            if (!string.IsNullOrEmpty(volValue))
                playerVolumeCookies[player] = volValue;
        }
    }
    void OnClientprefDatabaseReady()
    {
        if (CLIENT_PREFS_API == null) return;

        MVPCookie = CLIENT_PREFS_API.RegPlayerCookie("MVP", "Player selected MVP", CookieAccess.CookieAccess_Public);
        if (MVPCookie == -1)
        {
            Logger.LogError("Failed to register MVP cookie");
        }

        VolumeCookie = CLIENT_PREFS_API.RegPlayerCookie("Volume", "Player volume preference", CookieAccess.CookieAccess_Public);
        if (VolumeCookie == -1)
        {
            Logger.LogError("Failed to register Volume cookie");
        }
    }

    void OnPlayerCookiesCached(CCSPlayerController player)
    {
        if (CLIENT_PREFS_API == null || MVPCookie == -1 || VolumeCookie == -1) return;

        string mvpValue = CLIENT_PREFS_API.GetPlayerCookie(player, MVPCookie);
        if (!string.IsNullOrEmpty(mvpValue))
            playerMVPCookies[player] = mvpValue;

        string volValue = CLIENT_PREFS_API.GetPlayerCookie(player, VolumeCookie);
        if (!string.IsNullOrEmpty(volValue))
            playerVolumeCookies[player] = volValue;
    }
}