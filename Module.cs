using System;
using System.Web;
using System.IO;
using System.Web.UI;
using DotNetNuke;
using System.Text;
using System.Text.RegularExpressions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Common;
using System.Collections;
using System.Collections.Generic;
namespace TwentyTech.LazyLoader
{
    public class Module : IHttpModule
    {

        private PortalSettings _myPortalSettings;
        private PortalSettings myPortalSettings
        {
            get { return _myPortalSettings; }
            set { _myPortalSettings = value; }
        }
        private CacheOptions curCacheOptions;
        private CacheStatus curCacheStatus;
        private enum CacheOptions
        {
            None,
            FilterOutput,
            CacheExists,
            DisableCache

        }
        private enum CacheStatus
        {
            Unknown,
            Active,
            Expired
        }

        public void Dispose()
        {
            //clean-up code here.
        }
        public void Init(HttpApplication context)
        {
            if (LazyLoaderConfiguration.Enabled)
            {
                context.BeginRequest += new EventHandler(OnBeginRequest);
                context.EndRequest += new EventHandler(OnEndRequest);
            }

        }
        public void OnBeginRequest(Object source, EventArgs e)
        {
            HttpApplication app = source as HttpApplication;
            if (app != null)
            {

                if (app.Request.Url.ToString().Contains(".aspx") && !isSearchEngine(app))
                {

                    string myAlias = Globals.GetDomainName(app.Request, true);
                    PortalAliasInfo objPortalAlias;
                    objPortalAlias = PortalAliasController.GetPortalAliasInfo(myAlias);
                    do
                    {
                        objPortalAlias = PortalAliasController.GetPortalAliasInfo(myAlias);

                        if (objPortalAlias != null)
                        {
                            break;
                        }

                        int slashIndex = myAlias.LastIndexOf('/');
                        if (slashIndex > 1)
                        {
                            myAlias = myAlias.Substring(0, slashIndex);
                        }
                        else
                        {
                            myAlias = "";
                        }
                    } while (myAlias.Length > 0);

                    int portalId = objPortalAlias.PortalID;
                    int tabid = 0;
                    if (app.Request.QueryString["tabId"] != null)
                    {
                        tabid = Convert.ToInt32(app.Request.QueryString["tabId"]);
                    }
                    else
                    {
                        string tabPath = (app.Request.Url.Host + app.Request.Url.AbsolutePath).Replace(myAlias, "").Replace("/", "//").Replace(".aspx", "");
                        if (tabPath.ToLower() == "//default")
                        {
                            tabid = new PortalSettings(portalId).HomeTabId;
                        }
                        else
                        {
                            tabid = DotNetNuke.Entities.Tabs.TabController.GetTabByTabPath(portalId, tabPath,"en-US");
                        }


                    }


                    if (tabid > 0)
                    {
                        myPortalSettings = new PortalSettings(tabid, portalId);


                        //Will need futher exceptions on clearing the cache.
                        //In order to use this we need to determine if the user is logged in.
                        if (ValidateCache(app))
                        {
                            //Need more definition when Response.End() runs
                            curCacheOptions = CacheOptions.CacheExists;
                            app.Response.End();
                        }
                        else
                        {
                            app.Response.Filter = new MyRewriterStream(app.Response.Filter);
                            curCacheOptions = CacheOptions.FilterOutput;

                        }
                    }
                    else
                    {
                        curCacheOptions = CacheOptions.DisableCache;
                    }
                }


            }
        }
        public void OnEndRequest(Object source, EventArgs e)
        {
            HttpApplication app = source as HttpApplication;
            if (app != null)
            {
                if (app.Request.Url.ToString().Contains(".aspx") && !isSearchEngine(app))
                {
                    if (curCacheOptions == CacheOptions.FilterOutput || curCacheOptions == CacheOptions.CacheExists)
                    {

                        app.Response.Clear();

                        string outputStr = MyRewriterStream.fullContent;

                        if (ValidateCache(app))
                        {
                            //Redundant check to make sure the cache actually still exists.
                            if (File.Exists(app.Server.MapPath("~/Cache/" + myPortalSettings.ActiveTab.TabID.ToString() + ".resources")))
                            {
                                outputStr = File.ReadAllText(app.Server.MapPath("~/Cache/" + myPortalSettings.ActiveTab.TabID.ToString() + ".resources"));
                            }
                        }
                        else
                        {
                            if (outputStr.Contains("<head>") || outputStr.Contains("<head id=\"Head\">"))
                            {
                                ImageSrcSwap(ref outputStr, app);
                            }

                            fullPageCache(outputStr, app);

                        }

                        app.Response.Write(outputStr);
                    }
                }
            }
        }

        private bool ValidateCache(HttpApplication app)
        {
            if (LazyLoaderConfiguration.FullPageCache.Enabled)
            {
                if (app.Request.HttpMethod != "POST")
                {
                    if (!app.Request.IsAuthenticated && app.Request.Cookies[".DOTNETNUKE"] == null)
                    {
                        if (app.Request.QueryString.ToString().ToLower() == "tabid=" + myPortalSettings.ActiveTab.TabID.ToString() || app.Request.QueryString.ToString() == "")
                        {
                            //Exclude by TabID check would go here.
                            return ValidateCacheStatus(app);


                        }
                    }
                }
            }
            return false;
        }
        private bool ValidateCacheStatus(HttpApplication app)
        {
            if (File.Exists(app.Server.MapPath("~/Cache/" + myPortalSettings.ActiveTab.TabID.ToString() + ".resources")))
            {
                DateTime dateCreated = File.GetLastWriteTime(app.Server.MapPath("~/Cache/" + myPortalSettings.ActiveTab.TabID.ToString() + ".resources"));
                string[] cacheDurList = LazyLoaderConfiguration.FullPageCache.CacheDuration.Split(':');
                DateTime cacheLimit = DateTime.Now.AddDays(-Convert.ToInt32(cacheDurList[0]));
                cacheLimit = cacheLimit.AddHours(-Convert.ToInt32(cacheDurList[1]));
                cacheLimit = cacheLimit.AddMinutes(-Convert.ToInt32(cacheDurList[2]));
                cacheLimit = cacheLimit.AddSeconds(-Convert.ToInt32(cacheDurList[3]));
                if (dateCreated > cacheLimit)
                {
                    curCacheStatus = CacheStatus.Active;
                    return true;
                }
                else
                {
                    curCacheStatus = CacheStatus.Expired;
                    return false;
                }

            }
            curCacheStatus = CacheStatus.Unknown;
            return false;
        }
        private void ImageSrcSwap(ref string outputStr, HttpApplication app)
        {

            if (!LazyLoaderConfiguration.ImageSrcSwap.Enabled)
            {
                return;
            }
            if (LazyLoaderConfiguration.ImageSrcSwap.EnabledWhenLoggedIn == false && app.Request.IsAuthenticated)
            {
                return;
            }
            if (LazyLoaderConfiguration.ImageSrcSwap.IncludePages.Count > 0)
            {
                bool isValidPage = false;
                foreach (string url in LazyLoaderConfiguration.ImageSrcSwap.IncludePages)
                {
                    int tabId;
                    if (int.TryParse(url, out tabId))
                    {
                        if (tabId == myPortalSettings.ActiveTab.TabID)
                        {
                            isValidPage = true;
                        }
                    }
                    else
                    {
                        if (System.Web.VirtualPathUtility.ToAbsolute(url).ToLower() == app.Request.RawUrl.ToLower())
                        {
                            isValidPage = true;
                        }
                    }
                }

                if (!isValidPage)
                {
                    return;
                }
            }

            if (LazyLoaderConfiguration.ImageSrcSwap.ExcludePages.Count > 0)
            {
                bool isValidPage = true;
                foreach (string url in LazyLoaderConfiguration.ImageSrcSwap.ExcludePages)
                {
                    int tabId;
                    if (int.TryParse(url, out tabId))
                    {
                        if (tabId == myPortalSettings.ActiveTab.TabID)
                        {
                            isValidPage = false;
                        }
                    }
                    else
                    {
                        if (System.Web.VirtualPathUtility.ToAbsolute(url).ToLower() == app.Request.RawUrl.ToLower())
                        {
                            isValidPage = false;
                        }
                    }
                }

                if (!isValidPage)
                {
                    return;
                }
            }

            Regex imageRegEx = new Regex("<img.+?>");

            MatchCollection matches = imageRegEx.Matches(outputStr);

            outputStr = outputStr.Insert(outputStr.IndexOf("</head>"), "<script type=\"text/javascript\" src=\"" + System.Web.VirtualPathUtility.ToAbsolute("~/DesktopModules/TwentyTech/LazyLoader/lazyloader.js") + "\"></script>");

            foreach (Match image in matches)
            {
                Regex SrcRegEx = new Regex("src=[\"'].+?[\"']");
                string src = SrcRegEx.Match(image.Value).Value;
                if (!src.Contains("1x1.gif") && src!="")
                {
                    string modImage = image.Value.Replace(src, "src=\"" + System.Web.VirtualPathUtility.ToAbsolute("~/") + "images/1x1.gif\" originalsrc=" + src.Replace("src=", ""));
                    outputStr = outputStr.Replace(image.Value, modImage);
                }
            }
        }
        private void fullPageCache(string outputStr, HttpApplication app)
        {
            if (LazyLoaderConfiguration.FullPageCache.Enabled)
            {

                if (LazyLoaderConfiguration.FullPageCache.IncludePages.Count > 0)
                {
                    bool isValidPage = false;
                    foreach (string url in LazyLoaderConfiguration.FullPageCache.IncludePages)
                    {
                        int tabId;
                        if (int.TryParse(url, out tabId))
                        {
                            if (tabId == myPortalSettings.ActiveTab.TabID)
                            {
                                isValidPage = true;
                            }
                        }
                        else
                        {
                            if (System.Web.VirtualPathUtility.ToAbsolute(url).ToLower() == app.Request.RawUrl.ToLower())
                            {
                                isValidPage = true;
                            }
                        }
                    }

                    if (!isValidPage)
                    {
                        return;
                    }
                }

                if (LazyLoaderConfiguration.FullPageCache.ExcludePages.Count > 0)
                {
                    bool isValidPage = true;
                    foreach (string url in LazyLoaderConfiguration.FullPageCache.ExcludePages)
                    {
                        int tabId;
                        if (int.TryParse(url, out tabId))
                        {
                            if (tabId == myPortalSettings.ActiveTab.TabID)
                            {
                                isValidPage = false;
                            }
                        }
                        else
                        {
                            if (System.Web.VirtualPathUtility.ToAbsolute(url).ToLower() == app.Request.RawUrl.ToLower())
                            {
                                isValidPage = false;
                            }
                        }
                    }

                    if (!isValidPage)
                    {
                        return;
                    }
                }


                if (app.Request.HttpMethod != "POST")
                {
                    if (!app.Request.IsAuthenticated && app.Request.Cookies[".DOTNETNUKE"] == null)
                    {
                        if (app.Request.QueryString.ToString().ToLower() == "tabid=" + myPortalSettings.ActiveTab.TabID.ToString() || app.Request.QueryString.ToString() == "")
                        {
                            if (LazyLoaderConfiguration.FullPageCache.CacheLocation == LazyLoaderConfiguration.CacheLocations.file.ToString())
                            {
                                string cachePath = app.Server.MapPath("~/Cache/" + myPortalSettings.ActiveTab.TabID.ToString() + ".resources");
                                if (curCacheStatus == CacheStatus.Expired || curCacheStatus == CacheStatus.Unknown)
                                {
                                    if (File.Exists(cachePath))
                                    {
                                        File.Delete(cachePath);
                                    }
                                    if (!Directory.Exists(app.Server.MapPath("/Cache")))
                                    {
                                        Directory.CreateDirectory(app.Server.MapPath("~/Cache"));
                                    }

                                    outputStr = RemoveViewState(outputStr);

                                    StreamWriter sw = new StreamWriter(cachePath);
                                    sw.Write(outputStr);
                                    sw.Close();
                                    //File.WriteAllText(cachePath, outputStr);
                                }
                            }
                            else if (LazyLoaderConfiguration.FullPageCache.CacheLocation == LazyLoaderConfiguration.CacheLocations.memory.ToString())
                            {
                                //if(memoryCache.ContainsKey(myPortalSettings.ActiveTab.TabID)){
                                //    memoryCache.Remove(myPortalSettings.ActiveTab.TabID);
                                //}
                                //memoryCache.Add(myPortalSettings.ActiveTab.TabID,outputStr);
                            }
                        }
                    }
                }
            }

        }

        private string RemoveViewState(string data)
        {
            data = data.Replace(new Regex("<input type=\"hidden\" name=\"__VIEWSTATE\".+? />").Match(data).Value, "");
            return data;
        }

        private bool isSearchEngine(HttpApplication app)
        {
            Regex regEx = new Regex("googlebot|msnbot|baidu|curl|wget|Mediapartners-Google|slurp|ia_archiver|Gigabot|libwww-perl|lwp-trivial");
            if (regEx.IsMatch(app.Request.UserAgent.ToLower()))
            {
                return true;
            }
            else { return false;}
        }

        private Dictionary<int, string> memoryCache = new Dictionary<int, string>();

    }
    
}
