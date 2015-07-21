using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Code.GUICode;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Assets.Code.Interfaces
{
    public class IGuiButton : IGuiItem
    {
        IGuiButton(IGuiCommand command) : base(command)
        {
            
        }
        public IGuiButton(IGuiCommand command, float x, float y) : base(command)
        {
            // Create game object from prefab
            GuiObject = (GameObject) Object.Instantiate(Resources.Load("BlankButton"));
            ButtonObject = GuiObject.GetComponent<Button>();
            Transform = GuiObject.GetComponent<RectTransform>();
            // Add destroy script to object
            if (ButtonObject.gameObject.GetComponent<DestroyGuiScript>() == null)
                ButtonObject.gameObject.AddComponent<DestroyGuiScript>();
            // Attach destroy script to object
            ButtonObject.gameObject.GetComponent<DestroyGuiScript>().Attach(ButtonObject.gameObject);

            // Set function to call when button is pressed
            ButtonObject.onClick.AddListener(Execute);

            // Set parent to UICanvas to display button
            ButtonObject.transform.parent = GameObject.Find("UICanvas").GetComponent<Transform>();
            ResetScale();

            PosX = x;
            PosY = y;
        }
        //properties to access the button position
        virtual protected float PosX
        {
            get { return Transform.localPosition.x; }
            set { Transform.localPosition.Set(value, PosY, 0); }
        }
        virtual protected float PosY
        {
            get { return Transform.localPosition.y; }
            set { Transform.localPosition.Set(PosX, value, 0); }
        }

        public float Width
        {
            get { return _width; }
            set
            {
                SetSize(Transform, new Vector2(value, Height));
                _width = value;
            }
        }

        public float _width;
        public float Height
        {
            get { return _height; }
            set
            {
                SetSize(Transform,new Vector2(Width,value));
                _height = value;
            }
        }
        public float _height;

        // reference to the Button object in unity
        public Button ButtonObject;
        public override void Delete()
        {
            ButtonObject.GetComponent<DestroyGuiScript>().DestroyGui();
        }

        public static void SetSize(RectTransform trans, Vector2 newSize)
        {
            Vector2 oldSize = trans.rect.size;
            Vector2 deltaSize = newSize - oldSize;
            trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
            trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
        }
        public static void SetWidth(RectTransform trans, float newSize)
        {
            SetSize(trans, new Vector2(newSize, trans.rect.size.y));
        }
        public static void SetHeight(RectTransform trans, float newSize)
        {
            SetSize(trans, new Vector2(trans.rect.size.x, newSize));
        }
    }
}
