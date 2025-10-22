using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_GameState : BaseUI<UI_GameState>
{
    public Text m_Text_Message; // 消息文本

    public override void InitUI()
    {
        base.InitUI();
        
        // 尝试从nodeDics获取消息文本
        if (nodeDics.ContainsKey("m_Text_Message"))
        {
            m_Text_Message = nodeDics["m_Text_Message"].GetComponent<Text>();
        }
        
        if (m_Text_Message != null)
        {
            m_Text_Message.gameObject.SetActive(false);
        }
    }

    public void ShowGameStart()
    {
        ShowMessage("游戏开始！", Color.green, 3f);
    }

    public void ShowVictory()
    {
        ShowMessage("游戏胜利！", Color.yellow, 5f);
    }

    public void ShowDefeat()
    {
        ShowMessage("游戏失败！", Color.red, 5f);
    }

    private void ShowMessage(string message, Color color, float duration)
    {
        if (m_Text_Message != null)
        {
            m_Text_Message.text = message;
            m_Text_Message.color = color;
            m_Text_Message.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(HideMessageAfterDelay(duration));
        }
        else
        {
            Debug.LogWarning("UI_GameState: m_Text_Message 未找到，无法显示消息");
        }
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (m_Text_Message != null)
        {
            m_Text_Message.gameObject.SetActive(false);
        }
    }
}
