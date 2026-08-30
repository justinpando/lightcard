# Light Card Tactics (lightcard)

A tactical collectible card game prototype in Unity **6 (6000.3+)** — originally
built in 2020.3, migrated in Aug 2026. 1v1 on a
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
| `Assets/Scripts/Field/` | The match view: `MatchContext` (scene entry, input, AI turns) + field/hand/HUD controllers. `Field.unity` is the playable match scene. |
| `CoreTests/` | Standalone .NET test runner for the engine — no Unity needed. |
| `Packages/com.unity.uiextensions/` | Unity UI Extensions v3.0.0, **embedded and locally patched** (Unity 6.3 EntityId guards) — upstream doesn't compile on 6000.3+, so keep the embedded copy rather than a manifest URL. |
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

- **Engine code stays at C# 8** (`CoreTests.csproj` pins `LangVersion 8.0`) —
  a conservative floor kept from the 2020.3 era; raise it deliberately, not
  incidentally.
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
deck-builder bugs fixed), phase 1 (engine + 20-card Expedition/Garden set), the
first slice of the phantom AI (heuristic agent + personalities + match runner),
and the first slice of phase 2: `Field.unity` is wired to the engine
(`MatchContext` + view controllers in `Assets/Scripts/Field/`) and playable
against `HeuristicAgent` — click/drag play, Shift, Activate, Attune, automatic combat, AI turns,
HUD, history log, and a victory/defeat panel with rematch. The full game loop
is connected: Main menu "Begin Match" plays the top deck of the collection
(drag-to-reorder chooses it) and loads `Field.unity`; match end returns to the
menu or rematches. Remaining phase 2 polish: animate from the `GameEvent`
stream instead of full-state refreshes. Decks thin out on entry (cards missing
from `CardCatalogV1` are skipped, then padded back from the catalog) until
more of the set is implemented. The catalog is COMPLETE: 161 cards, every named sheet card, across all six
archetypes (Spirit Bind, equips, Guardian, discard, transforms, Flood/Desert
mutations all live; rulings in `Docs/design/rules-v2.md`, deferred-card list
there too). `LightCard/Sync Card Assets From Catalog` mirrors
the catalog into library Card assets; a PNG at
`Assets/Art/CardPlaceholders/<Card Name>.png` overrides all art on sync
(Midjourney workflow: prompts in `Docs/art/midjourney-placeholders.md`),
otherwise procedural placeholders are generated. Balance table in rules-v2. Legacy note: Garden
0/8 vs Expedition/Atelier in agent sweeps — structural, pending tuning.

## Caveats

- **Secrets in git history (defused)**: an old commit (`d516ea8^`) contains
  Google OAuth credentials from the Sheets importer config. The client was
  verified deleted on 2026-08-29 (Google returns `401: deleted_client`), so the
  strings in history are inert. `.gitignore` now blocks `GSTU_Config.asset` and
  `StreamingAssets/Key/`; keep it that way when creating fresh credentials.
- The Sheets importer (`LightCard/Card Data Import` menu) null-refs until a new
  GSTU config with fresh credentials is created. It also still uses the
  legacy `WWW` API (obsolete warning) — migrate to `UnityWebRequest` when
  reviving it.
- **Removed in the Unity 6 migration** (don't reintroduce casually): Odin
  Inspector, DOTween Pro, SRDebugger, and the old UI Extensions samples. If
  tweening is needed later, add a current DOTween fresh.
- Shapes, Translucent Image, and Cartoon FX are old versions under
  `Assets/_Packages/` — expect Unity 6 breakage when scenes using them are
  touched; update or replace them then. Keep the repo private (paid assets).
- `Assets/Scripts/Utility/` is largely vendored/unreferenced code — don't
  extend it; add new utilities near their use site.
