using UnityEditor;

namespace SeweralIdeas.BuildAndUpload.Editor
{
    /// <summary>
    /// The settings of this machine of type <typeparamref name="T"/> (a serializable class), kept as one JSON in EditorPrefs:
    /// loaded on first use, and written back by <see cref="Save"/>. Fields the saved JSON doesn't have keep the defaults of the class.
    /// </summary>
    public static class MachineSettingsStore<T> where T : class, new()
    {
        private static T s_instance;

        private static string PrefsKey => "SeweralIdeas.BuildAndUpload.Machine." + typeof(T).FullName;

        public static T Instance
        {
            get
            {
                if(s_instance == null)
                {
                    s_instance = new T();
                    string json = EditorPrefs.GetString(PrefsKey, "");
                    if(json.Length > 0)
                        EditorJsonUtility.FromJsonOverwrite(json, s_instance);
                }
                return s_instance;
            }
        }

        public static void Save() => EditorPrefs.SetString(PrefsKey, EditorJsonUtility.ToJson(Instance));
    }
}
