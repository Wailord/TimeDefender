using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.GUICode
{
    public static class GuiStatics
    {
        #region Fields
        private static float _minBound = -182.5f;
        private static float _posScaleFactor = 18.25f;
        private static float _sizeScaleFactor = 28.6f;

        #endregion

        #region Properties

        public static float MinBound
        {
            get { return _minBound; }
            set { _minBound = value; }
        }

        public static float PosScaleFactor
        {
            get { return _posScaleFactor; }
            set { _posScaleFactor = value; }
        }

        public static float SizeScaleFactor
        {
            get { return _sizeScaleFactor; }
            set { _sizeScaleFactor = value; }
        }
        #endregion
    }
}
