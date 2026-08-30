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
                    //Sheet also gives it "Parry 1 on a Bramble space" - deferred until Parry exists
                    Id = "Rose Knight", Archetype = Archetype.Garden, Type = CardType.Unit,
                    Cost = 3, Power = 2, Life = 4,
                    Text = "Armor 1.",
                    Armor = 1,
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

                //---- Atelier ----
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
