using System.Collections.Generic;

namespace LightCard.Core
{
    /// <summary>
    /// The playable set: 10 Expedition + 10 Garden (v1) + 12 Atelier (v2, the
    /// ability-damage slice) from the design sheet, chosen because their printed
    /// text is implementable with the current trigger/effect grammar. Long-term
    /// this catalog is imported from the same spreadsheet that feeds
    /// CardDataImporter; until then CardAssetSync mirrors it into Card assets.
    /// </summary>
    public static class CardCatalogV1
    {
        public static readonly Dictionary<string, CardDefinition> Cards = Build();

        public static CardDefinition Get(string id) => Cards[id];

        private static Dictionary<string, CardDefinition> Build()
        {
            var list = new List<CardDefinition>
            {
                //---- Expedition ----
                new CardDefinition
                {
                    Id = "Conscript", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 1, Power = 1, Life = 2,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "Eager Recruit", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 1, Power = 1, Life = 2,
                    Text = "Auto-Advance. Advance: Gain +1/0. Retreat: Gain +0/1.",
                    AutoAdvance = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnAdvance, Action = EffectAction.GainStats, Scope = TargetScope.Self, Power = 1 },
                        new EffectDef { Trigger = Trigger.OnRetreat, Action = EffectAction.GainStats, Scope = TargetScope.Self, Life = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Shieldbearer", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 2, Power = 1, Life = 4,
                    Text = "Armor 1. Adjacent allies gain Armor 1.",
                    Armor = 1,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticArmor, Scope = TargetScope.Adjacent, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Spearbearer", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 2, Power = 2, Life = 3,
                    Text = "Pierce 1. Adjacent allies gain +1/0.",
                    Pierce = 1,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticStats, Scope = TargetScope.Adjacent, Power = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Vanguard", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 3, Power = 2, Life = 5,
                    Text = "Auto-Advance. Frontline: Has +2/0.",
                    AutoAdvance = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticStats, Scope = TargetScope.Self, Condition = EffectCondition.Frontline, Power = 2 }
                    }
                },
                new CardDefinition
                {
                    Id = "Siege Knight", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 4, Power = 2, Life = 6,
                    Text = "Armor 1. Push.",
                    Armor = 1, PushOnAttack = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "Navigator", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 2, Power = 1, Life = 3,
                    Text = "Frontline: When you apply a Space Effect, draw a card.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnOwnerSpaceEffect, Action = EffectAction.Draw, Condition = EffectCondition.Frontline, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Battering Ram", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 3, Power = 0, Life = 5,
                    Text = "On Advance: Push.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnAdvance, Action = EffectAction.PushAway, Scope = TargetScope.NearestEnemyInLane }
                    }
                },
                new CardDefinition
                {
                    Id = "Squad Leader", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 3, Power = 1, Life = 4,
                    Text = "Units called to adjacent spaces gain +1/+1.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnAllyCallAdjacent, Action = EffectAction.GainStats, Scope = TargetScope.TargetUnit, Power = 1, Life = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Skilled Armorer", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 3, Power = 1, Life = 4,
                    Text = "Units called in front of this gain Armor 1.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnAllyCallInFront, Action = EffectAction.GainArmor, Scope = TargetScope.TargetUnit, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Martial Musician", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 2, Power = 1, Life = 3,
                    Text = "Nearby allies have Auto-Advance.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticAutoAdvance, Scope = TargetScope.Nearby }
                    }
                },
                new CardDefinition
                {
                    Id = "Honor Guard", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 4, Power = 2, Life = 5,
                    Text = "Armor 1. Guardian.",
                    Armor = 1, Guardian = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "In This Together", Archetype = Archetype.Expedition, Type = CardType.Ability,
                    Cost = 6,
                    Text = "Your units that are adjacent to friendly units gain +1/+1.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.GainStats, Scope = TargetScope.FriendlyUnitsWithAdjacentAlly, Power = 1, Life = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Blood-Tinged Lance", Archetype = Archetype.Expedition, Type = CardType.Charm,
                    Cost = 2, Power = 0, Life = 4,
                    Text = "Equip: Bestow +2/0 and Pierce 1.",
                    IsEquip = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEquip, Action = EffectAction.GainStats, Scope = TargetScope.Self, Power = 2 },
                        new EffectDef { Trigger = Trigger.OnEquip, Action = EffectAction.GainPierce, Scope = TargetScope.Self, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Stone-Hewn Blade", Archetype = Archetype.Expedition, Type = CardType.Charm,
                    Cost = 2, Power = 0, Life = 4,
                    Text = "Equip: Bestow +1/0 and Parry 1.",
                    IsEquip = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEquip, Action = EffectAction.GainStats, Scope = TargetScope.Self, Power = 1 },
                        new EffectDef { Trigger = Trigger.OnEquip, Action = EffectAction.GainParry, Scope = TargetScope.Self, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Well-Worn Shield", Archetype = Archetype.Expedition, Type = CardType.Charm,
                    Cost = 2, Power = 0, Life = 4,
                    Text = "Armor 1. Equip: Bestow Armor 1.",
                    Armor = 1, IsEquip = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEquip, Action = EffectAction.GainArmor, Scope = TargetScope.Self, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Campfire", Archetype = Archetype.Expedition, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 2,
                    Text = "Nearby units Heal 1 at end of turn.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.EndOfTurn, Action = EffectAction.Heal, Scope = TargetScope.Nearby, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Battle Banner", Archetype = Archetype.Expedition, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 2,
                    Text = "Units in the next row have +1/0.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticStats, Scope = TargetScope.RowInFront, Power = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Marching Orders", Archetype = Archetype.Expedition, Type = CardType.Ability,
                    Cost = 2,
                    Text = "All friendly units advance.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.AdvanceAllFriendly }
                    }
                },
                new CardDefinition
                {
                    Id = "Guiding Star", Archetype = Archetype.Expedition, Type = CardType.Ability,
                    Cost = 3,
                    Text = "Your unblocked Units gain +1/0.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.GainStats, Scope = TargetScope.UnblockedFriendlyUnits, Power = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Promised Land", Archetype = Archetype.Expedition, Type = CardType.Ability,
                    Cost = 2,
                    Text = "Target space becomes Vista.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.TargetSpace, SpaceEffect = SpaceEffectType.Vista }
                    }
                },

                //---- Garden ----
                new CardDefinition
                {
                    Id = "Thorny Hedge", Archetype = Archetype.Garden, Type = CardType.Unit,
                    Cost = 1, Power = 0, Life = 3,
                    Text = "Deal 1 damage to attackers.",
                    Retaliate = 1,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "Growing Sprout", Archetype = Archetype.Garden, Type = CardType.Unit,
                    Cost = 2, Power = 1, Life = 3,
                    Text = "Gains +1/1 at end of turn if on a Verdant space.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.EndOfTurn, Action = EffectAction.GainStats, Scope = TargetScope.Self, Power = 1, Life = 1, SpaceEffect = SpaceEffectType.Verdant }
                    }
                },
                new CardDefinition
                {
                    Id = "Sower of Seeds", Archetype = Archetype.Garden, Type = CardType.Unit,
                    Cost = 3, Power = 1, Life = 2,
                    Text = "At end of turn, space becomes Verdant.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.EndOfTurn, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.Self, SpaceEffect = SpaceEffectType.Verdant }
                    }
                },
                new CardDefinition
                {
                    Id = "Rose Beast", Archetype = Archetype.Garden, Type = CardType.Unit,
                    Cost = 4, Power = 1, Life = 1,
                    Text = "Summon: Gains 1 attack per Brambled space. Gains 1 life per Verdant space.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnCall, Action = EffectAction.GainPowerPerSpaceEffect, Scope = TargetScope.Self, SpaceEffect = SpaceEffectType.Brambled },
                        new EffectDef { Trigger = Trigger.OnCall, Action = EffectAction.GainLifePerSpaceEffect, Scope = TargetScope.Self, SpaceEffect = SpaceEffectType.Verdant }
                    }
                },
                new CardDefinition
                {
                    Id = "Fate-Cursed Lover", Archetype = Archetype.Garden, Type = CardType.Unit,
                    Cost = 4, Power = 1, Life = 3,
                    Text = "Summon: This row becomes Verdant. Destroyed: This row becomes Brambled.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnCall, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.SourceRow, SpaceEffect = SpaceEffectType.Verdant },
                        new EffectDef { Trigger = Trigger.OnDestroy, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.SourceRow, SpaceEffect = SpaceEffectType.Brambled }
                    }
                },
                new CardDefinition
                {
                    Id = "Cosmic Flower", Archetype = Archetype.Garden, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 2,
                    Text = "On Destroy: Gain 1 Temporary Energy.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnDestroy, Action = EffectAction.GainEnergy, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Guest Registry", Archetype = Archetype.Garden, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 1,
                    Text = "Gains +0/1 when your opponent summons a unit.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEnemyCall, Action = EffectAction.GainStats, Scope = TargetScope.Self, Life = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Kiss From A Rose", Archetype = Archetype.Garden, Type = CardType.Ability,
                    Cost = 1,
                    Text = "Heal 3, then Bramble target space.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.Heal, Scope = TargetScope.TargetUnit, Amount = 3 },
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.TargetSpace, SpaceEffect = SpaceEffectType.Brambled }
                    }
                },
                new CardDefinition
                {
                    Id = "Sweet Nothings", Archetype = Archetype.Garden, Type = CardType.Ability,
                    Cost = 1,
                    Text = "Units in target row Heal 2. Draw a card.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.Heal, Scope = TargetScope.TargetRow, Amount = 2 },
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.Draw, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Reverie", Archetype = Archetype.Garden, Type = CardType.Ability,
                    Cost = 1,
                    Text = "Target unit is Asleep.",
                    PlayTarget = PlayTargetKind.AnyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.SetAsleep, Scope = TargetScope.TargetUnit }
                    }
                },

                new CardDefinition
                {
                    Id = "Rose Knight", Archetype = Archetype.Garden, Type = CardType.Unit,
                    Cost = 3, Power = 2, Life = 4,
                    Text = "Armor 1. Has Parry 1 while on a Brambled space.",
                    Armor = 1,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticParry, Scope = TargetScope.Self, SpaceEffect = SpaceEffectType.Brambled, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Duelist", Archetype = Archetype.Garden, Type = CardType.Unit,
                    Cost = 3, Power = 2, Life = 4,
                    Text = "Shift: Gain +2/0 and Parry until start of next turn.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnShift, Action = EffectAction.GainTempPower, Scope = TargetScope.Self, Power = 2 },
                        new EffectDef { Trigger = Trigger.OnShift, Action = EffectAction.GainTempParry, Scope = TargetScope.Self, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Dancer", Archetype = Archetype.Garden, Type = CardType.Unit,
                    Cost = 3, Power = 2, Life = 3,
                    Text = "Agile. Shift: Gain Evade 1 until start of next turn.",
                    Agile = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnShift, Action = EffectAction.GainTempEvade, Scope = TargetScope.Self, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    //Sheet prints 1/4 with "Base Atk 3"; ruled as attack stat 3
                    Id = "Windstriker", Archetype = Archetype.Garden, Type = CardType.Unit,
                    Cost = 3, Power = 3, Life = 4,
                    Text = "Agile.",
                    Agile = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "Constant Gardener", Archetype = Archetype.Garden, Type = CardType.Unit,
                    Cost = 2, Power = 2, Life = 3,
                    Text = "Has +1/0 while on a Brambled space.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticStats, Scope = TargetScope.Self, SpaceEffect = SpaceEffectType.Brambled, Power = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Magic Fertilizer", Archetype = Archetype.Garden, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 2,
                    Text = "Destroyed: This space becomes Verdant.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnDestroy, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.Self, SpaceEffect = SpaceEffectType.Verdant }
                    }
                },
                new CardDefinition
                {
                    Id = "Fertile Soil", Archetype = Archetype.Garden, Type = CardType.Ability,
                    Cost = 3,
                    Text = "Call a Cosmic Flower to each empty Verdant space on your half.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.CallUnit, CalledCardId = "Cosmic Flower", SpaceEffect = SpaceEffectType.Verdant }
                    }
                },
                new CardDefinition
                {
                    Id = "Hedge Maze", Archetype = Archetype.Garden, Type = CardType.Ability,
                    Cost = 2,
                    Text = "Call a Thorny Hedge to three random empty spaces on your half.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.CallUnit, CalledCardId = "Thorny Hedge", Amount = 3 }
                    }
                },
                new CardDefinition
                {
                    Id = "Entangling Vines", Archetype = Archetype.Garden, Type = CardType.Ability,
                    Cost = 2,
                    Text = "Bramble target space, then pull all units in its row one space toward you.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.TargetSpace, SpaceEffect = SpaceEffectType.Brambled },
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.Pull, Scope = TargetScope.TargetRow }
                    }
                },
                new CardDefinition
                {
                    Id = "Pin Down", Archetype = Archetype.Garden, Type = CardType.Ability,
                    Cost = 1,
                    Text = "Deal 1 damage to target Unit, then Pin it.",
                    PlayTarget = PlayTargetKind.AnyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.DealDamage, Scope = TargetScope.TargetUnit, Amount = 1 },
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.Pin, Scope = TargetScope.TargetUnit }
                    }
                },
                new CardDefinition
                {
                    Id = "Pin Prick", Archetype = Archetype.Garden, Type = CardType.Ability,
                    Cost = 1,
                    Text = "Deal 1 damage to target Unit, then Poison it.",
                    PlayTarget = PlayTargetKind.AnyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.DealDamage, Scope = TargetScope.TargetUnit, Amount = 1 },
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.Poison, Scope = TargetScope.TargetUnit, Amount = 1 }
                    }
                },

                new CardDefinition
                {
                    Id = "Vinewhip", Archetype = Archetype.Garden, Type = CardType.Charm,
                    Cost = 2, Power = 0, Life = 2,
                    Text = "Equip: Bestow +1/0 and Strike: Pull.",
                    IsEquip = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEquip, Action = EffectAction.GainStats, Scope = TargetScope.Self, Power = 1 },
                        new EffectDef
                        {
                            Trigger = Trigger.OnEquip, Action = EffectAction.GrantAbility, Scope = TargetScope.Self,
                            Granted = new EffectDef { Trigger = Trigger.OnAttack, Action = EffectAction.Pull, Scope = TargetScope.TargetUnit }
                        }
                    }
                },
                new CardDefinition
                {
                    Id = "Slumbering Sprout", Archetype = Archetype.Garden, Type = CardType.Unit,
                    Cost = 2, Power = 1, Life = 4,
                    Text = "Falls asleep at end of turn.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.EndOfTurn, Action = EffectAction.SetAsleep, Scope = TargetScope.Self }
                    }
                },

                new CardDefinition
                {
                    Id = "Enchanted Rose", Archetype = Archetype.Garden, Type = CardType.Charm,
                    Cost = 2, Power = 0, Life = 1,
                    Text = "Nearby units have Regen 2. When this breaks, Bramble this row.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticRegen, Scope = TargetScope.Nearby, Amount = 2 },
                        new EffectDef { Trigger = Trigger.OnDestroy, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.SourceRow, SpaceEffect = SpaceEffectType.Brambled }
                    }
                },
                new CardDefinition
                {
                    Id = "Sunlamp", Archetype = Archetype.Garden, Type = CardType.Charm,
                    Cost = 2, Power = 0, Life = 1,
                    Text = "Verdant and Bramble effects are more powerful.",
                    BoostsGardenEffects = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },

                //---- Atelier ----
                new CardDefinition
                {
                    Id = "Thunder Rod", Archetype = Archetype.Atelier, Type = CardType.Charm,
                    Cost = 6, Power = 0, Life = 6,
                    Text = "Whenever your opponent plays an Ability, deal 1 damage to every other unit and charm on the board.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEnemyAbilityPlay, Action = EffectAction.DealDamage, Scope = TargetScope.AllOtherUnits, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Amber Spyglass", Archetype = Archetype.Atelier, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 3,
                    Text = "Whenever your opponent discards a card, draw a card.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEnemyDiscard, Action = EffectAction.Draw, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Percussive Maintenance", Archetype = Archetype.Atelier, Type = CardType.Ability,
                    Cost = 2,
                    Text = "Deal 2 damage to target. Then, if it's a charm, restore it to full durability.",
                    PlayTarget = PlayTargetKind.AnyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.PercussiveMend }
                    }
                },
                new CardDefinition
                {
                    Id = "Reverse Engineer", Archetype = Archetype.Atelier, Type = CardType.Unit,
                    Cost = 3, Power = 1, Life = 4,
                    Text = "If this deals damage to an opponent's Charm, add a copy of that card to your hand.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnDealtDamage, Action = EffectAction.CopyStruckCharm }
                    }
                },
                new CardDefinition
                {
                    Id = "Crystal Amplifier", Archetype = Archetype.Atelier, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 2,
                    Text = "Abilities targeting this have power -1, but also hit adjacent spaces.",
                    Resist = 1, SplashesAdjacent = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "Attenuating Rod", Archetype = Archetype.Atelier, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 3,
                    Text = "Whenever your opponent plays an Ability, reduce the cost of a random Ability in your hand by 1.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEnemyAbilityPlay, Action = EffectAction.DiscountRandomHandAbility, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Dark Mirror", Archetype = Archetype.Atelier, Type = CardType.Charm,
                    Cost = 3, Power = 0, Life = 4,
                    Text = "Ability damage aimed at this is reflected to the mirrored space on the enemy field.",
                    Reflects = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "Adaptive Armature", Archetype = Archetype.Atelier, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 2,
                    Text = "Equip: a Charm played onto this space gains +0/2.",
                    IsEquip = true, EquipsCharms = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEquip, Action = EffectAction.GainStats, Scope = TargetScope.Self, Life = 2 }
                    }
                },
                new CardDefinition
                {
                    Id = "Sword of Damocles", Archetype = Archetype.Atelier, Type = CardType.Ability,
                    Cost = 3,
                    Text = "Delay: At the beginning of your next turn, destroy any unit on target space.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ScheduleDoom }
                    }
                },
                new CardDefinition
                {
                    Id = "Fire Sale", Archetype = Archetype.Atelier, Type = CardType.Ability,
                    Cost = 1,
                    Text = "Remove all your Charms from play. Gain a Valuable Coin for each removed Charm.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.FireSale }
                    }
                },
                new CardDefinition
                {
                    Id = "Geo", Archetype = Archetype.Atelier, Type = CardType.Unit,
                    Cost = 9, Power = 2, Life = 2,
                    Text = "Costs 1 less per space effect on your field. Call: Gain +1/+1 per space effect on your field, then clear them.",
                    CostPerOwnSpaceEffect = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnCall, Action = EffectAction.GeoAbsorb }
                    }
                },
                new CardDefinition
                {
                    Id = "Automaton", Archetype = Archetype.Atelier, Type = CardType.Unit,
                    Cost = 1, Power = 1, Life = 1,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "Diligent Student", Archetype = Archetype.Atelier, Type = CardType.Unit,
                    Cost = 1, Power = 1, Life = 1,
                    Text = "Gains +0/1 when you play an Ability.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnOwnerAbilityPlay, Action = EffectAction.GainStats, Scope = TargetScope.Self, Life = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Art Critic", Archetype = Archetype.Atelier, Type = CardType.Unit,
                    Cost = 2, Power = 1, Life = 4,
                    Text = "Ability Resist 1.",
                    Resist = 1,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "Master Painter", Archetype = Archetype.Atelier, Type = CardType.Unit,
                    Cost = 3, Power = 2, Life = 4,
                    Text = "Attacks apply space effect Primed.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnAttack, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.TargetSpace, SpaceEffect = SpaceEffectType.Primed }
                    }
                },
                new CardDefinition
                {
                    Id = "Workshop Guardian", Archetype = Archetype.Atelier, Type = CardType.Unit,
                    Cost = 4, Power = 3, Life = 6,
                    Text = "Armor 1. Resist 1.",
                    Armor = 1, Resist = 1,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "Splash of Primer", Archetype = Archetype.Atelier, Type = CardType.Ability,
                    Cost = 1,
                    Text = "Target row becomes Primed.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.TargetRow, SpaceEffect = SpaceEffectType.Primed }
                    }
                },
                new CardDefinition
                {
                    Id = "Magnifying Lance", Archetype = Archetype.Atelier, Type = CardType.Ability,
                    Cost = 2,
                    Text = "Deal 1 damage in a lane. Deal 1 more damage per unit hit.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.LaneDamage, Power = 1, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Diminishing Lance", Archetype = Archetype.Atelier, Type = CardType.Ability,
                    Cost = 2,
                    Text = "Deal 3 damage in a lane. Deal 1 less damage per unit hit.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.LaneDamage, Power = 3, Amount = -1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Sharpen Edge", Archetype = Archetype.Atelier, Type = CardType.Ability,
                    Cost = 2,
                    Text = "Target unit gains +2/0 and Pierce.",
                    PlayTarget = PlayTargetKind.AnyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.GainStats, Scope = TargetScope.TargetUnit, Power = 2 },
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.GainPierce, Scope = TargetScope.TargetUnit, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Erasure", Archetype = Archetype.Atelier, Type = CardType.Ability,
                    Cost = 3,
                    Text = "Remove effect from target space. If an effect was removed, deal 3 damage to that space.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ClearSpaceEffect, Scope = TargetScope.TargetSpace, Amount = 3 }
                    }
                },
                new CardDefinition
                {
                    Id = "Lightning Rod", Archetype = Archetype.Atelier, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 3,
                    Text = "Whenever your opponent plays an Ability, deal 1 damage to the enemy lane.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEnemyAbilityPlay, Action = EffectAction.DealDamage, Scope = TargetScope.NearestEnemyInLane, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Combat Bellows", Archetype = Archetype.Atelier, Type = CardType.Charm,
                    Cost = 2, Power = 0, Life = 5,
                    Text = "Whenever you play an Ability, Push opposing Unit.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnOwnerAbilityPlay, Action = EffectAction.PushAway, Scope = TargetScope.NearestEnemyInLane }
                    }
                },
                new CardDefinition
                {
                    Id = "Arcane Umbrella", Archetype = Archetype.Atelier, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 2,
                    Text = "Equip: Bestow Resist 1.",
                    IsEquip = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEquip, Action = EffectAction.GainResist, Scope = TargetScope.Self, Amount = 1 }
                    }
                },

                new CardDefinition
                {
                    Id = "Scholar", Archetype = Archetype.Atelier, Type = CardType.Unit,
                    Cost = 1, Power = 1, Life = 2,
                    Text = "The first time you use an Ability each turn, it costs 1 less.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticAbilityDiscount, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Quick Sketch", Archetype = Archetype.Atelier, Type = CardType.Ability,
                    Cost = 1,
                    Text = "Draw a card that costs 2 or less. If it's a Unit, Call it.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.TutorLowCost, Amount = 2 }
                    }
                },
                new CardDefinition
                {
                    Id = "Ailing Scholar", Archetype = Archetype.Atelier, Type = CardType.Unit,
                    Cost = 4, Power = 0, Life = 4,
                    Text = "At end of turn, add a random Ability to your hand and this gets 0/-1.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.EndOfTurn, Action = EffectAction.AddRandomAbility },
                        new EffectDef { Trigger = Trigger.EndOfTurn, Action = EffectAction.GainStats, Scope = TargetScope.Self, Life = -1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Trace", Archetype = Archetype.Atelier, Type = CardType.Ability,
                    Cost = 2,
                    Text = "Add a copy of the last card played to your hand.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.CopyLastCard }
                    }
                },
                new CardDefinition
                {
                    //Sheet prints no cost; ruled 2
                    Id = "Dense Lecture", Archetype = Archetype.Atelier, Type = CardType.Ability,
                    Cost = 2,
                    Text = "Units in target column gain +0/1 and fall asleep.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.GainStats, Scope = TargetScope.TargetLane, Life = 1 },
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.SetAsleep, Scope = TargetScope.TargetLane }
                    }
                },
                new CardDefinition
                {
                    Id = "Geo Rod", Archetype = Archetype.Atelier, Type = CardType.Charm,
                    Cost = 2, Power = 0, Life = 1,
                    Text = "When a space effect is applied to your field, this gets 0/+1.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnOwnerSpaceEffect, Action = EffectAction.GainStats, Scope = TargetScope.Self, Life = 1 }
                    }
                },

                //---- Tower ----
                new CardDefinition
                {
                    Id = "Seeker of Redemption", Archetype = Archetype.Tower, Type = CardType.Unit,
                    Cost = 4, Power = 0, Life = 6,
                    Text = "Guardian. Gain +1/+1 when Guardian activates.",
                    Guardian = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnGuard, Action = EffectAction.GainStats, Scope = TargetScope.Self, Power = 1, Life = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Prideful Soul", Archetype = Archetype.Tower, Type = CardType.Unit,
                    Cost = 4, Power = 4, Life = 1,
                    Text = "Rush. Transforms into a Guilt-Wracked Soul when it defeats another Unit.",
                    Rush = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnKill, Action = EffectAction.TransformSelf, CalledCardId = "Guilt-Wracked Soul" }
                    }
                },
                new CardDefinition
                {
                    Id = "Guilt-Wracked Soul", Archetype = Archetype.Tower, Type = CardType.Unit,
                    Cost = 4, Power = 1, Life = 6,
                    Text = "Transforms into a Prideful Soul when it takes damage.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnDamaged, Action = EffectAction.TransformSelf, CalledCardId = "Prideful Soul" }
                    }
                },
                new CardDefinition
                {
                    Id = "Wretch", Archetype = Archetype.Tower, Type = CardType.Unit,
                    Cost = 0, Power = 1, Life = 1,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "Snarling Hound", Archetype = Archetype.Tower, Type = CardType.Unit,
                    Cost = 2, Power = 2, Life = 2,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "Thrower of Stones", Archetype = Archetype.Tower, Type = CardType.Unit,
                    Cost = 2, Power = 2, Life = 3,
                    Text = "Ranged.",
                    Ranged = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "Overworked Laborer", Archetype = Archetype.Tower, Type = CardType.Unit,
                    Cost = 3, Power = 2, Life = 6,
                    Text = "Falls asleep at end of turn.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.EndOfTurn, Action = EffectAction.SetAsleep, Scope = TargetScope.Self }
                    }
                },
                new CardDefinition
                {
                    Id = "Tombstone", Archetype = Archetype.Tower, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 4,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "Forlorn Whisper", Archetype = Archetype.Tower, Type = CardType.Ability,
                    Cost = 0,
                    Text = "Discard a card, then draw a card.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.DiscardRandom, Amount = 1 },
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.Draw, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Keeper of Debts", Archetype = Archetype.Tower, Type = CardType.Unit,
                    Cost = 2, Power = 1, Life = 3,
                    Text = "Gains +1/+1 when you discard a card.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnOwnerDiscard, Action = EffectAction.GainStats, Scope = TargetScope.Self, Power = 1, Life = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Mourner's Altar", Archetype = Archetype.Tower, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 3,
                    Text = "Each time a friendly unit is defeated, gain 1 life.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnFriendlyDestroyed, Action = EffectAction.GainPlayerLife, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Font of Sorrows", Archetype = Archetype.Tower, Type = CardType.Charm,
                    Cost = 2, Power = 0, Life = 1,
                    Text = "Gain 1 life when a nearby friendly unit is destroyed.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnFriendlyDestroyedNearby, Action = EffectAction.GainPlayerLife, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Seer's Guillotine", Archetype = Archetype.Tower, Type = CardType.Charm,
                    Cost = 2, Power = 0, Life = 3,
                    Text = "Each time a player draws a card, they take 1 damage.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnAnyDraw, Action = EffectAction.DamageDrawingPlayer, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Grotesque Mirror", Archetype = Archetype.Tower, Type = CardType.Charm,
                    Cost = 2, Power = 0, Life = 2,
                    Text = "At the start of your opponent's turn, they discard a card at random if this is unblocked.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEnemyTurnStart, Action = EffectAction.EnemyDiscardRandom, Condition = EffectCondition.Unblocked, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Dinner Bell", Archetype = Archetype.Tower, Type = CardType.Charm,
                    Cost = 3, Power = 0, Life = 3,
                    Text = "When a nearby friendly unit is defeated, call a Snarling Hound to its space and this takes 1 damage. Activate (1): Call a Snarling Hound to a random nearby space.",
                    ActivateCost = 1,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnFriendlyDestroyedNearby, Action = EffectAction.CallUnitAtTarget, CalledCardId = "Snarling Hound" },
                        new EffectDef { Trigger = Trigger.OnFriendlyDestroyedNearby, Action = EffectAction.DealDamage, Scope = TargetScope.Self, Amount = 1 },
                        new EffectDef { Trigger = Trigger.OnActivate, Action = EffectAction.CallUnitNearby, CalledCardId = "Snarling Hound" }
                    }
                },
                new CardDefinition
                {
                    Id = "Iron Shackles", Archetype = Archetype.Tower, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 1,
                    Text = "Equip: Bestow Armor 1 and Heavy.",
                    IsEquip = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEquip, Action = EffectAction.GainArmor, Scope = TargetScope.Self, Amount = 1 },
                        new EffectDef { Trigger = Trigger.OnEquip, Action = EffectAction.GrantHeavy, Scope = TargetScope.Self }
                    }
                },

                new CardDefinition
                {
                    Id = "Trailblazer", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 2, Power = 2, Life = 1,
                    Text = "Auto-Advance. Call cost behind this unit is reduced by 1.",
                    AutoAdvance = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticCallDiscountBehind, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Flagbearer", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 2, Power = 1, Life = 3,
                    Text = "Auto-Advance. Call cost behind this unit is reduced by 1.",
                    AutoAdvance = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticCallDiscountBehind, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Virtuous Call", Archetype = Archetype.Expedition, Type = CardType.Ability,
                    Cost = 0,
                    Text = "The next Unit you Call costs 1 less and gains +1/+1.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.BlessNextCall, Amount = 1, Power = 1, Life = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Valorous Call", Archetype = Archetype.Expedition, Type = CardType.Ability,
                    Cost = 1,
                    Text = "The next unit you call gains On Attack: gain +1/+1.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef
                        {
                            Trigger = Trigger.OnPlay, Action = EffectAction.BlessNextCall,
                            Granted = new EffectDef { Trigger = Trigger.OnAttack, Action = EffectAction.GainStats, Scope = TargetScope.Self, Power = 1, Life = 1 }
                        }
                    }
                },
                new CardDefinition
                {
                    Id = "Give Your All", Archetype = Archetype.Expedition, Type = CardType.Ability,
                    Cost = 5,
                    Text = "Units in your front line gain +2/0, Overpower, and die at end of turn.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.GiveYourAll }
                    }
                },
                new CardDefinition
                {
                    Id = "Break Through", Archetype = Archetype.Expedition, Type = CardType.Ability,
                    Cost = 1,
                    Text = "Units in target lane have Push until end of turn.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.TempPushLane }
                    }
                },
                new CardDefinition
                {
                    Id = "Superior Tempo", Archetype = Archetype.Expedition, Type = CardType.Ability,
                    Cost = 4,
                    Text = "Trigger an attack from all friendly units who have moved this turn.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.AttackWithMoved }
                    }
                },
                new CardDefinition
                {
                    Id = "Mountaineer", Archetype = Archetype.Expedition, Type = CardType.Unit,
                    Cost = 3, Power = 2, Life = 4,
                    Text = "On Advance: Space becomes Rugged. Has Armor 1 on Rugged spaces.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnAdvance, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.Self, SpaceEffect = SpaceEffectType.Rugged },
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticArmor, Scope = TargetScope.Self, SpaceEffect = SpaceEffectType.Rugged, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Covenant of Valor", Archetype = Archetype.Expedition, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 3,
                    Text = "When one of your units destroys an enemy unit, restore its Defense and give it +1/0.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnFriendlyKill, Action = EffectAction.HealFull, Scope = TargetScope.TargetUnit },
                        new EffectDef { Trigger = Trigger.OnFriendlyKill, Action = EffectAction.GainStats, Scope = TargetScope.TargetUnit, Power = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "War Drum", Archetype = Archetype.Expedition, Type = CardType.Charm,
                    Cost = 2, Power = 0, Life = 3,
                    Text = "Whenever a friendly unit is defeated, units behind it Advance.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnFriendlyDestroyed, Action = EffectAction.AdvanceBehindTarget }
                    }
                },

                //---- Heart ----
                new CardDefinition
                {
                    Id = "Scorch", Archetype = Archetype.Heart, Type = CardType.Ability,
                    Cost = 2,
                    Text = "Deal 2 damage to target space. If it had a Space Effect, the space becomes Scorched.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ScorchSpace, Scope = TargetScope.TargetSpace }
                    }
                },
                new CardDefinition
                {
                    Id = "Ash Eater", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 4, Power = 2, Life = 4,
                    Text = "On Destroy: This lane becomes Scorched.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnDestroy, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.TargetLane, SpaceEffect = SpaceEffectType.Scorched }
                    }
                },
                new CardDefinition
                {
                    Id = "Lash Out", Archetype = Archetype.Heart, Type = CardType.Ability,
                    Cost = 3,
                    Text = "Deal damage to all spaces in front of target friendly unit equal to its Life, then destroy it.",
                    PlayTarget = PlayTargetKind.FriendlyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.LashOut }
                    }
                },
                new CardDefinition
                {
                    Id = "Ritual of Ascendance", Archetype = Archetype.Heart, Type = CardType.Ability,
                    Cost = 2,
                    Text = "Destroy target friendly non-spirit unit and gain energy equal to its cost. Draw a random spirit card.",
                    PlayTarget = PlayTargetKind.FriendlyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.SacrificeForEnergy }
                    }
                },
                new CardDefinition
                {
                    Id = "Ritual of Renewal", Archetype = Archetype.Heart, Type = CardType.Ability,
                    Cost = 2,
                    Text = "Destroy target friendly unit and then re-call it. Give it +1/0 and Regen 1.",
                    PlayTarget = PlayTargetKind.FriendlyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.RebirthTarget }
                    }
                },
                new CardDefinition
                {
                    Id = "Ritual of Reckoning", Archetype = Archetype.Heart, Type = CardType.Ability,
                    Cost = 6,
                    Text = "Friendly units in target column each deal their life to every unit ahead of them in the lane, then are destroyed.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ReckoningColumn }
                    }
                },
                new CardDefinition
                {
                    Id = "Ritual of Reinvention", Archetype = Archetype.Heart, Type = CardType.Ability,
                    Cost = 3,
                    Text = "Destroy target unit's spirit. Draw a card; if it's a spirit, bind it to the same unit.",
                    PlayTarget = PlayTargetKind.FriendlyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ReinventSpirit }
                    }
                },
                new CardDefinition
                {
                    Id = "Soulcatcher", Archetype = Archetype.Heart, Type = CardType.Charm,
                    Cost = 3, Power = 0, Life = 3,
                    Text = "When one of your spirit bonds breaks, this takes 1 damage and the spirit returns to your hand.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnOwnerBondBreak, Action = EffectAction.ReclaimSpirit },
                        new EffectDef { Trigger = Trigger.OnOwnerBondBreak, Action = EffectAction.DealDamage, Scope = TargetScope.Self, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Spirit Caller", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 4, Power = 2, Life = 3,
                    Text = "When this unit's bond breaks, bind a new copy of the spirit at the end of the turn.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnOwnBondBreak, Action = EffectAction.ScheduleRebind }
                    }
                },
                new CardDefinition
                {
                    Id = "Spirit Fencer", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 3, Power = 2, Life = 3,
                    Text = "While bonded, has Parry 1 and Regen 1.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticParry, Scope = TargetScope.Self, Condition = EffectCondition.WhileBonded, Amount = 1 },
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticRegen, Scope = TargetScope.Self, Condition = EffectCondition.WhileBonded, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Spirit of Renewal", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 1, Power = 0, Life = 2, IsSpirit = true,
                    Text = "Bond: The host gains +0/1 at end of turn.",
                    PlayTarget = PlayTargetKind.FriendlyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.EndOfTurn, Action = EffectAction.GainStats, Scope = TargetScope.Self, Life = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Spirit of Ferocity", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 3, Power = 2, Life = 3, IsSpirit = true,
                    Text = "Bond: When the host deals damage, it gains +1/0.",
                    PlayTarget = PlayTargetKind.FriendlyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnDealtDamage, Action = EffectAction.GainStats, Scope = TargetScope.Self, Power = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Spirit of Perspective", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 3, Power = 0, Life = 2, IsSpirit = true,
                    Text = "Spirit Bind: Flying.",
                    Flying = true,
                    PlayTarget = PlayTargetKind.FriendlyUnit
                },
                new CardDefinition
                {
                    Id = "Spirit of Reticence", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 3, Power = 1, Life = 4, IsSpirit = true,
                    Text = "Spirit Bind: Heavy.",
                    Heavy = true,
                    PlayTarget = PlayTargetKind.FriendlyUnit
                },
                new CardDefinition
                {
                    Id = "Spirit of Opportunity", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 2, Power = 0, Life = 3, IsSpirit = true,
                    Text = "Spirit Bind: The host deals double damage if unblocked.",
                    DoubleWhenUnblocked = true,
                    PlayTarget = PlayTargetKind.FriendlyUnit
                },
                new CardDefinition
                {
                    Id = "Spirit of Vulnerability", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 1, Power = 0, Life = 3, IsSpirit = true,
                    Text = "Spirit Bind: Ability Amp 1.",
                    Amplify = 1,
                    PlayTarget = PlayTargetKind.FriendlyUnit
                },
                new CardDefinition
                {
                    Id = "Spirit of Reprisal", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 1, Power = 1, Life = 1, IsSpirit = true,
                    Text = "Bond Break: Deal 1 damage to the source of the break.",
                    PlayTarget = PlayTargetKind.FriendlyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnBondBreak, Action = EffectAction.DamageBreaker, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Dead Man Walking", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 1, Power = 1, Life = 3,
                    Text = "Loses 1 life at start of your turn. On Destroy: Draw a card.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnCall, Action = EffectAction.Poison, Scope = TargetScope.Self, Amount = 1 },
                        new EffectDef { Trigger = Trigger.OnDestroy, Action = EffectAction.Draw, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Proximity Bomb", Archetype = Archetype.Heart, Type = CardType.Charm,
                    Cost = 2, Power = 0, Life = 2,
                    Text = "Destroyed: Deal 2 damage to this lane.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnDestroy, Action = EffectAction.LaneDamage, Power = 2, Amount = 0 }
                    }
                },
                new CardDefinition
                {
                    Id = "Burn", Archetype = Archetype.Heart, Type = CardType.Ability,
                    Cost = 1,
                    Text = "Deal 1 damage to target space and apply Inferno effect.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.DealDamage, Scope = TargetScope.TargetUnit, Amount = 1 },
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.TargetSpace, SpaceEffect = SpaceEffectType.Inferno }
                    }
                },
                new CardDefinition
                {
                    Id = "Fire-spitter", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 3, Power = 2, Life = 3,
                    Text = "On Damaged: The space in front of this ignites and its occupant takes 1 damage.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnDamaged, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.InFront, SpaceEffect = SpaceEffectType.Inferno },
                        new EffectDef { Trigger = Trigger.OnDamaged, Action = EffectAction.DealDamage, Scope = TargetScope.InFront, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Release Energy", Archetype = Archetype.Heart, Type = CardType.Ability,
                    Cost = 4,
                    Text = "Destroy target friendly Unit. Deal damage to enemies in its lane equal to its life.",
                    PlayTarget = PlayTargetKind.FriendlyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.SacrificeLaneBurst }
                    }
                },
                new CardDefinition
                {
                    Id = "Novice Attuner", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 2, Power = 1, Life = 3,
                    Text = "On Bond: Gain 0/+1.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnBonded, Action = EffectAction.GainStats, Scope = TargetScope.Self, Life = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Adept Attuner", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 4, Power = 2, Life = 4,
                    Text = "On Bond: Gain 0/+2.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnBonded, Action = EffectAction.GainStats, Scope = TargetScope.Self, Life = 2 }
                    }
                },
                new CardDefinition
                {
                    Id = "Dreamwalker", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 2, Power = 2, Life = 3,
                    Text = "On Destroy: Draw a random Spirit card.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnDestroy, Action = EffectAction.AddRandomSpirit }
                    }
                },
                new CardDefinition
                {
                    Id = "Spirit of Wrath", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 1, Power = 1, Life = 1, IsSpirit = true,
                    Text = "Bond Break: Deal 1 damage to enemies in this lane.",
                    PlayTarget = PlayTargetKind.FriendlyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnBondBreak, Action = EffectAction.DealDamage, Scope = TargetScope.EnemiesInLane, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Spirit of Tenacity", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 2, Power = 2, Life = 3, IsSpirit = true,
                    Text = "Bond Break: This unit gains +0/2.",
                    PlayTarget = PlayTargetKind.FriendlyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnBondBreak, Action = EffectAction.GainStats, Scope = TargetScope.Self, Life = 2 }
                    }
                },
                new CardDefinition
                {
                    Id = "Spirit of Balance", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 2, Power = 1, Life = 1, IsSpirit = true,
                    Text = "Bond Break: Deal 1 damage to both players.",
                    PlayTarget = PlayTargetKind.FriendlyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnBondBreak, Action = EffectAction.DamageBothPlayers, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Spirit of Curiosity", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 2, Power = 0, Life = 2, IsSpirit = true,
                    Text = "Bond Break: Draw a card.",
                    PlayTarget = PlayTargetKind.FriendlyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnBondBreak, Action = EffectAction.Draw, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Spirit of Inferno", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 2, Power = 2, Life = 2, IsSpirit = true,
                    Text = "Bond Break: This space ignites, mirrored on the opponent's side.",
                    PlayTarget = PlayTargetKind.FriendlyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnBondBreak, Action = EffectAction.ApplySpaceEffectMirrored, SpaceEffect = SpaceEffectType.Inferno }
                    }
                },
                new CardDefinition
                {
                    Id = "Spirit of Rejection", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 2, Power = 0, Life = 3, IsSpirit = true,
                    Text = "Spirit Bind: Parry 1.",
                    Parry = 1,
                    PlayTarget = PlayTargetKind.FriendlyUnit
                },
                new CardDefinition
                {
                    Id = "Spirit of Resistance", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 1, Power = 0, Life = 4, IsSpirit = true,
                    Text = "Spirit Bind: Resist 1.",
                    Resist = 1,
                    PlayTarget = PlayTargetKind.FriendlyUnit
                },
                new CardDefinition
                {
                    Id = "Spirit of Haste", Archetype = Archetype.Heart, Type = CardType.Unit,
                    Cost = 3, Power = 0, Life = 1, IsSpirit = true,
                    Text = "Spirit Bind: Rush.",
                    Rush = true,
                    PlayTarget = PlayTargetKind.FriendlyUnit
                },

                new CardDefinition
                {
                    Id = "Valuable Coin", Archetype = Archetype.Tower, Type = CardType.Charm,
                    Cost = 0, Power = 0, Life = 1,
                    Text = "Equip: Bestow +0/2.",
                    IsEquip = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEquip, Action = EffectAction.GainStats, Scope = TargetScope.Self, Life = 2 }
                    }
                },
                new CardDefinition
                {
                    Id = "Bauble Merchant", Archetype = Archetype.Tower, Type = CardType.Unit,
                    Cost = 2, Power = 1, Life = 4,
                    Text = "When you give this a Valuable Coin, gain a random Charm that costs 2 or less.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnEquipped, Action = EffectAction.AddRandomCheapCharm, Amount = 2, CalledCardId = "Valuable Coin" }
                    }
                },
                new CardDefinition
                {
                    Id = "Lose Hope", Archetype = Archetype.Tower, Type = CardType.Ability,
                    Cost = 3,
                    Text = "Return target friendly unit and target enemy unit to their owners' hands.",
                    PlayTarget = PlayTargetKind.FriendlyUnitThenEnemyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ReturnToHand },
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ReturnToHand, UsesSecondTarget = true }
                    }
                },
                new CardDefinition
                {
                    Id = "Shatter", Archetype = Archetype.Tower, Type = CardType.Ability,
                    Cost = 3,
                    Text = "Destroy target friendly Charm. Deal damage equal to its remaining Defense to all units in that lane.",
                    PlayTarget = PlayTargetKind.AnyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ShatterCharm }
                    }
                },
                new CardDefinition
                {
                    Id = "Flagellant's Charm", Archetype = Archetype.Tower, Type = CardType.Charm,
                    Cost = 0, Power = 0, Life = 1,
                    Text = "Each time you take damage, Attune a random card from your hand.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnOwnerPlayerDamaged, Action = EffectAction.AutoAttune }
                    }
                },

                //---- Ocean ----
                new CardDefinition
                {
                    Id = "Catalytic Spike", Archetype = Archetype.Ocean, Type = CardType.Ability,
                    Cost = 3,
                    Text = "Deal 2 damage to target unit and give it a random Keyword ability.",
                    PlayTarget = PlayTargetKind.AnyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.DealDamage, Scope = TargetScope.TargetUnit, Amount = 2 },
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.GainRandomKeyword, Scope = TargetScope.TargetUnit }
                    }
                },
                new CardDefinition
                {
                    Id = "Dispersal", Archetype = Archetype.Ocean, Type = CardType.Ability,
                    Cost = 5,
                    Text = "Destroy target unit. Randomly distribute its Power and Life to nearby units.",
                    PlayTarget = PlayTargetKind.AnyUnit,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.DisperseTarget }
                    }
                },
                new CardDefinition
                {
                    Id = "Beached Whale", Archetype = Archetype.Ocean, Type = CardType.Unit,
                    Cost = 4, Power = 0, Life = 7,
                    Text = "Call: This space becomes Desert, then deal 1 damage to a random enemy per Flooded space. Immobile.",
                    Immobile = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnCall, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.Self, SpaceEffect = SpaceEffectType.Desert },
                        new EffectDef { Trigger = Trigger.OnCall, Action = EffectAction.FloodBarrage }
                    }
                },
                new CardDefinition
                {
                    Id = "Chimera", Archetype = Archetype.Ocean, Type = CardType.Unit,
                    Cost = 1, Power = 1, Life = 1,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace
                },
                new CardDefinition
                {
                    Id = "Flicker", Archetype = Archetype.Ocean, Type = CardType.Unit,
                    Cost = 1, Power = 1, Life = 2,
                    Text = "Call: Gains a random Keyword ability.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnCall, Action = EffectAction.GainRandomKeyword, Scope = TargetScope.Self }
                    }
                },
                new CardDefinition
                {
                    Id = "Floppy Fish", Archetype = Archetype.Ocean, Type = CardType.Unit,
                    Cost = 1, Power = 0, Life = 3,
                    Text = "Moves to a random adjacent space upon taking damage.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnDamaged, Action = EffectAction.MoveRandomAdjacent }
                    }
                },
                new CardDefinition
                {
                    Id = "Floppier Fish", Archetype = Archetype.Ocean, Type = CardType.Unit,
                    Cost = 3, Power = 0, Life = 5,
                    Text = "Moves to a random adjacent space upon taking damage. Destroyed: Call a Floppy Fish to this space.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnDamaged, Action = EffectAction.MoveRandomAdjacent },
                        new EffectDef { Trigger = Trigger.OnDestroy, Action = EffectAction.CallUnitAtTarget, CalledCardId = "Floppy Fish" }
                    }
                },
                new CardDefinition
                {
                    Id = "Fractal", Archetype = Archetype.Ocean, Type = CardType.Unit,
                    Cost = 2, Power = 2, Life = 2,
                    Text = "Call: Destroy an adjacent friendly unit. Gain +1/+1 and Rush if so.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnCall, Action = EffectAction.ConsumeAdjacent, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Amalgam", Archetype = Archetype.Ocean, Type = CardType.Unit,
                    Cost = 5, Power = 3, Life = 3,
                    Text = "Call: Destroy all adjacent friendly units. Gain +1/+1 and a random Keyword per unit destroyed.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnCall, Action = EffectAction.ConsumeAdjacent, Amount = 0 }
                    }
                },
                new CardDefinition
                {
                    Id = "Sand Spirit", Archetype = Archetype.Ocean, Type = CardType.Unit,
                    Cost = 1, Power = 2, Life = 2,
                    Text = "Rush. Desert-bound. Destroyed: This space becomes Desert.",
                    Rush = true, BoundTo = SpaceEffectType.Desert,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnDestroy, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.Self, SpaceEffect = SpaceEffectType.Desert }
                    }
                },
                new CardDefinition
                {
                    Id = "Dune Beast", Archetype = Archetype.Ocean, Type = CardType.Unit,
                    Cost = 2, Power = 4, Life = 3,
                    Text = "Call: This space becomes Desert. Desert-bound.",
                    BoundTo = SpaceEffectType.Desert,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnCall, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.Self, SpaceEffect = SpaceEffectType.Desert }
                    }
                },
                new CardDefinition
                {
                    Id = "Sand Shark", Archetype = Archetype.Ocean, Type = CardType.Unit,
                    Cost = 3, Power = 3, Life = 4,
                    Text = "Immune to Desert. On Move: This space becomes Desert.",
                    DesertImmune = true,
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnShift, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.Self, SpaceEffect = SpaceEffectType.Desert },
                        new EffectDef { Trigger = Trigger.OnAdvance, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.Self, SpaceEffect = SpaceEffectType.Desert },
                        new EffectDef { Trigger = Trigger.OnRetreat, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.Self, SpaceEffect = SpaceEffectType.Desert }
                    }
                },
                new CardDefinition
                {
                    Id = "Living Torrent", Archetype = Archetype.Ocean, Type = CardType.Unit,
                    Cost = 5, Power = 0, Life = 9,
                    Text = "Has +1 Attack per Flooded space. Each turn, Flood a random nearby space.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticPowerPerSpace, SpaceEffect = SpaceEffectType.Flooded, Power = 1 },
                        new EffectDef { Trigger = Trigger.StartOfTurn, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.RandomNearbySpace, SpaceEffect = SpaceEffectType.Flooded }
                    }
                },
                new CardDefinition
                {
                    Id = "Obelisk", Archetype = Archetype.Ocean, Type = CardType.Charm,
                    Cost = 3, Power = 0, Life = 5,
                    Text = "Destroyed: The space in front of this becomes Desert.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnDestroy, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.InFront, SpaceEffect = SpaceEffectType.Desert }
                    }
                },
                new CardDefinition
                {
                    Id = "Font of Spirits", Archetype = Archetype.Ocean, Type = CardType.Charm,
                    Cost = 3, Power = 0, Life = 6,
                    Text = "Creates a Flicker on a random space when damaged.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnDamaged, Action = EffectAction.CallUnit, CalledCardId = "Flicker", Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Tidal Wave", Archetype = Archetype.Ocean, Type = CardType.Ability,
                    Cost = 3,
                    Text = "Fire a projectile in each lane that deals 2 damage and floods on impact.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.LaneProjectiles, Amount = 2, SpaceEffect = SpaceEffectType.Flooded }
                    }
                },
                new CardDefinition
                {
                    Id = "Focus Form", Archetype = Archetype.Ocean, Type = CardType.Ability,
                    Cost = 0,
                    Text = "Draw a random Unit that costs 1 or less from your deck. It gains a random Keyword.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.DrawLowCostUnitWithKeyword, Amount = 1 }
                    }
                },
                new CardDefinition
                {
                    Id = "Oasis", Archetype = Archetype.Ocean, Type = CardType.Ability,
                    Cost = 3,
                    Text = "Target space becomes Flooded. All adjacent spaces become Desert.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.TargetSpace, SpaceEffect = SpaceEffectType.Flooded },
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.AdjacentToTarget, SpaceEffect = SpaceEffectType.Desert }
                    }
                },
                new CardDefinition
                {
                    Id = "Sand Storm", Archetype = Archetype.Ocean, Type = CardType.Ability,
                    Cost = 3,
                    Text = "Spaces in target column become Desert.",
                    PlayTarget = PlayTargetKind.AnySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.ApplySpaceEffect, Scope = TargetScope.TargetLane, SpaceEffect = SpaceEffectType.Desert }
                    }
                },
                new CardDefinition
                {
                    Id = "Mirage", Archetype = Archetype.Ocean, Type = CardType.Ability,
                    Cost = 3,
                    Text = "Desert and Flooded spaces switch.",
                    PlayTarget = PlayTargetKind.None,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnPlay, Action = EffectAction.SwapFloodDesert }
                    }
                },
                new CardDefinition
                {
                    Id = "Message in a Bottle", Archetype = Archetype.Ocean, Type = CardType.Charm,
                    Cost = 1, Power = 0, Life = 2,
                    Text = "Whenever one of your units is destroyed, gain a charge. Every 3 charges, draw a card.",
                    PlayTarget = PlayTargetKind.FriendlyEmptySpace,
                    Effects =
                    {
                        new EffectDef { Trigger = Trigger.OnFriendlyDestroyed, Action = EffectAction.ChargeDraw, Amount = 3 }
                    }
                }
            };

            var dict = new Dictionary<string, CardDefinition>();
            foreach (var card in list)
            {
                //Rules-v2 default AL requirement: cost - 1, unless the card overrides it
                if (card.AffinityRequirement < 0)
                    card.AffinityRequirement = System.Math.Max(0, card.Cost - 1);
                dict.Add(card.Id, card);
            }
            return dict;
        }
    }
}
