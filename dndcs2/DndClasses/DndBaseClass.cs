using System.Collections.ObjectModel;
using static Dndcs2.constants.DndClassDescription;
using Dndcs2.events;
using Dndcs2.dtos;
using Dndcs2.Sql;
using DndClass = Dndcs2.dtos.DndClass;


namespace Dndcs2.DndClasses;

public abstract class DndBaseClass : DndClass
{
    public List<EventCallbackFeatureContainer> DndClassSpecieEvents { get; protected set; } = new();
    
    protected DndBaseClass(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        int dndClassId, string dndClassName, string dndClassDescription, 
        Collection<DndClassRequirement> dndClassRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, dndClassId, dndClassName, dndClassDescription, 
            dndClassRequirements)
    {
        Dndcs2.Instance.Log.LogInformation($"Created class {GetType().Name}");
    }

    public static void RegisterClasses()
    {
        var dndClasses = new List<Tuple<constants.DndClass, Type>>()
        {
            new Tuple<constants.DndClass, Type>(constants.DndClass.Fighter, typeof(DndClasses.Fighter)),
            new Tuple<constants.DndClass, Type>(constants.DndClass.Rogue, typeof(DndClasses.Rogue))
        };        
        
        var dndClassEnumType = typeof(constants.DndClass);
        foreach (var dndClassEnumCombo in dndClasses)
        {
            var dndClassEnum = dndClassEnumCombo.Item1;
            var dndClassType = dndClassEnumCombo.Item2;
            var constructor = dndClassType.GetConstructors();
            
            int dndClassId = (int)dndClassEnum;
            var dndClassRecord = CommonMethods.RetrieveDndClass(dndClassId);
            
            if (dndClassRecord == null)
            {
                DateTime creationTime = DateTime.UtcNow;
                string author = "D&D Initial Creation";
                bool enabled = true;
                string dndClassName = Enum.GetName(dndClassEnumType, dndClassEnum).Replace('_', ' ');
                string dndClassDescription = DndClassDescriptions[dndClassEnum];
                var classReqs = new Collection<DndClassRequirement>();
                var newDndClass = constructor[0].Invoke(new object[]
                {
                    author, 
                    creationTime, 
                    author, 
                    creationTime, 
                    enabled, 
                    dndClassName, 
                    dndClassDescription, 
                    classReqs
                });
                CommonMethods.CreateNewDndClass((DndBaseClass) newDndClass);
                Dndcs2.Instance.DndClassLookup[dndClassEnum] = (DndBaseClass) newDndClass;
            }
            else
            {
                Collection<DndClassRequirement> classReqs = new();
                foreach (var classReq in dndClassRecord.DndClassRequirements)
                {
                    classReqs.Add(new DndClassRequirement(
                        classReq.CreatedBy,
                        classReq.CreateDate, 
                        classReq.UpdatedBy,
                        classReq.UpdatedDate,
                        classReq.Enabled, 
                        dndClassRecord.DndClassId,
                        classReq.DndRequiredClassId, 
                        classReq.DndRequiredClassLevel
                    ));
                }
                
                var newDndClass = constructor[0].Invoke(new object[]
                {
                    dndClassRecord.CreatedBy,
                    dndClassRecord.CreateDate, 
                    dndClassRecord.UpdatedBy,
                    dndClassRecord.UpdatedDate,
                    dndClassRecord.Enabled, 
                    dndClassRecord.DndClassName, 
                    dndClassRecord.DndClassDescription, 
                    classReqs
                });
                Dndcs2.Instance.DndClassLookup[dndClassEnum] = (DndBaseClass) newDndClass;
            }
        }        
    }
}
