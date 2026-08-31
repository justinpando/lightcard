using System.Collections.Generic;
using System.Linq;

namespace LightCard.Core.Agents
{
    /// <summary>
    /// The Tier-1/2 phantom pilot from Docs/design/phantom-ai.md: enumerates every
    /// legal command, simulates each on a cloned state through the real engine,
    /// scores the result with its personality's evaluation, and takes the best.
    /// Because it searches over the actual rules it never plays illegally and
    /// automatically understands every keyword — new cards need no AI code.
    /// Fully deterministic: same state, same personality, same choice.
    /// </summary>
    public class HeuristicAgent
    {
        public readonly int Player;
        public readonly AgentPersonality Personality;

        public HeuristicAgent(int player, AgentPersonality personality)
        {
            Player = player;
            Personality = personality;
        }

        /// <summary>Pick the next command for the current turn (call until it returns EndTurn).</summary>
        public Command ChooseCommand(GameState state)
        {
            Command best = new EndTurnCommand { Player = Player };
            float bestScore = ScoreCommand(state, best);

            foreach (var candidate in EnumerateCommands(state))
            {
                float score = ScoreCommand(state, candidate);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        private float ScoreCommand(GameState state, Command command)
        {
            var clone = state.Clone();
            var result = GameEngine.Execute(clone, command);
            return result.Success ? Evaluate(clone, Player, Personality) : float.MinValue;
        }

        private IEnumerable<Command> EnumerateCommands(GameState state)
        {
            var playerState = state.Players[Player];

            //Card plays
            for (int handIndex = 0; handIndex < playerState.Hand.Count; handIndex++)
            {
                var definition = CardCatalogV1.Get(playerState.Hand[handIndex]);
                if (state.EffectiveCost(Player, definition) > playerState.Energy) continue;
                if (playerState.Affinity[definition.Archetype] < definition.AffinityRequirement) continue;

                if (definition.PlayTarget == PlayTargetKind.FriendlyUnitThenEnemyUnit)
                {
                    //Two-target cards (Lose Hope): every friendly x enemy pair
                    foreach (var friendly in state.UnitsOf(Player).ToList())
                        foreach (var enemy in state.Units.Where(u => u.Owner != Player).ToList())
                            yield return new PlayCardCommand
                            {
                                Player = Player, HandIndex = handIndex,
                                TargetX = friendly.X, TargetY = friendly.Y,
                                Target2X = enemy.X, Target2Y = enemy.Y
                            };
                    continue;
                }

                foreach (var (x, y) in EnumerateTargets(state, definition))
                    yield return new PlayCardCommand { Player = Player, HandIndex = handIndex, TargetX = x, TargetY = y };
            }

            //Activations (rules-v3: attacks are automatic; this is the manual unit action)
            foreach (var unit in state.UnitsOf(Player))
            {
                if (unit.Definition.ActivateCost < 0 || unit.ActivatedThisTurn) continue;
                if (unit.Flux || unit.Asleep) continue;
                if (unit.Definition.ActivateCost > playerState.Energy) continue;
                yield return new ActivateCommand { Player = Player, UnitId = unit.Id };
            }

            //Shift (only if the deck brought it)
            if (playerState.Power == PlayerPower.Shift &&
                !playerState.PowerUsedThisTurn && playerState.Energy >= GameConfig.ShiftEnergyCost)
            {
                foreach (var unit in state.UnitsOf(Player).Where(u => !u.IsCharm && !u.Asleep && !u.Pinned && (!u.AttackedThisTurn || u.Definition.Agile)))
                {
                    yield return new ShiftCommand { Player = Player, UnitId = unit.Id, Direction = MoveDirection.Forward };
                    yield return new ShiftCommand { Player = Player, UnitId = unit.Id, Direction = MoveDirection.Back };
                    yield return new ShiftCommand { Player = Player, UnitId = unit.Id, Direction = MoveDirection.Left };
                    yield return new ShiftCommand { Player = Player, UnitId = unit.Id, Direction = MoveDirection.Right };
                }
            }

            //Clear (only if the deck brought it)
            if (playerState.Power == PlayerPower.Clear &&
                !playerState.PowerUsedThisTurn && playerState.Energy >= GameConfig.ClearEnergyCost)
            {
                for (int x = 0; x < GameConfig.Lanes; x++)
                    for (int y = 0; y < GameConfig.Rows; y++)
                        if (state.SpaceEffects[x, y] != SpaceEffectType.None)
                            yield return new ClearCommand { Player = Player, X = x, Y = y };
            }

            //Replace (ramp)
            if (!playerState.ReplaceUsedThisTurn)
            {
                for (int handIndex = 0; handIndex < playerState.Hand.Count; handIndex++)
                    yield return new ReplaceCardCommand { Player = Player, HandIndex = handIndex };
            }
        }

        private IEnumerable<(int x, int y)> EnumerateTargets(GameState state, CardDefinition definition)
        {
            switch (definition.PlayTarget)
            {
                case PlayTargetKind.None:
                    yield return (0, 0);
                    break;
                case PlayTargetKind.FriendlyEmptySpace:
                {
                    for (int x = 0; x < GameConfig.Lanes; x++)
                    {
                        for (int y = 0; y < GameConfig.Rows; y++)
                        {
                            if (GameState.SideOfRow(y) != Player) continue;
                            var occupant = state.GetUnitAt(x, y);
                            if (occupant == null || (occupant.Owner == Player && occupant.Definition.IsEquip && definition.Type != CardType.Charm))
                                yield return (x, y);
                        }
                    }
                    break;
                }
                case PlayTargetKind.AnySpace:
                {
                    for (int x = 0; x < GameConfig.Lanes; x++)
                        for (int y = 0; y < GameConfig.Rows; y++)
                            yield return (x, y);
                    break;
                }
                case PlayTargetKind.AnyUnit:
                {
                    foreach (var unit in state.Units)
                        yield return (unit.X, unit.Y);
                    break;
                }
                case PlayTargetKind.FriendlyUnit:
                {
                    foreach (var unit in state.UnitsOf(Player).Where(u => !u.IsCharm && !u.Definition.IsSpirit))
                        yield return (unit.X, unit.Y);
                    break;
                }
            }
        }

        //---- Evaluation ----

        public static float Evaluate(GameState state, int me, AgentPersonality p)
        {
            if (state.Winner == me) return float.MaxValue;
            if (state.Winner == 1 - me) return float.MinValue;

            int opponent = 1 - me;
            float score = 0f;

            score += state.Players[me].Life * p.OwnLife;
            score -= state.Players[opponent].Life * p.OpponentLife;

            foreach (var unit in state.Units)
            {
                float material = state.EffectivePower(unit) * p.UnitPower
                               + state.CurrentLife(unit) * p.UnitLife
                               + state.EffectiveArmor(unit) * 0.5f;

                //Pending poison damage and pin tempo discount a unit's value for
                //whoever owns it - this is how the agent "sees" DoT and lockdown
                material -= unit.Poison * 1.2f;
                if (unit.Pinned) material -= 0.4f;

                if (unit.Owner != me)
                {
                    score -= material * p.EnemyMaterial;
                    continue;
                }

                score += material;
                score += RowsAdvanced(unit) * p.Advancement;

                var spaceEffect = state.SpaceEffects[unit.X, unit.Y];
                if (spaceEffect == SpaceEffectType.Verdant || spaceEffect == SpaceEffectType.Vista) score += p.SpaceAlignment;
                else if (spaceEffect == SpaceEffectType.Brambled) score -= p.SpaceAlignment;

                if (p.Adjacency != 0f)
                {
                    foreach (var other in state.Units)
                    {
                        if (other.Owner != me || other == unit) continue;
                        if (System.Math.Abs(other.X - unit.X) + System.Math.Abs(other.Y - unit.Y) == 1)
                            score += p.Adjacency * 0.5f; //each pair counted from both ends
                    }
                }
            }

            for (int lane = 0; lane < GameConfig.Lanes; lane++)
            {
                if (state.LaneUnblockedFor(me, lane)) score += p.LaneControl;
                if (state.LaneUnblockedFor(opponent, lane)) score -= p.LaneControl;
            }

            score += (state.Players[me].Hand.Count - state.Players[opponent].Hand.Count) * p.CardInHand;
            score += state.Players[me].MaxEnergy * p.EnergyRamp;
            score += state.Players[me].Affinity.Values.Sum() * p.Affinity;

            return score;
        }

        private static int RowsAdvanced(UnitState unit) =>
            unit.Owner == 0 ? unit.Y : GameConfig.Rows - 1 - unit.Y;
    }
}
