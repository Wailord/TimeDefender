using UnityEngine;
using System.Collections;
using Assets.Code.Interfaces;

namespace Assets.Code.Controllers
{
    public class InputController : IServiceable
    {
        public static InputController Instance
        {
            get { return _instance ?? (_instance = new InputController()); }
        }
        private static InputController _instance;

        public void Service()
        {

        }
    }
}