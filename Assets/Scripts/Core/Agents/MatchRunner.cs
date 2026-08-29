using System.Collections.Generic;

namespace LightCard.Core.Agents
{
    public class MatchResult
    {
        /// <summary>0 or 1; -1 if the command cap was reached (a stalled match).</summary>
        public int Winner = -1;
        public int Turns;
        public int CommandsIssued;
        public int FailedCommands;
        public GameState FinalState;
        public List<GameEvent> Events = new List<GameEvent>();
    }

    /// <summary>
    /// Plays a full agent-vs-agent match headlessly. This is the phantom battle
    /// in miniature — and the balance-testing harness: run thousands of seeds and
    /// diff archetype winrates.
    /// </summary>
    public static class MatchRunner
    {
        public static MatchResult PlayMatch(List<string> deck0, List<string> deck1,
            AgentPersonality personality0, AgentPersonality personality1,
            int seed, int maxCommands = 4000)
        {
            var result = new MatchResult();
            var state = GameEngine.CreateGame(deck0, deck1, seed, result.Events);
            var agents = new[]
            {
                new HeuristicAgent(0, personality0),
                new HeuristicAgent(1, personality1)
            };

            while (!state.IsOver && result.CommandsIssued < maxCommands)
            {
                var command = agents[state.ActivePlayer].ChooseCommand(state);
                var commandResult = GameEngine.Execute(state, command);
                result.CommandsIssued++;

                if (commandResult.Success)
                {
                    result.Events.AddRange(commandResult.Events);
                }
                else
                {
                    //An agent proposing an illegal command is a bug; fail safe by passing
                    result.FailedCommands++;
                    var endResult = GameEngine.Execute(state, new EndTurnCommand { Player = state.ActivePlayer });
                    if (endResult.Success) result.Events.AddRange(endResult.Events);
                }
            }

            result.Winner = state.Winner;
            result.Turns = state.TurnNumber;
            result.FinalState = state;
            return result;
        }
    }
}
