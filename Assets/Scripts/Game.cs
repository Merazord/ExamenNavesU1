using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExamenNavesU1
{
    public class Game : MonoBehaviour
    {
        public static Game instance;
        public int asteroidPoints = 1;
        public int enemyPoints = 3;
        public int score, record;
        public bool finished;
        public bool paused { get; private set; }
        public Player player;
        public AudioSource effects;
        float nextRock, nextEnemy, nextPower;
        string recordPath;
        [Serializable] class RecordData { public int score; }

        void Awake()
        {
            instance = this;
            Time.timeScale = 1;
            AudioListener.pause = false;
            recordPath = Path.Combine(Application.persistentDataPath, "record.json");
            try { if (File.Exists(recordPath)) record = JsonUtility.FromJson<RecordData>(File.ReadAllText(recordPath)).score; }
            catch (Exception e) { Debug.LogWarning("No se pudo leer el record: " + e.Message); }
            effects = gameObject.AddComponent<AudioSource>();
        }

        void Start()
        {
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 5;
            Camera.main.backgroundColor = new Color(.035f, .045f, .09f);
            // El fondo, la nave y la interfaz ya estan guardados en la escena.
            var music = gameObject.AddComponent<AudioSource>();
            music.clip = Resources.Load<AudioClip>("Audio/music");
            music.loop = true; music.volume = .65f; music.Play();
            nextRock = Time.time + 2;
            nextEnemy = Time.time + 5;
            nextPower = Time.time + 7;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
            if (finished || paused) return;
            if (Time.time >= nextRock)
            {
                SpawnObstacle(false);
                nextRock = Time.time + UnityEngine.Random.Range(.95f, 1.5f) * Mathf.Lerp(1f, .7f, score / 200f);
            }
            if (Time.time >= nextEnemy)
            {
                SpawnObstacle(true);
                nextEnemy = Time.time + Mathf.Lerp(3.8f, 2.1f, score / 200f);
            }
            if (Time.time >= nextPower)
            {
                int type = UnityEngine.Random.Range(0, 3);
                var item = CreateSprite("Powerup", "power" + type,
                    new Vector2(UnityEngine.Random.Range(-HalfWidth + 1, HalfWidth - 1), UnityEngine.Random.Range(-3f, 3f)), new Vector2(.6f, .6f));
                Body(item, new Vector2(.6f, .6f));
                item.AddComponent<Pickup>().kind = type;
                nextPower = Time.time + UnityEngine.Random.Range(8f, 12f);
            }
        }

        void SpawnObstacle(bool enemy)
        {
            int direction = UnityEngine.Random.value < .5f ? 1 : -1;
            int type = UnityEngine.Random.Range(0, 2);
            float size = enemy ? .9f : UnityEngine.Random.Range(.65f, 1.3f);
            var obj = CreateSprite(enemy ? "Enemigo" : "Asteroide", enemy ? "enemy" + type : "rock0",
                new Vector2(UnityEngine.Random.Range(-HalfWidth + 1, HalfWidth - 1), -direction * 5.6f), new Vector2(size, size));
            Body(obj, new Vector2(.75f, .75f));
            var obstacle = obj.AddComponent<Obstacle>();
            obstacle.enemy = enemy; obstacle.kind = type; obstacle.direction = direction;
            obstacle.speed = UnityEngine.Random.Range(1.1f, 1.9f) * Mathf.Lerp(1f, 1.25f, score / 200f);
            if (enemy && direction < 0) obj.transform.rotation = Quaternion.Euler(0, 0, 180);
        }

        public static float HalfWidth => Mathf.Min(8.8f, Camera.main.orthographicSize * Camera.main.aspect);

        public static GameObject CreateSprite(string name, string art, Vector2 position, Vector2 size, int order = 0)
        {
            var obj = new GameObject(name);
            obj.transform.position = position;
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = Resources.Load<Sprite>("Art/" + art);
            renderer.sortingOrder = order;
            obj.transform.localScale = new Vector3(size.x / renderer.sprite.bounds.size.x, size.y / renderer.sprite.bounds.size.y, 1);
            return obj;
        }

        public static void Body(GameObject obj, Vector2 size)
        {
            var body = obj.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0;
            var collider = obj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = obj.GetComponent<SpriteRenderer>().sprite.bounds.size * size;
        }

        public void Sound(string name) { effects.PlayOneShot(Resources.Load<AudioClip>("Audio/" + name), .45f); }

        public void AddScore(int points)
        {
            if (finished) return;
            score += points;
            if (score > record)
            {
                record = score;
                try { File.WriteAllText(recordPath, JsonUtility.ToJson(new RecordData { score = record })); }
                catch (Exception e) { Debug.LogWarning("No se pudo guardar el record: " + e.Message); }
            }
            if (score >= 200) End(true);
        }

        public void End(bool won)
        {
            if (finished) return;
            finished = true;
            GetComponent<Hud>().won = won;
            Sound(won ? "win" : "lose");
        }
        public void TogglePause()
        {
            if (finished) return;
            SetPaused(!paused);
        }
        void SetPaused(bool value)
        {
            paused = value;
            Time.timeScale = value ? 0 : 1;
            AudioListener.pause = value;
            var hud = GetComponent<Hud>();
            hud.pausePanel.SetActive(value);
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(value ? hud.continueButton.gameObject : null);
        }
        public void Continue() { SetPaused(false); }
        public void Restart()
        {
            SetPaused(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        public void Quit()
        {
            SetPaused(false);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        void OnDestroy()
        {
            if (instance != this) return;
            Time.timeScale = 1;
            AudioListener.pause = false;
            instance = null;
        }
    }
}
