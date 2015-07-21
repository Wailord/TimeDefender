using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Code.GUICode
{
    public abstract class IGuiItemInterface : MonoBehaviour
    {
        private float _posX;
        private float _posY;
        public GameObject Unit;

        // Used for checking if location changed between updates.
        private float _lastX;
        private float _lastY;
        private int _deathTimer = 1;

        // TODO: fix magic numbers for scaling. unknown how to get dynamically. 
        public const float X_MIN_BOUND = -182.5f;
        public const float Y_MIN_BOUND = -182.5f;
        public const float POSITION_SCALE = 18.25f;
        public const float SIZE_SCALE = 28.6f;


        public int DeathTimer
        {
            get { return _deathTimer; }
            set { _deathTimer = value; }
        }

        public float PosX
        {
            get { return _posX; }
            set { _posX = value; }
        }

        public float PosY
        {
            get { return _posY; }
            set { _posY = value; }
        }

        // unity update
        protected virtual void Update()
        {
            UpdatePosition();
        }

        // set the unit to proper location
        protected virtual void UpdatePosition()
        {
            // Only do computation if position has changed.
            if (Math.Abs(PosX - _lastX) > 0.001 || Math.Abs(PosY - _lastY) > 0.001)
            {
                // get x and y
                float X = (X_MIN_BOUND + (PosX * POSITION_SCALE));
                float Y = -(Y_MIN_BOUND + (PosY * POSITION_SCALE));

                // use local position for UI Canvas scaling
                Unit.transform.localPosition = new Vector3(X, Y);
                Unit.transform.localScale = new Vector3(SIZE_SCALE, SIZE_SCALE, 1);

                _lastX = PosX;
                _lastY = PosY;
            }
        }

        public virtual void StartDeathAnimation()
        {
            Unit.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .5f);
        }

        protected virtual IEnumerator DeathActions()
        {
            StartDeathAnimation();
            //Debug.Log("Dead. Doing animation");
            yield return new WaitForSeconds(DeathTimer);
            //Debug.Log("Removing unit");
            GameObject.Destroy(Unit);
        }

        public void Kill()
        {
            StartCoroutine(DeathActions());
        }
    }
}
