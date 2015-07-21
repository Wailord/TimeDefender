/***********************************************************
* Primary author:           Brandon Westmoreland
* Secondary author(s):        
* Date Created:           	1/27/2015
* Last Modification Date: 	2/12/2015
* Filename:               	UnitInterface.cpp
*
* Overview:
* 	Interacts with the unity specific location and 
*	other
*
* Input:
* 	none
*
* Output:
* 	none
************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Code.GUICode
{
    public class UnitInterface : IGuiItemInterface
    {
        public UnitInterface()
        {
            DeathTimer = 1;
        }
    }
}
