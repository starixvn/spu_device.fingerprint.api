/* global Detecas */

Detecas.Hex = (function () {
    "use strict";

    // refer: http://stackoverflow.com/questions/21647928/javascript-unicode-string-to-hex
    function encode(str) {
        var hex = "";
        for (var i = 0; i < str.length; i++) {
            hex += "" + str.charCodeAt(i).toString(16);
        }
        return hex;
    }

    function decode(hex) {
        var str = "";
        for (var i = 0; i < hex.length; i += 2) {
            str += String.fromCharCode(parseInt(hex.substr(i, 2), 16));
        }
        return str;
    }

    return {
        encode: encode,
        decode: decode
    };
})();