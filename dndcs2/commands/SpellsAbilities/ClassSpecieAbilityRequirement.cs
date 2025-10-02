using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using Dndcs2.Sql;

namespace Dndcs2.commands.SpellsAbilities;

public static class ClassSpecieAbilityRequirementFactory
{
    private static Dictionary<int, Dictionary<int, AbilityClassSpecieRequirement>> _classRequirements = new();
    private static Dictionary<int, Dictionary<int, AbilityClassSpecieRequirement>> _specieRequirements = new();
    private static Dictionary<Tuple<int, int>, Dictionary<Tuple<int, int>, AbilityClassSpecieRequirement>> _classSpecieRequirements = new();
    
    // Null checks in the AbilityClassSpecieRequirement allow to avoid database hits, all classes & Species start at level 1

    public static AbilityClassSpecieRequirement ClassSpecieAbilityRequirement(DndClass dndClass, int levelRequirement = 1)
    {
        var classId = (int)dndClass;
        if (!_classRequirements.ContainsKey(classId))
            _classRequirements[classId] = new Dictionary<int, AbilityClassSpecieRequirement>();
        if (!_classRequirements[classId].ContainsKey(levelRequirement))
        {
            var requirement = new AbilityClassSpecieRequirement(dndClass, levelRequirement == 1 ? null : levelRequirement, null, null);
            _classRequirements[classId][levelRequirement] = requirement;
        }
        
        return _classRequirements[classId][levelRequirement];
    }
    
    public static AbilityClassSpecieRequirement ClassSpecieAbilityRequirement(DndSpecie dndSpecie, int levelRequirement = 1)
    {
        var specieId = (int)dndSpecie;
        if (!_specieRequirements.ContainsKey(specieId))
            _specieRequirements[specieId] = new Dictionary<int, AbilityClassSpecieRequirement>();
        if (!_specieRequirements[specieId].ContainsKey(levelRequirement))
        {
            var requirement = new AbilityClassSpecieRequirement(null, null, dndSpecie, levelRequirement == 1 ? null : levelRequirement);
            _specieRequirements[specieId][levelRequirement] = requirement;
        }
        
        return _specieRequirements[specieId][levelRequirement];
    }
    
    public static AbilityClassSpecieRequirement ClassSpecieAbilityRequirement(DndClass dndClass, DndSpecie dndSpecie, int classLevelRequirement = 1, int specieLevelRequirement = 1)
    {
        var classSpecieKey = new Tuple<int, int>((int) dndClass, (int)dndSpecie);
        var levelKey =  new Tuple<int, int>(classLevelRequirement, specieLevelRequirement);
        
        if (!_classSpecieRequirements.ContainsKey(classSpecieKey))
            _classSpecieRequirements[classSpecieKey] = new Dictionary<Tuple<int, int>, AbilityClassSpecieRequirement>();
        if (!_classSpecieRequirements[classSpecieKey].ContainsKey(levelKey))
        {
            var requirement = new AbilityClassSpecieRequirement(
                dndClass, classLevelRequirement == 1 ? null : classLevelRequirement, 
                dndSpecie, specieLevelRequirement == 1 ? null : specieLevelRequirement);
            _classSpecieRequirements[classSpecieKey][levelKey] = requirement;
        }
        
        return _classSpecieRequirements[classSpecieKey][levelKey];
    }
    
}

public class AbilityClassSpecieRequirement
{
    public constants.DndClass? DndClass;
    public int? ClassLevel;
    public constants.DndSpecie? DndSpecie;
    public int? SpecieLevel;
    
    public AbilityClassSpecieRequirement(constants.DndClass? dndClass, int? classLevel, constants.DndSpecie? dndSpecie,
        int? specieLevel)
    {
        DndClass = dndClass;
        ClassLevel = classLevel;
        DndSpecie = dndSpecie;
        SpecieLevel = specieLevel;
    }

    public bool PlayerMeetsRequirements(CCSPlayerController player)
    {
        var dndPlayer = CommonMethods.RetrievePlayer(player);
        
        if (DndClass != null)
        {
            if(dndPlayer.DndClassId != (int)DndClass)
                return false;
            if(ClassLevel != null)
                if (CommonMethods.RetrievePlayerClassLevel(player) < ClassLevel)
                    return false;
        }
        
        if (DndSpecie != null)
        {
            if(dndPlayer.DndSpecieId != (int)DndSpecie)
                return false;
            if(SpecieLevel != null)
                if (CommonMethods.RetrievePlayerSpecieLevel(player) < ClassLevel)
                    return false;
        }

        return true;
    }
}