using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.GUICode
{
    public class HudInterface : MonoBehaviour
    {

        public GameObject MoneyText;
        public GameObject LivesText;
        public GameObject WaveText;
        public GameObject ScoreText;
        public GameObject NextText;
        public GameObject TimeText;

        // Use this for initialization
        void Start ()
        {
            GuiAPI.HUD = this; // set the current HUD to the one created in the scene.
        }
	
        // Update is called once per frame
        void Update () {
	
        }
    }
}
