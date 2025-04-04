﻿using static MVPAnthem.MVPAnthem;
using static MVPAnthem.Lib;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
namespace MVPAnthem;

public static class Menu
{
    public static void DisplayMVP(CCSPlayerController player)
    {
        if (player == null)
            return;
        
        var mainMenu = new CS2ScreenMenuAPI.Menu(player, Instance)
        {
            Title = Instance.Localizer.ForPlayer(player, "mvp<mainmenu>"),
            ShowDisabledOptionNum = false,
        };

        if (Instance.playerMVPCookies.TryGetValue(player, out string? mvpCookie) && !string.IsNullOrEmpty(mvpCookie))
        {
            string[] parts = mvpCookie.Split(';');
            string displayMVP = parts.Length > 0 ? parts[0] : mvpCookie;
            mainMenu.AddItem(Instance.Localizer.ForPlayer(player, "mvp<currentmvp>", displayMVP), (p, o) => { }, true);
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

            mainMenu.AddItem(Instance.Localizer.ForPlayer(player, "mvp<currentvolume>", volumeLabel), (p, o) => { }, true);
        }

        mainMenu.AddItem(" ", (p, o) => { }, true);

        if (Instance.playerMVPCookies.TryGetValue(player, out string? activeMvp) && !string.IsNullOrEmpty(activeMvp))
        {
            mainMenu.AddItem(Instance.Localizer.ForPlayer(player, "mvp<remove>"), (p, option) =>
            {
                var confirmMenu = new CS2ScreenMenuAPI.Menu(p, Instance)
                {
                    Title = Instance.Localizer.ForPlayer(p, "mvp<remove.confirm>"),
                    IsSubMenu = true,
                    ParentMenu = mainMenu,
                };

                confirmMenu.AddItem(Instance.Localizer.ForPlayer(p, "remove<yes>"), (p, option) =>
                {
                    if (Instance.CLIENT_PREFS_API != null && Instance.MVPCookie != -1)
                    {
                        Instance.CLIENT_PREFS_API.SetPlayerCookie(p, Instance.MVPCookie, string.Empty);
                        if (Instance.playerMVPCookies.ContainsKey(p))
                        {
                            Instance.playerMVPCookies.Remove(p);
                        }
                        p.PrintToChat(Instance.Localizer["prefix"] + Instance.Localizer["mvp.removed"]);
                        confirmMenu.Close(p);
                    }
                });

                confirmMenu.AddItem(Instance.Localizer.ForPlayer(p, "remove<no>"), (p, option) =>
                {
                    confirmMenu.Close(p);
                });

                confirmMenu.Display();
            });
        }

        mainMenu.AddItem(Instance.Localizer.ForPlayer(player, "volume<option>"), (p, option) =>
        {
            var volumeMenu = new CS2ScreenMenuAPI.Menu(p, Instance)
            {
                Title = Instance.Localizer.ForPlayer(p, "volume<menu>"),
                IsSubMenu = true,
                ParentMenu = mainMenu,
            };

            foreach (var kvp in Instance.Config.Settings.VolumeSettings)
            {
                float volume = kvp.Value;
                string display = kvp.Key;

                volumeMenu.AddItem(display, (p, o) =>
                {
                    if (Instance.CLIENT_PREFS_API != null && Instance.VolumeCookie != -1)
                    {
                        Instance.CLIENT_PREFS_API.SetPlayerCookie(p, Instance.VolumeCookie, volume.ToString());
                        Instance.playerVolumeCookies[p] = volume.ToString();
                        p.PrintToChat(Instance.Localizer["prefix"] + Instance.Localizer["volume.selected", display]);
                    }
                });
            }

            volumeMenu.Display();
        });

        mainMenu.AddItem(Instance.Localizer.ForPlayer(player, "mvp<option>"), (p, o) =>
        {
            var categoryMenu = new CS2ScreenMenuAPI.Menu(p, Instance)
            {
                Title = Instance.Localizer.ForPlayer(p, "categories<menu>"),
                IsSubMenu = true,
                ParentMenu = mainMenu,
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
                    categoryMenu.AddItem(category.Key, (categoryPlayer, categoryOption) =>
                    {
                        var mvpsMenu = new CS2ScreenMenuAPI.Menu(categoryPlayer, Instance)
                        {
                            Title = category.Key,
                            IsSubMenu = true,
                            ParentMenu = categoryMenu,
                        };

                        foreach (var mvpEntry in category.Value)
                        {
                            var mvpSettings = mvpEntry.Value;
                            if (ValidatePlayerForMVP(categoryPlayer, mvpSettings))
                            {
                                mvpsMenu.AddItem(mvpSettings.MVPName, (mvpPlayer, mvpOption) =>
                                {
                                    var mvpActionMenu = new CS2ScreenMenuAPI.Menu(mvpPlayer, Instance)
                                    {
                                        Title = Instance.Localizer.ForPlayer(mvpPlayer, "mvp<equip>", mvpSettings.MVPName),
                                        IsSubMenu = true,
                                        ParentMenu = mvpsMenu,
                                        HasExitButon = true
                                    };

                                    mvpActionMenu.AddItem(Instance.Localizer.ForPlayer(mvpPlayer, "equip<yes>"), (actionPlayer, actionOption) =>
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
                                        mvpActionMenu.AddItem(Instance.Localizer.ForPlayer(mvpPlayer, "preview<option>"), (actionPlayer, actionOption) =>
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

                                    mvpActionMenu.Display();
                                });
                            }
                        }

                        mvpsMenu.Display();
                    });
                }
            }

            categoryMenu.Display();
        });

        mainMenu.HasExitButon = true;
        mainMenu.Display();
    }

    public static void DisplayVolume(CCSPlayerController player)
    {
        if (player == null)
            return;

        var volumeMenu = new CS2ScreenMenuAPI.Menu(player, Instance)
        {
            Title = Instance.Localizer.ForPlayer(player, "volume<menu>"),

        };

        foreach (var kvp in Instance.Config.Settings.VolumeSettings)
        {
            float volume = kvp.Value;
            string display = kvp.Key;

            volumeMenu.AddItem(display, (p, option) =>
            {
                if (Instance.CLIENT_PREFS_API != null && Instance.VolumeCookie != -1)
                {
                    Instance.CLIENT_PREFS_API.SetPlayerCookie(p, Instance.VolumeCookie, volume.ToString());
                    Instance.playerVolumeCookies[p] = volume.ToString();
                    p.PrintToChat(Instance.Localizer["prefix"] + Instance.Localizer["volume.selected", display]);
                }
            });
        }

        volumeMenu.Display();
    }
}