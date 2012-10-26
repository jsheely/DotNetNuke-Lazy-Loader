using System;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using TwentyTech.LazyLoader.Components.Entities;

namespace TwentyTech.LazyLoader.Components.Configuration
{
    public class LazyLoaderConfiguration
    {
        private static bool _Enabled;
        public static FullPageCache FullPageCache = new FullPageCache();
        public static ImageSrcSwap ImageSrcSwap = new ImageSrcSwap();
        private static bool isInitialized = false;

        public enum CacheLocations
        {
            none,
            file,
            memory
        }

        public static bool Enabled
        {
            get 
            {
                if(isInitialized)
                {
                    return _Enabled;
                }
                else {
                    Initialize();
                    return _Enabled;
                }
                
                
            }

        }

         

        public static void Initialize()
        {
            //if (!HttpContext.Current.IsDebuggingEnabled)
            //{
            //    isInitialized = true;    
            //}

            isInitialized = true;    
            
            XDocument settings = XDocument.Load(XmlReader.Create(new StringReader(File.ReadAllText(HttpContext.Current.Server.MapPath("~/TwentyTech.LazyLoader.config")))));
            var query = from s in settings.Descendants("TwentyTech").Descendants("LazyLoader") select s;

            
            _Enabled = Convert.ToBoolean(query.Elements("Enabled").Single().Value);
            
            //Full Page Settings
            foreach(var v in query.Elements("FullPageCache"))
            {
                FullPageCache.Enabled = Convert.ToBoolean(v.Element("Enabled").Value);
                FullPageCache.CacheDuration=v.Element("CacheDuration").Value;
                FullPageCache.CacheLocation=v.Element("CacheLocation").Value;
                if (v.Element("IncludePages").Value != "")
                {
                    if (v.Element("IncludePages").Value.Contains(","))
                    {
                        FullPageCache.IncludePages.AddRange(v.Element("IncludePages").Value.Split(','));
                    }
                    else
                    {
                        FullPageCache.IncludePages.Add(v.Element("IncludePages").Value);
                    }
                }
                if (v.Element("ExcludePages").Value != "")
                {
                    if (v.Element("ExcludePages").Value.Contains(","))
                    {
                        FullPageCache.ExcludePages.AddRange(v.Element("ExcludePages").Value.Split(','));
                    }
                    else
                    {
                        FullPageCache.ExcludePages.Add(v.Element("ExcludePages").Value);
                    }
                }
            }

            
            //ImageSwap
            foreach(var v in query.Elements("ImageSrcSwap"))
            {
                ImageSrcSwap.Enabled = Convert.ToBoolean(v.Element("Enabled").Value);
                ImageSrcSwap.EnabledWhenLoggedIn = Convert.ToBoolean(v.Element("EnabledWhenLoggedIn").Value);
                if (v.Element("IncludePages").Value != "")
                {
                    if (v.Element("IncludePages").Value.Contains(","))
                    {
                        ImageSrcSwap.IncludePages.AddRange(v.Element("IncludePages").Value.Split(','));
                    }
                    else
                    {
                        ImageSrcSwap.IncludePages.Add(v.Element("IncludePages").Value);
                    }
                }
                if (v.Element("ExcludePages").Value != "")
                {
                    if (v.Element("ExcludePages").Value.Contains(","))
                    {
                        ImageSrcSwap.ExcludePages.AddRange(v.Element("ExcludePages").Value.Split(','));
                    }
                    else
                    {
                        ImageSrcSwap.ExcludePages.Add(v.Element("ExcludePages").Value);
                    }
                }
            }
            
            

        }

    }
}