![Version](https://img.shields.io/github/v/release/Flazhik/CybergrindDeathcam)
![Licence](https://img.shields.io/github/license/Flazhik/CybergrindDeathcam)

# Cyber Grind Deathcam

![deathcam](https://github.com/Flazhik/CybergrindDeathcam/assets/2077991/8c520929-bc84-4800-985c-097289590482)

Ever wonder what the hell has just killed you in Cyber Grind? Sometimes it's hard keeping track on things like this with a ton of projectiles flying around.
This mod enables TF2-like camera upon death in Cyber Grind mode, allowing to see what's killed you.

Please note that it doesn't track dead enemies.

# Installation
1. Download the **BepInEx** release from [here](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21). It's recommended to choose **BepInEx_x64** release unless you're certain you have a 32-bit system
2. Extract the contents of **BepInEx** archive in your local **ULTRAKILL** folder. If you're not sure where this folder is located, find **ULTRAKILL** in your Steam Library > Right mouse click > **Properties** > **Local files** > **Browse**
3. Download the CybergrindDeathcam archive [here](https://github.com/Flazhik/CybergrindDeathcam/releases/download/v1.0.0/CybergrindDeathcam.v1.0.0.zip), then extract its contents at **ULTRAKILL/BepInEx/plugins** (create *plugins* folder manually in case it's missing)

## Leaderboards skip (Optional feature)
The mod is ready-to-go right off, but you can utilise its another feature.
If the mod has already been launched at least once, there a config file inside your ULTRAKILL folder located at `BepInEx\config\CybergrindDeathcam\config.cfg`

If you're willing to skip the leaderboards sequence at the end of your run and get back to Terminal room ASAP, you can change `SkipLeaderboards` value to `true`.
In this case, CG will be restarted almost instantly upon your death.

You can also notice `LeaderboardsSkipThreshold` value (set to 5 by default). If the difference between the wave you've just reached and your personal best is
lower than `LeaderboardsSkipThreshold` value, the leaderboards sequence will still be played.

## Extras
Deathcam sound effect was taken from Team Fortress 2 owned by Valve