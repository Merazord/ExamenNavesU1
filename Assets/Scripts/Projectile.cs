using UnityEngine;

namespace ExamenNavesU1
{
    public class Projectile : MonoBehaviour
    {
        public bool enemy;
        public Vector2 velocity;
        public static Projectile Fire(Vector2 position, Vector2 direction, bool enemy, int kind)
        {
            string art = (enemy ? "enemybullet" : "bullet") + kind;
            var obj = Game.CreateSprite("Bala", art, position, kind == 1 ? new Vector2(.23f, .38f) : new Vector2(.15f, .32f), 2);
            obj.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90);
            Game.Body(obj, Vector2.one);
            var projectile = obj.AddComponent<Projectile>();
            projectile.enemy = enemy; projectile.velocity = direction.normalized * (enemy ? 3.6f : 8);
            Destroy(obj, 6); return projectile;
        }
        void FixedUpdate()
        {
            if (!Game.instance.finished) GetComponent<Rigidbody2D>().MovePosition(GetComponent<Rigidbody2D>().position + velocity * Time.fixedDeltaTime);
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (Game.instance.finished) return;
            var shot = other.GetComponent<Projectile>();
            if (shot != null && shot.enemy != enemy)
            {
                // Solo la bala del jugador procesa esta colision.
                if (!enemy) { Game.instance.Sound("cancel"); Destroy(other.gameObject); Destroy(gameObject); }
                return;
            }
            var obstacle = other.GetComponent<Obstacle>();
            if (!enemy && obstacle != null) { obstacle.Damage(); Destroy(gameObject); }
        }
    }
}
