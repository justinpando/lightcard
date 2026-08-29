# Light Card Tactics (lightcard)

A tactical collectible card game prototype in Unity **2020.3.1f1**. 1v1 on a
3-lane x 6-row board (each player owns a 3x3 half); six Archetypes representing
psychological motivations (Garden, Tower, Expedition, Ocean, Atelier, Heart);
an Affinity/energy ramp economy driven by replacing cards.

## Where things live

| Path | What |
| --- | --- |
| `Assets/Scripts/Core/` | **LightCard.Core** — the deterministic, headless rules engine. Plain C# (own asmdef, `noEngineReferences: true`). Commands in, ordered `GameEvent` list out. No UnityEngine anywhere in it — keep it that way. |
| `Assets/Scripts/Core/Agents/` | Heuristic AI: `HeuristicAgent` (simulate-and-score over cloned states), `AgentPersonality` (archetype weight presets), `MatchRunner` (headless agent-vs-agent matches). |
| `Assets/Scripts/Library/` | The working deck-builder UI (collection, filters, deck editor, save/load). Entry point: `MainContext.cs` in `Main.unity`. |
| `Assets/Scripts/Data/` | JSON save system + `CardDataImporter` (editor-only Google Sheets → ScriptableObject importer; currently unconfigured, see caveats). |
| `Assets/Scripts/Field/` | Mostly-empty stubs for the match view; `Field.unity` is art layout with no logic yet. |
| `CoreTests/` | Standalone .NET test runner for the engine — no Unity needed. |
| `Docs/design/` | Design docs: roadmap (`README.md`), roguelike mode, multiplayer, phantom AI, and **`rules-v1.md`** (engine rulings — read before changing rules code). |

Game design source of truth is the "LT Cards" Google Sheet (cards, keywords,
space effects); `Docs/design/` builds the modes on top of it.

## Commands

```bash
cd CoreTests && dotnet run     # build + run all engine tests (~30 scenario tests)
```

There is no CI. Run the tests before and after any engine change. The Unity
project itself has no play-mode tests; verify UI changes by opening `Main.unity`
in the editor.

## Conventions

- **Engine code is C# 8 max** (Unity 2020.3 compiler) — `CoreTests.csproj` pins
  `LangVersion 8.0` to enforce this. No records, no init-only setters.
- The engine must stay **deterministic**: all randomness through
  `GameState.NextRandom`, no wall-clock, no iteration over unordered
  collections where order reaches game state.
- Card behavior is **data, not code**: cards are `EffectDef` lists
  (Condition/Trigger/Target/Effect grammar) in `CardCatalogV1.cs`. Prefer a new
  `Trigger`/`EffectAction`/`TargetScope` enum member plus resolution logic over
  any per-card special case.
- New rules interpretations get a line in `Docs/design/rules-v1.md`.
- Every new file under `Assets/` needs a `.meta` (let Unity generate them, then
  commit them). The root `.gitignore` ignores `*.csproj` for Unity's sake —
  `CoreTests/CoreTests.csproj` is force-added; use `git add -f` if it changes.

## Current state / roadmap position

Roadmap is in `Docs/design/README.md`. Done: phase 0 (build blockers and
deck-builder bugs fixed), phase 1 (engine + 20-card Expedition/Garden set), and
the first slice of the phantom AI (heuristic agent + personalities +
match runner). **Next: phase 2** — wire `Field.unity` to the engine: a
GameEvent-consuming view layer plus input that emits Commands, playing against
`HeuristicAgent`. `_LightCard.asmdef` will need a reference to `LightCard.Core`
(name-based references work).

## Caveats

- **Secrets in git history**: an old commit (`d516ea8^`) contains Google OAuth
  credentials from the Sheets importer config. They must be revoked in Google
  Cloud Console; deleting the file did not remove them from history. Never
  commit `GSTU_Config.asset` or anything under `StreamingAssets/Key/`.
- The Sheets importer (`LightCard/Card Data Import` menu) null-refs until a new
  GSTU config with fresh credentials is created.
- Unity 2020.3 is past EOL; an engine upgrade is planned before any
  networking work (see `Docs/design/multiplayer-mode.md`).
- `Assets/Scripts/Utility/` is largely vendored/unreferenced code — don't
  extend it; add new utilities near their use site.
- Paid assets (Odin, DOTween Pro, Shapes, SRDebugger, etc.) are committed under
  `Assets/` — keep the repo private.
