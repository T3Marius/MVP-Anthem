using static MVPAnthem.MVPAnthem;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Translations;
using static MVPAnthem.Lib;

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
                foreach (var kvp in Instance.Config.Settings.VolumeSettings)
                {
                    if (Math.Abs(kvp.Value - volumeValue) < 0.01f)
                    {
                        volumeLabel = kvp.Key;
                        break;
                    }
                }
            }

            mainMenu.AddTextOption(Instance.Localizer.ForPlayer(player, "mvp<currentvolume>", volumeLabel));
        }

        if (Instance.playerMVPCookies.TryGetValue(player, out string? activeMvp) && !string.IsNullOrEmpty(activeMvp))
        {
            mainMenu.Add(Instance.Localizer.ForPlayer(player, "mvp<remove>"), (p, option) =>
            {
                var confirmMenu = manager.CreateMenu(Instance.Localizer.ForPlayer(p, "mvp<remove.confirm>"), isSubMenu: true);
                confirmMenu.ParentMenu = mainMenu;

                confirmMenu.Add(Instance.Localizer.ForPlayer(p, "remove<yes>"), (p, option) =>
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

                confirmMenu.Add(Instance.Localizer.ForPlayer(p, "remove<no>"), (p, option) =>
                {
                    manager.CloseMenu(p);
                });

                manager.OpenSubMenu(player, confirmMenu);
            });
        }

        mainMenu.Add(Instance.Localizer.ForPlayer(player, "volume<option>"), (p, option) =>
        {
            var volumeMenu = manager.CreateMenu(Instance.Localizer.ForPlayer(p, "volume<menu>"), isSubMenu: true);
            volumeMenu.ParentMenu = mainMenu;

            foreach (var kvp in Instance.Config.Settings.VolumeSettings)
            {
                float volume = kvp.Value;
                string display = kvp.Key;



                volumeMenu.Add(display, (p, o) =>
                {
                    if (Instance.CLIENT_PREFS_API != null && Instance.VolumeCookie != -1)
                    {
                        Instance.CLIENT_PREFS_API.SetPlayerCookie(p, Instance.VolumeCookie, volume.ToString());
                        Instance.playerVolumeCookies[p] = volume.ToString();
                        p.PrintToChat(Instance.Localizer["prefix"] + Instance.Localizer["volume.selected", display]);
                    }
                });
            }
            manager.OpenSubMenu(p, volumeMenu);
        });

        mainMenu.Add(Instance.Localizer.ForPlayer(player, "mvp<option>"), (p, o) =>
        {
            var categoryMenu = manager.CreateMenu(Instance.Localizer.ForPlayer(p, "categories<menu>"), isSubMenu: true);
            categoryMenu.ParentMenu = mainMenu;

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
                    categoryMenu.Add(category.Key, (categoryPlayer, categoryOption) =>
                    {
                        var mvpsMenu = manager.CreateMenu(category.Key, isSubMenu: true);
                        mvpsMenu.ParentMenu = categoryMenu;

                        foreach (var mvpEntry in category.Value)
                        {
                            var mvpSettings = mvpEntry.Value;
                            if (ValidatePlayerForMVP(categoryPlayer, mvpSettings))
                            {
                                mvpsMenu.Add(mvpSettings.MVPName, (mvpPlayer, mvpOption) =>
                                {
                                    var mvpActionMenu = manager.CreateMenu(Instance.Localizer.ForPlayer(mvpPlayer, "mvp<equip>", mvpSettings.MVPName), isSubMenu: true);
                                    mvpActionMenu.ParentMenu = mvpsMenu;

                                    mvpActionMenu.Add(Instance.Localizer.ForPlayer(mvpPlayer, "equip<yes>"), (actionPlayer, actionOption) =>
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
                                        mvpActionMenu.Add(Instance.Localizer.ForPlayer(mvpPlayer, "preview<option>"), (actionPlayer, actionOption) =>
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
                        }

                        manager.OpenSubMenu(player, mvpsMenu);
                    });
                }
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

        foreach (var kvp in Instance.Config.Settings.VolumeSettings)
        {
            float volume = kvp.Value;
            string display = kvp.Key;



            volumeMenu.Add(display, (p, o) =>
            {
                if (Instance.CLIENT_PREFS_API != null && Instance.VolumeCookie != -1)
                {
                    Instance.CLIENT_PREFS_API.SetPlayerCookie(p, Instance.VolumeCookie, volume.ToString());
                    Instance.playerVolumeCookies[p] = volume.ToString();
                    p.PrintToChat(Instance.Localizer["prefix"] + Instance.Localizer["volume.selected", display]);
                }
            });
        }
        manager.OpenMainMenu(player, volumeMenu);
    }
}