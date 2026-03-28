using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.GlobalIllumination;



[Serializable]
struct LightDayData
{
    public float TimeOfDay; // 0 - 1

    public Color LightColor;
    public float LightIntensity;
}

[Serializable]
struct LightData
{
    public Light LightSource;
    public List<LightDayData> DayData;
}

public class WorldDayCycle : MonoBehaviour
{
    [Header("Sky Settings")]
    [SerializeField] private GameObject _skySphere;
    private Material _skyMaterial;
    private static float _cycle;

    [SerializeField] private float _cycleSpeed = 0.01f;

    [SerializeField] private const float _sunriseTime = 0.25f;
    [SerializeField] private const float _sunsetTime = 0.75f;

    [Header("Lamps")] 
    [SerializeField] private Material _lampMaterial;
    private const string LAMP_ON = "_LIGHTS_ON";
    private List<Light> _lampLights;

    [Header("Directional Light")]
    [SerializeField] private List<LightData> _lights;



    [Header("Events")]
    public UnityEvent OnSunrise = new();
    public UnityEvent OnSunset = new();


    // START
    //------------------------------------------------
    private void Awake()
    {
        if (_skySphere != null)
            _skyMaterial = _skySphere.GetComponent<Renderer>().material;

        // find all lamp lights in the scene
        var allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        _lampLights = new List<Light>();
        foreach (var light in allLights)
        {
            if (light.gameObject.CompareTag("LanterLight"))
            {
                _lampLights.Add(light);
            }
        }
    }
    private void OnEnable()
    {
        _skyMaterial.SetFloat("_Cycle", _cycle);
    }


    // UPDATE
    //------------------------------------------------
    private void Update()
    {
        if (_skyMaterial != null)
        {
            _cycle += Time.deltaTime * _cycleSpeed;
            if (_cycle > 1f) _cycle = 0f;

            _skyMaterial.SetFloat("_Cycle", _cycle);


            // if sunrise
            if (_cycle >= _sunriseTime && _cycle < _sunsetTime)
            {
                OnSunrise.Invoke();
                TurnOnLights();
            }
            else // sunset
            {
                OnSunset.Invoke();
                TurnOnLights(false);
            }
        }
    }
    private void LateUpdate()
    {
        UpdateDirectionalLight();
    }


    // HELPERS
    //------------------------------------------------
    private void TurnOnLights(bool on = true)
    {
        if (_lampMaterial != null)
        {
            if (on)
                _lampMaterial.EnableKeyword(LAMP_ON);
            else
                _lampMaterial.DisableKeyword(LAMP_ON);  
        }

        foreach (var light in _lampLights)
        {
            light.enabled = on;
        }
    }

    private void UpdateDirectionalLight()
    {
        if(_lights.Count == 0)
            return;

        foreach (var lightData in _lights)
        {
            if (lightData.LightSource != null && lightData.DayData.Count > 0)
            {
                LightDayData before = lightData.DayData[0];
                LightDayData after = lightData.DayData[0];

                // find the two keyframes to interpolate between
                foreach (var data in lightData.DayData)
                {
                    if (data.TimeOfDay <= _cycle)
                        before = data;
                    if (data.TimeOfDay >= _cycle)
                    {
                        after = data;
                        break;
                    }
                }

                float range = after.TimeOfDay - before.TimeOfDay;
                if (range < 0f) range += 1f; // wrap around midnight
                float t = range == 0f ? 0f : (_cycle - before.TimeOfDay) / range;
                if (t < 0f) t += 1f;

                // interpolate light properties
                lightData.LightSource.color = Color.Lerp(before.LightColor, after.LightColor, t);
                lightData.LightSource.intensity = Mathf.Lerp(before.LightIntensity, after.LightIntensity, t);
            }
        }
        
    }

}

