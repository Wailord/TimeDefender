using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Code.Interfaces;
using UnityEngine;

namespace Assets.Code.GUICode
{
    /// <summary>
    /// This button destroys itself after being pressed.
    /// </summary>
    public class VolitileGuiButton : IGuiTextButton
    {
        public VolitileGuiButton(IGuiCommand command, float x, float y, float width, float height, String text)
            : base(command, x,y, text)
        {

        }

        public override void Execute()
        {
            base.Execute();
            Delete();
            Debug.Log("Button removed.");
        }
    }
}
