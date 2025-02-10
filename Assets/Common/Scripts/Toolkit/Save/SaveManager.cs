using System.Collections;
using UnityEngine;
using System.Threading;
using UnityEngine.Events;
using OctoberStudio.Easing;
using System.IO;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace OctoberStudio.Save
{
    public class SaveManager : MonoBehaviour
    {
        public static readonly string SAVE_FILE_NAME = "game_save";

        private static SaveManager instance;

        [Space]
        [SerializeField] bool clearSave;

        [Space]
        [SerializeField] bool autoSaveEnabled;
        [SerializeField] float autoSaveDelay;

        private SaveDatabase SaveDatabase { get; set; }

        public bool IsSaveLoaded { get; private set; }

        public event UnityAction OnSaveLoaded;

        private Coroutine saveCoroutine;

        public void Init()
        {
            if(instance != null)
            {
                Destroy(this);

                return;
            }

            instance = this;

            DontDestroyOnLoad(gameObject);

            if (clearSave)
            {
                InitClear();
            }
            else
            {
                Load();
            }

            if (autoSaveEnabled)
            {
                StartCoroutine(AutoSaveCoroutine());
            }
        }

        public T GetSave<T>(int hash) where T : ISave, new()
        {
            if (!IsSaveLoaded)
            {
                Debug.LogError("Save file has not been loaded yet");
                return default;
            }

            return SaveDatabase.GetSave<T>(hash);
        }

        public T GetSave<T>(string uniqueName) where T : ISave, new()
        {
            return GetSave<T>(uniqueName.GetHashCode());
        }

        private void InitClear()
        {
            SaveDatabase = new SaveDatabase();
            SaveDatabase.Init();

            Debug.Log("New save file is created");

            IsSaveLoaded = true;
        }

        private void Load()
        {
            if (IsSaveLoaded)
                return;

            // Try to read and deserialize file or create new one
            SaveDatabase = LoadSave();

            SaveDatabase.Init();

            Debug.Log("Save file is loaded");

            IsSaveLoaded = true;

            OnSaveLoaded?.Invoke();
        }

        private SaveDatabase LoadSave()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string jsonObject = load(SAVE_FILE_NAME);
            if(!string.IsNullOrEmpty(jsonObject))
            {
                try
                {
                    SaveDatabase deserializedObject = JsonUtility.FromJson<SaveDatabase>(jsonObject);

                    return deserializedObject;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }

            return new SaveDatabase();
#else
            return SerializationHelper.DeserializePersistent<SaveDatabase>(SAVE_FILE_NAME, useLogs: false);
#endif
        }

        private IEnumerator SaveCoroutine(bool multithreading = false)
        {
            var wait = new WaitForSeconds(0.2f);
            while (SerializationHelper.IsFileLocked(SAVE_FILE_NAME))
            {
                yield return wait;
            }
            if (multithreading)
            {
                var saveThread = new Thread(() => {
                    SerializationHelper.SerializePersistent(SaveDatabase, SAVE_FILE_NAME);
                });
                saveThread.Start();
            }
            else
            {
                SerializationHelper.SerializePersistent(SaveDatabase, SAVE_FILE_NAME);
            }

            Debug.Log("Save file is updated");

            saveCoroutine = null;
        }

        private void ForceSave()
        {
            if (SaveDatabase == null) return;
            SaveDatabase.Flush();

#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLSave(SaveDatabase, SAVE_FILE_NAME);
            Debug.Log("Save file is updated");
#else
            if (!SerializationHelper.IsFileLocked(SAVE_FILE_NAME))
            {
                SerializationHelper.SerializePersistent(SaveDatabase, SAVE_FILE_NAME);

                Debug.Log("Save file is updated");
            }
#endif
        }

        public void Save(bool multithreading = false)
        {
            if (SaveDatabase == null) return;
            SaveDatabase.Flush();

#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLSave(SaveDatabase, SAVE_FILE_NAME);
            Debug.Log("Save file is updated");
#else
            if (saveCoroutine == null) saveCoroutine = StartCoroutine(SaveCoroutine(multithreading));
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private void WebGLSave(SaveDatabase saveDatabase, string fileName)
        {
            string jsonObject = JsonUtility.ToJson(saveDatabase);

            save(fileName, jsonObject);
        }
#endif

        private IEnumerator AutoSaveCoroutine()
        {
            var wait = new WaitForSecondsRealtime(autoSaveDelay);

            while (true)
            {
                yield return wait;

                Save(true);
            }
        }

        public static void DeleteSaveFile()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            deleteItem(SAVE_FILE_NAME);
#else
            SerializationHelper.DeletePersistent(SAVE_FILE_NAME);
#endif

            Debug.Log("Save file is deleted!");
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            ForceSave();
#endif
        }

        private void OnDisable()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ForceSave();
#endif
        }

        private void OnApplicationFocus(bool focus)
        {
#if !UNITY_EDITOR
            if(!focus) ForceSave();
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string load(string keyName);

        [DllImport("__Internal")]
        private static extern void save(string keyName, string data);

        [DllImport("__Internal")]
        private static extern void deleteItem(string keyName);
#endif
    }
}