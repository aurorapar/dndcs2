namespace Dndcs2.constants;

public static class DndClassDescription
{
    public static Dictionary<DndClass, string> DndClassDescriptions = new()
    {
        { DndClass.Fighter, "A master of the martial arts, constantly refining their skill and honing their talents" },
        { DndClass.Rogue, "Subtle by nature, cruel through action, always effective in results" },
        { DndClass.Cleric, "Intermediaries between the mortal world and the distant planes of the gods" },
        { DndClass.Wizard, "Draws on the subtle weave of magic that permeates the cosmos to cast dazzling, and frightening, spells" },
        { DndClass.Druid, "Embodiment of nature's resilience, cunning, and fury." },
        { DndClass.Ranger, "Endlessly watch amid the dense-packed trees of trackless forests and across wide and empty plains." },
        { DndClass.Monk, "Infused by their ability to magically harness the energy that flows in their bodies." },
    };
}