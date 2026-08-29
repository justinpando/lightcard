# Light Card Tactics — Design Docs

LT Cards is a 1v1 tactical card game on a 6x3 field (each player controls a 3x3 half).
Six Archetypes represent psychological motivations; players build Affinity with
archetypes by playing their cards. The core game design (cards, keywords, space
effects, affinity economy) lives in the
[LT Cards sheet](https://docs.google.com/spreadsheets/d/1yQOt8G8o4LON2B3nm3Oreq9GnfaWEn6Nx5Iourz_4Pw)
and the LT Cards Overview deck on Drive. These docs expand the project with the
two game modes and the systems they share.

| Doc | Contents |
| --- | --- |
| [roguelike-mode.md](roguelike-mode.md) | Single-player run structure: the Descent through the Collective Unconscious, deck drafting, encounters, bosses, meta progression |
| [phantom-ai.md](phantom-ai.md) | Phantoms — AI-piloted snapshots of real players' decks — capture pipeline, deck snapshot format, AI architecture, difficulty tiers |
| [multiplayer-mode.md](multiplayer-mode.md) | 1v1 multiplayer: architecture, netcode approach, matchmaking, and how ranked play feeds the phantom pool |

## The two modes, one loop

- **Multiplayer (The Waking Duel):** ranked and casual 1v1 with decks built in the
  existing deck editor. Every completed ranked match snapshots the winning deck
  (opt-out available) into the **phantom pool**.
- **Roguelike (The Descent):** a single-player run through the Collective
  Unconscious. The opponents are **phantoms** — real players' decks piloted by AI.
  Winning a run submits *your* run deck to the pool as a boss-tier phantom, so the
  two modes feed each other: multiplayer supplies the roguelike's opponents, the
  roguelike gives multiplayer decks a second life and gives lapsed players a
  presence in the living world.

This closes the loop sketched in the original design notes: *"When you win a run,
your deck merges with the void and becomes a potential mini boss for yourself and
other players."*

## Shared foundation: the rules engine

Both modes, and the phantom AI, depend on one piece of tech that must come first:
a **deterministic, headless rules engine** in a plain C# assembly
(`LightCard.Core`) with no UnityEngine dependency in its logic types.

- **State:** `GameState` = both fields (3x3 grids of spaces with effects and
  occupants), hands, decks, energy/affinity pools, life totals, RNG seed.
- **Input:** a small closed set of `Command`s (PlayCard, Shift, Attack,
  ReplaceCard, EndTurn). Commands are validated then resolved.
- **Output:** an ordered list of `GameEvent`s (UnitCalled, DamageDealt,
  SpaceEffectApplied, BondBroken, ...) that the Unity view layer consumes to
  animate. The engine never touches a GameObject.
- **Effects:** card text compiles to data following the grammar already drafted in
  the sheet — **Condition / Trigger / Target / Effect** — plus a keyword table
  (Armor, Pierce, Push, Bond, etc.). No per-card C# classes.
- **Determinism:** same seed + same command list = same events. This one property
  buys AI simulation (phantoms think by running the engine forward), server-side
  validation for multiplayer, replays, and automated balance testing.

## Roadmap

| Phase | Deliverable | Notes |
| --- | --- | --- |
| 0. Hygiene | Buildable project, bugs fixed | Done on this branch. Remaining: revoke the leaked Google OAuth credentials, upgrade off Unity 2020.3 (EOL) before netcode work |
| 1. Rules engine | `LightCard.Core` + tests; ~20 cards from Expedition and Garden playable headless | First slice done on this branch: `Assets/Scripts/Core` + `CoreTests/`, rulings in [rules-v1.md](rules-v1.md) |
| 2. Playable match | Field scene wired to the engine; play vs. a Tier-1 heuristic AI | First slice done: `Assets/Scripts/Field/` MatchContext + view layer, playable vs. HeuristicAgent in `Field.unity`. Remaining: event-driven animation, history log, match end/restart flow |
| 3. The Descent | Run map, drafting, local phantom pool (starter decks + own saved decks as stand-in phantoms) | Fully offline-capable |
| 4. Services | Accounts, phantom upload/fetch, then multiplayer | Multiplayer last: it needs the engine, the UI, and the service layer all mature |

The ordering is deliberate: the roguelike ships value before any server exists
(phase 3 works offline against a local pool), and by the time multiplayer arrives
the engine has been hardened by thousands of AI-vs-AI and player-vs-AI games.
