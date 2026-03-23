const { contextBridge, ipcRenderer } = require("electron");

contextBridge.exposeInMainWorld("overlayApi", {
  onGsiUpdate: (cb) => ipcRenderer.on("gsi-update", (_, payload) => cb(payload)),
  onCs2Status: (cb) => ipcRenderer.on("cs2-status", (_, payload) => cb(payload)),
  getInitialState: () => ipcRenderer.invoke("get-initial-state"),
  getAssetsBasePath: () => ipcRenderer.invoke("get-assets-base-path"),
  getCfgInstallResult: () => ipcRenderer.invoke("get-cfg-install-result")
});

