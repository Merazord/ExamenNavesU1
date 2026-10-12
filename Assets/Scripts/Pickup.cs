using UnityEngine;
namespace ExamenNavesU1
{
    public class Pickup : MonoBehaviour
    {
        public int kind;
        void Start() { Destroy(gameObject, 12); }
    }
}
