using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceFingerprint.WebAPI.Services
{
    public interface IJWTAuthenticationManager
    {
        string GenerateToken(int subscriberId);
    }
}
