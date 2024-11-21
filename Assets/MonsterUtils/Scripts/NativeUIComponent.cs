using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class NativeUIComponent : MonoBehaviour
{
    [SerializeField] private string adUnitId = "";
    [SerializeField] private Image iconImage, defaultImage, adChoices;
    [SerializeField] private Text headlineText;
    [SerializeField] private Text callToActionText;

    [SerializeField] private GameObject adView, defaultView;
    [SerializeField] private Button storeButton;

    [SerializeField] private List<Sprite> defaultImgList;

    private bool nativeAdLoaded;
    private float timeReload = 10f;
    private Coroutine nativeReload = null;
    private bool hasRequestAds = false, isNativeActive = true;

    private NativeAd nativeAd;

    private IEnumerator Start()
    {
        //Turn off adView
        adView.SetActive(false);
        defaultView.SetActive(true);
        storeButton.onClick.AddListener(StoreCall);

        yield return new WaitForSeconds(.1f);
        yield return new WaitUntil(() => AdmobController.initialized);
        Debug.Log("[MobileAds] Native start.");
        if (string.IsNullOrEmpty(adUnitId))
        {
            Debug.LogError($"NativeUI: No unit ID");
            yield break;
        }

        if (defaultImgList != null && defaultImgList.Count > 0)
        {
            UnityEngine.Random.InitState((int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            int rand = UnityEngine.Random.Range(0, defaultImgList.Count);
            defaultImage.sprite = defaultImgList[rand];
        }

        timeReload = 10f;
        isNativeActive = timeReload > 0;
        if (!isNativeActive)
        {
            Debug.Log($"NativeUI destroy {timeReload}");
            yield break;
        }

        hasRequestAds = true;
        RequestAdStart();
    }

    private void RequestAdStart()
    {
        if (nativeReload != null) StopCoroutine(nativeReload);
        nativeReload = StartCoroutine(RequestNativeAdYield());
    }

    private IEnumerator RequestNativeAdYield()
    {
        yield return new WaitForSeconds(0.5f);
        while (timeReload > 0)
        {
            RequestNativeAd();
            yield return new WaitForSeconds(timeReload);
            Debug.Log($"NativeUI {name} reloaded");
        }
    }

    private void OnEnable()
    {
        if (hasRequestAds)
        {
            RequestAdStart();
        }
    }

    private void OnDisable()
    {
        if (nativeReload != null)
        {
            StopCoroutine(nativeReload);
            nativeReload = null;
        }
    }

    public void StoreCall()
    {
        Application.OpenURL($"https://play.google.com/store/apps/dev?id=");
    }

    private void RequestNativeAd()
    {
        AdLoader adLoader = new AdLoader.Builder(adUnitId)
            .ForNativeAd()
            .Build();

        adLoader.OnNativeAdLoaded += HandleNativeAdLoaded;
        adLoader.OnAdFailedToLoad += HandleNativeAdFailedToLoad;
        adLoader.OnNativeAdClicked += HandleNativeAdClicked;
        adLoader.OnNativeAdClosed += HandleNativeAdClosed;
        adLoader.OnNativeAdImpression += HandleNativeAdImpression;
        adLoader.OnNativeAdOpening += HandleNativeAdOpening;

        adLoader.LoadAd(new AdRequest());
        Debug.Log("[MobileAds] RequestNativeAd.");
    }

    private void HandleNativeAdLoaded(object sender, NativeAdEventArgs args)
    {
        Debug.Log("[MobileAds] Native ad loaded.");
        if (args == null) return;
        nativeAd = args.nativeAd;
        nativeAdLoaded = true;

        if (nativeReload != null && nativeAd != null)
        {
            ResponseInfo adInfo = nativeAd.GetResponseInfo();
            AdapterResponseInfo adapterInfo = adInfo.GetLoadedAdapterResponseInfo();
            Debug.Log($"Native loaded an ad with response : {adapterInfo}, {adInfo}, {adInfo.GetResponseExtras()}");
            string adId = adInfo.GetResponseId();
            string adNetwork = adapterInfo.AdSourceName;
            string adPlacement = adapterInfo.AdSourceInstanceName;

            nativeAd.OnPaidEvent += HandleNativeAdPaid;
        }
        else
        {
            Debug.Log($"[MobileAds] Native ad loaded null {nativeReload != null} - {nativeAd != null}.");
        }
    }

    private void HandleNativeAdPaid(object sender, AdValueEventArgs adValueEventArgs)
    {
        var adInfo = nativeAd.GetResponseInfo();
        AdapterResponseInfo adapterInfo = adInfo.GetLoadedAdapterResponseInfo();
        string adId = adInfo.GetResponseId();
        string adNetwork = adapterInfo.AdSourceName;
        string adPlacement = adapterInfo.AdSourceInstanceName;

        var adValue = adValueEventArgs.AdValue;
        var currencyCode = adValue.CurrencyCode;
        var value = adValue.Value;
        Debug.Log(string.Format("Native paid {0} {1}.", value, currencyCode));
    }

    private void HandleNativeAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log("[MobileAds] Native ad failed to load: " + args.LoadAdError.GetResponseInfo());
    }

    private void HandleNativeAdClicked(object sender, EventArgs e)
    {
        Debug.Log("[MobileAds] Native ad clicked.");
    }

    private void HandleNativeAdClosed(object sender, EventArgs e)
    {
        Debug.Log("[MobileAds] Native ad closed.");
    }

    private void HandleNativeAdImpression(object sender, EventArgs args)
    {
        Debug.Log("[MobileAds] HandleNativeAdImpression.");
    }

    private void HandleNativeAdOpening(object sender, EventArgs e)
    {
        Debug.Log("[MobileAds] Native ad opening.");
    }

    private void Update()
    {
        if (nativeAdLoaded && nativeAd != null)
        {
            nativeAdLoaded = false;
            if (iconImage)
            {
                var icon = nativeAd.GetIconTexture();
                if (icon != null)
                {
                    iconImage.sprite = Sprite.Create(icon,
                        new Rect(0, 0, icon.width, icon.height),
                        Vector2.one * 0.5f);
                }

                if (!nativeAd.RegisterIconImageGameObject(iconImage.gameObject))
                {
                    Debug.Log("[MobileAds] Native ad failed to RegisterIconImageGameObject");
                }
            }

            if (headlineText)
            {
                headlineText.text = nativeAd.GetHeadlineText();
                if (!nativeAd.RegisterHeadlineTextGameObject(headlineText.gameObject))
                {
                    Debug.Log("[MobileAds] Native ad failed to RegisterHeadlineTextGameObject");
                }
            }

            if (callToActionText)
            {
                callToActionText.text = nativeAd.GetCallToActionText();
                if (!nativeAd.RegisterCallToActionGameObject(callToActionText.gameObject))
                {
                    Debug.Log("[MobileAds] Native ad failed to RegisterCallToActionGameObject");
                }
            }

            if (adChoices)
            {
                var adChoice = nativeAd.GetAdChoicesLogoTexture();
                if (adChoice != null)
                {
                    adChoices.sprite = Sprite.Create(adChoice,
                        new Rect(0, 0, adChoice.width, adChoice.height),
                        Vector2.one * 0.5f);
                }

                if (!nativeAd.RegisterAdChoicesLogoGameObject(adChoices.gameObject))
                {
                    Debug.Log("[MobileAds] Native ad failed to RegisterAdChoicesLogoGameObject");
                }
            }

            adView.SetActive(true);
            defaultView.SetActive(false);
        }
    }
}