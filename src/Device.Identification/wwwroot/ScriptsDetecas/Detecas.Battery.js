/*
File Name: Detecas.Battery.js
Description : Batttery API
*/

Detecas.Battery = (function (window, document, debug) {
    "use strict";
    
    if (fpsChkIntr('Detecas.Battery') == true) {
        return;
    }

    var battery;
    var batteryInfo = "";

    function toTime(sec) {
        sec = parseInt(sec, 10);

        var hours = Math.floor(sec / 3600),
        minutes = Math.floor((sec - (hours * 3600)) / 60),
        seconds = sec - (hours * 3600) - (minutes * 60);

        if (hours < 10) { hours = '0' + hours; }
        if (minutes < 10) { minutes = '0' + minutes; }
        if (seconds < 10) { seconds = '0' + seconds; }

        return hours + ':' + minutes;
    }

    function batterySuccess(batteryManager) {
        // Assign batteryManager to globally 
        //   available `battery` variable.
        battery = batteryManager;

        // Register event handlers  
        battery.addEventListener('chargingchange', updatedBatteryStats);
        battery.addEventListener('chargingtimechange', updatedBatteryStats);
        battery.addEventListener('dischargingtimechange', updatedBatteryStats);
        battery.addEventListener('levelchange', updatedBatteryStats);

        // Process updated data
        updatedBatteryStats();
    }

    /**
    * Failure callback.
    */
    function batteryFailure() {
        // Fail gracefully. 
    }

    /**
    * Update HTML with current battery stats
    */
    function updatedBatteryStats() {

        var percentage = parseFloat((battery.level * 100).toFixed(2)) + '%',
        fully,
        remaining;

        if (battery.charging && battery.chargingTime === Infinity) {
            fully = 'Calculating...';
        } else if (battery.chargingTime !== Infinity) {
            fully = toTime(battery.chargingTime);
        } else {
            fully = '---';
        }

        if (!battery.charging && battery.dischargingTime === Infinity) {
            remaining = 'Calculating...';
        } else if (battery.dischargingTime !== Infinity) {
            remaining = toTime(battery.dischargingTime);
        } else {
            remaining = '---';
        }

        batteryInfo = battery.charging ? 'charging' : 'not charging'
        batteryInfo += "#" + percentage + "#" + fully + "#" + remaining;
    }

    function getBatteryInfo() {
        return batteryInfo;
    }

    function start() {
        try {
            if ("getBattery" in navigator) {
                // API is supported

                // Request battery manager object.
                navigator.getBattery().then(batterySuccess, batteryFailure);

            } else {
                // API is not supported, fail gracefully.
            }
        } catch (exception) {
            //debug.addException(exception, "Detecas.Battery.start");
        }
    }

    return {
        start: start,
        getBatteryInfo: getBatteryInfo
    };
} (window, document, Detecas.Debug));
