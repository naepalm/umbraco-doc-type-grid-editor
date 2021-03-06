﻿using System;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace Our.Umbraco.DocTypeGridEditor.Web.Helpers
{
    internal static class SurfaceControllerHelper
    {
        public static bool SurfaceControllerExists(string controllerName, string actionName = "Index")
        {
            using (var timer = ApplicationContext.Current.ProfilingLogger.DebugDuration<Bootstrap>(string.Format("SurfaceControllerExists ({0}, {1})", controllerName, actionName)))
            {
                // Setup dummy route data
                var rd = new RouteData();
                rd.DataTokens.Add("area", "umbraco");
                rd.DataTokens.Add("umbraco", "true");

                // Setup dummy request context
                var rc = new RequestContext(
                    new HttpContextWrapper(HttpContext.Current),
                    rd);

                // Get controller factory
                var cf = ControllerBuilder.Current.GetControllerFactory();

                // Try and create the controller
                try
                {
                    var ctrl = cf.CreateController(rc, controllerName);
                    if (ctrl == null)
                        return false;

                    var ctrlInstance = ctrl as SurfaceController;
                    if (ctrlInstance == null)
                        return false;

                    foreach (var method in ctrlInstance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => typeof(ActionResult).IsAssignableFrom(x.ReturnType)))
                    {
                        if (method.Name.InvariantEquals(actionName))
                        {
                            return true;
                        }

                        var attr = method.GetCustomAttribute<ActionNameAttribute>();
                        if (attr != null && attr.Name.InvariantEquals(actionName))
                        {
                            return true;
                        }
                    }

                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool SurfaceControllerExists(string name, string actionName = "Index", bool cacheResult = true)
        {
            if (!cacheResult)
                return SurfaceControllerExists(name, actionName);

            return (bool)ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(
                string.Join("_", new[] { "Our.Umbraco.DocTypeGridEditor.Web.Helpers.SurfaceControllerHelper.SurfaceControllerExists", name, actionName }),
                () => SurfaceControllerExists(name, actionName));
        }
    }
}