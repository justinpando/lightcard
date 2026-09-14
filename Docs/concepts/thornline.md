# Thornline — Concept Design

> **Working title.** Not Light Card Tactics. This is a separate game concept
> parked in this repo because it is the same designer's next idea and because
> several systems already built here — a data-driven effect grammar, a headless
> deterministic sim, a standalone test runner, a simulate-and-score heuristic
> agent — transfer almost directly to it (see [Reuse from LT Cards](#reuse-from-lt-cards)).
> Nothing here is committed to; it is a design to argue with.

## The pitch

A session-based online action RPG with MOBA structure and Guild Wars 1's build
economy: pick a **primary** and **secondary** class, fill **eight ability slots**
from the combined pool, equip a weapon, armor and two accessories, and bring a
small **NPC retinue** you have outfitted yourself. Matches are fought on maps
whose terrain is *material*, not decoration — a Verdurist grows a wall of
brambles to seal a lane and a trellis to climb a cliff nobody expected to be
climbable; an Emberwright sets the tall grass her enemy is hiding in on fire and
takes the concealment away from the whole quarter of the map for the next forty
seconds.

The three things that make it not-another-MOBA, in priority order:

1. **Terrain is a first-class combatant.** Abilities write to the map, and the
   map's state feeds every other ability's behavior.
2. **Build-crafting over character-picking.** You do not pick a hero with a fixed
   kit; you author one from ~8 slots out of hundreds of abilities, locked at
   match start.
3. **You bring a squad, not just a body.** Creep waves are replaced by units you
   chose, equipped and trained.

## Design pillars

- **The map remembers.** Every skill should ideally ask "what is the ground doing
  here?" before it asks "what is the health bar doing?" A match should end with a
  visibly different map than it started with.
- **Horizontal power.** Time played buys you *options*, never *numbers*. See
  [The power ceiling law](#the-power-ceiling-law) — this is the load-bearing rule
  and the one most likely to be eroded by well-meaning later decisions.
- **Squads are strategy, not APM.** A retinue is a standing decision (composition,
  gear, stance), not a micro contest. The best player must not be the fastest
  clicker.

## The laws

Four non-negotiables. Everything below is negotiable; these are the ones that,
once broken, turn the concept into a different and worse game.

| Law | Why |
| --- | --- |
| Ranked play normalizes gear and unlocks to maximum | A competitive game where the ladder rewards grinding is a grinding game with a ladder attached |
| One elite ability per bar, hard cap | GW1's single best limiter. It makes 8 slots a real budget instead of a list of your favorites |
| Terrain state is authoritative and shared | No client-side cosmetic fire. If it looks burning it *is* burning, for both teams, on the server |
| Retinue commands are stance-level, on a cooldown | The moment direct unit micro outperforms stance play, this becomes a bad RTS |

## Classes and the skill bar

Six at launch. Each is defined first by **what verb it performs on the world**,
and only second by a damage role — this is deliberate, because the terrain pillar
dies if only two classes can touch terrain.

| Class | World verb | Role sketch | Primary attribute (primary-only) |
| --- | --- | --- | --- |
| **Verdurist** ("green mage") | **Grows** — vines, brambles, tall grass, climbable trellises | Control, area denial, vertical routes | *Rootedness* — your grown terrain lasts longer and regrows once |
| **Emberwright** | **Ignites and consumes** — burns foliage, spreads fire, denies areas | Damage over time, zoning, counter-concealment | *Combustion* — your fires spread one ring further |
| **Tidecaller** | **Changes state** — floods, freezes, douses, conducts | Mobility surfaces, hard counters to fire, chain shock setup | *Current* — refunds resource when a combo you set up triggers |
| **Warden** | **Breaks and blocks** — shatters terrain, raises cover, body-blocks | Frontline, peel, objective soak | *Bulwark* — armor scales with number of adjacent blocked cells |
| **Beastbinder** | **Inhabits** — beasts scout, flush, and hold ground | Retinue specialist, map pressure | *Kinship* — +2 retinue points, and one beast's abilities appear on your own bar |
| **Cogwright** | **Installs** — turrets, ziplines, oil slicks, scout drones | Siege, vision, infrastructure, steampunk constructs | *Fabrication* — installations cost less and can be repaired |

The skill bar:

- **8 slots**, chosen out of match, **locked at match start**. No mid-match
  respec, no item-shop rebuild.
- **At most 1 elite.** Elites are the build's thesis statement.
- **Attributes**: a shared pool (~200 points) spent across your two classes'
  attribute lines with GW1-style escalating costs, so ranks 1–9 are cheap and
  10–12 are expensive. You can be broad and shallow or narrow and deep, never
  both. The **primary attribute** in the table above is available *only* when
  that class is your primary — this is what stops "everyone runs X secondary"
  and gives the primary choice weight beyond skill access.

### Multiclassing

- **Primary** grants: full skill pool (elites included), armor class, and the
  primary attribute.
- **Secondary** grants: its **non-elite** skills and its attribute lines. No
  primary attribute, no elites.
- Secondary is swappable out of match, free, at any time. Primary is chosen per
  character.

So `Verdurist/Tidecaller` is a terraformer who floods first and grows reeds in
the water; `Tidecaller/Verdurist` is a water mage whose primary attribute refunds
resource every time her ice shatters, using vines only as cheap setup. Same skill
access, different games.

## Elements, surfaces and the combo grammar

This is the system the whole concept rests on, so it is specified as a closed
data table rather than as per-ability special cases — the same discipline LT
Cards applies to card text.

**Surfaces** (one per terrain cell, mutually exclusive):
`Bare` · `Foliage` · `Bramble` · `Water` · `Ice` · `Oil` · `Mud` · `Burning` · `Scorched`

**Volumes** (occupy the space above a cell, stack with any surface):
`Smoke` · `Steam` · `Pollen` · `Gas`

**Elements** (what an ability applies): `Fire` · `Frost` · `Shock` · `Water` ·
`Growth` · `Force`

### The interaction matrix

| ↓ Element applied to → | Foliage | Water | Oil | Ice | Mud | Burning |
| --- | --- | --- | --- | --- | --- | --- |
| **Fire** | ignites → `Burning`, **spreads cell to cell** | → `Steam` volume (vision block) | → `Burning`, instant, whole slick | melts → `Water` | — | intensifies, +1 spread ring |
| **Frost** | wilts → `Bare` | → `Ice` (slide movement) | brittle — shatters on `Force` | thickens, +duration | hardens → fast walkable | douses → `Scorched` |
| **Shock** | — | **chains to every unit touching the pool** | ignites → `Burning` | chains + stun | grounded, halved | — |
| **Water** | soaks — fireproof ~8s | deepens → `Deep Water` (swim, drops non-swimmers' aim) | spreads the slick outward | melts → `Water` | — | douses → `Bare` |
| **Growth** | thickens → `Bramble` | → `Reeds` (walkable *and* concealing) | — | — | grows at double rate | — |
| **Force** | flattens — **reveals anyone concealed** | displaces units | spreads the slick | shatters → `Bare` + slow | — | scatters embers to adjacent cells |

`Scorched` is the pressure valve: nothing grows on it for ~20s. It is how an
Emberwright permanently answers a Verdurist within a fight, and why Verdurists
want Tidecallers.

### Rules that fall out of the matrix

- **Concealment**: `Foliage`, `Reeds` and `Smoke` conceal. You are revealed if you
  attack, if `Force` flattens the cell, or if it catches fire — so hiding is a
  real but burnable resource, and counter-concealment is a whole strategic axis
  rather than a single "reveal" item.
- **Climbing**: `Growth` applied to a *wall* surface produces a `Trellis` —
  climbable for ~15s. Maps must therefore be authored with deliberate
  "sometimes-vertical" walls, and the flank routes of a map are partly
  player-created. This is the single most distinctive traversal idea here and
  also the most expensive (see [Technical spine](#technical-spine)).
- **Fire spreads**, it is not an aura. It propagates as a cellular automaton over
  flammable surfaces at a fixed tick, which means fire is *committed* — you can
  start one you cannot control, and both teams' plans get rewritten.

## The Retinue

Each player brings a small squad. This is the part of the concept most likely to
go wrong, so it is constrained hard.

### Budget

Retinue points scale **inversely to team size**, so total unit count on the field
stays roughly constant and a 5v5 never becomes a 20-unit RTS:

| Mode | Retinue points per player | Typical squad |
| --- | --- | --- |
| 5v5 Skirmish | 3 | one beast, or three drones |
| 3v3 Arena | 5 | a beast + a scout, or one heavy construct + a drone |
| 1v1 Duel | 8 | a real squad |
| PvE explorable | 8 (party fills empty player slots with retinue) | this is the GW1 henchmen slot |

Unit costs: scout drone 1 · beast or skirmisher 2 · heavy construct 3 ·
named/trained companion 4.

### Control model

Stances plus one directed command, on a cooldown. Four stances: **Follow** ·
**Hold** (guard a point) · **Hunt** (engage nearest enemy in radius) ·
**Screen** (interpose between you and the nearest threat). One `Command` key
issues "go there / attack that" with a ~6s cooldown, so a directed order is a
decision, not a stream of inputs.

**Beastbinder** buys depth here rather than getting it for free: extra command
charges, direct control of one beast, and that beast's abilities surfaced onto
the player's own 8-slot bar (where they compete for slots like everything else).

### Squads replace creep waves

Lane pressure is player-authored: you push with units you built, and losing them
costs tempo (base respawn timer, ~45s) rather than gold. Map income is
**territory-based** instead — which is how the retinue system and the terrain
system are made to need each other, below.

### Squad units are equipped, like you

Every retinue unit has the same four equipment slots as a player — **weapon,
armor, accessory, accessory** — plus a **2-slot ability bar** (not 8: a squad of
three 8-slot builds is a second game you did not ask to play). A drone with a
spotter scope and a smoke charge is a different object than a drone with a
welding torch, and neither is stronger, which is the point.

## Equipment

Four slots, per the concept: **Weapon · Armor · Accessory · Accessory**. All four
are *horizontal* — they change what you do, not how big your numbers are.

| Slot | What it decides |
| --- | --- |
| **Weapon** | Your basic attack's shape (staff = slow pierce line, scepter = fast single target, bow = arcing, sword = melee arc) **and gates some abilities** — bow attacks need a bow. Carries 1 weapon-native skill |
| **Armor** | Defense *profile*, not defense *amount*: armor class trades off (e.g. +vs fire / −vs shock). 2 rune slots (attribute ranks, capped) + 1 insignia (a situational passive) |
| **Accessory ×2** | Where builds get strange. A **Focus** shapes your resource economy; a **Sigil** rewrites one ability's behavior — *"your vines also grow a trellis on the nearest wall"*, *"your steam volumes damage"* |

Sigils are the intended long-tail content: each one is a small, legible rules
patch to a single ability, which means new content ships as data and the combo
matrix stays the only global system.

## Map and modes

**Skirmish (signature, 5v5).** Three lanes, but the objectives are **terrain
control sites** — a Grove, a Well, a Forge — captured not by standing on them but
by putting the *right surface state* around them: grow the Grove, flood the Well,
scorch the Forge. Holding sites pays team income and unlocks retinue reinforcements.

This is the design's keystone: it makes the terrain system the win condition
rather than a garnish, and it makes every class's world verb a capture tool, so
no composition is locked out of objectives.

**Arena (3v3).** No lanes, one contested terrain feature, bigger retinues.

**Explorable zones (PvE, co-op 1–4).** GW1's structure: persistent-ish zones
where you unlock abilities, sigils, and beasts, with retinue units filling empty
party slots. This is where acquisition lives, and the reason it can exist without
poisoning the ladder is the next section.

## Progression and the power ceiling

### The power ceiling law

- Maximum *effective power* is reachable in roughly **10 hours**.
- **Ranked matches normalize everything to maximum** — gear quality, attribute
  points, and access to every ability and sigil in the game. What you actually
  own affects casual, PvE and cosmetics only.
- Everything earned past the ceiling is **horizontal**: new abilities, new sigils,
  new retinue units, new beasts, cosmetics.

Stated bluntly because it will be under constant pressure: every retention
consultant, every mid-project "players need a reason to log in", every
monetization pass will propose a small vertical exception. The concept does not
survive one.

### What you actually earn

Unlocking abilities is the progression curve, and it is also the balance risk:
a newer player in *casual* facing someone with 400 abilities unlocked is in a
worse position even with equal gear. Mitigations: ranked normalizes (above),
casual is matched partly on unlock breadth, and a rotating free "full access"
loadout set exists at all times.

## Readability and counterplay

The hardest tension in the concept: **hidden 8-slot builds destroy MOBA
counterplay.** In League you know what a Zed does. Here you know someone is a
Verdurist/Emberwright, which narrows it to a few hundred possibilities.

Three-part answer:

1. **Silhouette telegraphs capability.** Class, weapon and armor class are visible
   and constrain the space hard — a staff Verdurist cannot be doing melee burst.
2. **Seen skills are logged.** Any ability used on or near you is added to a
   per-opponent panel on the scoreboard for the rest of the match. Scouting is a
   mechanic, and information advantage is something a retinue drone can buy.
3. **Terrain is honest.** The most impactful actions in the game — the burning
   field, the bramble wall, the trellis — are large, visible and permanent-ish.
   The unreadable part of a build is never the part that decides the map.

Lobby: classes are visible at pick, bars are not. Deliberately *not* a full
pick/ban — the counter-play happens on the map, not in the lobby.

## Technical spine

### The terrain grid is the architecture

Represent terrain as a **uniform cell grid** (~0.5 m cells) carrying
`{surface, volume, timers, owner}` — roughly 2 bytes per cell. A 120 m × 120 m
map is ~240 × 240 = 57,600 cells, ~115 KB of authoritative state, replicated as
deltas. Everything else falls out of this:

- **Combos** are a lookup into the matrix, evaluated per cell.
- **Fire spread** is a cellular automaton ticking at ~5 Hz over the same grid.
- **Pathing** uses **flow fields over the grid**, not navmesh rebuilds — vines and
  walls flip cell costs instantly with no bake, which is exactly the operation a
  navmesh is worst at. This is the single most important technical call in the
  document.
- **Retinue AI** reads the same grid, so squad units understand fire and brambles
  for free rather than needing a parallel representation.

### The rest

| Concern | Approach | Risk |
| --- | --- | --- |
| Networking | Server-authoritative, fixed ~30 Hz tick, client prediction on own movement only | Standard, well-trodden |
| Vision / concealment | **Server-side** visibility culling — hidden enemies are not sent to the client at all | Non-negotiable; client-side fog is a wallhack |
| Climbing | Traversal volumes spawned from `Trellis` cells, with authored wall tagging | **High.** Dynamic verticality is where animation, pathing and level design all get expensive at once |
| Replays / debugging | Fixed-tick + input log replay | Cheap if the tick is fixed from day one, near-impossible to retrofit |
| Engine | Unity 6 is fine and matches existing team knowledge; a networked action game wants a real stack on top (dedicated-server Netcode for GameObjects, or Photon Fusion) | Unreal is the stronger default for traversal/animation; the honest tiebreaker is team familiarity |

### Reuse from LT Cards

| From this repo | To Thornline |
| --- | --- |
| `EffectDef` grammar (Condition/Trigger/Target/Effect) as *data, not code* | Ability definitions, sigil rules, and the combo matrix — same discipline, and the reason content can ship without engineers |
| Headless sim with no engine references | A headless combat sim for balance sweeps and squad AI training |
| `CoreTests/` standalone runner | Same pattern: the combo matrix is exactly the kind of system that needs a few hundred cheap assertions |
| `HeuristicAgent` (simulate-and-score over cloned states) + `AgentPersonality` | Retinue AI. Stance-based squad behavior is a much easier target than a full card game opponent |
| `MatchRunner` headless agent-vs-agent sweeps | Automated balance passes on abilities and the combo matrix |

## What is actually hard

Stated plainly, because the concept is a genuine multi-year team project and
each of these has killed a similar game:

1. **Scope.** A networked action MOBA + an RPG metagame + dynamic terrain + squad
   AI is four games. Any one of them is a studio's full output.
2. **Dynamic verticality.** Player-created climbable surfaces invalidate most
   assumptions level design and pathing make. This is the feature most likely to
   be cut, and it should be validated *first*, not polished last.
3. **Retinue clutter.** Visual noise and target confusion in a 5v5 with 15 extra
   units. Needs a strict silhouette and a hard field cap, enforced by the budget.
4. **Build-space balance.** Hundreds of abilities × 30 class pairs × sigils is a
   combinatorial surface no balance team can cover by hand. This is why the
   headless sim and automated sweeps are listed as spine, not polish.
5. **The ceiling law under commercial pressure.** See above.

## Vertical slice

The slice must answer one question — **is terrain-combo combat fun?** — and
nothing else. Deliberately no progression, no metagame, no matchmaking.

**Target: 2v2, one small map, ~6 weeks of prototype work.**

- 3 classes: Verdurist, Emberwright, Tidecaller. (The three that fight over the
  matrix. Warden, Beastbinder and Cogwright prove nothing here.)
- ~18 abilities total, 6 per class, 1 elite each.
- 8-slot bar and primary/secondary already in — the build economy is half the
  question and costs almost nothing to prototype.
- Terrain grid with 5 surfaces: `Foliage`, `Burning`, `Scorched`, `Water`, `Ice`.
  Fire spread on. No volumes, no `Trellis` — **verticality is slice 2**, tested
  alone so its cost is visible.
- 1 retinue unit each (a scout drone, `Hold`/`Follow` only) to test whether
  stance-level squad play reads at all.
- Local or simple client-server; no prediction yet.

**Kill criteria, decided before building:** if players in playtests treat the
terrain as scenery — if they fight where they would have fought in any MOBA and
the fire is a damage aura with extra steps — the concept does not work and no
amount of content fixes it.

## Open questions

Genuinely undecided, and the answers change the design substantially:

1. **Persistent world, or lobby-only?** GW1 had towns and explorable zones; a
   pure lobby game is dramatically cheaper and loses the "online RPG" feel you
   opened with.
2. **Team size.** 5v5 is the genre default and the worst fit for retinues; 3v3
   makes squads matter far more and makes every ability more readable. My
   instinct is 3v3 as the flagship, 5v5 as the second mode — the opposite of the
   usual ordering.
3. **Does owned gear ever matter competitively?** This doc says no. It is worth
   your explicit sign-off, because it constrains monetization and retention
   design permanently.
4. **Death and respawn.** MOBA-style timers, or something with less downtime
   given how much of the game is terrain setup you would hate to lose?
5. **How deep does retinue configuration go?** 2 ability slots per unit is the
   conservative call; the beast-tamer fantasy might want more, at the cost of
   pre-match build time and in-match legibility.
6. **Is the Beastbinder a class, or an axis?** "Bring beasts" and "bring
   steampunk robots" could be retinue unlocks available to everyone, with the
   Beastbinder/Cogwright classes being *better at commanding* them rather than
   being the gate. That is probably the healthier design.
