/***********************************************************
* Primary author:           Luke Thompson
* Filename:               	SkillTree.cs
*
* Overview:
* 	This is a class for managing player skill tree upgrades.
*   Any attempts to add points to a skill will run checks to
*   see whether the attempt is valid. each function returns
*   "true" in the event that skill points are successfully
*   added, and false otherwise.
*   
*   The tree looks something like this:
*          
*  6--7     0     4--3   
*   \   \   |   /   /
*     9   8   1   5
*       \ |   | /   
*         A   2    
*          \ /
*          B C
* 
*   symbols are based on the skills enum below
*
************************************************************/
using System;

namespace Assets.Code.Skilltree
{
    //enum for each of the skills
    public enum skill 
    {
        critical = 0,   //Modifies critical attack chance
        armor,          //Decreases enemy resistances
        movement,       //Decreases enemy movement speed
        attack,         //Increases the base attack of all towers
        rate_of_fire,   //Increases the base rate of fire of all towers
        range,          //Increases the base range of all towers
        laser_cost,     //Unlocks the laser tower, then decreases its cost
        gun_upgrade,    //Modifies gun tower upgrading
        gun_cost,       //Unlocks the gun tower, then decreases its cost
        archer_upgrade, //Modifies archer tower upgrading
        archer_cost,    //Unlocks the archer tower, then decreases its cost
        hut_upgrade,    //Modifies hut tower upgrading
        money           //Modifies passive money gain and/or money from kills
    }               

    //Player_Skill_Tree singleton wrapper
    public class SkillTree
    {
        private static Player_Skill_Tree instance = null;

		private SkillTree(){}

		public static Player_Skill_Tree getInstance()
		{
			if (instance == null) {
				instance = new Player_Skill_Tree ();
			}
			return instance;
		}
    }

    //Player_Skill_tree class
    public class Player_Skill_Tree
    {
        int skill_points = 0;
        int[] level = new int[13];
        float[] modifier = new float[13];
        bool[] unlocked = new bool[11];
        bool completed = false;

        public Player_Skill_Tree()
        {
            for(int i = 0; i < 13; ++i)
            {
                level[i] = 0;
                modifier[i] = 0;
                if (i < 11)
                    unlocked[i] = false;
            }
        }

        public void AddSkillPoint()
        {
            ++skill_points;
        }

        public int GetSkillPoints()
        {
            return skill_points;
        }

        //Base skill functions
        public bool IncHutUpgrade()
        {
            return IncBaseNode(
                (int)skill.hut_upgrade,
                (int)skill.archer_cost,
                1,
                1);
        }

        public bool IncMoney()
        {
            return IncBaseNode(
                (int)skill.money,
                (int)skill.movement,
                3,
                1);
        }

        //2nd tier skill functions
        public bool IncArcherCost()
        {
            return IncTwoWayNode(
                (int)skill.archer_cost,
                (int)skill.archer_upgrade,
                (int)skill.gun_cost,
                10,
                3);
        }

        public bool IncMovement()
        {
            return IncTwoWayNode(
                (int)skill.movement,
                (int)skill.range,
                (int)skill.armor,
                3,
                1);
        }

        //Upgrades with parallel unlock dependencies and two connections
        public bool IncArcherUpgrade()
        {
            return IncTwoPathParallelNode(
                (int)skill.archer_upgrade,
                (int)skill.gun_upgrade,
                (int)skill.laser_cost,
                1,
                1);
        }

        public bool IncGunUpgrade()
        {
            return IncTwoPathParallelNode(
                (int)skill.gun_upgrade,
                (int)skill.archer_upgrade,
                (int)skill.laser_cost,
                1,
                1);
        }

        public bool IncRange()
        {
            return IncTwoPathParallelNode(
                (int)skill.range,
                (int)skill.rate_of_fire,
                (int)skill.attack,
                1,
                1);
        }

        public bool IncRateOfFire()
        {
            return IncTwoPathParallelNode(
                (int)skill.rate_of_fire,
                (int)skill.range,
                (int)skill.attack,
                .1f,
                .1f);
        }

        //Upgrades with parallel unlock dependencies and three connections
        public bool IncGunCost()
        {
            return IncThreePathParallelNode(
                (int)skill.gun_cost,
                (int)skill.armor,
                (int)skill.gun_upgrade,
                (int)skill.critical,
                15,
                3);
        }

        public bool IncArmor()
        {
            return IncThreePathParallelNode(
                (int)skill.armor,
                (int)skill.gun_cost,
                (int)skill.rate_of_fire,
                (int)skill.critical,
                15,
                3);
        }

        //End skill functions
        public bool IncLaserCost()
        {
            return IncEndNode(
                (int)skill.laser_cost,
                15,
                3);
        }

        public bool IncAttack()
        {
            return IncEndNode(
                (int)skill.attack,
                25 - level[(int)skill.attack],
                5);
        }

        public bool IncCritical()
        {
            return IncEndNode(
                (int)skill.critical,
                .01f,
                .005f);
        }

        //Node Increase functions

        //IncBaseNode(node, path, mod1, mod2)
        //
        // This function is used to increase skills which have no unlock
        // requirements and lead to one new skill.
        //
        // node - skill to be increased
        // path - skill to be unlocked
        // mod1 - value by which to adjust the modifier before completing the tree
        // mod2 - value for modifier adjustment after completing the tree
        //
        public bool IncBaseNode(int node, int path, float mod1, float mod2)
        {
            //Check if the skill is below max level and there are available skill points
            if (skill_points > 0 && level[node] < 5)
            {
                if (!unlocked[path] && level[node] > 0)
                    unlocked[path] = true;

                modifier[node] += mod1;
                ++level[node];
                --skill_points;
                return true;
            }
            else if (completed)
            {
                modifier[node] += mod2;
                ++level[node];
                --skill_points;
                return true;
            }
            return false;
        }

        //IncTwoWayNode(node, path1, path2, mod1, mod2)
        //
        // This function is used to increase skills which have one unlock requirement
        // and lead to two new skills
        //
        // node - skill to be increased
        // path1 - skill to be unlocked
        // path2 - other skill to be unlocked
        // mod1 - value by which to adjust the modifier before completing the tree
        // mod2 - value for modifier adjustment after completing the tree
        //
        public bool IncTwoWayNode(int node, int path1, int path2, float mod1, float mod2)
        {
            if (unlocked[node] && skill_points > 0 && level[node] < 5)
            {
                if (level[node] > 0)
                {
                    unlocked[path1] = true;
                    unlocked[path2] = true;
                }

                modifier[node] += mod1;
                ++level[node];
                --skill_points;
                return true;
            }
            else if (completed)
            {
                modifier[node] += mod2;
                ++level[node];
                --skill_points;
                return true;
            }
            return false;
        }

        //IncTwoPathParallelNode(node, par_node, path, mod1, mod2)
        //
        // This function is used to increase skills which have one unlock requirement
        // and lead to one unlock that is also dependent on another skill
        //
        // node - skill to be increased
        // par_node - the other skill on which progression is dependent
        // path - skill to be unlocked
        // mod1 - value by which to adjust the modifier before completing the tree
        // mod2 - value for modifier adjustment after completing the tree
        //
        public bool IncTwoPathParallelNode(int node, int par_node, int path, float mod1, float mod2)
        {
            if (unlocked[node] && skill_points > 0 && level[node] < 5)
            {
                if (!unlocked[path] && level[par_node] > 1 && level[node] > 0)
                    unlocked[path] = true;

                modifier[node] += mod1;
                ++level[node];
                --skill_points;
                return true;
            }
            else if (completed)
            {
                modifier[node] += mod2;
                ++level[node];
                --skill_points;
                return true;
            }
            return false;
        }

        //IncThreePathParallelNode(node, par_node, path1, path2, mod1, mod2)
        //
        // This function is used to increase skills which have one unlock requirement
        // and lead to two unlockable skills, one of which is also dependent on 
        // another skill
        //
        // node - skill to be increased
        // par_node - the other skill on which progression is dependent
        // path1 - skill that can be unlocked through upgrading the current skill
        // path2 - unlockable that is dependent on another skill
        // mod1 - value by which to adjust the modifier before completing the tree
        // mod2 - value for modifier adjustment after completing the tree
        //
        public bool IncThreePathParallelNode(int node, int par_node, int path1, int path2, float mod1, float mod2)
        {
            if (unlocked[node] && skill_points > 0 && level[node] < 5)
            {
                if (!unlocked[path2] && level[node] > 0)
                {
                    unlocked[path1] = true;
                    if (level[par_node] > 1)
                        unlocked[path2] = true;
                }

                modifier[node] += mod1;
                ++level[node];
                --skill_points;
                return true;
            }
            else if (completed)
            {
                modifier[node] += mod2;
                ++level[node];
                --skill_points;
                return true;
            }
            return false;
        }

        //IncEndNode(node, mod1, mod2)
        //
        // This function is used to increase skills which have two unlock
        // requirements and lead to no further skill unlocks.
        // 
        // If the skill becomes maxed, a completion check will be performed.
        // Once all skills are confirmed to be at max, the level cap is removed.
        //
        // node - skill to be increased
        // par_node - the other skill on which progression is dependent
        // path - skill to be unlocked
        // mod1 - value by which to adjust the modifier before completing the tree
        // mod2 - value for modifier adjustment after completing the tree
        //
        public bool IncEndNode(int node, float mod1, float mod2)
        {
            if (unlocked[node] && skill_points > 0 && level[node] < 5)
            {
                modifier[node] += mod1;
                ++level[node];
                --skill_points;
                if(level[node] == 5)
                    CompletionCheck();
                return true;
            }
            else if (completed)
            {
                modifier[node] += mod2;
                ++level[node];
                --skill_points;
                return true;
            }
            return false;
        }

        //Completed()
        //
        // Checks to see whether the skill tree has been completely filled out,
        // which will allow players to continue upgrading maxed out skills beyond
        // the maximum level.
        public bool CompletionCheck()
        {
            if(!completed)
            {
                int total_level = 0;

                for(int i = 0; i < 13; i++)
                {
                    total_level += level[i];
                }

                if(total_level >= 65) // 5 pts times 13 skills == 65
                {
                    completed = true;
                }
            }
            return completed;
        }
    }
}
