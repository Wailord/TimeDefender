using System;
using Assets.Code.GUICode;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Assets.Code.Interfaces
{
    /// <summary>
    /// This class is the base class for all text based buttons.
    /// </summary>
    public abstract class IGuiTextButton : IGuiButton
    {

        protected String ButtonText
        {
            get { return ButtonObject.GetComponent<Text>().text; }
            set { ButtonObject.GetComponent<Text>().text = value; }
        }


        protected IGuiTextButton(IGuiCommand command, float x, float y, String text) : base(command,x,y)
        {
            ButtonText = text;
        }

        /// <summary>
        /// Removes the button object from unity
        /// </summary>
        public override void Delete()
        {
            ButtonObject.GetComponent<DestroyGuiScript>().DestroyGui();
        }
    }
}
