## CS2 Broadcast Overlay (Electron)

Electron version for a more flexible HUD UI:

- top-center score strip (CT/T colors)
- top-left minimap
- bottom 5v5 player bars
- dead players turn gray
- health-reactive lower fill (drops as damage is taken)
- CS2 process status indicator

## Run in dev

```bash
npm install
npm run dev
```

## Build Windows MSI installer

```bash
npm run build:win
```

Output:

- `dist/Cs2Overlay *.msi`

## GitHub Actions (Windows MSI)

Workflow file:
- `.github/workflows/build-windows-msi.yml`

It runs on `windows-latest`, builds the x64 MSI, and uploads:
- `cs2overlay-msi` artifact (`dist/*.msi`)
- `cs2overlay-win-unpacked` artifact (`dist/win-unpacked/**`)

Tagging a release like `v1.0.0` also attaches the MSI to the GitHub Release.

## GitHub Packages auto-publish

Workflow file:
- `.github/workflows/publish-github-package.yml`

On every push to `main`/`master`, it publishes an npm package to GitHub Packages:
- package name: `@<repo-owner>/cs2overlay-electron`
- version format: `<base-version>-ci.<run_number>`

This makes a package entry appear under the repo/account Packages page automatically.
It is chained to run after the `Build Windows MSI` workflow completes successfully on `main`/`master` (or can be started manually via workflow dispatch).

## CS2 GSI config

The app now tries to auto-install `gamestate_integration_cs2overlay.cfg` on startup into detected CS2 `cfg` folders (Steam default + library folders).

If it cannot write due to permissions, it also drops a copy here:
- `%USERPROFILE%\Documents\Cs2Overlay\gamestate_integration_cs2overlay.cfg`

Manual content (if needed):

```cfg
"Cs2Overlay"
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
```

## OBS setup

1. Run the installed `Cs2Overlay` app.
2. In OBS, add `Window Capture` and pick `Cs2Overlay`.
3. Add `Chroma Key` filter (green) to remove the overlay background.

## Assets

Assets are bundled into the MSI via `extraResources` and loaded from the installed app resources path automatically.

See `assets/README.txt` for required file names.