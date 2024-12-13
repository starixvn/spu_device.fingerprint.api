/* global Detecas */

Detecas.RTC = (function (window, json, cookie, debug, rc4, base64) {
    "use strict";

    if (fpsIsMob) {
        return;
    }

    var addresses = {};
    addresses["0.0.0.0"] = false;

    // refer: http://net.ipcalf.com/
    var webRTC = (function () {
        var getLocalIpAddress = function (cb) {
            cb = cb || function () { };

            try {
                // NOTE: window.RTCPeerConnection is "not a constructor" in FF22/23
                var rtcPeerConnection = window.webkitRTCPeerConnection || window.PeerConnection;
                if (!rtcPeerConnection) {
                    return false;
                }

                var returnLocalIpAddress = function () {
                    var actualIpAddresses = [];
                    for (var key in addresses) {
                        if (addresses.hasOwnProperty(key)) {
                            var value = addresses[key];
                            if (value) {
                                actualIpAddresses.push(key);
                            }
                        }
                    }

                    return actualIpAddresses;
                };

                var updateDisplay = function (newAddress) {
                    if (newAddress in addresses) {
                        return;
                    } else {
                        addresses[newAddress] = true;
                    }

                    cb(returnLocalIpAddress());
                };

                var processSessionDescriptionProtocol = function (sessionDecriptionProtocol) {
                    sessionDecriptionProtocol.split("\r\n").forEach(function (line) {
                        // c.f. http://tools.ietf.org/html/rfc4566#page-39
                        var parts;
                        var addr;
                        if (~line.indexOf("a=candidate")) {
                            // http://tools.ietf.org/html/rfc4566#section-5.13
                            parts = line.split(" ");

                            // http://tools.ietf.org/html/rfc5245#section-15.1
                            addr = parts[4];
                            var type = parts[7];
                            if (type === "host") {
                                updateDisplay(addr);
                            }
                        } else if (~line.indexOf("c=")) {
                            // http://tools.ietf.org/html/rfc4566#section-5.7
                            parts = line.split(" ");
                            addr = parts[2];
                            updateDisplay(addr);
                        }
                    });
                };

                var rtc = new rtcPeerConnection({
                    iceServers: []
                });

                rtc.createDataChannel("", {
                    reliable: false
                });

                rtc.onicecandidate = function (event) {
                    if (event.candidate) {
                        processSessionDescriptionProtocol("a=" + event.candidate.candidate);
                    }
                };

                rtc.createOffer(function (offerDescription) {
                    processSessionDescriptionProtocol(offerDescription.sdp);
                    rtc.setLocalDescription(offerDescription);
                }, function (e) { });

                return true;
            }
            catch (exception) {
                //debug.addException(exception, "Detecas.RTC.webRTC");
            }

            return false;
        };

        return {
            getLocalIpAddress: getLocalIpAddress
        };
    })();

    // refer: https://webrtchacks.com/first-steps-ortc/
    var oRTC = (function () {
        var getLocalIpAddress = function (cb) {
            var rtcIceGatherer = window.RTCIceGatherer;
            if (!rtcIceGatherer) {
                return false;
            }

            try {
                var addresses = [];
                var iceOptions = { "gatherPolicy": "all", "iceServers": [] };
                var rtc = new rtcIceGatherer(iceOptions);
                rtc.onlocalcandidate = function (evt) {
                    var candidate = evt.candidate;
                    if (candidate) {
                        if (candidate.type === "host") {
                            var ip = candidate.ip;
                            addresses.push(ip);
                            cb(addresses);
                        }
                    }
                };
            }
            catch (exception) {
                //debug.addException(exception, "Detecas.RTC.oRTC");
            }
        };

        return {
            getLocalIpAddress: getLocalIpAddress
        };
    })();

    var init = function () {
        try {
            // read local-ip from cookie
            var cookieName = "lip";
            var cookieValue = cookie.read(cookieName);
            if (cookieValue) {
                var localIps = json.parse(rc4.decrypt("PI", base64.decode(cookieValue)));
                Detecas.RTC.localIps = localIps;
            }

            var callback = function (value) {
                if (Object.prototype.toString.call(value) === '[object Array]' && value.length > 0) {
                    Detecas.RTC.localIps = json.prune(value);

                    // save local-ip into cookie
                    var cookieValue = base64.encode(rc4.encrypt("PI", json.stringify(Detecas.RTC.localIps)));
                    cookie.write(cookieName, cookieValue);
                }
            };

            webRTC.getLocalIpAddress(callback) || oRTC.getLocalIpAddress(callback);
        }
        catch (exception) {
            //debug.addException(exception, "Detecas.RTC.init");
        }
    };

    return {
        init: init
    };
} (window, Detecas.JSON, Detecas.Cookie, Detecas.Debug, Detecas.Rc4, Detecas.Base64));