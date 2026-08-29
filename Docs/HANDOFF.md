# Session Handoff — 2026-08-29

State transfer from the cloud Claude Code session that did the initial project
work, for continuing locally. Read `CLAUDE.md` first (project map, conventions,
commands); this file covers only what that session left in flight.

## What that session produced (all on `claude/project-evaluation-expansion-l20af1`)

1. **Project evaluation** — verdict: polished deck-builder front end, empty
   game. Design docs on Drive (LT Cards sheet + Overview deck) are strong.
2. **Bug fixes** — build blockers (`UnityEditor` in runtime asmdefs), first-run
   NRE in `CardLibrary`, deck limit off-by-ones, starter decks now seeded into
   the player's editable/persisted deck list on first run, filter crashes,
   tooltip debug loop replaced with Show/Hide API.
3. **Design docs** — `Docs/design/`: roadmap (`README.md`), roguelike mode
   ("The Descent", with AI phantoms of real players' decks), multiplayer,
   phantom AI architecture, and `rules-v1.md` (engine rulings).
4. **`LightCard.Core`** (`Assets/Scripts/Core/`) — deterministic headless rules
   engine, 20 Expedition/Garden cards implemented verbatim, data-driven
   effects (Condition/Trigger/Target/Effect). Plus `Agents/`: simulate-and-score
   `HeuristicAgent`, archetype `AgentPersonality` presets, headless
   `MatchRunner`. **28 tests pass** via `cd CoreTests && dotnet run`.
5. **Unity 6 migration (in progress)** — user's editor is **6000.5.8f1**:
   - Deleted: Odin Inspector, DOTween Pro, SRDebugger, old UI Extensions
     samples (usage was verified first; `CardDataImporter` rewritten as plain
     `EditorWindow`, `DeckItemView` de-Odined).
   - UI Extensions v3.0.0 embedded at `Packages/com.unity.uiextensions`
     (replaces unpinned Bitbucket URL; script GUIDs verified to match existing
     scene/prefab references; two `GetInstanceID` uses patched).
   - Obsolete-API fixes in Shapes / UI Extensions / GSTU; final fix replaced
     EntityId int casts with `Object.GetHashCode()` identity tokens
     (Unity 6000.5 hard-obsoleted the EntityId→int cast).

## In flight / unverified — the local session's first jobs

- [ ] **Confirm the project now compiles clean in 6000.5.8f1** (last two known
      errors were fixed in commit `07ee64c`, not yet verified in-editor).
- [ ] **Commit Unity's own migration edits**: `ProjectVersion.txt`,
      `Packages/manifest.json` rewrites, and the API-updater's edits to
      Cartoon FX / Shapes `UnityInfo.cs` / GSTU. These are wanted changes.
- [ ] **Smoke-test `Main.unity`**: deck collection, filters, deck editing,
      drag-to-reorder + drag-to-delete (exercises embedded UI Extensions),
      save/load across a restart. First run should seed starter deck copies.
- [ ] **Push `main`** — the user merged the work branch into local `main`;
      origin/main is behind until pushed.
- [ ] Expect old-asset runtime issues when scenes using Shapes / Translucent
      Image / Cartoon FX are exercised (compile ≠ working); update or replace
      those assets when the Field scene work starts.

## Only the user can do

- **Revoke the leaked Google OAuth credentials** (Google Cloud Console) —
  still in git history at `d516ea8^`. Highest-priority open item.

## Next roadmap step (after the checklist)

**Phase 2 — playable match**: wire `Field.unity` to the engine. A view layer
that consumes `GameEvent`s + input that emits `Command`s, playing vs.
`HeuristicAgent`. `_LightCard.asmdef` needs a reference to `LightCard.Core`
(name-based reference works). Roadmap and specs: `Docs/design/README.md`.

## Suggested opening prompt for the local session

> Read CLAUDE.md and Docs/HANDOFF.md. We're mid-Unity-6-migration on branch
> main (merged from claude/project-evaluation-expansion-l20af1). Here's the
> current Unity console output: [paste]. Work through the handoff checklist,
> then start phase 2.
