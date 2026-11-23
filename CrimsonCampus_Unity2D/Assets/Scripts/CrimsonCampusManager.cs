using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrimsonCampus
{
    public class CrimsonCampusManager : MonoBehaviour
    {
        [Header("Dash Chauve-Souris")]
        [Tooltip("Duree de la transformation en chauve-souris (secondes).")]
        public float batDashDuration = 0.28f;
        [Tooltip("Multiplicateur de vitesse pendant le dash.")]
        public float batDashSpeedMult = 3.1f;
        [Tooltip("Temps de recharge normal du dash (secondes).")]
        public float batDashCooldown = 3.2f;
        [Tooltip("Temps de recharge reduit avec l'amelioration Ailes de Chauve-Souris.")]
        public float batDashWingsCooldown = 1.9f;

        [Header("Equilibrage Chasseur de Vampire")]
        [Tooltip("Nombre de fideles a partir duquel un Chasseur peut apparaitre.")]
        public int hunterFollowerThreshold = 6;
        [Tooltip("Intervalle entre deux tirages d'apparition (secondes).")]
        public float hunterCheckInterval = 7f;
        [Tooltip("Probabilite de base d'apparition du Chasseur.")]
        [Range(0.05f, 1f)] public float hunterBaseChance = 0.30f;
        [Tooltip("Duree maximale de traque avant qu'il abandonne (secondes).")]
        public float hunterHuntDuration = 22f;
        public float hunterSpeed = 6.1f;

        [Header("Equilibrage Loups-Garous")]
        [Tooltip("Quantite de sang en poche a partir de laquelle les Loups-Garous te chassent.")]
        public double werewolfBloodThreshold = 300.0;
        [Tooltip("Intervalle entre deux tirages d'apparition des loups (secondes).")]
        public float werewolfCheckInterval = 6f;
        [Range(0.05f, 1f)] public float werewolfSpawnChance = 0.55f;
        public float werewolfHuntDuration = 20f;
        public float werewolfBaseSpeed = 6.3f;

        // Etat global
        private SaveState _state;
        private RetroAudioManager _audio;
        private Camera _cam;

        // Entite Joueur
        private GameObject _playerGo;
        private SpriteRenderer _playerSr;
        private Vector2 _playerPos;
        private int _playerDir; // 0=down, 1=up, 2=left, 3=right
        private float _facingAngle = -Mathf.PI * 0.5f;
        private float _playerWalkTimer;
        private bool _playerMoving;
        private float _pulseCooldown;
        private float _altarTimer;
        private Vector2? _pointerTarget;

        // Etat du Dash Chauve-Souris
        private float _dashTimer;
        private float _dashInvulnTimer;
        private float _dashCooldown;
        private Vector2 _dashDir = Vector2.down;
        private float _dashTrailAccum;
        private bool _dashDodgeNotified;

        // Effets visuels attaches au joueur / autel
        private SpriteRenderer _auraSr;
        private SpriteRenderer _pulseWaveSr;
        private float _pulseWaveScale;
        private float _pulseWaveAlpha;
        private SpriteRenderer _altarRingSr;
        private SpriteRenderer _altarChargeSr;

        // Camera & effets de choc
        private float _shakeTimer;
        private float _shakeIntensity;
        private float _crimsonFlashAlpha;

        // Mort & Recommencement
        private bool _isDead;
        private string _deathTitle = "";
        private string _deathReason = "";

        private enum ParticleKind { Heart, BloodOrb, MiniBat }

        private class GirlEntity
        {
            public GirlArchetype Type;
            public GameObject Go;
            public SpriteRenderer Sr;
            public SpriteRenderer HeartIndicatorSr;
            public SpriteRenderer BarBgSr;
            public SpriteRenderer BarFillSr;
            public Vector2 Pos;
            public Vector2 Vel;
            public int Dir;
            public int Frame;
            public float WalkTimer;
            public float WanderTimer;
            public bool Moving;
            public bool Following;
            public float CharmProgress;
            public float Seed;
        }

        private class SacrificeGhost
        {
            public GameObject Go;
            public SpriteRenderer Sr;
            public GameObject BeamGo;
            public SpriteRenderer BeamSr;
            public Vector2 CenterPos;
            public float Angle;
            public float StartRadius;
            public float Elapsed;
            public float Duration;
        }

        private class RitualSigilFx
        {
            public GameObject Go;
            public SpriteRenderer Sr;
            public GameObject Wave1Go;
            public SpriteRenderer Wave1Sr;
            public GameObject Wave2Go;
            public SpriteRenderer Wave2Sr;
            public float Elapsed;
            public float Duration;
            public bool ClimaxDone;
            public double PendingPayout;
            public bool AtAltar;
            public int SacrificedCount;
        }

        private class ParticleFx
        {
            public GameObject Go;
            public SpriteRenderer Sr;
            public Vector2 Pos;
            public Vector2 Vel;
            public float Life;
            public float MaxLife;
            public ParticleKind Kind;
        }

        private class FloatingLabel
        {
            public Vector2 WorldPos;
            public string Text;
            public Color Color;
            public float Life;
            public float MaxLife;
        }

        private class EnemyEntity
        {
            public bool IsHunter;
            public GameObject Go;
            public SpriteRenderer Sr;
            public SpriteRenderer AlertRingSr;
            public Vector2 Pos;
            public float LifeLeft;
            public float WalkTimer;
            public float LungeTimer;
            public float LungeActive;
        }

        private readonly List<GirlEntity> _roamingGirls = new List<GirlEntity>();
        private readonly List<GirlEntity> _followers = new List<GirlEntity>();
        private readonly List<SacrificeGhost> _sacrificeGhosts = new List<SacrificeGhost>();
        private readonly List<RitualSigilFx> _ritualFx = new List<RitualSigilFx>();
        private readonly List<ParticleFx> _particles = new List<ParticleFx>();
        private readonly List<FloatingLabel> _floatLabels = new List<FloatingLabel>();
        private readonly List<EnemyEntity> _enemies = new List<EnemyEntity>();

        private float _spawnTimer;
        private float _hunterCheckTimer;
        private float _werewolfCheckTimer;
        private float _saveAccum;

        // UI & Notifications
        private int _activeTab;
        private Vector2 _shopScroll;
        private readonly List<(string msg, float time)> _toasts = new List<(string, float)>();
        private bool _showConfirmModal;
        private string _modalTitle;
        private string _modalBody;
        private Action _modalAction;

        // Styles GUI caches
        private bool _stylesInit;
        private GUIStyle _panelStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _cardBuyStyle;
        private GUIStyle _btnRedStyle;
        private GUIStyle _btnGoldStyle;
        private GUIStyle _btnPurpleStyle;
        private GUIStyle _btnDarkStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _boldStyle;
        private GUIStyle _smallStyle;
        private GUIStyle _centerBoldStyle;
        private GUIStyle _alertBannerStyle;
        private Texture2D _whiteTex;

        // =====================================================================
        // INITIALISATION
        // =====================================================================
        private void Start()
        {
            _audio = gameObject.AddComponent<RetroAudioManager>();
            _whiteTex = PixelSpriteFactory.CreateBlankTex(2, 2);
            for (int i = 0; i < 4; i++) _whiteTex.SetPixel(i % 2, i / 2, Color.white);
            _whiteTex.Apply();

            LoadGame();
            _audio.SoundEnabled = _state.soundOn;

            SetupCamera();
            BuildCampusWorld();
            BuildPlayer();
            RebuildFollowersFromSave();

            for (int i = 0; i < 14; i++) SpawnRoamingGirl();

            _hunterCheckTimer = hunterCheckInterval;
            _werewolfCheckTimer = werewolfCheckInterval;
        }

        private void SetupCamera()
        {
            _cam = Camera.main;
            if (_cam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                _cam = camGo.AddComponent<Camera>();
            }
            _cam.orthographic = true;
            _cam.orthographicSize = 9.8f;
            _cam.backgroundColor = GameCatalog.Hex("#0b0612");
            _cam.transform.position = new Vector3(WorldConfig.PlayerStartPos.x, WorldConfig.PlayerStartPos.y, -10f);
        }

        private void BuildCampusWorld()
        {
            GameObject worldRoot = new GameObject("CampusWorld");

            GameObject grass = new GameObject("Grass");
            grass.transform.SetParent(worldRoot.transform);
            grass.transform.position = new Vector3(WorldConfig.WorldW * 0.5f, WorldConfig.WorldH * 0.5f, 0f);
            SpriteRenderer gsr = grass.AddComponent<SpriteRenderer>();
            gsr.sprite = PixelSpriteFactory.GetGrassTileSprite();
            gsr.drawMode = SpriteDrawMode.Tiled;
            gsr.size = new Vector2(WorldConfig.WorldW, WorldConfig.WorldH);
            gsr.sortingOrder = -10000;

            Color pathCol = GameCatalog.Hex("#352b42");
            CreateStaticRect(worldRoot.transform, "Plaza", new Vector2(45f, 32.5f), new Vector2(18.8f, 16.2f), pathCol, -9990);
            CreateStaticRect(worldRoot.transform, "PathV", new Vector2(45f, 32.5f), new Vector2(5.6f, 60f), pathCol, -9990);
            CreateStaticRect(worldRoot.transform, "PathH", new Vector2(45f, 32.5f), new Vector2(85f, 5f), pathCol, -9990);
            CreateStaticRect(worldRoot.transform, "PathTL", new Vector2(17f, 41f), new Vector2(3.2f, 15f), pathCol, -9990);
            CreateStaticRect(worldRoot.transform, "PathTR", new Vector2(72f, 41f), new Vector2(3.2f, 15f), pathCol, -9990);
            CreateStaticRect(worldRoot.transform, "PathBL", new Vector2(17.5f, 23f), new Vector2(3.2f, 15f), pathCol, -9990);
            CreateStaticRect(worldRoot.transform, "PathBR", new Vector2(72.2f, 23f), new Vector2(3.2f, 15f), pathCol, -9990);

            Color fenceCol = GameCatalog.Hex("#3b1c4a");
            CreateStaticRect(worldRoot.transform, "FenceN", new Vector2(45f, WorldConfig.WorldH - 0.6f), new Vector2(WorldConfig.WorldW, 1.2f), fenceCol, -9985);
            CreateStaticRect(worldRoot.transform, "FenceS", new Vector2(45f, 0.6f), new Vector2(WorldConfig.WorldW, 1.2f), fenceCol, -9985);
            CreateStaticRect(worldRoot.transform, "FenceW", new Vector2(0.6f, 32.5f), new Vector2(1.2f, WorldConfig.WorldH), fenceCol, -9985);
            CreateStaticRect(worldRoot.transform, "FenceE", new Vector2(WorldConfig.WorldW - 0.6f, 32.5f), new Vector2(1.2f, WorldConfig.WorldH), fenceCol, -9985);

            GameObject altarGo = new GameObject("CrimsonAltar");
            altarGo.transform.SetParent(worldRoot.transform);
            altarGo.transform.position = new Vector3(WorldConfig.AltarPos.x, WorldConfig.AltarPos.y, 0f);
            _altarRingSr = altarGo.AddComponent<SpriteRenderer>();
            _altarRingSr.sprite = PixelSpriteFactory.GetPentagramSigilSprite("altar_sigil", 96, GameCatalog.Hex("#e11d48"), GameCatalog.Hex("#fda4af"));
            _altarRingSr.transform.localScale = Vector3.one * WorldConfig.AltarRadius;
            _altarRingSr.sortingOrder = -9975;

            GameObject altarChargeGo = new GameObject("AltarCharge");
            altarChargeGo.transform.SetParent(altarGo.transform);
            altarChargeGo.transform.localPosition = Vector3.zero;
            _altarChargeSr = altarChargeGo.AddComponent<SpriteRenderer>();
            _altarChargeSr.sprite = PixelSpriteFactory.GetRingSprite("altar_charge", 64, new Color(1f, 0.88f, 0.28f, 0.16f), GameCatalog.Hex("#fde047"), 3f);
            _altarChargeSr.sortingOrder = -9974;
            _altarChargeSr.enabled = false;

            foreach (BuildingData b in GameCatalog.Buildings)
            {
                GameObject bGo = new GameObject("Bldg_" + b.Id);
                bGo.transform.SetParent(worldRoot.transform);
                bGo.transform.position = new Vector3(b.Bounds.x, b.Bounds.y, 0f);
                SpriteRenderer bsr = bGo.AddComponent<SpriteRenderer>();
                bsr.sprite = PixelSpriteFactory.GetBuildingSprite(b);
                bsr.sortingOrder = Mathf.RoundToInt(-b.Bounds.y * 100f);
            }

            foreach (TreeData t in GameCatalog.Trees)
            {
                GameObject tGo = new GameObject("Tree");
                tGo.transform.SetParent(worldRoot.transform);
                tGo.transform.position = new Vector3(t.Position.x, t.Position.y, 0f);
                SpriteRenderer tsr = tGo.AddComponent<SpriteRenderer>();
                tsr.sprite = PixelSpriteFactory.GetTreeSprite(t.IsCrimsonCherry);
                tsr.sortingOrder = Mathf.RoundToInt(-t.Position.y * 100f);
            }

            foreach (Vector2 lp in GameCatalog.Lamps)
            {
                GameObject lGo = new GameObject("Lamp");
                lGo.transform.SetParent(worldRoot.transform);
                lGo.transform.position = new Vector3(lp.x, lp.y, 0f);
                SpriteRenderer lsr = lGo.AddComponent<SpriteRenderer>();
                lsr.sprite = PixelSpriteFactory.GetLampSprite();
                lsr.sortingOrder = Mathf.RoundToInt(-lp.y * 100f);
            }
        }

        private void CreateStaticRect(Transform parent, string name, Vector2 pos, Vector2 size, Color color, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelSpriteFactory.GetSolidRectSprite("rect_" + ColorUtility.ToHtmlStringRGBA(color), 16, 16, color, 16f);
            sr.sortingOrder = sortingOrder;
        }

        private void BuildPlayer()
        {
            _playerPos = WorldConfig.PlayerStartPos;
            _playerGo = new GameObject("VampirePlayer");
            _playerGo.transform.position = new Vector3(_playerPos.x, _playerPos.y, 0f);
            _playerSr = _playerGo.AddComponent<SpriteRenderer>();

            GameObject auraGo = new GameObject("CharmAura");
            auraGo.transform.SetParent(_playerGo.transform);
            auraGo.transform.localPosition = Vector3.zero;
            _auraSr = auraGo.AddComponent<SpriteRenderer>();
            _auraSr.sprite = PixelSpriteFactory.GetRingSprite("charm_aura", 96, new Color(0.93f, 0.28f, 0.6f, 0.08f), new Color(0.98f, 0.44f, 0.52f, 0.55f), 2f);
            _auraSr.sortingOrder = -9965;

            GameObject pulseGo = new GameObject("PulseWave");
            pulseGo.transform.SetParent(_playerGo.transform);
            pulseGo.transform.localPosition = Vector3.zero;
            _pulseWaveSr = pulseGo.AddComponent<SpriteRenderer>();
            _pulseWaveSr.sprite = PixelSpriteFactory.GetRingSprite("pulse_wave", 64, Color.clear, GameCatalog.Hex("#fb7185"), 3f);
            _pulseWaveSr.sortingOrder = -9960;
            _pulseWaveSr.enabled = false;

            RefreshPlayerSprite();
        }

        // =====================================================================
        // BOUCLE PRINCIPALE
        // =====================================================================
        private void Update()
        {
            float dt = Mathf.Min(0.08f, Time.deltaTime);

            for (int i = _toasts.Count - 1; i >= 0; i--)
            {
                float nt = _toasts[i].time - dt;
                if (nt <= 0f) _toasts.RemoveAt(i);
                else _toasts[i] = (_toasts[i].msg, nt);
            }

            if (_crimsonFlashAlpha > 0f)
                _crimsonFlashAlpha = Mathf.Max(0f, _crimsonFlashAlpha - dt * 1.15f);

            if (_isDead)
            {
                if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return))
                    RestartAfterDeath();
                UpdateCamera(dt);
                return;
            }

            if (_showConfirmModal)
            {
                UpdateCamera(dt);
                return;
            }

            HandleShortcuts();
            UpdateTimers(dt);
            UpdatePlayerMovement(dt);
            UpdateSpawning(dt);
            UpdateRoamingGirls(dt);
            UpdateFollowersFormation(dt);
            UpdateSacrificeAnimations(dt);
            UpdateThreatsAndEnemies(dt);
            UpdateParticlesAndLabels(dt);
            UpdateCamera(dt);

            _saveAccum += dt;
            if (_saveAccum >= 6f)
            {
                _saveAccum = 0f;
                SaveGame(false);
            }
        }

        private void HandleShortcuts()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
                TriggerCharmPulse();

            if (Input.GetKeyDown(KeyCode.F))
                SacrificeCrowd(IsPlayerAtAltar());

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) ||
                (Input.GetMouseButtonDown(1) && !IsPointerOverUi()))
                TriggerBatDash();
        }

        private void UpdateTimers(float dt)
        {
            if (_state.moonTimeLeft > 0f)
            {
                _state.moonTimeLeft = Mathf.Max(0f, _state.moonTimeLeft - dt);
                if (_state.moonTimeLeft <= 0f) _state.moonCharge = 0f;
            }

            if (_pulseCooldown > 0f)
                _pulseCooldown = Mathf.Max(0f, _pulseCooldown - dt);

            if (_dashCooldown > 0f)
                _dashCooldown = Mathf.Max(0f, _dashCooldown - dt);

            if (_dashInvulnTimer > 0f)
                _dashInvulnTimer = Mathf.Max(0f, _dashInvulnTimer - dt);

            if (_pulseWaveAlpha > 0f)
            {
                _pulseWaveScale += dt * 13f;
                _pulseWaveAlpha = Mathf.Max(0f, _pulseWaveAlpha - dt * 2.0f);
                _pulseWaveSr.enabled = _pulseWaveAlpha > 0.01f;
                _pulseWaveSr.transform.localScale = Vector3.one * _pulseWaveScale;
                _pulseWaveSr.color = new Color(1f, 1f, 1f, _pulseWaveAlpha);
            }

            bool atAltar = IsPlayerAtAltar();
            int totalGirls = GetTotalGirls();
            if (atAltar && totalGirls > 0)
            {
                _altarTimer += dt;
                _altarChargeSr.enabled = true;
                float prog = Mathf.Clamp01(_altarTimer / 0.65f);
                _altarChargeSr.transform.localScale = Vector3.one * (WorldConfig.AltarRadius * (0.25f + prog * 0.85f));
                if (_altarTimer >= 0.65f)
                {
                    SacrificeCrowd(true);
                }
            }
            else
            {
                _altarTimer = Mathf.Max(0f, _altarTimer - dt * 2f);
                _altarChargeSr.enabled = _altarTimer > 0.02f;
                if (_altarChargeSr.enabled)
                {
                    float prog = Mathf.Clamp01(_altarTimer / 0.65f);
                    _altarChargeSr.transform.localScale = Vector3.one * (WorldConfig.AltarRadius * (0.25f + prog * 0.85f));
                }
            }

            _altarRingSr.transform.Rotate(0f, 0f, dt * 14f);
        }

        // =====================================================================
        // DASH CHAUVE-SOURIS (TRANSFORMATION, INVINCIBILITE & NUEE)
        // =====================================================================
        public void TriggerBatDash()
        {
            if (_dashCooldown > 0f || _dashTimer > 0f || _isDead) return;

            Vector2 dir = ReadKeyboardDirection();
            if (dir.sqrMagnitude < 0.01f && !IsPointerOverUi())
            {
                Vector3 wp = _cam.ScreenToWorldPoint(Input.mousePosition);
                Vector2 toMouse = new Vector2(wp.x, wp.y) - _playerPos;
                if (toMouse.sqrMagnitude > 0.25f) dir = toMouse.normalized;
            }
            if (dir.sqrMagnitude < 0.01f)
            {
                dir = new Vector2(Mathf.Cos(_facingAngle), Mathf.Sin(_facingAngle));
            }

            _dashDir = dir.normalized;
            _facingAngle = Mathf.Atan2(_dashDir.y, _dashDir.x);
            _dashTimer = batDashDuration;
            _dashInvulnTimer = batDashDuration + 0.14f;
            _dashCooldown = _state.upgrades.Contains("wings") ? batDashWingsCooldown : batDashCooldown;
            _dashTrailAccum = 0f;
            _dashDodgeNotified = false;
            _pointerTarget = null;

            _audio.Play("bat_dash");
            TriggerShake(0.12f, 0.14f);

            // Nuee de mini chauves-souris au depart
            for (int i = 0; i < 6; i++)
            {
                Vector2 spread = -_dashDir * UnityEngine.Random.Range(1.2f, 3.2f) + UnityEngine.Random.insideUnitCircle * 1.8f;
                SpawnParticle(_playerPos + Vector2.up * 0.5f, spread, UnityEngine.Random.Range(0.35f, 0.65f), Color.white, ParticleKind.MiniBat, UnityEngine.Random.Range(0.7f, 1.1f));
            }
        }

        private Vector2 ReadKeyboardDirection()
        {
            float dx = 0f;
            float dy = 0f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dx += 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftArrow)) dx -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.UpArrow)) dy += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) dy -= 1f;
            Vector2 v = new Vector2(dx, dy);
            return v.sqrMagnitude > 0.001f ? v.normalized : Vector2.zero;
        }

        private void UpdatePlayerMovement(float dt)
        {
            if (_dashTimer > 0f)
            {
                _dashTimer = Mathf.Max(0f, _dashTimer - dt);
                float dashSpd = GetPlayerSpeed() * batDashSpeedMult;
                float nx = _playerPos.x + _dashDir.x * dashSpd * dt;
                float ny = _playerPos.y + _dashDir.y * dashSpd * dt;

                if (!IsPointBlocked(nx, _playerPos.y, 0.45f)) _playerPos.x = nx;
                if (!IsPointBlocked(_playerPos.x, ny, 0.45f)) _playerPos.y = ny;

                _dashTrailAccum += dt;
                if (_dashTrailAccum >= 0.035f)
                {
                    _dashTrailAccum = 0f;
                    Vector2 off = UnityEngine.Random.insideUnitCircle * 0.35f;
                    SpawnParticle(_playerPos + off + Vector2.up * 0.4f, -_dashDir * 1.4f + UnityEngine.Random.insideUnitCircle * 0.8f, 0.42f, Color.white, ParticleKind.MiniBat, 0.85f);
                }

                // Fin du dash : petite explosion de chauves-souris
                if (_dashTimer <= 0f)
                {
                    for (int i = 0; i < 4; i++)
                        SpawnParticle(_playerPos + Vector2.up * 0.5f, UnityEngine.Random.insideUnitCircle * 2.2f, 0.45f, Color.white, ParticleKind.MiniBat, 0.8f);
                }

                _playerMoving = true;
                _playerGo.transform.position = new Vector3(_playerPos.x, _playerPos.y, 0f);
                _playerSr.sortingOrder = Mathf.RoundToInt(-_playerPos.y * 100f) + 5;
                RefreshPlayerSprite();
                UpdateAuraVisual();
                return;
            }

            Vector2 move = ReadKeyboardDirection();

            if (Input.GetMouseButton(0) && !IsPointerOverUi())
            {
                Vector3 wp = _cam.ScreenToWorldPoint(Input.mousePosition);
                _pointerTarget = new Vector2(wp.x, wp.y);
            }
            if (move.sqrMagnitude > 0.001f)
            {
                _pointerTarget = null;
            }
            else if (_pointerTarget.HasValue)
            {
                Vector2 delta = _pointerTarget.Value - _playerPos;
                if (delta.magnitude > 0.35f)
                    move = delta / delta.magnitude;
                else if (!Input.GetMouseButton(0))
                    _pointerTarget = null;
            }

            if (move.sqrMagnitude > 0.001f)
            {
                move.Normalize();
                float spd = GetPlayerSpeed();
                float nx = _playerPos.x + move.x * spd * dt;
                float ny = _playerPos.y + move.y * spd * dt;

                if (!IsPointBlocked(nx, _playerPos.y, 0.55f)) _playerPos.x = nx;
                if (!IsPointBlocked(_playerPos.x, ny, 0.55f)) _playerPos.y = ny;

                float targetAngle = Mathf.Atan2(move.y, move.x);
                float diff = Mathf.DeltaAngle(_facingAngle * Mathf.Rad2Deg, targetAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
                _facingAngle += diff * Mathf.Min(1f, dt * 8.5f);

                _playerMoving = true;
                _playerWalkTimer += dt * 9f;
                if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
                    _playerDir = move.x > 0f ? 3 : 2;
                else
                    _playerDir = move.y > 0f ? 1 : 0;

                if (_state.upgrades.Contains("cologne") && UnityEngine.Random.value < dt * 6f)
                {
                    SpawnParticle(_playerPos + UnityEngine.Random.insideUnitCircle * 0.4f, new Vector2(0f, -0.4f), 0.75f, GameCatalog.Hex("#fb7185"), ParticleKind.Heart, 0.35f);
                }
            }
            else
            {
                _playerMoving = false;
            }

            _playerGo.transform.position = new Vector3(_playerPos.x, _playerPos.y, 0f);
            _playerSr.sortingOrder = Mathf.RoundToInt(-_playerPos.y * 100f);
            RefreshPlayerSprite();
            UpdateAuraVisual();
        }

        private void UpdateAuraVisual()
        {
            float auraR = GetCharmAuraRadius();
            float pulse = Mathf.Sin(Time.time * 4f) * 0.12f;
            _auraSr.transform.localScale = Vector3.one * (auraR + pulse);
            _auraSr.color = _state.moonTimeLeft > 0f ? GameCatalog.Hex("#fde047") : Color.white;
        }

        private void RefreshPlayerSprite()
        {
            if (_dashTimer > 0f)
            {
                int batFrame = Mathf.FloorToInt(Time.time * 24f) % 2;
                _playerSr.sprite = PixelSpriteFactory.GetBatDashSprite(batFrame);
                _playerSr.flipX = _dashDir.x < -0.1f;
                _playerSr.color = Color.white;
                return;
            }

            _playerSr.flipX = false;
            int frame = _playerMoving ? (Mathf.FloorToInt(_playerWalkTimer) % 2) : 0;
            _playerSr.sprite = PixelSpriteFactory.GetVampireSprite(
                _state.upgrades.Contains("hair"),
                _state.upgrades.Contains("eyes"),
                _state.upgrades.Contains("cape"),
                _state.upgrades.Contains("rose"),
                _state.upgrades.Contains("wings"),
                _playerDir,
                frame
            );
            // Legere translucidite durant les frames d'invincibilite post-dash
            _playerSr.color = _dashInvulnTimer > 0f ? new Color(1f, 0.85f, 0.95f, 0.78f) : Color.white;
        }

        // =====================================================================
        // ETUDIANTES LIBRES & MEUTE 180 DEGRES (COEURS DANS LES YEUX)
        // =====================================================================
        private void UpdateSpawning(float dt)
        {
            _spawnTimer -= dt;
            if (_spawnTimer <= 0f)
            {
                _spawnTimer = Mathf.Max(0.35f, 1.25f - _state.repParty * 0.07f);
                if (_roamingGirls.Count < GetMaxRoamingGirls())
                    SpawnRoamingGirl();
            }
        }

        private void SpawnRoamingGirl()
        {
            int myAllure = GetAllure();
            List<GirlArchetype> unlocked = new List<GirlArchetype>();
            GirlArchetype lockedNext = null;

            foreach (GirlArchetype g in GameCatalog.GirlTypes)
            {
                if (myAllure >= g.ReqAllure) unlocked.Add(g);
                else if (lockedNext == null) lockedNext = g;
            }

            GirlArchetype chosen;
            if (lockedNext != null && UnityEngine.Random.value < 0.15f)
            {
                chosen = lockedNext;
            }
            else
            {
                float totalW = 0f;
                for (int i = 0; i < unlocked.Count; i++) totalW += Mathf.Pow(1.45f, i);
                float r = UnityEngine.Random.Range(0f, totalW);
                chosen = unlocked[0];
                for (int i = 0; i < unlocked.Count; i++)
                {
                    r -= Mathf.Pow(1.45f, i);
                    if (r <= 0f) { chosen = unlocked[i]; break; }
                }
            }

            for (int tries = 0; tries < 25; tries++)
            {
                float x = UnityEngine.Random.Range(chosen.SpawnBox.xMin, chosen.SpawnBox.xMax);
                float y = UnityEngine.Random.Range(chosen.SpawnBox.yMin, chosen.SpawnBox.yMax);
                Vector2 p = new Vector2(x, y);
                if (!IsPointBlocked(x, y, 0.9f) && Vector2.Distance(p, _playerPos) > 6f)
                {
                    _roamingGirls.Add(CreateGirlEntity(chosen, p, false));
                    return;
                }
            }
        }

        private GirlEntity CreateGirlEntity(GirlArchetype type, Vector2 pos, bool following)
        {
            GameObject go = new GameObject("Girl_" + type.Id);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelSpriteFactory.GetGirlSprite(type, following, 0, 0);

            GameObject hGo = new GameObject("HeartIcon");
            hGo.transform.SetParent(go.transform);
            hGo.transform.localPosition = new Vector3(0f, 1.75f, 0f);
            SpriteRenderer hsr = hGo.AddComponent<SpriteRenderer>();
            hsr.sprite = PixelSpriteFactory.GetPixelHeartSprite(GameCatalog.Hex("#fb7185"));
            hsr.enabled = following;

            GameObject bgGo = new GameObject("CharmBg");
            bgGo.transform.SetParent(go.transform);
            bgGo.transform.localPosition = new Vector3(0f, 1.72f, 0f);
            bgGo.transform.localScale = new Vector3(1.1f, 0.18f, 1f);
            SpriteRenderer bgSr = bgGo.AddComponent<SpriteRenderer>();
            bgSr.sprite = PixelSpriteFactory.GetSolidRectSprite("bar_bg", 8, 8, GameCatalog.Hex("#1f0b2e"), 8f);
            bgSr.enabled = false;

            GameObject fillGo = new GameObject("CharmFill");
            fillGo.transform.SetParent(go.transform);
            fillGo.transform.localPosition = new Vector3(0f, 1.72f, 0f);
            fillGo.transform.localScale = new Vector3(1.0f, 0.12f, 1f);
            SpriteRenderer fillSr = fillGo.AddComponent<SpriteRenderer>();
            fillSr.sprite = PixelSpriteFactory.GetSolidRectSprite("bar_fill", 8, 8, GameCatalog.Hex("#f43f5e"), 8f);
            fillSr.enabled = false;

            return new GirlEntity
            {
                Type = type,
                Go = go,
                Sr = sr,
                HeartIndicatorSr = hsr,
                BarBgSr = bgSr,
                BarFillSr = fillSr,
                Pos = pos,
                WanderTimer = UnityEngine.Random.Range(0.5f, 2.2f),
                Following = following,
                Seed = UnityEngine.Random.Range(0f, 100f)
            };
        }

        private void UpdateRoamingGirls(float dt)
        {
            float auraR = GetCharmAuraRadius();
            int myAllure = GetAllure();
            float charmSpeed = GetCharmSpeedMult();

            for (int i = _roamingGirls.Count - 1; i >= 0; i--)
            {
                GirlEntity g = _roamingGirls[i];
                float dist = Vector2.Distance(g.Pos, _playerPos);
                bool canWoo = myAllure >= g.Type.ReqAllure;

                if (canWoo && dist <= auraR)
                {
                    g.Moving = false;
                    g.CharmProgress += (dt / g.Type.CharmTime) * charmSpeed;
                    if (UnityEngine.Random.value < dt * 5f)
                        SpawnParticle(g.Pos + new Vector2(UnityEngine.Random.Range(-0.35f, 0.35f), 0.9f), new Vector2(0f, 1.4f), 0.55f, GameCatalog.Hex("#fda4af"), ParticleKind.Heart, 0.6f);

                    if (g.CharmProgress >= 1f)
                    {
                        CollectGirl(g, i);
                        continue;
                    }
                }
                else
                {
                    if (g.CharmProgress > 0f)
                        g.CharmProgress = Mathf.Max(0f, g.CharmProgress - dt * 0.35f);

                    g.WanderTimer -= dt;
                    if (g.WanderTimer <= 0f)
                    {
                        g.WanderTimer = UnityEngine.Random.Range(1.2f, 3.4f);
                        if (UnityEngine.Random.value < 0.35f) g.Vel = Vector2.zero;
                        else
                        {
                            float ang = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                            g.Vel = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * 1.7f;
                        }
                    }

                    Vector2 next = g.Pos + g.Vel * dt;
                    if (!IsPointBlocked(next.x, next.y, 0.8f))
                    {
                        g.Pos = next;
                        g.Moving = g.Vel.magnitude > 0.1f;
                        if (g.Moving)
                        {
                            g.WalkTimer += dt * 6f;
                            g.Frame = Mathf.FloorToInt(g.WalkTimer) % 2;
                            g.Dir = Mathf.Abs(g.Vel.x) > Mathf.Abs(g.Vel.y) ? (g.Vel.x > 0 ? 3 : 2) : (g.Vel.y > 0 ? 1 : 0);
                        }
                    }
                    else
                    {
                        g.Vel = -g.Vel;
                    }
                }

                g.Go.transform.position = new Vector3(g.Pos.x, g.Pos.y, 0f);
                int order = Mathf.RoundToInt(-g.Pos.y * 100f);
                g.Sr.sortingOrder = order;
                g.Sr.sprite = PixelSpriteFactory.GetGirlSprite(g.Type, false, g.Dir, g.Moving ? g.Frame : 0);

                bool showBar = g.CharmProgress > 0.01f;
                g.BarBgSr.enabled = showBar;
                g.BarFillSr.enabled = showBar;
                if (showBar)
                {
                    g.BarBgSr.sortingOrder = order + 50;
                    g.BarFillSr.sortingOrder = order + 51;
                    g.BarFillSr.transform.localScale = new Vector3(Mathf.Clamp01(g.CharmProgress), 0.12f, 1f);
                }
            }
        }

        private void CollectGirl(GirlEntity g, int roamingIndex)
        {
            _roamingGirls.RemoveAt(roamingIndex);
            _state.girlCounts[g.Type.Index]++;
            _state.lifetimeGirls++;
            _audio.Play("woo");

            SpawnBurst(g.Pos + Vector2.up * 0.7f, 6, GameCatalog.Hex("#fb7185"));
            SpawnFloatLabel(g.Pos + Vector2.up * 1.3f, g.Type.Name + " charmee !", GameCatalog.Hex("#fda4af"));

            if (_followers.Count < 65)
            {
                g.Following = true;
                g.CharmProgress = 0f;
                g.BarBgSr.enabled = false;
                g.BarFillSr.enabled = false;
                g.HeartIndicatorSr.enabled = true;
                g.Sr.sprite = PixelSpriteFactory.GetGirlSprite(g.Type, true, g.Dir, 0);
                _followers.Add(g);
            }
            else
            {
                Destroy(g.Go);
            }
        }

        private Vector2 GetBlockSlotTarget(int index, int totalCount)
        {
            int remaining = index;
            int row = 0;
            int rowCap = 5;
            while (remaining >= rowCap)
            {
                remaining -= rowCap;
                row++;
                rowCap = 5 + row * 2;
            }

            int girlsBeforeRow = index - remaining;
            int countInRow = Mathf.Min(rowCap, Mathf.Max(1, totalCount - girlsBeforeRow));

            float backAngle = _facingAngle + Mathf.PI;
            float arcSpan = Mathf.PI * 0.96f;
            float frac = countInRow == 1 ? 0f : (remaining / (float)(countInRow - 1)) - 0.5f;
            float angle = backAngle + frac * arcSpan;
            float radius = 1.65f + row * 1.0f;

            return _playerPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private void UpdateFollowersFormation(float dt)
        {
            float followSpeed = GetPlayerSpeed() * 1.25f;
            int count = _followers.Count;

            for (int i = 0; i < count; i++)
            {
                GirlEntity f = _followers[i];
                Vector2 slot = GetBlockSlotTarget(i, count);

                Vector2 sep = Vector2.zero;
                for (int j = 0; j < count; j++)
                {
                    if (i == j) continue;
                    Vector2 diff = f.Pos - _followers[j].Pos;
                    float d = diff.magnitude;
                    if (d > 0.01f && d < 0.85f)
                        sep += (diff / d) * (0.85f - d) * 0.7f;
                }

                Vector2 target = slot + sep;
                Vector2 delta = target - f.Pos;
                float dist = delta.magnitude;

                if (dist > 0.16f)
                {
                    float boost = dist > 3.5f ? 1.6f : 1.05f;
                    float step = Mathf.Min(dist, followSpeed * dt * boost);
                    Vector2 next = f.Pos + (delta / dist) * step;

                    if (!IsPointBlocked(next.x, f.Pos.y, 0.45f)) f.Pos.x = next.x;
                    if (!IsPointBlocked(f.Pos.x, next.y, 0.45f)) f.Pos.y = next.y;

                    if (Vector2.Distance(f.Pos, _playerPos) > 14f)
                        f.Pos = _playerPos + UnityEngine.Random.insideUnitCircle * 1.2f;

                    f.Moving = true;
                    f.WalkTimer += dt * 8.5f;
                    f.Frame = Mathf.FloorToInt(f.WalkTimer) % 2;
                    f.Dir = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? (delta.x > 0 ? 3 : 2) : (delta.y > 0 ? 1 : 0);
                }
                else
                {
                    f.Moving = false;
                    f.Dir = _playerDir;
                }

                f.Go.transform.position = new Vector3(f.Pos.x, f.Pos.y, 0f);
                int order = Mathf.RoundToInt(-f.Pos.y * 100f);
                f.Sr.sortingOrder = order;
                f.Sr.sprite = PixelSpriteFactory.GetGirlSprite(f.Type, true, f.Dir, f.Moving ? f.Frame : 0);

                f.HeartIndicatorSr.sortingOrder = order + 40;
                f.HeartIndicatorSr.transform.localPosition = new Vector3(0f, 1.82f + Mathf.Sin(Time.time * 5f + f.Seed) * 0.12f, 0f);
            }
        }

        private void RebuildFollowersFromSave()
        {
            ClearList(_followers);
            int maxVis = 65;
            int total = Mathf.Min(maxVis, GetTotalGirls());
            int added = 0;

            for (int t = GameCatalog.GirlTypes.Length - 1; t >= 0; t--)
            {
                GirlArchetype type = GameCatalog.GirlTypes[t];
                int c = _state.girlCounts[type.Index];
                for (int i = 0; i < c && added < maxVis; i++)
                {
                    Vector2 slot = GetBlockSlotTarget(added, total);
                    _followers.Add(CreateGirlEntity(type, slot, true));
                    added++;
                }
            }
        }

        // =====================================================================
        // ANIMATION DE SACRIFICE AMELIOREE (RITUEL MULTI-PHASES)
        // =====================================================================
        public void SacrificeCrowd(bool atAltar)
        {
            int count = GetTotalGirls();
            if (count <= 0) return;

            double payout = GetCrowdSacrificeValue(atAltar);
            Vector2 center = atAltar ? WorldConfig.AltarPos : _playerPos;

            for (int i = 0; i < GameCatalog.GirlTypes.Length; i++)
            {
                _state.sacrificedCounts[i] += _state.girlCounts[i];
                _state.girlCounts[i] = 0;
            }
            _state.lifetimeSacrificed += count;
            _altarTimer = 0f;

            foreach (GirlEntity f in _followers)
            {
                f.HeartIndicatorSr.enabled = false;
                Vector2 rel = f.Pos - center;
                float ang = Mathf.Atan2(rel.y, rel.x);
                float rad = Mathf.Max(0.8f, rel.magnitude);

                GameObject beamGo = new GameObject("SacrificeBeam");
                beamGo.transform.position = new Vector3(f.Pos.x, f.Pos.y, 0f);
                beamGo.transform.localScale = new Vector3(0.75f, 0.1f, 1f);
                SpriteRenderer beamSr = beamGo.AddComponent<SpriteRenderer>();
                beamSr.sprite = PixelSpriteFactory.GetLightBeamSprite();
                beamSr.sortingOrder = 5000;

                _sacrificeGhosts.Add(new SacrificeGhost
                {
                    Go = f.Go,
                    Sr = f.Sr,
                    BeamGo = beamGo,
                    BeamSr = beamSr,
                    CenterPos = center,
                    Angle = ang,
                    StartRadius = rad,
                    Elapsed = 0f,
                    Duration = 0.85f
                });
            }
            _followers.Clear();

            GameObject sigilGo = new GameObject("RitualSigil");
            sigilGo.transform.position = new Vector3(center.x, center.y, 0f);
            SpriteRenderer sigilSr = sigilGo.AddComponent<SpriteRenderer>();
            sigilSr.sprite = PixelSpriteFactory.GetPentagramSigilSprite("ritual_sigil_fx", 96, GameCatalog.Hex("#f43f5e"), GameCatalog.Hex("#fde047"));
            sigilSr.sortingOrder = -9950;
            sigilGo.transform.localScale = Vector3.one * 0.4f;

            GameObject w1Go = new GameObject("SacWave1");
            w1Go.transform.position = new Vector3(center.x, center.y, 0f);
            SpriteRenderer w1Sr = w1Go.AddComponent<SpriteRenderer>();
            w1Sr.sprite = PixelSpriteFactory.GetRingSprite("sac_wave_red", 64, Color.clear, GameCatalog.Hex("#e11d48"), 4f);
            w1Sr.sortingOrder = 5100;
            w1Sr.enabled = false;

            GameObject w2Go = new GameObject("SacWave2");
            w2Go.transform.position = new Vector3(center.x, center.y, 0f);
            SpriteRenderer w2Sr = w2Go.AddComponent<SpriteRenderer>();
            w2Sr.sprite = PixelSpriteFactory.GetRingSprite("sac_wave_gold", 64, Color.clear, GameCatalog.Hex("#fde047"), 3f);
            w2Sr.sortingOrder = 5101;
            w2Sr.enabled = false;

            _ritualFx.Add(new RitualSigilFx
            {
                Go = sigilGo,
                Sr = sigilSr,
                Wave1Go = w1Go,
                Wave1Sr = w1Sr,
                Wave2Go = w2Go,
                Wave2Sr = w2Sr,
                Elapsed = 0f,
                Duration = 1.35f,
                ClimaxDone = false,
                PendingPayout = payout,
                AtAltar = atAltar,
                SacrificedCount = count
            });

            _audio.Play("sacrifice_start");
        }

        private void UpdateSacrificeAnimations(float dt)
        {
            for (int i = _sacrificeGhosts.Count - 1; i >= 0; i--)
            {
                SacrificeGhost g = _sacrificeGhosts[i];
                g.Elapsed += dt;
                float t = Mathf.Clamp01(g.Elapsed / g.Duration);

                float spiralT = Mathf.Clamp01((t - 0.25f) / 0.75f);
                float curRadius = Mathf.Lerp(g.StartRadius, 0.1f, spiralT * spiralT);
                float curAngle = g.Angle + spiralT * 5.2f;
                float liftY = Mathf.Sin(t * Mathf.PI) * 2.2f;

                Vector2 pos = g.CenterPos + new Vector2(Mathf.Cos(curAngle) * curRadius, Mathf.Sin(curAngle) * curRadius);
                g.Go.transform.position = new Vector3(pos.x, pos.y + liftY, 0f);
                g.Go.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.25f, spiralT * spiralT);
                g.Sr.color = Color.Lerp(GameCatalog.Hex("#fb7185"), GameCatalog.Hex("#fde047"), t);
                g.Sr.sortingOrder = 5050;

                g.BeamGo.transform.position = new Vector3(pos.x, pos.y, 0f);
                float beamHeight = Mathf.Sin(t * Mathf.PI) * 3.8f;
                g.BeamGo.transform.localScale = new Vector3(Mathf.Lerp(0.9f, 0.2f, t), Mathf.Max(0.05f, beamHeight), 1f);
                g.BeamSr.color = new Color(1f, 1f, 1f, 1f - t * 0.75f);

                if (t >= 1f)
                {
                    SpawnBurst(pos, 4, GameCatalog.Hex("#e11d48"));
                    for (int k = 0; k < 3; k++)
                        SpawnParticle(pos + UnityEngine.Random.insideUnitCircle * 0.5f, UnityEngine.Random.insideUnitCircle * 2.5f, 0.75f, GameCatalog.Hex("#e11d48"), ParticleKind.BloodOrb, 1f);

                    Destroy(g.Go);
                    Destroy(g.BeamGo);
                    _sacrificeGhosts.RemoveAt(i);
                }
            }

            for (int i = _ritualFx.Count - 1; i >= 0; i--)
            {
                RitualSigilFx fx = _ritualFx[i];
                fx.Elapsed += dt;
                float t = Mathf.Clamp01(fx.Elapsed / fx.Duration);

                fx.Go.transform.Rotate(0f, 0f, -dt * (120f + t * 240f));
                float sigilScale = t < 0.6f ? Mathf.Lerp(0.4f, 4.4f, t / 0.6f) : Mathf.Lerp(4.4f, 5.4f, (t - 0.6f) / 0.4f);
                fx.Go.transform.localScale = Vector3.one * sigilScale;
                fx.Sr.color = new Color(1f, 1f, 1f, t < 0.65f ? 0.9f : Mathf.Lerp(0.9f, 0f, (t - 0.65f) / 0.35f));

                if (!fx.ClimaxDone && fx.Elapsed >= 0.78f)
                {
                    fx.ClimaxDone = true;
                    _state.blood += fx.PendingPayout;
                    _state.lifetimeBlood += fx.PendingPayout;

                    TriggerShake(0.32f, 0.38f);
                    _crimsonFlashAlpha = 0.42f;

                    if (_state.moonTimeLeft <= 0f)
                    {
                        _state.moonCharge = Mathf.Min(100f, _state.moonCharge + fx.SacrificedCount * 7f);
                        if (_state.moonCharge >= 100f)
                        {
                            _state.moonCharge = 100f;
                            _state.moonTimeLeft = 18f;
                            _audio.Play("moon");
                            ShowToast("LUNE DE SANG ! x2 Sang & Vitesse pendant 18s !");
                        }
                        else
                        {
                            _audio.Play("sacrifice_boom");
                        }
                    }
                    else
                    {
                        _audio.Play("sacrifice_boom");
                    }

                    string tag = fx.AtAltar ? " (AUTEL +50%)" : "";
                    SpawnFloatLabel(_playerPos + Vector2.up * 1.8f, "+" + GameCatalog.FormatNumber(fx.PendingPayout) + " SANG" + tag, GameCatalog.Hex("#fde047"));
                    ShowToast($"{fx.SacrificedCount} fidele(s) sacrifiee(s) : +{GameCatalog.FormatNumber(fx.PendingPayout)} Sang{tag}");
                    SaveGame(false);
                }

                if (fx.ClimaxDone)
                {
                    float wt = Mathf.Clamp01((fx.Elapsed - 0.78f) / 0.55f);
                    fx.Wave1Sr.enabled = true;
                    fx.Wave2Sr.enabled = true;
                    fx.Wave1Go.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 9.5f, wt);
                    fx.Wave1Sr.color = new Color(1f, 1f, 1f, 1f - wt);
                    fx.Wave2Go.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 6.8f, wt);
                    fx.Wave2Sr.color = new Color(1f, 1f, 1f, 1f - wt);
                }

                if (fx.Elapsed >= fx.Duration)
                {
                    Destroy(fx.Go);
                    Destroy(fx.Wave1Go);
                    Destroy(fx.Wave2Go);
                    _ritualFx.RemoveAt(i);
                }
            }
        }

        // =====================================================================
        // CHASSEUR DE VAMPIRE & LOUPS-GAROUS (MENACES MORTELLES)
        // =====================================================================
        private void UpdateThreatsAndEnemies(float dt)
        {
            int activeHunters = 0;
            int activeWolves = 0;
            foreach (EnemyEntity e in _enemies)
            {
                if (e.IsHunter) activeHunters++;
                else activeWolves++;
            }

            int followersCount = GetTotalGirls();
            if (followersCount >= hunterFollowerThreshold && activeHunters == 0)
            {
                _hunterCheckTimer -= dt;
                if (_hunterCheckTimer <= 0f)
                {
                    _hunterCheckTimer = hunterCheckInterval;
                    float chance = Mathf.Min(0.85f, hunterBaseChance + (followersCount - hunterFollowerThreshold) * 0.05f);
                    if (UnityEngine.Random.value < chance)
                        SpawnEnemy(true);
                }
            }
            else
            {
                _hunterCheckTimer = hunterCheckInterval * 0.6f;
            }

            if (_state.blood >= werewolfBloodThreshold)
            {
                int maxWolves = _state.blood >= 15000 ? 3 : (_state.blood >= 2500 ? 2 : 1);
                if (activeWolves < maxWolves)
                {
                    _werewolfCheckTimer -= dt;
                    if (_werewolfCheckTimer <= 0f)
                    {
                        _werewolfCheckTimer = werewolfCheckInterval;
                        if (UnityEngine.Random.value < werewolfSpawnChance)
                            SpawnEnemy(false);
                    }
                }
            }
            else
            {
                _werewolfCheckTimer = werewolfCheckInterval * 0.6f;
            }

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                EnemyEntity e = _enemies[i];
                e.LifeLeft -= dt;

                if (e.IsHunter && followersCount == 0 && e.LifeLeft > 4f) e.LifeLeft = 4f;
                if (!e.IsHunter && _state.blood < werewolfBloodThreshold && e.LifeLeft > 4f) e.LifeLeft = 4f;

                if (e.LifeLeft <= 0f)
                {
                    ShowToast(e.IsHunter ? "Le Chasseur de Vampire a perdu ta trace." : "Un Loup-Garou s'est retire dans la nuit.");
                    Destroy(e.Go);
                    _enemies.RemoveAt(i);
                    continue;
                }

                Vector2 toPlayer = _playerPos - e.Pos;
                float dist = toPlayer.magnitude;
                Vector2 dir = dist > 0.01f ? toPlayer / dist : Vector2.zero;

                float spd;
                if (e.IsHunter)
                {
                    spd = hunterSpeed;
                }
                else
                {
                    e.LungeTimer -= dt;
                    if (e.LungeTimer <= 0f)
                    {
                        e.LungeTimer = 3.5f;
                        e.LungeActive = 0.75f;
                    }
                    if (e.LungeActive > 0f)
                    {
                        e.LungeActive -= dt;
                        spd = werewolfBaseSpeed * 1.38f;
                    }
                    else
                    {
                        spd = werewolfBaseSpeed;
                    }
                }

                float nx = e.Pos.x + dir.x * spd * dt;
                float ny = e.Pos.y + dir.y * spd * dt;
                bool movedX = false;
                bool movedY = false;

                if (!IsPointBlocked(nx, e.Pos.y, 0.5f)) { e.Pos.x = nx; movedX = true; }
                if (!IsPointBlocked(e.Pos.x, ny, 0.5f)) { e.Pos.y = ny; movedY = true; }

                if (!movedX && Mathf.Abs(dir.x) > 0.2f)
                {
                    float slideY = e.Pos.y + (dir.y >= 0 ? 1f : -1f) * spd * dt;
                    if (!IsPointBlocked(e.Pos.x, slideY, 0.5f)) e.Pos.y = slideY;
                }
                if (!movedY && Mathf.Abs(dir.y) > 0.2f)
                {
                    float slideX = e.Pos.x + (dir.x >= 0 ? 1f : -1f) * spd * dt;
                    if (!IsPointBlocked(slideX, e.Pos.y, 0.5f)) e.Pos.x = slideX;
                }

                e.WalkTimer += dt * (e.IsHunter ? 8.5f : 11f);
                int frame = Mathf.FloorToInt(e.WalkTimer) % 2;

                e.Go.transform.position = new Vector3(e.Pos.x, e.Pos.y, 0f);
                e.Sr.sortingOrder = Mathf.RoundToInt(-e.Pos.y * 100f);
                e.Sr.sprite = e.IsHunter
                    ? PixelSpriteFactory.GetHunterSprite(frame)
                    : PixelSpriteFactory.GetWerewolfSprite(frame);
                e.Sr.flipX = dir.x < -0.1f;

                float pulse = 1.25f + Mathf.Sin(Time.time * 8f) * 0.15f;
                e.AlertRingSr.transform.localScale = Vector3.one * pulse;

                if (Vector2.Distance(e.Pos, _playerPos) < 0.92f)
                {
                    if (_dashInvulnTimer > 0f)
                    {
                        if (!_dashDodgeNotified)
                        {
                            _dashDodgeNotified = true;
                            SpawnFloatLabel(_playerPos + Vector2.up * 1.4f, "ESQUIVE !", GameCatalog.Hex("#c084fc"));
                        }
                    }
                    else
                    {
                        TriggerDeath(e.IsHunter);
                        return;
                    }
                }
            }
        }

        private void SpawnEnemy(bool isHunter)
        {
            Vector2 spawnPos = _playerPos + new Vector2(15f, 0f);
            for (int tries = 0; tries < 30; tries++)
            {
                float ang = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float dist = UnityEngine.Random.Range(14f, 19f);
                Vector2 cand = _playerPos + new Vector2(Mathf.Cos(ang) * dist, Mathf.Sin(ang) * dist);
                if (!IsPointBlocked(cand.x, cand.y, 0.9f))
                {
                    spawnPos = cand;
                    break;
                }
            }

            GameObject go = new GameObject(isHunter ? "VampireHunter" : "Werewolf");
            go.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = isHunter ? PixelSpriteFactory.GetHunterSprite(0) : PixelSpriteFactory.GetWerewolfSprite(0);

            GameObject ringGo = new GameObject("ThreatRing");
            ringGo.transform.SetParent(go.transform);
            ringGo.transform.localPosition = Vector3.zero;
            SpriteRenderer ringSr = ringGo.AddComponent<SpriteRenderer>();
            Color ringCol = isHunter ? GameCatalog.Hex("#38bdf8") : GameCatalog.Hex("#f97316");
            ringSr.sprite = PixelSpriteFactory.GetRingSprite(isHunter ? "ring_hunter" : "ring_wolf", 48, new Color(ringCol.r, ringCol.g, ringCol.b, 0.14f), ringCol, 2.5f);
            ringSr.sortingOrder = -9960;

            _enemies.Add(new EnemyEntity
            {
                IsHunter = isHunter,
                Go = go,
                Sr = sr,
                AlertRingSr = ringSr,
                Pos = spawnPos,
                LifeLeft = isHunter ? hunterHuntDuration : werewolfHuntDuration,
                LungeTimer = 2.5f
            });

            if (isHunter)
            {
                _audio.Play("hunter_alert");
                ShowToast("DANGER : Un Chasseur de Vampire a repere ta meute !");
            }
            else
            {
                _audio.Play("wolf_howl");
                ShowToast("DANGER : Un Loup-Garou est attire par ton sang !");
            }
        }

        private void TriggerDeath(bool killedByHunter)
        {
            _isDead = true;
            _pointerTarget = null;
            _dashTimer = 0f;
            _crimsonFlashAlpha = 0.7f;
            TriggerShake(0.45f, 0.55f);
            _audio.Play("death");

            _deathTitle = "VOUS ETES MORT";
            _deathReason = killedByHunter
                ? "Un Chasseur de Vampire vous a rattrape et transperce le coeur. Votre meute etait trop voyante !"
                : "Un Loup-Garou attire par votre reserve de sang vous a devore. Depensez votre sang plus souvent !";

            PlayerPrefs.DeleteKey(WorldConfig.SaveKey);
            PlayerPrefs.Save();
        }

        private void RestartAfterDeath()
        {
            _isDead = false;
            ResetEverythingInternal();
            ShowToast("Nouvelle nuit commencee. Utilise SHIFT pour le Dash Chauve-Souris !");
        }

        private void ResetEverythingInternal()
        {
            PlayerPrefs.DeleteKey(WorldConfig.SaveKey);
            PlayerPrefs.Save();

            bool keepSound = _state != null ? _state.soundOn : true;
            _state = new SaveState { soundOn = keepSound };
            _state.EnsureValid();

            ClearList(_followers);
            ClearList(_roamingGirls);
            ClearEnemies();
            ClearRitualsAndParticles();

            _playerPos = WorldConfig.PlayerStartPos;
            _altarTimer = 0f;
            _pulseCooldown = 0f;
            _dashTimer = 0f;
            _dashInvulnTimer = 0f;
            _dashCooldown = 0f;
            _hunterCheckTimer = hunterCheckInterval;
            _werewolfCheckTimer = werewolfCheckInterval;

            for (int i = 0; i < 14; i++) SpawnRoamingGirl();
            RefreshPlayerSprite();
        }

        // =====================================================================
        // CLIN D'OEIL, PARTICULES, CAMERA & COLLISIONS
        // =====================================================================
        public void TriggerCharmPulse()
        {
            if (_pulseCooldown > 0f || _isDead) return;
            _pulseCooldown = _state.upgrades.Contains("fangs") ? 1.8f : 2.8f;
            _pulseWaveScale = 0.8f;
            _pulseWaveAlpha = 0.95f;
            _pulseWaveSr.enabled = true;
            _audio.Play("pulse");

            float pulseRange = GetCharmAuraRadius() * 1.95f;
            int myAllure = GetAllure();

            for (int i = _roamingGirls.Count - 1; i >= 0; i--)
            {
                GirlEntity g = _roamingGirls[i];
                if (myAllure >= g.Type.ReqAllure && Vector2.Distance(g.Pos, _playerPos) <= pulseRange)
                {
                    g.CharmProgress += 0.72f;
                    SpawnBurst(g.Pos + Vector2.up * 0.7f, 3, GameCatalog.Hex("#f43f5e"));
                    if (g.CharmProgress >= 1f) CollectGirl(g, i);
                }
            }
            SpawnBurst(_playerPos + Vector2.up * 0.7f, 5, GameCatalog.Hex("#fb7185"));
        }

        private void SpawnBurst(Vector2 pos, int count, Color color)
        {
            for (int i = 0; i < count; i++)
            {
                float a = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float spd = UnityEngine.Random.Range(1.2f, 3.0f);
                SpawnParticle(pos, new Vector2(Mathf.Cos(a) * spd, Mathf.Sin(a) * spd + 1.0f), UnityEngine.Random.Range(0.55f, 0.95f), color, ParticleKind.Heart, 0.75f);
            }
        }

        private void SpawnParticle(Vector2 pos, Vector2 vel, float life, Color color, ParticleKind kind, float scale)
        {
            GameObject go = new GameObject("FxParticle");
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = Vector3.one * scale;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();

            if (kind == ParticleKind.BloodOrb)
                sr.sprite = PixelSpriteFactory.GetBloodOrbSprite();
            else if (kind == ParticleKind.MiniBat)
                sr.sprite = PixelSpriteFactory.GetMiniBatParticleSprite();
            else
                sr.sprite = PixelSpriteFactory.GetPixelHeartSprite(color);

            sr.sortingOrder = 6000;

            _particles.Add(new ParticleFx
            {
                Go = go,
                Sr = sr,
                Pos = pos,
                Vel = vel,
                Life = life,
                MaxLife = life,
                Kind = kind
            });
        }

        private void SpawnFloatLabel(Vector2 worldPos, string text, Color col)
        {
            _floatLabels.Add(new FloatingLabel
            {
                WorldPos = worldPos,
                Text = text,
                Color = col,
                Life = 1.3f,
                MaxLife = 1.3f
            });
        }

        private void UpdateParticlesAndLabels(float dt)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                ParticleFx p = _particles[i];
                p.Life -= dt;
                if (p.Life <= 0f)
                {
                    Destroy(p.Go);
                    _particles.RemoveAt(i);
                    continue;
                }

                if (p.Kind == ParticleKind.BloodOrb)
                {
                    Vector2 target = _playerPos + Vector2.up * 0.6f;
                    p.Pos = Vector2.Lerp(p.Pos, target, Mathf.Min(1f, dt * 8.5f));
                }
                else
                {
                    p.Pos += p.Vel * dt;
                }

                p.Go.transform.position = new Vector3(p.Pos.x, p.Pos.y, 0f);
                Color c = p.Sr.color;
                c.a = Mathf.Clamp01(p.Life / p.MaxLife);
                p.Sr.color = c;
            }

            for (int i = _floatLabels.Count - 1; i >= 0; i--)
            {
                FloatingLabel fl = _floatLabels[i];
                fl.Life -= dt;
                fl.WorldPos += Vector2.up * (1.6f * dt);
                if (fl.Life <= 0f) _floatLabels.RemoveAt(i);
            }
        }

        private void TriggerShake(float duration, float intensity)
        {
            _shakeTimer = Mathf.Max(_shakeTimer, duration);
            _shakeIntensity = Mathf.Max(_shakeIntensity, intensity);
        }

        private void UpdateCamera(float dt)
        {
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            float tx = Mathf.Clamp(_playerPos.x, halfW, WorldConfig.WorldW - halfW);
            float ty = Mathf.Clamp(_playerPos.y, halfH, WorldConfig.WorldH - halfH);

            Vector3 cur = _cam.transform.position;
            Vector3 next = Vector3.Lerp(cur, new Vector3(tx, ty, -10f), Mathf.Min(1f, dt * 8f));

            if (_shakeTimer > 0f)
            {
                _shakeTimer = Mathf.Max(0f, _shakeTimer - dt);
                Vector2 off = UnityEngine.Random.insideUnitCircle * _shakeIntensity * (_shakeTimer / 0.35f);
                next.x += off.x;
                next.y += off.y;
            }

            _cam.transform.position = next;
        }

        private bool IsPointBlocked(float x, float y, float pad)
        {
            if (x < WorldConfig.BorderPad + pad || x > WorldConfig.WorldW - WorldConfig.BorderPad - pad ||
                y < WorldConfig.BorderPad + pad || y > WorldConfig.WorldH - WorldConfig.BorderPad - pad)
                return true;

            foreach (BuildingData b in GameCatalog.Buildings)
            {
                if (x > b.Bounds.xMin - pad && x < b.Bounds.xMax + pad &&
                    y > b.Bounds.yMin - pad && y < b.Bounds.yMax - 1.5f + pad)
                    return true;
            }

            foreach (TreeData t in GameCatalog.Trees)
            {
                if (Vector2.Distance(new Vector2(x, y), t.Position) < 0.65f + pad * 0.45f)
                    return true;
            }
            return false;
        }

        // =====================================================================
        // FORMULES DE PROGRESSION, BOUTIQUE & PRESTIGE
        // =====================================================================
        private int GetTotalGirls()
        {
            int s = 0;
            for (int i = 0; i < _state.girlCounts.Length; i++) s += _state.girlCounts[i];
            return s;
        }

        private int GetAllure()
        {
            int a = 1 + _state.roses * 2;
            foreach (SexyUpgrade u in GameCatalog.SexyUpgrades)
                if (_state.upgrades.Contains(u.Id)) a += u.Allure;
            a += _state.repGlamour * 3;
            a += _state.lifetimeSacrificed / 15;
            return a;
        }

        private double GetBloodMultiplier()
        {
            double m = 1.0 + GetAllure() * 0.06;
            foreach (SexyUpgrade u in GameCatalog.SexyUpgrades)
                if (_state.upgrades.Contains(u.Id) && u.BloodMult > 0) m *= u.BloodMult;
            m *= Math.Pow(1.3, _state.repGlamour);
            m *= 1.0 + _state.roses * 0.35;
            if (_state.moonTimeLeft > 0f) m *= 2.0;
            return m;
        }

        private double GetCrowdComboMult()
        {
            int c = GetTotalGirls();
            return c <= 1 ? 1.0 : 1.0 + (c - 1) * 0.08;
        }

        private bool IsPlayerAtAltar()
        {
            return Vector2.Distance(_playerPos, WorldConfig.AltarPos) <= WorldConfig.AltarRadius + 0.6f;
        }

        private double GetCrowdSacrificeValue(bool atAltar)
        {
            double baseVal = 0;
            for (int i = 0; i < GameCatalog.GirlTypes.Length; i++)
                baseVal += _state.girlCounts[i] * GameCatalog.GirlTypes[i].SacrificeBlood;
            if (baseVal <= 0) return 0;
            return Math.Round(baseVal * GetBloodMultiplier() * GetCrowdComboMult() * (atAltar ? 1.5 : 1.0));
        }

        private float GetCharmAuraRadius()
        {
            float r = 2.9f;
            foreach (SexyUpgrade u in GameCatalog.SexyUpgrades)
                if (_state.upgrades.Contains(u.Id)) r += u.AuraBonus;
            r += _state.repBoots * 0.5f;
            if (_state.moonTimeLeft > 0f) r += 1.4f;
            return r;
        }

        private float GetCharmSpeedMult()
        {
            float s = 1f;
            foreach (SexyUpgrade u in GameCatalog.SexyUpgrades)
                if (_state.upgrades.Contains(u.Id)) s += u.CharmSpeedBonus;
            if (_state.moonTimeLeft > 0f) s *= 1.8f;
            return s;
        }

        private float GetPlayerSpeed()
        {
            float m = 1f;
            foreach (SexyUpgrade u in GameCatalog.SexyUpgrades)
                if (_state.upgrades.Contains(u.Id)) m += u.SpeedBonus;
            m += _state.repBoots * 0.12f;
            if (_state.moonTimeLeft > 0f) m *= 1.3f;
            return 7.2f * m;
        }

        private int GetMaxRoamingGirls() => 16 + _state.repParty * 4;

        private int GetRepLevel(string id)
        {
            if (id == "rep_glamour") return _state.repGlamour;
            if (id == "rep_boots") return _state.repBoots;
            return _state.repParty;
        }

        private double GetRepCost(RepeatableUpgrade rep)
        {
            return Math.Floor(rep.BaseCost * Math.Pow(rep.CostMult, GetRepLevel(rep.Id)));
        }

        private int GetClaimableRoses()
        {
            if (_state.lifetimeBlood < 25000) return 0;
            int total = (int)Math.Floor(Math.Pow(_state.lifetimeBlood / 8000.0, 1.0 / 3.0));
            return Math.Max(0, total - _state.roses);
        }

        private void BuySexyUpgrade(SexyUpgrade u)
        {
            if (_state.upgrades.Contains(u.Id) || _state.blood < u.Cost) return;
            _state.blood -= u.Cost;
            _state.upgrades.Add(u.Id);
            _audio.Play("buy");
            SpawnBurst(_playerPos + Vector2.up * 0.8f, 10, GameCatalog.Hex("#fde047"));
            ShowToast("Amelioration acquise : " + u.Name + " (+" + u.Allure + " Charme)");
            RefreshPlayerSprite();
            SaveGame(false);
        }

        private void BuyRepeatable(RepeatableUpgrade rep)
        {
            int lvl = GetRepLevel(rep.Id);
            if (lvl >= rep.MaxLevel) return;
            double cost = GetRepCost(rep);
            if (_state.blood < cost) return;

            _state.blood -= cost;
            if (rep.Id == "rep_glamour") _state.repGlamour++;
            else if (rep.Id == "rep_boots") _state.repBoots++;
            else _state.repParty++;

            _audio.Play("buy");
            ShowToast(rep.Name + " Niv." + (lvl + 1));
            SaveGame(false);
        }

        private void PerformPrestige()
        {
            int claim = GetClaimableRoses();
            if (claim <= 0) return;

            _state.roses += claim;
            _state.semesters++;
            _state.blood = 0;
            _state.upgrades.Clear();
            _state.repGlamour = 0;
            _state.repBoots = 0;
            _state.repParty = 0;
            for (int i = 0; i < 6; i++)
            {
                _state.girlCounts[i] = 0;
                _state.sacrificedCounts[i] = 0;
            }
            _state.moonCharge = 0f;
            _state.moonTimeLeft = 0f;

            ClearList(_followers);
            ClearList(_roamingGirls);
            ClearEnemies();
            _playerPos = WorldConfig.PlayerStartPos;
            _altarTimer = 0f;
            for (int i = 0; i < 14; i++) SpawnRoamingGirl();

            _audio.Play("moon");
            ShowToast("Semestre " + (_state.semesters + 1) + " lance avec " + _state.roses + " Roses !");
            SaveGame(false);
        }

        // =====================================================================
        // SAUVEGARDE & UTILITAIRES
        // =====================================================================
        private void LoadGame()
        {
            if (PlayerPrefs.HasKey(WorldConfig.SaveKey))
            {
                try
                {
                    _state = JsonUtility.FromJson<SaveState>(PlayerPrefs.GetString(WorldConfig.SaveKey));
                }
                catch { _state = new SaveState(); }
            }
            if (_state == null) _state = new SaveState();
            _state.EnsureValid();
        }

        private void SaveGame(bool toast)
        {
            PlayerPrefs.SetString(WorldConfig.SaveKey, JsonUtility.ToJson(_state));
            PlayerPrefs.Save();
            if (toast) ShowToast("Partie sauvegardee !");
        }

        private void ShowToast(string msg)
        {
            _toasts.Add((msg, 3.2f));
            if (_toasts.Count > 4) _toasts.RemoveAt(0);
        }

        private void ClearList(List<GirlEntity> list)
        {
            foreach (GirlEntity g in list) if (g.Go != null) Destroy(g.Go);
            list.Clear();
        }

        private void ClearEnemies()
        {
            foreach (EnemyEntity e in _enemies) if (e.Go != null) Destroy(e.Go);
            _enemies.Clear();
        }

        private void ClearRitualsAndParticles()
        {
            foreach (SacrificeGhost g in _sacrificeGhosts) { if (g.Go) Destroy(g.Go); if (g.BeamGo) Destroy(g.BeamGo); }
            _sacrificeGhosts.Clear();
            foreach (RitualSigilFx fx in _ritualFx) { if (fx.Go) Destroy(fx.Go); if (fx.Wave1Go) Destroy(fx.Wave1Go); if (fx.Wave2Go) Destroy(fx.Wave2Go); }
            _ritualFx.Clear();
            foreach (ParticleFx p in _particles) if (p.Go) Destroy(p.Go);
            _particles.Clear();
            _floatLabels.Clear();
        }

        private string GetCurrentZoneName()
        {
            if (_playerPos.x < 34f && _playerPos.y > 38f) return "BIBLIOTHEQUE";
            if (_playerPos.x > 56f && _playerPos.y > 38f) return "CAFE LUNAIRE";
            if (_playerPos.x < 34f && _playerPos.y < 27f) return "SORORITE";
            if (_playerPos.x > 56f && _playerPos.y < 27f) return "OBSERVATOIRE";
            return "COUR CENTRALE";
        }

        private bool IsPointerOverUi()
        {
            float mx = Input.mousePosition.x;
            float my = Screen.height - Input.mousePosition.y;
            float shopW = Mathf.Min(340f, Screen.width * 0.30f);
            if (mx >= Screen.width - shopW - 10f) return true;
            if (my <= 68f || my >= Screen.height - 76f) return true;
            return false;
        }

        // =====================================================================
        // INTERFACE GOTHIQUE SANS EMOJI
        // =====================================================================
        private void InitStyles()
        {
            if (_stylesInit) return;
            _stylesInit = true;

            _panelStyle = MakeBoxStyle(new Color(0.09f, 0.04f, 0.14f, 0.92f));
            _cardStyle = MakeBoxStyle(new Color(0.14f, 0.07f, 0.21f, 0.88f));
            _cardBuyStyle = MakeBoxStyle(new Color(0.28f, 0.08f, 0.24f, 0.94f));
            _btnRedStyle = MakeButtonStyle(GameCatalog.Hex("#be123c"), GameCatalog.Hex("#e11d48"));
            _btnGoldStyle = MakeButtonStyle(GameCatalog.Hex("#b45309"), GameCatalog.Hex("#d97706"));
            _btnPurpleStyle = MakeButtonStyle(GameCatalog.Hex("#6b21a8"), GameCatalog.Hex("#9333ea"));
            _btnDarkStyle = MakeButtonStyle(new Color(0.18f, 0.10f, 0.25f, 0.95f), new Color(0.28f, 0.14f, 0.38f, 0.95f));

            _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold };
            _titleStyle.normal.textColor = GameCatalog.Hex("#ffe4e6");

            _boldStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = FontStyle.Bold, wordWrap = true };
            _boldStyle.normal.textColor = Color.white;

            _smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
            _smallStyle.normal.textColor = GameCatalog.Hex("#fecdd3");

            _centerBoldStyle = new GUIStyle(_boldStyle) { alignment = TextAnchor.MiddleCenter };

            _alertBannerStyle = MakeBoxStyle(new Color(0.55f, 0.05f, 0.12f, 0.92f));
            _alertBannerStyle.alignment = TextAnchor.MiddleCenter;
            _alertBannerStyle.fontStyle = FontStyle.Bold;
            _alertBannerStyle.fontSize = 13;
            _alertBannerStyle.normal.textColor = GameCatalog.Hex("#fef08a");
        }

        private GUIStyle MakeBoxStyle(Color bg)
        {
            Texture2D t = PixelSpriteFactory.CreateBlankTex(2, 2);
            for (int i = 0; i < 4; i++) t.SetPixel(i % 2, i / 2, bg);
            t.Apply();
            GUIStyle st = new GUIStyle(GUI.skin.box);
            st.normal.background = t;
            st.padding = new RectOffset(8, 8, 6, 6);
            return st;
        }

        private GUIStyle MakeButtonStyle(Color normal, Color hover)
        {
            Texture2D tn = PixelSpriteFactory.CreateBlankTex(2, 2);
            Texture2D th = PixelSpriteFactory.CreateBlankTex(2, 2);
            for (int i = 0; i < 4; i++) { tn.SetPixel(i % 2, i / 2, normal); th.SetPixel(i % 2, i / 2, hover); }
            tn.Apply(); th.Apply();

            GUIStyle st = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            st.normal.background = tn;
            st.hover.background = th;
            st.active.background = normal;
            st.normal.textColor = Color.white;
            st.hover.textColor = Color.white;
            return st;
        }

        private void OnGUI()
        {
            InitStyles();

            if (_state.moonTimeLeft > 0f)
            {
                GUI.color = new Color(0.88f, 0.11f, 0.28f, 0.10f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _whiteTex);
                GUI.color = Color.white;
            }
            if (_crimsonFlashAlpha > 0f)
            {
                GUI.color = new Color(0.88f, 0.08f, 0.22f, _crimsonFlashAlpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _whiteTex);
                GUI.color = Color.white;
            }

            DrawWorldLabels();

            float shopW = Mathf.Min(340f, Screen.width * 0.30f);
            float playW = Screen.width - shopW - 18f;

            DrawTopHud(playW);
            DrawThreatBanners(playW);
            DrawMinimap(playW - 138f, 76f, 128f, 92f);
            DrawBottomActionBar(playW);
            DrawRightPanel(Screen.width - shopW - 8f, 8f, shopW, Screen.height - 16f);

            for (int i = 0; i < _toasts.Count; i++)
            {
                Rect tr = new Rect(12f, Screen.height - 118f - i * 34f, Mathf.Min(440f, playW - 24f), 28f);
                GUI.Box(tr, _toasts[i].msg, _panelStyle);
            }

            if (_showConfirmModal) DrawConfirmModal();
            if (_isDead) DrawDeathOverlay();
        }

        private void DrawWorldLabels()
        {
            Vector3 altarScreen = _cam.WorldToScreenPoint(new Vector3(WorldConfig.AltarPos.x, WorldConfig.AltarPos.y + 4.4f, 0f));
            if (altarScreen.z > 0)
            {
                float y = Screen.height - altarScreen.y;
                string txt = IsPlayerAtAltar() && GetTotalGirls() > 0 ? "SACRIFICE +50% !" : "AUTEL CRAMOISI (+50% SANG)";
                GUI.Box(new Rect(altarScreen.x - 95f, y - 11f, 190f, 22f), txt, _panelStyle);
            }

            foreach (BuildingData b in GameCatalog.Buildings)
            {
                Vector3 bs = _cam.WorldToScreenPoint(new Vector3(b.Bounds.center.x, b.Bounds.yMax - 1.5f, 0f));
                if (bs.z > 0)
                {
                    float y = Screen.height - bs.y;
                    GUI.Label(new Rect(bs.x - 70f, y - 10f, 140f, 20f), b.DisplayName, _centerBoldStyle);
                }
            }

            foreach (FloatingLabel fl in _floatLabels)
            {
                Vector3 sp = _cam.WorldToScreenPoint(new Vector3(fl.WorldPos.x, fl.WorldPos.y, 0f));
                if (sp.z <= 0) continue;
                float y = Screen.height - sp.y;
                Color old = GUI.color;
                GUI.color = new Color(fl.Color.r, fl.Color.g, fl.Color.b, Mathf.Clamp01(fl.Life / fl.MaxLife));
                GUI.Label(new Rect(sp.x - 110f, y - 12f, 220f, 24f), fl.Text, _centerBoldStyle);
                GUI.color = old;
            }
        }

        private void DrawTopHud(float playW)
        {
            Rect r = new Rect(8f, 8f, playW, 58f);
            GUI.Box(r, GUIContent.none, _panelStyle);

            GUI.Label(new Rect(18f, 14f, 190f, 22f), "CRIMSON CAMPUS", _titleStyle);
            GUI.Label(new Rect(18f, 36f, 190f, 18f), GetCurrentZoneName() + " | Sem " + (_state.semesters + 1), _smallStyle);

            float sx = 205f;
            float statW = Mathf.Max(95f, (playW - 320f) / 4f);
            DrawStatChip(sx, 14f, statW - 6f, "blood", GameCatalog.FormatNumber(_state.blood), "Sang (Meute +" + GameCatalog.FormatNumber(GetCrowdSacrificeValue(IsPlayerAtAltar())) + ")");
            DrawStatChip(sx + statW, 14f, statW - 6f, "heart", GetTotalGirls().ToString(), "Combo x" + GetCrowdComboMult().ToString("0.00"));
            DrawStatChip(sx + statW * 2f, 14f, statW - 6f, "star", GetAllure().ToString(), "Charme");
            DrawStatChip(sx + statW * 3f, 14f, statW - 6f, "rose", _state.roses.ToString(), "Roses");

            float bx = playW - 108f;
            if (GUI.Button(new Rect(bx, 16f, 48f, 38f), _state.soundOn ? "SON" : "MUET", _btnDarkStyle))
            {
                _state.soundOn = !_state.soundOn;
                _audio.SoundEnabled = _state.soundOn;
                SaveGame(false);
            }
            if (GUI.Button(new Rect(bx + 54f, 16f, 54f, 38f), "RESET", _btnDarkStyle))
            {
                OpenConfirm("Reinitialiser ?", "Tout effacer (Sang, Meute, Ameliorations et Roses) pour repartir de zero ?", () =>
                {
                    ResetEverythingInternal();
                    ShowToast("Partie reinitialisee.");
                });
            }
        }

        private void DrawStatChip(float x, float y, float w, string iconKey, string val, string sub)
        {
            GUI.Box(new Rect(x, y, w, 44f), GUIContent.none, _cardStyle);
            GUI.DrawTexture(new Rect(x + 6f, y + 12f, 20f, 20f), PixelSpriteFactory.GetUiIcon(iconKey));
            GUI.Label(new Rect(x + 30f, y + 4f, w - 34f, 20f), val, _boldStyle);
            GUI.Label(new Rect(x + 30f, y + 22f, w - 34f, 18f), sub, _smallStyle);
        }

        private void DrawThreatBanners(float playW)
        {
            float y = 74f;
            bool hasHunter = false;
            int wolfCount = 0;
            foreach (EnemyEntity e in _enemies)
            {
                if (e.IsHunter) hasHunter = true;
                else wolfCount++;
            }

            if (hasHunter)
            {
                Rect br = new Rect(12f, y, Mathf.Min(430f, playW - 160f), 30f);
                GUI.Box(br, "ALERTE : CHASSEUR DE VAMPIRE EN TRAQUE ! FUYEZ !", _alertBannerStyle);
                GUI.DrawTexture(new Rect(br.x + 8f, br.y + 6f, 18f, 18f), PixelSpriteFactory.GetUiIcon("hunter"));
                y += 34f;
            }
            if (wolfCount > 0)
            {
                Rect br = new Rect(12f, y, Mathf.Min(430f, playW - 160f), 30f);
                GUI.Box(br, $"ALERTE : {wolfCount} LOUP(S)-GAROU(S) ATTIRE(S) PAR VOTRE SANG !", _alertBannerStyle);
                GUI.DrawTexture(new Rect(br.x + 8f, br.y + 6f, 18f, 18f), PixelSpriteFactory.GetUiIcon("wolf"));
                y += 34f;
            }
            if (!hasHunter && GetTotalGirls() >= hunterFollowerThreshold)
            {
                GUI.Box(new Rect(12f, y, 275f, 24f), "Risque de Chasseur : meute tres grande", _cardStyle);
                y += 28f;
            }
            if (wolfCount == 0 && _state.blood >= werewolfBloodThreshold)
            {
                GUI.Box(new Rect(12f, y, 275f, 24f), "Risque Loup-Garou : trop de sang en poche", _cardStyle);
            }
        }

        private void DrawBottomActionBar(float playW)
        {
            float barH = 62f;
            Rect r = new Rect(8f, Screen.height - barH - 8f, playW, barH);
            GUI.Box(r, GUIContent.none, _panelStyle);

            // Jauge Lune de Sang
            float barX = 16f;
            float barW = Mathf.Clamp(playW * 0.22f, 120f, 190f);
            string moonTxt = _state.moonTimeLeft > 0f
                ? "LUNE DE SANG x2 : " + _state.moonTimeLeft.ToString("0.0") + "s"
                : "Lune de Sang : " + Mathf.FloorToInt(_state.moonCharge) + "%";
            GUI.Label(new Rect(barX, r.y + 8f, barW, 18f), moonTxt, _boldStyle);

            float fillPct = _state.moonTimeLeft > 0f ? (_state.moonTimeLeft / 18f) : (_state.moonCharge / 100f);
            GUI.Box(new Rect(barX, r.y + 30f, barW, 16f), GUIContent.none, _cardStyle);
            GUI.color = GameCatalog.Hex("#f43f5e");
            GUI.DrawTexture(new Rect(barX + 2f, r.y + 32f, (barW - 4f) * Mathf.Clamp01(fillPct), 12f), _whiteTex);
            GUI.color = Color.white;

            // 3 Boutons d'action alignes a droite : DASH CHAUVE-SOURIS, CLIN D'OEIL, SACRIFIER
            float btnW = Mathf.Clamp((playW - barW - 44f) / 3f, 110f, 152f);
            float rightEdge = r.xMax - 10f;
            bool atAltar = IsPlayerAtAltar();
            int girls = GetTotalGirls();
            double sacVal = GetCrowdSacrificeValue(atAltar);

            // 1. Sacrifier [F]
            GUI.enabled = girls > 0;
            string sacLabel = atAltar && girls > 0
                ? "AUTEL (+" + GameCatalog.FormatNumber(sacVal) + ")"
                : "SACRIFIER [F] (+" + GameCatalog.FormatNumber(sacVal) + ")";
            if (GUI.Button(new Rect(rightEdge - btnW, r.y + 10f, btnW, 42f), sacLabel, _btnGoldStyle))
                SacrificeCrowd(atAltar);
            GUI.enabled = true;

            // 2. Clin d'oeil [ESPACE]
            GUI.enabled = _pulseCooldown <= 0f;
            string pulseLabel = _pulseCooldown > 0f ? "CHARME (" + _pulseCooldown.ToString("0.0") + "s)" : "CLIN D'OEIL [ESPACE]";
            if (GUI.Button(new Rect(rightEdge - btnW * 2f - 6f, r.y + 10f, btnW, 42f), pulseLabel, _btnRedStyle))
                TriggerCharmPulse();
            GUI.enabled = true;

            // 3. Dash Chauve-Souris [SHIFT / Clic Droit]
            GUI.enabled = _dashCooldown <= 0f && _dashTimer <= 0f;
            string dashLabel = _dashCooldown > 0f ? "DASH (" + _dashCooldown.ToString("0.0") + "s)" : "DASH BAT [SHIFT]";
            if (GUI.Button(new Rect(rightEdge - btnW * 3f - 12f, r.y + 10f, btnW, 42f), dashLabel, _btnPurpleStyle))
                TriggerBatDash();
            GUI.enabled = true;

            // Rappel discret si assez de place au centre
            float hintX = barX + barW + 10f;
            float hintW = (rightEdge - btnW * 3f - 18f) - hintX;
            if (hintW >= 120f)
            {
                GUI.Label(new Rect(hintX, r.y + 12f, hintW, 40f), "ZQSD / Fleches : Marcher\nSHIFT / Clic Droit : Dash", _smallStyle);
            }
        }

        private void DrawRightPanel(float x, float y, float w, float h)
        {
            GUI.Box(new Rect(x, y, w, h), GUIContent.none, _panelStyle);

            float tabW = (w - 24f) / 3f;
            if (GUI.Button(new Rect(x + 8f, y + 8f, tabW, 30f), "BOUTIQUE", _activeTab == 0 ? _btnRedStyle : _btnDarkStyle)) _activeTab = 0;
            if (GUI.Button(new Rect(x + 12f + tabW, y + 8f, tabW, 30f), "MEUTE", _activeTab == 1 ? _btnRedStyle : _btnDarkStyle)) _activeTab = 1;
            if (GUI.Button(new Rect(x + 16f + tabW * 2f, y + 8f, tabW, 30f), "SEMESTRE", _activeTab == 2 ? _btnGoldStyle : _btnDarkStyle)) _activeTab = 2;

            Rect contentRect = new Rect(x + 8f, y + 46f, w - 16f, h - 54f);
            GUILayout.BeginArea(contentRect);
            _shopScroll = GUILayout.BeginScrollView(_shopScroll);

            if (_activeTab == 0) DrawShopTab();
            else if (_activeTab == 1) DrawRosterTab();
            else DrawPrestigeTab();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawShopTab()
        {
            GUILayout.Label("Depense ton Sang pour evoluer et eviter les Loups-Garous !", _smallStyle);
            GUILayout.Space(6);

            foreach (RepeatableUpgrade rep in GameCatalog.RepeatableUpgrades)
            {
                int lvl = GetRepLevel(rep.Id);
                bool maxed = lvl >= rep.MaxLevel;
                double cost = GetRepCost(rep);
                bool canBuy = !maxed && _state.blood >= cost;

                GUILayout.BeginVertical(canBuy ? _cardBuyStyle : _cardStyle);
                GUILayout.BeginHorizontal();
                GUILayout.Box(PixelSpriteFactory.GetUiIcon(rep.IconKey), GUIStyle.none, GUILayout.Width(24), GUILayout.Height(24));
                GUILayout.BeginVertical();
                GUILayout.Label($"{rep.Name} (Niv.{lvl})", _boldStyle);
                GUILayout.Label(rep.Description, _smallStyle);
                GUILayout.EndVertical();
                GUI.enabled = canBuy;
                if (GUILayout.Button(maxed ? "MAX" : GameCatalog.FormatNumber(cost), canBuy ? _btnRedStyle : _btnDarkStyle, GUILayout.Width(72), GUILayout.Height(34)))
                    BuyRepeatable(rep);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(4);
            }

            GUILayout.Space(6);
            GUILayout.Label("RELOUKING VAMPIRE", _titleStyle);
            GUILayout.Space(4);

            foreach (SexyUpgrade u in GameCatalog.SexyUpgrades)
            {
                bool owned = _state.upgrades.Contains(u.Id);
                bool canBuy = !owned && _state.blood >= u.Cost;

                GUILayout.BeginVertical(canBuy ? _cardBuyStyle : _cardStyle);
                GUILayout.BeginHorizontal();
                GUILayout.Box(PixelSpriteFactory.GetUiIcon(u.IconKey), GUIStyle.none, GUILayout.Width(24), GUILayout.Height(24));
                GUILayout.BeginVertical();
                GUILayout.Label(u.Name + (u.ChangesSprite ? " [SPRITE]" : ""), _boldStyle);
                GUILayout.Label(u.Description, _smallStyle);
                GUILayout.EndVertical();
                GUI.enabled = canBuy;
                string lbl = owned ? "EQUIPE" : GameCatalog.FormatNumber(u.Cost);
                if (GUILayout.Button(lbl, canBuy ? _btnRedStyle : _btnDarkStyle, GUILayout.Width(72), GUILayout.Height(34)))
                    BuySexyUpgrade(u);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(4);
            }
        }

        private void DrawRosterTab()
        {
            GUILayout.Label("Tes fideles te suivent en bloc a 180 degres avec des coeurs dans les yeux. Attention : 6+ fideles attirent le Chasseur !", _smallStyle);
            GUILayout.Space(6);

            int allure = GetAllure();
            double mult = GetBloodMultiplier();

            foreach (GirlArchetype g in GameCatalog.GirlTypes)
            {
                bool unlocked = allure >= g.ReqAllure;
                int count = _state.girlCounts[g.Index];
                int sac = _state.sacrificedCounts[g.Index];
                double valEach = Math.Round(g.SacrificeBlood * mult);

                GUILayout.BeginVertical(unlocked ? _cardBuyStyle : _cardStyle);
                GUILayout.Label($"{g.Name}  |  En meute : {count}", _boldStyle);
                GUILayout.Label($"{g.Zone}  |  Sacrifiees : {sac}", _smallStyle);
                GUILayout.Label(unlocked ? $"Valeur : {GameCatalog.FormatNumber(valEach)} Sang chacune" : $"Requis : {g.ReqAllure} Charme", _smallStyle);
                GUILayout.EndVertical();
                GUILayout.Space(4);
            }
        }

        private void DrawPrestigeTab()
        {
            int claim = GetClaimableRoses();
            GUILayout.BeginVertical(_cardBuyStyle);
            GUILayout.Label("NOUVEAU SEMESTRE", _titleStyle);
            GUILayout.Label("Reinitialise ton Sang, ta Meute et la Boutique contre des Roses Immortelles (+35% Sang & +2 Charme initial par Rose).", _smallStyle);
            GUILayout.Space(6);
            GUILayout.Label($"Roses possedees : {_state.roses}", _boldStyle);
            GUILayout.Label($"A reclamer : +{claim}", _boldStyle);
            GUILayout.Space(6);

            GUI.enabled = claim > 0;
            if (GUILayout.Button("DIPLOMER & RECLAMER (+" + claim + ")", _btnGoldStyle, GUILayout.Height(38)))
            {
                OpenConfirm("Nouveau Semestre ?", $"Reclamer +{claim} Rose(s) Immortelle(s) et recommencer un semestre ?", PerformPrestige);
            }
            GUI.enabled = true;
            GUILayout.EndVertical();

            GUILayout.Space(10);
            GUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("STATISTIQUES", _boldStyle);
            GUILayout.Label("Sang total recolte : " + GameCatalog.FormatNumber(_state.lifetimeBlood), _smallStyle);
            GUILayout.Label("Filles charmees : " + _state.lifetimeGirls, _smallStyle);
            GUILayout.Label("Filles sacrifiees : " + _state.lifetimeSacrificed, _smallStyle);
            GUILayout.Label("Seuil Chasseur : " + hunterFollowerThreshold + "+ fideles", _smallStyle);
            GUILayout.Label("Seuil Loup-Garou : " + GameCatalog.FormatNumber(werewolfBloodThreshold) + "+ Sang", _smallStyle);
            GUILayout.EndVertical();
        }

        private void DrawMinimap(float x, float y, float w, float h)
        {
            GUI.Box(new Rect(x, y, w, h), GUIContent.none, _panelStyle);
            float sx = w / WorldConfig.WorldW;
            float sy = h / WorldConfig.WorldH;

            GUI.color = GameCatalog.Hex("#4c1d45");
            foreach (BuildingData b in GameCatalog.Buildings)
            {
                float bx = x + b.Bounds.x * sx;
                float by = y + h - (b.Bounds.yMax * sy);
                GUI.DrawTexture(new Rect(bx, by, b.Bounds.width * sx, b.Bounds.height * sy), _whiteTex);
            }

            GUI.color = GameCatalog.Hex("#e11d48");
            GUI.DrawTexture(new Rect(x + WorldConfig.AltarPos.x * sx - 3f, y + h - WorldConfig.AltarPos.y * sy - 3f, 6f, 6f), _whiteTex);

            int allure = GetAllure();
            foreach (GirlEntity g in _roamingGirls)
            {
                GUI.color = allure >= g.Type.ReqAllure ? GameCatalog.Hex("#fb7185") : GameCatalog.Hex("#94a3b8");
                GUI.DrawTexture(new Rect(x + g.Pos.x * sx - 1.5f, y + h - g.Pos.y * sy - 1.5f, 3f, 3f), _whiteTex);
            }

            foreach (EnemyEntity e in _enemies)
            {
                GUI.color = e.IsHunter ? GameCatalog.Hex("#38bdf8") : GameCatalog.Hex("#f97316");
                GUI.DrawTexture(new Rect(x + e.Pos.x * sx - 3f, y + h - e.Pos.y * sy - 3f, 6f, 6f), _whiteTex);
            }

            GUI.color = GameCatalog.Hex("#fde047");
            GUI.DrawTexture(new Rect(x + _playerPos.x * sx - 2.5f, y + h - _playerPos.y * sy - 2.5f, 5f, 5f), _whiteTex);
            GUI.color = Color.white;
        }

        private void OpenConfirm(string title, string body, Action onConfirm)
        {
            _modalTitle = title;
            _modalBody = body;
            _modalAction = onConfirm;
            _showConfirmModal = true;
        }

        private void DrawConfirmModal()
        {
            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _whiteTex);
            GUI.color = Color.white;

            Rect r = new Rect(Screen.width * 0.5f - 190f, Screen.height * 0.5f - 85f, 380f, 170f);
            GUI.Box(r, GUIContent.none, _panelStyle);
            GUI.Label(new Rect(r.x + 16f, r.y + 14f, r.width - 32f, 24f), _modalTitle, _titleStyle);
            GUI.Label(new Rect(r.x + 16f, r.y + 46f, r.width - 32f, 64f), _modalBody, _smallStyle);

            if (GUI.Button(new Rect(r.x + 16f, r.yMax - 46f, 150f, 34f), "ANNULER", _btnDarkStyle))
                _showConfirmModal = false;
            if (GUI.Button(new Rect(r.xMax - 166f, r.yMax - 46f, 150f, 34f), "CONFIRMER", _btnRedStyle))
            {
                _showConfirmModal = false;
                _modalAction?.Invoke();
            }
        }

        private void DrawDeathOverlay()
        {
            GUI.color = new Color(0.08f, 0f, 0.03f, 0.84f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _whiteTex);
            GUI.color = Color.white;

            Rect r = new Rect(Screen.width * 0.5f - 230f, Screen.height * 0.5f - 125f, 460f, 250f);
            GUI.Box(r, GUIContent.none, _panelStyle);

            GUIStyle deadTitle = new GUIStyle(_titleStyle) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
            deadTitle.normal.textColor = GameCatalog.Hex("#f43f5e");
            GUI.Label(new Rect(r.x + 16f, r.y + 18f, r.width - 32f, 34f), _deathTitle, deadTitle);

            GUIStyle reasonStyle = new GUIStyle(_boldStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 13 };
            GUI.Label(new Rect(r.x + 22f, r.y + 62f, r.width - 44f, 64f), _deathReason, reasonStyle);

            string stats = $"Sang recolte : {GameCatalog.FormatNumber(_state.lifetimeBlood)}   |   Fideles sacrifiees : {_state.lifetimeSacrificed}";
            GUI.Label(new Rect(r.x + 16f, r.y + 134f, r.width - 32f, 24f), stats, _centerBoldStyle);

            if (GUI.Button(new Rect(r.x + 80f, r.yMax - 64f, r.width - 160f, 44f), "RECOMMENCER LA PARTIE [R]", _btnRedStyle))
                RestartAfterDeath();
        }
    }
}
