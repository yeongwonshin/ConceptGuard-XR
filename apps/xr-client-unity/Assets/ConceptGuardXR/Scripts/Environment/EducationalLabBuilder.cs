using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace ConceptGuardXR
{
    public sealed class LabRuntimeBindings
    {
        public LabPalette Palette { get; set; }
        public CircuitGraphBuilder GraphBuilder { get; set; }
        public ConceptGuardXRApiClient ApiClient { get; set; }
        public SessionEventLogger EventLogger { get; set; }
        public XRWireAuthoringTool WireTool { get; set; }
        public CurrentFlowVisualizer FlowVisualizer { get; set; }
        public MisconceptionCoachPanel CoachPanel { get; set; }
        public NodeOverlayPresenter OverlayPresenter { get; set; }
        public XRWorldButton AnalyzeButton { get; set; }
    }

    public static class EducationalLabBuilder
    {
        private const float BenchSurfaceY = 1.13f;

        public static LabRuntimeBindings Build(Transform applicationRoot)
        {
            var palette = new LabPalette();
            ConfigureRendering(palette);

            var environmentRoot = new GameObject("Educational Environment").transform;
            environmentRoot.SetParent(applicationRoot, false);
            CreateArchitecture(environmentRoot, palette);
            CreateLighting(environmentRoot, palette);

            var runtimeRoot = new GameObject("Runtime Services").transform;
            runtimeRoot.SetParent(applicationRoot, false);
            var graphBuilder = runtimeRoot.gameObject.AddComponent<CircuitGraphBuilder>();
            var apiClient = runtimeRoot.gameObject.AddComponent<ConceptGuardXRApiClient>();
            var eventLogger = runtimeRoot.gameObject.AddComponent<SessionEventLogger>();
            var wireTool = new GameObject("Wire Authoring").AddComponent<XRWireAuthoringTool>();
            wireTool.transform.SetParent(runtimeRoot, false);
            var flowVisualizer = new GameObject("Current Flow Visualization").AddComponent<CurrentFlowVisualizer>();
            flowVisualizer.transform.SetParent(runtimeRoot, false);
            flowVisualizer.Configure(palette);
            var overlayPresenter = new GameObject("Node Overlays").AddComponent<NodeOverlayPresenter>();
            overlayPresenter.transform.SetParent(runtimeRoot, false);
            overlayPresenter.Configure(palette);

            var coachPanel = CreateCoachPanel(environmentRoot, palette);
            wireTool.Configure(graphBuilder, eventLogger, palette);

            var componentRoot = new GameObject("Circuit Components").transform;
            componentRoot.SetParent(environmentRoot, false);
            CreateCircuitComponents(componentRoot, palette, graphBuilder, eventLogger);

            var rig = CreateRig(applicationRoot, palette, wireTool);
            CreateInstructionPanel(environmentRoot, palette);
            CreateMissionPanel(environmentRoot, palette);

            XRWorldButton analyzeButton = null;
            CreateControlDeck(
                environmentRoot,
                palette,
                out analyzeButton,
                apiClient.AnalyzeCurrentCircuit,
                wireTool.UndoLastWire,
                () =>
                {
                    wireTool.ClearAllWires();
                    flowVisualizer.Clear();
                    overlayPresenter.Clear();
                },
                () => ResetLearningSpace(wireTool, flowVisualizer, overlayPresenter, eventLogger)
            );

            CreateControllerHint(rig.RightHand, palette);

            return new LabRuntimeBindings
            {
                Palette = palette,
                GraphBuilder = graphBuilder,
                ApiClient = apiClient,
                EventLogger = eventLogger,
                WireTool = wireTool,
                FlowVisualizer = flowVisualizer,
                CoachPanel = coachPanel,
                OverlayPresenter = overlayPresenter,
                AnalyzeButton = analyzeButton
            };
        }

        private static void ConfigureRendering(LabPalette palette)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.28f, 0.34f, 0.44f);
            RenderSettings.ambientEquatorColor = new Color(0.12f, 0.16f, 0.22f);
            RenderSettings.ambientGroundColor = palette.Background;
            RenderSettings.fog = true;
            RenderSettings.fogColor = palette.Background;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.008f;
            QualitySettings.shadowDistance = 25f;
        }

        private static void CreateArchitecture(Transform parent, LabPalette palette)
        {
            CreatePrimitive(
                parent,
                "Floor",
                PrimitiveType.Cube,
                new Vector3(0f, -0.08f, 0.6f),
                new Vector3(8.6f, 0.16f, 7.2f),
                palette.Get("Floor", palette.Navy, 0.1f, 0.5f)
            );

            CreatePrimitive(
                parent,
                "Back Wall",
                PrimitiveType.Cube,
                new Vector3(0f, 2.2f, 2.85f),
                new Vector3(8.6f, 4.5f, 0.14f),
                palette.Get("Wall", new Color(0.10f, 0.15f, 0.23f), 0f, 0.3f)
            );

            CreatePrimitive(
                parent,
                "Left Wall",
                PrimitiveType.Cube,
                new Vector3(-4.25f, 2.2f, 0.3f),
                new Vector3(0.14f, 4.5f, 5.2f),
                palette.Get("Side Wall", new Color(0.065f, 0.10f, 0.16f), 0f, 0.28f)
            );

            CreatePrimitive(
                parent,
                "Right Wall",
                PrimitiveType.Cube,
                new Vector3(4.25f, 2.2f, 0.3f),
                new Vector3(0.14f, 4.5f, 5.2f),
                palette.Get("Side Wall", new Color(0.065f, 0.10f, 0.16f), 0f, 0.28f)
            );

            var platform = CreatePrimitive(
                parent,
                "Learning Platform",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.02f, -0.15f),
                new Vector3(4.7f, 0.06f, 4.7f),
                palette.Get("Platform", new Color(0.09f, 0.14f, 0.22f), 0.2f, 0.65f)
            );
            platform.transform.localScale = new Vector3(2.4f, 0.04f, 2.4f);

            var platformRing = CreatePrimitive(
                parent,
                "Platform Accent",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.066f, -0.15f),
                Vector3.one,
                palette.Get("Platform Accent", palette.Cyan, 0.1f, 0.8f, true),
                false
            );
            platformRing.transform.localScale = new Vector3(2.46f, 0.015f, 2.46f);

            CreateWorkbench(parent, palette);
            CreateWallAccents(parent, palette);
        }

        private static void CreateWorkbench(Transform parent, LabPalette palette)
        {
            CreatePrimitive(
                parent,
                "Workbench Top",
                PrimitiveType.Cube,
                new Vector3(0f, 1.02f, 0.25f),
                new Vector3(3.65f, 0.18f, 1.72f),
                palette.Get("Workbench", new Color(0.80f, 0.86f, 0.91f), 0.12f, 0.72f)
            );

            CreatePrimitive(
                parent,
                "Workbench Inlay",
                PrimitiveType.Cube,
                new Vector3(0f, 1.116f, 0.25f),
                new Vector3(3.25f, 0.025f, 1.34f),
                palette.Get("Workbench Inlay", new Color(0.075f, 0.12f, 0.18f), 0.05f, 0.55f)
            );

            foreach (var x in new[] { -1.55f, 1.55f })
            {
                foreach (var z in new[] { -0.35f, 0.86f })
                {
                    CreatePrimitive(
                        parent,
                        "Workbench Leg",
                        PrimitiveType.Cube,
                        new Vector3(x, 0.49f, z),
                        new Vector3(0.16f, 0.98f, 0.16f),
                        palette.Get("Workbench Frame", new Color(0.25f, 0.32f, 0.39f), 0.55f, 0.7f)
                    );
                }
            }

            CreatePrimitive(
                parent,
                "Front Light Strip",
                PrimitiveType.Cube,
                new Vector3(0f, 1.055f, -0.62f),
                new Vector3(3.28f, 0.035f, 0.025f),
                palette.Get("Workbench Light", palette.Cyan, 0f, 0.8f, true),
                false
            );

            CreateTray(parent, palette, new Vector3(-1.35f, 1.145f, 0.55f), "POWER");
            CreateTray(parent, palette, new Vector3(1.35f, 1.145f, 0.55f), "LOADS");
        }

        private static void CreateTray(Transform parent, LabPalette palette, Vector3 position, string label)
        {
            var tray = CreatePrimitive(
                parent,
                $"{label} Tray",
                PrimitiveType.Cube,
                position,
                new Vector3(0.78f, 0.045f, 0.55f),
                palette.Get($"{label} Tray", new Color(0.12f, 0.19f, 0.26f), 0.15f, 0.75f)
            );
            tray.GetComponent<Collider>().enabled = false;

            var text = WorldTextFactory.Create(
                parent,
                $"{label} Tray Label",
                label,
                position + new Vector3(0f, 0.045f, -0.20f),
                0.028f,
                44,
                palette.PanelMuted,
                TextAnchor.MiddleCenter,
                TextAlignment.Center
            );
            text.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void CreateWallAccents(Transform parent, LabPalette palette)
        {
            for (var i = -3; i <= 3; i++)
            {
                CreatePrimitive(
                    parent,
                    $"Wall Accent {i}",
                    PrimitiveType.Cube,
                    new Vector3(i * 1.08f, 3.85f, 2.76f),
                    new Vector3(0.72f, 0.035f, 0.035f),
                    palette.Get("Wall Accent", i % 2 == 0 ? palette.Cyan : palette.Purple, 0f, 0.8f, true),
                    false
                );
            }
        }

        private static void CreateLighting(Transform parent, LabPalette palette)
        {
            var key = new GameObject("Key Directional Light").AddComponent<Light>();
            key.transform.SetParent(parent, false);
            key.type = LightType.Directional;
            key.color = new Color(0.78f, 0.86f, 1f);
            key.intensity = 0.85f;
            key.shadows = LightShadows.Soft;
            key.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

            var fill = new GameObject("Workbench Fill Light").AddComponent<Light>();
            fill.transform.SetParent(parent, false);
            fill.type = LightType.Point;
            fill.color = palette.Cyan;
            fill.range = 6f;
            fill.intensity = 1.2f;
            fill.transform.localPosition = new Vector3(-2f, 2.5f, -0.2f);

            var warm = new GameObject("Warm Fill Light").AddComponent<Light>();
            warm.transform.SetParent(parent, false);
            warm.type = LightType.Point;
            warm.color = palette.Accent;
            warm.range = 5f;
            warm.intensity = 0.8f;
            warm.transform.localPosition = new Vector3(2.2f, 2.25f, 0.8f);

            foreach (var x in new[] { -1.8f, 0f, 1.8f })
            {
                var fixture = CreatePrimitive(
                    parent,
                    "Ceiling Light Fixture",
                    PrimitiveType.Cylinder,
                    new Vector3(x, 3.65f, 0.1f),
                    Vector3.one,
                    palette.Get("Ceiling Fixture", palette.Panel, 0.1f, 0.8f, true),
                    false
                );
                fixture.transform.localScale = new Vector3(0.42f, 0.025f, 0.42f);
            }
        }

        private static XRTrackedRig CreateRig(Transform parent, LabPalette palette, XRWireAuthoringTool wireTool)
        {
            var rigRoot = new GameObject("OpenXR Rig").transform;
            rigRoot.SetParent(parent, false);
            rigRoot.localPosition = new Vector3(0f, 0f, -2.45f);

            var head = new GameObject("Head").transform;
            head.SetParent(rigRoot, false);
            head.localPosition = new Vector3(0f, 1.65f, 0f);
            var camera = head.gameObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = palette.Background;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 60f;
            head.gameObject.AddComponent<AudioListener>();

            var leftHand = CreateHand(rigRoot, "Left Hand", new Vector3(-0.27f, 1.32f, 0.42f), palette.Cyan);
            var rightHand = CreateHand(rigRoot, "Right Hand", new Vector3(0.27f, 1.32f, 0.42f), palette.Accent);

            var rig = rigRoot.gameObject.AddComponent<XRTrackedRig>();
            rig.Configure(head, leftHand, rightHand);

            leftHand.gameObject.AddComponent<XRHandGrabber>().Configure(XRNode.LeftHand, 0.14f);
            rightHand.gameObject.AddComponent<XRHandGrabber>().Configure(XRNode.RightHand, 0.14f);

            var pointerLine = rightHand.gameObject.AddComponent<LineRenderer>();
            pointerLine.shadowCastingMode = ShadowCastingMode.Off;
            pointerLine.receiveShadows = false;
            var pointer = rightHand.gameObject.AddComponent<XRPointerInteractor>();
            pointer.Configure(
                XRNode.RightHand,
                wireTool,
                palette.Get("Pointer", palette.Cyan, 0f, 0.8f, true),
                6f
            );

            return rig;
        }

        private static Transform CreateHand(Transform parent, string name, Vector3 initialPosition, Color color)
        {
            var hand = new GameObject(name).transform;
            hand.SetParent(parent, false);
            hand.localPosition = initialPosition;

            var palm = CreatePrimitive(
                hand,
                "Controller Visual",
                PrimitiveType.Capsule,
                Vector3.zero,
                new Vector3(0.075f, 0.12f, 0.075f),
                new LabPalette().Get(name, color, 0.15f, 0.75f, true),
                false
            );
            palm.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            return hand;
        }

        private static void CreateControllerHint(Transform rightHand, LabPalette palette)
        {
            if (rightHand == null)
            {
                return;
            }

            var indicator = CreatePrimitive(
                rightHand,
                "Pointer Tip",
                PrimitiveType.Sphere,
                new Vector3(0f, 0f, 0.095f),
                Vector3.one * 0.03f,
                palette.Get("Pointer Tip", palette.Cyan, 0f, 0.8f, true),
                false
            );
            indicator.GetComponent<Collider>().enabled = false;
        }

        private static void CreateMissionPanel(Transform parent, LabPalette palette)
        {
            var panelRoot = new GameObject("Mission Board").transform;
            panelRoot.SetParent(parent, false);
            panelRoot.localPosition = new Vector3(-1.55f, 2.15f, 2.70f);
            panelRoot.localRotation = Quaternion.Euler(0f, 180f, 0f);

            CreatePrimitive(
                panelRoot,
                "Mission Panel Surface",
                PrimitiveType.Cube,
                Vector3.zero,
                new Vector3(2.25f, 1.12f, 0.08f),
                palette.Get("Mission Panel", new Color(0.10f, 0.17f, 0.25f), 0.1f, 0.72f)
            );
            CreatePrimitive(
                panelRoot,
                "Mission Panel Header",
                PrimitiveType.Cube,
                new Vector3(0f, 0.49f, -0.05f),
                new Vector3(2.25f, 0.14f, 0.03f),
                palette.Get("Mission Header", palette.Purple, 0f, 0.75f, true),
                false
            );

            WorldTextFactory.Create(
                panelRoot,
                "Mission Kicker",
                "MISSION 02 · CIRCUIT PATHS",
                new Vector3(-0.98f, 0.42f, -0.075f),
                0.032f,
                46,
                palette.LightText
            );
            WorldTextFactory.Create(
                panelRoot,
                "Mission Title",
                "직렬과 병렬 회로의\n전류 경로를 비교하세요",
                new Vector3(-0.98f, 0.22f, -0.075f),
                0.045f,
                48,
                Color.white
            );
            WorldTextFactory.Create(
                panelRoot,
                "Mission Detail",
                "1. 부품을 실험대 중앙으로 옮기기\n2. Trigger로 두 단자를 차례로 선택하기\n3. 닫힌 경로를 만든 뒤 분석하기",
                new Vector3(-0.98f, -0.10f, -0.075f),
                0.031f,
                42,
                palette.PanelMuted
            );
        }

        private static void CreateInstructionPanel(Transform parent, LabPalette palette)
        {
            var panelRoot = new GameObject("Interaction Guide").transform;
            panelRoot.SetParent(parent, false);
            panelRoot.localPosition = new Vector3(0f, 1.52f, 2.72f);
            panelRoot.localRotation = Quaternion.Euler(0f, 180f, 0f);

            CreatePrimitive(
                panelRoot,
                "Guide Surface",
                PrimitiveType.Cube,
                Vector3.zero,
                new Vector3(0.95f, 0.86f, 0.07f),
                palette.Get("Guide Panel", new Color(0.08f, 0.13f, 0.20f), 0.1f, 0.65f)
            );
            WorldTextFactory.Create(
                panelRoot,
                "Guide Title",
                "XR CONTROLS",
                new Vector3(-0.39f, 0.34f, -0.06f),
                0.03f,
                45,
                palette.Cyan
            );
            WorldTextFactory.Create(
                panelRoot,
                "Guide Body",
                "Grip   부품 이동\nTrigger 단자·버튼 선택\nB 버튼 연결 선택 취소",
                new Vector3(-0.39f, 0.17f, -0.06f),
                0.029f,
                42,
                palette.LightText
            );
        }

        private static MisconceptionCoachPanel CreateCoachPanel(Transform parent, LabPalette palette)
        {
            var panelRoot = new GameObject("AI Coach Panel").transform;
            panelRoot.SetParent(parent, false);
            panelRoot.localPosition = new Vector3(1.60f, 2.14f, 2.70f);
            panelRoot.localRotation = Quaternion.Euler(0f, 180f, 0f);

            CreatePrimitive(
                panelRoot,
                "Coach Surface",
                PrimitiveType.Cube,
                Vector3.zero,
                new Vector3(2.40f, 1.14f, 0.08f),
                palette.Get("Coach Surface", palette.Panel, 0f, 0.62f)
            );
            var statusBar = CreatePrimitive(
                panelRoot,
                "Coach Status Bar",
                PrimitiveType.Cube,
                new Vector3(0f, 0.50f, -0.052f),
                new Vector3(2.40f, 0.12f, 0.035f),
                palette.Get("Coach Status", palette.Success, 0f, 0.8f, true),
                false
            ).GetComponent<Renderer>();

            var title = WorldTextFactory.Create(
                panelRoot,
                "Coach Title",
                string.Empty,
                new Vector3(-1.04f, 0.38f, -0.07f),
                0.042f,
                48,
                palette.DarkText
            );
            var body = WorldTextFactory.Create(
                panelRoot,
                "Coach Body",
                string.Empty,
                new Vector3(-1.04f, 0.13f, -0.07f),
                0.031f,
                42,
                new Color(0.15f, 0.20f, 0.27f)
            );
            var risk = WorldTextFactory.Create(
                panelRoot,
                "Coach Risk",
                string.Empty,
                new Vector3(-1.04f, -0.43f, -0.07f),
                0.027f,
                40,
                new Color(0.36f, 0.42f, 0.50f)
            );

            var coach = panelRoot.gameObject.AddComponent<MisconceptionCoachPanel>();
            coach.Configure(title, body, risk, statusBar, palette);
            return coach;
        }

        private static void CreateControlDeck(
            Transform parent,
            LabPalette palette,
            out XRWorldButton analyzeButton,
            Action analyze,
            Action undo,
            Action clear,
            Action reset)
        {
            var deck = new GameObject("Control Deck").transform;
            deck.SetParent(parent, false);
            deck.localPosition = new Vector3(0f, 1.30f, -0.78f);
            deck.localRotation = Quaternion.Euler(0f, 180f, 0f);

            analyzeButton = CreateButton(deck, palette, "회로 분석", new Vector3(-1.18f, 0f, 0f), palette.Cyan, analyze);
            CreateButton(deck, palette, "연결 취소", new Vector3(-0.40f, 0f, 0f), palette.Purple, undo);
            CreateButton(deck, palette, "전선 지우기", new Vector3(0.40f, 0f, 0f), palette.Accent, clear);
            CreateButton(deck, palette, "전체 초기화", new Vector3(1.18f, 0f, 0f), palette.Danger, reset);
        }

        private static XRWorldButton CreateButton(
            Transform parent,
            LabPalette palette,
            string label,
            Vector3 localPosition,
            Color color,
            Action callback)
        {
            var root = new GameObject($"Button - {label}").transform;
            root.SetParent(parent, false);
            root.localPosition = localPosition;

            var surfaceObject = CreatePrimitive(
                root,
                "Surface",
                PrimitiveType.Cube,
                Vector3.zero,
                new Vector3(0.68f, 0.22f, 0.10f),
                palette.Get($"Button {label}", color, 0.05f, 0.72f, true)
            );
            var surface = surfaceObject.GetComponent<Renderer>();
            var text = WorldTextFactory.Create(
                root,
                "Label",
                label,
                new Vector3(0f, 0f, -0.06f),
                0.032f,
                44,
                Color.white,
                TextAnchor.MiddleCenter,
                TextAlignment.Center
            );

            var button = root.gameObject.AddComponent<XRWorldButton>();
            button.Configure(
                text,
                surface,
                palette.Get($"Button {label}", color, 0.05f, 0.72f, true),
                palette.Get($"Button {label} Hover", Color.Lerp(color, Color.white, 0.25f), 0f, 0.85f, true),
                palette.Get("Button Disabled", new Color(0.22f, 0.25f, 0.30f), 0f, 0.35f),
                callback
            );
            return button;
        }

        private static void CreateCircuitComponents(
            Transform parent,
            LabPalette palette,
            CircuitGraphBuilder graphBuilder,
            SessionEventLogger eventLogger)
        {
            CreateBattery(parent, palette, new Vector3(-1.35f, BenchSurfaceY + 0.12f, 0.53f));
            CreateBulb(parent, palette, "bulb_1", "전구 A", new Vector3(0.72f, BenchSurfaceY + 0.16f, 0.47f));
            CreateBulb(parent, palette, "bulb_2", "전구 B", new Vector3(1.35f, BenchSurfaceY + 0.16f, 0.47f));
            CreateResistor(parent, palette, new Vector3(1.05f, BenchSurfaceY + 0.10f, 0.04f));
            CreateSwitch(parent, palette, graphBuilder, eventLogger, new Vector3(-0.95f, BenchSurfaceY + 0.10f, 0.04f));
        }

        private static void CreateBattery(Transform parent, LabPalette palette, Vector3 position)
        {
            var root = CreateComponentRoot(parent, "battery_1", "3V 배터리", position, palette);
            var body = CreatePrimitive(root.transform, "Battery Body", PrimitiveType.Cube, Vector3.zero, new Vector3(0.36f, 0.22f, 0.24f), palette.Get("Battery Body", new Color(0.14f, 0.18f, 0.23f), 0.35f, 0.65f));
            body.layer = 2;
            CreatePrimitive(root.transform, "Battery Band", PrimitiveType.Cube, new Vector3(0.07f, 0f, 0f), new Vector3(0.05f, 0.225f, 0.245f), palette.Get("Battery Band", palette.Accent, 0.1f, 0.65f), false).layer = 2;

            var node = root.AddComponent<XRComponentNode>();
            var terminals = new List<Transform>
            {
                CreateTerminal(root.transform, node, palette, "Positive Terminal", new Vector3(-0.13f, 0.15f, 0f), palette.Danger),
                CreateTerminal(root.transform, node, palette, "Negative Terminal", new Vector3(0.13f, 0.15f, 0f), palette.Navy)
            };
            node.Configure("battery_1", CircuitComponentType.Battery, "3V Battery", 0f, 3f, terminals);
            ConfigureGrabbable(root, position);
            CreateComponentLabel(root.transform, palette, "3V BATTERY");
        }

        private static void CreateBulb(Transform parent, LabPalette palette, string id, string label, Vector3 position)
        {
            var root = CreateComponentRoot(parent, id, label, position, palette);
            CreatePrimitive(root.transform, "Bulb Base", PrimitiveType.Cylinder, new Vector3(0f, 0.01f, 0f), new Vector3(0.19f, 0.13f, 0.19f), palette.Get("Bulb Base", new Color(0.33f, 0.38f, 0.44f), 0.65f, 0.78f)).layer = 2;
            var globe = CreatePrimitive(root.transform, "Bulb Globe", PrimitiveType.Sphere, new Vector3(0f, 0.19f, 0f), new Vector3(0.27f, 0.27f, 0.27f), palette.Get($"{id} Globe", new Color(0.44f, 0.49f, 0.52f), 0f, 0.78f));
            globe.layer = 2;

            var node = root.AddComponent<XRComponentNode>();
            var terminals = new List<Transform>
            {
                CreateTerminal(root.transform, node, palette, "Terminal L", new Vector3(-0.18f, 0.02f, 0f), palette.Copper),
                CreateTerminal(root.transform, node, palette, "Terminal R", new Vector3(0.18f, 0.02f, 0f), palette.Copper)
            };
            node.Configure(id, CircuitComponentType.Bulb, label, 10f, 0f, terminals);

            var glow = new GameObject("Bulb Glow").AddComponent<Light>();
            glow.transform.SetParent(root.transform, false);
            glow.transform.localPosition = new Vector3(0f, 0.19f, 0f);
            glow.type = LightType.Point;
            glow.color = new Color(1f, 0.72f, 0.24f);
            glow.range = 1.4f;
            glow.shadows = LightShadows.None;

            root.AddComponent<BulbVisualController>().Configure(node, globe.GetComponent<Renderer>(), glow, new Color(0.42f, 0.46f, 0.50f), new Color(1f, 0.70f, 0.20f));
            ConfigureGrabbable(root, position);
            CreateComponentLabel(root.transform, palette, id == "bulb_1" ? "BULB A" : "BULB B");
        }

        private static void CreateResistor(Transform parent, LabPalette palette, Vector3 position)
        {
            var root = CreateComponentRoot(parent, "resistor_1", "20Ω 저항", position, palette);
            CreatePrimitive(root.transform, "Resistor Body", PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.13f, 0.28f, 0.13f), palette.Get("Resistor Body", new Color(0.86f, 0.74f, 0.52f), 0.05f, 0.55f)).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            CreatePrimitive(root.transform, "Resistor Band 1", PrimitiveType.Cylinder, new Vector3(-0.08f, 0f, 0f), new Vector3(0.135f, 0.025f, 0.135f), palette.Get("Resistor Band 1", palette.Danger, 0f, 0.5f), false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            CreatePrimitive(root.transform, "Resistor Band 2", PrimitiveType.Cylinder, new Vector3(0.02f, 0f, 0f), new Vector3(0.135f, 0.025f, 0.135f), palette.Get("Resistor Band 2", palette.Purple, 0f, 0.5f), false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            var node = root.AddComponent<XRComponentNode>();
            var terminals = new List<Transform>
            {
                CreateTerminal(root.transform, node, palette, "Terminal L", new Vector3(-0.24f, 0f, 0f), palette.Copper),
                CreateTerminal(root.transform, node, palette, "Terminal R", new Vector3(0.24f, 0f, 0f), palette.Copper)
            };
            node.Configure("resistor_1", CircuitComponentType.Resistor, "20 Ohm Resistor", 20f, 0f, terminals);
            ConfigureGrabbable(root, position);
            CreateComponentLabel(root.transform, palette, "20Ω RESISTOR");
            SetBodyLayers(root.transform);
        }

        private static void CreateSwitch(
            Transform parent,
            LabPalette palette,
            CircuitGraphBuilder graphBuilder,
            SessionEventLogger eventLogger,
            Vector3 position)
        {
            var root = CreateComponentRoot(parent, "switch_1", "스위치", position, palette);
            CreatePrimitive(root.transform, "Switch Base", PrimitiveType.Cube, Vector3.zero, new Vector3(0.48f, 0.10f, 0.24f), palette.Get("Switch Base", new Color(0.20f, 0.26f, 0.32f), 0.35f, 0.7f)).layer = 2;
            var lever = CreatePrimitive(root.transform, "Switch Lever", PrimitiveType.Cube, new Vector3(0f, 0.10f, 0f), new Vector3(0.30f, 0.045f, 0.055f), palette.Get("Switch Lever", palette.Accent, 0.4f, 0.75f)).transform;
            lever.gameObject.layer = 2;

            var node = root.AddComponent<XRComponentNode>();
            var terminals = new List<Transform>
            {
                CreateTerminal(root.transform, node, palette, "Terminal L", new Vector3(-0.20f, 0.08f, 0f), palette.Copper),
                CreateTerminal(root.transform, node, palette, "Terminal R", new Vector3(0.20f, 0.08f, 0f), palette.Copper)
            };
            node.Configure("switch_1", CircuitComponentType.Switch, "Switch", 0f, 0f, terminals);
            root.AddComponent<XRSwitchToggle>().Configure(node, lever, graphBuilder, eventLogger);
            var interactionCollider = root.AddComponent<BoxCollider>();
            interactionCollider.center = new Vector3(0f, 0.10f, 0f);
            interactionCollider.size = new Vector3(0.24f, 0.18f, 0.16f);
            ConfigureGrabbable(root, position);
            CreateComponentLabel(root.transform, palette, "SWITCH");
        }

        private static GameObject CreateComponentRoot(Transform parent, string id, string label, Vector3 position, LabPalette palette)
        {
            var root = new GameObject($"{id} - {label}");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            return root;
        }

        private static Transform CreateTerminal(
            Transform parent,
            XRComponentNode owner,
            LabPalette palette,
            string name,
            Vector3 localPosition,
            Color color)
        {
            var terminal = CreatePrimitive(
                parent,
                name,
                PrimitiveType.Sphere,
                localPosition,
                Vector3.one * 0.105f,
                palette.Get($"Terminal {color}", color, 0.65f, 0.8f, true)
            );
            terminal.layer = 0;
            terminal.GetComponent<SphereCollider>().radius = 0.65f;
            var selectedMaterial = palette.Get($"Terminal Selected {color}", Color.Lerp(color, Color.white, 0.55f), 0.2f, 0.9f, true);
            terminal.AddComponent<CircuitTerminal>().Configure(owner, terminal.GetComponent<Renderer>(), terminal.GetComponent<Renderer>().sharedMaterial, selectedMaterial);
            return terminal.transform;
        }

        private static void ConfigureGrabbable(GameObject root, Vector3 homePosition)
        {
            var grabbable = root.AddComponent<XRGrabbable>();
            grabbable.Configure(
                homePosition.y,
                new Vector2(-1.55f, 1.55f),
                new Vector2(-0.40f, 0.80f)
            );
        }

        private static void CreateComponentLabel(Transform parent, LabPalette palette, string text)
        {
            var label = WorldTextFactory.Create(
                parent,
                "Component Label",
                text,
                new Vector3(0f, -0.16f, -0.18f),
                0.021f,
                40,
                palette.LightText,
                TextAnchor.MiddleCenter,
                TextAlignment.Center
            );
            label.transform.localRotation = Quaternion.Euler(65f, 180f, 0f);
        }

        private static void ResetLearningSpace(
            XRWireAuthoringTool wireTool,
            CurrentFlowVisualizer flowVisualizer,
            NodeOverlayPresenter overlayPresenter,
            SessionEventLogger eventLogger)
        {
            wireTool.ClearAllWires();
            flowVisualizer.Clear();
            overlayPresenter.Clear();
            foreach (var grabbable in UnityEngine.Object.FindObjectsByType<XRGrabbable>(FindObjectsSortMode.None))
            {
                grabbable.ResetToHome();
            }
            eventLogger.Log("reset", "{\"target\":\"learning_space\"}");
        }

        private static GameObject CreatePrimitive(
            Transform parent,
            string name,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            bool colliderEnabled = true)
        {
            var primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;
            var renderer = primitive.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            var collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = colliderEnabled;
            }
            return primitive;
        }

        private static void SetBodyLayers(Transform root)
        {
            foreach (Transform child in root)
            {
                if (child.GetComponent<CircuitTerminal>() == null)
                {
                    child.gameObject.layer = 2;
                }
            }
        }
    }
}
