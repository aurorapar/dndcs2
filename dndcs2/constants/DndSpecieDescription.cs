namespace Dndcs2.constants;

public static class DndSpecieDescription
{
    public static Dictionary<DndSpecie, string> DndSpecieDescriptions = new()
    {
        { DndSpecie.Human, "An adaptable and young race, ever eager to prove themselves and mutable to any situation"},
        { DndSpecie.Dragonborn, "Born of dragons, they walk proudly through a world that greets them with fearful incomprehension."},
        { DndSpecie.Aasimar, "Mortals who carry a spark of the Upper Planes within their souls."},
        { DndSpecie.Tiefling, "A pact struck generations ago infused the essence of Asmodeus, Lord of the Nine Hells, into their bloodline"},
    };
}