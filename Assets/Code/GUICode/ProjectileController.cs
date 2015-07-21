using System;
using Assets.Code.Units;
using Assets.Code.Enums;
using UnityEngine;

namespace Assets.Code.GUICode
{
    public class ProjectileController : IGuiItemController
    {
        public ProjectileController(float posX, float posY, ProjectileTypes projectile)
            : base(posX,posY,projectile.ToString())
        {
        }

        public void SetLocation(Vector2 newPos)
        {
            base.SetLocation(newPos.x,newPos.y);
        }
    }
}
