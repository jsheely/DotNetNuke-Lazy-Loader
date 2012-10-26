using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TwentyTech.LazyLoader.Components.Entities
{
    public class FullPageCache
    {

        private bool _Enabled;
        private string _CacheLocation;
        private string _CacheDuration;
        private List<string> _IncludePages = new List<string>();
        private List<string> _ExcludePages = new List<string>();

        public bool Enabled
        {
            get
            {
                return _Enabled;
            }
            set
            {
                _Enabled = value;
            }
        }
        public string CacheLocation
        {
            get
            {
                return _CacheLocation;
            }
            set
            {
                _CacheLocation = value;
            }
        }
        public string CacheDuration
        {
            get
            {
                return _CacheDuration;
            }
            set
            {
                _CacheDuration = value;
            }
        }
        public List<string> IncludePages
        {
            get { return _IncludePages; }
        }
        public List<string> ExcludePages
        {
            get { return _ExcludePages; }
        }
    }
    
    public class ImageSrcSwap
    {
        public ImageSrcSwap(){}


        private bool _Enabled;
        private bool _EnabledWhenLoggedIn;
        private List<string> _IncludePages = new List<string>();
        private List<string> _ExcludePages = new List<string>();

        public bool Enabled
        {
            get
            {
                return _Enabled;
            }
            set
            {
                _Enabled = value;
            }
        }
        public bool EnabledWhenLoggedIn
        {
            get { return _EnabledWhenLoggedIn; }
            set { _EnabledWhenLoggedIn = value; }
        }
        public List<string> IncludePages
        {
            get { return _IncludePages; }
        }
        public List<string> ExcludePages
        {
            get { return _ExcludePages; }
        }
    }
}