using System;
using UnityEngine;
using GoogleMobileAds.Api;
using PimDeWitte.UnityMainThreadDispatcher;


/// <summary>
/// Production-ready AdMob interstitial manager.
/// - Preloads interstitials
/// - Enforces cooldown + max per session
/// - Auto reloads after show/close/failure
/// - Exponential backoff on load failures
/// </summary>
public sealed class InterstitialAdManager : MonoBehaviour
{
    public static InterstitialAdManager Instance { get; private set; }

    [Header("Ad Unit IDs")]
    [Tooltip("Android interstitial ad unit id")]
    [SerializeField] private string androidAdUnitId = "ca-app-pub-8084077036098985~8462842086";

    [Tooltip("iOS interstitial ad unit id")]
    [SerializeField] private string iosAdUnitId = "ca-app-pub-xxxxxxxxxxxxxxxx/xxxxxxxxxx";

    [Header("Frequency Controls")]
    [Tooltip("Minimum seconds between interstitial shows (safety rail).")]
    [SerializeField] private float cooldownSeconds = 420f; // 7 minutes

    [Tooltip("Max interstitials per session (another safety rail).")]
    [SerializeField] private int maxInterstitialsPerSession = 6;

    [Header("Behavior")]
    [Tooltip("If true, manager will keep trying to load in background.")]
    [SerializeField] private bool autoLoad = true;

    [Tooltip("Enable extra logs in development.")]
    [SerializeField] private bool verboseLogging = true;

    private InterstitialAd _interstitial;
    private bool _isLoading;
    private bool _showRequested;
    private Action _pendingOnClosed;
    private string _pendingPlacement;

    private float _lastShowTime = -999999f;
    private int _shownThisSession = 0;

    // Backoff
    private int _consecutiveLoadFailures = 0;
    private float _nextLoadAllowedTime = 0f;

    private string AdUnitId
    {
        get
        {
#if UNITY_ANDROID
            return androidAdUnitId;
#elif UNITY_IOS
            return iosAdUnitId;
#else
            return androidAdUnitId;
#endif
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Debug.Log("Initializing Google Mobile Ads SDK...");
        MobileAds.Initialize((InitializationStatus initstatus) =>
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => 
                    {
                        if (initstatus == null)
                        {
                            Debug.LogError("Google Mobile Ads initialization failed.");
                            return;
                        }

                        Debug.Log("Google Mobile Ads initialization complete.");

                        // 3. Optional: Verify specific adapters if needed
                        // var map = initStatus.getAdapterStatusMap();

                        if (autoLoad)
                        {
                            LoadIfNeeded();
                        }
                    });
                });
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        DestroyInterstitial();
    }

    /// <summary>
    /// Call this at natural breaks, e.g. after gameplay complete or side story complete.
    /// Will show immediately if eligible, else will queue and show as soon as an ad is ready (still respecting cooldown).
    /// </summary>
    public bool TryShow(string placement, Action onClosed = null)
    {
        _pendingPlacement = placement;
        _pendingOnClosed = onClosed;

        if (!IsEligibleToShow())
        {
            // Still try to load for later.
            _showRequested = true;
            LoadIfNeeded();
            LogError($"TryShow blocked. ready={IsReady()} shownSession={_shownThisSession}/{maxInterstitialsPerSession} " +
             $"cooldownLeft={Mathf.Max(0, cooldownSeconds - (Time.unscaledTime - _lastShowTime)):0.0}s " +
             $"placement={placement}");
            return false;
        }

        Debug.Log(_interstitial != null ? (_interstitial.CanShowAd() ? "Interstitial can show ad" : "Interstitial cannot show ad") : "No interstitial to check");
        if (_interstitial != null && _interstitial.CanShowAd())
        {
            ShowInternal();
            return true;
        }

        // Not ready: queue request and load.
        _showRequested = true;
        LoadIfNeeded();
        Log($"TryShow queued (ad not ready). placement={placement}");
        return false;
    }

    /// <summary>Manually force a preload (safe to call anytime).</summary>
    public void Preload()
    {
        LoadIfNeeded(force: true);
    }

    public bool IsReady()
    {
        return _interstitial != null && _interstitial.CanShowAd();
    }

    public int ShownThisSession => _shownThisSession;

    private bool IsEligibleToShow()
    {
        if (string.IsNullOrEmpty(AdUnitId))
            return false;

        if (_shownThisSession >= maxInterstitialsPerSession)
            return false;

        // if (Time.unscaledTime - _lastShowTime < cooldownSeconds)
        //     return false;

        // Add your own global "no-ads" flags here if needed, e.g.:
        // if (StoryState.IsInCriticalMoment) return false;

        return true;
    }

    private void ShowInternal()
    {
        if (_interstitial == null || !_interstitial.CanShowAd())
        {
            _showRequested = true;
            LoadIfNeeded();
            return;
        }

        _showRequested = false;

        // Reset pending callback locals (keep references safe)
        string placement = _pendingPlacement ?? "unknown";
        Log($"Showing interstitial. placement={placement}");

        try
        {
            _interstitial.Show();
        }
        catch (Exception e)
        {
            LogError($"Interstitial.Show exception: {e}");
            // Attempt reload
            DestroyInterstitial();
            LoadIfNeeded(force: true);
            // Continue game flow if show failed
            InvokePendingClosed();
        }
    }

    private void LoadIfNeeded(bool force = false)
    {
        if (string.IsNullOrEmpty(AdUnitId))
        {
            LogError("AdUnitId is empty. Set it in Inspector.");
            return;
        }

        if (_isLoading) return;

        // Already loaded and ready
        if (!force && _interstitial != null && _interstitial.CanShowAd())
            return;

        // Respect backoff window
        if (!force && Time.unscaledTime < _nextLoadAllowedTime)
            return;

        // Clear any old interstitial instance before loading a fresh one
        DestroyInterstitial();

        _isLoading = true;

        var request = new AdRequest();

        Log("Loading interstitial...");

        InterstitialAd.Load(AdUnitId, request, (InterstitialAd ad, LoadAdError error) =>
        {
            _isLoading = false;

            if (error != null || ad == null)
            {
                _consecutiveLoadFailures++;
                float delay = ComputeBackoffSeconds(_consecutiveLoadFailures);
                _nextLoadAllowedTime = Time.unscaledTime + delay;

                LogError($"Interstitial failed to load. failures={_consecutiveLoadFailures} backoff={delay:0.0}s error={(error != null ? error.ToString() : "null")}");
                return;
            }

            _consecutiveLoadFailures = 0;
            _nextLoadAllowedTime = 0f;

            _interstitial = ad;
            RegisterEvents(_interstitial);

            Log("Interstitial loaded.");

            // If caller requested show and we are eligible now, show it.
            if (_showRequested && IsEligibleToShow() && _interstitial.CanShowAd())
            {
                ShowInternal();
            }
        });
    }

    private void RegisterEvents(InterstitialAd ad)
    {
        // Note: Fullscreen events can be delayed on some devices/creatives.
        // Avoid relying on ultra-precise timing.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Log("Interstitial opened.");
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Log("Interstitial closed.");
            InvokePendingClosed();

            // Must load a new one after show (single-use)
            DestroyInterstitial();
            if (autoLoad) LoadIfNeeded(force: true);
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            LogError($"Interstitial failed to present: {error}");
            InvokePendingClosed();

            DestroyInterstitial();
            if (autoLoad) LoadIfNeeded(force: true);
        };

        ad.OnAdImpressionRecorded += () =>
        {
            Log("Interstitial impression recorded.");
        };

        ad.OnAdClicked += () =>
        {
            Log("Interstitial clicked.");
        };

        ad.OnAdPaid += (AdValue adValue) =>
        {
            // Useful for LTV analysis / paid event logging
            Log($"Interstitial paid event: {adValue.Value} {adValue.CurrencyCode} precision={adValue.Precision}");
        };
    }

    private void InvokePendingClosed()
    {
        var cb = _pendingOnClosed;
        _pendingOnClosed = null;
        _pendingPlacement = null;

        try
        {
            cb?.Invoke();
        }
        catch (Exception e)
        {
            LogError($"onClosed callback exception: {e}");
        }
    }

    private void DestroyInterstitial()
    {
        try
        {
            if (_interstitial != null)
            {
                _interstitial.Destroy();
                _interstitial = null;
            }
        }
        catch (Exception e)
        {
            LogError($"DestroyInterstitial exception: {e}");
        }
    }

    private float ComputeBackoffSeconds(int failures)
    {
        // Exponential backoff: 2, 4, 8, 16, 30, 30...
        // Keep it reasonable; interstitial fill may recover quickly.
        if (failures <= 1) return 2f;
        if (failures == 2) return 4f;
        if (failures == 3) return 8f;
        if (failures == 4) return 16f;
        return 30f;
    }

    private void Log(string msg)
    {
        if (!verboseLogging) return;
        Debug.Log($"[InterstitialAdManager] {msg}");
    }

    private void LogError(string msg)
    {
        Debug.LogError($"[InterstitialAdManager] {msg}");
    }
}
