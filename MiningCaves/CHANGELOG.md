## 0.2.1

* Changed the Copper/Tin Cave large copper node to many smaller nodes, will take effect only on freshly generated (or reset) caves, backwards compatible.

## 0.2.0

* Added more enemies to the caves, some will now respawn at 30 second to 1 minute intervals.
* Tweaked both caves slightly, the Copper/Tin Cave is now visually distinct from the Troll Cave.
* Added feature to remove wishbone ping from silver nodes.

## 0.1.5

* Courtesy update for Ashlands, new logo. No feature changes.

## 0.1.4

* Preliminary update for Jotunn version 2.19.2+

## 0.1.3

* Fixed issue where patches may have applied before the initial server configuration syncing event. This made some server configurations not apply correctly.
* Fixed LockTerrainIgnoreItems config not applying to the correct items when used.
* Fixed Obsidian falling out of caves.
* Internally removed clearing Jotunn cache, this is now a feature in version 2.17.0, update Jotunn to upgrade this mod.

## 0.1.2

* Fixed copper node in cave painting the surface terrain.
* Tweaked silver cave spawn rates, about 40 should be placed consistently now on world generation.
* Fixed Tin falling out of caves.

## 0.1.1

* Fixed an issue where items like ores would sometimes not display (needed to clear mesh cache for mocking).
* Added missing file watcher for updating configurations without a server restart.

## 0.1.0

* First release