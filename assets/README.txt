Asset manifest (synced to current code references).

Top/Map UI:
- logo_left.png
- logo_right.png
- minimap_frame.png
- radar_background.png

Players:
- players/default.png
- players/<player_name>.png
  - name is sanitized to lowercase with non-alphanumeric replaced by `_`
  - example: `players/fallen.png`, `players/m0nesy.png`

Weapons (SVG, inside `assets/weapons/`):
- ak47.svg
- awp.svg
- aug.svg
- bizon.svg
- c4.svg
- cz75a.svg
- deagle.svg
- decoy.svg
- famas.svg
- fiveseven.svg
- flashbang.svg
- g3sg1.svg
- galilar.svg
- glock.svg
- hegrenade.svg
- hkp2000.svg
- incgrenade.svg
- knife.svg
- m4a1.svg
- m4a1_silencer.svg
- mac10.svg
- molotov.svg
- mp5sd.svg
- mp7.svg
- mp9.svg
- p250.svg
- p90.svg
- revolver.svg
- scar20.svg
- sg556.svg
- smokegrenade.svg
- ssg08.svg
- taser.svg
- tec9.svg
- ump45.svg
- usp_silencer.svg

Notes:
- Weapon icons are now mapped to these SVG names in both Electron and C# code.
- Team/minimap/player portraits are currently expected as PNG.

