/***********************************************************
* Primary author:           Brandon Westmoreland
* Secondary author(s):        
* Date Created:           	1/27/2015
* Last Modification Date: 	2/12/2015
* Filename:               	UnitController.cpp
*
* Overview:
* 	This class acts as a high level controller for the units
*	in the game window
*
************************************************************/
using System;
using Assets.Code.Units;
using UnityEngine;
using Assets.Code.Enums;

namespace Assets.Code.GUICode
{
    public class UnitController : IGuiItemController
    {
		// Ctor creates a unit of passed type at passed location. using matrix notation
        public UnitController(float posX, float posY, UnitTypes unit)
            : base(posX,posY,unit.ToString())
        {
            
        }
    }
}
