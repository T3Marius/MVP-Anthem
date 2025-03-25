# MVP Anthem plugin

# Requirements
- **[** [**CounterStrikeSharp**](https://github.com/roflmuffin/CounterStrikeSharp) **]**
- **[** [**MultiAddonManager**](https://github.com/Source2ZE/MultiAddonManager) **]**
- **[** [**ClientPrefsAPI**](https://github.com/Cruze03/Clientprefs) **]**

##You use for testing my WorkshopAddon: **[** [**Workshop**](https://steamcommunity.com/sharedfiles/filedetails/?id=3450055137) **]**
- Just add the addon id in multiaddongmanager.cfg and you can use the 2 mvp's.

# Basic Config:
```toml

# MVP Configuration.

[Settings]
DefaultVolume = 0.4             # this volume will be set to players who don't have one setted.
GiveRandomMVP = true            # when a player with no mvp joins the server, a random MVP is assinged to him.
DisablePlayerDefaultMVP = true  # with this on true the player mvp from steam will be disabled.
SoundEventFiles = ["soundevents/soundevents_zp.vsndevts", "soundevents/mvp_anthem.vsndevts"]            # VERY IMPORTANT: In order for the sounds to work you need to add the path for soundevent file here.

[Settings.VolumeSettings]
"100%" = 1.0                  # '100%' is how it shown in the menu '1.0' is the actual volume.
"80%" = 0.8
"60%" = 0.6
"40%" = 0.4
"20%" = 0.2
"0%" = 0.0

[Commands]
MVPCommands = ["mvp", "music"]        # Opens the MVP Menu
VolumeCommands = ["mvpvol", "vol"]    # Opens the Volume Menu (this is just a separate command)

[Timer]
CenterHtmlDuration = 10.0
CenterDuration = 10.0
AlertDuration = 10.0

[MVPSettings."PUBLIC MVP"."mvp.1"] # 'PUBLIC MVP' is the category which will be shown in the menu. 'mvp.1' is the key which u set the message too.
MVPName = "Flawless"
MVPSound = "MVP_Flawless"
EnablePreview = true
ShowChatMessage = true
ShowCenterMessage = false
ShowAlertMessage = false
ShowHtmlMessage = true
SteamID = ""
Flags = []

[MVPSettings."PUBLIC MVP"."mvp.2"]
MVPName = "Protection Charm"
MVPSound = "MVP_ProtectionCharm"
EnablePreview = true
ShowChatMessage = true
ShowCenterMessage = false
ShowAlertMessage = false
ShowHtmlMessage = true
SteamID = ""
Flags = []

```

# Commands
```
!mvp (opens the mvp menu)
!mvpvol (opens a separate volume menu) (though you have the mvp volume in the main mvp menu too.)
```

If you wanna ask about the plugin or support me contact me here:
Discord: mariust3

Donations:
- **[** [**Revolut**](revolut.me/dynutrqxrj) **]**
