# AdvancedCompany

Sell From Terminal adds a few commands that let you sell scrap directly from your ship.

You must be landed at The Company Building to use these commands.

## Commands

### sell

- `sell all` will sell all the scrap on your ship
- `sell quota` will attempt to sell enough scrap to perfectly match the amount needed to fulfill quota
- `sell <amount>`, where `amount` is any positive integer, will attempt to sell enough scrap to perfectly match that amount

Note: This mod uses an approximation for finding a perfect match, it may not find one even if one exists.\
This is necessary to keep the game performing well even when processing hundreds of items. If a perfect match isn't found, the received credits will just be slightly over the requested amount.

## target

## Misc

These are just some additional ones I added whilst playing around with the terminal to which I just decided to keep them in.

## Config

- An ignore list for items to ignore, syncs from host to clients. defaults to "shotgun,gunammo,airhorn,gift"
- [to remove] How much allowance to grant the algorithm when calculating a perfect match. Ex: Setting it to 5 will make `sell 50` also accept 51, 52, 53, 54 and 55
