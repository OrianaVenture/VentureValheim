# Norse Personality Construction System

A questing framework for servers. Create your own NPCs and give them purpose!

## Disclaimer!

This mod is not finished! There are many features that still need to get added. Feedback on current supported features will be greatly appreciated. See the list of possible future improvements and contributing information at the bottom of this readme.

## Features

* Create your own custom NPC characters and assign them different functions including:
  * Information: Give the player information or a quest.
  * Reward: Give a reward to players for speaking with them or giving them an item.
  * Sellsword: Hireable and will fight for the player. (Not finished)
  * SlayTarget: Defeat this NPC for rewards. (Not finished)
  * Trader: Runs a store similar to Haldor/Hildir.
* NPCs can have random movement and actions such as sitting in chairs. (Not finished)
* NPCs come back to life once killed by default.
* NPCs can be set to permanently die and players can choose when to bury the body.

## Global and Player Keys

What is a key and what is controlled by them? In vanilla Valheim there exists a "global key" list that is a bunch of strings shared by all players. There also exist vanilla "player keys"; These keys are saved separately for each player. Worldly spawns, raids, dreams, and Haldor's items are all controlled by the presence of specific keys. Each boss, select creatures in the game, as well as Hildir's quests each have keys associated with their completion. This mod also adds keys and uses the existing vanilla systems to control game behavior.

This mod can add to both the Global Key and Player Unique Key systems. Quest rewards can be customized to either apply to all players, or the individual that completed the action. When this mod checks for the existence of keys it will check both of these lists. This will be pointed out in the examples provided below.

### World Advancement & Progression Mod

This mod will pair nicely with WAP's private key system. If an NPC is configured to use Global keys then only the players within the radius (about 100 meters) of the receiving player will get the keys. If you wish all player in the area receive the key, then use the "Global" setting for your NPC configuration. Otherwise use the "Player" setting to have true individual progress. The mod will default to "Player" when not specified.

If you desire that some keys are allowed to be Global you can use the advanced configurations for the progression mod to allow for these keys to be added to the global list. This is useful for quests that you only want one player to be able to complete, or if you only want to require one player to perform an action to unlock a certain quest line for the entire server.

## Getting Started

Currently there are four ways to spawn NPCs: Using **RightCtrl + E** on either a Bed or Chair, the ``npcs_spawnrandom`` command, or from a configuration with the command ``npcs_spawnsaved``. Random NPCs will all be of the default NPC type none and will have random styles. They have no functionality other than adding clutter and life to the world. To customize NPCs and give them functions you need to first define them using the YAML file. The next section explains this process.

All NPCs act like players by respawning automatically upon death at their original spawn point. If you want an NPC to truly die you must set them up with either the "true death" command or in your yaml config for the NPC. During a true death NPC ragdolls do not disappear until a player removes them.

By default all NPCs will move around randomly, you can use the console command to make them stop moving, or edit the yaml config to have them stand still once spawned.

See all available commands in the Commands section below.

## Using the YAML file

There is a file that comes with the mod called **Example.VV.NPCS.yaml** in the config folder with examples. This file must be manually renamed to **VV.NPCS.yaml** to load in the mod, this prevents mod updates from overriding your configurations. This file is where you can define your custom NPCs to be able to spawn them in game. You can also update existing NPCs from these configurations without needing to remove them from the game. When you update an existing NPC it will get overwritten and lose all data previously tied to it.

This file does not automatically live update if you change it while the game is running, but there is a command ``npcs_reloadconfig`` to force reload it. If there is information missing from your NPC configurations it will be set to the default value and should not throw errors. If you see errors check you have used the correct data type for the config and do not have any spelling mistakes. The error should tell you which line is causing the problem.

The yaml file is local and will not sync to others. This file is not required on the server nor is it needed for the mod to function. It is purely for setup if you want to customize NPCs.

You can always reach out for support if you have trouble setting things up (see contributing at bottom).

## NPC Types

Acceptable types include: None, Quest, Sellsword, SlayTarget, Trader

### Quest

These NPCs can simply say something when interacted with or have the ability to accept an item in exchange for a reward item. To customize what your NPC does and says you will need to define certain data for it using the YAML file described above.

The building blocks of Quest NPCs is the list of "quests". The list of quests for your NPC will try to complete in the order they are defined. Only one quest can be active on an NPC at a time for a player. I recommend the last quest in the questline to be a "default text" you want your npc to say with no defined constraints. If all of the other quests in the questlines have constraints and none are met for a player, this last quest will be selected. This prevents your NPC from "doing nothing". Examples are explained below under their respective NPC type.

Simple is key. If your quests become long and complex this can introduce lag as data is sent from the server and the client selects the current available quest. The more NPCs you have in an area the worst this lag can get. If you experience issues try spreading your NPCs out to different areas or split quests between other NPCs.

#### Example 1

Here is an example of a pair of NPCs. Ragnar1 will give the key "ragnarbrave" when interacting with them, which Ragnar2 requires to tell you a secret. Ragnar1 will only tell you their secret once, so you better pay attention! The notRequiredKeys field is used to "skip" quests, once this key is present the quest will be non-qualifying and will try to select the next quest in the list. These keys are set to "Global" so only the first player to access the quest will be able to see the interactText for Ragnar1 and the rewardText for Ragnar2 (Unless you have another mod that changes this behavior).

Note both of these are set to stand still on spawn, which can be very helpful when setting things up.

```yaml
npcs:
  -
    id: Ragnar1
    name: Ragnar The Brave
    standStill: true
    type: Information
    quests:
      -
        text: "Here's some gossip about Ragnar The Bold!"
        notRequiredKeys: ragnarbrave
        interactKey: ragnarbrave
        interactKeyType: Global
      -
        text: "Did you say you had mead?"

  -
    id: Ragnar2
    name: Ragnar The Bold
    standStill: true
    type: Information
    quests:
      -
        text: "Have you seen Ragnar The Brave?"
        notRequiredKeys: ragnarbold
        requiredKeys: ragnarbrave
        rewardText: "He said that? Let me tell you a secret about Ragnar The Brave."
        rewardKey: ragnarbold
        rewardKeyType: Global
      -
        text: "I love a good mead."
```

#### Example 2

Here's an example of four NPCs that require you speak to them and complete their quests in order. Liv will give out a reward of a 2 star Flint Knife, which Vivica will accept and give the Rahshahs quest to bring him Deer Stew in exchange for Coins. The Jarl then rewards you for helping them all with an Iron Sword.

Liv is set up to give the player a knife when spoken to for the first time. Then this NPC will ask if you have completed the task until you receive the quest reward key "vivica2". Once both stages are completed Liv defaults to saying the last quest text. Since the ``interactKeyType`` is not specified here it will default to "Player". This means every player will be able to complete the quest.

Vivica is set up such that you must speak first to Liv before you can see her rewardText for the first quest. Then, when giving her the requested item in the second quest she will say the rewardText and move to the third quest which reminds you of your task. After you receive the "rahshahs2" key Vivica will default to the last quest in her list.

Similar to Vivica, Rahshahs will also ignore you until the quest granting the "vivica2" key has begun. He asks for DeerStew but does not remove this item from the player, changing his mind about needing it but rewarding the player anyway.

Finally, the Jarl will give you a reward for helping all his people once all the other NPCs quest lines are finished.

This can be a bit confusing, so try spawning these examples and interacting with them in game to see how they work.

```
The difference between the ``interactKey`` and ``rewardKey`` is important to note here since ``interactKey`` will always be given to the player when the NPC is spoken to, where the reward key will only be given when reward conditions are met.
```

```
There are two "keywords" you can use when setting up your NPC text: **{giveitem}** and **{reward}**, which will be automatically replaced with the corresponding prefab and amount so you don't have to type it out.
```

```yaml
npcs:
  -
    id: Liv
    name: Liv the Wise
    type: Reward
    quests:
      -
        text: "Can you give this to Vivica? She's too shy to admit she needs my help."
        notRequiredKeys: liv
        rewardItems:
          -
            prefabName: KnifeFlint
            quality: 2
        rewardKey: liv
      -
        text: "Did you bring that knife to Vivica?"
        notRequiredKeys: vivica2
      -
        text: "Beautiful day, is it not?"

  -
    id: Vivica
    name: Vivica
    type: Reward
    quests:
      -
        text: "I'm too busy to talk."
        requiredKeys: liv
        notRequiredKeys: vivica1
        rewardKey: vivica1
        rewardText: "Liv sent you? I guess I could use some help. I need a sharper knife."
      -
        text: "I need {giveitem}."
        requiredKeys: vivica1
        notRequiredKeys: vivica2
        giveItem:
          prefabName: KnifeFlint
          quality: 2
        rewardKey: vivica2
        rewardText: "My husband Rahshahs could use some help too, can you find him?"
      -
        text: "Did you find Rahshahs?"
        notRequiredKeys: rahshahs2
      -
        text: "I love the smell of the open seas."

  -
    id: MrHamHands
    name: Rahshahs
    type: Reward
    quests:
      -
        text: "What do you want? Go away."
        requiredKeys: vivica2
        notRequiredKeys: rahshahs1
        rewardKey: rahshahs1
        rewardText: "Vivica sent you? She didn't pay you!?"
      -
        text: "I need {giveitem}, I will give you {reward} in exchange."
        requiredKeys: vivica2
        notRequiredKeys: rahshahs2
        giveItem:
          prefabName: DeerStew
          amount: 1
          removeItem: false
        rewardItems:
          -
            prefabName: Coins
            amount: 10
        rewardKey: rahshahs2
        rewardText: "Thanks, but Vivica found me already, keep it! Sorry about my wife, this should be more than enough to cover your help."
      -
        text: "Thanks for your help."

  -
    id: Jarl
    name: Jarl Halsin
    type: Reward
    quests:
      -
        text: "Help my people and I will give you {reward}."
        requiredKeys: liv, vivica2, rahshahs2
        notRequiredKeys: thaneofvalheim
        rewardItems:
          -
            prefabName: SwordIron
            quality: 5
        rewardText: "You actually did it, I'm impressed!"
        rewardKey: thaneofvalheim
      -
        text: "Good day citizen."

```

### Sellsword

Not Finished.

### SlayTarget

Not Finished.

### Trader

Traders function like Haldor and the other vanilla traders. You can specify items for them to sell and buy using coins as currency. The "Texts" fields must exist for Traders, if left blank in your configuration they will appear as blank floating boxes in game.

#### Example

```yaml

  -
    id: Wildir1
    name: Wildir
    type: Trader
    tradeItems:
      -
        prefabName: Stone
        amount: 20
        cost: 5
        requiredKey: gave_pickaxe
      -
        prefabName: Wood
        amount: 20
        cost: 5
      -
        prefabName: AxeStone
        quality: 2
        cost: 200
    traderUseItems:
      -
        prefabName: PickaxeAntler
        rewardKey: gave_pickaxe
        removeItem: true
        text: "Hey thanks with this I can get some more stone."
    talkTexts:
      - "Beautiful day."
      - "The smell of pine makes me smile."
      - "I could really use a new pickaxe."
    greetTexts:
      - "Hello there!"
    goodbyeTexts:
      - "Always sharpen your axe before a long day!"
    startTradeTexts:
      - "Need something?"
      - "I have the best lumber!"
    buyTexts:
      - "Thanks, come again!"
    sellTexts:
      - "This is acceptable quality."
    notCorrectTexts:
      - "Umm..."
    notAvailableTexts:
      - "I already have what I need."
```

## NPC Style

There are fields for setting the NPC appearance using item prefab names. Anything not specified will be left blank or randomized. Skin and Hair color require all r,g,b values to be defined to override; they are float values between 0 and 1. Overriding colors can allow you to make some very strange characters. When left blank these colors will be randomized to any possible vanilla customization value. There are commands to get these values from existing NPCs, so you can spawn a group of randomly generated ones to find possible vanilla values rather quickly.

When spawning an npc from the yaml file it will automatically clear all usual default items that exists on the original prefab. This can cause them sometimes to not have attacks. To allow npcs to keep their original items and attacks you can use the ``GiveDefaultItems`` set to true. (This feature will get more attention and better support in the future).

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
    shoulder: CapeLinen
    shoulderVariant: 2
    utility: BeltStrength
    rightHand: KnifeFlint
    leftHand: Torch
    leftHandVariant: 1

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

  -
    id: Boar
    name: An Exquisite Specimen
    model: Boar
    giveDefaultItems: true
    type: Information
    interactText: "Feed me human!"

```

### Supported Models

This mod currently only supports the following vanilla models. If a model is not specified the type Player is assumed:

* Player
* Asksvin
* Boar
* Charred_Melee
* Deer
* Draugr
* Dverger
* Fenring
* Fenring_Cultist
* Ghost
* Goblin
* GoblinShaman
* Greydwarf
* Greyling
* Lox
* Neck
* Skeleton
* Troll
* Wolf

## Commands

You can spawn a random NPC on a chair or bed with **RightCtrl + E** (Subject to change in the future).

| Command            | Arguments  | Description |
| ------------------ | ---------- | ----------- |
| npcs_reloadconfig  |            | Reloads any changes to your yaml file |
| npcs_spawnrandom   | \[name\] \[Model\] | Spawns a random NPC (only for supported models) |
| npcs_spawnsaved    | \[id\]     | Spawns an NPC from the yaml config |
| npcs_remove        |            | Deletes the closest NPC, use with caution! |
| npcs_info          |            | Lists the closest NPC's items | 
| npcs_randomize     |            | Randomizes the closest NPC |
| npcs_set           | \[id\]     | Updates the closest NPC from the yaml config |
| npcs_set_move      |            | Updates the closest NPC to walk around |
| npcs_set_still     |            | Updates the closest NPC to stand still |
| npcs_set_sit       |            | Updates the closest NPC to sit in the closest chair |
| npcs_set_calm      | \[radius\] | Calms all hostile NPCs in the specified radius |
| npcs_set_truedeath |            | Updates the closest NPC to not respawn upon next death |
| npcs_set_faceme    |            | Makes the closest NPC turn toward you |
| npcs_get_skincolor |            | Returns the skin color of the closest NPC |
| npcs_get_haircolor |            | Returns the hair color of the closest NPC |

## Possible Future Improvements

* NPC option to drop items on death.
* NPC options to perform more kinds of things than just sitting/standing/walking.
* Randomly generated NPC names.
* More NPC model options.
* Graves with signs so you can properly mourn your fallen NPCs.
* Cooldown timer on completing quests.
* Player quest tracker in the UI.
* Easier ways to list/save/change/copy NPC information.
* Necromancy options for fallen NPCs.
* Translation options.
* More complex data:
  * Items/Attacks: ability to assign multiple weapons and attacks to one NPC.
* Traders: Ability to change currency type.
* Sound effects for things.

## Installation

This mod needs to be on both the client and server; the mod will enforce installation. Players without the mod will NOT be able to connect to the server.

If the mod is removed from the server after NPCs have been generated they will disappear.

## Changelog

Moved to new file, it will appear as a new tab on the thunderstore page.

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).