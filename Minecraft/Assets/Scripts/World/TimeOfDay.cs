using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeOfDay : MonoBehaviour
{
    public eTimeOfDay TOD;
    [Range(1, 1440)] public int time;
    [Range(0, 15)] public int globalLightLevel;
    public Map map;
    public Material skyboxMaterial;

    private float scaledTime = 0;
    private WaitForSeconds waitOneSecond = new WaitForSeconds(1.0f);


    private float globalLightIntensity = 1.0f;
    [SerializeField]
    private Color dayFogColor = new Color();
    [SerializeField]
    private Color nightFogColor = new Color();


    private void Awake ()
    {
        SetTime(time);
    }

    public void SetTime (int time)
    {
        StopAllCoroutines();
        
        this.time = time;
        TOD = eTimeOfDay.UNDEFINIED;
        StartCoroutine(TimeProgressor()); 
    }

    public void SetTime (string timeName)
    {
        timeName = timeName.ToLower();

        switch (timeName)
        {
            case "sunrise":
                SetTime(0);
                break;
            case "day":
                SetTime(61);
                break;
            case "sunset":
                SetTime(661);
                break;
            case "night":
                SetTime(721);
                break;
            default:
                Debug.Log("Unknown command /time set " + timeName);
                break;
        }
    }

    private IEnumerator TimeProgressor ()
    {
        while (true)
        {
            if (time < 1440)
                time++;
            else
                time = 1;

            scaledTime = (float)time / 1440f;
            skyboxMaterial.SetInt("_SunMoonRotation", (int)(scaledTime * 360f));

            switch (time)
            {
                case int n when n <= 60:
                    if (TOD == eTimeOfDay.SUNRISE)
                        break;

                    TOD = eTimeOfDay.SUNRISE;
                    SetGlobalLightLevel(4, false);
                    StartCoroutine(SunriseProgressor(true, 60 - time));
                    break;
                case int n when n <= 660:
                    if (TOD == eTimeOfDay.DAY)
                        break;

                    TOD = eTimeOfDay.DAY;
                    SetGlobalLightLevel(15);
                    skyboxMaterial.SetFloat("_CubemapTransition", 0f);
                    RenderSettings.fogColor = dayFogColor;
                    break;
                case int n when n <= 720:
                    if (TOD == eTimeOfDay.SUNSET)
                        break;

                    TOD = eTimeOfDay.SUNSET;
                    SetGlobalLightLevel(15, false);
                    StartCoroutine(SunriseProgressor(false, 720 - time));
                    break;
                case int n when n <= 1440:
                    if (TOD == eTimeOfDay.NIGHT)
                        break;

                    TOD = eTimeOfDay.NIGHT;
                    SetGlobalLightLevel(4);
                    skyboxMaterial.SetFloat("_CubemapTransition", 1f);
                    RenderSettings.fogColor = nightFogColor;
                    break;
            }

            yield return waitOneSecond;
        }
    }

    private IEnumerator SunriseProgressor (bool isSunrise, float timeLeft)
    {
        float currentTimeProgress = 60 - timeLeft;
        float progress;
        float progressGlobalLight;

        float startSunIntensity = LightLevelToIntensity(isSunrise ? 4 : 15);
        float endSunIntensity = LightLevelToIntensity(isSunrise ? 15 : 4);

        while (currentTimeProgress < 60)
        {
            if (isSunrise)
                progress = Mathf.Lerp(1, 0, currentTimeProgress / 60);
            else
                progress = Mathf.Lerp(0, 1, currentTimeProgress / 60);

            progressGlobalLight = Mathf.Lerp(startSunIntensity, endSunIntensity, currentTimeProgress / 60);

            skyboxMaterial.SetFloat("_CubemapTransition", progress);
            Shader.SetGlobalFloat("GlobalLightIntensity", progressGlobalLight);
            RenderSettings.fogColor = Color.Lerp(dayFogColor, nightFogColor, progress);

            currentTimeProgress += Time.deltaTime;
            yield return null;
        }
    }

    public void SetGlobalLightLevel (int lightLevel, bool applyLightIntensityNow = true)
    {
        if (lightLevel == globalLightLevel)
            return;

        globalLightLevel = lightLevel;

        if (applyLightIntensityNow)
        {
            globalLightIntensity = LightLevelToIntensity(globalLightLevel);
            Shader.SetGlobalFloat("GlobalLightIntensity", globalLightIntensity);
        }

        // Force light update and rerender chunks
       /* List<Chunk> chunks = map.chunks;
        
        foreach (Chunk chunk in chunks)
        {
            if (!chunk.IsRendered)
                continue;

            chunk.chunkLight.UpdateNaturalLight((byte)globalLightLevel);
        } */

    }

    private float LightLevelToIntensity(int lightLevel)
    {
        return lightLevel switch
        {
            15 => 0.0f,
            14 => 0.0625f,
            13 => 0.125f,
            12 => 0.1875f,
            11 => 0.25f,
            10 => 0.3125f,
            9 => 0.375f,
            8 => 0.4375f,
            7 => 0.5f,
            6 => 0.5625f,
            5 => 0.625f,
            4 => 0.6875f,
            3 => 0.75f,
            2 => 0.8125f,
            1 => 0.875f,
            0 => 0.9375f,
            _ => 1.0f,
        };
    }

}

public enum eTimeOfDay
{
    SUNRISE = 0,
    DAY = 1,
    SUNSET = 2,
    NIGHT = 3,
    UNDEFINIED = 4
}
