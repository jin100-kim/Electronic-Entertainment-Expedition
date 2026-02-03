using UnityEngine;

public class GameHUD : MonoBehaviour
{
    [SerializeField]
    private Vector2 position = new Vector2(12f, 12f);

    private void OnGUI()
    {
        var session = GameSession.Instance;
        if (session == null)
        {
            return;
        }

        var style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;
        style.normal.textColor = Color.white;

        float hp = session.PlayerHealth != null ? session.PlayerHealth.CurrentHealth : 0f;
        float maxHp = session.PlayerHealth != null ? session.PlayerHealth.MaxHealth : 0f;
        int level = session.PlayerExperience != null ? session.PlayerExperience.Level : 1;
        float xp = session.PlayerExperience != null ? session.PlayerExperience.CurrentXp : 0f;
        float xpNext = session.PlayerExperience != null ? session.PlayerExperience.XpToNext : 0f;

        string text = $"HP: {hp:0}/{maxHp:0}   LV: {level}   XP: {xp:0.0}/{xpNext:0.0}   Time: {session.ElapsedTime:0.0}";
        if (session.IsGameOver)
        {
            text += "   GAME OVER";
        }

        GUI.Label(new Rect(position.x, position.y, 520f, 30f), text, style);
    }
}
