using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using static MVPAnthem.MVPAnthem;

namespace MVPAnthem;

public static class Lib
{
    public static bool ValidatePlayerForMVP(CCSPlayerController player, MVP_Settings settings)
    {
        if (!string.IsNullOrEmpty(settings.SteamID) && player.SteamID.ToString() == settings.SteamID)
            return true;

        if (settings.Flags.Count > 0)
        {
            return IsPlayerInGroupPermission(player, string.Join(',', settings.Flags));
        }

        return string.IsNullOrEmpty(settings.SteamID) && settings.Flags.Count == 0;
    }
    public static bool IsPlayerInGroupPermission(CCSPlayerController player, string groups, string? requiredSteamId = null)
    {
        var excludedGroups = groups.Split(',');
        foreach (var group in excludedGroups)
        {
            if (group.StartsWith("#") && AdminManager.PlayerInGroup(player, group))
                return true;
            else if (group.StartsWith("@") && AdminManager.PlayerHasPermissions(player, group))
                return true;
        }

        if (!string.IsNullOrEmpty(requiredSteamId))
        {
            return player.SteamID.ToString() == requiredSteamId;
        }

        return false;
    }
}