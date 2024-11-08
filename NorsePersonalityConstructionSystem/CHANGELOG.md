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
