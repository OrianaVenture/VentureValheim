

## 0.1.0

* Overhauled the config system!!! Readme has been updated, please read it again!
    * Supports more complex questlines on a single NPC, can now add multiple quest stages.
    * Can now set multiple reward items on a single quest stage.
* Support for Trader NPCS.
* Interact and give texts now uses vanilla localization keys.
* New option to not remove items for give quests, now can just check for presence.
* Humans will now properly punch things, hiya!
* Player now performs interact animation when giving npc an item. todo test
* Improvements to commands: 
    * Calm command now functions better.
    * Fixed NPCS not restoring a manually set rotation after using faceme command. TODO test
    * Remove command now supports an optional range field to remove multiple npcs at once.

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
