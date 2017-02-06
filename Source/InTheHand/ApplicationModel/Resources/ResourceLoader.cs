﻿//-----------------------------------------------------------------------
// <copyright file="ResourceLoader.cs" company="In The Hand Ltd">
//     Copyright © 2013-17 In The Hand Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Reflection;
using System.Resources;
using System.Threading;
#if WINDOWS_PHONE
using System.Windows;
#endif

namespace InTheHand.ApplicationModel.Resources
{
    /// <summary>
    /// Provides simplified access to app resources such as app UI strings.
    /// </summary>
    /// <remarks>
    /// <list type="table">
    /// <listheader><term>Platform</term><description>Version supported</description></listheader>
    /// <item><term>Windows UWP</term><description>Windows 10</description></item>
    /// <item><term>Windows Store</term><description>Windows 8.1 or later</description></item>
    /// <item><term>Windows Phone Store</term><description>Windows Phone 8.1 or later</description></item>
    /// <item><term>Windows Phone Silverlight</term><description>Windows Phone 8.0 or later</description></item></list>
    /// </remarks>
    public sealed class ResourceLoader
    {
        private static ResourceLoader instance;

        /// <summary>
        /// Gets a ResourceLoader object for the Resources subtree of the currently running app's main ResourceMap. 
        /// This ResourceLoader uses a default context associated with the current view.
        /// </summary>
        /// <returns></returns>
        public static ResourceLoader GetForCurrentView()
        {
#if WINDOWS_UWP || WINDOWS_APP || WINDOWS_PHONE_APP
            var rl = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            return rl == null ? null : new Resources.ResourceLoader(rl);
#elif WINDOWS_PHONE
            if (instance == null)
            {
                // the trick is the logic to find the default resource set. this assumes using the Resources.AppResources generated by WP8 templates
                instance = new ResourceLoader(new ResourceManager(Deployment.Current.EntryPointType.Substring(0, Deployment.Current.EntryPointType.LastIndexOf('.')) + "Resources.AppResources", Assembly.Load(Deployment.Current.EntryPointAssembly)));
            }

            return instance;
#else
            throw new PlatformNotSupportedException();
#endif
        }

        /// <summary>
        /// Gets a ResourceLoader object for the Resources subtree of the currently running app's main ResourceMap.
        /// This ResourceLoader uses a default context that's not associated with any view.
        /// </summary>
        /// <returns></returns>
        public static ResourceLoader GetForViewIndependentUse()
        {
#if WINDOWS_UWP || WINDOWS_APP || WINDOWS_PHONE_APP
            var rl = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
            return rl == null ? null : new Resources.ResourceLoader(rl);
#else
            return GetForCurrentView();
#endif
        }

#if WINDOWS_UWP || WINDOWS_APP || WINDOWS_PHONE_APP
        private Windows.ApplicationModel.Resources.ResourceLoader _loader;

        private ResourceLoader(Windows.ApplicationModel.Resources.ResourceLoader loader)
        {
            _loader = loader;
        }

        public static implicit operator Windows.ApplicationModel.Resources.ResourceLoader(ResourceLoader rl)
        {
            return rl._loader;
        }

        public static implicit operator ResourceLoader(Windows.ApplicationModel.Resources.ResourceLoader rl)
        {
            return new Resources.ResourceLoader(rl);
        }
#else
        private ResourceManager _resourceManager;

        private ResourceLoader(ResourceManager manager)
        {
            _resourceManager = manager;
        }
#endif

        /// <summary>
        /// Returns the most appropriate string value of a resource, specified by resource identifier, for the default ResourceContext of the view in which the ResourceLoader was obtained using ResourceLoader.GetForCurrentView.
        /// </summary>
        /// <param name="resource">The resource identifier of the resource to be resolved.</param>
        /// <returns>The most appropriate string value of the specified resource for the default ResourceContext.</returns>
        public string GetString(string resource)
        {
#if __ANDROID__ || __IOS__ || WINDOWS_PHONE
            if(Thread.CurrentThread.CurrentCulture == null)
            {
                Thread.CurrentThread.CurrentCulture = global::System.Globalization.CultureInfo.DefaultThreadCurrentCulture;
            }

            return _resourceManager.GetString(resource);
#elif WINDOWS_UWP || WINDOWS_APP || WINDOWS_PHONE_APP
            return _loader.GetString(resource);
#else
            return null;
#endif
        }
    }
}