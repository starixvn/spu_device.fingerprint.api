using Fps.Device.FingerPrint.Features.FingerPrint.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceFingerprint.WebAPI.Services
{
    public static class InitServices
    {
        public static void RegisterService(this IServiceCollection services)
        {
            RegisterFingerPrintService(services);
        }

        private static void RegisterFingerPrintService(this IServiceCollection services)
        {
            services.AddSingleton<IFingerPrintDataAccess, FingerPrintDataAccess>();
            services.AddSingleton<IFingerPrintService, FingerPrintService>();
        }
    }
}
