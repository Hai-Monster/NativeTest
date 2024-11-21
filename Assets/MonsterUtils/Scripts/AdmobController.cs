using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdmobController : MonoBehaviour
{
    public static bool initialized;
    // Start is called before the first frame update
    void Start()
    {
        // When true all events raised by GoogleMobileAds will be raised
        // on the Unity main thread. The default value is false.
        MobileAds.RaiseAdEventsOnUnityMainThread = true;

        MobileAds.Initialize((InitializationStatus initstatus) =>
        {
            // TODO: Request an ad.
            foreach (var adapterStatusMap in initstatus.getAdapterStatusMap())
            {
                Debug.Log($"[{adapterStatusMap.Key}," +
                          $" InitializationState: {adapterStatusMap.Value.InitializationState}," +
                          $" Description: {adapterStatusMap.Value.Description}," +
                          $" Latency: {adapterStatusMap.Value.Latency}]");
            }

            initialized = true;
            Debug.Log("[MobileAds] Initialized");
        });
    }

    // Update is called once per frame
    void Update()
    {
    }
}