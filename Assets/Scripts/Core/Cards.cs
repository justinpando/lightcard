using System.Collections.Generic;

namespace LightCard.Core
{
    public enum Archetype { Garden, Atelier, Heart, Ocean, Tower, Expedition }

    public enum CardType { Unit, Charm, Ability }

    public enum SpaceEffectType
    {
        None, Verdant, Brambled, Vista,
        /// <summary>Ability damage to a unit on this space is +2; consumed by that boost (Atelier).</summary>
        Primed,
        /// <summary>Units here have +1/0 and take 1 damage at the end of their owner's turn (Heart).</summary>
        Inferno,
        /// <summary>Units called to this space gain a random keyword (Ocean).</summary>
        Flooded,
        /// <summary>Units here lose 1/1 at the start of their owner's turn (Ocean).</summary>
        Desert,
        /// <summary>Units here cannot be healed (provisional ruling; Heart's Scorch line).</summary>
        Scorched,
        /// <summary>Shifting into this space costs +1; units damaged here are pushed toward their backline (Expedition).</summary>
        Rugged
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
        OnEquip,
        /// <summary>Fires after this unit's attack destroys a unit (Prideful Soul).</summary>
        OnKill,
        /// <summary>Fires after this unit takes damage and survives.</summary>
        OnDamaged,
        /// <summary>Fires after this Guardian redirects a strike onto itself.</summary>
        OnGuard,
        /// <summary>Spirit cards: fires (host as source) when the spirit's bond breaks.</summary>
        OnBondBreak,
        /// <summary>Fires on a unit when a spirit binds to it (Attuners).</summary>
        OnBonded,
        /// <summary>Fires at the start of the owner's turn (Living Torrent).</summary>
        StartOfTurn,
        /// <summary>Fires at the start of the OPPOSING player's turn (Grotesque Mirror).</summary>
        OnEnemyTurnStart,
        /// <summary>Fires when the owner discards a card - burns and effect discards alike (Keeper of Debts).</summary>
        OnOwnerDiscard,
        /// <summary>Fires when any friendly unit or charm is destroyed (Mourner's Altar).</summary>
        OnFriendlyDestroyed,
        /// <summary>Fires when a friendly unit or charm is destroyed in the eight surrounding spaces; target coords are its space (Dinner Bell).</summary>
        OnFriendlyDestroyedNearby,
        /// <summary>Fires whenever either player draws a card (Seer's Guillotine).</summary>
        OnAnyDraw,
        /// <summary>Fires when the owner pays this card's ActivateCost (rules-v3 activations).</summary>
        OnActivate,
        /// <summary>Fires on the attacker after its strike deals damage; target coords are the victim's space (Ferocity, Reverse Engineer).</summary>
        OnDealtDamage,
        /// <summary>Fires on the owner's OTHER units when a friendly unit's attack destroys an enemy; target coords are the killer's space (Covenant of Valor).</summary>
        OnFriendlyKill,
        /// <summary>Fires on the owner's units when the owner takes player damage (Flagellant's Charm).</summary>
        OnOwnerPlayerDamaged,
        /// <summary>Fires on the owner's units when one of the owner's spirit bonds breaks (Soulcatcher).</summary>
        OnOwnerBondBreak,
        /// <summary>Fires when the OPPOSING player discards a card (Amber Spyglass).</summary>
        OnEnemyDiscard,
        /// <summary>Fires on the HOST unit when its own spirit bond breaks (Spirit Caller).</summary>
        OnOwnBondBreak,
        /// <summary>Fires on a unit when an Equip charm attaches to it (Bauble Merchant).</summary>
        OnEquipped
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
        GrantAbility,
        /// <summary>Static: the owner's first Ability each turn costs Amount less (Scholar).</summary>
        StaticAbilityDiscount,
        /// <summary>Take the first card in the deck costing at most Amount; Units are called to a random empty own-half space, others go to hand (Quick Sketch).</summary>
        TutorLowCost,
        /// <summary>Add a random Ability card from the catalog to the owner's hand (Ailing Scholar).</summary>
        AddRandomAbility,
        /// <summary>Add a copy of the card played before this one to the owner's hand (Trace).</summary>
        CopyLastCard,
        /// <summary>The unit becomes CalledCardId, keeping damage, bonuses, and statuses (Prideful Soul).</summary>
        TransformSelf,
        /// <summary>Deal Amount damage to both players (Spirit of Balance).</summary>
        DamageBothPlayers,
        /// <summary>Apply SpaceEffect to the source's space AND the mirrored space on the enemy half (Spirit of Inferno).</summary>
        ApplySpaceEffectMirrored,
        /// <summary>Add a random spirit card from the catalog to the owner's hand (Dreamwalker).</summary>
        AddRandomSpirit,
        /// <summary>Destroy the targeted friendly unit, then deal its remaining life as ability damage to every enemy in its lane (Release Energy).</summary>
        SacrificeLaneBurst,
        /// <summary>The owner discards Amount random cards from hand.</summary>
        DiscardRandom,
        /// <summary>The opposing player discards Amount random cards from hand (Grotesque Mirror).</summary>
        EnemyDiscardRandom,
        /// <summary>The owner gains Amount life (uncapped).</summary>
        GainPlayerLife,
        /// <summary>OnAnyDraw watchers: the player who drew takes Amount damage (Seer's Guillotine).</summary>
        DamageDrawingPlayer,
        /// <summary>Call CalledCardId at the trigger's target space if it is empty and on the owner's half (Dinner Bell, Floppier Fish).</summary>
        CallUnitAtTarget,
        /// <summary>Units in Scope gain a random small keyword: +1 Armor, Pierce, Parry, +1/0, or +0/1 (Ocean).</summary>
        GainRandomKeyword,
        /// <summary>The source moves to a random empty adjacent space on its half (Floppy Fish).</summary>
        MoveRandomAdjacent,
        /// <summary>Destroy adjacent friendly units: one random if Amount is 1, all if 0. Source gains +1/+1 per unit consumed, sheds Flux, and with SpaceEffect set also rolls a random keyword each (Fractal, Amalgam).</summary>
        ConsumeAdjacent,
        /// <summary>Per lane: the nearest enemy takes Amount ability damage and its space becomes SpaceEffect (Tidal Wave).</summary>
        LaneProjectiles,
        /// <summary>Every Flooded space becomes Desert and vice versa (Mirage).</summary>
        SwapFloodDesert,
        /// <summary>Static: the source has +Power attack per board space bearing SpaceEffect (Living Torrent).</summary>
        StaticPowerPerSpace,
        /// <summary>Call CalledCardId to a random empty space adjacent (8-way) to the source, on the owner's half (Dinner Bell's activation).</summary>
        CallUnitNearby,
        /// <summary>Deal 2 ability damage to the target space's occupant; if the space carried an effect, it becomes Scorched (Scorch).</summary>
        ScorchSpace,
        /// <summary>Static: units in Scope heal Amount at their owner's turn start (Enchanted Rose, Spirit Fencer).</summary>
        StaticRegen,
        /// <summary>Deal Amount to the unit whose attack broke this bond, if any (Spirit of Reprisal).</summary>
        DamageBreaker,
        /// <summary>Heal units in Scope to full (Covenant of Valor).</summary>
        HealFull,
        /// <summary>Friendly units behind the target space in its lane each advance one step (War Drum).</summary>
        AdvanceBehindTarget,
        /// <summary>2 ability damage to the target unit; a surviving charm is restored to full instead (Percussive Maintenance).</summary>
        PercussiveMend,
        /// <summary>If the trigger's target is an enemy charm, add a copy of its card to hand (Reverse Engineer).</summary>
        CopyStruckCharm,
        /// <summary>Return the target unit or charm to its owner's hand, dropping all bonuses (Lose Hope, adapted to one target).</summary>
        ReturnToHand,
        /// <summary>Destroy the targeted friendly charm; every unit in its lane takes its remaining life as ability damage (Shatter).</summary>
        ShatterCharm,
        /// <summary>Attune a random hand card for free: discard it for +1 max energy and +1 of its Affinity (Flagellant's Charm).</summary>
        AutoAttune,
        /// <summary>Deal the target friendly unit's life to every occupant in front of it, then destroy it (Lash Out).</summary>
        LashOut,
        /// <summary>Destroy the target friendly non-spirit unit, gain energy equal to its cost, draw a random spirit (Ritual of Ascendance).</summary>
        SacrificeForEnergy,
        /// <summary>Destroy the target friendly unit, re-call it to the same space with +1/0 and Regen 1 (Ritual of Renewal).</summary>
        RebirthTarget,
        /// <summary>Each friendly unit in the target lane deals its life to the space in front of it, then dies (Ritual of Reckoning).</summary>
        ReckoningColumn,
        /// <summary>Break the target unit's bond (triggers fire), draw a card, and bind it if it is a spirit (Ritual of Reinvention).</summary>
        ReinventSpirit,
        /// <summary>Destroy the target unit and randomly distribute its power and life to its owner's nearby units (Dispersal).</summary>
        DisperseTarget,
        /// <summary>Deal 1 ability damage to a random enemy per Flooded space on the board (Beached Whale).</summary>
        FloodBarrage,
        /// <summary>Add the most recently broken spirit's card to the owner's hand (Soulcatcher).</summary>
        ReclaimSpirit,
        /// <summary>Reduce the cost of a random Ability copy in the owner's hand by Amount, permanently for that copy (Attenuating Rod).</summary>
        DiscountRandomHandAbility,
        /// <summary>Static: calling to the space directly behind the source costs Amount less (Trailblazer, Flagbearer).</summary>
        StaticCallDiscountBehind,
        /// <summary>Absorb the owner's half: +1/+1 per own-half space effect, then clear them (Geo).</summary>
        GeoAbsorb,
        /// <summary>Bless the owner's next Unit call: Amount cost discount, +Power/+Life, and EffectDef.Granted attached (Virtuous/Valorous Call).</summary>
        BlessNextCall,
        /// <summary>Schedule: at the start of the owner's next turn, destroy whatever occupies the target space (Sword of Damocles).</summary>
        ScheduleDoom,
        /// <summary>Schedule: at the start of the owner's next turn, rebind a copy of the just-broken spirit to the source (Spirit Caller).</summary>
        ScheduleRebind,
        /// <summary>Frontline friendly units gain +2/0, temp Overpower, and die at end of turn (Give Your All).</summary>
        GiveYourAll,
        /// <summary>Friendly units in the target lane gain Push until end of turn (Break Through).</summary>
        TempPushLane,
        /// <summary>Every friendly unit that moved this turn immediately attacks (Superior Tempo).</summary>
        AttackWithMoved,
        /// <summary>Banish all your charms (no destroy triggers); gain a Valuable Coin card per charm removed (Fire Sale).</summary>
        FireSale,
        /// <summary>Add a random Charm costing at most Amount to the owner's hand (Bauble Merchant).</summary>
        AddRandomCheapCharm,
        /// <summary>The source gains a charge; every Amount-th charge, the owner draws a card (Message in a Bottle).</summary>
        ChargeDraw,
        /// <summary>Draw a random Unit costing at most Amount from the deck to hand, tagging that copy with a random keyword applied when called (Focus Form).</summary>
        DrawLowCostUnitWithKeyword,
        /// <summary>On the source's space: spawn the CalledCardId charm if the space is empty, else return the card to the owner's hand (Valuable Coin's drop).</summary>
        DropCharm
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
        /// <summary>All units in the target space's lane (same x), friend or foe.</summary>
        TargetLane,
        /// <summary>Friendly units with no enemy unit or charm in their lane.</summary>
        UnblockedFriendlyUnits,
        /// <summary>The nearest enemy unit or charm in the source's lane, scanning from the enemy frontline back.</summary>
        NearestEnemyInLane,
        /// <summary>Every enemy unit or charm in the source's lane (Spirit of Wrath).</summary>
        EnemiesInLane,
        /// <summary>Every enemy unit and charm on the board.</summary>
        AllEnemyUnits,
        /// <summary>Every unit and charm on the board except the source itself, both sides (Thunder Rod).</summary>
        AllOtherUnits,
        /// <summary>The space (or its occupant) directly in front of the source.</summary>
        InFront,
        /// <summary>Friendly units in the eight spaces around the source (glossary "Nearby").</summary>
        Nearby,
        /// <summary>Friendly non-charm units that have at least one adjacent friendly unit or charm.</summary>
        FriendlyUnitsWithAdjacentAlly,
        /// <summary>Spaces orthogonally adjacent to the target space (Oasis).</summary>
        AdjacentToTarget,
        /// <summary>One random space in the eight around the source (Living Torrent's flood).</summary>
        RandomNearbySpace
    }

    /// <summary>Extra requirement for static effects.</summary>
    public enum EffectCondition
    {
        None,
        /// <summary>Source is on its owner's frontline row.</summary>
        Frontline,
        /// <summary>No enemy unit or charm in the source's lane.</summary>
        Unblocked,
        /// <summary>Source has a spirit bound to it (Spirit Fencer).</summary>
        WhileBonded
    }

    /// <summary>What a card may be aimed at when played.</summary>
    public enum PlayTargetKind
    {
        /// <summary>Units and charms: an empty space on the owner's half.</summary>
        FriendlyEmptySpace,
        None,
        AnySpace,
        AnyUnit,
        /// <summary>Spirits: a friendly non-charm, non-spirit-bearing unit to bind to.</summary>
        FriendlyUnit
    }

    /// <summary>Whose side a target slot accepts.</summary>
    public enum Team { Any, Self, Enemy }

    /// <summary>
    /// One target slot of a multi-target card. A card with a non-empty
    /// Targets list ignores PlayTarget: the player picks one unit per slot,
    /// in order, all distinct; effects address slots via EffectDef.TargetIndex.
    /// </summary>
    public class TargetDef
    {
        public Team Team = Team.Any;
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
        /// <summary>On multi-target cards, which target slot this effect resolves against.</summary>
        public int TargetIndex;
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
        /// <summary>Takes attack damage in place of friendly units beside or behind it.</summary>
        public bool Guardian;
        /// <summary>
        /// Spirit unit (Heart): played onto a friendly unit instead of a space.
        /// While bonded, the host gains the spirit's printed Parry/Resist/Rush and
        /// the spirit takes all damage; its Trigger.OnBondBreak effects fire (with
        /// the host as source) when it is destroyed.
        /// </summary>
        public bool IsSpirit;
        /// <summary>Energy cost of this card's activatable ability; -1 = none (rules-v3).</summary>
        public int ActivateCost = -1;
        /// <summary>X-bound (Ocean): destroyed at its owner's turn start unless standing on this effect.</summary>
        public SpaceEffectType BoundTo = SpaceEffectType.None;
        /// <summary>Immune to the Desert drain (Sand Shark).</summary>
        public bool DesertImmune;
        /// <summary>While any unit with this is on the board, Verdant and Bramble magnitudes are doubled (Sunlamp).</summary>
        public bool BoostsGardenEffects;
        /// <summary>Cannot Shift or Auto-Advance, and cannot be moved by effects (Beached Whale).</summary>
        public bool Immobile;
        /// <summary>Attacks pass over blockers and hit the player directly (Spirit of Perspective's Flying).</summary>
        public bool Flying;
        /// <summary>Attacks deal double damage when the lane is unblocked (Spirit of Opportunity).</summary>
        public bool DoubleWhenUnblocked;
        /// <summary>Ability damage taken is increased by this much (Spirit of Vulnerability's Amp).</summary>
        public int Amplify;
        /// <summary>Ability damage aimed at this is redirected to the mirrored space's occupant (Dark Mirror).</summary>
        public bool Reflects;
        /// <summary>Ability damage aimed at this also hits adjacent spaces' occupants (Crystal Amplifier).</summary>
        public bool SplashesAdjacent;
        /// <summary>Equip charm that friendly CHARMS may also enter (Adaptive Armature).</summary>
        public bool EquipsCharms;
        /// <summary>Geo: costs 1 less per space effect on the owner's half.</summary>
        public bool CostPerOwnSpaceEffect;

        public PlayTargetKind PlayTarget = PlayTargetKind.None;

        /// <summary>Target slots for multi-target cards; non-empty overrides PlayTarget.</summary>
        public List<TargetDef> Targets = new List<TargetDef>();

        public List<EffectDef> Effects = new List<EffectDef>();
    }
}
