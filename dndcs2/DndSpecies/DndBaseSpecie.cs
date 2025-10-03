using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.commands.SpellsAbilities;
using Dndcs2.constants;
using Dndcs2.events;
using Dndcs2.dtos;
using Dndcs2.Sql;
using Dndcs2.stats;
using static Dndcs2.messages.DndMessages;
using DndSpecie = Dndcs2.dtos.DndSpecie;

namespace Dndcs2.DndSpecies;

public abstract class DndBaseSpecie : DndSpecie
{
    public List<EventCallbackFeatureContainer> DndClassSpecieEvents { get; protected set; } = new();
    private static bool _spawnEventRegistered = false;

    protected DndBaseSpecie(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled,
        constants.DndSpecie specie, int dndSpecieLevelAdjustment,
        Collection<DndSpecieRequirement> dndSpecieRequirements) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled, specie, dndSpecieLevelAdjustment,
            dndSpecieRequirements)
    {
        Dndcs2.Instance.Log.LogInformation($"Created Specie {GetType().Name}");

        if (!_spawnEventRegistered)
        {
            _spawnEventRegistered = true;
            DndClassSpecieEvents.AddRange(new List<EventCallbackFeatureContainer>()
            {
                new AllSpeciesSpawn()
            });
        }
    }

    public static void RegisterSpecies()
    {
        var dndSpecies = new List<Tuple<constants.DndSpecie, Type>>()
        {
            new Tuple<constants.DndSpecie, Type>(constants.DndSpecie.Human, typeof(DndSpecies.Human)),
            new Tuple<constants.DndSpecie, Type>(constants.DndSpecie.Dragonborn, typeof(DndSpecies.Dragonborn)),
            new Tuple<constants.DndSpecie, Type>(constants.DndSpecie.Aasimar, typeof(DndSpecies.Aasimar)),
            new Tuple<constants.DndSpecie, Type>(constants.DndSpecie.Tiefling, typeof(DndSpecies.Tiefling)),
        };

        var dndSpecieEnumType = typeof(constants.DndSpecie);
        foreach (var dndSpecieEnumTypeCombo in dndSpecies)
        {
            var dndSpecieEnum = dndSpecieEnumTypeCombo.Item1;
            var dndSpecieType = dndSpecieEnumTypeCombo.Item2;
            var constructor = dndSpecieType.GetConstructors();

            int dndSpecieId = (int)dndSpecieEnum;
            var dndSpecieRecord = CommonMethods.RetrieveDndSpecie(dndSpecieId);

            if (dndSpecieRecord == null)
            {
                try
                {
                    DateTime creationTime = DateTime.UtcNow;
                    string author = "D&D Initial Creation";
                    bool enabled = true;
                    var newDndSpecie = constructor[0].Invoke(new object[]
                    {
                        author,
                        creationTime,
                        author,
                        creationTime,
                        enabled
                    });
                    CommonMethods.CreateNewDndSpecie((DndBaseSpecie)newDndSpecie);
                    Dndcs2.Instance.DndSpecieLookup[dndSpecieEnum] = (DndBaseSpecie)newDndSpecie;
                    Dndcs2.Instance.Log.LogInformation($"{((DndBaseSpecie)newDndSpecie).DndSpecieName} added to database");
                }
                catch (Exception e)
                {
                    Dndcs2.Instance.Log.LogError($"Error registering specie {dndSpecieEnum}");
                    Dndcs2.Instance.Log.LogError(e.ToString());
                    return;
                }
            }
            else
            {
                Collection<DndSpecieRequirement> specieReqs = new();
                foreach (var specieReq in dndSpecieRecord.DndSpecieRequirements)
                {
                    specieReqs.Add(new DndSpecieRequirement(
                        specieReq.CreatedBy,
                        specieReq.CreateDate,
                        specieReq.UpdatedBy,
                        specieReq.UpdatedDate,
                        specieReq.Enabled,
                        dndSpecieRecord.DndSpecieId,
                        specieReq.DndRequiredSpecieId,
                        specieReq.DndRequiredSpecieLevel
                    ));
                }

                var newDndSpecie = constructor[0].Invoke(new object[]
                {
                    dndSpecieRecord.CreatedBy,
                    dndSpecieRecord.CreateDate,
                    dndSpecieRecord.UpdatedBy,
                    dndSpecieRecord.UpdatedDate,
                    dndSpecieRecord.Enabled
                });
                Dndcs2.Instance.DndSpecieLookup[dndSpecieEnum] = (DndBaseSpecie)newDndSpecie;
                Dndcs2.Instance.Log.LogInformation($"{((DndBaseSpecie)newDndSpecie).DndSpecieName} loaded from database");
            }
        }
    }

    public class AllSpeciesSpawn : EventCallbackFeature<EventPlayerSpawn>
    {
        public AllSpeciesSpawn() :
            base(false, EventCallbackFeaturePriority.Low, HookMode.Post, PlayerPostSpawn, null, null)
        {

        }

        public static HookResult PlayerPostSpawn(EventPlayerSpawn @event, GameEventInfo info, DndPlayer dndPlayer,
            DndPlayer? dndPlayerAttacker)
        {
            if (@event.Userid == null || @event.Userid.ControllingBot || (int) @event.Userid.Team == 0)
                return HookResult.Continue;

            var userid = (int)@event.Userid.UserId;

            Server.NextFrame(() =>
            {
                var player = Utilities.GetPlayerFromUserid(userid);
                if (player == null)
                    return;
                if (player.ControllingBot || player.Team == 0)
                    return;

                var playerStats = PlayerStats.GetPlayerStats(player);
                var dndPlayer = CommonMethods.RetrievePlayer(player);

                var playerSpecie = dndPlayer.DndSpecieId;

                foreach (var spellAbilityKvp in DndAbility.DndAbilities)
                {
                    var ability = spellAbilityKvp.Value;
                    if (!ability.CheckClassSpecieRequirements(player))
                        continue;
                    if(ability.IsCastingWithSpecie(playerStats, player))
                        MessagePlayer(player,$"You have {ability.SpecieLimitedUses} uses of {ability.CommandName} as a(n) {(constants.DndSpecie)playerSpecie} ({ability.CommandDescription})");
                    else
                    {
                        playerStats.ChangeMana(ability.ManaCost);
                        MessagePlayer(player,$"You have been given {ability.ManaCost} Mana because your class already has {ability.CommandName}");
                    }
                }
            });
            return HookResult.Continue;
        }
    }
}