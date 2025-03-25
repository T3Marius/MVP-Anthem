using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;
using CS2ScreenMenuAPI.Internal;
using Microsoft.Extensions.Logging;
using static MVPAnthem.Lib;

namespace MVPAnthem;

public partial class MVPAnthem : BasePlugin
{
    public override string ModuleAuthor => "T3Marius";
    public override string ModuleName => "MVP-Anthem";
    public override string ModuleVersion => "1.0.0";
    public PluginConfig Config { get; set; } = new PluginConfig();
    public static MVPAnthem Instance { get; set; } = new MVPAnthem();
    public override void Load(bool hotReload)
    {
        Instance = this;

        Events.Initialize();
        Commands.Initialize();
        Config = ConfigLoader.Load();
    }
    public override void OnAllPluginsLoaded(bool hotReload)
    {
        LoadClientPrefs();

        if (hotReload)
        {
            ReloadClientprefs();
        }
    }
    public override void Unload(bool hotReload)
    {
        UnloadClientprefis();
        Events.Dispose();
    }
}
