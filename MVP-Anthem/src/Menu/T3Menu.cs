using static MVPAnthem.MVPAnthem;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Translations;
using static MVPAnthem.Lib;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Extensions.Logging;
using MVPAnthem;

public static class T3Menu
{
    public static void DisplayMVP(CCSPlayerController player)
    {
        if (player == null)
            return;

        var manager = Instance.GetMenuManager();
        if (manager == null) return;

        var mainMenu = manager.CreateMenu(Instance.Localizer.ForPlayer(player, "mvp<mainmenu>"), isSubMenu: false);

        if (Instance.playerMVPCookies.TryGetValue(player, out string? mvpCookie) && !string.IsNullOrEmpty(mvpCookie))
        {
            string[] parts = mvpCookie.Split(';');
            string displayMVP = parts.Length > 0 ? parts[0] : mvpCookie;
            mainMenu.AddTextOption(Instance.Localizer.ForPlayer(player, "mvp<currentmvp>", displayMVP));
        }

        if (Instance.playerVolumeCookies.TryGetValue(player, out string? currentVolume) && !string.IsNullOrEmpty(currentVolume))
        {
            string volumeLabel = currentVolume;
            if (float.TryParse(currentVolume, out float volumeValue))
            {
                int volumePercentage = (int)(volumeValue * 100);

                int closestValue = Instance.Config.Settings.VolumeSettings
                    .OrderBy(v => Math.Abs(v - volumePercentage))
                    .FirstOrDefault();

                volumeLabel = closestValue + "%";
            }
            mainMenu.AddTextOption(Instance.Localizer.ForPlayer(player, "mvp<currentvolume>", volumeLabel));
        }

        if (Instance.playerMVPCookies.TryGetValue(player, out string? activeMvp) && !string.IsNullOrEmpty(activeMvp))
        {
            mainMenu.AddOption(Instance.Localizer.ForPlayer(player, "mvp<remove>"), (p, option) =>
            {
                var confirmMenu = manager.CreateMenu(Instance.Localizer.ForPlayer(p, "mvp<remove.confirm>"), isSubMenu: true);
                confirmMenu.ParentMenu = mainMenu;

                confirmMenu.AddOption(Instance.Localizer.ForPlayer(p, "remove<yes>"), (p, option) =>
                {
                    if (Instance.CLIENT_PREFS_API != null && Instance.MVPCookie != -1)
                    {
                        Instance.CLIENT_PREFS_API.SetPlayerCookie(p, Instance.MVPCookie, string.Empty);
                        if (Instance.playerMVPCookies.ContainsKey(p))
                        {
                            Instance.playerMVPCookies.Remove(p);
                        }
                        p.PrintToChat(Instance.Localizer["prefix"] + Instance.Localizer["mvp.removed"]);
                        manager.CloseMenu(p);
                    }
                });

                confirmMenu.AddOption(Instance.Localizer.ForPlayer(p, "remove<no>"), (p, option) =>
                {
                    manager.CloseMenu(p);
                });

                manager.OpenSubMenu(player, confirmMenu);
            });
        }
        mainMenu.AddOption(Instance.Localizer.ForPlayer(player, "volume<option>"), (p, option) =>
        {
            var volumeMenu = manager.CreateMenu(Instance.Localizer.ForPlayer(p, "volume<menu>"), isSubMenu: true);
            volumeMenu.ParentMenu = mainMenu;
            List<object> volumeValues = Instance.Config.Settings.VolumeSettings.Cast<object>().ToList();

            object defaultVolume = volumeValues.FirstOrDefault() ?? 100;

            if (Instance.CLIENT_PREFS_API != null && Instance.VolumeCookie != -1 &&
                Instance.playerVolumeCookies.TryGetValue(p, out string? savedVolume))
            {
                if (float.TryParse(savedVolume, out float savedVolumeValue))
                {
                    int savedPercentage = (int)(savedVolumeValue * 100);

                    int closestValue = Instance.Config.Settings.VolumeSettings
                        .OrderBy(v => Math.Abs(v - savedPercentage))
                        .FirstOrDefault();

                    defaultVolume = closestValue;
                }
            }

            volumeMenu.AddSliderOption(Instance.Localizer.ForPlayer(p, "slider<volume>"), volumeValues, defaultVolume, 3, (p, o, index) =>
            {
                if (o is IT3Option sliderOption && sliderOption.DefaultValue != null)
                {
                    int volumePercentage = Convert.ToInt32(sliderOption.DefaultValue);
                    float volumeDecimal = volumePercentage / 100.0f;

                    if (Instance.CLIENT_PREFS_API != null && Instance.VolumeCookie != -1)
                    {
                        Instance.CLIENT_PREFS_API.SetPlayerCookie(p, Instance.VolumeCookie, volumeDecimal.ToString());
                        Instance.playerVolumeCookies[p] = volumeDecimal.ToString();
                        p.PrintToChat(Instance.Localizer["prefix"] + Instance.Localizer["volume.selected", volumePercentage + "%"]);
                    }
                }
            });

            manager.OpenSubMenu(p, volumeMenu);
        });

        mainMenu.AddOption(Instance.Localizer.ForPlayer(player, "mvp<option>"), (p, o) =>
        {
            var categoryMenu = manager.CreateMenu(Instance.Localizer.ForPlayer(p, "categories<menu>"), isSubMenu: true);
            categoryMenu.ParentMenu = mainMenu;

            Dictionary<string, List<KeyValuePair<string, MVP_Settings>>> accessibleMVPsByCategory = new();

            foreach (var category in Instance.Config.MVPSettings)
            {
                var accessibleMVPs = new List<KeyValuePair<string, MVP_Settings>>();

                foreach (var mvpEntry in category.Value)
                {
                    if (ValidatePlayerForMVP(player, mvpEntry.Value))
                    {
                        accessibleMVPs.Add(mvpEntry);
                    }
                }

                if (accessibleMVPs.Count > 0)
                {
                    accessibleMVPsByCategory[category.Key] = accessibleMVPs;
                }
            }

            foreach (var categoryEntry in accessibleMVPsByCategory)
            {
                string categoryName = categoryEntry.Key;
                var accessibleMVPs = categoryEntry.Value;

                categoryMenu.AddOption(categoryName, (categoryPlayer, categoryOption) =>
                {
                    var mvpsMenu = manager.CreateMenu(categoryName, isSubMenu: true);
                    mvpsMenu.ParentMenu = categoryMenu;

                    foreach (var mvpEntry in accessibleMVPs)
                    {
                        var mvpSettings = mvpEntry.Value;
                        mvpsMenu.AddOption(mvpSettings.MVPName, (mvpPlayer, mvpOption) =>
                        {
                            var mvpActionMenu = manager.CreateMenu(Instance.Localizer.ForPlayer(mvpPlayer, "mvp<equip>", mvpSettings.MVPName), isSubMenu: true);
                            mvpActionMenu.ParentMenu = mvpsMenu;

                            mvpActionMenu.AddOption(Instance.Localizer.ForPlayer(mvpPlayer, "equip<yes>"), (actionPlayer, actionOption) =>
                            {
                                string newValue = $"{mvpSettings.MVPName};{mvpSettings.MVPSound}";
                                if (Instance.CLIENT_PREFS_API != null && Instance.MVPCookie != -1)
                                {
                                    Instance.CLIENT_PREFS_API.SetPlayerCookie(actionPlayer, Instance.MVPCookie, newValue);
                                    Instance.playerMVPCookies[actionPlayer] = newValue;
                                    actionPlayer.PrintToChat(Instance.Localizer["prefix"] + Instance.Localizer["mvp.equipped", mvpSettings.MVPName]);
                                }
                            });

                            if (mvpSettings.EnablePreview)
                            {
                                mvpActionMenu.AddOption(Instance.Localizer.ForPlayer(mvpPlayer, "preview<option>"), (actionPlayer, actionOption) =>
                                {
                                    float volume = 0;
                                    if (Instance.playerVolumeCookies.TryGetValue(actionPlayer, out string? volumeStr) && !string.IsNullOrEmpty(volumeStr))
                                    {
                                        float.TryParse(volumeStr, out volume);
                                    }
                                    if (volume > 0)
                                    {
                                        RecipientFilter filter = [actionPlayer];
                                        actionPlayer.EmitSound(mvpSettings.MVPSound, filter, volume);
                                        actionPlayer.PrintToChat(Instance.Localizer["prefix"] + Instance.Localizer["mvp.previewed", mvpSettings.MVPName]);
                                    }
                                    else
                                    {
                                        actionPlayer.PrintToChat(Instance.Localizer["prefix"] + Instance.Localizer["preview.no.volume"]);
                                    }
                                });
                            }

                            manager.OpenSubMenu(player, mvpActionMenu);
                        });
                    }

                    manager.OpenSubMenu(player, mvpsMenu);
                });
            }

            manager.OpenSubMenu(player, categoryMenu);
        });
        manager.OpenMainMenu(player, mainMenu);
    }
    public static void DisplayVolume(CCSPlayerController player)
    {
        if (player == null)
            return;

        var manager = Instance.GetMenuManager();
        if (manager == null) return;

        var volumeMenu = manager.CreateMenu(Instance.Localizer.ForPlayer(player, "volume<menu>"));

        List<object> volumeValues = Instance.Config.Settings.VolumeSettings.Cast<object>().ToList();
        object defaultVolume = volumeValues.FirstOrDefault() ?? 100;

        if (Instance.CLIENT_PREFS_API != null && Instance.VolumeCookie != -1 &&
            Instance.playerVolumeCookies.TryGetValue(player, out string? savedVolume))
        {
            if (float.TryParse(savedVolume, out float savedVolumeValue))
            {
                int savedPercentage = (int)(savedVolumeValue * 100);

                int closestValue = Instance.Config.Settings.VolumeSettings
                    .OrderBy(v => Math.Abs(v - savedPercentage))
                    .FirstOrDefault();

                defaultVolume = closestValue;
            }
        }

        volumeMenu.AddSliderOption(Instance.Localizer.ForPlayer(player, "slider<volume>"), volumeValues, defaultVolume, 3, (p, o, index) =>
        {
            if (o is IT3Option sliderOption && sliderOption.DefaultValue != null)
            {
                int volumePercentage = Convert.ToInt32(sliderOption.DefaultValue);
                float volumeDecimal = volumePercentage / 100.0f;

                if (Instance.CLIENT_PREFS_API != null && Instance.VolumeCookie != -1)
                {
                    Instance.CLIENT_PREFS_API.SetPlayerCookie(p, Instance.VolumeCookie, volumeDecimal.ToString());
                    Instance.playerVolumeCookies[p] = volumeDecimal.ToString();
                    p.PrintToChat(Instance.Localizer["prefix"] + Instance.Localizer["volume.selected", volumePercentage + "%"]);
                }
            }
        });
        manager.OpenMainMenu(player, volumeMenu);
    }
}