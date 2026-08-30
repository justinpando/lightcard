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

## Combat is fully automatic; Activations are the manual unit action (rules-v3, designer ruling)

There are **no manual attacks**. All combat resolves in the end-of-turn sweep:
every eligible unit attacks, front-most first (melee blocking, Flux, sleep,
moved-non-Agile, 0-power all apply), before end-of-turn triggers. The
`AttackCommand` no longer exists; `GameEngine.ResolveAutoAttacks` is the one
combat entry point (public for tests).

The manual per-unit action is **Activate**: cards may carry an
`ActivateCost` — pay it once per turn (not while Asleep or in Flux; charms can
activate the turn they arrive since they have no Flux) to fire their
OnActivate effects. First activatable: Dinner Bell ("Activate (1): Call a
Snarling Hound to a random nearby space", from its sheet note). In the match
view, clicking a selected unit again Activates it.

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

## Overdraw burns (rules-v2)

Drawing with a full hand **burns** the drawn card: it leaves the deck and is
discarded (`CardBurned` event). Without this, a player who hoards a full hand
stops depleting their deck and the fatigue clock never arrives — 60-turn
agent games ended with one side untouched. With burn, every game has a clock,
and card-draw effects become a real tradeoff: tempo now, fatigue sooner.

## Movement and defense keywords (rules-v2)

- **Agile**: may move and attack in the same turn, in either order. For
  everyone else move/attack exclusivity is now enforced both ways (attacking
  then Shifting was an implementation gap in v1).
- **Parry X**: prevents combat damage X times per turn (armor is not consumed;
  riders like Push and Retaliate still apply). Charges refresh at the owner's
  turn start, so they cover the enemy's whole turn.
- **Evade X**: prevents any damage instance — combat, ability, poison, bramble
  — X times per turn. Currently only granted temporarily (Dancer).
- **Temp grants** ("until the start of your next turn"): expire when the
  owner's next turn starts.
- **Shift triggers** (Duelist, Dancer) fire on voluntary Shifts of that unit
  only — not auto-advance, not forced movement, not the swap partner.
- Call-watchers (Squad Leader adjacent, Skilled Armorer in-front) fire when
  the owner calls a unit or charm to a qualifying space, buffing the arrival.
- **Martial Musician's aura** grants Auto-Advance while adjacent (8-way
  "Nearby"); auras never affect their own source.
- **Navigator** draws once per space-effect *card effect*, even if it painted
  several spaces; conditions like Frontline now apply to triggered effects.
- **Windstriker** ruling: the sheet's "Base Atk 3" on 1/4 stats is read as
  printed attack 3 (3/4 Agile).

## Balance state (2026-08-29, burn rule, 40-card decks, 6 seeds per pairing)

With Garden (21 cards), Expedition (16), Atelier (12) kits at their current
completeness: **Expedition beats Garden 5-1** (close: Expedition finishing at
10-17 life with fatigue running on both sides), **Garden and Atelier split
3-3**, **Expedition beats Atelier 6-0**. Expedition is the front-runner at
11-1 overall — but per the "complete the kits before balancing" principle,
Atelier (the least complete: no Scholar, Quick Sketch, rods, or equips) gets
its completion pass before any Expedition numbers move. The shared
**equipment system** (Equip charms) is the biggest missing cross-archetype
mechanic. Agent sweeps still overstate spreads; human play decides magnitude.

## Full build-out rulings (catalog v3: 113 cards, all six archetypes)

- **Equip charms**: a friendly non-charm entering the charm's space (Shift,
  call, push, pull) consumes it and gains its OnEquip bestowals — stats,
  Armor/Pierce/Parry/Resist grants, **Heavy** (immune to Push/Pull), or whole
  granted abilities (Vinewhip's Strike: Pull). Consumption is removal, not
  destruction: no OnDestroy triggers.
- **Guardian**: takes attack damage in place of friendly units beside or
  behind it (asleep guardians don't; no guardian chains). OnGuard fires after.
- **Spirit Bind (Heart)**: spirits play onto a friendly unit and occupy no
  space. The spirit soaks ALL damage (before Evade); at absorbed damage >=
  its printed life the bond breaks, firing OnBondBreak with the host as
  source. Rebinding overwrites silently. Hosts gain the spirit's printed
  Parry/Resist/Rush while bonded. Dead Man Walking's life-drain is modeled
  as self-poison.
- **Inferno**: +1/0 to occupants; 1 damage at the end of the occupant
  owner's turn.
- **Discard (Tower)**: effect discards are random (deterministic RNG) — a
  player-choice UI can come later. Overdraw burns count as discards for
  OnOwnerDiscard watchers. Grotesque Mirror triggers at the opponent's turn
  start while its lane is unblocked. Seer's Guillotine charges per card drawn.
  Player healing (Mourner's Altar) is uncapped.
- **Transforms**: the unit's card identity swaps; damage, bonuses, and
  statuses persist (Prideful/Guilt-Wracked loop).
- **Flooded**: non-charms called there roll a random small keyword (+1 Armor,
  Pierce, Parry, +1/0, or +0/1 — the same table as Flicker/Amalgam rolls).
  **Desert**: -1/-1 at the occupant owner's turn start (Sand Shark immune);
  X-bound units die at turn start off their effect. Tidal Wave hits the
  nearest enemy per lane and floods the impact space. Mirage swaps every
  Flooded/Desert space. Fractal/Amalgam consume adjacent *friendly* units
  (random one / all), shedding Flux ("Rush if so").
## COMPLETE catalog (161 cards) — provisional rulings and adaptations

Every named sheet card is implemented. Where sheet text was ambiguous or
required per-card-instance state, these adaptations were made (each needs a
designer yes/no):

- **Scorched** (undefined on the sheet): units standing on it cannot be
  healed. **Confirmed by the designer 2026-08-30** (the "dead ground" variant
  was rejected as too oppressive with lane-wide sources).
- **Player powers** (designer ruling 2026-08-30): the sheet's Powers table
  gives players TWO powers — **Shift (1 energy)** and **Clear (2 energy:
  remove a space effect)** — and they share ONE power action per turn: Shift
  or Clear, not both. Clear works on any space, both halves. In the match
  view, right-click a space effect to Clear.
- **Rugged**: shifting in costs +1 energy; damaged survivors on it are knocked
  one space toward their own backline.
- **Attenuating Rod**: "reduce a random hand Ability's cost" → "your NEXT
  Ability costs 1 less" (stacking) — avoids per-hand-card state.
- **Thunder Rod**: "all other targets" → 1 damage to every enemy unit.
- **Lose Hope**: two targets → "return target unit to its owner's hand".
- **Bauble Merchant**: triggers on ANY equip attaching, not only Valuable Coin.
- **Valuable Coin** (two sheet versions): 0-cost Equip charm, Bestow +0/2.
- **Crystal Amplifier**: Resist 1 only (adjacent-splash half deferred).
- **Adaptive Armature**: charm-accepting Equip bestowing +0/2 (its cost
  discount deferred).
- **Spirit Caller / Sword of Damocles**: "end of turn"/"beginning of next
  turn" both resolve at the caster's next turn start via the pending queue.
- **Spirit of Reprisal**: only combat breaks have a source to punish.
- **Message in a Bottle**: draw + 1 self-damage per friendly death (charges
  deferred). **Focus Form**: tutors the first matching card, no bonus keyword.
- **Reckoning**: each sacrificed unit blasts the space directly in front.
- **Dispersal**: stats scatter to the victim's owner's nearby units.
- **Frozen** space is defined by the sheet but no card applies it — dormant.
- **Forge** (copying equipment) remains the single unimplemented card.
- New keywords live: Overpower (excess kill damage rolls onto the unit
  behind, Give Your All), Flying (attacks skip blockers), Immobile,
  Reflects, Amplify, Regen (static aura form), next-call blessings,
  positional call discounts, delayed effects, player-facing Activations.

## Full-catalog round-robin (3 seeds per pairing, 45 matches)

Wins of 15: **Expedition 10, Ocean 10, Atelier 9, Tower 7, Garden 5,
Heart 4.** Every archetype wins games; zero illegal AI commands. Top tuning
questions for human play: Expedition/Ocean's edge, Heart's bond economics
(better but still last), Garden vs Ocean 0-3.

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
