var GfpPickle = {}, GfpSpot = {};

GfpPickle.Config = {
    host: "__host__", // Current host of script
    cdn: "__cdn__",
    ip: "127.0.0.1", // Client ip
    encodedIp: "2130706433", // Encoded Client ip
    cookieStorage: "__cookieStorage__", // Storage name
    version: "__version__", // Server script version
    ep: "__encryptpassword__",
    sep: "__separator__",
    na: "__na__",
    storages: "__storages__",
    cachedDeviceId: "__cachedDeviceId__", // Cached DeviceId (get from Cross-domain cookie data for device identifier)
    defaultDeviceId: "__defaultDeviceId__", // New device id or get value of cachedDeviceId.
    accept: "__accept__", // Http Accept
    headers: "__headers__", // Http Headers
    capturedDate: "__capturedDate__", // DeviceId's captured date
};

GfpSpot.Config = {
    apiUrl: "__apiUrl__",
    portalAPIUrl: "__portalAPIUrl__",
    version: "202406211000",
    encodeKey: "__key__",
    sessionKey: "__sessionKey__",
    dbIndex: "__dbIndex__",
    jsChallengeAPIUrl: "__jsChallengeAPIUrl__",
    jsChallengeVersion: "__jsChallengeVersion__",
    fdiElementKey: "__fdi",
    storageKey: "gfp"
};