# Rules v2 — Economy, Endings, and Affinity

Supersedes the economy and game-end rulings in [rules-v1.md](rules-v1.md);
everything else in v1 stands. Decided with the designer 2026-08-29.

## Economy: Replace is the only ramp (designer ruling)

v1 merged two energy systems (+1 max energy per turn *and* Replace). That was a
misreading of the docs — **there is no automatic energy gain**. The sheet's
version is canonical:

- Players start at 0 energy, 0 max energy.
- **Replace** (once per turn): discard a card → +1 max energy, +1 current
  energy, +1 Affinity of the discarded card's archetype. No replacement draw.
- Current energy refills to max at the start of your turn ("playing a card
  reduces current Energy until end of turn").
- Temporary energy (e.g. Cosmic Flower) raises current energy only and washes
  out at the next refill.

Consequence: energy can only ever grow by burning cards, so "power now or
options later" is the core economic decision every turn. Max energy on turn N
is at most N.

## Fatigue: empty-deck draws hurt

Each draw attempted from an empty deck deals escalating damage to that player
instead: 1 for the first missed draw, 2 for the second, 3 for the third, and so
on (counter never resets). Applies to turn-start draws and effect draws alike.
Emits `FatigueDamage` then the usual `PlayerDamaged`.

Rationale: guarantees every game terminates. AI-vs-AI testing showed passive
standoffs were a stable equilibrium with no game-end pressure (3 of 5 matches
hit the 4000-command cap). With ~30-card decks and 2 draws/turn, fatigue ends a
stalled game around turn 25.

## Affinity Level gating

A card may only be played if the owner's Affinity in the **card's own
archetype** is at least the card's **AL requirement**.

- Default requirement: **cost − 1** (minimum 0). Cards can override this in
  their data (`CardDefinition.AffinityRequirement`); the sheet should
  eventually grow an AL column.
- Since Replace grants +1 AL per discard, a mono-archetype deck is never gated
  (AL tracks max energy). A two-archetype deck sees its expensive cards delayed
  roughly two turns per color split. That is the point: splashing costs
  commitment, not cards.
- **Deferred to v3:** AL *scaling* (card text that improves at AL thresholds —
  "deal 2→3 damage at Level 5", "Draw 1 becomes Draw 2"). Gating first; it is
  the balance lever (delay a strong card instead of nerfing it).

## Second-player compensation

Going first is an advantage (extra half-turn of initiative). The second player
draws **one extra card in their opening hand** (4 instead of 3). In this
economy a card is also potential energy and affinity, so one card is meaningful
but modest compensation. Provisional — revisit with winrate data from
MatchRunner seed sweeps once both players are piloted well.

## Keyword policy (designer ruling)

No keyword merges. Every glossary keyword is mechanically distinct and stays in
the design space — deckbuilding breadth is a goal. Implementation is staged:
ship the keywords the current card slice needs, keep the rest in the back
pocket, and introduce them as their archetypes come online. Nothing is cut.

## Atelier slice rulings (v2 catalog: 12 cards)

Ability damage is a new pipeline, distinct from attacks:

- **Ability damage ignores Armor** (consistent with the v1 Retaliate ruling
  that effect damage bypasses armor). **Resist X** reduces ability damage only,
  never attack damage.
- **Primed** (space effect): ability damage dealt to a unit standing on a
  Primed space is +2, and the boost consumes the effect (the space clears).
  Priming an empty space keeps the charge until someone stands there.
- **Lances** (`LaneDamage`) sweep the target lane starting from the caster's
  backline, hitting **every unit in the path — friend or foe** (keep your
  spellwork lanes clear). The traveling damage changes per unit hit
  (Diminishing 3/−1, Magnifying 1/+1) and the sweep stops when it reaches 0.
  A hit a victim fully Resists still counts toward the change. Lane sweeps
  never hit players.
- **Erasure** removes the space effect first, then deals its damage — so
  erasing a Primed space does not get the Primed bonus.
- **"The enemy lane"** (Lightning Rod, Combat Bellows) = the nearest enemy
  unit or charm in the charm's own lane, scanning from the enemy frontline
  back — the same targeting as an attack. Combat Bellows cannot push charms
  (immobile).
- **Master Painter** primes the space of the unit it strikes; attacks that hit
  the player prime nothing.

## Auto-attack at end of turn (designer ruling)

When a player ends their turn, every one of their units that can still legally
attack does so automatically, front-most first (the same legality as a manual
attack: melee blocking, Flux, sleep, moved/attacked flags, 0-power all apply).
Manual attacks earlier in the turn remain possible — attacking early lets you
resolve combat before playing more cards — and any unit that already attacked
or moved is skipped by the sweep. Auto-attacks resolve before end-of-turn
triggers.

## "Attune" — the in-world term for Replace

Player-facing UI calls the Replace action **Attuning** a card: you dissolve a
card to gain +1 max energy and +1 Affinity of its archetype (matching the
roguelike's Attunement sites). The engine keeps `ReplaceCardCommand` as the
code name. In the match view: drag a card onto the energy dial (or the Attune
drop area) to Attune it.

## Garden offense rulings (Pin Down, Pin Prick)

- **Pinned**: the unit cannot Shift and its Auto-Advance does not fire. Forced
  movement (Push/Pull) still works. The pin expires when the unit's owner ends
  their turn — so pinning on your turn denies one full turn of movement.
- **Poison X**: the unit takes X damage at the start of its owner's turn.
  Permanent and stacking; no cure exists yet. Poison is status damage: it
  ignores Armor *and* Resist (it is neither an attack nor a targeted ability).
- Charms cannot be pinned or poisoned.
- Agent evaluation prices pending poison damage and pin tempo into unit value.

## Garden terrain-offense package (v2 catalog: +6 cards, Garden = 18)

Implemented from the sheet: Constant Gardener (bramble-Pierce clause deferred),
Rose Knight (bramble-Parry clause deferred until Parry exists), Magic
Fertilizer, Fertile Soil, Hedge Maze, Entangling Vines. New machinery rulings:

- **CallUnit** (tokens): summoned to empty spaces on the owner's half only.
  "Each Verdant space" means each empty own-half Verdant space; "random spaces"
  draw through the seeded RNG (deterministic). Tokens arrive in Flux as usual.
- **Pull** moves a unit **one space** toward the effect's owner; collisions and
  half-boundary blocking exactly as Push. Pulling a row moves every non-charm
  unit in it, friend or foe — pulled units entering or leaving Brambled spaces
  take the usual damage (this is Garden's armor-ignoring offense).
- **Static buffs can require a space effect**: an EffectDef with
  Trigger.Static and a SpaceEffect applies only while its source stands on
  that effect (Constant Gardener's +1/0 on Bramble).
- Entangling Vines resolves bramble first, then the pull — so a unit pulled
  off the freshly brambled space is damaged leaving it.

## Balance state (2026-08-29 agent sweeps, 6 seeds per pairing)

A rock-paper-scissors triangle has emerged: **Expedition beats Garden (6-0),
Garden beats Atelier (6-6 → 6-0 with Rose Knight), Atelier splits Expedition
(3-3)**. The triangle is the right shape for three archetypes; both sharp
corners are steeper than we'd want. Notes for the human-play pass:

- Garden's wins over Expedition come from surviving to fatigue; in losses
  Expedition finishes at 17-20 life untouched. Armor doesn't answer
  Spearbearer (Pierce + adjacency aura) — the standout Expedition card.
- Agent sweeps exaggerate spreads (no mulligan, no adaptation, full-catalog
  piles); treat these as direction, not magnitude.
- Likely real fix for Garden vs aggro: its unimplemented movement half
  (Duelist, Dancer, Windstriker — needs Agile/Parry/Evade and until-next-turn
  buff infrastructure, which Heart's spirits will want anyway).

## Rejected (designer ruling)

- **Standing positional bonuses** (the sheet's "back row Armor 1, front row
  +1/0" note): no always-on field effect — inelegant. Anti-turtle pressure
  comes from fatigue and from cards, not from a standing rule.

## Open questions (not yet ruled)

- **Charm placement**: charms competing with units for the 9 board spaces is
  intended tension for Tower, but may self-jam charm-heavy Atelier. Designer
  direction to explore: an additional **aura rank** — per-lane (and/or per-row)
  slots behind the back line where a charm/enchantment affects its whole
  row/column instead of occupying a space. See exploration notes in the design
  discussion; needs prototyping before a ruling.
- **Mulligan**: none yet.
- Agent evaluation weights were retuned for the replace-only economy (ramp
  valued above holding a card) so agents actually develop; personalities need a
  fresh balance pass once more archetypes exist.
