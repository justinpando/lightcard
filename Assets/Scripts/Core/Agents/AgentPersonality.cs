namespace LightCard.Core.Agents
{
    /// <summary>
    /// Evaluation weights for the heuristic agent. Personalities make a phantom
    /// play like its deck's archetype: the same search, different values.
    /// See Docs/design/phantom-ai.md.
    /// </summary>
    public class AgentPersonality
    {
        public string Name = "Balanced";

        /// <summary>Value per point of enemy life removed. Higher = more aggressive.</summary>
        public float OpponentLife = 3.0f;
        /// <summary>Value per point of own life preserved.</summary>
        public float OwnLife = 2.5f;

        /// <summary>Value per point of own unit power on the board.</summary>
        public float UnitPower = 1.0f;
        /// <summary>Value per point of own unit remaining life.</summary>
        public float UnitLife = 0.8f;
        /// <summary>Multiplier on the enemy's material (their power/life valued against us).</summary>
        public float EnemyMaterial = 1.0f;

        /// <summary>Value per lane with no enemy unit or charm in it (a lane that hits face).</summary>
        public float LaneControl = 1.5f;
        /// <summary>Value per row each unit has advanced toward the enemy.</summary>
        public float Advancement = 0.3f;
        /// <summary>Value per adjacent friendly pair (formation play).</summary>
        public float Adjacency = 0.0f;
        /// <summary>Value for standing on good space effects (and off bad ones).</summary>
        public float SpaceAlignment = 0.5f;

        /// <summary>Value per card in hand (card advantage).</summary>
        public float CardInHand = 0.4f;
        /// <summary>
        /// Value per point of max energy. Rules-v2 made Replace the only energy
        /// source, so ramp must outvalue holding the burned card for any
        /// personality — otherwise the agent never develops at all.
        /// </summary>
        public float EnergyRamp = 0.8f;
        /// <summary>Value per point of accumulated affinity (also the AL-gating unlock).</summary>
        public float Affinity = 0.3f;

        public static AgentPersonality Balanced() => new AgentPersonality();

        /// <summary>Expedition: advances as a unit and values formations and equips.</summary>
        public static AgentPersonality Formation() => new AgentPersonality
        {
            Name = "Formation",
            Adjacency = 0.8f,
            Advancement = 0.6f,
            LaneControl = 1.8f
        };

        /// <summary>Garden: delays commitment, values space effects and staying healthy.</summary>
        public static AgentPersonality Patient() => new AgentPersonality
        {
            Name = "Patient",
            SpaceAlignment = 1.2f,
            OwnLife = 3.0f,
            OpponentLife = 2.4f,
            UnitLife = 1.0f,
            CardInHand = 0.6f
        };

        /// <summary>Heart: face damage above all; happily trades life for tempo.</summary>
        public static AgentPersonality Relentless() => new AgentPersonality
        {
            Name = "Relentless",
            OpponentLife = 4.5f,
            OwnLife = 1.5f,
            Advancement = 0.5f,
            LaneControl = 2.2f
        };

        /// <summary>Atelier: card advantage and trades; values removal over racing.</summary>
        public static AgentPersonality Control() => new AgentPersonality
        {
            Name = "Control",
            CardInHand = 0.7f,
            EnemyMaterial = 1.4f,
            OwnLife = 2.8f,
            OpponentLife = 2.2f,
            Advancement = 0.15f
        };

        /// <summary>Tower: defensive lines and on-death value; wins slow.</summary>
        public static AgentPersonality Attrition() => new AgentPersonality
        {
            Name = "Attrition",
            OwnLife = 3.2f,
            OpponentLife = 2.0f,
            UnitLife = 1.1f,
            EnemyMaterial = 1.2f,
            Advancement = 0.05f,
            LaneControl = 1.0f,
            CardInHand = 0.6f
        };

        /// <summary>Ocean: transformation and sacrifice combos; own material is fuel.</summary>
        public static AgentPersonality Chaotic() => new AgentPersonality
        {
            Name = "Chaotic",
            UnitLife = 0.5f,
            UnitPower = 1.3f,
            OwnLife = 2.0f,
            OpponentLife = 3.2f,
            SpaceAlignment = 0.9f,
            Advancement = 0.4f
        };
    }
}
