////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////        EnviroSky- Renders sky with sun, moon, clouds and weather.                          ////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

[ExecuteInEditMode]
public class EnviroSkyLite : EnviroCore
{
    #region Var
    private static EnviroSkyLite _instance; // Creat a static instance for easy access!

    public static EnviroSkyLite instance
    {
        get
        {
            //If _instance hasn't been set yet, we grab it from the scene!
            //This will only happen the first time this reference is used.
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<EnviroSkyLite>();
            return _instance;
        }
    }

    public string prefabVersion = "2.2.2";

    public bool useParticleClouds = false;
    public bool usePostEffectFog = true;
    public bool showFogInEditor = true;

    //Camera Components
    [HideInInspector]public EnviroSkyRenderingLW EnviroSkyRender;

    //Materials
    [HideInInspector]public Material skyMat;

    private double lastMoonUpdate;

    //Inspector
    [HideInInspector]public bool showSettings = false;

    #endregion

    void Start()
    {
        //Check for Manager first!
        if (EnviroSkyMgr.instance == null)
        {
            Debug.Log("Please use the EnviroSky Manager!");
            gameObject.SetActive(false);
            return;
        }

        //Time
        SetTime(GameTime.Years, GameTime.Days, GameTime.Hours, GameTime.Minutes, GameTime.Seconds);
        lastHourUpdate = Mathf.RoundToInt(internalHour);
        currentTimeInHours = GetInHours(internalHour, GameTime.Days, GameTime.Years, GameTime.DaysInYear);
        Weather.weatherFullyChanged = false;
        thunder = 0f;
                     
        // Check for Profile
        if (profileLoaded)
        {
            InvokeRepeating("UpdateEnviroment", 0, qualitySettings.UpdateInterval);// Vegetation Updates
            CreateEffects("Enviro Effects LW");  //Create Weather Effects Holder

            // Instantiate Lightning Effect
            if (weatherSettings.lightningEffect != null && lightningEffect == null)
                lightningEffect = Instantiate(weatherSettings.lightningEffect, EffectsHolder.transform).GetComponent<ParticleSystem>();

            if (PlayerCamera != null && Player != null && AssignInRuntime == false && profile != null)
            {
                Init();
            }
        }

        StartCoroutine(SetSkyBoxLateAdditive());
    }

    private IEnumerator SetSkyBoxLateAdditive()
    {
        yield return 0;
        if (skyMat != null && RenderSettings.skybox != skyMat)
            SetupSkybox();
    }

    void OnEnable()
    {
        //Check for Manager first!
        if (EnviroSkyMgr.instance == null)
        {
            return;
        }


        if (Weather.zones.Count < 1)
            Weather.zones.Add(GetComponent<EnviroZone>());

        //Set Weather
        Weather.currentActiveWeatherPreset = Weather.zones[0].currentActiveZoneWeatherPreset;
        Weather.lastActiveWeatherPreset = Weather.currentActiveWeatherPreset;

        if (profile == null)
        {
            Debug.LogError("No profile assigned!");
            return;
        }

        // Auto Load profile
        if (profileLoaded == false)
            ApplyProfile(profile);

        PreInit();

        if (AssignInRuntime)
        {
            started = false;    //Wait for assignment
        }
        else if (PlayerCamera != null && Player != null)
        {
            Init();
        }
    }
    /// <summary>
    /// Re-Initilize the system.
    /// </summary>
    public void ReInit()
    {
        OnEnable();
    }
    /// <summary>
    /// Pee-Initilize the system.
    /// </summary>
    private void PreInit()
    {
        // Check time
        if (GameTime.solarTime < GameTime.dayNightSwitch)
            isNight = true;
        else
            isNight = false;

        //return when in server mode!
        if (serverMode)
            return;

        CheckSatellites();

        // Setup Fog Mode
        RenderSettings.fogMode = fogSettings.Fogmode;

        // Setup Skybox Material
        SetupSkybox();

        // Set ambient mode
        RenderSettings.ambientMode = lightSettings.ambientMode;

        // Set Fog
        //RenderSettings.fogDensity = 0f;
        //RenderSettings.fogStartDistance = 0f;
        //RenderSettings.fogEndDistance = 1000f;

        // Setup ReflectionProbe
        if (Components.GlobalReflectionProbe == null)
        {
            GameObject temp;

            foreach (Transform child in transform)
            {
                if (child.name == "GlobalReflections")
                {
                    temp = child.gameObject;
                    Components.GlobalReflectionProbe = temp.GetComponent<EnviroReflectionProbe>();
                    if (Components.GlobalReflectionProbe == null)
                        Components.GlobalReflectionProbe = temp.AddComponent<EnviroReflectionProbe>();
                }
            }
        }

        if (!Components.Sun)
        {
            Debug.LogError("Please set sun object in inspector!");
        }

        if (!Components.satellites)
        {
            Debug.LogError("Please set satellite object in inspector!");
        }

        if (Components.Moon)
        {
            MoonTransform = Components.Moon.transform;
            MoonRenderer = Components.Moon.GetComponent<Renderer>();

            if (MoonRenderer == null)
                MoonRenderer = Components.Moon.AddComponent<MeshRenderer>();

            MoonRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            MoonRenderer.receiveShadows = false;

            if (MoonRenderer.sharedMaterial != null)
                DestroyImmediate(MoonRenderer.sharedMaterial);

            if (skySettings.moonPhaseMode == EnviroSkySettings.MoonPhases.Realistic)
                MoonShader = new Material(Shader.Find("Enviro/Lite/MoonShader"));
            else
                MoonShader = new Material(Shader.Find("Enviro/Lite/MoonShaderPhased"));

            MoonShader.SetTexture("_MainTex", skySettings.moonTexture);

            MoonRenderer.sharedMaterial = MoonShader;
        }
        else { Debug.LogError("Please set moon object in inspector!"); }


        if (lightSettings.directionalLightMode == EnviroLightSettings.LightingMode.Single)
        {
            SetupMainLight();
        }
        else
        {
            SetupMainLight();
            SetupAdditionalLight();
        }

        if (Components.particleClouds)
        {
            ParticleSystem[] systems = Components.particleClouds.GetComponentsInChildren<ParticleSystem>();
            if (systems.Length > 0)
                particleClouds.layer1System = systems[0];
            if (systems.Length > 1)
                particleClouds.layer2System = systems[1];

            if (particleClouds.layer1System != null)
                particleClouds.layer1Material = particleClouds.layer1System.GetComponent<ParticleSystemRenderer>().sharedMaterial;

            if (particleClouds.layer2System != null)
                particleClouds.layer2Material = particleClouds.layer2System.GetComponent<ParticleSystemRenderer>().sharedMaterial;
        }
        else
        {
            Debug.LogError("Please set particleCLouds object in inspector!");
        }
    }

    /// <summary>
    /// Creation and assignment of skybox
    /// </summary>
    public void SetupSkybox()
    {
        if (skySettings.skyboxModeLW == EnviroSkySettings.SkyboxModiLW.Simple)
        {
            if (skyMat != null)
                DestroyImmediate(skyMat);

            skyMat = new Material(Shader.Find("Enviro/Lite/SkyboxSimple"));

            if (skySettings.starsCubeMap != null)
                skyMat.SetTexture("_Stars", skySettings.starsCubeMap);
            if (skySettings.galaxyCubeMap != null)
                skyMat.SetTexture("_Galaxy", skySettings.galaxyCubeMap);

            RenderSettings.skybox = skyMat;
        }
        else if (skySettings.skyboxMode == EnviroSkySettings.SkyboxModi.CustomSkybox)
        {
            if (skySettings.customSkyboxMaterial != null)
                RenderSettings.skybox = skySettings.customSkyboxMaterial;
        }

        //Update environment texture in next frame!
        if (lightSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Skybox)
            StartCoroutine(UpdateAmbientLightWithDelay());
    }
    /// <summary>
    /// Update the environment texture for skybox ambient mode with one frame delay. Somehow not working in same frame as we create the skybox material.
    /// </summary>
    private IEnumerator UpdateAmbientLightWithDelay()
    {
        yield return 0;
        DynamicGI.UpdateEnvironment();

    }
    /// <summary>
    /// Final Initilization and startup.
    /// </summary>
    private void Init()
    {
        if (profile == null)
            return;

        if (serverMode)
        {
            started = true;
            return;
        }

        InitImageEffects();

        // Setup Camera
        if (PlayerCamera != null)
        {

            if (setCameraClearFlags)
                PlayerCamera.clearFlags = CameraClearFlags.Skybox;

            // Workaround for deferred forve HDR...
            if (PlayerCamera.actualRenderingPath == RenderingPath.DeferredShading)
                SetCameraHDR(PlayerCamera, true);
            else
                SetCameraHDR(PlayerCamera, HDR);

            Components.GlobalReflectionProbe.myProbe.farClipPlane = PlayerCamera.farClipPlane;
        }

        started = true;

        //Render moon once always on start
        if (MoonShader != null)
        {
            MoonShader.SetFloat("_Phase", customMoonPhase);
            MoonShader.SetColor("_Color", skySettings.moonColor);
            MoonShader.SetFloat("_Brightness", skySettings.moonBrightness * (1 - GameTime.solarTime));
        }
    }
    /// <summary>
	/// Creation and setup of post processing components.
	/// </summary>
	private void InitImageEffects()
    {

        EnviroSkyRender = PlayerCamera.gameObject.GetComponent<EnviroSkyRenderingLW>();

        if (EnviroSkyRender == null)
            EnviroSkyRender = PlayerCamera.gameObject.AddComponent<EnviroSkyRenderingLW>();

        EnviroPostProcessing = PlayerCamera.gameObject.GetComponent<EnviroPostProcessing>();

        if (EnviroPostProcessing == null)
            EnviroPostProcessing = PlayerCamera.gameObject.AddComponent<EnviroPostProcessing>();

    }

    /// <summary>
    /// Setup Main light that is eather used for sun and moon or only sun based on used lighting mode.
    /// </summary>
    private void SetupMainLight()
    {
        if (Components.DirectLight)
        {
            MainLight = Components.DirectLight.GetComponent<Light>();

            if (EnviroSkyMgr.instance.dontDestroy && Application.isPlaying)
                DontDestroyOnLoad(Components.DirectLight);
        }
        else
        {
            GameObject oldLight = GameObject.Find("Enviro Directional Light");

            if (oldLight != null)
                Components.DirectLight = oldLight.transform;
            else
                Components.DirectLight = CreateDirectionalLight(false);

            MainLight = Components.DirectLight.GetComponent<Light>();

            if (EnviroSkyMgr.instance.dontDestroy && Application.isPlaying)
                DontDestroyOnLoad(Components.DirectLight);
        }

        //Remove the additional light if in single mode
        if (lightSettings.directionalLightMode == EnviroLightSettings.LightingMode.Single)
        {
            if (Components.AdditionalDirectLight != null)
                DestroyImmediate(Components.AdditionalDirectLight.gameObject);
        }
    }
    /// <summary>
    /// Setup additional light that is used for moon in dual lighting mode.
    /// </summary>
    private void SetupAdditionalLight()
    {

        if (Components.AdditionalDirectLight)
        {
            AdditionalLight = Components.AdditionalDirectLight.GetComponent<Light>();

            if (EnviroSkyMgr.instance.dontDestroy && Application.isPlaying)
                DontDestroyOnLoad(Components.AdditionalDirectLight);
        }
        else
        {
            GameObject oldLight = GameObject.Find("Enviro Directional Light - Moon");

            if (oldLight != null)
                Components.AdditionalDirectLight = oldLight.transform;
            else
                Components.AdditionalDirectLight = CreateDirectionalLight(true);

            AdditionalLight = Components.DirectLight.GetComponent<Light>();

            if (EnviroSkyMgr.instance.dontDestroy && Application.isPlaying)
                DontDestroyOnLoad(Components.AdditionalDirectLight);
        }
    }

    void Update()
    {
        if (profile == null)
        {
            Debug.Log("No profile applied! Please create and assign a profile.");
            return;
        }

        if (!started && !serverMode)
        {
            UpdateTime(GameTime.DaysInYear);
            UpdateSunAndMoonPosition();
            UpdateSceneView();
            CalculateDirectLight();
            UpdateAmbientLight();
            UpdateReflections();

            if (AssignInRuntime && PlayerTag != "" && CameraTag != "" && Application.isPlaying)
            {

                // Search for Player by tag
                GameObject plr = GameObject.FindGameObjectWithTag(PlayerTag);
                if (plr != null)
                    Player = plr;

                // Search for camera by tag
                for (int i = 0; i < Camera.allCameras.Length; i++)
                {
                    if (Camera.allCameras[i].tag == CameraTag)
                        PlayerCamera = Camera.allCameras[i];
                }

                if (Player != null && PlayerCamera != null)
                {
                    Init();
                    started = true;
                }
                else { started = false; return; }
            }
            else { started = false; return; }
        }

        UpdateTime(GameTime.DaysInYear);
        ValidateParameters();
     
        if (!serverMode)
        {
            UpdateSceneView();

            if (!Application.isPlaying && Weather.startWeatherPreset != null && startMode == EnviroStartMode.Started)
            {
                UpdateClouds(Weather.startWeatherPreset, false);
                UpdateFog(Weather.startWeatherPreset, false);
                UpdateWeatherVariables(Weather.startWeatherPreset);
#if AURA_IN_PROJECT
            if(EnviroSkyMgr.instance.aura2Support)
               UpdateAura2(Weather.startWeatherPreset, true);
#endif
            }

            UpdateAmbientLight();
            UpdateReflections();
            UpdateWeather();
            UpdateParticleClouds(useParticleClouds);
            UpdateSunAndMoonPosition();
            CalculateDirectLight();
            SetMaterialsVariables();
            CalculateSatPositions(LST);

#if !ENVIRO_HDRP
            if (RenderSettings.skybox != skyMat)
                SetupSkybox();
#endif

            //Enable or Disable Moon
            if (skySettings.renderMoon && !Components.Moon.activeSelf)
                Components.Moon.SetActive(true);
            else if(!skySettings.renderMoon && Components.Moon.activeSelf)
                Components.Moon.SetActive(false);

            if (EnviroSkyRender == null && PlayerCamera != null)
                InitImageEffects();

            // Switch to Unity forward fog for best performance.
            if(fogSettings.useUnityFog && PlayerCamera != null && PlayerCamera.actualRenderingPath == RenderingPath.Forward)
            {
                RenderSettings.fog = true;
                if(EnviroSkyRender != null && EnviroSkyRender.isActiveAndEnabled)
                   EnviroSkyRender.enabled = false;
            }
            else
            {
                //Enable or Disable Enviro Fog Post Effect
                if (usePostEffectFog && EnviroSkyRender != null && !EnviroSkyRender.isActiveAndEnabled)
                    EnviroSkyRender.enabled = true;
                else if (!usePostEffectFog && EnviroSkyRender != null && EnviroSkyRender.isActiveAndEnabled)
                    EnviroSkyRender.enabled = false;
            }

            if (!isNight && GameTime.solarTime < GameTime.dayNightSwitch)
            {
                isNight = true;
                if (Audio.AudioSourceAmbient != null)
                    TryPlayAmbientSFX();
                EnviroSkyMgr.instance.NotifyIsNight();
            }
            else if (isNight && GameTime.solarTime >= GameTime.dayNightSwitch)
            {
                isNight = false;
                if (Audio.AudioSourceAmbient != null)
                    TryPlayAmbientSFX();
                EnviroSkyMgr.instance.NotifyIsDay();
            }
        }
        else
        {
            UpdateWeather();

            if (!isNight && GameTime.solarTime < GameTime.dayNightSwitch)
            {
                isNight = true;
                EnviroSkyMgr.instance.NotifyIsNight();
            }
            else if (isNight && GameTime.solarTime >= GameTime.dayNightSwitch)
            {
                isNight = false;
                EnviroSkyMgr.instance.NotifyIsDay();
            }
        }
    }

    void LateUpdate()
    {
        if (!serverMode && PlayerCamera != null && Player != null)
        {
            transform.position = Player.transform.position;
            float scale = PlayerCamera.farClipPlane - (PlayerCamera.farClipPlane * 0.1f);
            transform.localScale = new Vector3(scale, scale, scale);

            if (EffectsHolder != null)
                EffectsHolder.transform.position = Player.transform.position;
        }
    }

    private void SetMaterialsVariables()
    {
        //Simple
        skyMat.SetFloat("_BlackGround", skySettings.blackGroundMode ? 1f : 0f);
        skyMat.SetColor("_SkyColor", skySettings.simpleSkyColor.Evaluate(GameTime.solarTime));
        skyMat.SetColor("_HorizonColor", skySettings.simpleHorizonColor.Evaluate(GameTime.solarTime));
        skyMat.SetColor("_SunColor", skySettings.simpleSunColor.Evaluate(GameTime.solarTime));
        skyMat.SetFloat("_SunDiskSizeSimple", skySettings.simpleSunDiskSize.Evaluate(GameTime.solarTime));
        skyMat.SetFloat("_StarsIntensity", skySettings.starsIntensity.Evaluate(GameTime.solarTime));
        //Clouds
        skyMat.SetVector("_CloudAnimation", cirrusAnim);

        //cirrus
        if (cloudsSettings.cirrusCloudsTexture != null)
            skyMat.SetTexture("_CloudMap", cloudsSettings.cirrusCloudsTexture);

        skyMat.SetColor("_CloudColor", cloudsSettings.cirrusCloudsColor.Evaluate(GameTime.solarTime));
        skyMat.SetFloat("_CloudAltitude", cloudsSettings.cirrusCloudsAltitude);
        skyMat.SetFloat("_CloudAlpha", cloudsConfig.cirrusAlpha);
        skyMat.SetFloat("_CloudCoverage", cloudsConfig.cirrusCoverage);
        skyMat.SetFloat("_CloudColorPower", cloudsConfig.cirrusColorPow);

        //Globals
        Shader.SetGlobalVector("_SunDir", -Components.Sun.transform.forward);
        Shader.SetGlobalColor("_EnviroLighting", lightSettings.LightColor.Evaluate(GameTime.solarTime));
        // Moon 
        Shader.SetGlobalVector("_SunPosition", Components.Sun.transform.localPosition + (-Components.Sun.transform.forward * 10000f));
        Shader.SetGlobalVector("_MoonPosition", Components.Moon.transform.localPosition);
        //Color Mods
        Shader.SetGlobalColor("_weatherSkyMod", Color.Lerp(currentWeatherSkyMod, interiorZoneSettings.currentInteriorSkyboxMod, interiorZoneSettings.currentInteriorSkyboxMod.a));
        Shader.SetGlobalColor("_weatherFogMod", Color.Lerp(currentWeatherFogMod, interiorZoneSettings.currentInteriorFogColorMod, interiorZoneSettings.currentInteriorFogColorMod.a));
        //Fog
        Shader.SetGlobalVector("_EnviroSkyFog", new Vector4(Fog.skyFogHeight, Fog.skyFogIntensity, Fog.skyFogStart, fogSettings.heightFogIntensity));
        Shader.SetGlobalFloat("_distanceFogIntensity", fogSettings.distanceFogIntensity);      
        Shader.SetGlobalFloat("_maximumFogDensity", 1 - fogSettings.maximumFogDensity);

        if (fogSettings.useSimpleFog)
        {
            Shader.EnableKeyword("ENVIRO_SIMPLE_FOG");
            Shader.SetGlobalVector("_EnviroParams", new Vector4(Mathf.Clamp(1f - GameTime.solarTime, 0.5f, 1f), fogSettings.distanceFog ? 1f : 0f, 0f, HDR ? 1f : 0f));
        }
        else
        {
            Shader.SetGlobalColor("_scatteringColor", skySettings.scatteringColor.Evaluate(GameTime.solarTime));
            Shader.SetGlobalFloat("_scatteringStrenght", Fog.scatteringStrenght);
            Shader.SetGlobalFloat("_scatteringPower", skySettings.scatteringCurve.Evaluate(GameTime.solarTime));
            Shader.SetGlobalFloat("_SunBlocking", Fog.sunBlocking);
            Shader.SetGlobalVector("_EnviroParams", new Vector4(Mathf.Clamp(1f - GameTime.solarTime, 0.5f, 1f), fogSettings.distanceFog ? 1f : 0f, fogSettings.heightFog ? 1f : 0f, HDR ? 1f : 0f));
            Shader.SetGlobalVector("_Bm", BetaMie(skySettings.turbidity, skySettings.waveLength) * (skySettings.mie * (Fog.scatteringStrenght * GameTime.solarTime)));
            Shader.SetGlobalVector("_BmScene", BetaMie(skySettings.turbidity, skySettings.waveLength) * (fogSettings.mie * (Fog.scatteringStrenght * GameTime.solarTime)));
            Shader.SetGlobalVector("_Br", BetaRay(skySettings.waveLength) * skySettings.rayleigh);
            Shader.SetGlobalVector("_mieG", GetMieG(skySettings.g));
            Shader.SetGlobalVector("_mieGScene", GetMieGScene(skySettings.g));
            Shader.SetGlobalVector("_SunParameters", new Vector4(skySettings.sunIntensity, skySettings.sunDiskScale, skySettings.sunDiskIntensity, 0f));
            Shader.SetGlobalFloat("_Exposure", skySettings.skyExposure);
            Shader.SetGlobalFloat("_SkyLuminance", skySettings.skyLuminence.Evaluate(GameTime.solarTime));
            Shader.SetGlobalFloat("_SkyColorPower", skySettings.skyColorPower.Evaluate(GameTime.solarTime));
            Shader.SetGlobalFloat("_heightFogIntensity", fogSettings.heightFogIntensity);
            Shader.SetGlobalFloat("_lightning", thunder);
            Shader.DisableKeyword("ENVIRO_SIMPLE_FOG");
        }

        Shader.DisableKeyword("ENVIROVOLUMELIGHT");

        if (MoonShader != null)
        {
            MoonShader.SetFloat("_Phase", customMoonPhase);
            MoonShader.SetColor("_Color", skySettings.moonColor);
            MoonShader.SetFloat("_Brightness", skySettings.moonBrightness * (1 - GameTime.solarTime));
            MoonShader.SetFloat("_moonFogIntensity", Fog.moonIntensity);
        }
    }

    // Make the parameters stay in reasonable range
    private void ValidateParameters()
    {
        // Keep GameTime Parameters right!
        internalHour = Mathf.Repeat(internalHour, 24f);
        GameTime.Longitude = Mathf.Clamp(GameTime.Longitude, -180, 180);
        GameTime.Latitude = Mathf.Clamp(GameTime.Latitude, -90, 90);
#if UNITY_EDITOR
        if (GameTime.cycleLengthInMinutes <= 0f)
        {
            if (GameTime.cycleLengthInMinutes < 0f)
                GameTime.cycleLengthInMinutes = 0f;
            internalHour = 12f;
            customMoonPhase = 0f;
        }

        if (GameTime.Days < 0)
            GameTime.Days = 0;

        if (GameTime.Years < 0)
            GameTime.Years = 0;

        // Moon
        customMoonPhase = Mathf.Clamp(customMoonPhase, -1f, 1f);
#endif
    }

    private void UpdateClouds(EnviroWeatherPreset i, bool withTransition)
    {
        if (i == null)
            return;

        float speed = 500f * Time.deltaTime;

        if (withTransition)
            speed = weatherSettings.cloudTransitionSpeed * Time.deltaTime;
       
        cloudsConfig.cirrusAlpha = Mathf.Lerp(cloudsConfig.cirrusAlpha, i.cloudsConfig.cirrusAlpha, speed);
        cloudsConfig.cirrusCoverage = Mathf.Lerp(cloudsConfig.cirrusCoverage, i.cloudsConfig.cirrusCoverage, speed);
        cloudsConfig.cirrusColorPow = Mathf.Lerp(cloudsConfig.cirrusColorPow, i.cloudsConfig.cirrusColorPow, speed);

        cloudsConfig.particleLayer1Alpha = Mathf.Lerp(cloudsConfig.particleLayer1Alpha, i.cloudsConfig.particleLayer1Alpha, speed);
        cloudsConfig.particleLayer1Brightness = Mathf.Lerp(cloudsConfig.particleLayer1Brightness, i.cloudsConfig.particleLayer1Brightness, speed);
        cloudsConfig.particleLayer1ColorPow = Mathf.Lerp(cloudsConfig.particleLayer1ColorPow, i.cloudsConfig.particleLayer1ColorPow, speed);

        cloudsConfig.particleLayer2Alpha = Mathf.Lerp(cloudsConfig.particleLayer2Alpha, i.cloudsConfig.particleLayer2Alpha, speed);
        cloudsConfig.particleLayer2Brightness = Mathf.Lerp(cloudsConfig.particleLayer2Brightness, i.cloudsConfig.particleLayer2Brightness, speed);
        cloudsConfig.particleLayer2ColorPow = Mathf.Lerp(cloudsConfig.particleLayer2ColorPow, i.cloudsConfig.particleLayer2ColorPow, speed);

        // globalVolumeLightIntensity = Mathf.Lerp(globalVolumeLightIntensity, i.volumeLightIntensity, speed);
        shadowIntensityMod = Mathf.Lerp(shadowIntensityMod, i.shadowIntensityMod, speed);

        currentWeatherSkyMod = Color.Lerp(currentWeatherSkyMod, i.weatherSkyMod.Evaluate(GameTime.solarTime), speed);
        currentWeatherFogMod = Color.Lerp(currentWeatherFogMod, i.weatherFogMod.Evaluate(GameTime.solarTime), speed * 10);
        currentWeatherLightMod = Color.Lerp(currentWeatherLightMod, i.weatherLightMod.Evaluate(GameTime.solarTime), speed);
    }

    private void UpdateFog(EnviroWeatherPreset i, bool withTransition)
    {
        // Set the Fog color to light color to match Day-Night cycle and weather
        Color fogClr = Color.Lerp(fogSettings.simpleFogColor.Evaluate(GameTime.solarTime), customFogColor, customFogIntensity);
        RenderSettings.fogColor = Color.Lerp(fogClr, currentWeatherFogMod, currentWeatherFogMod.a);

        if (i != null)
        {

            float speed = 500f * Time.deltaTime;

            if (withTransition)
                speed = weatherSettings.fogTransitionSpeed * Time.deltaTime;

            if (fogSettings.Fogmode == FogMode.Linear)
            {
                RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, i.fogDistance, speed);
                RenderSettings.fogStartDistance = Mathf.Lerp(RenderSettings.fogStartDistance, i.fogStartDistance, speed);
            }
            else
            {
                float targetDensity = i.fogDensity;

                if (fogSettings.useUnityFog)
                    targetDensity *= fogSettings.distanceFogIntensity;

                if (updateFogDensity)
                    RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetDensity, speed) * interiorZoneSettings.currentInteriorFogMod;
            }

            Fog.scatteringStrenght = Mathf.Lerp(Fog.scatteringStrenght, i.FogScatteringIntensity, speed);
            Fog.sunBlocking = Mathf.Lerp(Fog.sunBlocking, i.fogSunBlocking, speed);
            Fog.moonIntensity = Mathf.Lerp(Fog.moonIntensity, i.moonIntensity, speed);
            fogSettings.heightDensity = Mathf.Lerp(fogSettings.heightDensity, i.heightFogDensity, speed);
            Fog.skyFogStart = Mathf.Lerp(Fog.skyFogStart, i.skyFogStart, speed);
            Fog.skyFogHeight = Mathf.Lerp(Fog.skyFogHeight, i.SkyFogHeight, speed);
            Fog.skyFogIntensity = Mathf.Lerp(Fog.skyFogIntensity, i.SkyFogIntensity, speed);

        }
    }

    private void UpdateEffectSystems(EnviroWeatherPrefab id, bool withTransition)
    {
        if (id != null)
        {
            float speed = 500f * Time.deltaTime;

            if (withTransition)
                speed = weatherSettings.effectTransitionSpeed * Time.deltaTime;

            for (int i = 0; i < id.effectSystems.Count; i++)
            {

                if (id.effectSystems[i].isStopped)
                    id.effectSystems[i].Play();

                // Set EmissionRate
                float val = Mathf.Lerp(EnviroSkyMgr.instance.GetEmissionRate(id.effectSystems[i]), id.effectEmmisionRates[i] * qualitySettings.GlobalParticleEmissionRates, speed) * interiorZoneSettings.currentInteriorWeatherEffectMod;
                EnviroSkyMgr.instance.SetEmissionRate(id.effectSystems[i], val);
            }

            for (int i = 0; i < Weather.WeatherPrefabs.Count; i++)
            {
                if (Weather.WeatherPrefabs[i].gameObject != id.gameObject)
                {
                    for (int i2 = 0; i2 < Weather.WeatherPrefabs[i].effectSystems.Count; i2++)
                    {
                        float val2 = Mathf.Lerp(EnviroSkyMgr.instance.GetEmissionRate(Weather.WeatherPrefabs[i].effectSystems[i2]), 0f, speed);

                        if (val2 < 1f)
                            val2 = 0f;

                        EnviroSkyMgr.instance.SetEmissionRate(Weather.WeatherPrefabs[i].effectSystems[i2], val2);

                        if (val2 == 0f && !Weather.WeatherPrefabs[i].effectSystems[i2].isStopped)
                        {
                            Weather.WeatherPrefabs[i].effectSystems[i2].Stop();
                        }
                    }
                }
            }

            UpdateWeatherVariables(id.weatherPreset);
        }
    }

    private void UpdateWeather()
    {
        //Current active weather not matching current zones weather
        if (Weather.currentActiveWeatherPreset != Weather.currentActiveZone.currentActiveZoneWeatherPreset)
        {
            Weather.lastActiveWeatherPreset = Weather.currentActiveWeatherPreset;
            Weather.lastActiveWeatherPrefab = Weather.currentActiveWeatherPrefab;
            Weather.currentActiveWeatherPreset = Weather.currentActiveZone.currentActiveZoneWeatherPreset;
            Weather.currentActiveWeatherPrefab = Weather.currentActiveZone.currentActiveZoneWeatherPrefab;
            if (Weather.currentActiveWeatherPreset != null)
            {
                EnviroSkyMgr.instance.NotifyWeatherChanged(Weather.currentActiveWeatherPreset);
                Weather.weatherFullyChanged = false;
                if (!serverMode)
                {
                    TryPlayAmbientSFX();
                    UpdateAudioSource(Weather.currentActiveWeatherPreset);

                    if (Weather.currentActiveWeatherPreset.isLightningStorm)
                        StartCoroutine(PlayThunderRandom());
                    else
                    {
                        StopCoroutine(PlayThunderRandom());
                        Components.LightningGenerator.StopLightning();
                    }
                }
            }
        }

        if (Weather.currentActiveWeatherPrefab != null && !serverMode)
        {
            UpdateClouds(Weather.currentActiveWeatherPreset, true);
            UpdateFog(Weather.currentActiveWeatherPreset, true);
            UpdateEffectSystems(Weather.currentActiveWeatherPrefab, true);
            if (!Weather.weatherFullyChanged)
                CalcWeatherTransitionState();
        }
        else if (Weather.currentActiveWeatherPrefab != null)
        {
            UpdateWeatherVariables(Weather.currentActiveWeatherPrefab.weatherPreset);
        }
    }

    #region API
    /// <summary>
    /// Changes clouds, fog and particle effects to current weather settings instantly.
    /// </summary>
    public void InstantWeatherChange(EnviroWeatherPreset preset, EnviroWeatherPrefab prefab)
    {
        UpdateClouds(preset, false);
        UpdateFog(preset, false);
        UpdateEffectSystems(prefab, false);
    }

    /// <summary>
    /// Assign your Player and Camera and Initilize.////
    /// </summary>
    public void AssignAndStart(GameObject player, Camera Camera)
    {
        this.Player = player;
        PlayerCamera = Camera;
        Init();
        started = true;
    }

    /// <summary>
    /// Assign your Player and Camera and Initilize.////
    /// </summary>
    public void StartAsServer()
    {
        Player = gameObject;
        serverMode = true;
        Init();
    }

    /// <summary>
    /// Changes focus on other Player or Camera on runtime.////
    /// </summary>
    /// <param name="Player">Player.</param>
    /// <param name="Camera">Camera.</param>
    public void ChangeFocus(GameObject player, Camera Camera)
    {
        this.Player = player;
        if(PlayerCamera != null)
        RemoveEnviroCameraComponents(PlayerCamera);
        PlayerCamera = Camera;
        InitImageEffects();
    }
    /// <summary>
    /// Destroy all enviro related camera components on this camera.
    /// </summary> 
    private void RemoveEnviroCameraComponents(Camera cam)
    {
        EnviroSkyRenderingLW renderComponent;
        EnviroPostProcessing postProcessingComponent;

        renderComponent = cam.GetComponent<EnviroSkyRenderingLW>();
        if (renderComponent != null)
            Destroy(renderComponent);

        postProcessingComponent = cam.GetComponent<EnviroPostProcessing>();
        if (postProcessingComponent != null)
            Destroy(postProcessingComponent);
    }

    public void Play(EnviroTime.TimeProgressMode progressMode = EnviroTime.TimeProgressMode.Simulated)
    {
        SetupSkybox();

        if (!Components.DirectLight.gameObject.activeSelf)
            Components.DirectLight.gameObject.SetActive(true);

        GameTime.ProgressTime = progressMode;
        if (EffectsHolder != null)
            EffectsHolder.SetActive(true);
        if (EnviroSkyRender != null)
            EnviroSkyRender.enabled = true;

        started = true;

        TryPlayAmbientSFX();
    }

    public void Stop(bool disableLight = false, bool stopTime = true)
    {
        if (disableLight)
            Components.DirectLight.gameObject.SetActive(false);
        if (stopTime)
            GameTime.ProgressTime = EnviroTime.TimeProgressMode.None;

        if(EffectsHolder != null)
            EffectsHolder.SetActive(false);

        if (EnviroSkyRender != null)
            EnviroSkyRender.enabled = false;
        if (EnviroPostProcessing != null)
            EnviroPostProcessing.enabled = false;
        started = false;
    }

    public void Deactivate(bool disableLight = false)
    {
        if (disableLight)
            Components.DirectLight.gameObject.SetActive(false);

        if (EffectsHolder != null)
            EffectsHolder.SetActive(false);

        if (EnviroSkyRender != null)
            EnviroSkyRender.enabled = false;

        if (EnviroPostProcessing != null)
            EnviroPostProcessing.enabled = false;
    }

    public void Activate()
    {
        Components.DirectLight.gameObject.SetActive(true);

        if (EffectsHolder != null)
            EffectsHolder.SetActive(true);

        if (EnviroSkyRender != null)
            EnviroSkyRender.enabled = true;

        if (EnviroPostProcessing != null)
            EnviroPostProcessing.enabled = true;

        TryPlayAmbientSFX();
        if(Weather.currentAudioSource != null)
        Weather.currentAudioSource.audiosrc.Play();

    }
    #endregion
}


