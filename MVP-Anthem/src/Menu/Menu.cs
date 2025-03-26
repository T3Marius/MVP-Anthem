using static MVPAnthem.MVPAnthem;
using static MVPAnthem.Lib;
using CounterStrikeSharp.API.Core;
using CS2ScreenMenuAPI.Internal;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using CS2ScreenMenuAPI;

namespace MVPAnthem;

public static class Menu
{
    public static void DisplayMVP(CCSPlayerController player)
    {
        if (player == null)
            return;

        ScreenMenu mainMenu = new ScreenMenu(Instance.Localizer.ForPlayer(player, "mvp<mainmenu>"), Instance);
        if (Instance.playerMVPCookies.TryGetValue(player, out string? mvpCookie) && !string.IsNullOrEmpty(mvpCookie))
        {
            string[] parts = mvpCookie.Split(';');
            string displayMVP = parts.Length > 0 ? parts[0] : mvpCookie;
            mainMenu.AddOption(Instance.Localizer.ForPlayer(player, "mvp<currentmvp>", displayMVP), (p, o) => { }, true);
        }
        if (Instance.playerVolumeCookies.TryGetValue(player, out string? currentVolume) && !string.IsNullOrEmpty(currentVolume))
        {
            string volumeLabel = currentVolume;
            if (float.TryParse(currentVolume, out float volumeValue))
            {
                foreach (var kvp in Instance.Config.Settings.VolumeSettings)
                {
                    if (Math.Abs(kvp.Value - volumeValue) < 0.01f)
                    {
                        volumeLabel = kvp.Key;
                        break;
                    }
                }
            }

            mainMenu.AddOption(Instance.Localizer.ForPlayer(player, "mvp<currentvolume>", volumeLabel), (p, o) => { }, true);
        }
        mainMenu.AddOption(" ", (p, o) => { }, true);

        if (Instance.playerMVPCookies.TryGetValue(player, out string? activeMvp) && !string.IsNullOrEmpty(activeMvp))
        {
            mainMenu.AddOption(Instance.Localizer.ForPlayer(player, "mvp<remove>"), (p, option) =>
            {
                ScreenMenu confirmMenu = new ScreenMenu(Instance.Localizer.ForPlayer(p, "mvp<remove.confirm>"), Instance)
                {
                    IsSubMenu = true,
                    ParentMenu = mainMenu,
                };

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
                        MenuAPI.CloseActiveMenu(p);
                    }
                });
                confirmMenu.AddOption(Instance.Localizer.ForPlayer(p, "remove<no>"), (p, option) =>
                {
                    MenuAPI.OpenMenu(Instance, p, mainMenu);
                });
                confirmMenu.Open(p);
            });
        }
        mainMenu.AddOption(Instance.Localizer.ForPlayer(player, "volume<option>"), (p, option) =>
        {
            ScreenMenu volumeMenu = new ScreenMenu(Instance.Localizer.ForPlayer(p, "volume<menu>"), Instance)
            {
                IsSubMenu = true,
                ParentMenu = mainMenu
            };

            foreach (var kvp in Instance.Config.Settings.VolumeSettings)
            {
                float volume = kvp.Value;
                string display = kvp.Key;

                volumeMenu.AddOption(display, (p, o) =>
                {
                    if (Instance.CLIENT_PREFS_API != null && Instance.VolumeCookie != -1)
                    {
                        Instance.CLIENT_PREFS_API.SetPlayerCookie(p, Instance.VolumeCookie, volume.ToString());
                        Instance.playerVolumeCookies[p] = volume.ToString();
                        p.PrintToChat(Instance.Localizer["prefix"] + Instance.Localizer["volume.selected", display]);
                    }
                });
            }
            volumeMenu.Open(player);
        });

        mainMenu.AddOption(Instance.Localizer.ForPlayer(player, "mvp<option>"), (p, o) =>
        {
            ScreenMenu categoryMenu = new ScreenMenu(Instance.Localizer.ForPlayer(p, "categories<menu>"), Instance)
            {
                IsSubMenu = true,
                ParentMenu = mainMenu
            };

            foreach (var category in Instance.Config.MVPSettings)
            {
                bool hasValidMVPs = false;

                foreach (var mvpEntry in category.Value)
                {
                    if (ValidatePlayerForMVP(player, mvpEntry.Value))
                    {
                        hasValidMVPs = true;
                        break;
                    }
                }

                if (hasValidMVPs)
                {
                    categoryMenu.AddOption(category.Key, (categoryPlayer, categoryOption) =>
                    {
                        ScreenMenu mvpsMenu = new ScreenMenu(category.Key, Instance)
                        {
                            IsSubMenu = true,
                            ParentMenu = categoryMenu
                        };

                        foreach (var mvpEntry in category.Value)
                        {
                            var mvpSettings = mvpEntry.Value;
                            if (ValidatePlayerForMVP(categoryPlayer, mvpSettings))
                            {
                                mvpsMenu.AddOption(mvpSettings.MVPName, (mvpPlayer, mvpOption) =>
                                {
                                    ScreenMenu mvpActionMenu = new ScreenMenu(Instance.Localizer.ForPlayer(mvpPlayer, "mvp<equip>", mvpSettings.MVPName), Instance)
                                    {
                                        IsSubMenu = true,
                                        ParentMenu = mvpsMenu,
                                    };

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

                                    mvpActionMenu.Open(mvpPlayer);
                                });
                            }
                        }

                        mvpsMenu.Open(categoryPlayer);
                    });
                }
            }

            categoryMenu.Open(player);
        });

        mainMenu.Open(player);
    }
    public static void DisplayVolume(CCSPlayerController player)
    {
        if (player == null)
            return;

        ScreenMenu volumeMenu = new ScreenMenu(Instance.Localizer.ForPlayer(player, "volume<menu>"), Instance);

        foreach (var kvp in Instance.Config.Settings.VolumeSettings)
        {
            float volume = kvp.Value;
            string display = kvp.Key;

            volumeMenu.AddOption(display, (p, option) =>
            {
                if (Instance.CLIENT_PREFS_API != null && Instance.VolumeCookie != -1)
                {
                    Instance.CLIENT_PREFS_API.SetPlayerCookie(p, Instance.VolumeCookie, volume.ToString());
                    Instance.playerVolumeCookies[p] = volume.ToString();
                    p.PrintToChat(Instance.Localizer["prefix"] + Instance.Localizer["volume.selected", display]);
                }
            });
        }
        volumeMenu.Open(player);
    }
}
