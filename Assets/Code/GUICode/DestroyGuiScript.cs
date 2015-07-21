using UnityEngine;

namespace Assets.Code.GUICode
{
    /// <summary>
    /// This class is attached to each gameobject in the unity scene controled by the business layer.
    /// 
    /// This is used to remove the game object from the scene using the business layer.
    /// </summary>
    public class DestroyGuiScript : MonoBehaviour
    {
        public GameObject GuiObject;

        /// <summary>
        /// Used to remove the game object this is attached to from the scene.
        /// </summary>
        public void DestroyGui()
        {
            Destroy(GuiObject);
        }

        /// <summary>
        /// Assigns the reference to the passed game object. 
        /// Used when a new GuiItem is created.
        /// </summary>
        /// <param name="guiGameObject"></param>
        public void Attach(GameObject guiGameObject)
        {
            GuiObject = guiGameObject;
        }

    }
}
