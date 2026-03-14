using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

namespace PlatNav
{
    [CustomEditor(typeof(PlatNavBake))]
    public class PlatNavBakeEditor : Editor
    {
        SerializedProperty wallTM, spikesTM, graph, entitySize;
        SerializedProperty minPos, maxPos;
        SerializedProperty maxJumpVelocity, gravityStrength, walkSpeed;
        SerializedProperty jumpSearchRadius, numTrajectoriesToTest, trajectoryStep, maxFallTime;
        SerializedProperty jumpCostMultiplier, fallCostMultiplier;
        SerializedProperty showBakeBounds;
        SerializedProperty showSegments, showJumpLinks, showFallLinks;
        SerializedProperty showRejectedTrajectories;
        SerializedProperty bakeBoundsColor;
        SerializedProperty segmentColor, jumpLinkColor, fallLinkColor;
        SerializedProperty rejectedArcColor;

        bool foldRefs    = true;
        bool foldBake    = true;
        bool foldGizmos  = true;
        bool foldColors  = false;

        static GUIStyle _headerStyle;
        static GUIStyle _boxStyle;

        static GUIStyle HeaderStyle => _headerStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            margin   = new RectOffset(0, 0, 6, 2)
        };

        static GUIStyle BoxStyle
        {
            get
            {
                if (_boxStyle != null) return _boxStyle;
                _boxStyle = new GUIStyle("HelpBox")
                {
                    padding = new RectOffset(8, 8, 6, 6),
                    margin  = new RectOffset(0, 0, 2, 4)
                };
                return _boxStyle;
            }
        }

        void OnEnable()
        {
            wallTM    = serializedObject.FindProperty("wallTM");
            spikesTM  = serializedObject.FindProperty("spikesTM");
            graph     = serializedObject.FindProperty("graph");
            entitySize = serializedObject.FindProperty("entitySize");

            minPos = serializedObject.FindProperty("minPos");
            maxPos = serializedObject.FindProperty("maxPos");

            maxJumpVelocity      = serializedObject.FindProperty("maxJumpVelocity");
            gravityStrength      = serializedObject.FindProperty("gravityStrength");
            walkSpeed            = serializedObject.FindProperty("walkSpeed");
            jumpSearchRadius     = serializedObject.FindProperty("jumpSearchRadius");
            numTrajectoriesToTest = serializedObject.FindProperty("numTrajectoriesToTest");
            trajectoryStep       = serializedObject.FindProperty("trajectoryStep");
            maxFallTime          = serializedObject.FindProperty("maxFallTime");
            jumpCostMultiplier   = serializedObject.FindProperty("jumpCostMultiplier");
            fallCostMultiplier   = serializedObject.FindProperty("fallCostMultiplier");

            showBakeBounds           = serializedObject.FindProperty("showBakeBounds");
            showSegments             = serializedObject.FindProperty("showSegments");
            showJumpLinks            = serializedObject.FindProperty("showJumpLinks");
            showFallLinks            = serializedObject.FindProperty("showFallLinks");
            showRejectedTrajectories = serializedObject.FindProperty("showRejectedTrajectories");

            bakeBoundsColor     = serializedObject.FindProperty("bakeBoundsColor");
            segmentColor        = serializedObject.FindProperty("segmentColor");
            jumpLinkColor       = serializedObject.FindProperty("jumpLinkColor");
            fallLinkColor       = serializedObject.FindProperty("fallLinkColor");
            rejectedArcColor    = serializedObject.FindProperty("rejectedArcColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var baker = (PlatNavBake)target;

            foldRefs = EditorGUILayout.Foldout(foldRefs, "References & Setup", true, EditorStyles.foldoutHeader);
            if (foldRefs)
            {
                EditorGUILayout.BeginVertical(BoxStyle);
                EditorGUILayout.PropertyField(wallTM,    new GUIContent("Wall Tilemap"));
                EditorGUILayout.PropertyField(spikesTM,  new GUIContent("Spikes Tilemap"));
                EditorGUILayout.PropertyField(graph,     new GUIContent("Graph Asset"));
                EditorGUILayout.Space(4);
                EditorGUILayout.PropertyField(entitySize, new GUIContent("Entity Size"));
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Bake Bounds (tile coords)", EditorStyles.miniLabel);
                EditorGUILayout.PropertyField(minPos, new GUIContent("Min"));
                EditorGUILayout.PropertyField(maxPos, new GUIContent("Max"));
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(2);

            foldBake = EditorGUILayout.Foldout(foldBake, "Bake Settings", true, EditorStyles.foldoutHeader);
            if (foldBake)
            {
                EditorGUILayout.BeginVertical(BoxStyle);

                EditorGUILayout.LabelField("Physics", HeaderStyle);
                EditorGUILayout.PropertyField(maxJumpVelocity);
                EditorGUILayout.PropertyField(gravityStrength);
                EditorGUILayout.PropertyField(walkSpeed);

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Search", HeaderStyle);
                EditorGUILayout.PropertyField(jumpSearchRadius);
                EditorGUILayout.PropertyField(numTrajectoriesToTest);
                EditorGUILayout.PropertyField(trajectoryStep);
                EditorGUILayout.PropertyField(maxFallTime);

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Cost", HeaderStyle);
                EditorGUILayout.PropertyField(jumpCostMultiplier);
                EditorGUILayout.PropertyField(fallCostMultiplier);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(2);

            foldGizmos = EditorGUILayout.Foldout(foldGizmos, "Gizmos", true, EditorStyles.foldoutHeader);
            if (foldGizmos)
            {
                EditorGUILayout.BeginVertical(BoxStyle);

                DrawToggleRow("Bake Bounds",          showBakeBounds,           bakeBoundsColor);
                DrawToggleRow("Segments",             showSegments,             segmentColor);
                DrawToggleRow("Jump Links",           showJumpLinks,            jumpLinkColor);
                DrawToggleRow("Fall Links",           showFallLinks,            fallLinkColor);
                DrawToggleRow("Rejected Trajectories", showRejectedTrajectories, rejectedArcColor);

                EditorGUILayout.Space(4);
                foldColors = EditorGUILayout.Foldout(foldColors, "All Colors", true);
                if (foldColors)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(bakeBoundsColor,    new GUIContent("Bake Bounds"));
                    EditorGUILayout.PropertyField(segmentColor,        new GUIContent("Segments"));
                    EditorGUILayout.PropertyField(jumpLinkColor,       new GUIContent("Jump Links"));
                    EditorGUILayout.PropertyField(fallLinkColor,       new GUIContent("Fall Links"));
                    EditorGUILayout.PropertyField(rejectedArcColor,    new GUIContent("Rejected Arcs"));
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(6);

            DrawBakeSection(baker);

            DrawGraphInfo(baker);

            serializedObject.ApplyModifiedProperties();
        }

        void DrawToggleRow(string label, SerializedProperty toggle, SerializedProperty color)
        {
            EditorGUILayout.BeginHorizontal();
            toggle.boolValue = EditorGUILayout.ToggleLeft(label, toggle.boolValue, GUILayout.MinWidth(160));
            color.colorValue = EditorGUILayout.ColorField(GUIContent.none, color.colorValue,
                                   false, true, false, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
        }

        void DrawBakeSection(PlatNavBake baker)
        {
            EditorGUILayout.BeginVertical(BoxStyle);

            var bakeColor = new Color(0.35f, 0.75f, 0.35f);
            GUI.backgroundColor = bakeColor;
            bool bake = GUILayout.Button("Bake Navigation Graph", GUILayout.Height(32));
            GUI.backgroundColor = Color.white;

            if (bake)
            {
                Undo.RecordObject(baker, "Bake PlatNav Graph");
                baker.Bake();
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();

            // Clear rejected arcs
            int rejCount = baker.rejectedArcs?.Count ?? 0;
            GUI.enabled = rejCount > 0;
            if (GUILayout.Button($"Clear Rejected Arcs ({rejCount})", GUILayout.Height(22)))
            {
                baker.rejectedArcs.Clear();
                SceneView.RepaintAll();
            }
            GUI.enabled = true;

            // Toggle all gizmos
            if (GUILayout.Button("All On", GUILayout.Width(55), GUILayout.Height(22)))
                SetAllGizmos(true);
            if (GUILayout.Button("All Off", GUILayout.Width(55), GUILayout.Height(22)))
                SetAllGizmos(false);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        void SetAllGizmos(bool on)
        {
            showBakeBounds.boolValue           = on;
            showSegments.boolValue             = on;
            showJumpLinks.boolValue            = on;
            showFallLinks.boolValue            = on;
            showRejectedTrajectories.boolValue = on;
            serializedObject.ApplyModifiedProperties();
            SceneView.RepaintAll();
        }

        void DrawGraphInfo(PlatNavBake baker)
        {
            var graphProp = graph.objectReferenceValue as PlatformNavGraphAsset;
            if (graphProp == null || graphProp.segments == null) return;

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginVertical(BoxStyle);
            EditorGUILayout.LabelField("Baked Graph Info", HeaderStyle);

            int segCount  = graphProp.segments?.Length ?? 0;
            int linkCount = graphProp.links?.Length ?? 0;

            int jumpLinks = 0, fallLinks = 0;
            if (graphProp.links != null)
            {
                for (int i = 0; i < graphProp.links.Length; i++)
                {
                    if (graphProp.links[i].moveType == LinkMoveType.Jump)
                        jumpLinks++;
                    else
                        fallLinks++;
                }
            }

            EditorGUILayout.LabelField("Segments",             segCount.ToString());
            EditorGUILayout.LabelField("Total Links",          linkCount.ToString());
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Jump",                 jumpLinks.ToString());
            EditorGUILayout.LabelField("Fall",                 fallLinks.ToString());
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Tile Origin",          graphProp.tileOrigin.ToString());
            EditorGUILayout.LabelField("Tilemap Size",         graphProp.tilemapSize.ToString());

            EditorGUILayout.EndVertical();
        }

        void OnSceneGUI()
        {
            serializedObject.Update();

            var tilemap = wallTM.objectReferenceValue as Tilemap;
            if (tilemap == null) return;

            Vector2Int min = minPos.vector2IntValue;
            Vector2Int max = maxPos.vector2IntValue;

            Vector3 wMin = tilemap.CellToWorld(new Vector3Int(min.x, min.y, 0));
            Vector3 wMax = tilemap.CellToWorld(new Vector3Int(max.x, max.y, 0));

            float midX = (wMin.x + wMax.x) * 0.5f;
            float midY = (wMin.y + wMax.y) * 0.5f;
            Vector3 center = new Vector3(midX, midY, 0);
            float hSize = HandleUtility.GetHandleSize(center) * 0.05f;

            Color handleCol = bakeBoundsColor != null
                ? bakeBoundsColor.colorValue
                : new Color(1f, 0.92f, 0.016f, 1f);
            Handles.color = handleCol;

            bool changed = false;
            int newMinX = min.x, newMinY = min.y, newMaxX = max.x, newMaxY = max.y;

            // Left edge
            EditorGUI.BeginChangeCheck();
            Vector3 leftH = Handles.Slider(
                new Vector3(wMin.x, midY, 0), Vector3.right, hSize, Handles.DotHandleCap, 0);
            if (EditorGUI.EndChangeCheck())
            {
                newMinX = SnapToTileBoundaryX(tilemap, leftH.x);
                changed = true;
            }

            // Right edge
            EditorGUI.BeginChangeCheck();
            Vector3 rightH = Handles.Slider(
                new Vector3(wMax.x, midY, 0), Vector3.right, hSize, Handles.DotHandleCap, 0);
            if (EditorGUI.EndChangeCheck())
            {
                newMaxX = SnapToTileBoundaryX(tilemap, rightH.x);
                changed = true;
            }

            // Bottom edge
            EditorGUI.BeginChangeCheck();
            Vector3 bottomH = Handles.Slider(
                new Vector3(midX, wMin.y, 0), Vector3.up, hSize, Handles.DotHandleCap, 0);
            if (EditorGUI.EndChangeCheck())
            {
                newMinY = SnapToTileBoundaryY(tilemap, bottomH.y);
                changed = true;
            }

            // Top edge
            EditorGUI.BeginChangeCheck();
            Vector3 topH = Handles.Slider(
                new Vector3(midX, wMax.y, 0), Vector3.up, hSize, Handles.DotHandleCap, 0);
            if (EditorGUI.EndChangeCheck())
            {
                newMaxY = SnapToTileBoundaryY(tilemap, topH.y);
                changed = true;
            }

            if (changed)
            {
                // Ensure min < max
                if (newMinX >= newMaxX) newMaxX = newMinX + 1;
                if (newMinY >= newMaxY) newMaxY = newMinY + 1;

                Undo.RecordObject(target, "Edit Bake Bounds");
                minPos.vector2IntValue = new Vector2Int(newMinX, newMinY);
                maxPos.vector2IntValue = new Vector2Int(newMaxX, newMaxY);
                serializedObject.ApplyModifiedProperties();
            }
        }

        static int SnapToTileBoundaryX(Tilemap tm, float worldX)
        {
            var cell = tm.WorldToCell(new Vector3(worldX, 0, 0));
            float edgeA = tm.CellToWorld(cell).x;
            float edgeB = tm.CellToWorld(new Vector3Int(cell.x + 1, cell.y, 0)).x;
            return worldX > (edgeA + edgeB) * 0.5f ? cell.x + 1 : cell.x;
        }

        static int SnapToTileBoundaryY(Tilemap tm, float worldY)
        {
            var cell = tm.WorldToCell(new Vector3(0, worldY, 0));
            float edgeA = tm.CellToWorld(cell).y;
            float edgeB = tm.CellToWorld(new Vector3Int(cell.x, cell.y + 1, 0)).y;
            return worldY > (edgeA + edgeB) * 0.5f ? cell.y + 1 : cell.y;
        }
    }
}
