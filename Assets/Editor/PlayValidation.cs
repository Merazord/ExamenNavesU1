using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ExamenNavesU1
{
    [InitializeOnLoad]
    public static class PlayValidation
    {
        static PlayValidation() { EditorApplication.playModeStateChanged += Changed; }
        public static void Run()
        {
            ProjectSetup.Validate();
            SessionState.SetBool("ExamenValidation", true);
            EditorSceneManager.OpenScene("Assets/Scenes/ExamenNavesU1.unity");
            EditorApplication.isPlaying = true;
        }
        static void Changed(PlayModeStateChange state)
        {
            if (!SessionState.GetBool("ExamenValidation", false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
                new GameObject("Validation").AddComponent<RuntimeValidation>();
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetBool("ExamenValidation", false);
                EditorApplication.Exit(SessionState.GetBool("ExamenPassed", false) ? 0 : 1);
            }
        }
    }

    public class RuntimeValidation : MonoBehaviour
    {
        string recordPath, previousRecord;
        bool hadRecord;
        void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
            Debug.Log("CHECK_OK: " + message);
        }
        IEnumerator Start()
        {
            yield return null;
            recordPath = Path.Combine(Application.persistentDataPath, "record.json");
            hadRecord = File.Exists(recordPath);
            if (hadRecord) previousRecord = File.ReadAllText(recordPath);
            var routine = Checks();
            bool passed = true;
            while (true)
            {
                object next = null;
                try { if (!routine.MoveNext()) break; next = routine.Current; }
                catch (Exception error) { Debug.LogError("EXAMEN_TEST_FAILED: " + error); passed = false; break; }
                yield return next;
            }
            if (hadRecord) File.WriteAllText(recordPath, previousRecord);
            else if (File.Exists(recordPath)) File.Delete(recordPath);
            SessionState.SetBool("ExamenPassed", passed);
            Debug.Log(passed ? "EXAMEN_PLAY_OK" : "EXAMEN_PLAY_FAILED");
            EditorApplication.isPlaying = false;
        }
        IEnumerator Checks()
        {
            var game = Game.instance;
            game.enabled = false;
            var player = game.player;
            Check(player.lives == 5 && player.weapon == 0 && player.transform.position.sqrMagnitude < .01f, "Inicio centrado con cinco vidas y arma normal");
            Check(UnityEngine.Object.FindObjectsByType<Player>(FindObjectsSortMode.None).Length == 1, "Una sola nave al iniciar Play");
            Check(game.GetComponent<Hud>().pointsText != null && !game.GetComponent<Hud>().resultPanel.activeSelf, "Interfaz guardada y fin de partida oculto");
            Check(Mathf.Abs(player.transform.eulerAngles.z) < 1, "Nave empieza mirando hacia arriba");
            yield return new WaitForSeconds(.2f);
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Validation/gameplay.png"));
            for (int kind = 0; kind < 3; kind++)
            {
                if (kind == 2) player.lives = 2;
                var obj = Game.CreateSprite("Test pickup", "power" + kind, player.transform.position, Vector2.one * .6f);
                Game.Body(obj, Vector2.one);
                obj.AddComponent<Pickup>().kind = kind;
                yield return new WaitForSeconds(.08f);
                Check(kind == 2 ? player.lives == 5 : player.weapon == kind + 1, "Powerup " + kind);
            }
            var hostile = Projectile.Fire(player.transform.position, Vector2.up, true, 0);
            hostile.velocity = Vector2.zero;
            yield return new WaitForSeconds(.08f);
            Check(player.lives == 4 && player.weapon == 0 && hostile == null, "Impacto quita vida, destruye bala y restablece arma normal");
            for (int type = 0; type < 2; type++)
            {
                var rock = Game.CreateSprite("Test obstacle", type == 0 ? "rock0" : "enemy0", new Vector2(3, 2), Vector2.one);
                Game.Body(rock, Vector2.one);
                var obstacle = rock.AddComponent<Obstacle>();
                obstacle.enemy = type == 1; obstacle.direction = 1; obstacle.speed = 0;
                int count = type == 0 ? 3 : 5;
                for (int hit = 0; hit < count; hit++)
                {
                    var shot = Projectile.Fire(rock.transform.position, Vector2.up, false, 0); shot.velocity = Vector2.zero;
                    yield return new WaitForSeconds(.08f);
                    if (hit < count - 1) Check(obstacle != null && obstacle.hits == hit + 1, "Impactos necesarios " + type + ": " + (hit + 1));
                    if (type == 0 && hit < 2) Check(rock.GetComponent<SpriteRenderer>().sprite == Resources.Load<Sprite>("Art/rock" + (hit + 1)), "Cambio de sprite de asteroide");
                }
                Check(rock == null, "Destruccion tras " + count + " disparos");
            }
            Check(game.score == 4, "Puntuacion: asteroide 1 y enemigo 3");
            var a = Projectile.Fire(new Vector2(-3, 2), Vector2.up, false, 0);
            var b = Projectile.Fire(new Vector2(-3, 2), Vector2.down, true, 1);
            a.velocity = b.velocity = Vector2.zero;
            yield return new WaitForSeconds(.08f);
            Check(a == null && b == null, "Balas opuestas se cancelan");
            game.record = 0;
            game.AddScore(196);
            Check(game.score == 200 && game.finished && game.GetComponent<Hud>().won, "Victoria al llegar a 200");
            Check(File.Exists(recordPath) && File.ReadAllText(recordPath).Contains("200"), "Record escrito en archivo persistente");
            game.finished = false;
            game.End(false);
            Check(game.finished && !game.GetComponent<Hud>().won, "Pantalla de derrota");
            yield return new WaitForSeconds(.2f);
        }
    }
}
