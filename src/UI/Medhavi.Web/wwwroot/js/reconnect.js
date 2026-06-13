window.setupConnectionListener = (dotNetHelper) => {
    const update = () => {
        const status = navigator.onLine ? "Connected" : "Disconnected";
        dotNetHelper.invokeMethodAsync("UpdateConnectionStatus", status);
    };
    window.addEventListener("online", update);
    window.addEventListener("offline", update);
    update();
};

window.setupKeyboardListener = (dotNetHelper) => {
    window.addEventListener("keydown", (e) => {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "k") {
            e.preventDefault();
            dotNetHelper.invokeMethodAsync("ToggleCommandPalette");
        }
    });
};
