using System.Collections.Generic;

namespace LightCard.Core
{
    public enum Archetype { Garden, Atelier, Heart, Ocean, Tower, Expedition }

    public enum CardType { Unit, Charm, Ability }

    public enum SpaceEffectType
    {
        None, Verdant, Brambled, Vista,
        /// <summary>Ability damage to a unit on this space is +2; consumed by that boost (Atelier).</summary>
        Primed
    }

    /// <summary>When an effect fires.</summary>
    public enum Trigger
    {
        /// <summary>Ability cards: resolves when the card is played.</summary>
        OnPlay,
        /// <summary>Fires when this unit or charm is called to the field.</summary>
        OnCall,
        /// <summary>Fires when this unit or charm is destroyed.</summary>
        OnDestroy,
        /// <summary>Fires at the end of the owner's turn while on the field.</summary>
        EndOfTurn,
        /// <summary>Fires when this unit moves toward the enemy field.</summary>
        OnAdvance,
        /// <summary>Fires when this unit moves toward its own back line.</summary>
        OnRetreat,
        /// <summary>Fires after this unit or charm is attacked (if it survives).</summary>
        OnAttacked,
        /// <summary>Fires when the opposing player calls a unit.</summary>
        OnEnemyCall,
        /// <summary>Continuous effect, recomputed on demand (auras and conditional stats).</summary>
        Static,
        /// <summary>Fires after the owner resolves an Ability card (Atelier).</summary>
        OnOwnerAbilityPlay,
        /// <summary>Fires after the opposing player resolves an Ability card (Atelier).</summary>
        OnEnemyAbilityPlay,
        /// <summary>Fires after this unit attacks another unit (not the player); target coords are the victim's space.</summary>
        OnAttack
    }

    /// <summary>What an effect does. Magnitudes live in EffectDef.Power/Life/Amount.</summary>
    public enum EffectAction
    {
        /// <summary>Permanently modify stats of the targets.</summary>
        GainStats,
        /// <summary>Grant stats while the static condition holds (Trigger.Static only).</summary>
        StaticStats,
        /// <summary>Grant armor while the static condition holds (Trigger.Static only).</summary>
        StaticArmor,
        /// <summary>Heal targets by Amount.</summary>
        Heal,
        /// <summary>Apply EffectDef.SpaceEffect to the target space(s).</summary>
        ApplySpaceEffect,
        /// <summary>All friendly units advance one space (front-most first).</summary>
        AdvanceAllFriendly,
        /// <summary>Owner draws Amount cards.</summary>
        Draw,
        /// <summary>Owner gains Amount current-turn energy.</summary>
        GainEnergy,
        /// <summary>Put the target unit to sleep.</summary>
        SetAsleep,
        /// <summary>Permanently gain Power per board space with EffectDef.SpaceEffect applied.</summary>
        GainPowerPerSpaceEffect,
        /// <summary>Permanently gain Life per board space with EffectDef.SpaceEffect applied.</summary>
        GainLifePerSpaceEffect,
        /// <summary>Ability damage: Amount to each unit in Scope (ignores armor; Resist and Primed apply).</summary>
        DealDamage,
        /// <summary>
        /// Ability damage swept along the target lane from the caster's side:
        /// starts at Power, changes by Amount after each unit hit, stops at 0.
        /// Hits every unit in the path, friend or foe.
        /// </summary>
        LaneDamage,
        /// <summary>Remove the target space's effect; if one was removed and Amount &gt; 0, deal Amount ability damage to its occupant.</summary>
        ClearSpaceEffect,
        /// <summary>Grant Amount permanent Pierce to units in Scope.</summary>
        GainPierce,
        /// <summary>Push units in Scope one space away from the effect's owner.</summary>
        PushAway
    }

    /// <summary>Which units an effect applies to, relative to its source or its play target.</summary>
    public enum TargetScope
    {
        Self,
        /// <summary>The unit occupying the space the ability targeted.</summary>
        TargetUnit,
        /// <summary>The space the ability targeted.</summary>
        TargetSpace,
        /// <summary>All units in the row (same y) of the play target space.</summary>
        TargetRow,
        /// <summary>All units in the source's row (same y).</summary>
        SourceRow,
        /// <summary>Spaces/units orthogonally adjacent to the source (front, behind, sides).</summary>
        Adjacent,
        /// <summary>The row one space forward of the source, from its owner's perspective.</summary>
        RowInFront,
        AllFriendlyUnits,
        /// <summary>Friendly units with no enemy unit or charm in their lane.</summary>
        UnblockedFriendlyUnits,
        /// <summary>The nearest enemy unit or charm in the source's lane, scanning from the enemy frontline back.</summary>
        NearestEnemyInLane
    }

    /// <summary>Extra requirement for static effects.</summary>
    public enum EffectCondition
    {
        None,
        /// <summary>Source is on its owner's frontline row.</summary>
        Frontline
    }

    /// <summary>What a card may be aimed at when played.</summary>
    public enum PlayTargetKind
    {
        /// <summary>Units and charms: an empty space on the owner's half.</summary>
        FriendlyEmptySpace,
        None,
        AnySpace,
        AnyUnit
    }

    /// <summary>
    /// One data-driven effect: when Trigger fires, apply Action to Scope.
    /// This is the Condition / Trigger / Target / Effect grammar from the design sheet.
    /// </summary>
    public class EffectDef
    {
        public Trigger Trigger;
        public EffectAction Action;
        public TargetScope Scope = TargetScope.Self;
        public EffectCondition Condition = EffectCondition.None;
        public int Power;
        public int Life;
        public int Amount;
        public SpaceEffectType SpaceEffect = SpaceEffectType.None;
    }

    /// <summary>Immutable definition of a card. Instances on the board are UnitState.</summary>
    public class CardDefinition
    {
        public string Id;              //unique name, matches the design sheet
        public Archetype Archetype;
        public CardType Type;
        public int Cost;
        /// <summary>
        /// Affinity Level in this card's archetype required to play it (rules-v2).
        /// -1 = use the default (cost - 1, min 0), resolved at catalog build.
        /// </summary>
        public int AffinityRequirement = -1;
        public int Power;              //Atk on the sheet
        public int Life;               //Def on the sheet
        public string Text = "";

        //Keywords
        public int Armor;
        public int Pierce;
        public int Resist;             //reduces ability damage (never attack damage)
        public int Retaliate;          //deal N damage to attackers
        public bool PushOnAttack;
        public bool AutoAdvance;
        public bool Ranged;
        public bool Rush;

        public PlayTargetKind PlayTarget = PlayTargetKind.None;

        public List<EffectDef> Effects = new List<EffectDef>();
    }
}
