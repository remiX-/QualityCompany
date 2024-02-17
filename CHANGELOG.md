# v1.3.2: Total Scrap UI Config

- Introduced config item to disable **Total Scrap UI** separate from item scrap UI

# v1.3.1: Additional Networking fixes + InfoMonitor fix

- Detached Save File from NetworkBehaviour instance entirely
  - This and the changes in 1.3.0 should be more solid now
  - Hosts with networking disabled will still save the same, making use of the games' `save number`
  - Clients with networking disabled will have their own `local` client save, which is not tied to a `save number`
  - **That said, this probably still needs more testing**
- Fixed a bug with `InfoMonitor` calculating "NEEDED" amount incorrectly in a certain case - thanks @throwitaway99 ([#4](https://github.com/remiX-/QualityCompany/issues/4#issuecomment-1940570052))
- Some class and file rename and reworks

# v1.3.0: Networking

- Add a ping hud display (networking only), configurable
  - This is untested with 3 or more people, please log an issue if anything is crazy
  - (worst case) Disable the feature in the config if it causes issues!
- **[UNTESTED]** (hopefully) fully implemented ability to disable Networking operations in config
  - This will cause everything to be done CLIENT-SIDE ONLY
  - This WILL cause desync issues with `sell` & `target` commands, *but* should recover on action of deposit desk eating the items
  - Let us know if anything breaks from this please :)

# v1.2.3: Mod Compatibility & Experimental fixes

- Fixed an issue where Scrap UI will have a null reference if the HUD `iconFrames` changes in size after initializing
- Fixed an issue with `view` experimental command overriding `view monitor` command

# v1.2.2: Dev & mod quality of life, scrap total, experimental features

- Improved dev & mod api quality of life
  - Terminal commands can be setup via a `[TerminalCommand]` attribute
  - Modules can be easily added in a static or instance manner via the `[Module]` attribute
  - see the example plugins under `samples/`
- Scrap UI: Add total scrap tally to first item slot
- Added 2 **experimental** commands: vs, view. This is disabled by default.

# v1.2.1: Improved Mod Compatibility, force inventory UI refresh

- Improved inventory mod compatibility ui updates
- Introduced new config for force updating all inventory ui values (disabled by default)
- Introduced new config inventory startup delay (default: 4.5 seconds)

# v1.2.0: Feature Configurations, Inventory UI, README image links

- Fixed README image links to work for Thunderstore page
- Combined Loot & Credit monitors into 1
- Updated ScrapValue Inventory UI to a TextMeshProUGUI with base game font
- Added config options for various different features
- (Hopefully) fixed Host vs Client mod config syncing
- Moved 'hack' terminal command to its' own 'debug' category

# v1.1.0: Some fixes, terminal commands updates + efficiency

- Update README
- Update ScrapValueUI - Position is calculated dynamically taking into account rotation on z-axis.
- Update Release build script
- GameEvents: to include more player actions: DropAllHeldItems, DiscardHeldItem, Death
- AdvancedTerminal: update command & sub command text processing to be more efficient
- new `sell target` command to just sell amount needed as per `target` calculation
- Config: Add "ShowDebugLogs" config option (should hopefully be client side)
- OvertimeMonitor:
  - Update for a case where `groupCredits > set target` which would just show `0` needed (but need to sell actual quota still...)
  - Added saving of target set (host)

# v1.0.1: Remove some message logging

- Removed some unnecessary message logging

# v1.0.0: Initial release

- Woow
