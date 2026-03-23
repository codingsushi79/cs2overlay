const path = require("path");
const { app, BrowserWindow, ipcMain } = require("electron");
const express = require("express");
const http = require("http");
const { exec } = require("child_process");
const fs = require("fs");
const os = require("os");

let overlayWindow = null;
let latestState = {};
let cfgInstallResult = "CFG not installed yet";

function getAssetsBasePath() {
  return app.isPackaged
    ? path.join(process.resourcesPath, "assets")
    : path.join(app.getAppPath(), "assets");
}

function buildCfgContent() {
  return `"Cs2Overlay"
{
  "uri" "http://127.0.0.1:3000/"
  "timeout" "1.0"
  "buffer" "0.1"
  "throttle" "0.1"
  "heartbeat" "5.0"
  "data"
  {
    "map" "1"
    "round" "1"
    "allplayers_id" "1"
    "allplayers_state" "1"
    "allplayers_weapons" "1"
    "phase_countdowns" "1"
  }
}
`;
}

function parseSteamLibraryPaths(libraryFoldersText) {
  const libs = [];
  const regex = /"path"\s+"([^"]+)"/g;
  let match = null;
  while ((match = regex.exec(libraryFoldersText)) !== null) {
    libs.push(match[1].replaceAll("\\\\", "\\"));
  }
  return libs;
}

function candidateCs2CfgDirs() {
  const dirs = [];
  const programFilesX86 = process.env["ProgramFiles(x86)"] || "C:\\Program Files (x86)";
  const steamRoot = path.join(programFilesX86, "Steam");

  const defaultDir = path.join(
    steamRoot,
    "steamapps",
    "common",
    "Counter-Strike Global Offensive",
    "game",
    "csgo",
    "cfg"
  );
  dirs.push(defaultDir);

  const libraryVdf = path.join(steamRoot, "steamapps", "libraryfolders.vdf");
  if (fs.existsSync(libraryVdf)) {
    try {
      const text = fs.readFileSync(libraryVdf, "utf8");
      const libraries = parseSteamLibraryPaths(text);
      for (const lib of libraries) {
        dirs.push(path.join(lib, "steamapps", "common", "Counter-Strike Global Offensive", "game", "csgo", "cfg"));
      }
    } catch {
      // Ignore parse failures and continue with default paths.
    }
  }

  return [...new Set(dirs)];
}

function ensureGsiConfig() {
  if (process.platform !== "win32") {
    cfgInstallResult = "CFG auto-install is only for Windows";
    return;
  }

  const cfgName = "gamestate_integration_cs2overlay.cfg";
  const content = buildCfgContent();
  const written = [];
  const attempted = [];

  for (const dir of candidateCs2CfgDirs()) {
    attempted.push(dir);
    try {
      if (!fs.existsSync(dir)) continue;
      fs.writeFileSync(path.join(dir, cfgName), content, "utf8");
      written.push(dir);
    } catch {
      // Permission issues are expected without admin in Program Files.
    }
  }

  // Keep a local copy so user can manually drop it if permissions blocked.
  try {
    const fallbackDir = path.join(os.homedir(), "Documents", "Cs2Overlay");
    fs.mkdirSync(fallbackDir, { recursive: true });
    fs.writeFileSync(path.join(fallbackDir, cfgName), content, "utf8");
  } catch {
    // ignore fallback errors
  }

  if (written.length > 0) {
    cfgInstallResult = `CFG installed to ${written.length} CS2 cfg folder(s)`;
  } else {
    cfgInstallResult = `Could not write into CS2 cfg automatically. Tried ${attempted.length} folder(s).`;
  }
}

function createWindow() {
  overlayWindow = new BrowserWindow({
    width: 1920,
    height: 1080,
    title: "Cs2Overlay",
    transparent: false,
    frame: false,
    alwaysOnTop: true,
    backgroundColor: "#00ff00",
    webPreferences: {
      preload: path.join(__dirname, "preload.js"),
      contextIsolation: true,
      nodeIntegration: false
    }
  });

  overlayWindow.setMenuBarVisibility(false);
  overlayWindow.loadFile(path.join(__dirname, "renderer", "index.html"));
}

function startGsiServer() {
  const ex = express();
  ex.use(express.json({ limit: "1mb" }));

  ex.post("/", (req, res) => {
    latestState = req.body || {};
    if (overlayWindow && !overlayWindow.isDestroyed()) {
      overlayWindow.webContents.send("gsi-update", latestState);
    }
    res.status(200).send("OK");
  });

  const server = http.createServer(ex);
  server.listen(3000, "127.0.0.1");
}

function pollCs2Status() {
  setInterval(() => {
    const cmd = process.platform === "win32"
      ? 'tasklist /FI "IMAGENAME eq cs2.exe" /NH'
      : "ps aux | rg -i 'cs2|csgo'";

    exec(cmd, (error, stdout) => {
      const running = !error && !!stdout && stdout.toLowerCase().includes("cs2");
      if (overlayWindow && !overlayWindow.isDestroyed()) {
        overlayWindow.webContents.send("cs2-status", { running });
      }
    });
  }, 2000);
}

ipcMain.handle("get-initial-state", () => latestState);
ipcMain.handle("get-assets-base-path", () => getAssetsBasePath());
ipcMain.handle("get-cfg-install-result", () => cfgInstallResult);

app.whenReady().then(() => {
  ensureGsiConfig();
  createWindow();
  startGsiServer();
  pollCs2Status();
});

app.on("window-all-closed", () => {
  if (process.platform !== "darwin") app.quit();
});

