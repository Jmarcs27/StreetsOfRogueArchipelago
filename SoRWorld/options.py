from dataclasses import dataclass

from Options import Choice, OptionGroup, PerGameCommonOptions, Range, Toggle, DefaultOnToggle, OptionSet

# In this file, we define the options the player can pick.

### OPTIONS FOR VICTORY ###
class MayorOnly(Toggle):
    """
    In Mayor only mode, you will win purely by becoming mayor with a single character.
    This is an option meant for new players or short Archipelago games.
    """

    # The docstring of an option is used as the description on the website and in the template yaml.

    # You'll also want to set a display name, which will determine what the option is called on the website.
    display_name = "Mayor Only"
    
class BecomeMayor(DefaultOnToggle):
    """
    Requires you to become mayor at least once.
    Only used if "Mayor Only" is disabled.
    """

    # The docstring of an option is used as the description on the website and in the template yaml.

    # You'll also want to set a display name, which will determine what the option is called on the website.
    display_name = "Become Mayor"

class BigQuestsCompleted(Range):
    """
    How many big quests to compelete.
    Only used if "Mayor Only" is disabled.
    """

    display_name = "Big Quests Completed"

    range_start = 0
    range_end = 32
    default = 5
    
class AchievementsCompleted(Range):
    """
    How many achievements to completed.
    Only used if "Mayor Only" is disabled.
    """

    display_name = "Achievements Completed"

    range_start = 0
    range_end = 52
    default = 15
    
### GAMEPLAY OPTIONS ###
    
class DlcEnabled(Toggle):
    """
    Enabled DLC in streets of rogue. Requires DLC purchase (well worth it!)
    """
    
    display_name = "DLC Enabled"

class RandomCharacters(Range):
    """
    How many random characters do you want to start with?
    Only used if "Gorilla Mode" is disabled.
    """

    display_name = "Random Characters"

    range_start = 0
    range_end = 10

    # Range options must define an explicit default value.
    default = 1

class StartingCharacters(OptionSet):
    """
    Which characters must you start with? I recommend none, but a new player may want an easier character to start with, such as Vampire or Cannibal.
    These are in addition to how many random characters you picked.
    If you chose 2 random characters and select Alien and Vampire, you will start with Alien and Vampire plus two random characters.
    Only used if "Gorilla Mode" is disabled.
    """

    display_name = "Starting Characters"

    valid_keys = {
        "Assassin",
        "Athlete",
        "Bartender",
        "Businessman",
        "Cannibal",
        "Comedian",
        "Cop",
        "Doctor",
        "Gangbanger",
        "GangbangerB",
        "Gorilla",
        "Hacker",
        "Hobo",
        "Scientist",
        "ShapeShifter",
        "Shopkeeper",
        "Slavemaster",
        "Soldier",
        "Thief",
        "Vampire",
        "Werewolf",
        "Wrestler",
        "Zombie",
        "Firefighter",
        "Mafia",
        "RobotPlayer",
        "Bouncer",
        "Courier",
        "Guard",
        "Demolitionist",
        "MechPilot",
        "Alien"
    }
    
    default = set(("Hobo",))
    
    

# We must now define a dataclass inheriting from PerGameCommonOptions that we put all our options in.
# This is in the format "option_name_in_snake_case: OptionClassName".
@dataclass
class SoROptions(PerGameCommonOptions):
    mayor_only: MayorOnly
    become_mayor: BecomeMayor
    big_quests_completed: BigQuestsCompleted
    achievements_completed: AchievementsCompleted
    random_characters: RandomCharacters
    starting_characters: StartingCharacters
    dlc_enabled: DlcEnabled


# If we want to group our options by similar type, we can do so as well. This looks nice on the website.
option_groups = [
    OptionGroup(
        "Victory Options",
        [MayorOnly, BecomeMayor, BigQuestsCompleted, AchievementsCompleted],
    ),
    OptionGroup(
        "Gameplay Options",
        [RandomCharacters, StartingCharacters, DlcEnabled],
    ),
]

# Finally, we can define some option presets if we want the player to be able to quickly choose a specific "mode".
option_presets = {
    "One Gorilla, One Mayor": {
        "mayor_only": True,
        "become_mayor": True,
        "big_quests_completed": 0,
        "random_characters": 0,
        "dlc_enaled": False
    },
}