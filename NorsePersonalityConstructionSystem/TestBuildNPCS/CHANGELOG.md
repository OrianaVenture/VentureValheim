

## 0.1.0

* Overhauled the config system!!! Readme has been updated, please read it again!
    * Automatic upgrade of existing NPCs to the new system as they are loaded in game.
    * Can now add multiple quest stages on one NPC.
    * Can now set multiple rewards for a quest.
    * Can now set animation states for NPCs.
    * Can now set player keep the quest give item upon reward.
    * Can now give NPCs "NpcTalk" values to some NPCs so they can say things without interacting with them.
* Can now create Trader NPCs with custom store items.
* Interact and Give hover texts now use vanilla localization keys.
* Player now performs interact animation when giving an NPC an item.
* Humans NPCs will now properly punch things when given no weapons, hiya!
* Manually set rotations should now persist and reapply to NPCs.
* NPCs now have a short delay before respawn on death.
* Improvements to existing commands:
    * npcs_set_calm should fully reset alertness and agitation.
    * npcs_set_faceme should now work as intended and save state.
    * npcs_remove now accepts optional range field to remove multiple in an area at once.
    * npcs_set_still and npcs_set_move now accepts optional animation field.

## 0.0.7

* Update for game version 0.219.13.

## 0.0.6

* Fixed a bug where NPC ragdolls were assigned to their vanilla counterparts.

## 0.0.5

* Fixed a compatibility bug with RRRCore npcs due to not cleaning up the vanilla ragdoll after manipulating it.
* Finished implementing ragdolls for all NPCs.
* Added removal effects to Player NPC ragdoll (the magic poof).
* Increased NPC discovery range for commands from 2 to 3, should make it easier to use them.

## 0.0.4

* Hotfix for example yaml file being incorrect for Boar. No code changes.

## 0.0.3

* Support for more models, see readme for the list.
* Fixed key configs not matching if they have any capitalization.
* Fixed NPC AI again due to eyes not set correctly, they really should attack stuff now.
* New config option GiveDefaultItems for spawning from yaml file.
* Changed example yaml file name to prevent mod update from overwriting values.

## 0.0.2

* New commands npcs_set_faceme, npcs_info, npcs_randomize.
* Rewards are now thrown forward by NPCs, and they perform an animation if possible.
* Fixed utility items from configs not setting.
* Fixed NPC AI to not suck, they attack stuff now.
* Removed configurations for back items, will revisit this later.
* NPCs show name above text when speaking now.

## 0.0.1

* First Release.
