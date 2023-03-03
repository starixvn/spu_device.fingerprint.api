/* global Detecas */

Detecas.Common = (function (document, hex) {
    "use strict";

    function isValidObject(obj) {
        return typeof (obj) !== "undefined" && !!obj;
    }

    function isValidDeviceId(deviceId) {
        return 100 > deviceId.length && 10 < deviceId.length && hex.decode(deviceId).length === 32; // Valid device cookie
    }

    function createTransparentDomElement(type, id) {
        var element = id !== undefined && document.getElementById(id)
            ? document.getElementById(id)
            : document.createElement(type);

        try {
            element.style.position = "absolute";
            element.style.width = "1px";
            element.style.height = "1px";
            element.style.overflow = "hidden";
            element.style.display = "block";
            element.style.zIndex = "-10";

            // Transparent
            element.filter = "alpha(opacity = 1)";
            element.MozOpacity = 0.1;
            element.KhtmlOpacity = 0.1;
            element.style.opacity = 0.1;
        } catch (exception) {
            //debug.addException(exception, "Detecas.Common.createTransparentDomElement");
        }

        if (id && element) {
            element.setAttribute("id", id);
        }

        return element;
    }

})(document, Detecas.Hex);