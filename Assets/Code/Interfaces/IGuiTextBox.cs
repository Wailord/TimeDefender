using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Object = UnityEngine.Object;
using Assets.Code.GUICode;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.Interfaces
{
    public enum TextBoxType {UpdateWhole, UpdateLeft, UpdateRight};

    public abstract class IGuiTextBox : IGuiItem
    {
        TextBoxType type;

        public string Name { get; set; }

        protected float PosX
        {
            get { return TextObject.transform.position.x; }
            set { } //TextObject.transform.position = new Vector3(value, PosY, 0); }
        }
        protected float PosY
        {
            get { return TextObject.transform.position.y; }
            set { }//TextObject.transform.position = new Vector3(PosX, value, 0); }
        }

        public String Text
        {
            get { return DisplayText.text; }
            set { DisplayText.text = value; }
        }

        public string BaseText
        {
            get { return _baseText; }
        }

        public GameObject TextObject;
        private String _baseText;
        public Text DisplayText;

        /// <summary>
        /// Updates the text to be the old base plus the new addition
        /// </summary>
        /// <param name="text">The text to be added to the textbox</param>
        public void UpdateText(String text)
        {
            if (type == TextBoxType.UpdateRight)
            {
                DisplayText.text = _baseText + text;
            }                
            else if (type == TextBoxType.UpdateWhole)
            {
                _baseText = text;
                DisplayText.text = _baseText;
            }
            else if (type == TextBoxType.UpdateLeft)
            {
                DisplayText.text = text + _baseText;
            }                
        }

        /// <summary>
        /// Creates a new GameObject with a GUIText component attached
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x">The position on the x-axis it will be drawn</param>
        /// <param name="y">The position on the y-axis it will be drawn</param>
        /// <param name="text">The base message</param>
        /// <param name="dynamic">The text that can be updated</param>
        /// <param name="gameObject"></param>
        /// <param name="panel"></param>
        /// <param name="nType"></param>
        public IGuiTextBox(string name,String text, String dynamic, TextBoxType nType, GameObject gameObject, string panel)
        {
            type = nType;
            Name = name;

            _baseText = text;
            TextObject = gameObject;
            DisplayText = TextObject.GetComponent<Text>();
            //Debug.Log(String.Format("Display text: {0}",DisplayText));

            DisplayText.text = _baseText + dynamic;

            // Add destroy script to object
            if (TextObject.GetComponent<DestroyGuiScript>() == null)
                TextObject.AddComponent<DestroyGuiScript>();
            // Attach destroy script to object
            TextObject.GetComponent<DestroyGuiScript>().Attach(TextObject);
            // Set parent to UICanvas to display text
            TextObject.transform.parent = GameObject.Find(panel).GetComponent<Transform>();
            TextObject.transform.localScale = new Vector3(1,1,1);
            //ResetScale();

            // section removed because text boxes are not initially set at a location.
            /*var refRez = GameObject.Find(panel).GetComponent<RectTransform>().rect;

            float xPos = (x * refRez.width);
            float yPos = -(y * refRez.height) + refRez.height/2;
            TextObject.transform.localPosition = new Vector3(xPos, yPos, 0);
            DisplayText.rectTransform.localPosition.Set(xPos,yPos,0);
            //Debug.Log(String.Format("Text created at: {0},{1}", DisplayText.rectTransform.localPosition.x, DisplayText.rectTransform.localPosition.y));
            */
        }

        /// <summary>
        /// Destroyes the TextObject
        /// </summary>
        public override void Delete()
        {
            TextObject.GetComponent<DestroyGuiScript>().DestroyGui();
        }
    }
}
