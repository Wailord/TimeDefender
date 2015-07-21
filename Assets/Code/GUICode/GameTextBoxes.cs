using System;
using Assets.Code.Interfaces;
using UnityEngine;

namespace Assets.Code.GUICode
{
    public class GameTextBoxes : IGuiTextBox
    {
        public GameTextBoxes(string name, float x, float y, String text, String dynamic, TextBoxType type,
            GameObject gameObject, string panel)
            : base(name, text, dynamic, type, gameObject,panel)
        {
            
        }


    }
}
