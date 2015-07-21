using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Code.Interfaces;

namespace Assets.Code.GUICode
{
    class CreateTowerButton : IGuiButton
    {
        private MapTileController _tile;
        public CreateTowerButton(IGuiCommand command, MapTileController tile)
            : base(command,tile.PosX,tile.PosY)
        {

        }

        public override void Execute()
        {
            GuiAPI.CreateTower(_tile.PosX, _tile.PosY);
        }
    }
}
