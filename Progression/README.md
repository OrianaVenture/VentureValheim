# World Advancement and Progression

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

This mod is in Beta: Use at your own risk! (make a backup of your data before you update). You will likely need to generate a fresh config file.

World Advancement and Progression lets you fine tune world settings. Ideal to use on multiplayer/RP (roleplay) servers to control world advancement.

## Features

NOTE: This mod is under heavy development and is not finished. All pre-releases are intended for those interested in helping test new features until the first official release.

The main feature of this mod is to have an easy way to fully customize the world difficulty. Set the difficulty of each biome by using the automatic scaling system or by defining your own custom scaling. Configure all creatures and items automatically with the auto-scaling feature. This mod will dynamically create a game balance that is very different from the Vanilla gameplay experience!

Below are some explanations of features and how to configure them. To see details generate the config file by launching the game once with this mod installed.

### Public Key Management

By default this mod will prevent/block public keys from being added to the global list. BlockAllGlobalKeys is true by default; To use Vanilla behavior change BlockAllGlobalKeys to false. Configure AllowedGlobalKeys or BlockedGlobalKeys lists to allow/block keys ONLY depending on the value of BlockAllGlobalKeys. To see details generate the config file. These keys are case sensitive and MUST match the value exactly or it will not work. This feature will work with other mods that add in custom keys.

Examples of Vanilla Public Keys:
* defeated_eikthyr
* defeated_gdking
* defeated_bonemass
* defeated_dragon
* defeated_goblinking
* KilledTroll
* killed_surtling
* KilledBat

Planned features include: Implementing a player key system separate from the global key system. For developers: define your own player keys and use them in your mods!

### Skill Manager

Customize skill drain by turning it off entirely, setting to an absolute number, or using a comparison to choose the lower/higher skill drain (the absolute or original value). Set the server wide maximum level for skill gain and minimum level for skill loss (set OverrideMaximumSkillLevel and/or OverrideMinimumSkillLevel to True). Any skills that are already above the maximum skill cap will remain "frozen" and will not gain, but can still be lowered on death.

Warning: Other mods that change how skill gain and loss functions may cause unexpected behaviors. Turn off this feature if using another mod for skill management if you see mod conflicts.

Planned features include: Implementing a skill floor and ceiling controlled by public and player keys.

### World Scaling (In Progress)

The world is scaled according to the natural vanilla game progression by default. To enable set AutoScale to true and change the AutoScaleType to Linear or Exponential scaling (if you are unfamiliar with these terms you should look these up before changing the default values). Vanilla creatures and items are sorted into their main or "natural" biome. To see this mod's classification of vanilla creatures and items you can view the code on Github.

The AutoScaleFactor option will let you change the scaling factor:

Linear scaling by default is a 75% growth (0.75). This means your 1st biome (Black Forest, Meadows is 0th) will have a scaling factor of 1.75, and 7th will be 6.25 for calculations.

To use Exponential scaling PLEASE READ THIS PART: Given that there are by default only 8 biome difficulties to scale, the maximum scaling value you can input is roughly 21 without blatantly breaking the code generating the values (If using 12 custom biome difficulties this number is about 6). However, 21 is a much, much bigger number than you could ever want. Recommended values for exponential scaling are in a range of 0.25 - 1. For example, an exponential scaling of 0.75 will set the 1st biome to 1.75x harder, 7th biome to be about 50x harder than the base biome. This is in stark contrast to the values set by linear scaling and is the ideal way (theoretically) to naturally enforce "Biome Locking" (which is the main reason why this mod exists).

Planned features include: Finishing the auto scaling to work for item upgrades and balancing the default configurations. Allowing users to override the default classifications of creatures and player items. (For now please give feedback on the defaults!)

#### Creature Scaling

To enable scaling of creatures set AutoScaleCreatures to true. To change the base health distribution enter a list of numbers for AutoScaleCreaturesHealth. Likewise, to scale the damage a creature can do enter a list of number for AutoScaleCreaturesDamage. The total damage will be scaled and then distributed to individual damage types for each attack; scaling will maintain the ratio for creatures with more than one attack (some attacks are stronger, some are weaker, scaling maintains this). All values for chop and pickaxe damage are ignored for creatures and will retain their original values without affecting scaling (The bosses were weird, decided not to touch this for now).

The list for configs is in the format (for difficulty spread): Harmless, Novice, Average, Intermediate, Expert, Boss

The default health values built into the code: 5, 10, 30, 50, 200, 500.
The default damage values built into the code: 0, 5, 10, 12, 15, 20.

#### Item Scaling

To enable scaling of player armor and weapons set AutoScaleItems to true. Vanilla items are grouped by type and are assigned the biome in which they naturally can be crafted.

To see defaults see the code in Github.

### Other Features

* ServerSync included

## Developers

### Custom Biomes (In Progress)

Planned features include: Define your own biomes and add them to the scaling system by using any int value as a key that is not used. This mod's default values can be overridden to set custom scaling after initialization. (Again, please use caution with the Exponential scaling feature configuration)

Examples (Will update this for first official release):

* To add a Biome '10' that is one biome harder than Plains (4th hardest by default, Meadows is 0th) use: AddBiome(10, 5)
* To add a Biome '10' that is 250% harder than the baseline use: AddCustomBiome(10, 2.5)
* To override Meadow's difficulty after it has been initialized: AddBiome(0, 8, true) or AddCustomBiome(0, 1.3, true)

Biomes are given the following int codes:

* Undefined = -1
* Meadow = 0
* BlackForest = 1
* Swamp = 2
* Mountain = 3,
* Plain = 4
* AshLand = 5
* DeepNorth = 6
* Ocean = 7
* Mistland = 8

### Custom Creature and Item support (In Progress)

Coming ASAP!

## Changelog

### 0.0.9

* Added configuration options for toggling the skill manager features and setting the ceiling and floor for skill gain and loss. Updated wording for other configurations, you will need to generate a new file.

### 0.0.8

* Added ability to scale player items. Does not scale the upgrade per level yet.

### 0.0.7

* Added ability to scale creature damage. Tweaked difficulty defaults for some creatures.

### 0.0.6

* Added ability to scale the world difficulty with maths! Scales creature health ONLY, updates to come!

### 0.0.5

* Added additional Skill Drain configuration option. Ability to use the minimum or maximum Skill Drain value (absolute skill drain vs vanilla).

### 0.0.4

* Packaged ServerSync into the main dll.

See all patch notes on Github.

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
