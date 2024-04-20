# Venture Debugger

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Not a mod, but a de-bugger for stuff. What stuff you ask? See the readme.

## Features

I created this project as my "fix all" for some common issues people report when using mods. I will update this project with more fixes are the opportunity presents itself.

Currently it fixes:

**Pickable Timer Corruption**
```
Pickable.UpdateRespawn: ArgumentOutOfRangeException: Ticks must be between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks.
```

**World Modifier Corruption**
```
ServerOptionsGUI.TryConvertModifierKeysToCompactKVP: ArgumentException: An item with the same key has already been added.
```

## Installation

This mod is client side only and has no configuration options. Does not need to be on the server.

## Changelog

### 0.0.3

* Added corruption fix for world modifiers, previously included in World Advancement & Progression.

### 0.0.2

* Modified Pickable.UpdateRespawn to fully catch all exceptions and not allow passthrough.

### 0.0.1

* Added fix for Pickable.UpdateRespawn ArgumentOutOfRangeException.

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).