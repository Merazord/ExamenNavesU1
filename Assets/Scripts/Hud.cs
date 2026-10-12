using UnityEngine;
using UnityEngine.UI;
namespace ExamenNavesU1
{
    public class Hud : MonoBehaviour
    {
        public bool won;
        public Text pointsText, recordText, weaponText;
        public Image[] hearts;
        public GameObject resultPanel;
        public GameObject pausePanel;
        public Button continueButton;
        public Text resultTitle, resultText;
        readonly string[] weapons = { "NORMAL", "MULTIPLE", "ADICIONAL" };
        void Update()
        {
            var game = Game.instance;
            if (game == null || game.player == null) return;
            pointsText.text = "PUNTOS: " + game.score + " / 200";
            recordText.text = "RECORD: " + game.record;
            weaponText.text = "ARMA: " + weapons[game.player.weapon];
            for (int i = 0; i < hearts.Length; i++)
                hearts[i].color = i < game.player.lives ? Color.white : new Color(.25f, .25f, .3f);
            resultPanel.SetActive(game.finished);
            if (!game.finished) return;
            resultTitle.text = won ? "META CUMPLIDA" : "FIN DE PARTIDA";
            resultText.text = won ? "Llegaste a los 200 puntos." : "Puntos obtenidos: " + game.score;
        }
    }
}
