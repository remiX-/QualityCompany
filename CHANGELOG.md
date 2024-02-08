# v1.2.2: Dev & mod quality of life

- Improved dev & mod api quality of life
  - Terminal commands can be setup via a `[TerminalCommand]` attribute
  - Modules can be easily added in a Static or Instance manner via the `[Module]` attribute
  - see the example plugins under `samples/`

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
