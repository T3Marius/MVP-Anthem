using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static MVPAnthem.MVPAnthem;
using CounterStrikeSharp.API;
using System.Text;

namespace MVPAnthem;

public static class Events
{
    private static Timer? CenterHtmlTimer;
    private static Timer? CenterTimer;
    private static Timer? AlertTimer;

    private static bool g_IsCenterHtmlActive;
    private static bool g_IsCenterActive;
    private static bool g_IsAlertActive;

    private static string HtmlMessage = string.Empty;
    private static string CenterMessage = string.Empty;
    private static string AlertMessage = string.Empty;

    public static void Initialize()
    {
        Instance.RegisterListener<Listeners.OnServerPrecacheResources>(OnPrecache);
        Instance.RegisterListener<Listeners.OnTick>(OnTick);
        Instance.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        Instance.RegisterEventHandler<EventRoundMvp>(OnRoundMvp);
    }
    public static HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        if (player == null)
            return HookResult.Continue;

        Instance.AddTimer(3.0f, () =>
        {
            if (!Instance.playerVolumeCookies.TryGetValue(player, out string? currentVolume) && string.IsNullOrEmpty(currentVolume))
            {
                float defaultVolume = Instance.Config.Settings.DefaultVolume;

                if (Instance.CLIENT_PREFS_API != null && Instance.VolumeCookie != -1)
                {
                    Instance.CLIENT_PREFS_API.SetPlayerCookie(player, Instance.VolumeCookie, defaultVolume.ToString());
                    Instance.playerMVPCookies[player] = defaultVolume.ToString();
                }
            }
            if (Instance.Config.Settings.GiveRandomMVP)
            {
                if (!Instance.playerMVPCookies.TryGetValue(player, out string? randomMvp) || string.IsNullOrEmpty(randomMvp))
                {
                    var allMvps = Instance.Config.MVPSettings
                        .SelectMany(category => category.Value)
                        .ToList();

                    if (allMvps.Any())
                    {
                        var randomMvpEntry = allMvps[new Random().Next(allMvps.Count)];

                        string newMvpCookie = $"{randomMvpEntry.Value.MVPName};{randomMvpEntry.Value.MVPSound}";

                        if (Instance.CLIENT_PREFS_API != null && Instance.MVPCookie != -1)
                        {
                            Instance.CLIENT_PREFS_API.SetPlayerCookie(player, Instance.MVPCookie, newMvpCookie);
                            Instance.playerMVPCookies[player] = newMvpCookie;
                        }
                    }
                }
            }
        });

        return HookResult.Continue;
    }
    public static HookResult OnRoundMvp(EventRoundMvp @event, GameEventInfo info)
    {
        CCSPlayerController? mvpPlayer = @event.Userid;
        if (mvpPlayer == null)
            return HookResult.Continue;

        if (Instance.Config.Settings.DisablePlayerDefaultMVP)
        {
            mvpPlayer.MVPs = 0;
        }

        string mvpSound = "";
        string mvpName = "";
        string mvpKey = "";

        if (Instance.playerMVPCookies.TryGetValue(mvpPlayer, out string? mvpCookie) && !string.IsNullOrEmpty(mvpCookie))
        {
            string[] parts = mvpCookie.Split(';');
            if (parts.Length > 1)
            {
                mvpName = parts[0];
                mvpSound = parts[1];

                foreach (var category in Instance.Config.MVPSettings)
                {
                    foreach (var entry in category.Value)
                    {
                        if (entry.Value.MVPName == mvpName && entry.Value.MVPSound == mvpSound)
                        {
                            mvpKey = entry.Key;
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(mvpKey)) break;
                }
            }
        }
        if (!string.IsNullOrEmpty(mvpSound) && !string.IsNullOrEmpty(mvpKey))
        {
            MVP_Settings? mvpSettings = null;
            foreach (var category in Instance.Config.MVPSettings)
            {
                if (category.Value.TryGetValue(mvpKey, out var settings))
                {
                    mvpSettings = settings;
                    break;
                }
            }

            foreach (var p in Utilities.GetPlayers().Where(p => !p.IsBot || !p.IsHLTV))
            {
                float volume = Instance.Config.Settings.DefaultVolume;
                if (Instance.playerVolumeCookies.TryGetValue(p, out string? volumeStr) && !string.IsNullOrEmpty(volumeStr))
                {
                    float.TryParse(volumeStr, out volume);
                }

                if (volume > 0)
                {
                    RecipientFilter filter = [p];
                    Server.NextFrame(() => p.EmitSound(mvpSound, filter, volume));
                }

                if (mvpSettings != null)
                {
                    if (mvpSettings.ShowChatMessage)
                    {
                        string chatKey = $"{mvpKey}.chat";
                        string localizedMessage = Instance.Localizer["prefix"] + Instance.Localizer[chatKey];

                        if (!string.IsNullOrEmpty(localizedMessage))
                        {
                            p.PrintToChat(string.Format(localizedMessage, mvpPlayer.PlayerName, mvpSettings.MVPName));
                        }
                    }

                    if (mvpSettings.ShowCenterMessage)
                    {
                        string centerKey = $"{mvpKey}.center";
                        string localizedCenterMessage = Instance.Localizer[centerKey];

                        if (!string.IsNullOrEmpty(localizedCenterMessage))
                        {
                            CenterMessage = string.Format(localizedCenterMessage, mvpPlayer.PlayerName, mvpSettings.MVPName);
                            g_IsCenterActive = true;
                            CenterTimer = Instance.AddTimer(Instance.Config.Timer.CenterDuration, () =>
                            {
                                CenterTimer?.Kill();
                                g_IsCenterActive = false;
                                CenterTimer = null;
                            });
                        }
                    }
                    if (mvpSettings.ShowAlertMessage)
                    {
                        string alertKey = $"{mvpKey}.alert";
                        string localizedMessage = Instance.Localizer[alertKey];

                        if (!string.IsNullOrEmpty(localizedMessage))
                        {
                            AlertMessage = string.Format(localizedMessage, mvpPlayer.PlayerName, mvpSettings.MVPName);
                            g_IsAlertActive = true;
                            AlertTimer = Instance.AddTimer(Instance.Config.Timer.AlertDuration, () =>
                            {
                                AlertTimer?.Kill();
                                g_IsAlertActive = false;
                                AlertTimer = null;
                            });
                        }
                    }
                    if (mvpSettings.ShowHtmlMessage)
                    {
                        string htmlKey = $"{mvpKey}.html";
                        string localizedMessage = Instance.Localizer[htmlKey];

                        if (!string.IsNullOrEmpty(localizedMessage))
                        {
                            HtmlMessage = string.Format(localizedMessage, mvpPlayer.PlayerName, mvpSettings.MVPName);
                            g_IsCenterHtmlActive = true;
                            CenterHtmlTimer = Instance.AddTimer(Instance.Config.Timer.CenterHtmlDuration, () =>
                            {
                                CenterHtmlTimer?.Kill();
                                g_IsCenterHtmlActive = false;
                                CenterHtmlTimer = null;
                            });
                        }
                    }
                }
            }
        }

        return HookResult.Continue;
    }
    public static void OnTick()
    {
        if (CenterHtmlTimer != null && g_IsCenterHtmlActive)
        {
            foreach (var p in Utilities.GetPlayers())
            {
                StringBuilder htmlBuilder = new StringBuilder();
                htmlBuilder.AppendLine(HtmlMessage);
                htmlBuilder.AppendLine("</div>");

                string htmlMessage = htmlBuilder.ToString();
                p.PrintToCenterHtml(htmlMessage);
            }
        }
        if (CenterTimer != null && g_IsCenterActive)
        {
            foreach (var p in Utilities.GetPlayers())
            {

                StringBuilder centerBuilder = new StringBuilder();
                centerBuilder.Append(CenterMessage);
                string centerMessage = centerBuilder.ToString();
                p.PrintToCenter(centerMessage);
            }
        }
        if (AlertTimer != null && g_IsAlertActive)
        {
            foreach (var p in Utilities.GetPlayers())
            {
                StringBuilder alertBuilder = new StringBuilder();
                alertBuilder.Append(AlertMessage);
                string alertMessage = alertBuilder.ToString();
                p.PrintToCenter(alertMessage);
            }
        }
    }
    public static void Dispose()
    {
        Instance.RemoveListener<Listeners.OnTick>(OnTick);
        Instance.RemoveListener<Listeners.OnServerPrecacheResources>(OnPrecache);
    }
    public static void OnPrecache(ResourceManifest resource)
    {
        foreach (var file in Instance.Config.Settings.SoundEventFiles)
        {
            resource.AddResource(file);
        }
    }
}