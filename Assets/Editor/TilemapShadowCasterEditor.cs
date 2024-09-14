using System.Linq;
using UnityEditor;
using UnityEngine;

[UnityEditor.CustomEditor(typeof(TilemapShadowCaster))]
public class TilemapShadowCasterEditor : UnityEditor.Editor
{
    private SerializedProperty m_Tilemap;
    private SerializedProperty m_CompositeCollider;
    private SerializedProperty m_UseRendererSilhouette;
    private SerializedProperty m_CastsShadows;
    private SerializedProperty m_SelfShadows;
    private SerializedProperty m_ApplyToSortingLayers;

    private string[] m_SortingLayerNames;
    private int[] m_SortingLayerIDs;

    private void OnEnable()
    {
        m_Tilemap = serializedObject.FindProperty("tilemap");
        m_CompositeCollider = serializedObject.FindProperty("compositeCollider");
        m_UseRendererSilhouette = serializedObject.FindProperty("m_UseRendererSilhouette");
        m_CastsShadows = serializedObject.FindProperty("m_CastsShadows");
        m_SelfShadows = serializedObject.FindProperty("m_SelfShadows");
        m_ApplyToSortingLayers = serializedObject.FindProperty("m_ApplyToSortingLayers");

        m_SortingLayerNames = SortingLayer.layers.Select(l => l.name).ToArray();
        m_SortingLayerIDs = SortingLayer.layers.Select(l => l.id).ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_Tilemap);
        EditorGUILayout.PropertyField(m_CompositeCollider);
        EditorGUILayout.PropertyField(m_UseRendererSilhouette, new GUIContent("Use Renderer Silhouette"));
        EditorGUILayout.PropertyField(m_CastsShadows, new GUIContent("Casts Shadows"));
        EditorGUILayout.PropertyField(m_SelfShadows, new GUIContent("Self Shadows"));

        EditorGUILayout.LabelField("Target Sorting Layers");
        int mask = 0;
        for (int i = 0; i < m_ApplyToSortingLayers.arraySize; i++)
        {
            int layerID = m_ApplyToSortingLayers.GetArrayElementAtIndex(i).intValue;
            int index = System.Array.IndexOf(m_SortingLayerIDs, layerID);
            if (index >= 0)
            {
                mask |= 1 << index;
            }
        }

        mask = EditorGUILayout.MaskField("Sorting Layers", mask, m_SortingLayerNames);

        m_ApplyToSortingLayers.ClearArray();
        for (int i = 0; i < m_SortingLayerIDs.Length; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                m_ApplyToSortingLayers.InsertArrayElementAtIndex(m_ApplyToSortingLayers.arraySize);
                m_ApplyToSortingLayers.GetArrayElementAtIndex(m_ApplyToSortingLayers.arraySize - 1).intValue = m_SortingLayerIDs[i];
            }
        }

        if (GUILayout.Button("Update Shadow Casters"))
        {
            TilemapShadowCaster shadowCaster = (TilemapShadowCaster)target;
            shadowCaster.UpdateShadowCasters();
        }

        serializedObject.ApplyModifiedProperties();
    }
}