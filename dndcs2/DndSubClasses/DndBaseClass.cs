using System.Collections.ObjectModel;
using static Dndcs2.constants.DndSubClassDescription;
using Dndcs2.events;
using Dndcs2.Sql;
using DndSubClass = Dndcs2.dtos.DndSubClass;



namespace Dndcs2.DndClasses;

public abstract class DndBaseSubClass : DndSubClass
{
    public List<EventCallbackFeatureContainer> DndClassSpecieEvents { get; protected set; } = new();
    
    protected DndBaseSubClass(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        constants.DndSubClass dndSubClass, constants.DndClass parentClass, int dndParentClassLevelRequirementId = 3) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, dndSubClass, parentClass, dndParentClassLevelRequirementId)
    {
        Dndcs2.Instance.Log.LogInformation($"Created class {GetType().Name}");

        var parentClassObj = Dndcs2.Instance.DndClassLookup[parentClass];
        DndClassSpecieEvents = parentClassObj.DndClassSpecieEvents;
    }

    public static void RegisterSubClasses()
    {
        var dndClasses = new List<Tuple<constants.DndSubClass, Type>>()
        {
            new(constants.DndSubClass.Champion, typeof(Champion)),
            new(constants.DndSubClass.Battle_Master, typeof(BattleMaster)),
            new(constants.DndSubClass.Eldritch_Knight, typeof(EldritchKnight)),
        };        
        
        var dndClassEnumType = typeof(constants.DndClass);
        foreach (var dndClassEnumCombo in dndClasses)
        {
            var dndSubClassEnum = dndClassEnumCombo.Item1;
            var dndSubClassType = dndClassEnumCombo.Item2;
            var constructor = dndSubClassType.GetConstructors();
            
            int dndSubClassId = (int)dndSubClassEnum;
            var dndSubClassRecord = CommonMethods.RetrieveDndSubClass(dndSubClassId);
            
            if (dndSubClassRecord == null)
            {
                DateTime creationTime = DateTime.UtcNow;
                string author = "D&D Initial Creation";
                bool enabled = true;
                try
                {
                    var newDndClass = constructor[0].Invoke(new object[]
                    {
                        author,
                        creationTime,
                        author,
                        creationTime,
                        enabled
                    });
                    CommonMethods.CreateNewDndSubClass((DndBaseSubClass) newDndClass);
                    Dndcs2.Instance.DndSubClassLookup[dndSubClassEnum] = (DndBaseSubClass) newDndClass;
                    Dndcs2.Instance.Log.LogInformation($"{dndSubClassEnum.ToString()} added to database");
                }
                catch (Exception e)
                {
                    Dndcs2.Instance.Log.LogError($"Error registering class {dndSubClassEnum}");
                    Dndcs2.Instance.Log.LogError(e.ToString());
                    return;
                }
            }
            else
            {                
                var newDndSubClass = constructor[0].Invoke(new object[]
                {
                    dndSubClassRecord.CreatedBy,
                    dndSubClassRecord.CreateDate, 
                    dndSubClassRecord.UpdatedBy,
                    dndSubClassRecord.UpdatedDate,
                    dndSubClassRecord.Enabled                    
                });
                Dndcs2.Instance.DndSubClassLookup[dndSubClassEnum] = (DndBaseSubClass) newDndSubClass;
                Dndcs2.Instance.Log.LogInformation($"{((DndBaseSubClass) newDndSubClass).DndSubClassName} loaded from database");
            }
        }        
    }    
}
