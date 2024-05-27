# Venture Quest

A questing framework for servers. Create your own NPCs and give them purpose!

## Features

Create your own custom npc characters and assign them different functions including:

* Random movement and actions such as sitting in chairs (Not finished)
* Give the Player information, or simply say something when interacted with.
* Give fetch quests to players and reward them for returning an item.
* Give players quests to speak to other npcs.
* Hireable and will fight for the player! (TODO)
* Consequences for killing your npcs like raids, or denial to give quests!
* Advantages for killing an npc, such as opening a certain quest or reward.

## World Advancement & Progression

This mod will pair nicely with WAP's private key system if you want players to have individual progress for setting questing keys. Otherwise the key system will use global configurations and all progress on the server will advance together. 

## Spawning NPCs

There are two ways to spawn NPCs: Random and from configurations. Random NPCs will all be of the default npc type none and will have random styles. They have no functionality other than adding clutter and life to the world. To customize NPCs and give them functions you need to first define them using the YAML file.

All NPCs act like players by respawning automatically upon death at their original spawn point. If you want an NPC to truly die you must set them up with either the "true death" command or yaml option. During a true death NPC ragdolls do not disappear until a player removes them.

By default all NPCs will move around randomly, you can use the console command to make them stop moving, or edit the ymal config to have the stand still once spawned.

## Using the YAML file

There is a file that comes with the mod called **VQ.NPCS.yaml** in the config folder with examples. This file is where you can define your custom NPCs to be able to spawn them in game. This file does not automatically live update if you change it while the game is running, but there is a command to force reload it. If there is information missing from your npc configurations it will be set to the default value and should not throw errors. If you see errors check you have used the correct data type for the config. You can always reach out for support if you have trouble setting things up (see contributing at bottom).

### NPC Types

#### Information

These NPCs simply say something when interacted with. You can use the RequiredKeys and NotRequiredKeys fields to lock receiving the information. Use the InteractKey field to record the player speaking with them. Use the RewardKey to record the player successfully speaking to them. This difference will be important to note, especially for the next npc type.

Here is an example of a pair of NPCs. Ragnar1 will give the "ragnarbrave" when interacting with him, which Ragnar2 requires to tell you a secret. Both of these are set to stand still on spawn.

```yaml
npcs:
  -
    id: Ragnar1
    name: Ragnar The Brave
    standStill: true
    type: Information
    defaultText: "Did you say you had mead?"
    interactText: "I have lots of gossip to share!"
    interactKey: ragnarbrave

  -
    id: Ragnar2
    name: Ragnar The Bold
    standStill: true
    type: Information
    defaultText: "I love a good mead."
    requiresKeys: ragnarbrave
    interactText: "Let me tell you a secret about Ragnar The Brave."
```

#### Reward

These NPCs have all the functionality of an information npc plus the ability to accept an item in exchange for a reward item and/or key.

There are two "keywords" you can use when setting up your npc text: **{giveitem}** and **{reward}**, which will be automatically replaced with the corresponding amount and prefab so you don't have to type it out (also makes updating the configs easier).

You can use two different methods to control how often a quest is completable: rewardlimit or notRequiredKeys. A rewardLimit of 10 will allow the quest the be completed 10 total times; when reaching 0 it will lock the quest. By default rewardLimit is set to -1, which allows for unlimited completion. The second way is to use notRequiresKeys, which when set to the same key as the interact or reward key will lock the quest. To have a quest only completable by one person use a rewardLimit of 1, to have to completable by all players once you can use the Progression mod private key system and the notRequiresKeys setting.

##### Example

Here's an example of four NPCs that require you speak to them and complete their quests in order. Liv will give out a reward of 10 wood, which Vivica will accept and give the Rahshahs quest to bring him DeerStew in exchange for coins. The Jarl then rewards you for helping them all.

Liv is set up in a way that only the first player (5 players when using private keys from WAP) there will get the wood and the rewardText and interactText. Other players when talking to this npc will still be able to receive the "liv" key but will not be given the wood reward and will only see the defaultText once the limit is reached. Again, note the difference between rewardKey and interactKey. To properly lock up the quest line you should use rewardKey. Since Liv does not have any requiredKeys nor require a giveItem to give a reward you will never see the InteractText used, so it is not specified here.

Vivica is set up such that you must speak first to liv before you can see her interactText. When giving her the requested item she will say the rewardText. After this quest is completed Vivica will return to saying her defaultText.

Similar to Vivica, Rahshahs will also require a key to unlock the interactText, then return to saying his defaultText once the quest is completed.

Finally, the Jarl will give you a reward for helping all his people. Since the Jarl does not have a giveItem requirement the interactText will be displayed until the requiredKeys conditions are met. The only time you will see the defaultText is after receiving your reward.

This an be a bit confusing, so try spawning these examples and interacting with them in game.

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
    rewardItem: Wood
    rewardItemAmount: 10
    rewardLimit: 5

  -
    id: Vivica
    name: Vivica
    type: Reward
    requiredKeys: liv
    notRequiredKeys: vivica
    defaultText: "I'm too busy to talk."
    interactText: "Liv sent you? I guess I could use some help. I need {giveitem}."
    giveItem: Wood
    giveItemAmount: 10
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
    rewardItem: Coins
    rewardItemAmount: 100
    rewardKey: thaneofvalheim

```

#### Sellsword

TODO: Hire these NPCs to fight for you

#### SlayTarget

TODO: Kill this lil fella for rewards and consequences

```yaml
npcs:
  -
    id: Cain1
    name: Cain
    type: SlayTarget
    defaultText: "Please don't kill me"
    defeatKey: defeated_cain
    trueDeath: true
    standStill: true
    model: Player
    modelIndex: 0
    hair: Hair10
    legs: ArmorRagsLegs
```

### NPC Style

There are fields for setting the NPC appearance using item prefab names. Anything not specified will be left blank or randomized. Skin and Hair color require all r,g,b values to be defined to override; they are float values between 0 and 1. Overriding colors can allow you to make some very strange characters. When left blank these colors will be randomized to any possible vanilla customization value. There are commands to get these values from existing npcs, so you can spawn a group of random generated ones to find good values rather quickly.

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

| Command             | Arguments | Description |
| ------------------- | --------- | ----------- |
| vq_reloadconfig     |           | Reloads any changes to your yaml file |
| vq_spawnrandomNPC   | \[name\] \[Model\] | Spawns a random npc (Player or Skeleton only right now) |
| vq_spawnsavedNPC    | \[id\]    | Spawns an npc from the yaml config |
| vq_setnpc           | \[id\]    | Updates the closest npc from the yaml config |
| vq_setnpc_move      |           | Updates the closest npc to walk around |
| vq_setnpc_still     |           | Updates the closest npc to stand still |
| vq_setnpc_sit       |           | Updates the closest npc to sit in the closest chair |
| vq_setnpc_truedeath |           | Updates the closest npc to not respawn upon next death |
| vq_getnpc_skincolor |           | Returns the skin color of the closest npc |
| vq_getnpc_haircolor |           | Returns the hair color of the closest npc |

## Possible Future Improvements

* NPC option to drop items on death.
* NPC options to perform more kinds of things than just sitting/standing/walking.
* Randomly generated NPC names.
* More NPC model options.
* Cooldown on completing quests.
* Graves with signs so you can properly mourn your fallen NPCs.
* Player quest tracker.
* Translation options.
