# Asocial Cartography

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Improved multiplayer Cartography Table handling. Toggle ability to share and receive player placed map pins at the table. Configure an overlap radius requirement for receiving map pins from the cartography table.

## Support Me!

Thank you to everyone who have used my mods during early access. All of your feedback and support has made the hundreds of hours sunk into these projects worth it. Even though modding has never really paid out in cash it has rewarded me with friends, community, and even more fun times! To all of you who have bought me coffees over the years, I am extremely grateful for your kindness!

If you would like to keep supporting the development and maintenance of my mods you can buy me a coffee through the image link below.

Looking for a Valhiem server? I have partnered with Survival Servers to get you 25% off with the code ``VALHEIM25``! Order through the link by clicking on the image below and I will get a portion of the proceeds. You get a server to play with your friends and I can go buy some more of that sweet sweet coffee to keep the mods working!

<p align="center">
<a href="https://buymeacoffee.com/sk2qbkydvk"><img src="https://raw.githubusercontent.com/OrianaVenture/VentureValheim/master/SharedImages/BuyMeACoffeeAd.png?raw=true"></a>
<a href="https://www.survivalservers.com/r/venturevalheim/valheim"><img src="https://raw.githubusercontent.com/OrianaVenture/VentureValheim/master/SharedImages/SurvivalServersAd.png?raw=true"></a>
</p>

## Features

Playing multiplayer can often cause cartography table debacle. Your friend NEEDS to mark every berry bush, and then they share it with you too. Well, no more.

### Player Placed Pins

This mod gives you two new ways to manage map pins from the cartography table: prevent adding and prevent taking. By default this mod will prevent the player from adding custom map pins to the cartography table, and allow taking all pins from the table. If your friends are less tech savy than you and don't know how to change config files this should fix your problems given they install the mod. If you are the only one installing the mod then you can change the ReceivePins config to false to prevent getting all those nasty berry bush pins.

These toggles only apply to the 5 placable pins in vanilla by default. All discovered boss alter locations and other types of pins will still be shared as usual. This makes it easier to share the pins that really matter without worrying about clutter. If you wish to also filter boss or hildir map pins set IgnoreBossPins and/or IgnoreHildirPins to false and they will be treated as player-placed pins for the other settings.

### Map Merging

In Valheim versions before 0.217.46 using the cartography table would overwrite the pins with your own when writing to the table. This mod historically merged map table pins but this feature is no longer required so was removed.

### Allowed Pin Radius

There is a configuration ReceivePinRadius to control how close pins are allowed to be in order to receive them when reading from the cartography table. This setting alone can greatly decrease clutter. Vanilla by default uses a value of 1. Recommended values are between 50 and 200.

### Adding To Existing Games

If you are adding this mod mid-game and need to clean up your existing map pins the vanilla command ``resetsharedmap`` will remove shared cartography data for you. If you desire to reset existing cartography tables you must destroy them and rebuild.

### Other Mod Support

The IgnoredCustomPins configuration will allow you to add the integer id of custom map pins to be ignored in this mod. However, this only works if you know the index of the pin type. For example, Multiplayer Tweaks adds three new pins that are assigned indexes based on the existing pin list when that mod loads and can change when you add or remove other mods with custom pins. Please reach out if this feature is not working as intended.

## Installation

This mod is client side only and changes made to the configurations will only affect your client. Live updates to the configs will take immediate effect.

## Contributing

All issues can be reported on Discord or on the project Github. To report issues please be as specific as possible and provide the following:

1. Mod name and version with the issue.
2. The Bepinex\Logoutput.log file when the issue occurred (Or at least a list of all other mods being used)

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).