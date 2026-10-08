from __future__ import annotations

from typing import TYPE_CHECKING

from rule_builder.options import OptionFilter
from rule_builder.rules import Has, HasAll, Rule

from . import items, locations

if TYPE_CHECKING:
    from .world import APQuestWorld


def set_all_rules(world: APQuestWorld) -> None:
    # In order for AP to generate an item layout that is actually possible for the player to complete,
    # we need to define rules for our Entrances and Locations.
    # Note: Regions do not have rules, the Entrances connecting them do!
    # We'll do entrances first, then locations, and then finally we set our victory condition.

    set_all_location_rules(world)

def set_all_location_rules(world: APQuestWorld) -> None:
    # Location rules work no differently from Entrance rules.
    # Most of our locations are chests that can simply be opened by walking up to them.
    # Thus, their logical requirements are covered by the Entrance rules of the Entrances that were required to
    # reach the region that the chest sits in.
    # However, our two enemies work differently.
    # Entering the room with the enemy is not enough, you also need to have enough combat items to be able to defeat it.
    # So, we need to set requirements on the Locations themselves.
    # Since combat is a bit more complicated, we'll use this chance to cover some advanced access rule concepts.

    # In "set_all_entrance_rules", we had a rule for a location that doesn't always exist.
    # In this case, we had to check for its existence (by checking the player's chosen options) before setting the rule.
    # Other times, you may have a situation where a location can have two different rules depending on the options.
    # In our case, the enemy in the right room has more health if hard mode is selected,
    # so ontop of the Sword, the player will either need one more health or a Shield in hard mode.
    # First, let's make our sword condition.
    
    # Sets the logic for big quests being behind character unlocks
    for agent in items.agents:
        world.set_rule(world.get_location(agent+"_BQ"), Has(agent))
        if(agent in locations.achievements):
            world.set_rule(world.get_location(agent), Has(agent))
    for agent in items.dlc_agents:
        if(world.options.dlc_enabled == False): break
        world.set_rule(world.get_location(agent+"_BQ"), Has(agent))
        if(agent in locations.achievements):
            world.set_rule(world.get_location(agent), Has(agent))
            
    # Sets achievement logic
    world.set_rule(world.get_location("KillVampireAsWerewolf"), Has("Werewolf"))