using System.IO;
using TheLastCrew.Audio;
using TheLastCrew.Maze;
using TheLastCrew.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastCrew.EditorTools
{
    /// <summary>
    /// Builds the whole navigation prototype from a menu item.
    ///
    /// This exists because wiring six components by hand is the step most
    /// likely to go wrong, and a mistyped reference produces "the audio feels
    /// weird" instead of an error. One click, correct wiring, every time.
    /// </summary>
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/AudioMaze/NavTest.unity";
        private const string AudioFolder = "Assets/Audio/Placeholder";

        [MenuItem("TheLastCrew/Build Playable Test Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "TheLastCrew",
                    "Stop play mode before building the scene.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            MazeBuilder maze = CreateMaze();
            GameObject player = CreatePlayer(maze);
            CreateCamera(player);
            CreateLight();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes/AudioMaze");
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            Selection.activeGameObject = player;
            SceneView.lastActiveSceneView?.FrameSelected();

            Debug.Log($"[TheLastCrew] Built {ScenePath}. Press Play, then put headphones on.");
        }

        [MenuItem("TheLastCrew/Open Playable Test Scene")]
        public static void OpenTestScene()
        {
            if (EditorApplication.isPlaying)
                return;

            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog(
                    "TheLastCrew",
                    "The test scene does not exist yet. Use TheLastCrew > Build Playable Test Scene.",
                    "OK");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }

        // ------------------------------------------------------------------

        private static MazeBuilder CreateMaze()
        {
            GameObject root = new GameObject("Maze");
            MazeBuilder builder = root.AddComponent<MazeBuilder>();

            builder.width = 19;
            builder.height = 19;
            builder.seed = 1234;
            builder.cellSize = 3f;
            builder.wallHeight = 3f;
            builder.humClip = LoadClip("landmark_hum_loop");

            builder.Build();
            return builder;
        }

        private static GameObject CreatePlayer(MazeBuilder maze)
        {
            GameObject player = new GameObject("Player");
            player.transform.position = maze.StartPosition;

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.slopeLimit = 45f;
            controller.stepOffset = 0.3f;
            controller.skinWidth = 0.08f;

            TouchInputProvider touch = player.AddComponent<TouchInputProvider>();

            PlayerMotor motor = player.AddComponent<PlayerMotor>();
            motor.inputSource = touch;
            motor.walkSpeed = 1.8f;
            motor.quietSpeed = 0.6f;
            motor.turnSpeed = 90f;

            FootstepAudio footsteps = player.AddComponent<FootstepAudio>();
            footsteps.motor = motor;
            footsteps.clips = new[]
            {
                LoadClip("footstep_01"),
                LoadClip("footstep_02"),
                LoadClip("footstep_03")
            };

            WallProximityAudio walls = player.AddComponent<WallProximityAudio>();
            walls.rustleClip = LoadClip("wall_rustle_loop");
            walls.wallMask = ~0;
            walls.maxRange = 5f;
            walls.probeCount = 8;

            return player;
        }

        private static void CreateCamera(GameObject player)
        {
            GameObject cameraObject = new GameObject("PlayerCamera");
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.03f, 0.04f);
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 200f;
            cameraObject.tag = "MainCamera";

            cameraObject.AddComponent<AudioListener>();

            SpatialAudioGuard guard = player.AddComponent<SpatialAudioGuard>();
            guard.expectedListener = cameraObject.transform;
        }

        private static void CreateLight()
        {
            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.7f;
        }

        private static AudioClip LoadClip(string name)
        {
            string path = $"{AudioFolder}/{name}.wav";
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

            if (clip == null)
                Debug.LogWarning($"[TheLastCrew] Missing audio clip: {path}");

            return clip;
        }
    }
}
