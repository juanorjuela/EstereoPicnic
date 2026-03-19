using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class WorldTiler : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player or camera rig whose position drives tile spawning.")]
    public Transform player;

    [Header("Tiles")]
    [Tooltip("Square island prefabs, each scaled to 5x5 units with pivot at center.")]
    public GameObject[] islandPrefabs;

    [Tooltip("Size of one tile edge in world units.")]
    public float tileSize = 5f;

    [Tooltip("Number of tiles to keep around the player in each direction (3 = 7x7 area).")]
    public int radiusInTiles = 3;

    // Current tiles keyed by grid coordinate. Guarantees at most one island per cell.
    private readonly Dictionary<Vector2Int, GameObject> _tiles = new Dictionary<Vector2Int, GameObject>();
    private readonly Dictionary<Vector2Int, int> _prefabIndexByCoord = new Dictionary<Vector2Int, int>();
    private Vector2Int _currentCenterCell;
    private bool _initialized;

    [Header("Safety / Debug")]
    [Tooltip("Hard safety cap to avoid accidental 10k+ instantiations causing editor hangs.")]
    public int maxSafeRadiusInTiles = 12;

    [Tooltip("If radius would exceed this cap, it will be clamped and a warning will be logged.")]
    public bool clampUnsafeSettings = true;

    [Tooltip("Press this key in Play Mode to print tiler stats to the Console.")]
    public KeyCode printStatsKey = KeyCode.F9;

    private int _regenCalls;
    private long _totalTilesSpawned;
    private long _totalTilesDestroyed;
    private readonly List<Vector2Int> _coordsToDestroy = new List<Vector2Int>(256);

    private void Awake()
    {
        // Reset counters whenever this component comes alive.
        _regenCalls = 0;
        _totalTilesSpawned = 0;
        _totalTilesDestroyed = 0;
        _initialized = false;
    }

    private void Start()
    {
        ApplySafetySettings();
        if (player == null)
        {
            Debug.LogError("[WorldTiler] Player reference is missing.");
            enabled = false;
            return;
        }

        _currentCenterCell = WorldToGrid(player.position);
        RegenerateTilesAroundCenter(_currentCenterCell);
        _initialized = true;
        Debug.Log($"[WorldTiler] Started. Active tiles: {_tiles.Count} (radiusInTiles={radiusInTiles}, tileSize={tileSize}).");
    }

    private void Update()
    {
        if (!_initialized || player == null)
        {
            return;
        }

        if (ShouldPrintStats())
        {
            PrintStats();
        }

        var newCenter = WorldToGrid(player.position);
        if (newCenter != _currentCenterCell)
        {
            _currentCenterCell = newCenter;
            RegenerateTilesAroundCenter(_currentCenterCell);
        }
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        // Snap to the nearest grid cell center.
        // Using FloorToInt(x / tileSize + 0.5f) tends to be more stable than RoundToInt
        // when the player is hovering close to a boundary.
        int gx = Mathf.FloorToInt(worldPos.x / tileSize + 0.5f);
        int gz = Mathf.FloorToInt(worldPos.z / tileSize + 0.5f);
        return new Vector2Int(gx, gz);
    }

    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        float x = gridPos.x * tileSize;
        float z = gridPos.y * tileSize;
        return new Vector3(x, 0f, z);
    }

    private void RegenerateTilesAroundCenter(Vector2Int center)
    {
        _regenCalls++;

        // Destroy tiles that are now outside the radius.
        _coordsToDestroy.Clear();
        _coordsToDestroy.AddRange(_tiles.Keys);
        foreach (var coord in _coordsToDestroy)
        {
            int dx = Mathf.Abs(coord.x - center.x);
            int dz = Mathf.Abs(coord.y - center.y);
            int chebyshev = Mathf.Max(dx, dz);

            if (chebyshev > radiusInTiles)
            {
                Destroy(_tiles[coord]);
                _tiles.Remove(coord);
                _prefabIndexByCoord.Remove(coord);
                _totalTilesDestroyed++;
            }
        }

        // Spawn tiles that should exist but are missing.
        for (int x = center.x - radiusInTiles; x <= center.x + radiusInTiles; x++)
        {
            for (int z = center.y - radiusInTiles; z <= center.y + radiusInTiles; z++)
            {
                var coord = new Vector2Int(x, z);
                if (_tiles.ContainsKey(coord))
                {
                    continue; // Already have a tile here; never spawn two islands in the same position.
                }

                SpawnTile(coord);
            }
        }
    }

    private void SpawnTile(Vector2Int coord)
    {
        if (islandPrefabs == null || islandPrefabs.Length == 0)
        {
            Debug.LogWarning("[WorldTiler] No island prefabs assigned.");
            return;
        }

        int prefabIndex = ChoosePrefabIndex(coord);
        var prefab = islandPrefabs[prefabIndex];
        if (prefab == null)
        {
            Debug.LogWarning("[WorldTiler] Prefab at index " + prefabIndex + " is null.");
            return;
        }

        int rotationIndex = Random.Range(0, 4); // 0, 1, 2, 3 -> 0, 90, 180, 270 degrees
        Quaternion rotation = Quaternion.Euler(0f, rotationIndex * 90f, 0f);
        Vector3 position = GridToWorld(coord);

        GameObject instance = Instantiate(prefab, position, rotation, transform);
        _tiles[coord] = instance;
        _prefabIndexByCoord[coord] = prefabIndex;
        _totalTilesSpawned++;
    }

    // Simple rules-based prefab selection:
    // - Prefer an index that isn't used by orthogonal neighbors, if possible.
    // - Otherwise fall back to any random index.
    private int ChoosePrefabIndex(Vector2Int coord)
    {
        int prefabCount = islandPrefabs.Length;
        if (prefabCount <= 1)
        {
            return 0;
        }

        // Collect forbidden indices from orthogonal neighbors.
        // Stored in _prefabIndexByCoord so we don't need any extra component lookups.
        int nUp = TryGetNeighborPrefabIndex(coord + Vector2Int.up);
        int nDown = TryGetNeighborPrefabIndex(coord + Vector2Int.down);
        int nLeft = TryGetNeighborPrefabIndex(coord + Vector2Int.left);
        int nRight = TryGetNeighborPrefabIndex(coord + Vector2Int.right);

        // Attempt a few random picks that are not forbidden; avoids per-tile allocations.
        for (int attempt = 0; attempt < 10; attempt++)
        {
            int candidate = Random.Range(0, prefabCount);
            if (candidate != nUp && candidate != nDown && candidate != nLeft && candidate != nRight)
            {
                return candidate;
            }
        }

        // Fallback: all candidates are effectively forbidden, so just pick anything.
        return Random.Range(0, prefabCount);
    }

    private int TryGetNeighborPrefabIndex(Vector2Int neighborCoord)
    {
        if (_prefabIndexByCoord.TryGetValue(neighborCoord, out int idx))
        {
            return idx;
        }

        return -1;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null)
        {
            return;
        }

        Vector2Int center = Application.isPlaying ? _currentCenterCell : WorldToGrid(player.position);

        Gizmos.color = Color.green;

        for (int x = center.x - radiusInTiles; x <= center.x + radiusInTiles; x++)
        {
            for (int z = center.y - radiusInTiles; z <= center.y + radiusInTiles; z++)
            {
                Vector2Int coord = new Vector2Int(x, z);
                Vector3 centerPos = GridToWorld(coord);
                Vector3 size = new Vector3(tileSize, 0.01f, tileSize);
                Gizmos.DrawWireCube(centerPos, size);
            }
        }
    }

    private void ApplySafetySettings()
    {
        if (tileSize <= 0.0001f)
        {
            Debug.LogError("[WorldTiler] tileSize must be > 0.");
            enabled = false;
            return;
        }

        if (!clampUnsafeSettings)
        {
            return;
        }

        if (radiusInTiles < 0)
        {
            radiusInTiles = 0;
        }

        if (radiusInTiles > maxSafeRadiusInTiles)
        {
            Debug.LogWarning($"[WorldTiler] radiusInTiles={radiusInTiles} is too large. Clamping to maxSafeRadiusInTiles={maxSafeRadiusInTiles}.");
            radiusInTiles = maxSafeRadiusInTiles;
        }
    }

    private bool ShouldPrintStats()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame)
            return true;
        return false;
#else
        return Input.GetKeyDown(printStatsKey);
#endif
    }

    public void PrintStats()
    {
        Debug.Log(
            $"[WorldTiler Stats] regenCalls={_regenCalls}, activeTiles={_tiles.Count}, " +
            $"totalSpawned={_totalTilesSpawned}, totalDestroyed={_totalTilesDestroyed}"
        );
    }
}

// Attached to spawned instances so WorldTiler can know which prefab index was used.
// NOTE: Current implementation no longer requires this component; it is kept only for backwards compatibility.
public class WorldTilerInstance : MonoBehaviour
{
    [HideInInspector]
    public int prefabIndex;
}

