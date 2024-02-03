# QualityCompany

![logo](/assets/logo.png)

## Features

### HUD improvements

#### Held items scrap value

++

#### Shotgun ammo counter

++

### Terminal Commands

#### sell

QualityCompany adds a few commands that let you sell scrap directly from your ship.

You must be landed at The Company Building to use these commands.

These can only be executed whilst landed at the Company Building.

- `sell all` will sell all the scrap on your ship
- `sell quota` will attempt to sell enough scrap to perfectly match the amount needed to fulfill quota
- `sell <amount>`, where `amount` is any positive integer, will attempt to sell enough scrap to perfectly match that amount
- `sell 500` will try to sell scrap equal to 500 scrap value
- `sell <item_name>`, where `item_name` is any portion of an item name to match against. All matching items will be listed
  - `sell whoop` will find all Whoopie Cushions on the ship

**Note:**
These commands use an approximation for finding a perfect match, it may not find one even if one exists (especially early game where you do not have many scrap of low value).\

#### target

++

#### launch

++

#### door

++

#### lights

++

#### tp

++

#### time

++

#### hack

++

## Config

- An ignore list for items to ignore, syncs from host to clients. defaults to "shotgun,gunammo,airhorn,gift"

## Development

You will need:

- Local development setup for [UnityNetcodePatcher by EvaisaDev](https://github.com/EvaisaDev/UnityNetcodePatcher)
- An IDE, Visual Studio / Rider / Visual Studio Code

## TODO

- Scrap selling animation
- scan <item> in ship
- character health hud display
