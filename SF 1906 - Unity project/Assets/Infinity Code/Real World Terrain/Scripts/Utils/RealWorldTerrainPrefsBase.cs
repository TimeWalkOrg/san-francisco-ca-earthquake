/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using UnityEngine;

#if PROCEDURAL_TOOLKIT
using ProceduralToolkit.Buildings;
#endif

namespace InfinityCode.RealWorldTerrain
{
    /// <summary>
    /// Basic settings generation of terrain.
    /// </summary>
    [Serializable]
    public class RealWorldTerrainPrefsBase
    {
        public Vector2 autoDetectElevationOffset = new Vector2(100, 100);

        /// <summary>
        /// Resolution of the base map used for rendering far patches on the terrain.
        /// </summary>
        public int baseMapResolution = 1024;

        /// <summary>
        /// Flag indicating that the zero is used as unknown value.
        /// </summary>
        public bool bingMapsUseZeroAsUnknown;

        /// <summary>
        /// Height of the floor.
        /// </summary>
        public float buildingFloorHeight = 3.5f;

        /// <summary>
        /// Index of building generator.
        /// 0 - Built-in
        /// 1 - BuildR
        /// 2 - BuildR2
        /// 3 - Instantiate prefabs
        /// </summary>
        public int buildingGenerator = 0;

        /// <summary>
        /// Range the number of floors buildings.
        /// </summary>
        public RealWorldTerrainRangeI buildingFloorLimits = new RealWorldTerrainRangeI(5, 7, 1, 50);

        /// <summary>
        /// List of buildings materials.
        /// </summary>
        public List<RealWorldTerrainBuildingMaterial> buildingMaterials;

        /// <summary>
        /// List of prefabs to instantiate
        /// </summary>
        public List<RealWorldTerrainBuildingPrefab> buildingPrefabs;

        public bool buildingSaveInResult = true;

        public bool buildingSingleRequest = true;

        /// <summary>
        /// Use colors from OSM?
        /// </summary>
        public bool buildingUseColorTags = true;

        /// <summary>
        /// Type of collider for BuildR buildings.
        /// </summary>
        public RealWorldTerrainBuildR2Collider buildRCollider = RealWorldTerrainBuildR2Collider.none;

        /// <summary>
        /// Render mode for BuildR buildings.
        /// </summary>
        public RealWorldTerrainBuildR2RenderMode buildRRenderMode = RealWorldTerrainBuildR2RenderMode.full;

        public List<RealWorldTerrainBuildR2Material> buildR2Materials;

        /// <summary>
        /// Resolution of control texture.
        /// </summary>
        public int controlTextureResolution = 512;

        /// <summary>
        /// Instance ID of BuildR generator style.
        /// </summary>
        public int customBuildRGeneratorStyle = 0;

        /// <summary>
        /// Instance ID of BuildR generator texture pack.
        /// </summary>
        public int customBuildRGeneratorTexturePack = 0;

        /// <summary>
        /// Array of BuildR presets.
        /// </summary>
        public RealWorldTerrainBuildRPresetsItem[] customBuildRPresets;

        /// <summary>
        /// Escarpment of the seabed. Greater value - steeper slope.
        /// </summary>
        public float depthSharpness = 0;

        /// <summary>
        /// The resolution of the map that controls grass and detail meshes.\n
        /// For performance reasons (to save on draw calls) the lower you set this number the better.
        /// </summary>
        public int detailResolution = 2048;

        /// <summary>
        /// Elevation provider
        /// </summary>
        public RealWorldTerrainElevationProvider elevationProvider = RealWorldTerrainElevationProvider.SRTM;

        public RealWorldTerrainElevationRange elevationRange = RealWorldTerrainElevationRange.autoDetect;

        public RealWorldTerrainElevationType elevationType = RealWorldTerrainElevationType.realWorld;

        public string[] erRoadTypes;

        public bool erGenerateConnection = true;

        /// <summary>
        /// EasyRoads3D SnapToTerrain
        /// </summary>
        public bool erSnapToTerrain = true;

        public float erWidthMultiplier = 1;

        /// <summary>
        /// The mode of generation of road types
        /// </summary>
        public RealWorldTerrainRoadTypeMode roadTypeMode = RealWorldTerrainRoadTypeMode.simple;

        /// <summary>
        /// The fixed size of terrain.\n
        /// X - Terrain Width\n
        /// Y - Terrain Height\n
        /// Z - Terrain Length
        /// </summary>
        public Vector3 fixedTerrainSize = new Vector3(500, 600, 500);

        /// <summary>
        /// The resolution of GAIA stamp
        /// </summary>
        public int gaiaStampResolution = 1024;

        /// <summary>
        /// Generate unknown underwater areas based on known data
        /// </summary>
        public bool generateUnderWater;

        /// <summary>
        /// Density of grass.
        /// </summary>
        public int grassDensity = 100;

        /// <summary>
        /// Grass engine ID.
        /// </summary>
        public string grassEngine;

        /// <summary>
        /// List of grass textures.
        /// </summary>
        public List<Texture2D> grassPrefabs;

        /// <summary>
        /// The HeightMap resolution for each Terrain.
        /// </summary>
        public int heightmapResolution = 129;

        /// <summary>
        /// Errors of SRTM should be ignored?
        /// </summary>
        public bool ignoreSRTMErrors;

        public float fixedMaxElevation = 1000;
        public float fixedMinElevation = 0;

        /// <summary>
        /// Texture type ID.
        /// </summary>
        public string mapTypeID;

        /// <summary>
        /// Texture type extra fields.
        /// </summary>
        public string mapTypeExtraFields;

        /// <summary>
        /// Type of max elevation value.
        /// </summary>
        public RealWorldTerrainMaxElevation maxElevationType = RealWorldTerrainMaxElevation.autoDetect;

        /// <summary>
        /// The maximum level of zoom, to be used for texture generation.\n
        /// 0 - Autodetect.\n
        /// 1+ - Level of zoom.
        /// </summary>
        public int maxTextureLevel;

        /// <summary>
        /// Elevation value when there is no data.
        /// </summary>
        public short nodataValue;

        /// <summary>
        /// List of points of interest.
        /// </summary>
        public List<RealWorldTerrainPOI> POI;

#if PROCEDURAL_TOOLKIT
        public FacadePlanningStrategy ptFacadePlanningStrategy;
        public FacadeConstructionStrategy ptFacadeConstructionStrategy;
        public RoofPlanningStrategy ptRoofPlanningStrategy;
        public RoofConstructionStrategy ptRoofConstructionStrategy;
#endif

        /// <summary>
        /// The order of bytes in a RAW file.
        /// </summary>
        public RealWorldTerrainByteOrder rawByteOrder = RealWorldTerrainByteOrder.Windows;

        /// <summary>
        /// Filename of RAW result
        /// </summary>
        public string rawFilename = "terrain";

        /// <summary>
        /// Height of RAW result
        /// </summary>
        public int rawHeight = 1024;

        /// <summary>
        /// Width of RAW result
        /// </summary>
        public int rawWidth = 1024;

        /// <summary>
        /// Type of RAW result
        /// </summary>
        public RealWorldTerrainRawType rawType = RealWorldTerrainRawType.RAW;

        /// <summary>
        /// Reducing the size of the texture, reduces the time texture generation and memory usage.
        /// </summary>
        public bool reduceTextures = true;

        /// <summary>
        /// Specifies the size in pixels of each individually rendered detail patch. \n
        /// A larger number reduces draw calls, but might increase triangle count since detail patches are culled on a per batch basis. \n
        /// A recommended value is 16. \n
        /// If you use a very large detail object distance and your grass is very sparse, it makes sense to increase the value.
        /// </summary>
        public int resolutionPerPatch = 16;

        /// <summary>
        /// Type of result (terrain, mesh).
        /// </summary>
        public RealWorldTerrainResultType resultType = RealWorldTerrainResultType.terrain;

        public string riverEngine;

        public Material riverMaterial;

        /// <summary>
        /// Name of road engine.
        /// </summary>
        public string roadEngine;

        /// <summary>
        /// Types of roads that will be created.
        /// </summary>
        public RealWorldTerrainRoadType roadTypes = (RealWorldTerrainRoadType)(~0);

        /// <summary>
        /// Specifies whether the projection will be determined by the size of the area.\n
        /// 0 - Real world sizes.\n
        /// 1 - Mercator sizes.\n
        /// 2 - Fixed size.
        /// </summary>
        public int sizeType = 0;

        /// <summary>
        /// The material that will be used to SplineBend roads.
        /// </summary>
        public Material splineBendMaterial;

        /// <summary>
        /// The mesh that will be used to SplineBend roads.
        /// </summary>
        public Mesh splineBendMesh;

        /// <summary>
        /// Scale of terrains.
        /// </summary>
        public Vector3 terrainScale = Vector3.one;

        /// <summary>
        /// Count of textures.
        /// </summary>
        public RealWorldTerrainVector2i textureCount = RealWorldTerrainVector2i.one;

        /// <summary>
        /// Title
        /// </summary>
        public string title;

        /// <summary>
        /// Type of texture file output
        /// </summary>
        public RealWorldTerrainTextureFileType textureFileType = RealWorldTerrainTextureFileType.jpg;

        /// <summary>
        /// Quality of file output
        /// </summary>
        public int textureFileQuality = 100;

        /// <summary>
        /// Provider of textires.
        /// </summary>
        public RealWorldTerrainTextureProvider textureProvider = RealWorldTerrainTextureProvider.virtualEarth;

        /// <summary>
        /// URL pattern of custom texture provider.
        /// </summary>
        public string textureProviderURL = "http://localhost/tiles/{zoom}/{x}/{y}";

        /// <summary>
        /// Size of texture.
        /// </summary>
        public RealWorldTerrainVector2i textureSize = new RealWorldTerrainVector2i(1024, 1024);

        /// <summary>
        /// Type of tile texture.
        /// </summary>
        public RealWorldTerrainTextureType textureType = RealWorldTerrainTextureType.satellite;

        /// <summary>
        /// Density of trees.
        /// </summary>
        public int treeDensity = 100;

        public string treeEngine;

        /// <summary>
        /// List of tree prefabs.
        /// </summary>
        public List<GameObject> treePrefabs;

        public List<int> vegetationStudioGrassTypes;
        public List<int> vegetationStudioTreeTypes;

#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
#if !VEGETATION_STUDIO_PRO
        public AwesomeTechnologies.VegetationPackage vegetationStudioPackage;
#else
        public AwesomeTechnologies.VegetationSystem.VegetationPackagePro vegetationStudioPackage;
#endif
#endif

        /// <summary>
        /// What to do with outside points for VolumeGrass?
        /// </summary>
        public RealWorldTerrainVolumeGrassOutsidePoints volumeGrassOutsidePoints;
    }
}