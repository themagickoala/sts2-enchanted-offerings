# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## Overview

This is a Slay the Spire 2 mod: **Enchanted Offerings**.

Randomly applies enchantments to card rewards, inspired by the STS1 Chimera Cards mod.

## Architecture

### Entry point

`ModEntry.cs` registers a run-state hook subscriber via `ModHelper.SubscribeForRunStateHooks`. This returns `EnchantedOfferingsHook.Instance` for every run, which receives card reward hooks without needing a relic in the player's deck.

This mod does **not** use Harmony patches or `ProcessFrame`. All card reward interception goes through the `RelicModel`-style virtual hooks on `EnchantedOfferingsHook`.

### ModConfig integration

`ModConfigBridge.cs` (copy from the ModConfig repo) handles optional ModConfig integration via reflection. `ModConfigBridge.DeferredRegister()` is called from `Initialize()`. If ModConfig is not installed, all `GetValue` calls return their fallback and the mod works normally.

Config keys: `enabled`, `modChance`, `commonWeight`, `uncommonWeight`, `rareWeight`, `rarityBias`, `modifyStarter`, `modifyInstant`, `modifyShop`.

### Key files

| File | Purpose |
|------|---------|
| `ModEntry.cs` | Mod entry point; registers run hook and ModConfig |
| `EnchantedOfferingsHook.cs` | `AbstractModel` subclass receiving card reward hooks |
| `ModConfigBridge.cs` | ModConfig reflection bridge (copy from ModConfig repo) |
| `Settings.cs` | Static settings store; read by all hooks |
| `EnchantmentPool.cs` | Weighted random enchantment selection logic |
| `Enchantments/` | New `EnchantmentModel` subclasses |
| `pack/images/enchantments/` | Icon PNGs (one per enchantment, `<id_lowercase>.png`) |
| `EnchantedOfferings.csproj` | Build config, MSBuild targets, mod metadata |
| `local.props` | Local path overrides (git-ignored) |

### Enchantment icons

Icon path resolved by the game: `res://enchantments/<id_lowercase>.png` (or `res://enchantments/beta/<id_lowercase>.png`). Place PNGs in `pack/images/enchantments/` — the PCK build picks them up automatically.

Icons must be **60×60 PNG with a transparent background**.

### Hook pattern

`EnchantedOfferingsHook` overrides the same `RelicModel` virtuals used by `FresnelLens` and `SilverCrucible`:
- `TryModifyCardRewardOptionsLate` — combat card rewards
- `ModifyMerchantCardCreationResults` — shop cards
- `TryModifyCardBeingAddedToDeck` — instant-obtain cards (Pandora's Box equivalents)

## Build

### Prerequisites

Copy `local.props.example` to `local.props` and set:
- `<STS2GamePath>` — path to your STS2 installation directory
- `<GodotExePath>` — path to Godot 4.5.1 Mono executable (required for PCK export)

### Build command

```bash
PATH="$HOME/.dotnet:$PATH" ~/.dotnet/dotnet build EnchantedOfferings.csproj
```

The build automatically:
1. Compiles C# to `EnchantedOfferings.dll`
2. Exports `EnchantedOfferings.pck` (enchantment icons and manifest) via Godot
3. Copies both to the game's `mods/EnchantedOfferings/` directory
4. Packages `dist/EnchantedOfferings/` and `EnchantedOfferings.zip` for release

## Critical reminders

- **`_Ready()` and `_Process()` are never called on mod DLL classes.** This mod intentionally avoids both — it uses run-state hooks only.
- **New enchantment types are auto-discovered** — any class extending `EnchantmentModel` in this assembly is found by `ReflectionHelper.GetSubtypesInMods<AbstractModel>()`. No registration needed.
- **`CardModel.Enchantment` is a single slot.** `CanEnchant()` rejects already-enchanted cards.
