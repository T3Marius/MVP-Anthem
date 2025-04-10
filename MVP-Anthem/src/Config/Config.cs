using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Reflection;
using Tomlyn;
using Tomlyn.Model;

namespace MVPAnthem;
public class PluginConfig
{
    public Settings_Config Settings { get; set; } = new();
    public Commands_Config Commands { get; set; } = new();
    public Timer_Settings Timer { get; set; } = new();
    public Dictionary<string, Dictionary<string, MVP_Settings>> MVPSettings { get; set; } = new()
    {
        {
            "PUBLIC MVP", new Dictionary<string, MVP_Settings>
            {
                {
                    "mvp.1", new MVP_Settings
                    {
                        MVPName = "Flawless",
                        MVPSound = "MVP_Flawless",
                        EnablePreview = true,
                        ShowChatMessage = true,
                        ShowCenterMessage = true,
                        ShowAlertMessage = true,
                        ShowHtmlMessage = true,
                        SteamID = "",
                        Flags = new List<string>()
                    }
                },
                {
                    "mvp.2", new MVP_Settings
                    {
                        MVPName = "Protection Charm",
                        MVPSound = "MVP_ProtectionCharm",
                        EnablePreview = true,
                        ShowChatMessage = true,
                        ShowCenterMessage = true,
                        ShowAlertMessage = true,
                        ShowHtmlMessage = true,
                        SteamID = "",
                        Flags = new List<string>()
                    }
                }
            }
        }
    };
}

public class Settings_Config
{
    public string MenuType { get; set; } = "screen";
    public List<string> SoundEventFiles { get; set; } = [""];
    public float DefaultVolume { get; set; } = 0.4f;
    public List<int> VolumeSettings = [100, 80, 60, 40, 20, 0];
    public bool GiveRandomMVP { get; set; } = true;
    public bool DisablePlayerDefaultMVP { get; set; } = true;
}

public class Timer_Settings
{
    public float CenterHtmlDuration { get; set; } = 10.0f;
    public float CenterDuration { get; set; } = 10.0f;
    public float AlertDuration { get; set; } = 10.0f;
}

public class MVP_Settings
{
    public string MVPName { get; set; } = string.Empty;
    public string MVPSound { get; set; } = string.Empty;
    public bool EnablePreview { get; set; } = true;
    public bool ShowChatMessage { get; set; } = true;
    public bool ShowCenterMessage { get; set; } = true;
    public bool ShowAlertMessage { get; set; } = true;
    public bool ShowHtmlMessage { get; set; } = true;
    public string SteamID { get; set; } = string.Empty;
    public List<string> Flags { get; set; } = new List<string>();
}

public class Commands_Config
{
    public List<string> MVPCommands { get; set; } = ["mvp", "music"];
    public List<string> VolumeCommands { get; set; } = ["mvpvol", "vol"];
}

public static class ConfigLoader
{
    private static readonly string ConfigPath;

    static ConfigLoader()
    {
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;

        ConfigPath = Path.Combine(
            Server.GameDirectory,
            "csgo",
            "addons",
            "counterstrikesharp",
            "configs",
            "plugins",
            assemblyName,
            "config.toml"
        );
    }

    public static PluginConfig Load()
    {
        if (!File.Exists(ConfigPath))
        {
            CreateDefaultConfig();
        }

        return LoadConfigFromFile();
    }

    private static PluginConfig LoadConfigFromFile()
    {
        string configText = File.ReadAllText(ConfigPath);
        TomlTable model = Toml.ToModel(configText);

        var config = new PluginConfig
        {
            Settings = LoadSettings((TomlTable)model["Settings"]),
            Commands = LoadCommands((TomlTable)model["Commands"]),
            Timer = LoadTimerSettings((TomlTable)model["Timer"]),
            MVPSettings = LoadMVPSettings((TomlTable)model["MVPSettings"])
        };

        return config;
    }

    private static Settings_Config LoadSettings(TomlTable settingsTable)
    {
        var settings = new Settings_Config
        {
            MenuType = settingsTable["MenuType"].ToString()!,
            DefaultVolume = float.Parse(settingsTable["DefaultVolume"].ToString()!),
            GiveRandomMVP = bool.Parse(settingsTable["GiveRandomMVP"].ToString()!),
            DisablePlayerDefaultMVP = bool.Parse(settingsTable["DisablePlayerDefaultMVP"].ToString()!),
        };

        if (settingsTable.ContainsKey("SoundEventFiles"))
        {
            settings.SoundEventFiles = GetStringArray((TomlArray)settingsTable["SoundEventFiles"]);
        }

        if (settingsTable.ContainsKey("VolumeSettings"))
        {
            var volumeArray = (TomlArray)settingsTable["VolumeSettings"];
            settings.VolumeSettings = new List<int>();

            foreach (var item in volumeArray)
            {
                settings.VolumeSettings.Add(int.Parse(item!.ToString()!));
            }
        }

        return settings;
    }

    private static Commands_Config LoadCommands(TomlTable commandsTable)
    {
        var commands = new Commands_Config();

        if (commandsTable.ContainsKey("MVPCommands"))
        {
            commands.MVPCommands = GetStringArray((TomlArray)commandsTable["MVPCommands"]);
        }

        if (commandsTable.ContainsKey("VolumeCommands"))
        {
            commands.VolumeCommands = GetStringArray((TomlArray)commandsTable["VolumeCommands"]);
        }

        return commands;
    }

    private static Timer_Settings LoadTimerSettings(TomlTable timerTable)
    {
        return new Timer_Settings
        {
            CenterHtmlDuration = float.Parse(timerTable["CenterHtmlDuration"].ToString()!),
            CenterDuration = float.Parse(timerTable["CenterDuration"].ToString()!),
            AlertDuration = float.Parse(timerTable["AlertDuration"].ToString()!)
        };
    }

    private static Dictionary<string, Dictionary<string, MVP_Settings>> LoadMVPSettings(TomlTable mvpSettingsTable)
    {
        var mvpSettings = new Dictionary<string, Dictionary<string, MVP_Settings>>();

        foreach (var categoryKey in mvpSettingsTable.Keys)
        {
            var categoryTable = (TomlTable)mvpSettingsTable[categoryKey];
            var categoryDict = new Dictionary<string, MVP_Settings>();

            foreach (var mvpKey in categoryTable.Keys)
            {
                var mvpTable = (TomlTable)categoryTable[mvpKey];
                var mvpSetting = new MVP_Settings
                {
                    MVPName = mvpTable["MVPName"].ToString()!,
                    MVPSound = mvpTable["MVPSound"].ToString()!,
                    EnablePreview = bool.Parse(mvpTable["EnablePreview"].ToString()!),
                    ShowChatMessage = bool.Parse(mvpTable["ShowChatMessage"].ToString()!),
                    ShowCenterMessage = bool.Parse(mvpTable["ShowCenterMessage"].ToString()!),
                    ShowAlertMessage = bool.Parse(mvpTable["ShowAlertMessage"].ToString()!),
                    ShowHtmlMessage = bool.Parse(mvpTable["ShowHtmlMessage"].ToString()!),
                    SteamID = mvpTable["SteamID"].ToString()!
                };

                if (mvpTable.ContainsKey("Flags"))
                {
                    mvpSetting.Flags = GetStringArray((TomlArray)mvpTable["Flags"]);
                }

                categoryDict[mvpKey] = mvpSetting;
            }

            mvpSettings[categoryKey] = categoryDict;
        }

        return mvpSettings;
    }

    private static List<string> GetStringArray(TomlArray array)
    {
        return new List<string>(array.Select(item => item!.ToString()!));
    }

    private static void CreateDefaultConfig()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);

        string defaultConfig = @"
    # MVP Configuration.

    [Settings]
    MenuType = ""screen""            # screen is the default menu, if you don't wanna use t3menu don't even add the shared of it.
    DefaultVolume = 0.4             # this volume will be set to players who don't have one setted.
    GiveRandomMVP = true            # when a player with no mvp joins the server, a random MVP is assinged to him.
    DisablePlayerDefaultMVP = true  # with this on true the player mvp from steam will be disabled.
    SoundEventFiles = [""soundevents/mvp_anthem.vsndevts""]            # VERY IMPORTANT: In order for the sounds to work you need to add the path for soundevent file here.
    VolumeSettings = [100, 80, 60, 40, 20, 0]

    [Commands]
    MVPCommands = [""mvp"", ""music""]        # Opens the MVP Menu
    VolumeCommands = [""mvpvol"", ""vol""]    # Opens the Volume Menu (this is just a separate command)

    [Timer]
    CenterHtmlDuration = 10.0
    CenterDuration = 10.0
    AlertDuration = 10.0

    [MVPSettings.""PUBLIC MVP"".""mvp.1""] # 'PUBLIC MVP' is the category which will be shown in the menu. 'mvp.1' is the key which u set the message to in lang folder.
    MVPName = ""Flawless""                 # This will be shown in the menu.
    MVPSound = ""MVP_Flawless""            # This is the soundevent name. With this the sound will play.
    EnablePreview = true
    ShowChatMessage = true
    ShowCenterMessage = false
    ShowAlertMessage = false
    ShowHtmlMessage = true
    SteamID = """"
    Flags = []

    [MVPSettings.""PUBLIC MVP"".""mvp.2""]
    MVPName = ""Protection Charm""
    MVPSound = ""MVP_ProtectionCharm""
    EnablePreview = true
    ShowChatMessage = true
    ShowCenterMessage = false
    ShowAlertMessage = false
    ShowHtmlMessage = true
    SteamID = """"
    Flags = []
    ";

        File.WriteAllText(ConfigPath, defaultConfig);
    }
}