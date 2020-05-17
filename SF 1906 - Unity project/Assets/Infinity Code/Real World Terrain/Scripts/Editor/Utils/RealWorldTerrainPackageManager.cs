/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using UnityEditor;

namespace InfinityCode.RealWorldTerrain
{
    public static class RealWorldTerrainPackageManager
    {
        [MenuItem("Window/Infinity Code/Real World Terrain/Packages/Playmaker Integration Kit")]
        public static void ImportPlayMakerIntegrationKit()
        {
            RealWorldTerrainEditorUtils.ImportPackage("Packages\\RWT-Playmaker-Integration-Kit.unitypackage",
                new RealWorldTerrainEditorUtils.Warning
                {
                    title = "Playmaker Integration Kit",
                    message = "You have Playmaker in your project?",
                    ok = "Yes, I have a Playmaker"
                },
                "Could not find Playmaker Integration Kit."
            );
        }

    }
}