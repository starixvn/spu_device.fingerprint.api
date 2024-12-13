/*
File Name: Detecas.Geolocation.js
Description : HTML5 Geolocation
*/

Detecas.Geolocation = (function (window, document, debug) {
    "use strict";

    if (fpsChkIntr('Detecas.Geolocation') == true) {
        return;
    }

    var location = "";

    function Geolocation(position) {
        location = position.coords.latitude + "," + position.coords.longitude;        
    }

    function showError() {
        //debug.addException(exception, "Location can't be found");
    }

    function start() {
        try {
            navigator.permissions && navigator.permissions.query({ name: 'geolocation' }).then(function (PermissionStatus) {
                if (PermissionStatus.state == 'granted') {
                    //allowed
                    navigator.geolocation.getCurrentPosition(Geolocation, showError);
                } else {
                    //denied
                }
            })            
        } catch (exception) {
            //debug.addException(exception, "Detecas.Geolocation.start");
        }
    }

    return {
        start: start,
        getGeolocation: function () { return location; }
    };
} (window, document, Detecas.Debug));
