using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallaxer : MonoBehaviour
{
    class PoolObject {
        public Transform transform; // the object
        public bool inUse;          // determine whether the object is in use or not
        public PoolObject(Transform t) { transform = t; }   // constructor
        public void Use() { inUse = true; }
        public void Dispose() { inUse = false; }
    }

    [System.Serializable]
    public struct YSpawnRange {     // range for the pipe to spawn
        public float min;
        public float max;
    }

    public GameObject Prefab;   // type of prefab to be spawn
    public int poolSize;        // how many objects to be spawn
    public float shiftSpeed;    // moving speed
    public float spawnRate;     // how often does the object spawn

    public YSpawnRange ySpawnRange;
    public Vector3 defaultSpawnPos;     // default spawn position
    public bool spawnImmediate;         // particle prewarm // whether to spawn immediately
    public Vector3 immediateSpawnPos;   // position for spawn immediate
    public Vector2 targetAspectRatio;   // make sure pipes are spawn within the screen space

    float spawnTimer;
    float targetAspect; 
    PoolObject[] poolObjects;
    GameManager game;

    void Awake() {
        Configure();
    }

    void Start() {
        game = GameManager.instance;
    }

    void OnEnable() {
        GameManager.OnGameOverConfirmed += OnGameOverConfirmed;
    }

    void OnDisable() {
        GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;
    }

    void OnGameOverConfirmed() {
        for (int i = 0; i < poolObjects.Length; i++) {
            poolObjects[i].Dispose();
            poolObjects[i].transform.localPosition = Vector3.one * 1000;

        }
        if (spawnImmediate) {
            SpawnImmediate();
        }
    }

    void Update() {
        if (game.GameOver) return;

        Shift();
        spawnTimer += Time.deltaTime;
        if (spawnTimer > spawnRate) {
            Spawn();
            spawnTimer = 0;
        }
    }

    void Configure() {
        targetAspect = targetAspectRatio.x / targetAspectRatio.y;
        poolObjects = new PoolObject[poolSize];
        for (int i = 0; i < poolObjects.Length; i++) {
            GameObject go = Instantiate(Prefab) as GameObject;
            Transform t = go.transform;
            t.SetParent(transform);
            t.position = Vector3.one * 1000;
            poolObjects[i] = new PoolObject(t);
        }

        if (spawnImmediate) {
            SpawnImmediate();
        }
    }

    void Spawn() {
        Transform t = GetPoolObject();
        if (t == null) return;  // if true, this indicates that poolSize is too small
        Vector3 pos= Vector3.zero;
        pos.x = (defaultSpawnPos.x * Camera.main.aspect) / targetAspect;
        pos.z = defaultSpawnPos.z;
        pos.y = Random.Range(ySpawnRange.min, ySpawnRange.max);
        t.position = pos;
    }

    void SpawnImmediate() {
        Transform t = GetPoolObject();
        if (t == null) return;  // if true, this indicates that poolSize is too small
        Vector3 pos= Vector3.zero;
        pos.x = (immediateSpawnPos.x * Camera.main.aspect) / targetAspect;
        pos.z = immediateSpawnPos.z;
        pos.y = Random.Range(ySpawnRange.min, ySpawnRange.max);
        t.position = pos;
        Spawn();
    }

    void Shift() {
        for (int i = 0; i < poolObjects.Length; i++) {
            poolObjects[i].transform.localPosition += -Vector3.right * shiftSpeed * Time.deltaTime;
            CheckDisposeObject(poolObjects[i]);
        }
    }

    void CheckDisposeObject(PoolObject poolObject) {
        if (poolObject.transform.localPosition.x < (-defaultSpawnPos.x * Camera.main.aspect) / targetAspect) {
            poolObject.Dispose();
            poolObject.transform.localPosition = Vector3.one * 1000;
        }
    }

    Transform GetPoolObject() {
        for (int i = 0; i < poolObjects.Length; i++) {
            if (!poolObjects[i].inUse) {
                poolObjects[i].Use();
                return poolObjects[i].transform;
            }
        }
        return null;
    }
}
