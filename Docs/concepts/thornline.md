# Thornline — Concept Design

> **Working title.** Not Light Card Tactics. This is a separate game concept
> parked in this repo because it is the same designer's next idea and because
> several systems already built here — a data-driven effect grammar, a headless
> deterministic sim, a standalone test runner, a simulate-and-score heuristic
> agent — transfer almost directly to it (see [Reuse from LT Cards](#reuse-from-lt-cards)).
> Nothing here is committed to; it is a design to argue with.

## The draw

**The fantasy of being a specific, strange, self-authored character — and the
craft of assembling one.**

Everything in this document is downstream of that sentence. Not competitive
integrity, not ladder health, not match pacing. The moment of joy this game is
built to produce is: *"I made a thing that works and nobody else is running it,
and it looks and feels like the character in my head."* The second moment is
showing someone.

That is a narrow, unfashionable target, and it excludes things. Named
consequences, stated up front:

- **It is not a MOBA.** MOBA structure — session-based team objective play — is
  borrowed as *one mode*, not as the game's shape. A 25-minute competitive
  instance with no world, no downtime, no NPCs and no persistence gives a
  self-authored character nowhere to *be*. See
  [The shape of the game](#the-shape-of-the-game).
- **PvE is primary, PvP is a wing.** This is the single biggest reversal from an
  arena-first design, and it is also what makes the combinatorial build space
  affordable — see [Why PvE-primary makes the build space possible](#why-pve-primary-makes-the-build-space-possible).
- **Balance means "no dead builds", not "no strong builds".** The failure state
  is a combination that is *boring*, not one that is 8% above the curve.

## Design pillars

1. **Combinations have names.** A build is not a spreadsheet row; it is a
   character concept the game recognizes, names, and reacts to.
2. **Your build changes the world you see.** Class abilities are traversal,
   exploration and expression verbs outside combat, not just in it. Two players
   walking the same zone with different builds should not have access to the same
   map.
3. **The map remembers.** Terrain is a system builds can *lean into* — not a tax
   every ability pays. A fight in the right place should leave a scar; a fight in
   a stone corridor is still a good fight.

## The shape of the game

Guild Wars 1's structure, because it is the only shipped structure that supports
both halves of the draw:

| Layer | What it is | Why it's here |
| --- | --- | --- |
| **Hubs** | Persistent social towns. Your character stands here with **your retinue visibly around you** | Where the fantasy is *worn*. Without a place to be seen, build-crafting has no second half |
| **Missions** | Short instanced co-op runs, 1–4 players, the retinue filling empty party slots | The primary game. Where abilities are discovered, beasts tamed, sigils found |
| **Arenas** | Instanced PvP, including the objective/lane mode from the MOBA idea | A proving ground and a second use for your build, not the point of the build |

Instanced, not a shared open world. That is a deliberate cost decision, and the
terrain system requires it: a map whose surfaces are being rewritten by players
needs to be able to *reset*.

## Missions

**The loop:** Hub (build) → Briefing (read the profile) → Mission (8–15 min) →
Reward → Hub.

Bite-size by design, and this is the structural decision that makes the rest of
the concept work rather than merely fitting alongside it. If the draw is making
cool combinations, then **the loadout decision has to recur**. A long
expedition-shaped game asks you to pick a build once a week; a 12-minute mission
asks several times an evening, and build-crafting stops being character creation
and becomes the game you are actually playing.

That is Monster Hunter's loop with GW1's build economy dropped into the slot
where MH puts weapons.

| | |
| --- | --- |
| **Length** | 8–15 minutes core. Optional *expedition* chains of 3 missions for a longer sitting, with no build swap between legs |
| **Shape** | One stated objective (hunt / retrieve / escort / hold / clear), 1–3 optional objectives, one complication or boss around 60% in |
| **Party** | 1–4. Retinue fills empty slots, so solo is never second-class |
| **Failure** | Cheap. ~12 minutes and partial rewards kept — experimentation has to be cheap or nobody experiments |

### The briefing is the puzzle

Every mission publishes its profile *before* you commit, and the profile is
written in the combo grammar:

> **The Drowned Kiln** — Marsh. Dominant surfaces `Water`, `Mud`, `Foliage`.
> Enemies: Drowned (resist `Fire`, vulnerable to `Shock`). Hazard: rising flood
> on a timer. Optional objectives need **Breaks** ×1, **Grows** ×1.

So the pre-mission screen is a real decision with real information: a
`Tidecaller/Cogwright` reads that and sees a chain-shock playground; a
`Blightburner` reads it and knows their fire does nothing until something dries
the ground out — which is either a reason to bring a different character, or a
reason to bring the *Kindling* sigil and make it work anyway.

This also converts pillar 2's build-gating from a wall into an invitation:
optional objectives name the world verbs they want, up front, so a verb you lack
is a reason to come back with a different character rather than a dead end you
discover ten minutes in.

### Modifiers

A rotating set of mission modifiers rewrites the profile of an existing map:
*Drought* (no `Water` surfaces persist), *Verdant Bloom* (`Foliage` regrows),
*Tempest* (`Shock` chains double), *Ashfall* (everything starts `Scorched`).

This is the direct answer to the content-volume risk: a modifier multiplies how
many builds a map is interesting for, and costs no new art. A small map count
with a deep modifier table is the affordable version of this game.

### Why PvE-primary makes the build space possible

An arena-first game with hundreds of abilities across 56 class pairs has a
balance surface no team can cover, and every patch invalidates someone's
character. That pressure is what forces competitive games toward small, legible,
homogenized kits — the exact opposite of this draw.

Going PvE-primary buys the build space directly:

- A build only has to be **viable and expressive**, not tournament-valid.
- Strong, weird, situational and over-the-top are all *features* in co-op.
- PvP lives in arenas with normalization, so competitive concerns stay quarantined
  in the mode that asked for them.

This is the trade being made on purpose: give up esports legitimacy, buy the
combinatorial toybox.

## Classes

**Eight at launch — four martial, four arcane.** The earlier six-class roster was
caster-heavy, and not by accident: defining classes by "what verb it performs on
the world" structurally favors people who cast spells at the ground. That is a
flaw in the method, not a truth about the game, and it needed fixing — half the
character fantasies anyone actually wants to roleplay are martial ones, and a
game whose combination space is three mages and a knight has a thin half.

The fix is not "give warriors spells." It is
[a different relationship to the same system](#how-martials-touch-the-matrix):
**arcane classes write to the map; martial classes read it, move through it, and
put people into it.**

### Martial

| Class | World verb | In combat | **Out of combat** | Primary attribute (primary-only) |
| --- | --- | --- | --- | --- |
| **Warden** | **Holds** — raises cover, body-blocks, anchors ground | Frontline, peel, objective soak | Braces collapsing structures; shields the party through a hazard; holds a gate open | *Bulwark* — armor scales with how much cover and blocked ground surrounds you |
| **Reaver** | **Displaces** — knocks, throws, charges, shatters | Aggressive melee, the `Force` specialist | Breaks sealed walls and rubble; carries heavy things; charges gaps | *Momentum* — your displacement distance scales, and moving builds adrenaline |
| **Veilblade** | **Slips** — concealment, verticality, gaps | Burst, flanks, assassination | Free-climbs without a `Trellis`; squeezes through gaps; scouts unseen | *Stillness* — you conceal in any volume, and break concealment on your terms |
| **Beastbinder** | **Inhabits** — beasts scout, flush, hold; bow and preparations | Ranged martial, retinue depth | Beasts track scents; some are mounts or fit through gaps | *Kinship* — +2 retinue points, one beast's abilities on your own bar |

### Arcane and artifice

| Class | World verb | In combat | **Out of combat** | Primary attribute (primary-only) |
| --- | --- | --- | --- | --- |
| **Verdurist** ("green mage") | **Grows** — vines, brambles, grass, trellises | Control, area denial | Grows a climbable trellis on any tagged wall; reaches ledges nobody else reaches | *Rootedness* — your terrain lasts longer, regrows once |
| **Emberwright** | **Ignites** — burns foliage, spreads fire | Damage over time, zoning | Burns away overgrowth sealing a path; lights dark areas | *Combustion* — your fires spread noticeably further |
| **Tidecaller** | **Changes state** — floods, freezes, douses | Mobility surfaces, chain setup | Freezes a river into a bridge; floods a channel to float something | *Current* — refunds resource when a combo you set up triggers |
| **Cogwright** | **Installs** — turrets, ziplines, drones | Siege, vision, infrastructure | Ziplines across gaps; drones scout ahead and map rooms | *Fabrication* — installations cost less, can be repaired |

Eight classes give **56 ordered pairs**. That is the number the whole draw rests
on, and it is why the roster grew rather than swapping a mage out: martial/arcane
pairs in *both* directions are the richest part of the space. A
`Reaver/Tidecaller` freezes the ground and shatters it under you; a
`Tidecaller/Reaver` is a mage who has learned to shoulder-check.

**Consequence to accept deliberately:** if builds gate exploration, a solo player
sometimes cannot reach something. That is the price of pillar 2, and the answer is
the retinue and co-op — your party (or your hired retinue) covers verbs you lack.
The alternative, making every verb universally available, deletes the pillar.

## The skill bar and multiclassing

- **6 slots**, chosen out of combat, locked when you enter a zone or match. One
  of them is almost always your self-heal — see [Healing](#healing).
- **At most 1 elite.** GW1's best limiter: it makes 6 slots a budget instead of a
  list of favorites, and it forces the build to have a *thesis*.
- **Attributes**: a shared ~200-point pool across both classes' lines, with
  escalating costs so ranks 1–9 are cheap and 10–12 are expensive. Broad and
  shallow, or narrow and deep — never both.
- **Primary** grants: full skill pool including elites, armor class, and the
  primary attribute. **Secondary** grants: non-elite skills and attribute lines
  only. Secondary is swappable freely out of combat; primary is per character.

### Why six and not eight

Six is tighter than GW1's eight, on purpose, and the arguments are mostly about
this game's specific shape rather than about bars in general:

- **Missions are 8–15 minutes.** A bar you can fully express inside one fight
  beats a bar you are still discovering at minute nine.
- **Six is a gamepad.** Four face buttons plus two shoulders, or one clean radial.
  Eight is not, and this is a real platform decision disguised as a number.
- **Scarcity is the expression engine.** Every slot you remove makes the remaining
  ones louder. With six, taking one skill from your secondary class is a
  *statement about your character*, not a leftover.
- **It shortens the read.** Inspecting someone's build in a hub should be a glance.

**And the elite slot is not a tax — it is the largest single expression surface
in the game.** An elite can be anything: a transformation, a persistent aura, a
new resource rule, a summon, a weapon technique that rewrites your chain, a
terrain effect at a scale no normal skill reaches. It is one of six, it is your
build's thesis, and two characters that share five skills and differ only in
their elite are genuinely different characters. Counting it alongside the heal as
"overhead" misreads what it does.

So the real budget is: **one elite (a wildcard), one heal (a real choice you may
decline), and four slots.** That is tighter than GW1 and it is meant to be.

The residual risk is narrower than "six is too few": it is that a *category* —
mobility, or a defensive cooldown — becomes compulsory and quietly eats the
flexible slots. The answer is to stop the bar from being the only place abilities
live, rather
than to add slots back:

- The **weapon** carries its own native skill, outside the six.
- **Sigils** modify abilities you already have instead of costing a slot.
- The **retinue** carries its own abilities (below), so party-wide utility need
  not come off your bar.
- Class kits are authored so that no *category* is mandatory — mobility should
  come bundled into a damage skill for some builds, not as a separate tax.

Effective capability is therefore closer to six-plus-two than to six, while the
thing you *tune* stays six. Whether four discretionary slots is enough room to
feel like build-crafting is a primary question for the slice.

The primary attribute is what stops "everyone runs the same secondary" and makes
`Verdurist/Tidecaller` and `Tidecaller/Verdurist` genuinely different games out of
identical skill access.

### Two economies, and the trap between them

Martial abilities run on **adrenaline** (built by connecting hits, spent in
bursts, lost out of combat). Arcane abilities run on **energy** (a pool that
regenerates). Every class has some of both, weighted heavily toward one.

This is GW1's shape and it carries GW1's most famous failure with it: a martial
primary with an arcane secondary had almost no energy, so half the pairs were
dead on arrival. Given "no dead builds" is the stated balance goal, that is not
acceptable here, and it needs an explicit answer rather than a shrug:

- Every class has a small universal energy pool — enough for two or three
  secondary-class utility casts per fight, never enough to be a mage.
- Martial primary attributes convert: *Momentum* and *Stillness* both pay out in
  a resource the other half of the bar can spend.
- The Focus accessory slot can be spent on energy, at the cost of a sigil.

A `Reaver/Emberwright` should be a real character who lands three strikes and
spends them on one detonation — not a warrior who cannot afford their own second
class.

### Named pairs

**The cheapest, highest-leverage feature in this document.** All 56 ordered pairs
get an authored in-world name, displayed under your character in hubs, with its
own emote and a title card the first time you assemble one.

| Pair | Name | The character it is |
| --- | --- | --- |
| Verdurist / Emberwright | **Blightburner** | Grows the fuel, then lights it. Slash-and-burn zoning |
| Emberwright / Verdurist | **Ashgardener** | Burns first and plants in the ruin. Wants the *Second Growth* sigil |
| Verdurist / Tidecaller | **Fenwright** | Floods, then grows reeds in it. The concealment specialist |
| Tidecaller / Emberwright | **Steamcaller** | Lives inside steam clouds and fights where nobody can see |
| Beastbinder / Cogwright | **Houndwright** | Machine-augmented animals. Your pack has brass in it |
| Cogwright / Beastbinder | **Menagerist** | A collector who builds habitats and lets things out of them |
| Verdurist / Beastbinder | **Grovekeeper** | Grows the terrain the pack hunts through |
| Warden / Verdurist | **Bramblewarden** | A wall that grows more wall |
| Warden / Emberwright | **Cinderguard** | Holds ground by making the ground unholdable |
| Cogwright / Emberwright | **Boilerwright** | Oil, pressure, and things that should not be indoors |
| Reaver / Tidecaller | **Frostbreaker** | Freezes the ground, then shatters it under you. The martial thesis in one build |
| Reaver / Emberwright | **Pyreblade** | Oiled weapon, and it throws you into what it lit |
| Veilblade / Verdurist | **Thornghost** | Grows its own hiding places and hunts from grass it planted |
| Veilblade / Tidecaller | **Mistwalker** | Makes the steam, lives in it, leaves before it clears |
| Warden / Reaver | **Breakwall** | Holds the line by moving it forward |
| Beastbinder / Emberwright | **Emberpack** | Burning arrows and hounds that drive things into the fire |

**Names are recognition, not rules.** A pair name describes an emergent
playstyle; it grants nothing by itself. Mechanical pair identity comes from
**sigils** instead, so there are never 30 hardcoded special cases in the balance
surface — the discipline LT Cards already applies to card text.

### Build codes

A build (class pair, 6 skills, attributes, equipment template) serializes to a
short shareable text code, paste-able in chat and in hubs. Trying someone else's
character should take ten seconds. If the draw is combinations, **friction on
trying a new one is the primary enemy** — so unlocks are account-wide, alts are
free and unlimited, and any character can hold multiple named build templates and
swap between them in a hub.

## Elements, surfaces and the combo grammar

The expression engine. Specified as a closed data table rather than per-ability
special cases, so new content ships as data.

**A cell** is one square of the terrain simulation grid, ~0.5 m across, holding
one surface, an optional volume above it, timers, and who made it. **The grid is
invisible** — it is a data substrate, not a tactics grid. Movement is free and
analog; adjacent cells sharing a state render as one patch with an organic edge,
so players see a burning *field*, never lit squares.

> **Vocabulary rule:** "cell" is an engineering word and must never appear in
> player-facing text. Ability and attribute descriptions speak in metres and plain
> language — *"armor scales with the cover around you"*, not *"per adjacent
> blocked cell"*. Designers count cells; players see ground.

**Surfaces** (one per terrain cell, mutually exclusive):
`Bare` · `Foliage` · `Bramble` · `Water` · `Ice` · `Oil` · `Mud` · `Burning` · `Scorched`

**Volumes** (above a cell, stack with any surface):
`Smoke` · `Steam` · `Pollen` · `Gas`

**Elements** (what abilities apply): `Fire` · `Frost` · `Shock` · `Water` ·
`Growth` · `Force`

### The interaction matrix

| ↓ Element applied to → | Foliage | Water | Oil | Ice | Mud | Burning |
| --- | --- | --- | --- | --- | --- | --- |
| **Fire** | ignites → `Burning`, **spreads cell to cell** | → `Steam` volume (vision block) | → `Burning`, instant, whole slick | melts → `Water` | — | intensifies, +1 spread ring |
| **Frost** | wilts → `Bare` | → `Ice` (slide movement) | brittle — shatters on `Force` | thickens, +duration | hardens → fast walkable | douses → `Scorched` |
| **Shock** | — | **chains to every unit touching the pool** | ignites → `Burning` | chains + stun | grounded, halved | — |
| **Water** | soaks — fireproof ~8s | deepens → `Deep Water` | spreads the slick outward | melts → `Water` | — | douses → `Bare` |
| **Growth** | thickens → `Bramble` | → `Reeds` (walkable *and* concealing) | — | — | grows at double rate | — |
| **Force** | flattens — **reveals anyone concealed** | displaces units | spreads the slick | shatters → `Bare` + slow | — | scatters embers to adjacent cells |

`Scorched` is the pressure valve: nothing grows on it for ~20s. It is how fire
answers growth, and why the *Ashgardener* wants a sigil that breaks that rule.

Rules that fall out of the matrix:

- **Concealment**: `Foliage`, `Reeds` and `Smoke` conceal. You are revealed by
  attacking, by `Force` flattening the cell, or by it catching fire. Hiding is a
  real but *burnable* resource.
- **Climbing**: `Growth` on a tagged wall produces a `Trellis`, climbable ~15s.
  Maps are authored with "sometimes-vertical" walls, so routes are partly
  player-created — the clearest expression of pillar 2, and the most expensive
  feature here.
- **Fire spreads.** It is a cellular automaton over flammable surfaces, not an
  aura. You can start one you cannot control.

### Portable terrain is a required category

Every terrain-adjacent class needs at least one cheap ability that **manufactures
its preferred surface on demand** — Seedfall grows a patch of `Foliage` at range,
Wellspring puts down water, Kindle lights a cell, Smokebomb makes a volume
chemically, Rime Underfoot freezes what is already there.

This is not flavor, it is what keeps terrain builds from being hostage to the
briefing. Without portable surfaces, a `Drought` or `Ashfall` modifier does not
*challenge* a build, it *deletes* one, and a player who brought their favorite
character to the wrong map simply does not get to play. With them, the modifier
becomes what it should be: your setup costs more slots and more time than usual.

### How martials touch the matrix

The trap with a terrain-and-element core is that martial classes become
stat-sticks who wait for the mages to finish decorating. Three verbs prevent it,
all of which use machinery the matrix already has:

**1. `Force` is the martial element.** It is already a row in the table, and
martials are its primary wielders. A Reaver does not set the fire — a Reaver
*throws you into it*, shatters the `Ice` under your feet, scatters a `Burning`
cell into four, and flattens the `Foliage` you were hiding in. Displacement turns
every surface an ally created into a weapon, which means a martial's power scales
with how interesting the map is, not despite it.

**2. Terrain that walls a caster is a road for a martial.** Martials buy
traversal mastery rather than immunity: run across `Ice` without sliding, vault
`Bramble` instead of pathing around, cross `Burning` at a stacking cost, climb
where there is no `Trellis`. The Veilblade is the extreme case — its whole kit is
*moving through the map other people made*.

**3. Carried elements, not cast ones.** Weapon coatings, oils, and the
Beastbinder's arrow preparations let a martial apply `Fire`, `Frost` or `Shock`
by having *prepared*, not by casting. This is the mechanism that opens every
matrix row to a pure martial build and makes martial/arcane pairs work in both
directions.

Read together: an arcane build asks "what should this ground become?" and a
martial build asks "what is this ground already, and who can I put on it?" Same
table, opposite grip.

## Healing

GW1's answer, adopted: **every class has its own self-heal, and there is no
dedicated healer class.** Three rules make that work.

**1. Your self-heal costs a slot.** It is an ordinary skill occupying one of your
six, competing with everything else. This is the quiet genius of the GW1 bar —
the heal slot is the most universally pressured slot in the game, and *choosing to
leave it empty* is a real, aggressive build statement rather than an oversight. At
six slots it costs a **sixth of your character**, which makes skipping it a much
louder statement than GW1 ever allowed.

**2. Nobody is obligated, and nobody is a tax.** A mandatory healer in a
12-minute mission is a queue tax and a person who did not get to pick their
fantasy. Distributed sustain means any four characters form a viable party. The
healer *fantasy* survives as a **build, not a class**: attribute lines and sigils
let a Cogwright specialize into field repair or a Verdurist into healing groves,
so someone who wants to play support can — and nobody has to.

**3. Heals read the map.** This is the part that is specific to this game rather
than borrowed. Most self-heals hook the terrain system, which has three
consequences worth the whole feature: sustain becomes part of the terrain game
instead of an orthogonal bar; enemies can **attack your healing** (burn the
foliage you root in, douse the water you drink from, knock you off your anchor);
and martials get another use for `Force` — displacing a healer off their ground is
counterplay with no new machinery.

| Class | Self-heal | Cost shape | Map hook |
| --- | --- | --- | --- |
| **Warden** | *Bracing Stance* — heal while planted, broken by movement | Immobility | Scales with adjacent blocked cells |
| **Reaver** | *Bloodwake* — spend adrenaline on a hit to heal | Must be winning | Doubles if the target was displaced into a hazard |
| **Veilblade** | *Quiet the Wound* — regen while concealed, ends on reveal | Disengagement | Requires `Foliage`, `Reeds` or `Smoke` |
| **Beastbinder** | *Tend* — your beast returns and heals you both | Retinue downtime — the beast stops fighting | None; the reliable one |
| **Verdurist** | *Rootdraw* — root yourself, heal fast | Rooted, and flammable | Scales with surrounding `Foliage`/`Bramble` |
| **Emberwright** | *Cauterize* — consume a nearby `Burning` cell for a large instant heal | Extinguishes your own zoning | Requires `Burning` |
| **Tidecaller** | *Drink Deep* — heal scaling with the water you stand in | Weak on dry ground | Scales with `Water`; self-solving |
| **Cogwright** | *Field Repair* — a deployable that heals anyone inside it | Stationary, destructible, slow | None — the only party-facing heal, and the seed of support-without-a-healer |

**Every heal has a floor that works on `Bare` ground.** Terrain is the amplifier,
never the prerequisite — otherwise a mission with the wrong surfaces is simply
unplayable for your character. Briefings advertise surfaces, so *"this map is dry,
my Tidecaller heal will be weak"* is a planning input, not an ambush.

### The heal slot is a combination engine

Because a secondary class grants its non-elite skills, **you can take someone
else's self-heal** — and since each one has a different cost shape, that single
slot generates an enormous amount of character identity for its size:

- A `Reaver/Veilblade` fights aggressively and vanishes into the grass to recover.
- A `Reaver/Tidecaller` makes its own puddle and heals standing in the fight.
- A `Verdurist/Beastbinder` never roots, because the hound does the healing.
- A `Veilblade/Emberwright` heals by putting out the fire it is hiding in the
  smoke of, which is a genuinely strange and specific way to play.

One slot, eight options, 56 pairs. This is the design pattern the whole game wants
more of: a small closed set whose *combinations* carry the expression.

### Between fights

Out-of-combat regeneration is fast and free. In 8–15 minute missions, downtime as
a resource tax is pure friction — the heal slot is meant to matter *during* a
fight, not to make you sit down after one.

## Sigils: where builds get weird

Two of your four equipment slots are accessories, and the **Sigil** is the engine
of build identity. A sigil is a small, legible rules patch to a *single ability*:

- *Second Growth* — your `Growth` works on `Scorched` ground. (The Ashgardener's
  thesis, available to anyone, senseless on most builds.)
- *Trellised* — your vines also grow a trellis on the nearest tagged wall.
- *Scalding* — your `Steam` volumes damage.
- *Pack Tactics* — your retinue's abilities trigger off *your* combos.
- *Kindling* — your beasts leave a trail of `Foliage` where they run.

This is the intended long-tail content and the answer to "how do combinations
stay interesting past month two": each sigil is data, rewrites one ability, and
the combo matrix stays the only global system. Sigils are also **where the named
pairs get mechanical teeth** without hardcoding 30 exceptions.

Sigils are found and earned in missions, never bought. Acquiring the
piece that completes a build concept *is* the progression curve.

## The Retinue

Your squad. Reframed from "lane pressure unit" to **entourage** — it is part of
your character's statement, and it follows you into hubs where people can see it.

### Composition and budget

Retinue points scale with context, so field clutter stays bounded:

| Context | Points | Typical squad |
| --- | --- | --- |
| Missions (solo) | 8 | a real squad; fills the party |
| Missions (4-player) | 3 each | one companion each |
| Arena 3v3 | 5 | a beast + a scout, or one heavy construct |
| Arena 5v5 | 3 | one beast, or three drones |

Costs: scout drone 1 · beast or skirmisher 2 · heavy construct 3 · named
companion 4.

### Equipment and abilities

Every retinue unit has the same four slots as you — **weapon, armor, accessory,
accessory** — plus a **3-slot ability bar**, which is deliberately *half a
character*: three to your six. That ratio is the whole statement — a companion is
a real build you author, not a pet with a trick, and not a second character you
have to pilot.

Three is enough for a companion to have a *role* (a heal, a control, a threat)
where two only ever gave it a thing and another thing. It also means a companion
can carry its own sustain, which is what lets support-as-a-build extend to the
retinue.

**Slots scale with cost**, so cheap units do not multiply the noise — a proposal
rather than a rule, and easy to overrule: a 1-point scout drone gets 1 slot, a
2-point beast gets 2, and anything 3 points or more gets the full 3. A solo
player fielding four units is then reading about eight companion abilities, not
twelve.

### Companions are programmed, not micromanaged

Each companion ability slot carries a **trigger condition** chosen alongside it —
*when an enemy is rooted*, *when I drop below half health*, *when an ally stands
in fire*, *on first contact*. You are not timing these in the fight; you are
authoring the behavior in the hub and then watching it play out.

This is the piece that makes 3 slots × 4 units tractable instead of an RTS, and
it is a direct lift of the Condition/Trigger grammar already built in this repo's
card engine — the same data shape, aimed at squad behavior. It also puts retinue
expression where the rest of the game puts it: in the loadout screen, as a puzzle
you solve before the mission and get graded on during it.

**Named companions** (cost 4) are the roleplay slot: they have a name you give
them, a persistent appearance, dyeable barding, and they level a small trait tree
across missions. This is the beast-tamer fantasy's real home — not raw power, but
*that specific animal, that you trained.*

### Control

Four stances — **Follow · Hold · Hunt · Screen** — plus one directed command on a
~6s cooldown. Stance-level by design: the moment direct micro outperforms stance
play this becomes a bad RTS, and "commanding a crew" stops feeling like
commanding.

**Beastbinder** buys depth rather than everyone being forced into it: extra
command charges, direct control of one beast, and that beast's abilities surfaced
onto the player's own 6-slot bar, where they compete for slots like everything
else.

**Shouts are the real command surface.** A shout like *Hold Fast* (party gains
Stability) or *Command: Screen* does honest combat work **and** redirects the
squad, which is better than a dedicated command key for three reasons: retinue
control competes for bar slots like everything else, a player who wants to
command more can build for it, and a player who wants to ignore the squad can
leave it on a stance and never think about it. The directed-command key stays as
the floor; shouts are how you buy above it.

## Equipment

Four slots: **Weapon · Armor · Accessory · Accessory**. All horizontal — they
change what you do, not how big your numbers are.

| Slot | What it decides |
| --- | --- |
| **Weapon** | Your attack's rhythm and shape, the abilities you can slot, and one weapon-native skill outside your six. See [Weapon families](#weapon-families) — this is the martial build's main expression surface, so its variety is not cosmetic |
| **Armor** | Defense *profile*, not amount: trade-offs (+vs fire / −vs shock). 2 rune slots (capped attribute ranks) + 1 insignia (situational passive). **Armor is the class silhouette** and the main visual identity surface — dyeable, per-piece |
| **Accessory ×2** | Sigils (above) and a Focus that shapes your resource economy |

### Weapon families

Seven martial families, plus implements for casters. A weapon is not a damage
number with a model on it — each family has its own rhythm, its own resource
quirk, and its own gated abilities, so **changing weapon changes how the character
plays before a single skill slot changes.**

| Family | Rhythm | Resource quirk | Gates | Mastery line |
| --- | --- | --- | --- | --- |
| **Sword** | Balanced 3-hit chain; reliable, interruptible at known points | Steady adrenaline | Ripostes, parries, chain-enders | **Reaver** |
| **Heavy arms** (axe, hammer, greatsword) | Slow, committed, wide arcs | Adrenaline in big lumps; whiffing hurts | Knockdowns, armor-breaks, the biggest `Force` | **Reaver** |
| **Spear** | Mid-range thrusts, keeps distance; throwable | Steady, rewards spacing | Pins, impales, shield pairings | **Warden** |
| **Dagger** (paired) | Fastest; off-hand triggers on-hit effects | Fast small adrenaline | Positional strikes, off-hand chains, coatings | **Veilblade** |
| **Martial arts** (unarmed) | Flowing chains, high mobility | Fastest adrenaline in the game | Grapples, throws, stance transitions | **Veilblade** |
| **Bow** | Draw-and-release, arcing, distance | Draw time is the cost | **Preparations** — elemental arrows | **Beastbinder** |
| **Gun** | Burst, flat trajectory, armor-piercing | **Reload** — a real rhythm, not a cooldown | Ammunition types, braced shots | **Cogwright** |
| *Implements* (staff, scepter, focus) | Cast-facing; the caster's stat surface | Energy | Nothing martial; scales off casting attributes | *none — scales off the arcane lines* |

Three rules make this carry real weight:

**Anyone can hold anything; mastery is an attribute line.** A Verdurist can carry
a greatsword. They will be bad with it — Heavy Arms is a Reaver line, and
investing in it means taking points out of growth. So *"I want to actually swing
this"* becomes a reason to take a martial secondary, which is exactly the kind of
decision the pair system exists to produce.

**Weapons gate abilities.** Hammer knockdowns need a hammer, off-hand chains need
daggers, preparations need a bow. That is what stops the weapon from being
cosmetic and makes the choice upstream of the bar.

**Weapon mastery is how an arcane class buys into the martial half**, and the
grapples-and-throws line on martial arts is a second home for `Force`, so the
martial relationship to terrain does not live only on the Reaver.

**Traps must be legible.** A Verdurist swinging a greatsword at Heavy Arms 6 is a
bad build, and that is fine — an open combination space that contains no bad
builds is not really open. But the loadout screen must say so *plainly and before
the mission*: show the mastery you actually have against the weapon's expected
rank, in words, not as a number the player has to know how to read. A trap you can
see is build-crafting. A trap you cannot see is bad information wearing a costume.

**And design primary attributes to be re-read by secondaries.** The Warden's
*Bulwark* scales armor with adjacent blocked cells; the Verdurist manufactures
blocked cells. Nobody authored that interaction — it fell out of two systems
meeting, and it is the single most satisfying thing theorycrafting this roster
produced. Authoring target: every primary attribute should have **at least two
secondaries that give it a non-obvious reading**. That is where players find
things and feel clever, and it costs nothing but care at design time.

## Beyond terrain: the ability vocabulary

Terrain is one system, not the tax every ability pays. Most abilities in this
game should be straightforwardly, legibly good at something that has nothing to do
with the ground:

| Family | What it is | Mostly |
| --- | --- | --- |
| **Conditions** | Stacking physical afflictions — Bleeding, Crippled, Blind, Weakness, Deep Wound, Dazed | Martial |
| **Hexes** | Targeted curses — drains, reversals, damage mirrors, action taxes | Arcane |
| **Enchantments and wards** | Persistent buffs on self, ally, or a patch of ground | Arcane |
| **Stances** | Self-only postures, **one at a time**, so switching is a real decision | Martial |
| **Shouts and commands** | Short party-wide effects — and the natural hook for directing the retinue | Both |
| **Interrupts** | Timing plays that punish casts; the main counterplay to arcane builds | Both |
| **Weapon techniques** | Chain-enders, positional strikes, stance transitions | Martial |

**Authoring rule: terrain-interacting abilities should be a minority — roughly a
quarter to a third of the catalog.** The reasons are concrete, not stylistic:

- If every skill touches the ground, every class is a landscaper and the
  characters stop being different from each other.
- Builds would homogenize toward terrain setup, because setup would be the only
  thing the system rewards.
- Missions indoors, on stone, on ships, in workshops and at night would all be
  dead content — and cutting those out narrows the world badly.
- A player who wants to be a duelist with a spear and a grudge should never be
  told the game is about gardening.

The matrix is spice. It should be the reason a *particular* build feels
extraordinary in a *particular* mission, not a checklist every ability passes
through.

Appearance is decoupled from stats (transmog by default, not as an unlock).
If the draw is being a specific character, making players choose between looking
right and playing right is self-defeating.

## Progression as discovery

- **Power ceiling reachable in ~10 hours.** Everything past it is horizontal:
  abilities, sigils, beasts, companions, cosmetics.
- **Arenas normalize** gear, attributes and access to maximum, so the PvP wing
  never becomes a grind gate.
- **Abilities are found, not purchased.** You learn a Verdurist elite by running
  the mission where it happens and doing the thing, and missions advertise what
  they can drop — so chasing the piece that completes a build concept is a
  concrete plan, not a loot-table lottery. Horizontal progression *is* the
  content, which is how "more options" avoids being a shop menu.
- **The world reacts to your pair.** Hub NPCs, faction greetings and mission
  briefing dialogue key off your named pair. Cheap authored content, disproportionate
  payoff for the roleplay half of the draw.

Stated as a rule because it will be under permanent pressure: every retention
pass will propose a small vertical exception. The concept does not survive one —
the instant playtime buys *numbers*, build choice becomes build *obligation*.

## Legibility

The competitive framing of this problem ("hidden 6-slot builds destroy
counterplay") mostly evaporates once PvP is a wing. What remains is the
*expressive* version of it: **your build should be legible enough to be admired.**

- **Silhouette telegraphs concept** — class armor, weapon, and your visible
  retinue say most of it before you act.
- **Named pair on your nameplate** says the rest.
- **Inspect and build codes** in hubs: looking at someone's character and getting
  their build is a social feature, not a security leak.
- In arenas only: abilities used on or near you are logged to a per-opponent
  panel, so counterplay is learnable in the mode that needs it.

## Technical spine

### The terrain grid is the architecture

A uniform cell grid (~0.5 m cells) carrying `{surface, volume, timers, owner}`,
roughly 2 bytes per cell. A 120 m × 120 m map is ~57,600 cells, ~115 KB of
authoritative state, replicated as deltas. Everything falls out of it:

- **Combos** are a lookup into the matrix, per cell.
- **Fire spread** is a cellular automaton at ~5 Hz over the same grid.
- **Pathing** uses **flow fields over the grid**, not navmesh rebuilds — vines and
  walls flip cell costs instantly with no bake, the operation navmeshes are worst
  at. The most important technical call in this document.
- **Retinue AI** reads the same grid, so squad units understand fire and brambles
  for free.

### The rest

| Concern | Approach | Risk |
| --- | --- | --- |
| Networking | Server-authoritative, fixed ~30 Hz tick. Short instanced missions keep peak player counts low and sessions short-lived — far cheaper than an open world, and instances recycle | Low to moderate |
| Hubs | Higher player count, **no terrain simulation, no combat** — a separate, much cheaper server path | Low, if kept genuinely combat-free |
| Vision / concealment | Server-side visibility culling; hidden enemies are not sent to the client | Non-negotiable in arenas; client-side fog is a wallhack |
| Climbing | Traversal volumes spawned from `Trellis` cells, with authored wall tagging | **High.** Dynamic verticality is where animation, pathing and level design all get expensive at once |
| Content pipeline | Abilities, sigils and the matrix as data with an editor and a headless test suite | The whole design depends on non-engineers shipping build-space content |
| Engine | Unity 6 matches existing team knowledge; a networked action game still needs a real netcode stack on top | Unreal is stronger for traversal/animation; team familiarity is the honest tiebreaker |

### Reuse from LT Cards

| From this repo | To Thornline |
| --- | --- |
| `EffectDef` grammar (Condition/Trigger/Target/Effect) as *data, not code* | Ability definitions, sigil rules, the combo matrix — and the reason content ships without engineers |
| Headless sim with no engine references | Headless combat sim for build sweeps and squad AI |
| `CoreTests/` standalone runner | The combo matrix is exactly the system that needs a few hundred cheap assertions |
| `HeuristicAgent` (simulate-and-score over cloned states) + `AgentPersonality` | Retinue AI, and zone enemy AI. Stance-based squad behavior is an easier target than a card opponent |
| `MatchRunner` headless sweeps | **Dead-build detection**: run every class pair against standard encounters and find combinations that cannot clear. Given the draw, this is the balance tool that matters — not tuning outliers down, but finding builds that are *boring* |

## Lessons from Guild Wars 2

GW2 is the closest thing to a controlled experiment this design will ever get:
same studio, same world, a deliberate rebuild of the same skill system. It is a
successful game and these were **trades, not errors** — but they are trades in
precisely the direction that loses a GW1 build-crafter, which is the exact player
this concept is for.

The one-line diagnosis: **GW1 put the depth before the fight; GW2 moved it into
the fight.** Preparation-game to execution-game. Everything below follows from
that single shift.

| What GW2 changed | What it cost | Guardrail here |
| --- | --- | --- |
| Weapon dictates skills 1–5 of a 10-slot bar; only 5 are freely chosen, from one profession's pool | Authorship. Half your bar is a property of the sword, not of you | **Weapons gate, never dictate** |
| Secondary profession removed entirely; elite specs later restored some of it as *fixed packages* | The identity engine. No W/Mo, no Mo/W — one of the best things about the original | **Free pairing is load-bearing; never ship a premade hybrid that replaces it** |
| Build expression migrated into trait grids and gear stat-combinations | Legibility. A build became percentage modifiers instead of eight nameable verbs | **Expression stays in named verbs** |
| Combo fields and finishers — the same idea as terrain combos | Nothing, because almost nobody noticed them | **Telegraph loudly or don't build it** |
| Trinity removed; everyone self-heals, damages, supports | Role fantasy. Open-world combat became undifferentiated | *(Already handled: distributed self-heals, but support as a real build)* |
| Level 80 and ascended gear above exotic | The "skill, not time" promise | *(Already handled: the power ceiling law)* |

### The four guardrails, and how each gets crossed

Each of these is crossed by a **reasonable-sounding local decision**, which is
why they are written down rather than trusted to judgment.

**1. Weapons gate, never dictate.** A weapon carries *one* native skill and
determines which abilities are *eligible* for your six. It never fills them.
*How it gets crossed:* a "make weapons feel more distinct" pass gives each family
two native skills, then three, and each increment sounds like an improvement.
*Tripwire:* the moment a weapon supplies more than one slot's worth of bar, stop
— you have rebuilt GW2's bar.

**2. Free pairing is load-bearing.** 56 self-assembled pairs, not a menu of
premade hybrids. *How it gets crossed:* an expansion wants a headline feature and
"signature specializations" are easier to balance, market and animate than an open
combination space. *Tripwire:* if a new package grants abilities that could not be
assembled by pairing, it is an elite specialization wearing a different name.

**3. Expression stays in named verbs.** You should be able to describe your build
in one sentence, naming things that happen. *How it gets crossed:* sigils drift
from "your vines also grow a trellis" toward "+6% growth duration", because
numbers are trivially easy to author and to balance. A progression system then
wants a trait grid for depth. *Tripwire:* if a sigil's text contains a percentage
and no verb, reject it.

**4. Telegraph the terrain system loudly.** GW2's combo fields are genuinely deep
and cross-player, and they mostly went unused because they are a translucent circle
under a pile of effects. This design's advantage is physical: brambles, burning
grass and flooded ground are large, persistent and obviously different from bare
ground. *How it gets crossed:* visual polish makes effects prettier and subtler at
the same time. *Tripwire:* a new player should be able to say what just happened
after a combo fires, without a wiki.

### The thing to keep from GW2

Execution depth is not the enemy — GW2's dodge-and-reposition combat is better
moment-to-moment than GW1's. The error would be letting execution depth *crowd
out* preparation depth. The target here is a game where the loadout screen decides
more than the rotation does, and where a fight still rewards you for moving well.

## What is actually hard

1. **Scope.** Still large, but mission structure is the biggest cut available:
   short instanced maps with modifiers replace an authored open world, and that
   alone removes most of the content and streaming cost. Hubs + missions + terrain
   + squad AI is an achievable target in a way hubs + open world never was.
2. **Content volume is the whole design.** Hundreds of abilities and sigils are
   not polish here; they *are* the product. A thin version of this game has no
   draw at all. This is the risk that should scare you most.
3. **Dynamic verticality.** Player-created climbable surfaces invalidate most
   pathing and level-design assumptions. Most likely to be cut — so validate it
   early, alone, where its cost is visible.
4. **Build-gated content.** Pillar 2 means some optional objectives are
   unreachable with your current character. Briefings make this an invitation
   rather than a wall, but it still needs a retinue that genuinely substitutes for
   a missing verb.
5. **Dead builds, not overpowered ones.** With 56 pairs × hundreds of abilities,
   the realistic failure is dozens of combinations nobody enjoys. Automated sweeps
   are spine, not polish.
6. **Martial parity in a game about spellcraft.** The terrain system is a caster's
   playground by default, and every system in this document had to be argued back
   toward martials rather than naturally including them. That asymmetry does not
   go away after launch: every new surface, volume and sigil will *want* to be
   arcane, and someone has to keep insisting that `Force`, traversal and coatings
   get the same attention. This is a standing organizational risk, not a design
   task that finishes.

## Vertical slice

The old slice asked "is terrain-combo combat fun?" Given the restated draw, that
is the *second* question. The first is:

> **Do players want to make another character?**

**Target: three missions on two small maps, 1–2 players, ~8 weeks.**

- **5 classes — Verdurist, Emberwright, Tidecaller, Reaver, Beastbinder.** Grown
  from four specifically to carry a martial: "does a melee character feel like a
  protagonist in a terrain-driven game, or like a spectator waiting for the mages
  to finish?" is now a primary question of the slice, and no arrangement of four
  casters answers it. Five gives 20 ordered pairs, both economies, and both grips
  on the matrix. It costs perhaps two weeks.
- ~35 abilities (7 per class, 1 elite each), the full 6-slot bar,
  primary/secondary, attributes, and **both resource economies** including the
  universal energy pool. **The build economy is the thing under test — it ships
  first.**
- **All five self-heals**, since the heal slot is both the most-pressured slot on
  the bar and the cheapest source of combination identity — a slice without it
  tests a bar with a hole in it. At six slots this is also the sharpest test of
  whether the bar is too tight.
- **One companion with 3 trigger-conditioned ability slots**, to test whether
  programming a companion reads as build-crafting or as homework.
- **Three weapon families** — heavy arms, dagger and bow — so the slice tests
  whether changing weapon changes the character before any skill does.
- Abilities weighted the way the catalog should be: roughly a third touching
  terrain, the rest conditions, stances, hexes and weapon techniques.
- ~8 sigils, deliberately including two that are near-useless on most builds.
- Named pairs for all 20, on the nameplate, with the title card on first assembly.
- Terrain grid with 5 surfaces: `Foliage`, `Burning`, `Scorched`, `Water`, `Ice`,
  fire spread on. No volumes, **no `Trellis` — verticality is slice 2**, tested
  alone so its cost is legible.
- One named companion each, `Hold`/`Follow`, with a name and dye.
- **Three missions, two maps, with real briefings** — the third mission is the
  second map under a modifier, to test whether a modifier actually changes the
  loadout decision or just the scenery.
- One small hub room where you can see another player and inspect their build.
- No progression systems, no matchmaking, no arenas, no economy.

**Kill criteria, written before building:**

- If testers finish a mission and go straight into the next one **without
  stopping in the hub**, the loadout decision is not real and the loop is hollow.
  This is the primary metric of the slice.
- If they never reroll or rebuild across a session, the draw is not there and no
  content volume fixes it.
- **If Reaver players report the good moments as things that happened *to* the
  map rather than things they did**, the martial integration failed and `Force`
  is not carrying enough weight.
- If two people running the same pair end up with near-identical bars, the build
  space is fake.
- If nobody asks another player what they are running, the expression layer failed.

## Open questions

Questions 1 and 3 from the first draft are now answered — persistent hubs with
instanced zones, and owned gear never matters competitively. What remains:

1. **Does any category become compulsory at six slots?** Not "is six too few" —
   the elite is a wildcard and carries enormous expression on its own. The risk is
   narrower: if every viable build ends up spending a slot on mobility or a
   defensive cooldown, the flexible budget silently drops to three. The fix is kit
   authoring (bundle mobility into damage skills for some builds), not more slots.
2. **Is 8–15 minutes the right mission length?** Short enough that a bad build
   costs nothing, long enough that terrain setup pays off. Below ~8 minutes the
   terrain pillar has no time to matter; above ~20 the loadout decision stops
   recurring often enough to *be* the game. The two pillars pull in opposite
   directions here, and only the slice settles it.
3. **How solo-able is it?** Retinue-fills-party makes it fully solo; a co-op
   requirement makes the world verbs interlock more, at the cost of a much harder
   launch.
4. **Is the Beastbinder a class or an axis?** "Bring beasts" and "bring robots"
   could be retinue unlocks available to everyone, with Beastbinder and Cogwright
   being *better commanders* rather than the gate. Probably healthier, and it
   widens the fantasy for every pair — but it costs those two classes their
   headline identity.
5. **How much authored narrative?** Class-pair reactivity is cheap and pays a lot;
   a full campaign is the single largest cost in this document. Where between?
6. **Arena modes at all in v1?** They are quarantined enough to cut, and cutting
   them buys a year. The counter-argument is that showing off needs an audience
   and PvP is the loudest one.
7. **Does support-as-a-build actually hold up?** Distributed self-heals settle
   that nobody is *obligated* to heal, but not whether someone who *wants* to play
   a healer has enough to work with. If the answer turns out to be no, the fix is a
   ninth class — and reintroducing a dedicated healer risks reintroducing the
   obligation, which is the whole thing this design avoided.
8. **Are eight classes too many for launch?** 56 pairs is the draw, but it is also
   8 ability pools, 8 armor sets, 8 silhouettes and 56 authored names before a
   single one is deep. Six deep classes may beat eight thin ones — and if it comes
   to cutting, cut an *arcane* one, because the martial half was the gap.
9. **Does the terrain system survive its own cost?** It is the most expensive
   pillar and the least connected to the restated draw. Worth asking honestly
   whether a cheaper world-verb system (traversal and utility without a full
   simulated surface grid) buys 80% of pillar 2 for 30% of the cost.

## Appendix: ten builds

Theorycrafted against the systems above, not balanced — the point is to show the
*shape* of the space, and to prove that a six-slot bar with one elite and one heal
can hold a character concept. Abilities here are invented to fill the slots;
treat them as illustrative.

Each is **1 elite + 1 heal + 4**, with a weapon and a named pair.

### Frostbreaker — Reaver/Tidecaller · Heavy Arms

**Glacier's Fault** *(elite)* — your next heavy-arms blow shatters all `Ice` in a
radius; everyone on it is knocked down, damage scaling with how long the ice has
existed · **Bloodwake** *(heal)* · Rime Underfoot · Overbear · Shatterstep ·
Cold Grip

*Build a rink nobody notices, then delete the floor.* Dies to any Emberwright, the
`Ashfall` modifier, and knockdown-immune enemies.

### Thornghost — Veilblade/Verdurist · Dagger

**The Long Patience** *(elite)* — your first strike out of concealment applies
Deep Wound and is unblockable; recharges only by re-entering concealment ·
**Quiet the Wound** *(heal)* · Seedfall · Hamstring · Whisper Chain · Vanish

*You bring your own bushes.* Seedfall is the build; without portable cover you are
hostage to the map. Dies to fire and to `Force` reveals.

### Breakwall — Warden/Reaver · Spear

**Immovable** *(elite)* — stance; cannot be moved or knocked down, and every
attack against you builds adrenaline; ends if you use a movement skill ·
**Bracing Stance** *(heal)* · Pin · Shieldwall · Bellow: Hold Fast · Overbear

*You are the door.* Touches terrain **zero** times — the proof that the
non-terrain vocabulary carries a build alone. Dies to stacked conditions, and to
being walked around.

### Powdersmoke — Cogwright/Veilblade · Gun

**Overclock** *(elite)* — instant reload for 6s; each shot costs you health ·
**Quiet the Wound** *(heal)* · Smokebomb · Braced Shot · Scatterload · Caltrop Line

*Concealment manufactured chemically rather than grown*, so a Veilblade heal that
wants grass works on an iron deck. Same rule, different fiction — the cheapest
kind of breadth there is.

### Thornreaver — Verdurist/Reaver · Heavy Arms ⚠️

**Heartwood** *(elite)* — root yourself, gain heavy armor, and grow `Bramble`
outward each second · **Rootdraw** *(heal)* · Bramble Wall · Entangle · Overbear ·
Wild Swing

*An angry tree with a greatsword* — and a **deliberate trap**. Heavy Arms is a
Reaver line, so every point spent swinging is a point not spent growing, and you
end up a bad Reaver and a weak Verdurist. Kept in the appendix because the trap
should be legible, not hidden.

### Houndwright — Beastbinder/Cogwright · Bow

**Pack Bond** *(elite)* — your retinue's abilities recharge twice as fast and they
share your boons · **Tend** *(heal)* · Balm Arrows · Marking Shot ·
Command: Screen · Field Repair

*Support without a healer class.* Most of this build's power lives in the
**companion loadout screen** — Pack Bond is only as good as the trigger conditions
you programmed.

### Ashgardener — Emberwright/Verdurist · Implement

**Wildfire** *(elite)* — your `Burning` cells never stop spreading while you live;
you take a stacking burn each second they do · **Cauterize** *(heal)* · Kindle ·
Seedfall · Emberstep · Flashover

*Plant the fuel, light it, walk through it, heal by eating it.* The elite is a
commitment: if you cannot close the fight, it kills you.

### Steamcaller — Tidecaller/Emberwright · Implement

**Whiteout** *(elite)* — all water you control flashes to `Steam`; for 10s you see
through steam and nobody else does · **Drink Deep** *(heal)* · Wellspring · Scald ·
Chill Depths · Flashfreeze

*A personal fog of war.* You fight an entire engagement where only you know where
anyone is.

### Galvanist — Tidecaller/Cogwright · Gun

**Arc Column** *(elite)* — a stationary pylon; every `Water` cell connected to it
chains shock continuously · **Drink Deep** *(heal)* · Wellspring · Conduct ·
Grounding Rod · Braced Shot

*Electrify a lake and stand in it safely.* Pure denial. Dies to being pulled off
your own flooded ground.

### Bramblewarden — Warden/Verdurist · Spear

**Hedge Sovereign** *(elite)* — `Bramble` in range counts as your blocked cells,
and allies inside it share your armor profile · **Bracing Stance** *(heal)* ·
Bramble Wall · Pin · Shieldwall · Entangle

**The best thing the exercise produced.** Warden's *Bulwark* scales armor with
adjacent blocked cells; the Verdurist manufactures blocked cells; *Bracing Stance*
scales off the same count. Three systems compound and nobody designed it. If the
shipped game produces discoveries like this, the draw is real.

### What the ten builds taught the design

1. **The elite carries the build.** Every one of these is named by its elite.
   Four flexible slots is plenty when one slot can be a whole thesis.
2. **Portable terrain is mandatory**, or modifiers delete builds instead of
   challenging them.
3. **Shouts are the retinue's control surface**, not a dedicated key.
4. **Same rule, different fiction** (grown cover vs. chemical smoke) is how the
   catalog gets breadth cheaply.
5. **The best builds were not designed, they were found.** Bramblewarden is three
   systems compounding by accident. Protecting the conditions for that — broad
   attributes, free pairing, legible rules — matters more than any single ability.
