# The Waking Duel — Multiplayer Mode

1v1 with decks from the existing editor. Multiplayer is deliberately the *last*
roadmap phase: it inherits a hardened rules engine from the roguelike, and its
real design job is feeding the phantom ecosystem as much as head-to-head play.

## Modes

- **Casual** — unranked, any legal deck, direct challenge or quick match.
- **Ranked** — MMR ladder. Every completed ranked match snapshots the winning
  deck into the phantom pool (see [phantom-ai.md](phantom-ai.md)); this is
  ranked's contribution to the wider game and the default players consent to at
  queue time (account-level opt-out).
- **Async (post-v1 candidate)** — correspondence play with multi-hour turn
  timers, several games in parallel. LT Cards' turn structure (no priority
  passing mid-turn, reactions handled by persistent Charms rather than instants)
  makes async unusually clean: a turn is a self-contained command batch. Cheap
  to add once the architecture below exists, and a good mobile fit.

## Architecture: server-authoritative, engine-shared

The same `LightCard.Core` assembly runs in three places: the client (prediction
and animation), the server (authority), and the AI (simulation).

```
Client A ── commands ──> Match service ── validate + resolve ──> events ──> A & B
                         (runs LightCard.Core;
                          owns the only true GameState)
```

- Clients send **commands** (PlayCard, Shift, Attack, ReplaceCard, EndTurn);
  the server validates against the true state, resolves, and broadcasts the
  resulting **event list**. Clients never exchange state with each other and a
  client is never trusted with hidden information (opponent hand and deck order
  live only on the server; the client renders card backs).
- Because the engine is deterministic, the acting client can **optimistically
  predict** its own command locally for zero-latency feel, then reconcile
  against the server's event list (which should match byte-for-byte; a mismatch
  is a bug alarm, not a gameplay path).
- A finished match is just `(seed, command log)` — that's the replay format, the
  anti-cheat audit trail, and free regression-test data for the engine.

### Why not deterministic lockstep peer-to-peer?

Lockstep leaks hidden information (both clients must hold full state, so a
hacked client sees the opponent's hand). Card games with hidden zones need a
referee. The server is that referee.

### Transport and stack

Turn-based, low-rate messaging — this is **not** a tick-synced action game, so
heavyweight netcode (Netcode for GameObjects, Mirror, Photon Fusion) is the
wrong shape. Recommended:

- **Transport:** WebSocket (or Unity Transport if staying all-Unity), JSON or
  MessagePack command/event payloads. Works on every platform including WebGL.
- **Match service:** a small stateless-per-match .NET service hosting
  `LightCard.Core` — the engine being a plain C# assembly means the server is a
  console app, not a headless Unity build. Host on anything; one cheap node
  handles thousands of concurrent turn-based matches.
- **Turn timer:** 90s soft / bank of 3 overtime extensions, server-enforced;
  disconnect grace = timer bank, then auto-concede with reconnect-and-resume up
  to match end.
- **Prerequisite:** upgrade off Unity 2020.3 (EOL) before this phase; pick the
  current LTS and re-pin the UI Extensions package (currently an unpinned
  Bitbucket git URL) at the same time.

## Matchmaking

- Simple MMR (Glicko-2), one rating for ranked.
- Queue widens rating band over wait time; below a population threshold the
  queue offers a **phantom fallback**: "No Seeker found — face their phantom
  instead?" A phantom match at your MMR band, using a real snapshot from the
  pool, paying reduced ladder points. Small playerbases are the reality of an
  indie CCG launch; phantoms convert dead queues into content, and it keeps the
  ladder alive at 4 a.m. forever.
- New accounts play 5 placement matches against Tier-2 phantoms before entering
  the human queue — calibration and tutorialization in one step, invisible to
  the ladder.

## Deck legality and balance surface

- Server validates decks at queue time against the current `cardSetVersion`
  (40-card max, 3-copy max — limits now enforced correctly client-side, but the
  server re-checks; the client is never trusted).
- Balance patches are data patches (card definitions are already
  spreadsheet-driven). The Affinity-Level lever from the design sheet is the
  preferred nerf tool — raise AL requirements to delay a card rather than gut
  it — which keeps phantom snapshots of older decks playable after patches.

## Social minimum (v1)

Emote wheel (6 archetype-themed emotes, mutable), friend challenge by handle,
end-of-match "add friend". No chat at launch — chat is a moderation liability a
solo project doesn't need.

## Phase plan

1. **MP-0:** hot-seat / same-client two-player using the command-event flow
   locally — proves the architecture with zero networking.
2. **MP-1:** friend challenge over the wire (no ladder), reconnect handling.
3. **MP-2:** ranked queue + MMR + phantom snapshot capture + phantom fallback.
4. **MP-3:** async mode, spectate-a-replay, seasonal ladder resets.
