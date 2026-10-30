using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class BuildObbyScene
{
    [MenuItem("Obby/Build Neon Obby Scene")]
    public static void Build()
    {
        AssetDatabase.DeleteAsset("Assets/Materials");
        AssetDatabase.DeleteAsset("Assets/Animation/ObbyPlayer.controller");
        Directory.CreateDirectory("Assets/Materials");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cyan = Material("StartCyan", new Color(.12f, .75f, .95f));
        var violet = Material("PlatformViolet", new Color(.42f, .29f, .85f));
        var gold = Material("AccentGold", new Color(1f, .64f, .12f));
        var lava = Material("LavaRed", new Color(1f, .12f, .04f));
        var finish = Material("FinishGreen", new Color(.15f, .96f, .44f));
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.3f, .37f, .5f);

        var light = new GameObject("Directional Light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.4f;
        light.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
        var course = new GameObject("OBBY — Neon Sky Course").transform;
        Block("Start Platform", Vector3.zero, new Vector3(9f, 1f, 9f), cyan, course);
        Block("Start Stripe", new Vector3(0f, .56f, 3.4f), new Vector3(8.4f, .12f, .45f), gold, course);

        Vector3[] points = { new(0,.8f,7), new(-2.4f,1.4f,11), new(2,2.1f,15.3f), new(-1.7f,2.8f,19.8f), new(2.5f,3.55f,24.7f), new(0,4.2f,30), new(-3.1f,4.8f,34.7f), new(1.4f,5.35f,39.4f) };
        for (var i = 0; i < points.Length; i++)
        {
            var width = i % 3 == 0 ? 2.35f : 3.35f;
            Block($"Jump Platform {i + 1}", points[i], new Vector3(width, .65f, width), i % 2 == 0 ? violet : gold, course);
            if (i == 2 || i == 5)
            {
                var checkpoint = Block($"Checkpoint {i + 1}", points[i] + Vector3.up * 1.2f, new Vector3(.8f, 1.6f, .8f), finish, course);
                checkpoint.GetComponent<BoxCollider>().isTrigger = true;
                var checkpointSpawn = new GameObject($"Checkpoint {i + 1} Respawn");
                checkpointSpawn.transform.position = points[i] + new Vector3(0f, 2.3f, 0f);
                SetRef(checkpoint.AddComponent<ObbyCheckpoint>(), "respawnPoint", checkpointSpawn.transform);
            }
        }
        Block("Finish Platform", new Vector3(0f, 5.8f, 46f), new Vector3(9f, .8f, 9f), finish, course);
        Block("Finish Arch Left", new Vector3(-3.5f, 8.2f, 47.9f), new Vector3(.6f, 4f, .6f), gold, course);
        Block("Finish Arch Right", new Vector3(3.5f, 8.2f, 47.9f), new Vector3(.6f, 4f, .6f), gold, course);
        Block("Finish Arch Top", new Vector3(0f, 10.1f, 47.9f), new Vector3(7.6f, .6f, .6f), gold, course);

        var kill = Block("Respawn Zone", new Vector3(0f, -5f, 25f), new Vector3(60f, .6f, 90f), lava, course);
        kill.GetComponent<BoxCollider>().isTrigger = true;
        var respawn = new GameObject("Spawn Point");
        respawn.transform.position = new Vector3(0f, 1.6f, -2f);
        kill.AddComponent<ObbyHazard>();

        var controller = MakeController();
        var player = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Animation/Idle.fbx"));
        player.name = "Player";
        player.transform.position = new Vector3(0f, .55f, -2f);
        var visual = player.GetComponentInChildren<Renderer>();
        if (visual != null && visual.bounds.size.y > .01f) player.transform.localScale *= 1.8f / visual.bounds.size.y;
        var animator = player.GetComponent<Animator>();
        if (animator == null) animator = player.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        var character = player.AddComponent<CharacterController>();
        character.height = 1.8f; character.radius = .35f; character.center = new Vector3(0f, .9f, 0f);
        var movement = player.AddComponent<PlayerObbyController>();
        var playerRespawn = player.AddComponent<PlayerRespawn>();
        SetRef(playerRespawn, "startPoint", respawn.transform);

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.045f, .09f, .18f); camera.fieldOfView = 65f;
        cameraObject.transform.position = player.transform.position + new Vector3(0f, 4.5f, -7.5f);
        var follow = cameraObject.AddComponent<ThirdPersonCamera>();
        SetRef(follow, "target", player.transform);
        SetRef(movement, "cameraTransform", cameraObject.transform);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/ObbyScene.unity");
        AssetDatabase.SaveAssets();
        Debug.Log("Neon Obby scene created.");
    }

    private static Material Material(string name, Color color)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name, color = color };
        AssetDatabase.CreateAsset(material, $"Assets/Materials/{name}.mat");
        return material;
    }
    private static GameObject Block(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name; block.transform.SetParent(parent); block.transform.position = position; block.transform.localScale = scale;
        block.GetComponent<Renderer>().sharedMaterial = material;
        return block;
    }
    private static AnimationClip Clip(string path) => AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(x => !x.name.StartsWith("__preview__"));
    private static void SetRef(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(field).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
    private static AnimatorController MakeController()
    {
        var controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/Animation/ObbyPlayer.controller");
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        var sm = controller.layers[0].stateMachine;
        var idle = sm.AddState("Idle"); idle.motion = Clip("Assets/Animation/Idle.fbx");
        var walk = sm.AddState("Walk"); walk.motion = Clip("Assets/Animation/Walk.fbx");
        var run = sm.AddState("Run"); run.motion = Clip("Assets/Animation/run.fbx");
        var jump = sm.AddState("Jump"); jump.motion = Clip("Assets/Animation/Jump.fbx");
        sm.defaultState = idle;
        Add(idle, walk, AnimatorConditionMode.Greater, .05f, "Speed");
        Add(walk, idle, AnimatorConditionMode.Less, .05f, "Speed");
        Add(walk, run, AnimatorConditionMode.Greater, .75f, "Speed");
        Add(run, walk, AnimatorConditionMode.Less, .75f, "Speed");
        foreach (var state in new[] { idle, walk, run }) Add(state, jump, AnimatorConditionMode.If, 0f, "Jump", .05f);
        Add(jump, idle, AnimatorConditionMode.If, 0f, "Grounded", .1f);
        return controller;
    }
    private static void Add(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float threshold, string parameter, float duration = .12f)
    {
        var transition = from.AddTransition(to); transition.hasExitTime = false; transition.duration = duration; transition.AddCondition(mode, threshold, parameter);
    }
}
