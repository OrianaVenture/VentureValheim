# World Scaling

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Rebalance creatures and items with a configurable auto-scaling system. Ability to add your own custom overrides.

## Features

Set the difficulty of each biome and configure all creatures and items by using the automatic scaling system. You may also define your own custom configuration for individual creatures and items (including other modded content). This mod can dynamically create a game balance that is very different from the Vanilla gameplay experience!

It is highly recommend to use a mod that shows the creature health in-game so you and your players can see the differences when scaling creatures. BetterUI_Reforged by thedefside is one of my favorites.

Below are some explanations of features and how to configure them. See more details in the config file. Generate the config file by launching the game once with this mod installed.

<details open>
<summary>Expand/Collapse Features</summary>

### World Scaling

The world is scaled according to the "natural" vanilla game progression by default. To see this mod's default classification of vanilla creatures and items you can view the code on Github. The scaling is applied after logging into a local world or server. You will have to have players log out and back in after changing these configurations for them to take effect.

#### Configuration Options

* AutoScaleType: Vanilla, Linear, Exponential, or Custom. If set to Vanilla these auto-scaling features will not be enabled!
* AutoScaleFactor: Change the biome scaling factor.
* AutoScaleIgnoreOverrides: When true ignores the overrides specified in the yaml files. (yaml files explained below)

If you do not want to rescale the whole game and just want to change just a few things (or a lot of things, you do you) you can use the "Custom" scaling type to ignore the mod defaults and just scale things in your yaml override(s). If any information is missing from your yaml configuration that is needed for scaling the mod will fallback to using the default values for the missing fields. Custom scaling will use the linear scaling methods whenever applicable.

Linear scaling by default is a 75% growth (0.75). This means your 1st biome (Black Forest, Meadows is 0th) will have a scaling factor of 1.75, and 7th will be 6.25 for calculations. This setting will make a pretty even difficulty progression.

To use Exponential scaling PLEASE READ THIS PART: Given that there are by default only 8 biome difficulties to scale, the maximum scaling value you can input is roughly 21 without blatantly breaking the code generating the values (If using 12 custom biome difficulties this number is about 6). However, 21 is a much, much bigger number than you could ever want. Recommended values for exponential scaling are in a range of 0.25 - 1. For example, an exponential scaling of 0.75 will set the 1st biome to 1.75x harder, 7th biome to be about 50x harder than the base biome. This setting will make the first few biomes closer together in difficulty than the later biomes.

### Creature Scaling

The total damage a creature can do will be scaled and then distributed to individual damage types for each attack; scaling will maintain the ratio for creatures with more than one attack (some attacks are stronger, some are weaker, scaling maintains this). All values for chop and pickaxe damage are ignored and will retain their original values without affecting scaling. To override this mod's default configurations there is a file called "VWS.CreatureOverrides.yaml" in which you can add customizations.

** Currently, all players must have the same file locally since this feature does not sync yet. Directions on how to use the override feature are included in that file.

#### Configuration Options

The list for health and damage configs is in the format (for difficulty spread): Harmless, Novice, Average, Intermediate, Expert, Boss

* AutoScaleCreatures: Set to true to scale creature health and damage.
* AutoScaleCreaturesHealth: Leave blank to use default values: 5, 10, 30, 50, 200, 500; to override enter a comma-separated list of 6 integers.
* AutoScaleCreaturesDamage: Leave blank to use default values: 0, 5, 10, 12, 15, 20; to override enter a comma-separated list of 6 integers.

### Item Scaling

Vanilla items are grouped by custom types and are assigned the biome in which they normally can be crafted. To override this mod's default configurations there is a file called "VWS.ItemOverrides.yaml" in which you can add customizations.

** Currently, all players must have the same file locally since this feature does not sync yet. Directions on how to use the override feature are included in that file.

#### Configuration Options

* AutoScaleItems: Set to true to scale player items.

### Customization Help

Under the general section of the config file there is a GenerateGameDataFiles setting. When true this will create files of game data (for viewing only) which can help if you want to use the override files. Move the two folders in the config path that get created to another location on your computer to save the files permanently. These files can be overwritten with scaled data if you launch multiple games in one session. To get the vanilla data load a world once then quit the game. To get scaled data load a world, logout, and load it again, then quit.

Once you have the files set GenerateGameDataFiles back to false to keep your mod manager configs clean.

Another way to view the things this mod is changing you can turn on debugging logs. To do this you must change your BepInEx.cfg settings and include "Debug" under the Logging.Disk - LogLevels setting. This will allow the mod to print out debugging information I use myself to check what the mod is doing.

#### Custom Data Definitions

| Biomes | Difficulty | Item Types |
| ------ | ---------- | ---------- |
| Meadow = 0 | Vanilla = 0 | None = 0 |
| BlackForest = 1 | Harmless = 1 | Shield = 1 |
| Swamp = 2 | Novice = 2 | Helmet = 2 |
| Mountain = 3 | Average = 3 | Chest = 3 |
| Plain = 4 | Intermediate = 4 | Legs = 4 |
| AshLand = 5 | Expert = 5 | Shoulder = 5 |
| DeepNorth = 6 | Boss = 6 | Utility = 6 |
| Ocean = 7 | | Tool = 7 |
| Mistland = 8 | | PickAxe = 8 |
| | | Axe = 9 |
| | | Bow = 10 |
| | | Ammo = 11 |
| | | Sword = 20 |
| | | Knife = 21 |
| | | Mace = 22 |
| | | Sledge = 23 |
| | | Atgeir = 25 |
| | | Battleaxe = 26 |
| | | Primative = 27 |
| | | Spear = 28 |
| | | TowerShield = 29 |
| | | BucklerShield = 30 |
| | | PrimativeArmor = 31 |
| | | Bolt = 32 |
| | | Crossbow = 33 |
| | | HelmetRobe = 34 |
| | | ChestRobe = 35 |
| | | LegsRobe = 36 |

</details>

## How to port over from World Advancement & Progression

If you were previously using this feature in my other mod the configs are very similar with some renaming. Manually update the config file after it auto-generates for you upon launching the game once. If you were using the yaml files all you have to do is rename them with the new prefix (from the old WAP): VWS.

## Installation

This mod needs to be on all clients to work properly. Config Syncing is included with Jotunn. Install on the server to enforce the same mod configuration for all players. Live changes to the configurations will not take effect until the player relogs into the world/server.

## Changelog

### 0.2.0

* Added Jotunn library as new dependency for config syncing, you now must also install Jotunn for this mod to work

### 0.1.0

* Pulled out from World Advancement & Progression version 0.0.28
* Some config renaming, no major feature changes

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
