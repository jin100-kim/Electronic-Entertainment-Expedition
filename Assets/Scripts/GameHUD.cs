using UnityEngine;

public class GameHUD : MonoBehaviour
{
    [SerializeField]
    private Vector2 position = new Vector2(12f, 12f);

    private GUIStyle _labelStyle;
    private GUIStyle _smallStyle;

    private void OnGUI()
    {
        var session = GameSession.Instance;
        if (session == null)
        {
            return;
        }

        if (_labelStyle == null || _smallStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = 16;
            _labelStyle.normal.textColor = Color.white;

            _smallStyle = new GUIStyle(GUI.skin.label);
            _smallStyle.fontSize = 12;
            _smallStyle.normal.textColor = new Color(1f, 1f, 1f, 0.9f);
        }

        float hp = session.PlayerHealth != null ? session.PlayerHealth.CurrentHealth : 0f;
        float maxHp = session.PlayerHealth != null ? session.PlayerHealth.MaxHealth : 0f;
        int level = session.PlayerExperience != null ? session.PlayerExperience.Level : 1;
        float xp = session.PlayerExperience != null ? session.PlayerExperience.CurrentXp : 0f;
        float xpNext = session.PlayerExperience != null ? session.PlayerExperience.XpToNext : 0f;

        string info = $"HP: {hp:0}/{maxHp:0}   XP: {xp:0.0}/{xpNext:0.0}   LV: {level}   몬스터 LV: {session.MonsterLevel}   Time: {session.ElapsedTime:0.0}";
        if (session.IsGameOver)
        {
            info += "   GAME OVER";
        }
        GUI.Label(new Rect(position.x, position.y, 900f, 22f), info, _labelStyle);
    }
}
