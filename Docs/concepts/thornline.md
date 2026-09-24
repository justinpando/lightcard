# Thornline — Concept Design

> **Working title.** Not Light Card Tactics. This is a separate game concept
> parked in this repo because it is the same designer's next idea and because
> several systems already built here — a data-driven effect grammar, a headless
> deterministic sim, a standalone test runner, a simulate-and-score heuristic
> agent, the phantom AI design — transfer almost directly to it (see
> [Reuse from LT Cards](#reuse-from-lt-cards)). Nothing here is committed to; it
> is a design to argue with.

## At a glance

- **Online 5v5 team PvP.** Each player brings one companion. Matches run ~15–20
  minutes. Some maps are lane-like, others are open; neither is the default.
- **You author a character.** A primary and a secondary class out of eight, which
  gives 56 named pairs. Six skill slots, at most one elite (primary only). A weapon
  and four pieces of gear.
- **No attribute points.** Every skill works at full strength. Your primary
  class's passive changes with your secondary.
- **No in-match economy.** No gold, items or last-hitting. **The map is the
  economy**: objectives change the battlefield, bosses hand the whole team a
  permanent skill capture, and you win by bringing down the enemy's Keeper.
- **Skills level up during the match.** A shared team level buys about four
  upgrades, each making one skill ~30% better along its own axis.
- **Your whole bar works from the first second**, elite included.
- **Persistent progression.** Character levels 1–20; the bar grows as you level,
  which doubles as the tutorial. Gear climbs in tier to an early cap, then
  progression is horizontal.
- **Matchmaking by level bracket, then skill.** Ranked play at the cap grants the
  full catalog. Phantoms — AI-piloted copies of real players' characters — fill
  empty seats.
- **Terrain is a system builds lean into**, on an invisible simulation grid:
  arcane classes write to the map; martial classes read it, move through it and
  put people into it.
- **Every class self-heals; there is no healer class.**
- **Later:** PvE missions. **Parked:** sigils.

## The draw

**The fantasy of being a specific, strange, self-authored character — and the
craft of assembling one.**

The moment of joy this game is built to produce is: *"I made a thing that works
and nobody else is running it, and it looks and feels like the character in my
head."* The second moment is showing someone.

Named consequences:

- **It is a MOBA without the economy.** Team PvP with real macro play — routes,
  splits, ambushes, objective races — on a map that changes as you fight over it.
  But your loadout is the whole build; nothing is bought mid-match. See
  [The match](#the-match).
- **PvP first, matched by level.** You level a persistent character and face
  opponents at a similar level. PvE missions are a later layer.
- **Balance means "no dead builds" and "no dominant builds".** The failure states
  are a combination nobody enjoys, and a combination everybody is forced into.

## Design pillars

1. **Combinations have names.** A build is not a spreadsheet row; it is a
   character concept the game recognizes, names, and reacts to.
2. **Your build changes the world you see.** Class abilities are traversal and
   utility verbs, not just combat. Two teams with different builds do not have
   access to the same map.
3. **The map remembers.** Terrain is a system builds can *lean into* — not a tax
   every ability pays. A fight in the right place should leave a scar; a fight in
   a stone corridor is still a good fight.
4. **The map is the economy.** Nothing is bought during a match. Objectives change
   the battlefield instead, and a match is the story of who changed it and how.

## The shape of the game

| Layer | What it is | Why it's here |
| --- | --- | --- |
| **Matches** | Online team PvP: 5v5, each player with a companion, ~15–20 minutes, matched by level and skill | The game |
| **The hub** | The social space between matches. Your character stands here with **your companion visibly beside you**; inspect anyone, copy their build | Where the fantasy is *worn*. Without a place to be seen, build-crafting has no second half |
| **PvE missions** *(later)* | Instanced co-op runs | See [PvE missions (later)](#pve-missions-later) |

## The match

**The map is the economy.** No gold, no items, no last-hitting. Your loadout —
six skills, a weapon, a companion — is the whole build, locked when the match
starts and **live from the first second, elite included**. The elite is often the
lynchpin a build is designed around, so nothing on your bar is ever switched off
to manufacture a turning point. Turning points come from the map, and your skills
grow stronger as the match goes on.

Two precedents: *Heroes of the Storm* removed items and gold, shared experience
across the team, and made the map's objectives the game. GW1's **Guild vs Guild**
had NPC guards, a guild lord to kill, flag-running, and whole team builds designed
around splitting up.

### What the match is built to produce

Four stories, and what the map has to provide for each:

| The story | What the map needs |
| --- | --- |
| *"We went here and did this"* | Several objectives spread across the map, each worth something different, so where you go is a choice |
| *"We split up, then came together"* | Objectives that spawn in pairs on opposite sides, and companions that can hold a point without you |
| *"We ambushed them"* | Fog of war; concealment (foliage, smoke, steam); routes some builds open and others can't see; objective timers both teams know — an ambush needs the enemy to be predictable |
| *"We beat the boss first"* | Neutral PvE challenges both teams race for, worth fighting over |

### Objectives change the battlefield, not the scoreboard

With no gold, rewards are expressed as map changes — which is where the terrain
system earns its cost:

- **Sites** — a Grove, a Well, a Forge — are captured **fastest by putting the
  right surface around them, and slowly by holding their ring**, so the right
  builds are an advantage and no team composition is locked out. Holding one
  reshapes a region for your team, and **terrain a site creates belongs to its
  holder**: cover the Grove grows conceals only the holding team; water the Well
  floods slows only the holder's enemies.
- **Neutral bosses** grant something you can see: a temporary extra companion (a
  captured beast or construct that fights for you until it dies), a map event, or
  vision.
- **Capture.** Bosses are also where skills come from. A team that brings one down
  captures its skill as a permanent unlock for everyone who was there — GW1's
  elite capture, shared. The race for the boss is a race for the match *and* for
  your collection.
- **The win condition is the enemy's Keeper** — a named NPC in their base,
  guarded by NPC wardens. Held sites weaken the guard. Nobody wins on kills.

### Maps

Maps differ in structure, not just in art. **Some are lane-like** — a few fixed
corridors between the bases, with sites along them — and play closer to a
traditional MOBA. **Others are open** — regions joined by routes, where where to
go is the whole question. Neither is the default: the pool mixes them.

Every map also carries its own surfaces and its own build-gated routes (a river
someone can freeze, a thicket someone can burn through), and a **rotating
modifier** — *Drought*, *Verdant Bloom*, *Tempest*, *Ashfall* — that rewrites its
profile. The lobby shows map and modifier before lock-in, so they are part of what
you build for.

**Scale is set by two ratios, not by meters.** In established MOBAs, crossing base
to base takes about a minute, and the map is roughly 20 ability ranges and 9–11
vision radii across. Splits, ambushes and detours all depend on the second ratio:
if a fight's reach covers half the map, there is nowhere to split to and no room
to hide. With ability ranges of 15–20 m and a run speed around 6 m/s, that puts a
5v5 map at **about 350 m on a side**. Greybox it and walk it before trusting the
number.

The first map is [Hollowmere](#first-map-hollowmere).

### Skills level up during the match

Your bar is fixed at lock-in, but it grows over the match. The team shares one
**match level**, earned from objectives, bosses and takedowns — never from
farming, and never split by who landed the last hit. Each match level gives every
player one **upgrade** to spend on one of their six skills. Match level resets
every match; it is separate from your character level, which is persistent and
decides who you are matched against.

- **Every skill has one upgrade axis, and an upgrade makes it about 30% better
  along it** — area, duration, recharge, range, distance, charges, windup.
  Bramble Wall grows longer; Entangle holds longer; Kindle burns wider; Overbear
  shoves further; Braced Shot winds up faster; Tend recharges sooner. The axis is
  a data field on the skill, not a second version of it.
- **Damage and healing can be axes too, tuned to be worth no more than the
  alternatives.** They matter in every match, so they usually want a smaller
  percentage than a situational axis to come out even. Prefer axes that matter
  more in some matches than others: a wider Kindle pays off against a team that
  groups up, a longer Entangle against a team that runs.
- **You can't upgrade everything.** About four upgrades a match for six skills.
  Which skills you upgrade, and in what order, is the in-match decision — how a
  build adapts to the other team and to what the map has become.
- **The elite has an upgrade axis too**, and works from the first second either
  way.
- **Persistent progression can widen it:** over time, some skills unlock a second
  axis to choose between. Ranked play at the cap grants all of them.

This is the one form of "putting points into things" that fits the design: it is
scoped to a single skill, so it never rewards stacking a group of skills; it
happens in the match, so it doesn't re-tax the build you made before it; and it
resets every match, so it never accumulates into a character sheet. It gives
matches a weak-to-strong arc with no gold, no shop and no last hits.

### Companions are how a team splits

A companion on `Hold` guards a point while its player is elsewhere. That makes the
companion a macro tool, not just a combat add-on: five players with five
companions can split more ways than five players alone, and *"who do we leave the
Well to?"* becomes a real decision. Companions hold; they cannot capture. A real
split still needs people.

### Keeping it from snowballing

- **Map rewards are temporary and local.** Regions regrow, floods drain, captured
  beasts die.
- **The Keeper's guard regenerates**, so an early lead has to be converted, not
  banked.
- **Objectives spawn biased toward the side holding fewer sites.**
- **The trailing team earns match levels faster** from the same objectives, so a
  lost opening costs tempo rather than the match.

### Shape

- **5v5, each player with one companion.** Splitting needs bodies, and five is
  where macro play gets rich; companions add holders, not decision-makers.
- **~15–20 minutes.** Long enough for a map to change; short enough that the
  loadout decision comes around several times an evening.
- **The lobby shows the other team's pairs and weapons** before lock-in. You
  can't see their bars, but you can build against their shape.

## First map: Hollowmere

An open map, 350 m square, rotationally symmetric, with the bases in the southwest
and northeast corners. Interactive version: [hollowmere.html](hollowmere.html) —
toggle your team's classes to see which routes open.

### Layout

- **The Mere** — a lake in the centre, ringed by marsh shallows. The **Heron
  King** sits on an island in the middle. Two stone **causeways** run from the
  island to the Grove's and the Well's gates: the only way onto the island for a
  team that can't cross water.
- **The Grove** (northwest) — a wooded plateau ringed by cliffs, with gates south,
  east, and toward its causeway. Captured fast by growing, slowly by holding;
  burning it leaves it neutral.
- **The Well** (southeast) — stone flats around a dry cistern, with low walls and
  gates west, north, and toward its causeway. Captured fast by flooding, slowly by
  holding.
- **The Bramblecut** — a thorn thicket between each base and the Mere. Burned, it
  opens a direct route from that base to the near causeway.
- **Four beast dens** off the main paths, two on each side.
- **Keepers**, each with three wardens.

### Routes are the build question

| From your base to… | Default | Bramblecut burned *(Emberwright)* | Across the Mere *(Tidecaller freezes, Verdurist grows reeds, Cogwright ziplines)* |
| --- | --- | --- | --- |
| The Heron King | ~48 s | ~38 s | ~24 s |
| The enemy Keeper | ~73 s | ~62 s | ~52 s |
| The Grove or the Well | ~38 s | — | — |

Reaching the boss in half the time is the case for bringing someone who can cross
water. It is counterable because the route is terrain: an Emberwright melts the
ice under you, a Reaver shatters it, fire burns the reeds. And the lobby shows
both teams which routes the other can open.

### Objectives and timers

- **1:00 — dens wake.** Each gives a captured beast, a temporary extra companion.
  A den returns 1:30 after it's taken — twice as fast on the side of the team
  holding fewer sites.
- **2:30 — the Grove and the Well wake together**, about 200 m apart on opposite
  corners. Both reset to neutral every 3:00.
  - *Grove held:* cover grows along both of its approaches and around its
    causeway, and only the holder can hide in it.
  - *Well held:* the lowlands flood knee-deep, slowing only the holder's enemies.
  - *Either held:* one warden leaves the enemy Keeper's guard. The third warden
    always stands.
- **6:00 — the Heron King wakes**, and returns 4:00 after it falls. The team that
  brings it down captures its skill, and **the Mere drains for 90 s**: the centre
  becomes open ground for both teams.

### How a match is won here

1. **Hold sites** to strip wardens from the enemy Keeper.
2. **Take the Heron King** to drain the Mere.
3. **Push the Keeper** through the drained centre before it refills.

The drain opens the centre to both teams, so the side that lost the boss fight
gets a counterattack route too.

### Where the four stories happen

| The story | On Hollowmere |
| --- | --- |
| *"We went here and did this"* | Dens, sites and the boss pull in different directions from the first minute |
| *"We split up, then came together"* | The Grove and the Well wake at once, 200 m apart; a companion on `Hold` keeps one while the team takes the other |
| *"We ambushed them"* | The causeways are chokepoints lined with reeds; a burned Bramblecut is a route the enemy may not have seen open; the Grove's cover hides only its holder |
| *"We beat the boss first"* | The island, and the race to cross the Mere |

### Modifiers on Hollowmere

- **Drought** — the Mere shrinks to a pond and the shallows dry to open ground.
  The island is a short wade from anywhere; the Well needs twice the water.
- **Verdant Bloom** — cover everywhere; every approach is an ambush, and the Grove
  captures fast.
- **Tempest** — rain pools across the map; the Well floods easily, shock chains
  travel far, and fire struggles to spread.
- **Ashfall** — nothing grows for the first two minutes, so the first Grove can
  only be taken the slow way.

### What to check in greybox

- **Base to base is ~73 s the long way**, longer than the ~60 s target. Walk it;
  if it drags, tighten the paths around the Grove and the Well before shrinking
  the Mere.
- **Whether 24 s to the boss is too decisive.** If a team that can cross water
  wins every boss race, the fixes are longer boss fights, a shorter default
  route, or ice that melts faster.

## Matchmaking

You level a persistent character and face opponents at a similar level. The
proven model is *World of Tanks*: a long persistent climb, and matchmaking that
puts you against tanks within a tier or two of yours.

### Level brackets, then skill

- **Few, wide brackets:** 1–5, 6–10, 11–15, 16–19, and the cap. Narrow brackets
  fragment the queue.
- **Skill rating inside each bracket.** Level says what you've unlocked; rating
  says how good you are. Matching on level alone lets a veteran's new character
  stomp real beginners.
- **Gear scales to the bracket's baseline.** Otherwise you get WoW's battleground
  "twinks": players parked at the top of a bracket in the best possible gear,
  farming newer ones. Gear *numbers* don't vary within a bracket; gear *options*
  — runes, insignia, looks — still come with you.
- **Parties queue in their highest member's bracket.** Everyone is scaled to that
  bracket's baseline numbers but brings only the slots and skills they have. A
  low-level member is a real handicap, and the party chose it.

### Low brackets are a smaller game

A level-4 match is everyone with three slots, one class and a small catalog. New
players learn against other new players, in a simpler version of the game. The
full combination space — 56 pairs × hundreds of skills — only exists at the cap,
which is where balance effort goes.

### Ranked at the cap

In PvP, options are power: more unlocked skills means more ways to counter-build.
So **ranked play at the cap grants every player the full catalog and cap-tier
gear**, as GW1 did for its PvP characters. Unranked and bracket play use your own
unlocks.

### Phantoms fill the queue

Brackets × ratings divide the player base, and an indie-sized population can leave
brackets empty. [LT Cards' phantoms](../design/phantom-ai.md) are the answer: a
phantom is a real player's posted character, piloted by the heuristic agent,
filling an empty seat — always labeled as a phantom, never passed off as a person.

### The balance cost

PvP needs no *dominant* builds as well as no dead ones, and 56 pairs × hundreds of
skills is a balance surface no team can fully cover. Brackets help, because the
space stays small until the cap. Phantom-vs-phantom sweeps (the repo's
`MatchRunner`) test the rest. When PvE arrives, it is the place where strong,
strange, over-the-top builds are allowed.

## Classes

**Eight at launch — four martial, four arcane.** Classes are defined first by a
**verb they perform on the world**, and that verb works out of combat too. Left
alone, that method favors people who cast spells at the ground; the balance is a
different relationship to the same system:
**arcane classes write to the map; martial classes read it, move through it, and
put people into it** (see [How martials touch the matrix](#how-martials-touch-the-matrix)).

### Martial

| Class | World verb | In combat | Out of combat | Primary passive — reads your secondary |
| --- | --- | --- | --- | --- |
| **Warden** | **Holds** — raises cover, body-blocks, anchors ground | Frontline, peel, objective soak | Braces collapsing structures; shields allies through a hazard; holds a gate open | *Bulwark* — armor scales with the cover around you, including anything your secondary raises, grows or installs |
| **Reaver** | **Displaces** — knocks, throws, charges, shatters | Aggressive melee, the `Force` specialist | Breaks sealed walls and rubble; charges gaps | *Momentum* — shove someone into ground your secondary made and it triggers at once, at full strength |
| **Veilblade** | **Slips** — concealment, verticality, gaps | Burst, flanks, assassination | Free-climbs without a `Trellis`; squeezes through gaps; scouts unseen | *Stillness* — anything your secondary creates is cover you can vanish into: bramble, steam, smoke, a turret, your own beast |
| **Beastbinder** | **Inhabits** — beasts scout, flush, hold; bow and preparations | Ranged martial, companion depth | Beasts track scents; some fit through gaps | *Kinship* — your beast learns one skill from your secondary: a hound that burns, a wolf that freezes, a hawk that carries smoke |

### Arcane and artifice

| Class | World verb | In combat | Out of combat | Primary passive — reads your secondary |
| --- | --- | --- | --- | --- |
| **Verdurist** ("green mage") | **Grows** — vines, brambles, grass, trellises | Control, area denial | Grows a climbable trellis on tagged walls | *Rootedness* — you can grow on any surface your secondary makes: ice, oil, ash, rubble |
| **Emberwright** | **Ignites** — burns foliage, spreads fire | Damage over time, zoning | Burns away overgrowth sealing a route | *Combustion* — surfaces your secondary creates burn hotter and longer when you light them |
| **Tidecaller** | **Changes state** — floods, freezes, douses | Mobility surfaces, chain setup | Freezes a river into a bridge | *Current* — effects your secondary applies to someone in your water spread to everyone in it |
| **Cogwright** | **Installs** — turrets, ziplines, drones | Siege, vision, infrastructure | Ziplines across gaps; drones scout ahead | *Fabrication* — your installations carry your secondary's trait: ember turrets, overgrown beacons, spiked barricades |

Eight classes give **56 ordered pairs**, and martial/arcane pairs in *both*
directions are the richest part of the space: a `Reaver/Tidecaller` freezes the
ground and shatters it under you; a `Tidecaller/Reaver` is a mage who has learned
to shoulder-check.

Out-of-combat verbs mean **routes depend on builds**: a team with a Tidecaller can
cross the river, a team with an Emberwright can burn through the thicket. That is
pillar 2 working as strategy — and it means a team whose five builds share no
verbs will find parts of the map closed.

### Primary passives must read your secondary

**Every primary passive is written in terms of what your secondary class makes or
does.** Two reasons:

- A passive that improves your own class ("your fires spread further") rewards
  stacking your primary — the same dominant-strategy trap that ruled out
  attribute points.
- A passive that reads the secondary gives all 56 pairs a mechanical identity from
  **eight rules instead of 56 special cases**. A `Reaver/Tidecaller` shoving
  someone onto ice and a `Reaver/Emberwright` shoving someone into fire are the
  same sentence, *Momentum*, producing two different characters.

The model case: the Warden's *Bulwark* scales armor with the cover around you, and
the Verdurist manufactures cover — nobody authored that interaction; it falls out
of two systems meeting. **Test for every new passive:** read it with each of the
seven possible secondaries substituted in. If it means the same thing for all
seven, rewrite it.

## The skill bar and multiclassing

- **6 slots**, chosen out of combat and locked at match start. One is almost
  always your self-heal — see [Healing](#healing).
- **At most 1 elite**, from your primary class. It makes six slots a budget rather
  than a list of favorites, and it forces the build to have a *thesis*.
- **Primary** grants its full skill pool including elites, its armor class, and
  its passive. **Secondary** grants non-elite skills only. Secondary is swappable
  freely between matches; primary is per character.
- **No attribute points.** Every skill works at full strength, whichever class it
  came from.

### Why there are no attribute points

GW1's attributes were the channel its leveling reward flowed through. This game
levels too, but its leveling rewards must favor no group of skills (see
[Leveling, unlocks and gear](#leveling-unlocks-and-gear)), and attributes are
exactly a reward aimed at groups of skills. They also fail on their own terms:

- **They duplicate the bar.** Six slots already make depth-versus-breadth the
  central decision — every secondary skill costs a slot a primary skill could
  have had. Charging it again in points double-taxes multiclassing, which is the
  thing the game exists to make attractive.
- **Any bonus scoped to a group of skills makes stacking that group the dominant
  move.** That rules out simplified versions too: a single "Focus" line would push
  every build toward five skills from one line.

The only bonuses that don't reward stacking are none at all, or one scoped to a
single skill. The design has two of those — **the elite**, and
[in-match upgrades](#skills-level-up-during-the-match) — and that is where "my
character is *really* good at this" lives. What remains of attributes has no
numbers: **weapon training is yes/no**, and **the primary passive is automatic**.

### Why six slots

- **Matches are 15–20 minutes.** A bar you can fully express inside one fight
  beats a bar you are still discovering at minute nine.
- **Six is a gamepad** — four face buttons plus two shoulders, or one clean radial.
- **Scarcity is the expression engine.** With six, taking one skill from your
  secondary is a *statement about your character*, not a leftover.
- **It shortens the read.** Inspecting someone's build should be a glance.

**The elite is not overhead — it is the largest single expression surface in the
game.** An elite can be a transformation, a persistent aura, a new resource rule,
a summon, a technique that rewrites your chain, a terrain effect at a scale no
normal skill reaches. Two characters that share five skills and differ only in
their elite are different characters. So the budget is **one elite (a wildcard),
one heal (a real choice you may decline), and four slots.**

The residual risk is that a *category* — mobility, or a defensive cooldown —
becomes compulsory and quietly eats the flexible slots. The answer is to stop the
bar being the only place abilities live, not to add slots:

- The **weapon** carries one native skill outside the six.
- The **companion** carries its own abilities, so team utility need not come off
  your bar.
- Class kits are authored so no *category* is mandatory — some builds get mobility
  bundled into a damage skill rather than as a separate tax.

The primary passive is what stops "everyone runs the same secondary", and because
every passive reads the secondary, `Verdurist/Tidecaller` and
`Tidecaller/Verdurist` are different characters from identical skill access.

### Two economies

Martial abilities run on **adrenaline** (built by connecting hits, spent in
bursts, lost out of combat). Arcane abilities run on **energy** (a regenerating
pool). Every class has some of both, weighted heavily toward one.

GW1's most famous failure lives here: a martial primary with an arcane secondary
had almost no energy, so half the pairs were dead on arrival. The answer:

- Every class has a small universal energy pool — enough for two or three
  secondary-class utility casts per fight, never enough to be a mage.
- A Focus accessory can be spent on more energy.

A `Reaver/Emberwright` should land three strikes and spend them on one detonation
— not be a warrior who cannot afford their own second class.

### Named pairs

**The cheapest, highest-leverage feature in this document.** All 56 ordered pairs
get an authored in-world name, shown on your nameplate, with its own emote and a
title card the first time you assemble one.

| Pair | Name | The character it is |
| --- | --- | --- |
| Verdurist / Emberwright | **Blightburner** | Grows the fuel, then lights it. Slash-and-burn zoning |
| Emberwright / Verdurist | **Ashgardener** | Burns first and plants in the ruin |
| Verdurist / Tidecaller | **Fenwright** | Floods, then grows reeds in it. The concealment specialist |
| Tidecaller / Emberwright | **Steamcaller** | Lives inside steam clouds and fights where nobody can see |
| Beastbinder / Cogwright | **Houndwright** | Machine-augmented animals. Your pack has brass in it |
| Cogwright / Beastbinder | **Menagerist** | A collector who builds habitats and lets things out of them |
| Verdurist / Beastbinder | **Grovekeeper** | Grows the terrain the pack hunts through |
| Warden / Verdurist | **Bramblewarden** | A wall that grows more wall |
| Warden / Emberwright | **Cinderguard** | Holds ground by making the ground unholdable |
| Cogwright / Emberwright | **Boilerwright** | Oil, pressure, and things that should not be indoors |
| Reaver / Tidecaller | **Frostbreaker** | Freezes the ground, then shatters it under you |
| Reaver / Emberwright | **Pyreblade** | Oiled weapon, and it throws you into what it lit |
| Veilblade / Verdurist | **Thornghost** | Grows its own hiding places and hunts from grass it planted |
| Veilblade / Tidecaller | **Mistwalker** | Makes the steam, lives in it, leaves before it clears |
| Warden / Reaver | **Breakwall** | Holds the line by moving it forward |
| Beastbinder / Emberwright | **Emberpack** | Burning arrows and hounds that drive things into the fire |

**Names are recognition, not rules.** A pair name grants nothing by itself;
mechanical pair identity comes from the primary passive reading the secondary, so
there are never 56 hardcoded special cases in the balance surface.

### Build codes

A build (class pair, six skills, weapon, gear template) serializes to a short
text code, paste-able in chat and in the hub. Trying someone else's character
should take ten seconds, so unlocks are account-wide, alts are free and unlimited,
and any character can hold several named build templates to swap between.

## Elements, surfaces and the combo grammar

Specified as a closed data table rather than per-ability special cases, so new
content ships as data.

**A cell** is one square of the terrain simulation grid, ~0.5 m across, holding
one surface, an optional volume above it, timers, and who made it. **The grid is
invisible** — a data substrate, not a tactics grid. Movement is free and analog;
adjacent cells sharing a state render as one patch with an organic edge, so
players see a burning *field*, never lit squares.

> **Vocabulary rule:** "cell" is an engineering word and never appears in
> player-facing text. Abilities and passives speak in meters and plain language —
> *"armor scales with the cover around you"*, not *"per adjacent blocked cell"*.

### Areas rasterize; they do not snap

An ability's footprint is computed from your exact position, not snapped to the
grid, so standing 20 cm to the left changes which ground is caught.

- **Fine positioning matters**, so a 0.5 m simulation never flattens action
  movement into a tactics game's steps.
- **Not snapping is what keeps the grid invisible.** A snapped footprint would
  draw the lattice for the player, one cast at a time.
- **The targeting preview shows the true footprint**, so variation is visible
  rather than surprising.
- **The server rasterizes**; the client preview is advisory. Expect occasional
  one-cell disagreement at the edge, and author abilities so a single edge cell
  never decides a fight.
- **The headless sim rasterizes identically**, or balance sweeps measure a
  different game from the one that ships.

### Surfaces, volumes, elements

**Surfaces** (one per cell, mutually exclusive):
`Bare` · `Foliage` · `Bramble` · `Water` · `Ice` · `Oil` · `Mud` · `Burning` · `Scorched`

**Volumes** (above a cell, stack with any surface): `Smoke` · `Steam` · `Pollen` · `Gas`

**Elements** (what abilities apply): `Fire` · `Frost` · `Shock` · `Water` ·
`Growth` · `Force`

### The interaction matrix

| ↓ Element applied to → | Foliage | Water | Oil | Ice | Mud | Burning |
| --- | --- | --- | --- | --- | --- | --- |
| **Fire** | ignites → `Burning`, **spreads cell to cell** | → `Steam` volume (vision block) | → `Burning`, instant, whole slick | melts → `Water` | — | intensifies, spreads further |
| **Frost** | wilts → `Bare` | → `Ice` (slide movement) | brittle — shatters on `Force` | thickens, +duration | hardens → fast walkable | douses → `Scorched` |
| **Shock** | — | **chains to every unit touching the pool** | ignites → `Burning` | chains + stun | grounded, halved | — |
| **Water** | soaks — fireproof ~8s | deepens → `Deep Water` | spreads the slick outward | melts → `Water` | — | douses → `Bare` |
| **Growth** | thickens → `Bramble` | → `Reeds` (walkable *and* concealing) | — | — | grows at double rate | — |
| **Force** | flattens — **reveals anyone concealed** | displaces units | spreads the slick | shatters → `Bare` + slow | — | scatters embers to adjacent ground |

`Scorched` is the pressure valve: nothing grows on it for ~20s. It is how fire
answers growth.

- **Concealment:** `Foliage`, `Reeds` and `Smoke` conceal. You are revealed by
  attacking, by `Force` flattening your cover, or by it catching fire. Hiding is a
  real but *burnable* resource.
- **Climbing:** `Growth` on a tagged wall produces a `Trellis`, climbable ~15s.
  Maps are authored with "sometimes-vertical" walls, so some routes are
  player-created — the clearest expression of pillar 2, and the most expensive
  feature here.
- **Fire spreads.** It is a cellular automaton over flammable surfaces, not an
  aura. You can start one you cannot control.

### Portable terrain is a required category

Every terrain-adjacent class needs at least one cheap ability that **manufactures
its preferred surface on demand** — Seedfall grows foliage at range, Wellspring
puts down water, Kindle lights a patch, Smokebomb makes a volume chemically, Rime
Underfoot freezes what is already there.

Without it, terrain builds are hostage to the map: a `Drought` or `Ashfall`
modifier doesn't *challenge* a build, it *deletes* one. With it, the modifier does
what it should — your setup costs more slots and more time.

### How martials touch the matrix

The trap with a terrain-and-element core is martial classes waiting for the mages
to finish decorating. Three verbs prevent it, all using machinery the matrix
already has:

1. **`Force` is the martial element.** A Reaver does not set the fire — a Reaver
   *throws you into it*, shatters the `Ice` under your feet, scatters a fire into
   four, and flattens the `Foliage` you were hiding in. A martial's power scales
   with how interesting the map is.
2. **Terrain that walls a caster is a road for a martial.** Martials buy
   traversal mastery: run on `Ice` without sliding, vault `Bramble`, cross
   `Burning` at a stacking cost, climb where there is no `Trellis`. The Veilblade's
   whole kit is *moving through the map other people made*.
3. **Carried elements, not cast ones.** Weapon coatings, oils and the Beastbinder's
   arrow preparations let a martial apply `Fire`, `Frost` or `Shock` by having
   *prepared*. That opens every matrix row to a pure martial build.

An arcane build asks "what should this ground become?"; a martial build asks
"what is this ground already, and who can I put on it?" Same table, opposite
grip.

## Beyond terrain: the ability vocabulary

Terrain is one system, not a tax every ability pays. Most abilities should be
legibly good at something that has nothing to do with the ground:

| Family | What it is | Mostly |
| --- | --- | --- |
| **Conditions** | Stacking physical afflictions — Bleeding, Crippled, Blind, Weakness, Deep Wound, Dazed | Martial |
| **Hexes** | Targeted curses — drains, reversals, damage mirrors, action taxes | Arcane |
| **Enchantments and wards** | Persistent buffs on self, ally, or a patch of ground | Arcane |
| **Stances** | Self-only postures, **one at a time**, so switching is a real decision | Martial |
| **Shouts and commands** | Short team-wide effects — and the natural hook for directing companions | Both |
| **Interrupts** | Timing plays that punish casts; the main counterplay to arcane builds | Both |
| **Weapon techniques** | Chain-enders, positional strikes, stance transitions | Martial |

**Authoring rule: terrain-interacting abilities are a minority — roughly a quarter
to a third of the catalog.** Otherwise every class is a landscaper; builds
homogenize toward setup; maps indoors, on stone, on ships and at night become
dead content; and a player who wants to be a duelist with a spear and a grudge is
told the game is about gardening. The matrix is spice: the reason a *particular*
build feels extraordinary on a *particular* map.

## Healing

**Every class has its own self-heal, and there is no dedicated healer class.**

1. **Your self-heal costs a slot** — one of six, competing with everything else.
   Leaving it out is a real, aggressive build statement: at six slots it costs a
   sixth of your character.
2. **Nobody is obligated.** A mandatory healer is a queue tax and a person who
   didn't get to pick their fantasy. Distributed sustain means any five characters
   form a viable team. The healer *fantasy* survives as a **build, not a class**:
   pair and skill choice let a Cogwright specialize into field repair or a
   Verdurist into healing groves.
3. **Heals read the map.** Most self-heals hook the terrain system, so sustain is
   part of the terrain game; enemies can **attack your healing** (burn the foliage
   you root in, douse the water you drink from); and knocking a healer off their
   ground is counterplay for `Force` with no new machinery.

| Class | Self-heal | Cost shape | Map hook |
| --- | --- | --- | --- |
| **Warden** | *Bracing Stance* — heal while planted, broken by movement | Immobility | Scales with the cover around you |
| **Reaver** | *Bloodwake* — spend adrenaline on a hit to heal | Must be winning | Doubles if the target was shoved into a hazard |
| **Veilblade** | *Quiet the Wound* — regen while concealed, ends on reveal | Disengagement | Requires `Foliage`, `Reeds` or `Smoke` |
| **Beastbinder** | *Tend* — your beast returns and heals you both | The beast stops fighting | None; the reliable one |
| **Verdurist** | *Rootdraw* — root yourself, heal fast | Rooted, and flammable | Scales with surrounding `Foliage`/`Bramble` |
| **Emberwright** | *Cauterize* — consume a nearby fire for a large instant heal | Extinguishes your own zoning | Requires `Burning` |
| **Tidecaller** | *Drink Deep* — heal scaling with the water you stand in | Weak on dry ground | Scales with `Water`; self-solving |
| **Cogwright** | *Field Repair* — a beacon that heals anyone near it | Stationary, destructible | None — the one team-facing heal |

**Every heal has a floor that works on bare ground.** Terrain amplifies, never
gates; the lobby shows the map, so *"this map is dry, my Tidecaller heal will be
weak"* is a planning input.

**The heal slot is a combination engine.** A secondary grants its non-elite skills,
so you can take someone else's self-heal, and each has a different cost shape. A
`Reaver/Veilblade` fights hard and vanishes into the grass to recover; a
`Reaver/Tidecaller` makes its own puddle and heals standing in the fight; a
`Veilblade/Emberwright` heals by putting out the fire it is hiding in the smoke
of. One slot, eight options, 56 pairs — a small closed set whose combinations
carry the expression.

**Out-of-combat regeneration is fast and free.** The heal slot matters *during* a
fight, not after one.

## Companions

Your companion is part of your character's statement: it stands beside you in the
hub and fights beside you in the match. In matches, **every player brings exactly
one companion** — a beast, a construct, a scout drone, or your named companion.

### Built like you, at half size

A companion has the same four gear slots as you and a **3-slot ability bar** —
deliberately *half a character*: three to your six. It is a real build you
author, not a pet with a trick, and not a second character to pilot. Three is
enough for a *role* — a heal, a control, a threat — so a companion can carry its
own sustain.

### Programmed, not micromanaged

Each companion ability carries a **trigger condition** chosen alongside it — *when
an enemy is rooted*, *when I drop below half health*, *when an ally stands in
fire*, *on first contact*. You author the behavior before the match and watch it
play out. This is what keeps ten companions on the field tractable, and it is a
direct lift of the Condition/Trigger grammar in this repo's card engine.

**Named companions** are the roleplay slot: a name you give them, a persistent
appearance, dyeable barding, and a few traits they grow across matches. That
specific animal, that you trained.

### Control

Four stances — **Follow · Hold · Hunt · Screen** — plus one directed command on a
~6s cooldown. Stance-level by design: the moment direct micro outperforms stance
play, "commanding a crew" stops feeling like commanding.

**Shouts are the real command surface.** *Hold Fast* (allies can't be knocked
down) or *Command: Screen* does real combat work **and** redirects companions, so
command competes for bar slots like everything else: a player who wants to command
more builds for it; a player who doesn't leaves a stance set.

The **Beastbinder** buys depth rather than everyone being forced into it: direct
control of its beast, and that beast's abilities surfaced onto its own bar, where
they compete for slots.

## Equipment

Four slots: **Weapon · Armor · Accessory · Accessory**. Gear climbs in tier on the
way to the level cap; what the slots *decide* is always what you do, not how big
your numbers are. Appearance is decoupled from stats — making players choose
between looking right and playing right is self-defeating.

| Slot | What it decides |
| --- | --- |
| **Weapon** | Your attack's rhythm and shape, which abilities you can slot, and one weapon-native skill outside your six |
| **Armor** | Defense *profile*, not amount: trade-offs (+vs fire / −vs shock). 2 rune slots (resistances and small utility effects) + 1 insignia (a situational passive). **Armor is the class silhouette** — dyeable, per piece |
| **Accessory ×2** | Simple and readable: a Focus that shapes your resource economy, and small defensive or utility effects |

### Weapon families

Seven martial families plus implements. Each has its own rhythm, resource quirk
and gated abilities, so **changing weapon changes how the character plays before a
single skill changes.**

| Family | Rhythm | Resource quirk | Gates | Trained by |
| --- | --- | --- | --- | --- |
| **Sword** | Balanced 3-hit chain; interruptible at known points | Steady adrenaline | Ripostes, parries, chain-enders | **Reaver** |
| **Heavy arms** (axe, hammer, greatsword) | Slow, committed, wide arcs | Adrenaline in big lumps; whiffing hurts | Knockdowns, armor-breaks, the biggest `Force` | **Reaver** |
| **Spear** | Mid-range thrusts; throwable | Steady, rewards spacing | Pins, impales | **Warden** |
| **Dagger** (paired) | Fastest; off-hand triggers on-hit effects | Fast small adrenaline | Positional strikes, off-hand chains, coatings | **Veilblade** |
| **Martial arts** (unarmed) | Flowing chains, high mobility | Fastest adrenaline in the game | Grapples, throws, stance transitions | **Veilblade** |
| **Bow** | Draw and release, arcing | Draw time is the cost | **Preparations** — elemental arrows | **Beastbinder** |
| **Gun** | Burst, flat trajectory, armor-piercing | **Reload** — a rhythm, not a cooldown | Ammunition types, braced shots | **Cogwright** |
| *Implements* (staff, scepter, focus) | Cast-facing | Energy | Nothing martial | *any class* |

- **Anyone can hold anything; only a class that trains it wields it well.** A
  Verdurist can carry a greatsword, but unless one of their classes is the Reaver
  they swing it like someone who has never held one. *"I want to actually swing
  this"* is a reason to take a martial secondary — and taking it is enough.
- **Weapons gate abilities.** Hammer knockdowns need a hammer, off-hand chains
  need daggers, preparations need a bow.
- **Weapon training is how an arcane class buys into the martial half**, and
  martial arts' grapples and throws give `Force` a second home beyond the Reaver.
- **Traps must be legible.** A Verdurist/Tidecaller with a greatsword is a bad
  build, and that's fine — an open combination space contains bad builds. But the
  loadout screen says so *plainly, before the match*: *"neither of your classes
  trains this."*

## Leveling, unlocks and gear

**A short climb to a level-20 cap, then horizontal progression** — GW1's shape.

### Leveling rewards favor no build

Nothing earned by leveling may make one group of skills stronger than another —
that brings back the stacking trap. So no attribute points, per-line bonuses or
talent trees. Instead, the bar grows:

| Level | Unlock | Why it's here |
| --- | --- | --- |
| 1 | Primary class, three skill slots, a weapon | You learn one class's verbs before you learn to combine them |
| 3 | First companion, with one ability slot | Companions arrive while fights are still simple |
| 5 | **Secondary class** — a story beat: someone from another discipline takes you on | Multiclassing arrives once you know what your primary is for. Your pair gets its name here |
| 7 | Fourth skill slot | |
| 10 | Fifth skill slot; companion's second ability slot | |
| 14 | **Elite slot** — your first elite is captured from a boss | The build's thesis arrives once you know what builds are |
| 16 | Companion's third ability slot | |
| 20 | Cap. Full bar, full companion | From here, progression is horizontal |
| Every level | A little more health and energy | Flat and universal |

**Leveling is the tutorial.** The bar grows in the order a new player can absorb
it — one class, then a second, then a thesis — so nobody meets 56 pairs and an
elite on their first evening. Low brackets play the smaller game (see
[Matchmaking](#matchmaking)).

### Skills are never bought

Skills come from level milestones, match rewards, and capture from bosses. Higher
brackets field bosses carrying rarer skills, and maps advertise which bosses carry
what — so chasing the piece that completes a build concept is a plan, not a
loot-table lottery.

### Gear: vertical until the cap, horizontal after

Gear tiers rise with level; a level-12 sword hits harder than a level-4 one, and
upgrades are a steady stream of match rewards on the way up. (Within a match,
gear numbers are scaled to the bracket.) Two rules keep it from becoming a grind:

- **Cap-tier gear is common.** You reach the top tier by playing to the cap, not
  by farming. GW1's lesson: max-stat armor was cheap; the expensive armor was the
  armor that *looked* better.
- **After the cap, gear changes what you do or how you look, never how big your
  numbers are.** Rare items carry an unusual rune combination, insignia or look —
  never a higher tier.

### The power ceiling

- **The ceiling is the level cap, and the cap comes early.** Past it, progression
  is horizontal: skills, companions, cosmetics.
- **Ranked play at the cap normalizes** gear and access, so the competitive game
  is never a grind gate.
- **The world reacts to your pair.** Hub NPCs, faction greetings and the match
  announcer key off your named pair.

This will be under permanent pressure: every retention pass will propose raising
the cap or adding a gear tier above it. The concept does not survive that — the
instant playtime *past the cap* buys numbers, build choice becomes build
*obligation*.

## Legibility

**Hidden six-slot builds can destroy counterplay.** In Dota you know what every
hero does; here you know someone is a Frostbreaker, which narrows it to a few
hundred possibilities. Four answers:

- **The lobby shows pairs and weapons before lock-in.** Pair, weapon and armor
  silhouette constrain the space hard — a staff Verdurist is not doing melee burst.
- **Seen skills are logged.** Any ability used on or near you is added to a
  per-opponent panel for the rest of the match, marked once upgraded. A scout drone
  can buy information.
- **Terrain is honest.** The most decisive actions — the burning field, the
  bramble wall, the flooded lowland — are large, visible and lasting.
- **Elites are announced** on the opposing team's screen the first time each is
  used.

And the expressive version: **your build should be legible enough to be
admired** — pair name on the nameplate, inspect and build codes in the hub.

## Technical spine

### The terrain grid is the architecture

A uniform grid of ~0.5 m cells carrying `{surface, volume, timers, owner}`, about
2 bytes per cell. A 350 m × 350 m map is ~490,000 cells — about 1 MB of
authoritative state, replicated as deltas. Only cells that are changing (burning,
spreading, timing out) simulate each tick, so map size mostly costs memory, not
simulation time. Everything falls out of it:

- **Combos** are a lookup into the matrix, per cell.
- **Fire spread** is a cellular automaton at ~5 Hz over the same grid.
- **Pathing uses flow fields over the grid**, not navmesh rebuilds — vines and
  walls change cell costs instantly with no bake, the operation navmeshes are
  worst at. The most important technical call in this document.
- **Companion AI reads the same grid**, so companions understand fire and bramble
  for free.

### The rest

| Concern | Approach | Risk |
| --- | --- | --- |
| Networking | Server-authoritative, fixed ~30 Hz tick. 10 players plus companions per instanced match; nothing persists but results | Moderate |
| Matchmaking and ranking | Level brackets, skill rating within them, phantom backfill, party rules | Moderate: well understood, but a live service to run |
| Hub | Higher player count, **no terrain simulation, no combat** — a separate, cheaper server path | Low, if kept combat-free |
| Vision / concealment | Server-side visibility culling; hidden enemies are never sent to the client | Non-negotiable: ambushes are a core story, and client-side fog is a wallhack |
| Climbing | Traversal volumes spawned from `Trellis` cells, with authored wall tagging | **High.** Dynamic verticality is where animation, pathing and level design all get expensive at once |
| Content pipeline | Abilities and the matrix as data, with an editor and a headless test suite | The design depends on non-engineers shipping build-space content |
| Engine | Unity 6 matches existing team knowledge; a networked action game still needs a real netcode stack on top | Unreal is stronger for traversal and animation; team familiarity is the tiebreaker |

### Reuse from LT Cards

| From this repo | To Thornline |
| --- | --- |
| `EffectDef` grammar (Condition/Trigger/Target/Effect) as *data, not code* | Ability definitions, primary passives, companion triggers, the combo matrix |
| Headless sim with no engine references | Headless combat sim for build sweeps and companion AI |
| `CoreTests/` standalone runner | The combo matrix is exactly the system that needs a few hundred cheap assertions |
| `HeuristicAgent` + `AgentPersonality` | Companion AI, neutral bosses, and phantom pilots |
| Phantom AI ([design](../design/phantom-ai.md)) | Queue backfill: real players' posted characters, piloted by the agent |
| `MatchRunner` headless sweeps | Phantom-vs-phantom matches that find builds that never win and builds that always do |

## Lessons from Guild Wars 2

GW2 rebuilt GW1's skill system — same studio, same world. It is a successful game
and these were **trades, not errors**, but they run in exactly the direction that
loses a GW1 build-crafter. The diagnosis: **GW1 put the depth before the fight;
GW2 moved it into the fight.**

| What GW2 changed | What it cost | Guardrail here |
| --- | --- | --- |
| Weapon dictates skills 1–5 of a 10-slot bar; only 5 are chosen, from one profession's pool | Authorship. Half your bar is a property of the sword | **Weapons gate, never dictate** |
| Secondary profession removed; elite specializations later restored some of it as *fixed packages* | The identity engine | **Free pairing is load-bearing** |
| Build expression moved into trait grids and gear stat-combinations | A build became percentage modifiers instead of nameable verbs | **Expression stays in named verbs** |
| Combo fields and finishers — the same idea as terrain combos | Almost nobody noticed them | **Telegraph loudly or don't build it** |

### The four guardrails, and how each gets crossed

Each is crossed by a **reasonable-sounding local decision**, which is why they are
written down.

1. **Weapons gate, never dictate.** A weapon carries *one* native skill and decides
   which abilities are *eligible* for your six. *Crossed by:* a "make weapons feel
   more distinct" pass that adds a second native skill, then a third. *Tripwire:*
   a weapon supplying more than one slot's worth of bar.
2. **Free pairing is load-bearing.** 56 self-assembled pairs, not a menu of
   premade hybrids. *Crossed by:* an expansion wanting a headline feature —
   signature specializations are easier to balance and market. *Tripwire:* a
   package granting abilities that couldn't be assembled by pairing.
3. **Expression stays in named verbs.** You should be able to describe your build
   in one sentence, naming things that happen. *Crossed by:* skills and passives
   drifting toward "+6% growth duration", because numbers are easy to author.
   *Tripwire:* a skill's or passive's text that is a percentage with no verb.
   In-match upgrades are the one sanctioned exception: temporary, per-skill,
   reset every match.
4. **Telegraph the terrain system loudly.** GW2's combo fields were deep and mostly
   unused because they are a translucent circle under a pile of effects; brambles,
   burning grass and flooded ground are large and persistent. *Crossed by:* visual
   polish that makes effects prettier and subtler at once. *Tripwire:* a new
   player who can't say what just happened after a combo fires.

**What to keep from GW2:** its dodge-and-reposition combat is better
moment-to-moment than GW1's. The error would be letting execution depth *crowd
out* preparation depth. The target: the loadout screen decides more than the
rotation does, and a fight still rewards moving well.

## What is actually hard

1. **Scope.** An online-only PvP game carries the costs of a service: matchmaking,
   ranking, anti-cheat, live balance and a population to keep. A few maps with
   rotating modifiers is the affordable launch.
2. **Content volume is the design.** Hundreds of abilities are the product, not
   polish. A thin version of this game has no draw at all. This is the risk that
   should scare you most.
3. **Dynamic verticality.** Player-created climbable surfaces break most pathing
   and level-design assumptions. Most likely to be cut — so validate it early,
   alone, where its cost is visible.
4. **Dead builds and dominant ones.** With 56 pairs × hundreds of abilities, the
   realistic failures are dozens of combinations nobody enjoys and a few everyone
   is forced into. Automated sweeps are spine, not polish.
5. **Martial parity in a game about spellcraft.** The terrain system is a caster's
   playground by default. Every new surface, volume and skill will *want* to be
   arcane, and someone has to keep insisting that `Force`, traversal and coatings
   get equal attention. A standing organizational risk, not a task that finishes.
6. **Snowballing.** The standing risk of every objective-driven game. The controls
   in [Keeping it from snowballing](#keeping-it-from-snowballing) are a start; only
   playtests show whether a lost opening feels recoverable.
7. **Population.** Level-matched PvP needs players in each bracket at each hour.
   Phantoms cover gaps, but an online-only game lives or dies on having people.

## Vertical slice

Two questions, in order:

> **Does a match produce a story — and does the story come from the builds?**
>
> **Do players want to make another character?**

**Target: one map, 3v3 with companions, phantoms filling empty seats** — smaller
than launch's 5v5 so a few testers plus phantoms can fill it.

- **5 classes — Verdurist, Emberwright, Tidecaller, Reaver, Beastbinder.** Both
  resource economies, both grips on the matrix, and a martial — "does a melee
  character feel like a protagonist in a terrain-driven game?" is a primary
  question.
- ~35 abilities (7 per class, 1 elite each), the full six-slot bar, primary and
  secondary with passives, and both resource economies. **The build economy ships
  first.**
- **All five self-heals** — the most-pressured slot and the cheapest source of
  combination identity.
- **Match level and an upgrade axis per skill.**
- **One companion each**, with 3 trigger-conditioned ability slots, usable on
  `Hold` to guard a point.
- **Three weapon families** — heavy arms, dagger and bow.
- **The map:** a 3v3 cut of [Hollowmere](#first-map-hollowmere), about 270 m on
  a side — the Grove and the Well, the Heron King, a Keeper each, fog of war, and
  both gated routes (the Mere and the Bramblecut). **No `Trellis`; verticality is
  slice 2.**
- Terrain grid with 5 surfaces — `Foliage`, `Burning`, `Scorched`, `Water`,
  `Ice` — fire spread on.
- A lobby that shows opposing pairs and weapons; a small hub to inspect builds.
- **Everyone at the cap.** No leveling, brackets or ranking — progression isn't
  what's under test.

**Kill criteria, written before building:**

- **After a match, ask each player what happened.** If every answer is about one
  teamfight — no route taken, no split, no ambush, no race for the boss — the map
  is not the economy and the design has collapsed into an arena brawler. The
  primary metric.
- If no one ever leaves a companion to hold a point, splitting isn't real.
- If players never rebuild between matches, the draw isn't there.
- If players of the same build upgrade in the same order every match, whatever the
  map and opponents, the upgrade axes aren't situational enough.
- If Reaver players describe the good moments as things that happened *to* the map
  rather than things they did, `Force` isn't carrying enough weight.
- If two people running the same pair end up with near-identical bars, the build
  space is fake.

## Open questions

1. **Does any category become compulsory at six slots?** If every viable build
   spends a slot on mobility or a defensive cooldown, the flexible budget silently
   drops to three. The fix is kit authoring, not more slots.
2. **Is 15–20 minutes the right match length?** Macro play pulls longer; the
   build-crafting draw pulls shorter.
3. **Is the Beastbinder a class or an axis?** "Bring beasts" and "bring
   constructs" could be companion unlocks for everyone, with the Beastbinder and
   Cogwright being *better commanders* rather than the gate. That widens the
   fantasy for every pair, but costs those two classes their headline identity.
4. **How much authored narrative?** In PvP it is mostly the hub, the announcer and
   pair reactivity — cheap and worth a lot. A campaign belongs to PvE, and is the
   single largest cost in this document.
5. **When does PvE come back?** It is where strange, overpowered builds get to be
   fun and where solo players live. The earliest cheap version: the map's bosses
   as a standalone co-op mode.
6. **Does support-as-a-build hold up?** Distributed self-heals mean nobody is
   *obligated* to heal, but not that someone who *wants* to heal has enough to work
   with. If not, the fix is a ninth class — which risks bringing the obligation
   back.
7. **Are eight classes too many for launch?** 56 pairs is the draw, but it is also
   8 ability pools, 8 armor sets and 56 authored names before one is deep. Six deep
   classes may beat eight thin ones — and if cutting, cut an *arcane* one.
8. **Does the terrain system survive its own cost?** It now carries the
   objectives and the build-gated routes, so it is central — but it is still the
   most expensive pillar. Could a cheaper surface model support sites and routes
   without full simulation?
9. **How long is the climb to the cap?** Too short and leveling stops teaching;
   too long and the tutorial overstays and mixed-level parties strain. A starting
   guess: 20–30 hours of matches, set by how fast playtesters absorb the bar.
10. **How much should objectives snowball?** Too little and early play doesn't
    matter; too much and a lost opening ends the match at minute five.

## PvE missions (later)

A later layer. The design so far:

- **The loop:** hub → briefing → mission (8–15 minutes, 1–4 players, companions
  filling empty seats) → reward → hub. Short, so the loadout decision recurs and
  failure is cheap enough to experiment.
- **The briefing is the puzzle.** Each mission publishes its profile before you
  commit, written in the combo grammar:

  > **The Drowned Kiln** — Marsh. Surfaces `Water`, `Mud`, `Foliage`. Enemies:
  > Drowned (resist `Fire`, weak to `Shock`). Hazard: a flood rising on a timer.
  > Optional objectives need **Breaks** and **Grows**.

  Optional objectives name the world verbs they want, so a verb you lack is a
  reason to come back as someone else, not a dead end ten minutes in.
- **Modifiers** multiply how many builds a map is interesting for, at no art cost.
- **Level sync:** a veteran scales down to the mission's level and keeps their full
  bar — more options, not bigger numbers.
- **Budgets:** solo players can field a larger squad of cheaper units (drones,
  beasts, constructs) under a point budget, with companion ability slots scaling
  with each unit's cost so the squad stays readable.

## Parked ideas

Designed once, deliberately left out for now — things to come back to when the
base game plays well, not options on the table.

- **Sigils** — accessories that rewrite one skill's rules (*your vines also grow a
  trellis*; *your steam clouds scald*). A large design space of its own, and
  nothing else in the design depends on it.

## Appendix: ten builds

Theorycrafted against the systems above, not balanced — to show the *shape* of the
space and prove a six-slot bar with one elite and one heal can hold a character
concept. Abilities are illustrative. Each is **1 elite + 1 heal + 4**, with a
weapon and a named pair.

### Frostbreaker — Reaver/Tidecaller · Heavy Arms

**Glacier's Fault** *(elite)* — your next heavy-arms blow shatters all `Ice`
nearby; everyone on it is knocked down, harder the longer the ice has stood ·
**Bloodwake** *(heal)* · Rime Underfoot · Overbear · Shatterstep · Cold Grip

*Build a rink nobody notices, then delete the floor.* Dies to any Emberwright, the
`Ashfall` modifier, and knockdown-immune enemies.

### Thornghost — Veilblade/Verdurist · Dagger

**The Long Patience** *(elite)* — your first strike from concealment leaves a deep
wound and can't be blocked; recharges only when you conceal again · **Quiet the
Wound** *(heal)* · Seedfall · Hamstring · Whisper Chain · Vanish

*You bring your own bushes.* Seedfall is the build. Dies to fire and to `Force`
reveals.

### Breakwall — Warden/Reaver · Spear

**Immovable** *(elite)* — stance; you cannot be moved or knocked down, and every
hit you take builds adrenaline; ends if you move · **Bracing Stance** *(heal)* ·
Pin · Shieldwall · Hold Fast · Overbear

*You are the door.* Touches terrain **zero** times — proof the non-terrain
vocabulary carries a build alone. Dies to stacked conditions, and to being walked
around.

### Powdersmoke — Cogwright/Veilblade · Gun

**Overclock** *(elite)* — instant reload for 6s; each shot costs you health ·
**Quiet the Wound** *(heal)* · Smokebomb · Braced Shot · Scatterload · Caltrop Line

*Concealment manufactured chemically rather than grown*, so a heal that wants grass
works on an iron deck. Same rule, different fiction — the cheapest breadth there is.

### Thornreaver — Verdurist/Reaver · Heavy Arms

**Heartwood** *(elite)* — root yourself, gain heavy armor, and grow `Bramble`
outward each second · **Rootdraw** *(heal)* · Bramble Wall · Entangle · Overbear ·
Wild Swing

*An angry tree with a greatsword.* The Reaver secondary trains heavy arms, so it
swings properly, and *Rootedness* reads the Reaver too — you can grow on the
rubble your swings leave. The trap version is the same character as a
Verdurist/Tidecaller carrying a greatsword neither class trains.

### Houndwright — Beastbinder/Cogwright · Bow

**Pack Bond** *(elite)* — your companion's abilities recharge twice as fast and it
shares your boons · **Tend** *(heal)* · Balm Arrows · Marking Shot · Command:
Screen · Field Repair

*Support without a healer class.* Most of this build's power lives in the
**companion loadout screen** — Pack Bond is only as good as the triggers you
programmed.

### Ashgardener — Emberwright/Verdurist · Implement

**Wildfire** *(elite)* — your fires never stop spreading while you live; you burn a
little more each second they do · **Cauterize** *(heal)* · Kindle · Seedfall ·
Emberstep · Flashover

*Plant the fuel, light it, walk through it, heal by eating it.* If you can't close
the fight, the elite kills you.

### Steamcaller — Tidecaller/Emberwright · Implement

**Whiteout** *(elite)* — all water you control flashes to `Steam`; for 10s you see
through steam and nobody else does · **Drink Deep** *(heal)* · Wellspring · Scald ·
Chill Depths · Flashfreeze

*A personal fog of war.* A whole engagement where only you know where anyone is.

### Galvanist — Tidecaller/Cogwright · Gun

**Arc Column** *(elite)* — a pylon; every flooded area connected to it chains shock
continuously · **Drink Deep** *(heal)* · Wellspring · Conduct · Grounding Rod ·
Braced Shot

*Electrify a lake and stand in it safely.* Dies to being pulled off your own
flooded ground.

### Bramblewarden — Warden/Verdurist · Spear

**Hedge Sovereign** *(elite)* — `Bramble` near you counts as your cover, and allies
inside it share your armor · **Bracing Stance** *(heal)* · Bramble Wall · Pin ·
Shieldwall · Entangle

**The best thing the exercise produced.** *Bulwark* scales armor with the cover
around you; the Verdurist manufactures cover; *Bracing Stance* scales off the same
thing. Three systems compound, and nobody designed it. If the shipped game produces
discoveries like this, the draw is real.

### What the ten builds taught

1. **The elite carries the build.** Every one is named by its elite.
2. **Portable terrain is mandatory**, or modifiers delete builds instead of
   challenging them.
3. **Shouts are the companion's control surface**, not a dedicated key.
4. **Same rule, different fiction** (grown cover vs. chemical smoke) is how the
   catalog gets breadth cheaply.
5. **The best builds are found, not designed.** Protecting the conditions for that
   — passives that read the secondary, free pairing, legible rules — matters more
   than any single ability.
