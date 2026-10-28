using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SeweralIdeas.BuildAndUpload.Editor
{
    [CustomEditor(typeof(UploadProfile))]
    public class UploadProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_buildProfile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_excludeAlways"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_excludeFromUpload"));

            // [SerializeReference] draws the fields of the object but has no way to choose its type.
            SerializedProperty uploader = serializedObject.FindProperty("_uploader");
            Type[] types = TypeCache.GetTypesDerivedFrom<BuildUploader>().Where(type => !type.IsAbstract).ToArray();
            string[] names = types.Select(type => ObjectNames.NicifyVariableName(type.Name)).ToArray();

            Type current = uploader.managedReferenceValue?.GetType();
            int index = Array.IndexOf(types, current);
            int chosen = EditorGUILayout.Popup("Uploader", index, names);
            if(chosen != index && chosen >= 0)
                uploader.managedReferenceValue = Activator.CreateInstance(types[chosen]);

            if(uploader.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(uploader, GUIContent.none, true);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();

            // Not a part of the asset: what is kept for this machine only, so it is set where the uploader is.
            BuildUploader uploaderObject = ((UploadProfile)target).Uploader;
            if(uploaderObject != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("This machine", EditorStyles.boldLabel);
                uploaderObject.DrawMachineSettings();
            }
        }
    }
}
