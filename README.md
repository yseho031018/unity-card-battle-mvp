# Unity Card Battle MVP

A small Unity 2D single-player card battle prototype inspired by trading card battle games.

This project is currently focused on a narrow MVP: card data, deck draw, hand UI, card selection, monster summoning, spell use stubs, and trap setting.

## Unity Version

- Unity 6000.4.8f1
- uGUI
- TextMesh Pro
- Universal Render Pipeline 2D

## Current Features

- `ScriptableObject`-based card data
- `CardType` enum
  - Monster
  - Spell
  - Trap
- Deck shuffle and opening hand draw
- Hand UI with generated `CardView` prefabs
- Card display
  - Name
  - Type
  - Cost or Level
  - ATK / DEF
  - Description
- Card hover and selection feedback
- Monster Zone with 5 slots
- Trap Zone with 5 slots
- Turn panel with draw button
- Hand layout auto-scaling for larger hands
- Spell use flow
  - Logs a placeholder message
  - Removes the card from hand
  - Adds it to the internal graveyard list
- Trap set flow
  - Places the trap into Trap Zone
  - Removes the card from hand
- Editor tools for UI and test card generation

## Not Implemented Yet

These are intentionally out of scope for the current MVP:

- Battle system
- Chain system
- AI opponent
- Real card effects
- Animation
- Graveyard UI
- Card artwork support

## Editor Tools

Use these Unity menu items:

```text
Tools > Create Default Card Prefab
Tools > Create Test Card Data
```

`Create Default Card Prefab` creates or updates:

- Canvas
- EventSystem
- CardView prefab
- Hand root
- Monster Zone
- Trap Zone
- Turn panel
- Action buttons
- Required managers

`Create Test Card Data` creates or updates the current 10 test cards.

## Test Cards

### Monsters

- Iron Knight
  - ATK 1600
  - DEF 1200
  - LV 4
- Fire Dragon
  - ATK 2400
  - DEF 1800
  - LV 6
- Stone Golem
  - ATK 1000
  - DEF 2500
  - LV 5
- Wind Falcon
  - ATK 1400
  - DEF 1000
  - LV 3
- Shadow Assassin
  - ATK 1800
  - DEF 800
  - LV 4

### Spells

- Power Boost
  - Placeholder: monster gains +500 ATK
- Healing Light
  - Placeholder: recover 1000 life
- Draw Scroll
  - Placeholder: draw 2 cards

### Traps

- Mirror Shield
  - Placeholder: negate an attack
- Pitfall Trap
  - Placeholder: destroy an attacking monster

## How To Run

1. Open the project in Unity.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Run:

```text
Tools > Create Default Card Prefab
Tools > Create Test Card Data
```

4. Select `DeckManager` in the scene.
5. Add the test card assets to `Starting Deck`.
6. Press Play.

## Current Gameplay Flow

1. Deck is shuffled.
2. 5 cards are drawn.
3. Select a monster card and click an empty Monster Zone slot to summon it.
4. Select a spell card and click `Use Card`.
5. Select a trap card and click `Set Trap`.
6. Click `Next Turn / Draw` to advance the turn and draw 1 card.

## Suggested Next Steps

- Rename `SampleScene` to `BattleScene`
- Add card artwork field to `CardData`
- Add card preview on hover
- Add Graveyard UI
- Implement simple spell effects
- Implement basic trap activation structure
- Add simple battle phase later

