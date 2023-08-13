using System;
using Editor;
using UnityEditor;
using UnityEngine;
using Voxels.Materials;

namespace Voxels.Simulation.Erosion.Materials.Editor
{
    [CustomEditor(typeof(MaterialPropertiesConfig))]
    public class MaterialPropertiesConfigEditor : BaseEditor
    {
        private static bool m_materialFoldout = true;

        private SerializedProperty materialInfos;

        private void OnEnable()
        {
            materialInfos = serializedObject.FindProperty(nameof(materialInfos));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DisplayScriptHeader();

            if (m_materialFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(m_materialFoldout, ObjectNames.NicifyVariableName(nameof(materialInfos))))
            {
                EditorGUI.indentLevel++;
                int materialCount = Enum.GetNames(typeof(MaterialIndex)).Length;

                if (materialInfos.arraySize != materialCount)
                {
                    materialInfos.arraySize = materialCount;
                }

                for (int index = 0; index < materialInfos.arraySize; index++)
                {
                    EditorGUILayout.PropertyField(materialInfos.GetArrayElementAtIndex(index), new GUIContent($"{(MaterialIndex)index}"));
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}