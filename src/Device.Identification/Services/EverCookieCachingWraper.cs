namespace Device.Identification.Services
{
    using Device.Identification.Factory;
    using Microsoft.AspNetCore.Http;

    public class EverCookieCachingWrapper(IHttpContextAccessor contextAccessor) : HttpCachingWrapper(contextAccessor)
    {
        public bool DoesRecoverDeviceIdFromETag()
        {
            return DeviceFactory.IsValidDeviceId(ETag);
        }
    }
}
