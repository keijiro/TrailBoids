using UnityEngine;
using UnityEditor;

namespace TrailBoids
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BoidController))]
    public class BoidControllerEditor : Editor
    {
        SerializedProperty _spawnCount;
        SerializedProperty _spawnRadius;

        SerializedProperty _velocity;
        SerializedProperty _velocityVariance;
        SerializedProperty _scroll;

        SerializedProperty _rotationSpeed;
        SerializedProperty _neighborDistance;

        static class Styles
        {
            public static readonly GUIContent variance = new GUIContent("Variance");
        }

        void OnEnable()
        {
            _spawnCount = serializedObject.FindProperty("_spawnCount");
            _spawnRadius = serializedObject.FindProperty("_spawnRadius");

            _velocity = serializedObject.FindProperty("_velocity");
            _velocityVariance = serializedObject.FindProperty("_velocityVariance");
            _scroll = serializedObject.FindProperty("_scroll");

            _rotationSpeed = serializedObject.FindProperty("_rotationSpeed");
            _neighborDistance = serializedObject.FindProperty("_neighborDistance");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_spawnCount);
            EditorGUILayout.PropertyField(_spawnRadius);

            EditorGUILayout.PropertyField(_velocity);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_velocityVariance, Styles.variance);
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(_scroll);

            EditorGUILayout.PropertyField(_rotationSpeed);
            EditorGUILayout.PropertyField(_neighborDistance);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
