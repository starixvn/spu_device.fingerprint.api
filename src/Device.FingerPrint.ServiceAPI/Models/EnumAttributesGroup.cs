namespace Device.FingerPrint.ServiceAPI.Models
{
    public enum ForbiddenChanges
    {
        LocalStorage,
        DoNotTrack,
        Cookies,
        Canvas
    }

    public enum AllowChangesWithSim
    {
        UserAgent,
        Vendor,
        GLRenderer,
        Plugins,
        Languages
    }

    public enum AllowChanges
    {
        Resolution,
        Timezone,
        GLRenderer
    }
}
