function byId(id) {
  return document.getElementById(id);
}

const els = {
  logoLeft: byId("logo-left"),
  logoRight: byId("logo-right"),
  minimapFrame: byId("minimap-frame"),
  minimapMap: byId("minimap-map"),
  teamLeftName: byId("team-left-name"),
  teamRightName: byId("team-right-name"),
  teamLeftScore: byId("team-left-score"),
  teamRightScore: byId("team-right-score"),
  roundInfo: byId("round-info"),
  roundTimer: byId("round-timer"),
  playersLeft: byId("players-left"),
  playersRight: byId("players-right"),
  focusAvatar: byId("focus-avatar"),
  focusName: byId("focus-name"),
  status: byId("cs2-status")
};

let assetsBasePath = "";

function asFileUrl(relPath) {
  if (!assetsBasePath) return `../../assets/${relPath}`;
  const normalizedBase = assetsBasePath.replace(/\\/g, "/");
  return `file://${normalizedBase}/${relPath}`;
}

function formatTime(sec) {
  const s = Math.max(0, Math.floor(Number(sec || 0)));
  const m = Math.floor(s / 60);
  const r = s % 60;
  return `${m}:${String(r).padStart(2, "0")}`;
}

function sanitizeName(n) {
  return String(n || "")
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]/g, "_");
}

function shortWeapon(name) {
  return String(name || "weapon").replace("weapon_", "");
}

function weaponIconPath(weaponName) {
  const n = String(weaponName || "").toLowerCase();
  const map = [
    ["ak47", "ak47.svg"], ["m4a4", "m4a1.svg"], ["m4a1_silencer", "m4a1_silencer.svg"], ["m4a1", "m4a1.svg"], ["awp", "awp.svg"],
    ["ssg08", "ssg08.svg"], ["scar20", "scar20.svg"], ["g3sg1", "g3sg1.svg"],
    ["mp9", "mp9.svg"], ["mp7", "mp7.svg"], ["mp5", "mp5sd.svg"], ["mac10", "mac10.svg"],
    ["ump45", "ump45.svg"], ["bizon", "bizon.svg"], ["p90", "p90.svg"],
    ["glock", "glock.svg"], ["hkp2000", "hkp2000.svg"], ["p2000", "hkp2000.svg"], ["usp", "usp_silencer.svg"], ["p250", "p250.svg"],
    ["cz75", "cz75a.svg"], ["fiveseven", "fiveseven.svg"], ["tec9", "tec9.svg"], ["deagle", "deagle.svg"], ["revolver", "revolver.svg"],
    ["hegrenade", "hegrenade.svg"], ["flashbang", "flashbang.svg"], ["smokegrenade", "smokegrenade.svg"],
    ["molotov", "molotov.svg"], ["incgrenade", "incgrenade.svg"], ["decoy", "decoy.svg"], ["taser", "taser.svg"], ["c4", "c4.svg"], ["knife", "knife.svg"]
  ];
  for (const [k, v] of map) if (n.includes(k)) return asFileUrl(`weapons/${v}`);
  return asFileUrl("weapons/ak47.svg");
}

function activeWeapon(player) {
  const weapons = player?.weapons || {};
  for (const key of Object.keys(weapons)) {
    if (weapons[key]?.state === "active") return weapons[key];
  }
  return { name: "weapon_knife" };
}

function buildPlayerCard(p, team) {
  const hp = Number(p?.state?.health ?? 0);
  const alive = hp > 0;
  const money = Number(p?.state?.money ?? 0);
  const w = activeWeapon(p);
  const fillHeight = Math.max(1, Math.min(100, hp)) * 0.9;

  const card = document.createElement("div");
  card.className = `player-card ${alive ? "" : "dead"}`;

  const fill = document.createElement("div");
  fill.className = `player-fill ${team.toLowerCase()}`;
  fill.style.height = `${fillHeight}px`;
  fill.style.opacity = alive ? "0.9" : "0.25";
  card.appendChild(fill);

  const content = document.createElement("div");
  content.className = "player-content";
  content.innerHTML = `
    <div class="hp">${hp}</div>
    <div class="name">${p?.name || "Player"}</div>
    <div class="weapon-row"><img src="${weaponIconPath(w?.name)}"/><span>${shortWeapon(w?.name)}</span></div>
    <div class="money">$ ${money}</div>
  `;
  card.appendChild(content);
  return card;
}

function renderPlayers(allPlayers) {
  const ct = [];
  const t = [];
  for (const id of Object.keys(allPlayers || {})) {
    const p = allPlayers[id];
    if (!p?.state) continue;
    if (String(p.team).toUpperCase() === "CT") ct.push(p);
    if (String(p.team).toUpperCase() === "T") t.push(p);
  }

  const sorter = (a, b) => (b.state.health > 0) - (a.state.health > 0) || String(a.name).localeCompare(String(b.name));
  ct.sort(sorter);
  t.sort(sorter);

  els.playersLeft.innerHTML = "";
  els.playersRight.innerHTML = "";

  t.slice(0, 5).forEach((p) => els.playersLeft.appendChild(buildPlayerCard(p, "T")));
  ct.slice(0, 5).forEach((p) => els.playersRight.appendChild(buildPlayerCard(p, "CT")));

  const focus = [...ct, ...t].find((p) => (p?.state?.health || 0) > 0) || ct[0] || t[0];
  if (focus) {
    els.focusName.textContent = focus.name || "POV";
    els.focusAvatar.src = asFileUrl(`players/${sanitizeName(focus.name)}.png`);
    els.focusAvatar.onerror = () => { els.focusAvatar.src = asFileUrl("players/default.png"); };
  }
}

function applyState(state) {
  const map = state?.map || {};
  const round = state?.round || {};
  const phase = state?.phase_countdowns || {};
  const teamCt = map?.team_ct || {};
  const teamT = map?.team_t || {};

  els.teamLeftName.textContent = teamCt?.name || "CT";
  els.teamLeftScore.textContent = String(teamCt?.score ?? 0);
  els.teamRightName.textContent = teamT?.name || "T";
  els.teamRightScore.textContent = String(teamT?.score ?? 0);

  els.roundInfo.textContent = `${round?.phase || "live"} ${map?.name || ""}`.trim();
  els.roundTimer.textContent = formatTime(phase?.phase_ends_in || 0);

  renderPlayers(state?.allplayers || {});
}

window.overlayApi.onGsiUpdate(applyState);
window.overlayApi.onCs2Status((x) => {
  els.status.textContent = x.running ? "CS2 detected - GSI live when in match" : "CS2 not detected";
  els.status.style.color = x.running ? "#7CFC00" : "#FF5D5D";
});

window.overlayApi.getInitialState().then((s) => {
  if (s) applyState(s);
});

window.overlayApi.getAssetsBasePath().then((base) => {
  assetsBasePath = base || "";
  els.logoLeft.src = asFileUrl("logo_left.png");
  els.logoRight.src = asFileUrl("logo_right.png");
  els.minimapFrame.src = asFileUrl("minimap_frame.png");
  els.minimapMap.src = asFileUrl("radar_background.png");
  els.focusAvatar.src = asFileUrl("players/default.png");
});

window.overlayApi.getCfgInstallResult().then((msg) => {
  if (msg) {
    els.status.textContent = `${els.status.textContent} | ${msg}`;
  }
});

