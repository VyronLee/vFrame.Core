using System;
using System.Collections.Generic;

namespace vFrame.Core.Patch
{
    [Serializable]
    public class ManifestJson
    {
        public string buildNumber;
        public string engineVersion;
        public string assetsVersion;
        public List<AssetInfo> assets = new List<AssetInfo>();
    }
}