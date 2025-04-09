using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using static MVPAnthem.MVPAnthem;

namespace MVPAnthem;

public static class Commands
{
    public static void Initialize()
    {
        foreach (var cmd in Instance.Config.Commands.MVPCommands)
        {
            Instance.AddCommand($"css_{cmd}", "Opens the MVP Menu", Command_MVP);
        }
        foreach (var cmd in Instance.Config.Commands.VolumeCommands)
        {
            Instance.AddCommand($"css_{cmd}", "Opens the volume menu", Command_Volume);
        }
    }
    public static void Command_MVP(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        switch (Instance.Config.Settings.MenuType)
        {
            case "screen":
            case "screenmenu":
                ScreenMenu.DisplayMVP(player);
                break;
            case "t3menu":
            case "t3":
                T3Menu.DisplayMVP(player);
                break;
        }
    }
    public static void Command_Volume(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        switch (Instance.Config.Settings.MenuType)
        {
            case "screen":
            case "screenmenu":
                ScreenMenu.DisplayVolume(player);
                break;
            case "t3menu":
            case "t3":
                T3Menu.DisplayVolume(player);
                break;
        }
    }
}