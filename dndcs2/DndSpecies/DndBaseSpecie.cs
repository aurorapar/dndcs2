using System.Collections.ObjectModel;
using Dndcs2.events;
using Dndcs2.dtos;
using Dndcs2.Sql;
using static Dndcs2.constants.DndSpecieDescription;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.DndSpecies;

public abstract class DndBaseSpecie : DndSpecie
{
    public List<EventCallbackFeatureContainer> DndClassSpecieEvents { get; protected set; } = new();

    protected DndBaseSpecie(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled,
        constants.DndSpecie specie, int dndSpecieLevelAdjustment, Collection<DndSpecieRequirement> dndSpecieRequirements) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled, specie, dndSpecieLevelAdjustment, dndSpecieRequirements)
    {
        Dndcs2.Instance.Log.LogInformation($"Created Specie {GetType().Name}");
    }

    public static void RegisterSpecies()
    {
        var dndSpecies = new List<Tuple<constants.DndSpecie, Type>>()
        {
            new Tuple<constants.DndSpecie, Type>(constants.DndSpecie.Human, typeof(DndSpecies.Human)),
            new Tuple<constants.DndSpecie, Type>(constants.DndSpecie.Dragonborn, typeof(DndSpecies.Dragonborn))
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
                    Dndcs2.Instance.Log.LogInformation($"{((DndBaseSpecie) newDndSpecie).DndSpecieName} added to database");
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
                Dndcs2.Instance.DndSpecieLookup[dndSpecieEnum] = (DndBaseSpecie) newDndSpecie;
                Dndcs2.Instance.Log.LogInformation($"{((DndBaseSpecie) newDndSpecie).DndSpecieName} loaded from database");
            }
        }
    }
}