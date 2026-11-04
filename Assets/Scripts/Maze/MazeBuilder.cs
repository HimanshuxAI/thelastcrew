using UnityEngine;

namespace TheLastCrew.Maze
{
    /// <summary>
    /// Turns a <see cref="MazeGrid"/> into geometry you can walk into.
    ///
    /// Deliberately dumb: one cube per solid cell, one floor slab, two
    /// landmarks. No pooling, no chunking, no optimisation - because none of
    /// that matters until we know the sound works.
    /// </summary>
    public class MazeBuilder : MonoBehaviour
    {
        [Header("Layout")]
        [Tooltip("Cells across. Forced odd; higher is a longer walk.")]
        public int width = 19;

        [Tooltip("Cells deep. Forced odd.")]
        public int height = 19;

        [Tooltip("Same seed, same maze. Change it for a different level.")]
        public int seed = 1234;

        [Header("Dimensions")]
        public float cellSize = 3f;
        public float wallHeight = 3f;

        [Header("Look")]
        public Material wallMaterial;
        public Material floorMaterial;

        [Header("Landmarks")]
        public AudioClip humClip;
        public float landmarkVolume = 0.55f;
        public float landmarkMaxDistance = 18f;

        public MazeGrid Grid { get; private set; }

        private void Awake()
        {
            EnsureGrid();
        }

        private void Start()
        {
            // Geometry saved into the scene is left alone; only an empty
            // builder builds itself at play time.
            if (transform.childCount == 0)
                BuildGeometry();
        }

        /// <summary>Rebuilds the maze. Safe to call from the editor.</summary>
        [ContextMenu("Build Maze")]
        public void Build()
        {
            EnsureGrid();
            Clear();
            BuildGeometry();
        }

        private void EnsureGrid()
        {
            if (Grid == null)
                Grid = MazeGrid.Generate(width, height, seed);
        }

        private void BuildGeometry()
        {
            BuildFloor();
            BuildWalls();
            BuildLandmarks();
        }

        public Vector3 CellToWorld(int x, int y)
        {
            float originX = -width * cellSize * 0.5f;
            float originZ = -height * cellSize * 0.5f;

            return transform.position
                   + new Vector3(originX + x * cellSize, 0f, originZ + y * cellSize);
        }

        public Vector3 StartPosition => CellToWorld(1, 1) + Vector3.up * 0.2f;

        public Vector3 LandmarkPositionA => CellToWorld(width - 2, 1);
        public Vector3 LandmarkPositionB => CellToWorld(1, height - 2);

        // ------------------------------------------------------------------

        private void BuildFloor()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(transform, false);
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(width * cellSize, 1f, height * cellSize);

            if (floorMaterial != null)
                floor.GetComponent<MeshRenderer>().sharedMaterial = floorMaterial;
        }

        private void BuildWalls()
        {
            GameObject root = new GameObject("Walls");
            root.transform.SetParent(transform, false);

            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Height; y++)
                {
                    if (!Grid.IsSolid(x, y))
                        continue;

                    GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.name = $"Wall_{x}_{y}";
                    wall.transform.SetParent(root.transform, false);
                    wall.transform.position = CellToWorld(x, y) + Vector3.up * (wallHeight * 0.5f);
                    wall.transform.localScale = new Vector3(cellSize, wallHeight, cellSize);

                    if (wallMaterial != null)
                        wall.GetComponent<MeshRenderer>().sharedMaterial = wallMaterial;
                }
            }
        }

        private void BuildLandmarks()
        {
            if (humClip == null)
                return;

            // Two landmarks with the same recording but a different pitch.
            // The player learns them as two identities, and the pitch
            // difference is enough to tell them apart with the eyes shut.
            CreateLandmark("Landmark_A", LandmarkPositionA, 1f);
            CreateLandmark("Landmark_B", LandmarkPositionB, 1.45f);
        }

        private void CreateLandmark(string name, Vector3 position, float pitch)
        {
            GameObject landmark = new GameObject(name);
            landmark.transform.SetParent(transform, false);
            landmark.transform.position = position + Vector3.up * 1.5f;

            AudioSource source = landmark.AddComponent<AudioSource>();
            source.clip = humClip;
            source.loop = true;
            source.playOnAwake = true;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 2f;
            source.maxDistance = landmarkMaxDistance;
            source.dopplerLevel = 0f;
            source.volume = landmarkVolume;
            source.pitch = pitch;
        }

        private void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;

                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }
    }
}
