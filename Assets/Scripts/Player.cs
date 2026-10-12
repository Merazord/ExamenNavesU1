using UnityEngine;

namespace ExamenNavesU1
{
    public class Player : MonoBehaviour
    {
        public int lives = 5;
        public int weapon;
        Vector2 movement, lastMouse;
        float nextShot, immuneUntil;
        bool aiming;

        void Start() { lastMouse = Input.mousePosition; }
        void Update()
        {
            if (Game.instance.finished || Game.instance.paused) return;
            Vector2 mouse = Input.mousePosition;
            if (Vector2.Distance(mouse, lastMouse) > 1) aiming = true;
            lastMouse = mouse;
            if (aiming)
            {
                Vector2 direction = Camera.main.ScreenToWorldPoint(mouse) - transform.position;
                if (direction.sqrMagnitude > .01f)
                    transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90);
            }
            movement = new Vector2(
                (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0),
                (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0)).normalized;
            if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space)) && Time.time >= nextShot)
            {
                Projectile.Fire(transform.position + transform.up * .6f, transform.up, false, weapon == 2 ? 1 : 0);
                if (weapon == 1)
                {
                    Projectile.Fire(transform.position + transform.up * .6f, Quaternion.Euler(0, 0, 14) * transform.up, false, 0);
                    Projectile.Fire(transform.position + transform.up * .6f, Quaternion.Euler(0, 0, -14) * transform.up, false, 0);
                }
                Game.instance.Sound(weapon == 2 ? "special" : "shot");
                nextShot = Time.time + .23f;
            }
            GetComponent<SpriteRenderer>().color = Time.time < immuneUntil && Mathf.Sin(Time.time * 25) > 0 ? new Color(1, 1, 1, .35f) : Color.white;
        }

        void FixedUpdate()
        {
            if (Game.instance.finished || Game.instance.paused) return;
            Vector2 pos = GetComponent<Rigidbody2D>().position + movement * 4.5f * Time.fixedDeltaTime;
            pos.x = Mathf.Clamp(pos.x, -Game.HalfWidth + .45f, Game.HalfWidth - .45f);
            pos.y = Mathf.Clamp(pos.y, -4.35f, 3.9f);
            GetComponent<Rigidbody2D>().MovePosition(pos);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (Game.instance.finished || Game.instance.paused) return;
            var pickup = other.GetComponent<Pickup>();
            if (pickup != null)
            {
                if (pickup.kind == 2) lives = 5;
                else weapon = pickup.kind + 1;
                Game.instance.Sound("pickup"); Destroy(other.gameObject); return;
            }
            var bullet = other.GetComponent<Projectile>();
            if (other.GetComponent<Obstacle>() == null && (bullet == null || !bullet.enemy)) return;
            Game.instance.Sound("hit");
            Destroy(other.gameObject);
            if (Time.time < immuneUntil) return;
            lives--; weapon = 0; immuneUntil = Time.time + 1.5f;
            if (lives <= 0) { Game.instance.End(false); gameObject.SetActive(false); }
        }
    }
}
