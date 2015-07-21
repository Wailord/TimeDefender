
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Code.Interfaces
{
    /// <summary>
    /// This class is the abstract base class for any GUI items in the game
    /// </summary>
    public abstract class IGuiItem
    {
        // The command to be done when the button is interacted with.
        protected IGuiCommand Command;
        protected GameObject GuiObject;

        protected IGuiItem(IGuiCommand command)
        {
            Command = command;
        }

        protected IGuiItem()
        {
            
        }

        protected RectTransform Transform { get; set; }

        public void ResetScale()
        {
            if (Transform)
            {
                Transform.localScale = new Vector3(1, 1, 1);
            }
        }

        /// <summary>
        /// This function executes the action of the stored command.
        /// </summary>
        public virtual void Execute()
        {
            if(Command != null)
                Command.Action();
        }


        /// <summary>
        /// This function removes the GUI item from the screen.
        /// </summary>
        public abstract void Delete();
    }
}
