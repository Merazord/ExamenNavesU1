using UnityEngine;

namespace ExamenNavesU1
{
    public class Obstacle : MonoBehaviour
    {
        public bool enemy;
        public int kind, direction;
        public float speed;
        public int hits;
        float startX, born, nextShot;
        void Start() { startX = transform.position.x; born = Time.time; nextShot = Time.time + 1.2f; }
        void FixedUpdate()
        {
            if (Game.instance.finished) return;
            var body = GetComponent<Rigidbody2D>();
            Vector2 pos = body.position;
            pos.y += direction * speed * Time.fixedDeltaTime;
            if (enemy)
                pos.x = Mathf.Clamp(startX + (kind == 0 ? Mathf.Sin((Time.time - born) * 2.5f) : Mathf.PingPong((Time.time - born) * 1.2f, 2) - 1), -Game.HalfWidth + .5f, Game.HalfWidth - .5f);
            else transform.Rotate(0, 0, 35 * Time.fixedDeltaTime);
            body.MovePosition(pos);
            if (Mathf.Abs(pos.y) > 6.5f) Destroy(gameObject);
            if (enemy && Mathf.Abs(pos.y) < 4.5f && Time.time >= nextShot)
            {
                Vector2 aim = kind == 0 ? new Vector2(0, direction) : (Vector2)(Game.instance.player.transform.position - transform.position);
                Projectile.Fire(pos + aim.normalized * .65f, aim, true, kind);
                nextShot = Time.time + (kind == 0 ? 1.5f : 1.9f) * Mathf.Lerp(1f, .8f, Game.instance.score / 200f);
            }
        }
        public void Damage()
        {
            if (hits >= (enemy ? 5 : 3)) return;
            hits++;
            Game.instance.Sound(enemy ? "metal" : "rock");
            if (hits >= (enemy ? 5 : 3))
            {
                Game.instance.Sound("explosion");
                Game.instance.AddScore(enemy ? Game.instance.enemyPoints : Game.instance.asteroidPoints);
                Destroy(gameObject);
            }
            else if (!enemy) GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Art/rock" + hits);
        }
    }
}
