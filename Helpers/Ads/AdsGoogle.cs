﻿using Android.App;
using Android.Content.Res;
using Android.Gms.Maps;
using Android.Util;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Anjo.Android.GoogleAds;
using Com.Google.Android.Gms.Ads;
using Com.Google.Android.Gms.Ads.Admanager;
using Com.Google.Android.Gms.Ads.Appopen;
using Com.Google.Android.Gms.Ads.Initialization;
using Com.Google.Android.Gms.Ads.Interstitial;
using Com.Google.Android.Gms.Ads.Nativead;
using Com.Google.Android.Gms.Ads.Rewarded;
using Com.Google.Android.Gms.Ads.Rewardedinterstitial;
using PlayTube.Helpers.Utils;
using System;
using System.Collections.Generic;
using Xamarin.Google.UserMesssagingPlatform;
using Exception = System.Exception;
using NativeAd = Com.Google.Android.Gms.Ads.Nativead.NativeAd;
using Object = Java.Lang.Object;

namespace PlayTube.Helpers.Ads
{
    public static class AdsGoogle
    {
        private static int CountInterstitial;
        private static int CountRewarded;
        private static int CountAppOpen;
        private static int CountRewardedInterstitial;
        public static List<NativeAd> NativeAdsPool = new List<NativeAd>();

        #region Interstitial

        private class AdMobInterstitial : AnjoInterstitialLoadCallback
        {
            private Activity ActivityContext;

            public void Show(Activity context)
            {
                try
                {
                    ActivityContext = context;
                    var requestBuilder = new AdRequest.Builder().Build();
                    InterstitialAd.Load(context, AppSettings.AdInterstitialKey, requestBuilder, this);
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnAdLoaded(InterstitialAd p0)
            {
                try
                {
                    p0?.Show(ActivityContext);
                    base.OnAdLoaded(p0);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnAdFailedToLoad(LoadAdError p0)
            {
                Log.Debug("Google-Ads", "I_Ad AdMobInterstitial Load Failed: " + p0.Message);
                base.OnAdFailedToLoad(p0);
            }
        }

        public static void Ad_Interstitial(Activity context, bool openDir = false)
        {
            try
            {
                if (AppTools.GetStatusAds() && AppSettings.ShowAdMobInterstitial)
                {
                    if (openDir)
                    {
                        AdMobInterstitial ads = new AdMobInterstitial();
                        ads.Show(context);
                        return;
                    }

                    if (CountInterstitial == AppSettings.ShowAdInterstitialCount)
                    {
                        CountInterstitial = 0;
                        AdMobInterstitial ads = new AdMobInterstitial();
                        ads.Show(context);
                        return;
                    }
                    else
                    {
                        Ad_AppOpenManager(context);
                    }

                    CountInterstitial++;
                }
                else
                {
                    Ad_AppOpenManager(context);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Native 

        public class AdMobNative : AdListener, NativeAd.IOnNativeAdLoadedListener
        {
            private TemplateView Template;
            private Activity Context;
            private AdLoader AdLoader;

            public void BindAdMobNative(Activity act)
            {
                try
                {
                    if (AppTools.GetStatusAds() && AppSettings.ShowAdMobNative)
                    {
                        AdLoader.Builder builder = new AdLoader.Builder(act, AppSettings.AdAdMobNativeKey);
                        builder.ForNativeAd(this);

                        VideoOptions videoOptions = new VideoOptions.Builder()
                            .SetCustomControlsRequested(true)
                            .SetStartMuted(true)
                            .Build();

                        NativeAdOptions adOptions = new NativeAdOptions.Builder()
                            .SetMediaAspectRatio(IMediaAspectRatio.Portrait)
                            .SetVideoOptions(videoOptions)
                            .Build();

                        builder.WithNativeAdOptions(adOptions);

                        AdLoader = builder.WithAdListener(this).Build();
                        AdLoader.LoadAds(new AdRequest.Builder().Build(), 5);
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void ShowAd(Activity context, TemplateView template = null)
            {
                try
                {
                    Context = context;

                    Template = template ?? Context.FindViewById<TemplateView>(Resource.Id.my_template);
                    if (Template != null)
                    {
                        Template.Visibility = ViewStates.Gone;

                        if (AppTools.GetStatusAds() && AppSettings.ShowAdMobNative)
                        {
                            BindAdMobNative(context);
                        }
                        else
                        {
                            Template.Visibility = ViewStates.Gone;
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnNativeAdLoaded(NativeAd ad)
            {
                try
                {
                    // some code that displays the ad.
                    if (AdLoader != null && AdLoader.IsLoading)
                    {
                        // The AdLoader is still loading ads.
                        // Expect more adLoaded or onAdFailedToLoad callbacks.
                        NativeAdsPool.Add(ad);
                    }
                    else
                    {
                        // The AdLoader has finished loading ads. 
                        if (Template != null)
                        {
                            NativeTemplateStyle styles = new NativeTemplateStyle.Builder().Build();

                            if (Template.GetTemplateTypeName() == TemplateView.NativeContentAd)
                            {
                                Template.NativeContentAdView(ad);
                            }
                            else
                            {
                                Template.SetStyles(styles);
                                Template.SetNativeAd(ad);
                            }

                            Template.Visibility = ViewStates.Visible;
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnAdFailedToLoad(LoadAdError p0)
            {
                try
                {
                    Log.Debug("Google-Ads", "I_Ad AdMobNative Load Failed: " + p0.Message);

                    if (Template != null)
                        Template.Visibility = ViewStates.Gone;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public static void Ad_AdMobNative(Activity context, TemplateView template = null)
        {
            try
            {
                if (AppTools.GetStatusAds() && AppSettings.ShowAdMobNative)
                {
                    AdMobNative ads = new AdMobNative();
                    ads.ShowAd(context, template);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Rewarded

        public class AdMobRewardedVideo : AnjoRewardedLoadCallback
        {
            private Activity Context;
            public void ShowAd(Activity context)
            {
                try
                {
                    Context = context;

                    AdRequest adRequest = new AdRequest.Builder().Build();
                    RewardedAd.Load(context, AppSettings.AdRewardVideoKey, adRequest, this);
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnAdLoaded(RewardedAd p0)
            {
                try
                {
                    p0?.Show(Context, new MyUserEarnedRewardListener(Context));
                    base.OnAdLoaded(p0);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnAdFailedToLoad(LoadAdError p0)
            {
                Log.Debug("Google-Ads", "I_Ad AdMobRewardedVideo Load Failed: " + p0.Message);
                base.OnAdFailedToLoad(p0);
            }
        }

        public static void Ad_RewardedVideo(Activity context)
        {
            try
            {
                if (AppTools.GetStatusAds() && AppSettings.ShowAdMobRewardVideo)
                {
                    if (CountRewarded == AppSettings.ShowAdRewardedVideoCount)
                    {
                        CountRewarded = 0;
                        AdMobRewardedVideo ads = new AdMobRewardedVideo();
                        ads.ShowAd(context);
                        return;
                    }
                    else
                    {
                        if (AppSettings.ShowFbInterstitialAds)
                            AdsFacebook.InitInterstitial(context);
                        else if (AppSettings.ShowAppLovinInterstitialAds)
                            AdsAppLovin.Ad_Interstitial(context);
                        else
                            AdsColony.Ad_Interstitial(context);
                    }

                    CountRewarded++;
                }
                else
                {
                    if (AppSettings.ShowFbInterstitialAds)
                        AdsFacebook.InitInterstitial(context);
                    else if (AppSettings.ShowAppLovinInterstitialAds)
                        AdsAppLovin.Ad_Interstitial(context);
                    else
                        AdsColony.Ad_Interstitial(context);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Banner

        public static void InitAdView(AdView mAdView, RecyclerView mRecycler)
        {
            try
            {
                if (mAdView == null) return;

                if (AppTools.GetStatusAds() && AppSettings.ShowAdMobBanner)
                {
                    mAdView.Visibility = ViewStates.Visible;
                    var adRequest = new AdRequest.Builder();
                    mAdView.LoadAd(adRequest.Build());
                    mAdView.AdListener = new MyAdListener(mAdView, mRecycler);
                }
                else
                {
                    mAdView.Pause();
                    mAdView.Visibility = ViewStates.Gone;
                    if (mRecycler != null) Methods.SetMargin(mRecycler, 0, 0, 0, 0);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void LifecycleAdView(AdView mAdView, string lifecycle)
        {
            try
            {
                if (mAdView == null) return;

                if (AppTools.GetStatusAds() && AppSettings.ShowAdMobBanner)
                {
                    switch (lifecycle)
                    {
                        case "Resume":
                            mAdView.Resume();
                            break;
                        case "Pause":
                            mAdView.Pause();
                            break;
                        case "Destroy":
                            mAdView.Destroy();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyAdListener : AdListener
        {
            private readonly AdView MAdView;
            private readonly RecyclerView MRecycler;
            public MyAdListener(AdView mAdView, RecyclerView mRecycler)
            {
                MAdView = mAdView;
                MRecycler = mRecycler;
            }

            public override void OnAdFailedToLoad(LoadAdError p0)
            {
                try
                {
                    Log.Debug("Google-Ads", "I_Ad AdView Load Failed: " + p0.Message);

                    MAdView.Visibility = ViewStates.Gone;
                    if (MRecycler != null) Methods.SetMargin(MRecycler, 0, 0, 0, 0);
                    base.OnAdFailedToLoad(p0);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }


            public override void OnAdLoaded()
            {
                try
                {
                    MAdView.Visibility = ViewStates.Visible;

                    Resources r = Application.Context.Resources;
                    int px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, MAdView.AdSize.Height, r.DisplayMetrics);
                    if (MRecycler != null) Methods.SetMargin(MRecycler, 0, 0, 0, px);

                    base.OnAdLoaded();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        #endregion

        #region Manager

        public static void InitAdManagerAdView(AdManagerAdView mAdView)
        {
            try
            {
                if (mAdView == null) return;

                if (AppTools.GetStatusAds() && AppSettings.ShowAdMobBanner)
                {
                    mAdView.Visibility = ViewStates.Visible;
                    var adRequest = new AdManagerAdRequest.Builder();
                    mAdView.AdListener = new MyAdManagerAdViewListener(mAdView);
                    mAdView.LoadAd(adRequest.Build());
                }
                else
                {
                    mAdView.Pause();
                    mAdView.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public static void LifecycleAdManagerAdView(AdManagerAdView mAdView, string lifecycle)
        {
            try
            {
                if (mAdView == null) return;

                if (AppTools.GetStatusAds() && AppSettings.ShowAdMobBanner)
                {
                    switch (lifecycle)
                    {
                        case "Resume":
                            mAdView.Resume();
                            break;
                        case "Pause":
                            mAdView.Pause();
                            break;
                        case "Destroy":
                            mAdView.Destroy();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyAdManagerAdViewListener : AdListener
        {
            private readonly AdManagerAdView MAdView;
            public MyAdManagerAdViewListener(AdManagerAdView mAdView)
            {
                MAdView = mAdView;
            }

            public override void OnAdFailedToLoad(LoadAdError p0)
            {
                try
                {
                    Log.Debug("Google-Ads", "I_Ad AdManagerAdView Load Failed: " + p0.Message);

                    MAdView.Visibility = ViewStates.Gone;
                    base.OnAdFailedToLoad(p0);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnAdLoaded()
            {
                try
                {
                    MAdView.Visibility = ViewStates.Visible;
                    base.OnAdLoaded();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        #endregion

        #region AppOpen

        public static void Ad_AppOpenManager(Activity context)
        {
            try
            {
                if (AppTools.GetStatusAds() && AppSettings.ShowAdMobAppOpen)
                {
                    if (CountAppOpen == AppSettings.ShowAdAppOpenCount)
                    {
                        CountAppOpen = 0;

                        AppOpenManager appOpenManager = new AppOpenManager(context);
                        appOpenManager.ShowAdIfAvailable();
                        return;
                    }
                    else
                    {
                        if (AppSettings.ShowFbInterstitialAds)
                            AdsFacebook.InitInterstitial(context);
                        else if (AppSettings.ShowAppLovinInterstitialAds)
                            AdsAppLovin.Ad_Interstitial(context);
                        else
                            AdsColony.Ad_Interstitial(context);
                    }

                    CountAppOpen++;
                }
                else
                {
                    if (AppSettings.ShowFbInterstitialAds)
                        AdsFacebook.InitInterstitial(context);
                    else if (AppSettings.ShowAppLovinInterstitialAds)
                        AdsAppLovin.Ad_Interstitial(context);
                    else
                        AdsColony.Ad_Interstitial(context);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private class AppOpenManager : AnjoAppOpenLoadCallback
        {
            private readonly Activity MostCurrentActivity;
            private static AppOpenAd Ad;

            public AppOpenManager(Activity context)
            {
                try
                {
                    MostCurrentActivity = context;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void ShowAdIfAvailable()
            {
                try
                {
                    AdRequest request = new AdRequest.Builder().Build();
                    AppOpenAd.Load(MostCurrentActivity, AppSettings.AdAdMobAppOpenKey, request, this);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnAdLoaded(AppOpenAd p0)
            {
                try
                {
                    base.OnAdLoaded(p0);

                    Ad = p0;
                    Ad.Show(MostCurrentActivity);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnAdFailedToLoad(LoadAdError p0)
            {
                Log.Debug("Google-Ads", "I_Ad AppOpenManager Load Failed: " + p0.Message);
                base.OnAdFailedToLoad(p0);
            }

        }

        #endregion

        #region RewardedInterstitial

        public class AdMobRewardedInterstitial : AnjoRewardedInterstitialLoadCallback
        {
            private Activity Context;
            public void ShowAd(Activity context)
            {
                try
                {
                    Context = context;

                    AdRequest adRequest = new AdRequest.Builder().Build();

                    // Use an activity context to get the rewarded video instance. 
                    RewardedInterstitialAd.Load(context, AppSettings.AdRewardedInterstitialKey, adRequest, this);
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnAdLoaded(RewardedInterstitialAd p0)
            {
                try
                {
                    p0?.Show(Context, new MyUserEarnedRewardListener(Context));
                    base.OnAdLoaded(p0);
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnAdFailedToLoad(LoadAdError p0)
            {
                Log.Debug("Google-Ads", "I_Ad AdMobRewardedInterstitial Load Failed: " + p0.Message);
                base.OnAdFailedToLoad(p0);
            }
        }

        public static void Ad_RewardedInterstitial(Activity context)
        {
            try
            {
                if (AppTools.GetStatusAds() && AppSettings.ShowAdMobRewardedInterstitial)
                {
                    if (CountRewardedInterstitial == AppSettings.ShowAdRewardedVideoCount)
                    {
                        CountRewardedInterstitial = 0;
                        AdMobRewardedInterstitial ads = new AdMobRewardedInterstitial();
                        ads.ShowAd(context);
                        return;
                    }
                    else
                    {
                        if (AppSettings.ShowFbRewardVideoAds)
                            AdsFacebook.InitRewardVideo(context);
                        else if (AppSettings.ShowAppLovinRewardAds)
                            AdsAppLovin.Ad_Rewarded(context);
                        else
                            AdsColony.Ad_Rewarded(context);
                    }

                    CountRewardedInterstitial++;
                }
                else
                {
                    if (AppSettings.ShowFbRewardVideoAds)
                        AdsFacebook.InitRewardVideo(context);
                    else if (AppSettings.ShowAppLovinRewardAds)
                        AdsAppLovin.Ad_Rewarded(context);
                    else
                        AdsColony.Ad_Rewarded(context);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private class MyUserEarnedRewardListener : Object, IOnUserEarnedRewardListener
        {
            private readonly Activity Activity;
            public MyUserEarnedRewardListener(Activity context)
            {
                Activity = context;
            }

            public void OnUserEarnedReward(IRewardItem rewardItem)
            {
                try
                {
                    // Handle the reward.
                    Console.WriteLine("The user earned the reward.");
                    int rewardAmount = rewardItem.Amount;
                    string rewardType = rewardItem.Type;

                    //if (!AppSettings.RewardedAdvertisingSystem)
                    //    return;

                    //if (!Methods.CheckConnectivity())
                    //    Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    //else
                    //{
                    //    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { RequestsAsync.Advertise.AddAdMobPointAsync });
                    //    Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_PointsAdded), ToastLength.Short)?.Show();
                    //}
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public static class InitializeAdsGoogle
        {
            public static void Initialize(Activity context)
            {
                try
                {
                    if (AppSettings.ShowAdMobBanner || AppSettings.ShowAdMobInterstitial || AppSettings.ShowAdMobRewardVideo || AppSettings.ShowAdMobNative || AppSettings.ShowAdMobAppOpen || AppSettings.ShowAdMobRewardedInterstitial)
                    {
                        RequestConfiguration configuration = new RequestConfiguration.Builder().SetTestDeviceIds(new List<string>() { "5BCFF0AAE83AF424648A954038C71DE6", "A5E3E2068BD88202CBC281AD76984BEE" ,
                           // AdRequest.DeviceIdEmulator
                            
                        }).Build();
                        MobileAds.RequestConfiguration = configuration;

                        MobileAds.Initialize(context, new MyInitializationCompleteListener(context));
                        MapsInitializer.Initialize(context);

                        // Set tag for under age of consent. false means users are not under age.
                        ConsentRequestParameters param = new ConsentRequestParameters.Builder()
                            .SetTagForUnderAgeOfConsent(false)
                            .Build();

                        var consentInformation = UserMessagingPlatform.GetConsentInformation(context);
                        var requestParameters = new MyConsentRequestParameters();
                        consentInformation.RequestConsentInfoUpdate(context, param, requestParameters, requestParameters);
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            private class MyInitializationCompleteListener : Object, IOnInitializationCompleteListener
            {
                private readonly Activity Context;
                public MyInitializationCompleteListener(Activity context)
                {
                    Context = context;
                }

                public void OnInitializationComplete(IInitializationStatus p0)
                {
                    try
                    {
                        AdsGoogle.AdMobNative ads = new AdsGoogle.AdMobNative();
                        ads.BindAdMobNative(Context);
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }
            }

            private class MyConsentRequestParameters : Object, IConsentInformationOnConsentInfoUpdateSuccessListener, IConsentInformationOnConsentInfoUpdateFailureListener
            {

                public void OnConsentInfoUpdateSuccess()
                {

                }

                public void OnConsentInfoUpdateFailure(FormError p0)
                {

                }
            }

        }
    }
}