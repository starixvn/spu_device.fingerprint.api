
var diInput = null;
var fpsStartTime = new Date();
var fpsTimeElapsed = 0;
var fpsIsStop = false;
var fpsMaxTime = 4000; // (total = 5s) use remaining 1s  for Detecas.Browser, Detecas.Device and Detecas.Core
var fpsIsMob = /iPhone|iPad|iPod|Android|BlackBerry|IEMobile/i.test(navigator.userAgent);
function fpsChkIntr(item, maxtime) {

    if (!fpsIsMob) {
        return false;
    }

    if (fpsIsStop == true) {
        return true;
    }

    if (maxtime === null) {
        maxtime = fpsMaxTime;
    }

    fpsTimeElapsed = new Date() - fpsStartTime;

    if (fpsTimeElapsed >= maxtime) {
        console.log('fpsChkIntr -->' + item);
        fpsIsStop = true;

        diInput = document.getElementById("__di");
        // create hidden input __di
        if (diInput === null) {
            var container = document.forms[0] || document.body;
            diInput = document.createElement("INPUT");
            diInput.id = diInput.name = "__di";
            diInput.setAttribute("type", "hidden");
            container.appendChild(diInput);
        }
        return true;
    }
    return false;
}