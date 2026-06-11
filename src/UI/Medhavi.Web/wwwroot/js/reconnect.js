window.setupConnectionListener = (dotNetHelper) => {
    const update = () => {
        const status = navigator.onLine ? "Connected" : "Disconnected";
        dotNetHelper.invokeMethodAsync("UpdateConnectionStatus", status);
    };
    window.addEventListener("online", update);
    window.addEventListener("offline", update);
    update();
};
