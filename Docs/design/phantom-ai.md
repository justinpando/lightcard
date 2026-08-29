# Phantoms — AI Opponents Built From Real Decks

A **phantom** is a snapshot of a real player's deck, piloted by AI. Phantoms are
the roguelike's entire opponent supply, which means the mode's content scales
with the playerbase instead of with hand-authored encounters — and every player
leaves a trace in the world whether or not they're online.

## Capture pipeline

Deck snapshots enter the pool from four sources:

| Source | Tier | Notes |
| --- | --- | --- |
| Seeded decks | 1 | Starter decks + designer-built decks; guarantee the pool is never empty, and are the entire pool in offline/v1 builds |
| Ranked multiplayer matches | 2 | Winning deck of each match is snapshotted (account-level opt-out). Deduplicated by content hash |
| Completed Descent runs | 3 (boss) | The run deck, tagged with score — "merges with the void" |
| Failed 0-Coherence runs | Shadow | Corrupted variants that haunt the owner and their friends |

### Snapshot format

Extends the existing name-keyed `DeckSaveData` (decks already serialize as card
name lists) with the metadata the AI and matchmaker need:

```json
{
  "schemaVersion": 1,
  "phantomId": "ph_8f3a...",
  "ownerHandle": "JPando",
  "source": "ranked | run_victory | run_shadow | seeded",
  "capturedAt": "2026-08-29T00:00:00Z",
  "cards": ["Shieldbearer", "Shieldbearer", "Navigator", "..."],
  "cardSetVersion": "2026.08",
  "archetypeSpread": {"Expedition": 26, "Heart": 14},
  "stats": {"gamesWon": 12, "gamesPlayed": 20, "runScore": 8420},
  "aiHints": {"personality": "formation", "mulliganKeeps": ["Eager Recruit"]}
}
```

- `cardSetVersion` handles balance patches: snapshots referencing changed cards
  are re-validated on fetch; cards that no longer exist are substituted by the
  importer's nearest same-archetype, same-cost card (name-keyed saves make this
  substitution table mandatory anyway).
- Owner handles are display-only and can be anonymized ("A Wandering Seeker") per
  the owner's privacy setting.

### Pool service

A thin HTTP service: `POST /phantoms` (validated against the card set — the
deterministic engine doubles as the validator), `GET /phantoms?tier=2&power=…`
returning a batch of ~30 snapshots the client caches locally. The roguelike
never needs a live connection mid-run; it fetches a run's worth of phantoms at
run start and falls back to the local cache, then the seeded pool. Phantom
battles are therefore fully offline-tolerant.

**Power banding:** each snapshot gets a computed power score (curve quality, card
rarity/AL weights, owner winrate) so Act I doesn't serve a tuned ladder deck.
Tier + power band, not raw MMR, decides where a phantom can appear.

## Why the AI is tractable here

Piloting arbitrary decks sounds like the hard version of the problem, but three
properties of LT Cards make it manageable:

1. **Perfect-information board, hidden hands.** The AI only reasons over public
   state plus its own hand — no opponent-modeling needed to be credible.
2. **Deterministic engine** (`LightCard.Core`): the AI can clone `GameState`,
   apply a candidate command, and score the resulting events. The AI is a search
   over the real rules, so it automatically "knows" every keyword and never
   plays illegally — new cards need zero AI code.
3. **Small action space.** A turn is a short sequence from: play card (≤ ~10
   playable after energy/AL gating × ≤ 9 target spaces), one Shift, declare
   attacks, replace a card, end turn. Greedy sequential selection over that
   space is fast.

## AI architecture: three tiers

### Tier 1 — Heuristic (ships with phase 2)

Rule-scored action selection, no lookahead. Enumerate legal commands, score each
with a static evaluation, take the best, repeat until end turn.

Evaluation terms (weights are the personality knobs):

- Material: sum of friendly (power + life) minus enemy, cost-weighted
- Board: lane control (unblocked lanes are worth face damage), front/back row
  placement fit (Melee forward, Ranged back), space-effect alignment
  (own units on Verdant/Vista, enemies on Desert/Bramble)
- Tempo: energy spent this turn, cards drawn
- Curve: value of holding vs. replacing a card (drives the replace-to-ramp
  economy decision)
- Face: damage to opponent life, weighted up sharply when lethal is in range

### Tier 2 — One-ply simulation (target for Descent v1 elites)

Same evaluation, but each candidate action is scored by actually simulating it in
a cloned engine state (so triggered effects, Bond Breaks, and pushes are priced
correctly), and end-of-turn states are scored after simulating the opponent's
forced responses (attacks only). This is where phantoms stop walking into
Thorny Hedges.

### Tier 3 — Search (bosses, post-v1)

Time-boxed MCTS over the turn's action sequence using the engine for rollouts,
with hand sampling for the hidden information. Only bosses and Shadow phantoms
get this; it's a tuning luxury, not a requirement.

## Personality: making a phantom play like its owner

A phantom should feel like the deck's archetype, not like one generic bot with
different cards. Personality = an evaluation weight preset, selected from the
snapshot's dominant archetype (overridable by `aiHints.personality`):

| Archetype | Preset | Expressed as |
| --- | --- | --- |
| Heart | `relentless` | Face damage and attack triggers up-weighted; life preservation down-weighted; happily breaks its own Bonds |
| Atelier | `control` | Card advantage and Ability value up; holds Charm durability; avoids overextending into board wipes |
| Expedition | `formation` | Adjacency and row bonuses up; advances as a unit; values Equips |
| Garden | `patient` | Space-effect alignment and healing up; movement tricks; delays commitment |
| Tower | `attrition` | Defensive lines, on-death value, discard pressure; wins slow |
| Ocean | `chaotic` | Transformation and sacrifice combos up; adds evaluation noise deliberately |

Two cheap authenticity tricks: seed the phantom's mulligan/replace choices from
`aiHints.mulliganKeeps` (derived from the owner's actual keep-rates when
available), and log each phantom's real games to auto-tune its power score.

## Difficulty scaling

Scale with *decision quality*, never with stat cheats — phantoms are real decks
and must stay believable:

- Evaluation noise: Act I phantoms pick from the top-3 scored actions weighted,
  bosses always take the top line
- Lookahead: Tier 1 → 2 → 3 by encounter type
- Lethal awareness: low tiers only check for lethal on their own turn; high
  tiers also play around yours
- Mulligan quality: random keep → heuristic keep

The one sanctioned "cheat" is thematic and visible: **Shadow phantoms** get an
explicit, displayed corruption buff (+1/+1). Players forgive labeled monsters;
they never forgive a bot that top-decks suspiciously.

## Implementation order

1. `LightCard.Core` playable headless (roadmap phase 1)
2. Tier-1 heuristic + personality presets — this alone makes the Field scene a
   game, using starter decks as seeded phantoms
3. Local phantom pool (own saved decks, content-hash dedupe) — the Descent v1
4. Snapshot schema + pool service + power banding (phase 4, alongside accounts)
5. Tier-2/3 AI, Shadow generation, `aiHints` capture from ranked play
