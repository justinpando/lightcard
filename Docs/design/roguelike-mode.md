# The Descent — Single-Player Roguelike Mode

## Fantasy

You are a **Seeker**: someone who enters the Collective Unconscious — "the Place"
— to face what lives there and come back whole. The six Archetypes are not
factions you join but forces you draw on: To Enjoy (Garden), To Possess (Tower),
To Discover (Expedition), To Become (Ocean), To Build (Atelier), To Connect
(Heart). A run is one descent; the opponents are **phantoms**, the residue of
other Seekers' decks left behind in the Place (see
[phantom-ai.md](phantom-ai.md)).

This builds directly on the run-mode notes already in the design sheet: the tarot
reading as run start, the three story acts, shadows born from failed Psyches, and
victory merging your deck into the void.

## Run start: The Reading

Instead of a class-select screen, a run opens with a short tarot-style reading —
three draws that double as the draft:

1. **Who you are** — pick 1 of 3 Archetype cards. Sets your primary archetype and
   grants its 8-card core package.
2. **What you carry** — pick 1 of 3 minor spreads (4-card synergy packages, can
   be off-archetype). This is how dual-archetype decks form organically.
3. **What you seek** — pick 1 of 3 run blessings (a Charm that starts in play, a
   persistent rule tweak, or a curse that pays out extra rewards).

Result: a lean ~15-card starting deck. Small starting decks are essential — the
run is about *becoming* a deck, and every reward has to matter.

## Run structure

Three acts, matching the story-act notes:

| Act | Theme | Opponents | Boss |
| --- | --- | --- | --- |
| I — The Monster Within | Your own shadow material | Tier-1 phantoms (low-power decks, gentle AI) | Your **Shadow**: a phantom built from a distorted copy of *your current deck* |
| II — The Monster Without | Other Seekers' residue | Tier-2 phantoms (real ranked decks) | A **Warden**: hand-designed archetype boss with a unique field (pre-set space effects) |
| III — The Creative Act | Integration | Tier-3 phantoms (boss phantoms: decks that won runs) | **The Unbeaten** — see below |

Each act is a small node map (Slay-the-Spire style, 6–8 floors, branching):

- **Phantom battle** — a match vs. a pooled phantom. Reward: pick 1 of 3 cards,
  weighted toward your affinity spread.
- **Elite phantom** — higher-tier phantom with a field modifier (e.g. two columns
  start Brambled). Reward: card pick + a Relic.
- **Event** — narrative choice nodes drawn from the archetype themes (a Garden
  event tempts, a Tower event bargains). Costs and payoffs touch deck, life, or
  Coherence.
- **Sanctuary** — heal, *or* remove a card, *or* duplicate a card. Never more
  than one benefit; rest sites are a real decision.
- **Attunement site** — raise a chosen archetype's starting Affinity for the rest
  of the run. This is the run-scale mirror of the in-match affinity economy.

## In-run progression

- **Cards** are the primary reward. Deck size is uncapped but drafting is
  optional — every reward screen has a Skip that grants a small consolation
  (2 life or 1 gold-equivalent, "Insight").
- **Relics** (run-persistent passives) map to the keyword system so they need no
  bespoke engine work: "your Units have Armor 1 on your back row", "the first
  Ability each turn costs 1 less", "Space Effects you apply also trigger once
  immediately".
- **Affinity persists across the run**: matches start with the Affinity levels
  you've attuned, so late-run matches begin mid-curve and play faster. This keeps
  match length from bloating as the run goes long and makes high-cost cards
  draftable.
- **Life persists** between fights (heal at Sanctuaries and on act completion).
  Match damage taken maps to run-life lost at a soft ratio (e.g. 1 run-life per 2
  overkill damage plus flat loss on defeat) so winning ugly still costs.

## Coherence (optional layer, from the balance notes)

The original notes propose an objective of *keeping yourself in balance*. As a
mechanic: **Coherence** is a 0–10 run meter. Mono-archetype extremism and
deck-warping events pull it down; integrating opposing archetypes (holding
affinity in 2–3 archetypes, resolving events "in balance") pulls it up.

- High Coherence: reward quality up (more rare offers).
- Low Coherence: shadows strengthen — elite and boss phantoms gain +1/+1.
- Hitting 0 doesn't end the run; it *marks* it. If you then lose, your deck is
  captured as a **Shadow phantom** — a corrupted, buffed phantom that haunts your
  (and your friends') future runs. Failure literally populates the world.

Ship the Descent without Coherence first; add it once the base loop is fun. It's
a modifier layer, not a foundation.

## Bosses

- **The Shadow (Act I)** is generated: take the player's current deck, invert its
  curve (cheapest cards get +1 cost, most expensive get -1), swap its archetype
  for its opposing pair, and pilot it with an aggressive AI personality. Cheap to
  build, infinitely replayable, and thematically exact — you fight who you are.
- **Wardens (Act II)** are the only hand-authored content: one per archetype, six
  total, each with a signature field layout and 2–3 unique cards. These unique
  cards are the long-term unlock currency (defeat the Garden Warden → its
  signature card can appear in future drafts).
- **The Unbeaten (Act III finale)** — from the notes: *"an entity that cannot be
  defeated."* The final fight is not winnable by damage; it's a survival/scoring
  fight (e.g. survive 8 turns, score by damage dealt, units preserved, Coherence).
  Your run rank comes from this score. This sidesteps the balance nightmare of a
  "fair" final boss against wildly variable player decks, and it lands the theme:
  you don't defeat the unconscious, you leave it intact — changed.

## Victory and the loop back to multiplayer

Completing a run (reaching and surviving The Unbeaten):

- Your run deck is submitted to the **boss phantom pool** — "your deck merges
  with the void" — tagged with your name, score, and the run's archetype spread.
  Other players meet it in their Act III.
- You earn cosmetic and draft-pool unlocks (Warden cards, card backs, new
  blessings). **No power-based meta-progression** — a fresh account and a
  veteran face the same Act I. Progression widens options, never raises stats.

## Scope guardrails (v1)

The v1 Descent is: 1 act of 7 nodes, the Reading with archetype pick only,
card rewards, Sanctuaries, local-only phantoms (starter decks + the player's own
saved decks), the generated Shadow as the boss. No Coherence, no Relics, no
events, no services. Everything else in this doc layers on after that loop is
fun for 30 minutes.
