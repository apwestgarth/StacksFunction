namespace ApplicationStack
{
    public class AvailableStackList
    {
        public List<Stack> value { get; set; }
        public object nextLink { get; set; }
        public object id { get; set; }
    }

    public class Stack
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public string? type { get; set; }
        public Properties? properties { get; set; }

    }

    public class Properties
    {
        public string? name { get; set; }
        public string? displayText { get; set; }
        public string? dependency { get; set; }
        public List<Majorversions> majorVersions { get; set; }
        public List<Framework>? frameworks { get; set; }
        public bool? isDeprecated { get; set; }
        public string preferredOS { get; set; }
    }

    public class Majorversions
    {
        public string? displayText { get; set; }
        public string? value { get; set; }
        public bool isDefault { get; set; }
        public List<Minorversions> minorVersions { get; set; }
        public bool applicationInsights { get; set; }
        public bool isPreview { get; set; }
        public bool isDeprecated { get; set; }
        public bool isHidden { get; set; }
        public Appsettingsdictionary? appSettingsDictionary { get; set; }
        public Siteconfigpropertiesdictionary? siteConfigPropertiesDictionary { get; set; }
    }

    public class Appsettingsdictionary
    {
    }

    public class Siteconfigpropertiesdictionary
    {
    }

    public class Minorversions
    {
        public string displayText { get; set; }
        public string value { get; set; }
        public bool isDefault { get; set; }
        public bool isRemoteDebuggingEnabled { get; set; }
        public StackSettings stackSettings { get; set; }
    }

    public class Framework
    {
        public string? name { get; set; }
        public string? display { get; set; }
        public string? dependency { get; set; }
        public List<Majorversions> majorVersions { get; set; }
        public object frameworks { get; set; }
        public object isDeprecated { get; set; }
    }

    public class StackSettings
    {
        public LinuxRuntimeSettings linuxRuntimeSettings { get; set; }
        public LinuxContainerSettings linuxContainerSettings { get; set; }
    }

    public class LinuxContainerSettings
    {
        public string java21Runtime { get; set; }
        public string java17Runtime { get; set; }
        public string java11Runtime { get; set; }
        public string java8Runtime { get; set; }
        public bool isAutoUpdate { get; set; }
        public bool? isHidden { get; set; }
        public bool? isDeprecated { get; set; }
        public DateTime? endOfLifeDate { get; set; }
    }

    public class LinuxRuntimeSettings
    {
        public string runtimeVersion { get; set; }
        public bool remoteDebuggingSupported { get; set; }
        public AppInsightsSettings appInsightsSettings { get; set; }
        public GitHubActionSettings gitHubActionSettings { get; set; }
        public SupportedFeatures supportedFeatures { get; set; }
        public DateTime? endOfLifeDate { get; set; }
        public bool? isDeprecated { get; set; }
        public bool? isHidden { get; set; }
        public bool? isEarlyAccess { get; set; }
        public bool? isAutoUpdate { get; set; }
    }

    public class AppInsightsSettings
    {
        public bool isSupported { get; set; }
        public bool isDefaultOff { get; set; }
    }

    public class GitHubActionSettings
    {
        public bool isSupported { get; set; }
        public string supportedVersion { get; set; }
        public bool? notSupportedInCreates { get; set; }
    }

    public class SupportedFeatures
    {
        public bool disableSsh { get; set; }
    }
}

