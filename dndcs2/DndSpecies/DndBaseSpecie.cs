using System.Collections.ObjectModel;
using Dndcs2.events;
using Dndcs2.dtos;
using Dndcs2.Sql;
using static Dndcs2.constants.DndSpecieDescription;

namespace Dndcs2.DndSpecies;

public abstract class DndBaseSpecie : DndSpecie
{
    public List<EventCallbackFeatureContainer> DndClassSpecieEvents { get; protected set; } = new();

    protected DndBaseSpecie(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled,
        int dndSpecieId, string dndSpecieName, int dndSpecieLevelAdjustment, string dndSpecieDescription,
        Collection<DndSpecieRequirement> dndSpecieRequirements) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled, dndSpecieId, dndSpecieName,
            dndSpecieLevelAdjustment,
            dndSpecieDescription, dndSpecieRequirements)
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
                DateTime creationTime = DateTime.UtcNow;
                string author = "D&D Initial Creation";
                bool enabled = true;
                string dndSpecieName = Enum.GetName(dndSpecieEnumType, dndSpecieEnum).Replace('_', ' ');
                string dndSpecieDescription = DndSpecieDescriptions[dndSpecieEnum];
                var specieReqs = new Collection<DndSpecieRequirement>();
                var newDndSpecie = constructor[0].Invoke(new object[]
                {
                    author, 
                    creationTime, 
                    author, 
                    creationTime, 
                    enabled, 
                    dndSpecieName, 
                    dndSpecieDescription, 
                    0, //TODO: define level adjustment somewhere
                    specieReqs
                });
                CommonMethods.CreateNewDndSpecie((DndBaseSpecie) newDndSpecie);
                Dndcs2.Instance.DndSpecieLookup[dndSpecieEnum] = (DndBaseSpecie) newDndSpecie;
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
                    dndSpecieRecord.Enabled, 
                    dndSpecieRecord.DndSpecieName, 
                    dndSpecieRecord.DndSpecieDescription, 
                    dndSpecieRecord.DndSpecieLevelAdjustment,
                    specieReqs
                });
                Dndcs2.Instance.DndSpecieLookup[dndSpecieEnum] = (DndBaseSpecie) newDndSpecie;
            }
        }
    }
}