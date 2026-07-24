#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// 特效测试面板 - 菜单 Tools/Effect Tester
/// 功能：在场景中直接生成实际特效进行测试，支持调整位置/旋转/偏移等参数
/// </summary>
public class EffectDebugTool : EditorWindow
{
    private AttackEffectConfig effectConfig;
    private GameObject testTarget;
    private int selectedIndex = 0;
    private GameObject lastSpawnedEffect;
    
    [MenuItem("Tools/Effect Tester")]
    public static void ShowWindow()
    {
        GetWindow<EffectDebugTool>("特效测试面板", true, typeof(SceneView));
    }
    
    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    void OnSceneGUI(SceneView sceneView)
    {
        if (effectConfig == null || effectConfig.attackEffects.Count == 0) return;
        if (selectedIndex >= effectConfig.attackEffects.Count) return;
        
        var data = effectConfig.attackEffects[selectedIndex];
        Vector3 spawnPos = GetSpawnPosition(data);
        
        Handles.color = new Color(1f, 0.3f, 0.3f, 0.3f);
        Handles.SphereHandleCap(0, spawnPos, Quaternion.identity, 0.5f, EventType.Repaint);
        
        if (testTarget != null)
        {
            Handles.color = Color.yellow;
            Handles.DrawLine(testTarget.transform.position, spawnPos);
        }
        
        Handles.Label(spawnPos + Vector3.up * 0.5f, 
            $"特效: {data.effectName}\n" +
            $"位置类型: {data.spawnPositionType}\n" +
            $"偏移: {data.positionOffset}\n" +
            $"旋转: {data.rotationOffset}");
    }
    
    void OnGUI()
    {
        minSize = new Vector2(320, 450);
        GUILayout.Label("=== 特效测试面板 ===", EditorStyles.boldLabel);
        
        effectConfig = EditorGUILayout.ObjectField("特效配置", effectConfig, typeof(AttackEffectConfig), false) as AttackEffectConfig;
        testTarget = EditorGUILayout.ObjectField("测试目标（敌人）", testTarget, typeof(GameObject), true) as GameObject;
        
        EditorGUILayout.Space();
        
        if (effectConfig != null && effectConfig.attackEffects.Count > 0)
        {
            GUILayout.Label("特效列表:", EditorStyles.boldLabel);
            
            for (int i = 0; i < effectConfig.attackEffects.Count; i++)
            {
                var data = effectConfig.attackEffects[i];
                EditorGUILayout.BeginHorizontal();
                
                GUI.backgroundColor = (i == selectedIndex) ? Color.green : Color.gray;
                if (GUILayout.Button("●", GUILayout.Width(25))) { selectedIndex = i; }
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.LabelField($"[{i}] {data.effectName}", GUILayout.Width(80));
                EditorGUILayout.LabelField($"类型:{data.spawnPositionType}", GUILayout.Width(50));
                EditorGUILayout.LabelField($"Y:{data.positionOffset.y:F1}", GUILayout.Width(40));
                
                if (GUILayout.Button("测试", GUILayout.Width(45)))
                {
                    selectedIndex = i;
                    SpawnTestEffect();
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            
            if (selectedIndex < effectConfig.attackEffects.Count)
            {
                EditorGUILayout.LabelField("--- 当前特效配置 ---", EditorStyles.boldLabel);
                var data = effectConfig.attackEffects[selectedIndex];
                
                EditorGUILayout.LabelField("预制体:", data.effectPrefabPath);
                
                // 位置类型
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("位置类型:", GUILayout.Width(70));
                string[] posTypeNames = new string[] { "0脚下", "1中心", "2头顶", "3相机", "4子物体" };
                for (int i = 0; i < 5; i++)
                {
                    GUI.backgroundColor = (data.spawnPositionType == i) ? Color.cyan : Color.gray;
                    if (GUILayout.Button(posTypeNames[i], GUILayout.Width(45)))
                    {
                        Undo.RecordObject(effectConfig, "Change Position Type");
                        data.spawnPositionType = i;
                        EditorUtility.SetDirty(effectConfig);
                    }
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
                
                // 子物体路径
                if (data.spawnPositionType == 4)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("子物体路径:", GUILayout.Width(70));
                    string newPath = EditorGUILayout.TextField(data.spawnTransformPath);
                    if (newPath != data.spawnTransformPath)
                    {
                        Undo.RecordObject(effectConfig, "Change Path");
                        data.spawnTransformPath = newPath;
                        EditorUtility.SetDirty(effectConfig);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                // 位置偏移
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("位置偏移:", GUILayout.Width(70));
                Vector3 newOffset = EditorGUILayout.Vector3Field("", data.positionOffset);
                if (newOffset != data.positionOffset)
                {
                    Undo.RecordObject(effectConfig, "Change Offset");
                    data.positionOffset = newOffset;
                    EditorUtility.SetDirty(effectConfig);
                }
                EditorGUILayout.EndHorizontal();
                
                // Y偏移微调
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Y偏移微调:", GUILayout.Width(70));
                if (GUILayout.Button("+0.5")) { AdjustOffset(0, 0.5f, 0); }
                if (GUILayout.Button("-0.5")) { AdjustOffset(0, -0.5f, 0); }
                if (GUILayout.Button("+1.0")) { AdjustOffset(0, 1f, 0); }
                if (GUILayout.Button("重置")) { ResetOffset(); }
                EditorGUILayout.EndHorizontal();
                
                // 旋转角度
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("旋转角度:", GUILayout.Width(70));
                Vector3 newRot = EditorGUILayout.Vector3Field("", data.rotationOffset);
                if (newRot != data.rotationOffset)
                {
                    Undo.RecordObject(effectConfig, "Change Rotation");
                    data.rotationOffset = newRot;
                    EditorUtility.SetDirty(effectConfig);
                }
                EditorGUILayout.EndHorizontal();
                
                // 旋转微调
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("旋转微调:", GUILayout.Width(70));
                if (GUILayout.Button("X+90")) { AdjustRotation(90, 0, 0); }
                if (GUILayout.Button("Y+90")) { AdjustRotation(0, 90, 0); }
                if (GUILayout.Button("Z+90")) { AdjustRotation(0, 0, 90); }
                if (GUILayout.Button("重置")) { ResetRotation(); }
                EditorGUILayout.EndHorizontal();
                
                // 缩放和延迟
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("缩放:", GUILayout.Width(70));
                float newScale = EditorGUILayout.FloatField(data.effectScale);
                if (newScale != data.effectScale)
                {
                    Undo.RecordObject(effectConfig, "Change Scale");
                    data.effectScale = newScale;
                    EditorUtility.SetDirty(effectConfig);
                }
                EditorGUILayout.LabelField("延迟:", GUILayout.Width(40));
                float newDelay = EditorGUILayout.FloatField(data.delayTime);
                if (newDelay != data.delayTime)
                {
                    Undo.RecordObject(effectConfig, "Change Delay");
                    data.delayTime = newDelay;
                    EditorUtility.SetDirty(effectConfig);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                // 测试按钮
                GUI.backgroundColor = new Color(0.2f, 1f, 0.2f);
                if (GUILayout.Button("▶ 在场景中生成实际特效", GUILayout.Height(40)))
                {
                    SpawnTestEffect();
                }
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("删除特效"))
                {
                    if (lastSpawnedEffect != null) DestroyImmediate(lastSpawnedEffect);
                }
                if (GUILayout.Button("清空所有"))
                {
                    ClearAllEffects();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("请先指定 AttackEffectConfig", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        GUILayout.Box("使用说明:\n" +
            "1. 拖入 AttackEffectConfig\n" +
            "2. 拖入敌人到测试目标\n" +
            "3. 选择特效，点击测试\n" +
            "4. 调整参数后再次测试\n" +
            "5. Ctrl+Z 撤销调整", GUILayout.Height(80));
    }
    
    Vector3 GetSpawnPosition(AttackEffectData data)
    {
        Vector3 basePos = testTarget != null ? testTarget.transform.position : Vector3.zero;
        switch (data.spawnPositionType)
        {
            case 0: return new Vector3(basePos.x, 0f, basePos.z) + data.positionOffset;
            case 1: return basePos + data.positionOffset;
            case 2: return new Vector3(basePos.x, basePos.y + 1f, basePos.z) + data.positionOffset;
            case 3: return Camera.main != null ? new Vector3(Camera.main.transform.position.x, 0f, Camera.main.transform.position.z) + data.positionOffset : basePos + data.positionOffset;
            case 4:
                if (testTarget != null && !string.IsNullOrEmpty(data.spawnTransformPath))
                {
                    Transform tf = testTarget.transform.Find(data.spawnTransformPath);
                    if (tf != null) return tf.position + data.positionOffset;
                }
                return basePos + data.positionOffset;
            default: return basePos + data.positionOffset;
        }
    }
    
    void SpawnTestEffect()
    {
        if (effectConfig == null || selectedIndex >= effectConfig.attackEffects.Count) return;
        var data = effectConfig.attackEffects[selectedIndex];
        
        GameObject prefab = Resources.Load<GameObject>(data.effectPrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"特效预制体未找到: {data.effectPrefabPath}");
            return;
        }
        
        if (lastSpawnedEffect != null) DestroyImmediate(lastSpawnedEffect);
        
        Vector3 spawnPos = GetSpawnPosition(data);
        lastSpawnedEffect = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        lastSpawnedEffect.transform.position = spawnPos;
        lastSpawnedEffect.transform.localScale = Vector3.one * data.effectScale;
        lastSpawnedEffect.transform.rotation = Quaternion.Euler(data.rotationOffset);
        
        Debug.Log($"[EffectTester] 生成特效: {data.effectName}\n" +
            $"位置: {spawnPos}, 类型: {data.spawnPositionType}, 偏移: {data.positionOffset}\n" +
            $"旋转: {data.rotationOffset}, 缩放: {data.effectScale}");
    }
    
    void AdjustOffset(float x, float y, float z)
    {
        if (effectConfig == null || selectedIndex >= effectConfig.attackEffects.Count) return;
        Undo.RecordObject(effectConfig, "Adjust Offset");
        effectConfig.attackEffects[selectedIndex].positionOffset += new Vector3(x, y, z);
        EditorUtility.SetDirty(effectConfig);
    }
    
    void ResetOffset()
    {
        if (effectConfig == null || selectedIndex >= effectConfig.attackEffects.Count) return;
        Undo.RecordObject(effectConfig, "Reset Offset");
        effectConfig.attackEffects[selectedIndex].positionOffset = Vector3.zero;
        EditorUtility.SetDirty(effectConfig);
    }
    
    void AdjustRotation(float x, float y, float z)
    {
        if (effectConfig == null || selectedIndex >= effectConfig.attackEffects.Count) return;
        Undo.RecordObject(effectConfig, "Adjust Rotation");
        effectConfig.attackEffects[selectedIndex].rotationOffset += new Vector3(x, y, z);
        EditorUtility.SetDirty(effectConfig);
    }
    
    void ResetRotation()
    {
        if (effectConfig == null || selectedIndex >= effectConfig.attackEffects.Count) return;
        Undo.RecordObject(effectConfig, "Reset Rotation");
        effectConfig.attackEffects[selectedIndex].rotationOffset = Vector3.zero;
        EditorUtility.SetDirty(effectConfig);
    }
    
    void ClearAllEffects()
    {
        foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
        {
            if (obj.name.Contains("Effect") || obj.name.Contains("VFX") || 
                obj.name.Contains("Particle") || obj.name.Contains("Hit"))
            {
                if (!EditorUtility.IsPersistent(obj)) DestroyImmediate(obj);
            }
        }
        if (lastSpawnedEffect != null) DestroyImmediate(lastSpawnedEffect);
    }
}
#endif
