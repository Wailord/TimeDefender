using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Code.Interfaces;
using UnityEngine;

namespace Assets.Code.GUICode
{
    /// <summary>
    /// This command class is used to log the action of the command
    /// </summary>
    public class LogGuiCommand : IGuiCommand
    {
        public LogGuiCommand(IGuiCommand command = null)
            : base(command)
        {
            
        }

        // String the user may set to be logged.
        public String LogString { get; set; }
        public override void Action()
        {
            if (Command != null)
                Command.Action();
            Debug.Log(LogString);
        }
    }
}
