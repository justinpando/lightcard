# Rules v1 — Engine Rulings

`Assets/Scripts/Core` (LightCard.Core) implements the first playable slice of the
rules: 20 cards (10 Expedition, 10 Garden — see `CardCatalogV1.cs`), chosen
because their printed sheet text is implementable verbatim. The design sheet and
overview deck leave some points ambiguous or contradictory; this file records the
v1 rulings so they're deliberate decisions, not accidents. Everything here is
data/tuning, revisitable without re-architecture.

Tests live in `CoreTests/` — `cd CoreTests && dotnet run` (no Unity needed).

## Board and coordinates

- 3 lanes (x: 0–2) x 6 rows (y: 0–5). Player 0 owns rows 0–2 (frontline row 2),
  player 1 owns rows 3–5 (frontline row 3). Units never leave their owner's half.
- *Adjacent* = orthogonal (front, behind, sides), per the glossary.
- A *row* is the 3 spaces at one y; a *lane* is the 6 spaces at one x.

## Turn structure

1. **Start of turn:** +1 max energy, refill energy; clear Flux and per-turn
   flags; Verdant regen (1) for your damaged units; Auto-Advance (front-most
   first); draw 2.
2. **Main:** any number of card plays and attacks, one Shift, one Replace, in
   any order.
3. **End of turn:** your end-of-turn triggers fire (front-most first), then the
   opponent's turn starts.

Openers: 20 life, 3-card opening hand, player 0 simply goes first (no draw
compensation yet — a balance question for later).

## Economy rulings

The docs describe two energy systems (overview: "+1 neutral energy per turn";
sheet notes: energy only from replacing cards). **v1 uses both, merged:** +1 max
energy automatically each turn, *plus* Replace — once per turn, discard a card
to permanently gain +1 max/current energy and +1 Affinity of the discarded
card's archetype. Replace does not draw a replacement; the card is the price.
Affinity is tracked but does not yet gate card plays (no v1 card has an AL
requirement).

## Combat rulings

- Default units are melee: a unit may only attack if no friendly unit or charm
  is in front of it in its lane. Attacks hit the nearest enemy unit or charm in
  the lane; an empty enemy lane means the attack hits the opposing player.
- **Armor** reduces attack damage only. **Retaliate** ("deal 1 damage to
  attackers") is effect damage — it ignores armor — and only fires if the
  defender survives.
- **Pierce X** continues the attack X spaces past the first target at full
  power (each victim applies its own armor).
- **Push/Pull collisions** deal 1 damage to both units and cancel the move;
  being pushed against the field edge does nothing. Push after an attack only
  triggers if the target survived.
- Moving (Shift) and attacking are exclusive per unit per turn (the glossary's
  *Agile* keyword will lift this; no v1 card has it). Auto-Advance movement is
  free — it does not consume the unit's action.
- **Asleep** units can't attack or move; any damage wakes them.
- **Flux**: units can't attack the turn they're called (*Rush* will bypass;
  no v1 card has it).

## Space effect rulings

- One effect per space; applying a new one overwrites the old.
- **Verdant**: your damaged units on it heal 1 at the start of your turn.
- **Brambled**: a unit takes 1 damage when entering *or* leaving the space,
  from any cause (Shift, advance, push).
- **Vista**: units called to the space gain +1/+1 permanently; the effect
  persists (not consumed).
- Rose Beast's "per Brambled/Verdant space" counts the whole board, both halves.

## Deliberately deferred (not designed away)

Keywords the v1 set doesn't need yet: Ranged in play (implemented, no card uses
it), Rush, Agile, Parry, Evade, Flying, Guardian, Equip, Spirit Bind, Melee/
Ranged blocking nuances beyond front-most, discard pile/graveyard tracking,
Affinity-Level gating, mulligans, deck-out rules (drawing from an empty deck
currently just does nothing).
