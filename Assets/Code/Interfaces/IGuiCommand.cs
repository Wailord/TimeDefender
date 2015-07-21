using System;

namespace Assets.Code.Interfaces
{
    /// <summary>
    /// This class is the interface for the Gui command pattern.
    /// 
    /// Actions to be completed should be derived from this class and 
    /// implement the Action() function to start the action when the 
    /// Gui item is interacted with.
    /// 
    /// For multiple commands to execute, pass the last command into the 
    /// constructor.
    /// </summary>
    public abstract class IGuiCommand
    {
        protected IGuiCommand Command;

        protected IGuiCommand(IGuiCommand command = null)
        {
            Command = command;
        }

        public virtual void Action()
        {
            throw new Exception("Cannot perform an action of an abstract command.");
        }
    }
}
