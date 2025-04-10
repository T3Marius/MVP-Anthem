using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using T3MenuSharedApi;

namespace MVPAnthem;

public partial class MVPAnthem : BasePlugin
{
    public override string ModuleAuthor => "T3Marius";
    public override string ModuleName => "MVP-Anthem";
    public override string ModuleVersion => "1.0.2";
    public PluginConfig Config { get; set; } = new PluginConfig();
    public static MVPAnthem Instance { get; set; } = new MVPAnthem();
    public IT3MenuManager? MenuManager;
    public IT3MenuManager? GetMenuManager()
    {
        if (MenuManager == null)
            MenuManager = new PluginCapability<IT3MenuManager>("t3menu:manager").Get();

        return MenuManager;
    }
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
