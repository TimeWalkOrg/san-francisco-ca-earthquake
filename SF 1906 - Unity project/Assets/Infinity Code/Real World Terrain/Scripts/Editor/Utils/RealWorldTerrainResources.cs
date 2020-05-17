/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public static class RealWorldTerrainResources
    {
        private static Texture2D _deleteIcon;
        private static Texture2D _finishIcon;
        private static Texture2D _helpIcon;
        private static GUIStyle _helpStyle;
        private static Texture2D _openIcon;
        private static Texture2D _saveIcon;
        private static Texture2D _rawIcon;
        private static Texture2D _refreshIcon;
        private static Texture2D _utilsIcon;
        private static Texture2D _resizeIcon;
        private static Texture2D _warningIcon;
        private static Texture2D _wizardIcon;

        public static Texture2D deleteIcon
        {
            get
            {
                if (_deleteIcon == null) _deleteIcon = GetIcon("TrashIcon");
                return _deleteIcon;
            }
        }

        public static Texture2D finishIcon
        {
            get
            {
                if (_finishIcon == null) _finishIcon = GetIcon("FinishIcon");
                return _finishIcon;
            }
        }

        public static Texture2D helpIcon
        {
            get
            {
                if (_helpIcon == null) _helpIcon = GetIcon("HelpIcon");
                return _helpIcon;
            }
        }

        public static GUIStyle helpStyle
        {
            get
            {
                if (_helpStyle == null)
                {
                    _helpStyle = new GUIStyle();
                    _helpStyle.margin = new RectOffset(0, 0, 2, 0);
                }
                return _helpStyle;
            }
        }

        public static Texture2D openIcon
        {
            get
            {
                if (_openIcon == null) _openIcon = GetIcon("OpenIcon");
                return _openIcon;
            }
        }

        public static Texture2D saveIcon
        {
            get
            {
                if (_saveIcon == null) _saveIcon = GetIcon("SaveIcon");
                return _saveIcon;
            }
        }

        public static Texture2D rawIcon
        {
            get
            {
                if (_rawIcon == null) _rawIcon = GetIcon("RawIcon");
                return _rawIcon;
            }
        }

        public static Texture2D refreshIcon
        {
            get
            {
                if (_refreshIcon == null) _refreshIcon = GetIcon("RWT");
                return _refreshIcon;
            }
        }

        public static Texture2D utilsIcon
        {
            get
            {
                if (_utilsIcon == null) _utilsIcon = GetIcon("UtilsIcon");
                return _utilsIcon;
            }
        }

        public static Texture2D resizeIcon
        {
            get
            {
                if (_resizeIcon == null) _resizeIcon = GetIcon("ResizeIcon");
                return _resizeIcon;
            }
        }

        public static Texture2D warningIcon
        {
            get
            {
                if (_warningIcon == null) _warningIcon = GetIcon("WarningIcon");
                return _warningIcon;
            }
        }

        public static Texture2D wizardIcon
        {
            get
            {
                if (_wizardIcon == null) _wizardIcon = GetIcon("WizardIcon");
                return _wizardIcon;
            }
        }

        public static Texture2D GetIcon(string name)
        {
            return AssetDatabase.LoadAssetAtPath(RealWorldTerrainEditorUtils.assetPath + "Icons/" + name + ".png", typeof(Texture2D)) as Texture2D;
        }
    }

}