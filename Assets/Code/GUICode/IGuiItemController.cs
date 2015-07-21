using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Code.GUICode
{
    public abstract class IGuiItemController
    {
        
        #region Fields
		protected GameObject _unit;
        protected IGuiItemInterface _interface;
        #endregion

        #region Properties
		// unity game object to control
        public GameObject Unit
        {
            get { return _unit; }
            set { _unit = value; }
        }
		
		// unity level calculations interface
        public IGuiItemInterface Interface
        {
            get { return _interface; }
            set { _interface = value; }
        }

        public int DeathTimer
        {
            get
            {
                if(Interface != null)
                    return Interface.DeathTimer;
                else
                    return 0;
            }
            set
            {
                if(Interface != null)
                    Interface.DeathTimer = value;
            }
        }

        #endregion

		// Ctor creates a unit of passed type at passed location. using matrix notation
        protected IGuiItemController(float posX, float posY, string prefabName)
        {
            // create unit from resources
            Unit = (GameObject)UnityEngine.Object.Instantiate(Resources.Load(prefabName));

            // Make the new sprite a child of the UI Canvas
            Unit.transform.parent = GameObject.Find("MapCanvas").GetComponent<Transform>();

            // Get the Interface with the new sprite
            Interface = Unit.GetComponent<IGuiItemInterface>();

			// move unit to appropriate location
            SetLocation(posX, posY);
        }

		// sets the location using the interface
        public void SetLocation(float posX , float posY )
        {
            Interface.PosX = posX;
            Interface.PosY = posY;
        }

        public void Kill()
        {
            Interface.Kill();
        }
    }
}
