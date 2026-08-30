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
        OnAttack,
        /// <summary>Fires after this unit is Shifted (any direction; voluntary moves only).</summary>
        OnShift,
        /// <summary>Fires when the owner calls a unit or charm to a space adjacent to this; target coords are the called unit's space.</summary>
        OnAllyCallAdjacent,
        /// <summary>Fires when the owner calls a unit or charm to the space directly in front of this; target coords are the called unit's space.</summary>
        OnAllyCallInFront,
        /// <summary>Fires when the owner applies a space effect (Navigator).</summary>
        OnOwnerSpaceEffect,
        /// <summary>Equip charms only: resolved on the unit that enters the equip's space.</summary>
        OnEquip
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
        PushAway,
        /// <summary>Pin units in Scope: no Shift or Auto-Advance until their owner's turn ends (Garden).</summary>
        Pin,
        /// <summary>Give units in Scope Amount Poison: they take that much damage at the start of their owner's turn (Garden).</summary>
        Poison,
        /// <summary>
        /// Call CalledCardId to empty spaces on the owner's half. SpaceEffect
        /// filters candidate spaces (None = any); Amount &gt; 0 picks that many at
        /// random (deterministic RNG), Amount 0 fills every match (Garden tokens).
        /// </summary>
        CallUnit,
        /// <summary>Forced movement of units in Scope one space toward the effect's owner; collisions as Push (Garden).</summary>
        Pull,
        /// <summary>Grant Power temp attack until the start of the unit's owner's next turn.</summary>
        GainTempPower,
        /// <summary>Grant Amount temp Parry (prevents combat damage) until the start of the unit's owner's next turn.</summary>
        GainTempParry,
        /// <summary>Grant Amount temp Evade (prevents any damage) until the start of the unit's owner's next turn.</summary>
        GainTempEvade,
        /// <summary>Grant Amount permanent Armor to units in Scope.</summary>
        GainArmor,
        /// <summary>Static: grant Parry Amount while the condition holds (Trigger.Static only).</summary>
        StaticParry,
        /// <summary>Static: units in Scope have Auto-Advance while the condition holds (Trigger.Static only).</summary>
        StaticAutoAdvance,
        /// <summary>Grant Amount permanent Parry to units in Scope.</summary>
        GainParry,
        /// <summary>Grant Amount permanent Resist to units in Scope.</summary>
        GainResist,
        /// <summary>Make units in Scope permanently Heavy.</summary>
        GrantHeavy,
        /// <summary>Attach EffectDef.Granted as a permanent extra ability of units in Scope (equips, Ocean mutations).</summary>
        GrantAbility
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
        NearestEnemyInLane,
        /// <summary>Friendly units in the eight spaces around the source (glossary "Nearby").</summary>
        Nearby,
        /// <summary>Friendly non-charm units that have at least one adjacent friendly unit or charm.</summary>
        FriendlyUnitsWithAdjacentAlly
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
        /// <summary>Catalog id of the card summoned by CallUnit.</summary>
        public string CalledCardId;
        /// <summary>The ability attached by GrantAbility.</summary>
        public EffectDef Granted;
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
        /// <summary>May move and attack in the same turn.</summary>
        public bool Agile;
        /// <summary>Prevents combat damage this many times per turn.</summary>
        public int Parry;
        /// <summary>Cannot be moved by Push or Pull.</summary>
        public bool Heavy;
        /// <summary>
        /// Equip charm: a friendly unit may enter its space, consuming the charm
        /// and receiving its Trigger.OnEquip effects (glossary "Equip").
        /// </summary>
        public bool IsEquip;

        public PlayTargetKind PlayTarget = PlayTargetKind.None;

        public List<EffectDef> Effects = new List<EffectDef>();
    }
}
