using System.Drawing;
using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using static Dndcs2.messages.DndMessages;
using Dndcs2.stats;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Dndcs2.commands.SpellsAbilities;

public class Wildshape : DndAbility
{
    public Wildshape() : 
        base(
            "Wildshape",
            new List<AbilityClassSpecieRequirement>()
            {
                ClassSpecieAbilityRequirementFactory.ClassSpecieAbilityRequirement(constants.DndClass.Druid),
            }, 
            0, 
            2, 
            .1,
            0,
            "!wildshape", 
            "Change into a dinosaur! Can only use knife, but gain bonus HP, speed, and damage until you take too much damage")
    {
        
    }

    public override bool UseAbility(CCSPlayerController player, PlayerBaseStats playerStats, List<string> arguments)
    {
        if (!playerStats.Wildshape)
        {
            playerStats.Wildshape = true;
            playerStats.WildshapeHealth = player.PlayerPawn.Value.Health + 100;
            playerStats.ChangeMaxHealth(100);
            playerStats.ChangeSpeed(.35);
            var restrictWeapons = new List<string>();
            restrictWeapons.AddRange(Dndcs2.Snipers);
            restrictWeapons.AddRange(Dndcs2.Rifles);
            restrictWeapons.AddRange(Dndcs2.MGs);
            restrictWeapons.AddRange(Dndcs2.Shotguns);
            restrictWeapons.AddRange(Dndcs2.SMGs);
            restrictWeapons.AddRange(Dndcs2.Pistols);
            restrictWeapons.AddRange(Dndcs2.Grenades);
            playerStats.RestrictWeapon(restrictWeapons);
            playerStats.OriginalModel = player.PlayerPawn.Value.CBodyComponent!.SceneNode!.GetSkeletonInstance()
                .ModelState.ModelName;
            player.PlayerPawn.Value.SetModel(
                "characters/models/nozb1/trex_noun_player_model/trex_noun_player_model.vmdl");
            player.PlayerPawn.Value.AcceptInput("SetScale", null, null, "0.75");

            MessagePlayer(player, "You have changed into a dinosaur!");

            return true;
        }
        
        MessagePlayer(player, "You are already in Wildshape!");
        return false;
    }
}