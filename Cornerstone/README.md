# Cornerstone

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Tweak the individual stability values of each material type. Can build higher and wider overhanging structures!

## Support Me!

Thank you to everyone who have used my mods during early access. All of your feedback and support has made the hundreds of hours sunk into these projects worth it. Even though modding has never really paid out in cash it has rewarded me with friends, community, and even more fun times! To all of you who have bought me coffees over the years, I am extremely grateful for your kindness!

If you would like to keep supporting the development and maintenance of my mods you can buy me a coffee through the image link below.

Looking for a Valhiem server? I have partnered with Survival Servers to get you 25% off with the code ``VALHEIM25``! Order through the link by clicking on the image below and I will get a portion of the proceeds. You get a server to play with your friends and I can go buy some more of that sweet sweet coffee to keep the mods working!

<p align="center">
<a href="https://buymeacoffee.com/sk2qbkydvk"><img src="https://raw.githubusercontent.com/OrianaVenture/VentureValheim/master/SharedImages/BuyMeACoffeeAd.png?raw=true"></a>
<a href="https://www.survivalservers.com/r/venturevalheim/valheim"><img src="https://raw.githubusercontent.com/OrianaVenture/VentureValheim/master/SharedImages/SurvivalServersAd.png?raw=true"></a>
</p>

## Features

Exposes the configurations for each building material type so you can tweak them to your liking.

* Maximum Support: The support given to the piece when on solid ground.
* Minimum Support: The support needed for the piece to remain standing.
* Vertical Loss: How much the vertical support diminishes calculated from the pieces supporting it. Lower values let you build higher.
* Horizontal Loss: How much the horizontal support diminishes calculated from the distance of the pieces supporting it. Lower values let you build more outward without supports underneath.

For example: a loss value of 1 would calculate to full negation of support in that direction. This is why you cannot build a stone piece without something underneath it in vanilla (except for arches which had additional support rules).

As of the last update to this mod the defaults for vanilla are as follows:

| Material | Max Support | Min Support | Vertical Loss | Horizontal Loss |
|--- |--- |--- |--- |--- |
| Wood | 100 | 10 | 0.125 | 0.2 |
| HardWood | 140 | 10 | 0.1 | 0.167 |
| Stone | 1000 | 100 | 0.125 | 1 |
| Iron | 1500 | 20 | 0.0769 | 0.0769 |
| Marble | 1500 | 100 | 0.125 | 0.5 |
| Ashstone | 2000 | 100 | 0.1 | 0.167 |
| Ancient | 5000 | 100 | 0.0667 | 0.25 |
| Ice | 1000 | 100 | 0.125 | 0.333 |
| Timberwood | 200 | 10 | 0.0769 | 0.2 |

### Tips

* Disable each material section individually to use vanilla settings for that section. Recommended if you do not want to change how that material works.
* To remove instability entirely set both loss values for the material to 0f.
* To make materials stronger increase the Maximum Support.
* To allow adding "just one more thing on top" decrease the Minimum Support.
* Corewood pieces are the HardWood material type.
* Darkwood pieces (made with Tar) are the Wood material type.

## Installation

This mod needs to be on both the client and server; the mod will enforce installation. Players without the mod will NOT be able to connect to the server. Live updates to the configurations will take immediate effect.

If the mod is removed from the server after pieces have been placed they can potential crumble! If this happens you will need to restore a backup and reinstall a mod that increases stability to fix it (or just let everything crash and burn).

## Contributing

All issues can be reported on Discord or on the project Github. To report issues please be as specific as possible and provide the following:

1. Mod name and version with the issue.
2. The Bepinex\Logoutput.log file when the issue occurred (Or at least a list of all other mods being used)

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).