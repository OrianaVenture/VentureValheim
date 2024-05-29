# Norse Personality Construction System

A questing framework for servers. Create your own NPCs and give them purpose!

## Disclaimer!

This mod is not finished! There are many features that still need to get added. Feedback on current supported features will be greatly appreciated. See the list of possible future improvements and contributing information at the bottom of this readme.

## Features

* Create your own custom NPC characters and assign them different functions including:
  * Information: Give the player information or a quest.
  * Reward: Give a reward to players for speaking with them or giving them an item.
  * Sellsword: Hireable and will fight for the player. (Not finished)
  * SlayTarget: Defeat this npc for rewards. (Not finished)
  * Trader: Runs a store similar to Haldor/Hildir. (Not finished)
* NPCs can have random movement and actions such as sitting in chairs. (Not finished)
* NPCs come back to life once killed by default.
* NPCs can be set to permanently die and players can choose when to bury the body.

## World Advancement & Progression

This mod will pair nicely with WAP's private key system if you want players to have individual progress for setting questing keys. Otherwise the key system will use global configurations and all progress on the server will advance together.

Support for private progress without the progression mod is being considered for a future update.

## Getting Started

Currently there are four ways to spawn NPCs: Using **RightCtrl + E** on either a Bed or Chair, the npcs_spawnrandom command, or from a configuration with the command npcs_spawnsaved. Random NPCs will all be of the default NPC type none and will have random styles. They have no functionality other than adding clutter and life to the world. To customize NPCs and give them functions you need to first define them using the YAML file. The next section explains this process.

All NPCs act like players by respawning automatically upon death at their original spawn point. If you want an NPC to truly die you must set them up with either the "true death" command or in your yaml config for the NPC. During a true death NPC ragdolls do not disappear until a player removes them.

By default all NPCs will move around randomly, you can use the console commands to make them stop moving, or edit the yaml config to have them stand still once spawned.

See all available commands in the Commands section below.

## Using the YAML file

There is a file that comes with the mod called **VV.NPCS.yaml** in the config folder with examples. This file is where you can define your custom NPCs to be able to spawn them in game. You can also update existing NPCs from these configurations without needing to remove them from the game. When you update an existing NPC it will get overwritten and lose all data previously tied to it.

This file does not automatically live update if you change it while the game is running, but there is a command to force reload it. If there is information missing from your npc configurations it will be set to the default value and should not throw errors. If you see errors check you have used the correct data type for the config.

The yaml file is local and will not sync to others. This file is not required on the server nor is it needed for the mod to function. It is purely for setup if you want to customize NPCs.

You can always reach out for support if you have trouble setting things up (see contributing at bottom).

### NPC Types

Acceptable types include: None, Information, Reward, Sellsword, SlayTarget, Trader

#### Information

These NPCs simply say something when interacted with. 

* The ``interactText`` information can be locked until the npc meets both their ``requiredKeys`` ``notRquiredKeys`` criteria. When those conditions are met they will say their ``interactText``, otherwise they will say their ``defaultText``.
* Use the ``interactKey`` field to award the player speaking with them.

Here is an example of a pair of NPCs. Ragnar1 will give the key "ragnarbrave" when interacting with them, which Ragnar2 requires to tell you a secret. Ragnar1 will only tell you their secret once, so you better pay attention! Note both of these are set to stand still on spawn, which can be very helpful when setting things up.

```yaml
npcs:
  -
    id: Ragnar1
    name: Ragnar The Brave
    standStill: true
    type: Information
    notRquiredKeys: ragnarbrave
    defaultText: "Did you say you had mead?"
    interactKey: ragnarbrave
    interactText: "Here's some gossip about Ragnar The Bold!"

  -
    id: Ragnar2
    name: Ragnar The Bold
    standStill: true
    type: Information
    requiredKeys: ragnarbrave
    defaultText: "I love a good mead."
    interactText: "He said that? Let me tell you a secret about Ragnar The Brave."
```

#### Reward

These NPCs have all the functionality of an information npc plus the ability to accept an item in exchange for a reward item.

* ``giveItem`` and ``giveItemAmount`` are the quest requirements that a player must give the NPC.
* Use the ``rewardKey`` to record the player successfully completing the quest. The difference between the ``interactKey`` and ``rewardKey`` is important to note here since ``interactKey`` will always be given to the player when the npc is spoken to.
* ``rewardText``, ``rewardItem``, and ``rewardItemAmount`` are used for the physical reward once the NPC receives the ``giveItem``. You can choose to just use the ``rewardKey`` too rather than specifying and item.
* There are two "keywords" you can use when setting up your npc text: **{giveitem}** and **{reward}**, which will be automatically replaced with the corresponding prefab and amount so you don't have to type it out.
* You can use two different methods to control how often a quest is completable: ``rewardLimit`` or ``notRequiredKeys``.
  * ``rewardLimit``: when set to 10 this will allow the quest the be completed 10 total times; when reaching 0 it will lock the quest. By default ``rewardLimit`` is set to -1, which allows for unlimited completion.
  * ``notRequiresKeys``: when set to the same key as the reward key will lock the quest the first time it is completed.

##### Example

Here's an example of four NPCs that require you speak to them and complete their quests in order. Liv will give out a reward of a 2 star Flint Knife, which Vivica will accept and give the Rahshahs quest to bring him Deer Stew in exchange for Coins. The Jarl then rewards you for helping them all with an Iron Sword.

Liv is set up in a way that only the first player (5 players when using private keys from the Progression mod) there will get the wood and the rewardText. Other players when talking to this npc will still be able to receive the "liv" key but will not be given the wood reward and will only see the defaultText once the limit is reached. Again, note the difference between rewardKey and interactKey. Since Liv does not have any requiredKeys nor require a giveItem to give a reward you will never see the InteractText used, so it is not specified here.

Vivica is set up such that you must speak first to liv before you can see her interactText. When giving her the requested item she will say the rewardText. After this quest is completed Vivica will return to saying her defaultText.

Similar to Vivica, Rahshahs will also require a key to unlock the interactText, then return to saying his defaultText once the quest is completed.

Finally, the Jarl will give you a reward for helping all his people. Since the Jarl does not have a giveItem requirement the interactText will be displayed until the requiredKeys conditions are met. The only time you will see the defaultText is after receiving your reward.

This an be a bit confusing, so try spawning these examples and interacting with them in game to see how they work.

```yaml
npcs:
  -
    id: Liv
    name: Liv the Wise
    type: Reward
    notRequiredKeys: liv
    defaultText: "Did you bring {reward} to Vivica?"
    interactKey: liv
    rewardText: "Can you give this to Vivica? She's too shy to admit she needs my help."
    rewardItem: KnifeFlint
    rewardItemQuality: 2
    rewardLimit: 5

  -
    id: Vivica
    name: Vivica
    type: Reward
    requiredKeys: liv
    notRequiredKeys: vivica
    defaultText: "I'm too busy to talk."
    interactText: "Liv sent you? I guess I could use some help. I need {giveitem}."
    giveItem: KnifeFlint
    giveItemQuality: 2
    rewardText: "My husband Rahshahs could use some help too, can you find him?"
    rewardKey: vivica

  -
    id: MrHamHands
    name: Rahshahs
    type: Reward
    requiredKeys: vivica
    notRequiredKeys: rahshahs
    defaultText: "What do you want? Go away."
    interactText: "Vivica sent you? She didn't pay you!? I need {giveitem}, I will give you {reward} in exchange."
    giveItem: DeerStew
    giveItemAmount: 1
    rewardText: "Thanks, I was hungry! Sorry about my wife, this should be more than enough to cover your help."
    rewardItem: Coins
    rewardItemAmount: 10
    rewardKey: rahshahs
  -
    id: Jarl
    name: Jarl Halsin
    type: Reward
    requiredKeys: liv, vivica, rahshahs
    notRequiredKeys: thaneofvalheim
    defaultText: "Good day citizen."
    interactText: "Help my people and I will give you {reward}."
    rewardText: "You actually did it, I'm impressed!"
    rewardItem: SwordIron
    rewardKey: thaneofvalheim

```

#### Sellsword

Not Finished.

#### SlayTarget

Not Finished.

#### Trader

Not Finished.

### NPC Style

There are fields for setting the NPC appearance using item prefab names. Anything not specified will be left blank or randomized. Skin and Hair color require all r,g,b values to be defined to override; they are float values between 0 and 1. Overriding colors can allow you to make some very strange characters. When left blank these colors will be randomized to any possible vanilla customization value. There are commands to get these values from existing npcs, so you can spawn a group of randomly generated ones to find possible vanilla values rather quickly.

```yaml
npcs:
  -
    id: Ragnar3
    name: Ragnar The Dressed
    model: Player
    modelIndex: 0
    hair: Hair1
    beard: Beard1
    helmet: HelmetHat1
    chest: ArmorTunic1
    legs: ArmorLeatherLegs
    shoulder: CapeDeerHide
    utility: BeltStrength
    rightHand: KnifeFlint
    leftHand: Torch
    RightBack:
    LeftBack:

  -
    id: Clown
    name: Creepo The Clown
    type: None
    model: Player
    modelIndex: 0
    hair: Hair19
    chest: ArmorTunic6
    legs: ArmorRagsLegs
    skinColorR: 0.9
    skinColorG: 0.8
    skinColorB: 0.1
    hairColorR: 0.2
    hairColorG: 0.3
    hairColorB: 0.9

```

## Commands

You can spawn a random npc on a chair with RightAlt + E, and on a bed with RightCtrl + E - subject to change.

| Command            | Arguments  | Description |
| ------------------ | ---------- | ----------- |
| npcs_reloadconfig  |            | Reloads any changes to your yaml file |
| npcs_spawnrandom   | \[name\] \[Model\] | Spawns a random npc (Player or Skeleton only right now) |
| npcs_spawnsaved    | \[id\]     | Spawns an npc from the yaml config |
| npcs_set           | \[id\]     | Updates the closest npc from the yaml config |
| npcs_set_move      |            | Updates the closest npc to walk around |
| npcs_set_still     |            | Updates the closest npc to stand still |
| npcs_set_sit       |            | Updates the closest npc to sit in the closest chair |
| npcs_set_calm      | \[radius\] | Updates the closest npc to sit in the closest chair |
| npcs_set_truedeath |            | Updates the closest npc to not respawn upon next death |
| npcs_get_skincolor |            | Returns the skin color of the closest npc |
| npcs_get_haircolor |            | Returns the hair color of the closest npc |

## Possible Future Improvements

* NPC option to drop items on death.
* NPC options to perform more kinds of things than just sitting/standing/walking.
* Randomly generated NPC names.
* More NPC model options than just Player/Skeleton.
* Graves with signs so you can properly mourn your fallen NPCs.
* Cooldown timer on completing quests.
* Player quest tracker in the UI.
* Easier ways to list/save/change/copy NPC information.
* Necromancy options for fallen NPCs.
* Translation options.

## Installation

This mod needs to be on both the client and server; the mod will enforce installation. Players without the mod will NOT be able to connect to the server.

If the mod is removed from the server after NPCs have been generated they will disappear.

## Changelog

### 0.0.1

* First Release.

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).