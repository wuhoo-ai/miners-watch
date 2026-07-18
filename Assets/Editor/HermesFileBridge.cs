// HermesFileBridge — file-polling editor bridge (no MCP dependency).
// Cloud Hermes writes .hermes-bridge/request.json; this script executes and
// writes response_<id>.json. Delete this file to remove the bridge entirely.
using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hermes
{
    [InitializeOnLoad]
    public static class HermesFileBridge
    {
        const double PollInterval = 1.0;
        static double s_NextPoll;

        static string BridgeDir
        {
            get
            {
                var projectRoot = Directory.GetParent(Application.dataPath).FullName;
                return Path.Combine(projectRoot, ".hermes-bridge");
            }
        }

        static HermesFileBridge()
        {
            try
            {
                Directory.CreateDirectory(BridgeDir);
                File.WriteAllText(Path.Combine(BridgeDir, "bridge-alive.txt"),
                    DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture) + " unity=" + Application.unityVersion);
            }
            catch { /* non-fatal */ }
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate()
        {
            if (EditorApplication.timeSinceStartup < s_NextPoll) return;
            s_NextPoll = EditorApplication.timeSinceStartup + PollInterval;

            string reqPath = Path.Combine(BridgeDir, "request.json");
            if (!File.Exists(reqPath)) return;

            string id = "unknown";
            try
            {
                string json = File.ReadAllText(reqPath);
                File.Delete(reqPath);
                var req = JsonUtility.FromJson<BridgeRequest>(json);
                id = string.IsNullOrEmpty(req.id) ? DateTime.UtcNow.Ticks.ToString() : req.id;
                string result = Execute(req);
                WriteResponse(id, true, result);
            }
            catch (Exception e)
            {
                WriteResponse(id, false, e.GetType().Name + ": " + e.Message);
            }
        }

        static string Execute(BridgeRequest req)
        {
            int w = req.width > 0 ? req.width : 1920;
            int h = req.height > 0 ? req.height : 1080;
            switch (req.action)
            {
                case "ping":
                    return "pong unity=" + Application.unityVersion;
                case "capture_sceneview":
                    return CaptureSceneView(w, h);
                case "capture_camera":
                    return CaptureCamera(req.target, w, h);
                default:
                    throw new Exception("unknown action: " + req.action);
            }
        }

        static string CaptureSceneView(int w, int h)
        {
            var sv = SceneView.lastActiveSceneView;
            var cam = sv != null ? sv.camera : null;
            if (cam == null) return CaptureCamera(null, w, h); // fallback: main camera
            return RenderToFile(cam, w, h, "sceneview");
        }

        static string CaptureCamera(string name, int w, int h)
        {
            Camera cam = null;
            if (!string.IsNullOrEmpty(name))
            {
                var go = GameObject.Find(name);
                if (go != null) cam = go.GetComponent<Camera>();
            }
            if (cam == null) cam = Camera.main;
            if (cam == null && Camera.allCamerasCount > 0) cam = Camera.allCameras[0];
            if (cam == null) throw new Exception("no camera found in scene");
            return RenderToFile(cam, w, h, "camera");
        }

        static string RenderToFile(Camera cam, int w, int h, string tag)
        {
            var rt = new RenderTexture(w, h, 24);
            var prevTarget = cam.targetTexture;
            var prevActive = RenderTexture.active;
            try
            {
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply();
                var png = tex.EncodeToPNG();
                UnityEngine.Object.DestroyImmediate(tex);
                string file = Path.Combine(BridgeDir,
                    "capture_" + tag + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
                File.WriteAllBytes(file, png);
                return file;
            }
            finally
            {
                cam.targetTexture = prevTarget;
                RenderTexture.active = prevActive;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        static void WriteResponse(string id, bool ok, string result)
        {
            var resp = new BridgeResponse { id = id, ok = ok, result = result };
            File.WriteAllText(Path.Combine(BridgeDir, "response_" + id + ".json"),
                JsonUtility.ToJson(resp));
        }

        [Serializable]
        class BridgeRequest
        {
            public string id;
            public string action;
            public string target;
            public int width;
            public int height;
        }

        [Serializable]
        class BridgeResponse
        {
            public string id;
            public bool ok;
            public string result;
        }

        [MenuItem("Hermes/Capture Scene View")]
        static void MenuCapture()
        {
            Debug.Log("HermesFileBridge: " + CaptureSceneView(1920, 1080));
        }
    }
}
