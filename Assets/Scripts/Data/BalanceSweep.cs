#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LightCard.Core;
using LightCard.Core.Agents;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Full-catalog archetype round-robin for balance testing. Run from the menu,
/// or headlessly while the editor is closed:
///   Unity.exe -batchmode -quit -projectPath . -executeMethod BalanceSweep.RunHeadless -logFile sweep-log.txt
/// Results land in Docs/design/balance-sweep-latest.md.
/// </summary>
public static class BalanceSweep
{
    private static readonly int[] Seeds = { 3, 7, 11 };
    private const string OutputPath = "Docs/design/balance-sweep-latest.md";

    [MenuItem("LightCard/Run Balance Sweep")]
    public static void RunFromMenu() => Run();

    public static void RunHeadless()
    {
        try
        {
            Run();
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogError($"Balance sweep failed: {e}");
            EditorApplication.Exit(1);
        }
    }

    private static void Run()
    {
        var archetypes = (Archetype[])Enum.GetValues(typeof(Archetype));
        var wins = archetypes.ToDictionary(a => a, a => 0);
        var log = new StringBuilder();
        int matches = 0, stalls = 0, illegal = 0;

        log.AppendLine($"# Balance sweep — {DateTime.Now:yyyy-MM-dd HH:mm}");
        log.AppendLine();
        log.AppendLine($"Round-robin, seeds {string.Join("/", Seeds)}, archetype personalities, full-catalog decks.");
        log.AppendLine();

        for (int i = 0; i < archetypes.Length; i++)
        {
            for (int j = i + 1; j < archetypes.Length; j++)
            {
                foreach (int seed in Seeds)
                {
                    var a = archetypes[i];
                    var b = archetypes[j];
                    var result = MatchRunner.PlayMatch(ArchetypeDeck(a), ArchetypeDeck(b),
                        PersonalityFor(a), PersonalityFor(b), seed);

                    matches++;
                    illegal += result.FailedCommands;
                    string outcome;
                    if (result.Winner == 0) { wins[a]++; outcome = a.ToString(); }
                    else if (result.Winner == 1) { wins[b]++; outcome = b.ToString(); }
                    else { stalls++; outcome = "STALL"; }

                    log.AppendLine($"- {a} vs {b} (seed {seed}): **{outcome}** in {result.Turns} turns, {result.CommandsIssued} commands");
                }
            }
        }

        log.AppendLine();
        log.AppendLine("## Wins per archetype (of 15 matches each)");
        log.AppendLine();
        foreach (var pair in wins.OrderByDescending(p => p.Value))
            log.AppendLine($"- {pair.Key}: {pair.Value}");
        log.AppendLine();
        log.AppendLine($"{matches} matches, {stalls} stalls, {illegal} illegal agent commands.");

        File.WriteAllText(OutputPath, log.ToString());
        Debug.Log($"Balance sweep complete: {matches} matches, {stalls} stalls, {illegal} illegal commands -> {OutputPath}\n" +
                  string.Join(", ", wins.OrderByDescending(p => p.Value).Select(p => $"{p.Key} {p.Value}")));
    }

    /// <summary>Two copies of every card in the archetype, same as the CoreTests deck.</summary>
    private static List<string> ArchetypeDeck(Archetype archetype)
    {
        var deck = new List<string>();
        foreach (var card in CardCatalogV1.Cards.Values.Where(c => c.Archetype == archetype))
        {
            deck.Add(card.Id);
            deck.Add(card.Id);
        }
        return deck;
    }

    private static AgentPersonality PersonalityFor(Archetype archetype)
    {
        switch (archetype)
        {
            case Archetype.Expedition: return AgentPersonality.Formation();
            case Archetype.Garden: return AgentPersonality.Patient();
            case Archetype.Heart: return AgentPersonality.Relentless();
            case Archetype.Atelier: return AgentPersonality.Control();
            case Archetype.Tower: return AgentPersonality.Attrition();
            case Archetype.Ocean: return AgentPersonality.Chaotic();
            default: return AgentPersonality.Balanced();
        }
    }
}
#endif
