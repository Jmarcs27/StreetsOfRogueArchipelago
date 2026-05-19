from __future__ import annotations

from typing import TYPE_CHECKING

from BaseClasses import ItemClassification, Location, Region

from . import items

if TYPE_CHECKING:
    from .world import SoRWorld

# Every location must have a unique integer ID associated with it.
# We will have a lookup from location name to ID here that, in world.py, we will import and bind to the world class.
# Even if a location doesn't exist on specific options, it must be present in this lookup.

bqs= [
    "Assassin_BQ",
    "Athlete_BQ",
    "Bartender_BQ",
    "Businessman_BQ",
    "Cannibal_BQ",
    "Comedian_BQ",
    "Cop_BQ",
    "Doctor_BQ",
    "Gangbanger_BQ",
    "GangbangerB_BQ",
    "Gorilla_BQ",
    "Hacker_BQ",
    "Hobo_BQ",
    "Scientist_BQ",
    "ShapeShifter_BQ",
    "Shopkeeper_BQ",
    "Slavemaster_BQ",
    "Soldier_BQ",
    "Thief_BQ",
    "Vampire_BQ",
    "Werewolf_BQ",
    "Wrestler_BQ",
    "Zombie_BQ",
    "Firefighter_BQ",
    "Mafia_BQ",
    "RobotPlayer_BQ"
]
dlc_bqs =  [
    "Bouncer_BQ",
    "Courier_BQ",
    "Guard_BQ",
    "Demolitionist_BQ",
    "MechPilot_BQ",
    "Alien_BQ"
]
achievements =[
    "CompleteTutorial",
    "CompleteAnyLevel",
    "CompleteLevel1",
    "CompleteLevel2",
    "CompleteLevel3",
    "CompleteLevel4",
    "CompleteLevel5",
    "CompleteLevel6",
    "WinElection",
    "KillMayor",
    "NonViolentWin",
    "BadEnding",
    "CompleteAnyBigQuest",
    "Floor2",
    "Floor3",
    "Floor4",
    "Floor5",
    "KilledKillerRobot",
    "FoundAlien",
    "RefrigeratorRun",
    "KillVampireAsWerewolf",
    "FallOffLevelEdge",
    "CreateCustomCharacter",
    "PutResurrectionInWater",
    "HaveFourStatusEffects",
    "EnslaveSlavemaster",
    "WinArenaFight",
    "KillEveryoneInLevel",
    "AskNPCLeaveLevel",
    "KillWithGravestone",
    "GiveCyanideCocktail",
    "ElectrocuteInWater",
    "Assassin",
    "Athlete",
    "Bartender",
    "Businessman",
    "Cannibal",
    "Comedian",
    "Cop",
    "GangbangerB",
    "Gorilla",
    "Scientist",
    "ShapeShifter",
    "Shopkeeper",
    "Slavemaster",
    "Vampire",
    "Werewolf",
    "Wrestler",
    "Zombie",
    "Firefighter",
    "Mafia",
    "RobotPlayer"
]


LOCATION_NAME_TO_ID = {}
count = 1
for bq in bqs:
    LOCATION_NAME_TO_ID[bq] = count
    count+=1
for dbq in dlc_bqs:
    LOCATION_NAME_TO_ID[dbq] = count
    count+=1
for achievement in achievements:
    LOCATION_NAME_TO_ID[achievement] = count
    count+=1

# Each Location instance must correctly report the "game" it belongs to.
# To make this simple, it is common practice to subclass the basic Location class and override the "game" field.
class SoRLocation(Location):
    game = "Streets of Rogue"

# Let's make one more helper method before we begin actually creating locations.
# Later on in the code, we'll want specific subsections of LOCATION_NAME_TO_ID.
# To reduce the chance of copy-paste errors writing something like {"Chest": LOCATION_NAME_TO_ID["Chest"]},
# let's make a helper method that takes a list of location names and returns them as a dict with their IDs.
# Note: There is a minor typing quirk here. Some functions want location addresses to be an "int | None",
# so while our function here only ever returns dict[str, int], we annotate it as dict[str, int | None].
def get_location_names_with_ids(location_names: list[str]) -> dict[str, int | None]:
    return {location_name: LOCATION_NAME_TO_ID[location_name] for location_name in location_names}


def create_all_locations(world: SoRWorld) -> None:
    create_regular_locations(world)

def create_regular_locations(world: SoRWorld) -> None:
    # Finally, we need to put the Locations ("checks") into their regions.
    # Once again, before we do anything, we can grab our regions we created by using world.get_region()
    addRegion = Region("Overworld", world.player, world.multiworld)
    world.multiworld.regions += [ addRegion ]
    
    homebase = world.get_region("Overworld")

    # A simpler way to do this is by using the region.add_locations helper.
    # For this, you need to have a dict of location names to their IDs (i.e. a subset of location_name_to_id)
    # Aha! So that's why we made that "get_location_names_with_ids" helper method earlier.
    # You also need to pass your overridden Location class.
    all_bq = get_location_names_with_ids (bqs)
    homebase.add_locations (all_bq, SoRLocation)
    
    all_achievements = get_location_names_with_ids (achievements)
    homebase.add_locations (all_achievements, SoRLocation)
    
    if world.options.dlc_enabled:
        all_dbq = get_location_names_with_ids (dlc_bqs)
        homebase.add_locations (all_dbq, SoRLocation)