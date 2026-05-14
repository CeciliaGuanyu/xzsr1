using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间以使用Text组件

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth = 100; // 当前生命值
    public Text healthText; // 用于显示生命值的UI Text组件

    void Start()
    {
        UpdateHealthDisplay(); // 初始化时更新UI显示
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateHealthDisplay(); // 更新UI显示

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth.ToString();
        }
    }

    void Die()
    {
        Debug.Log("Player Died!");
        // 这里可以添加玩家死亡后的逻辑，如禁用玩家对象、显示死亡画面等
        gameObject.SetActive(false); // 简单地禁用玩家对象作为死亡效果
    }
}

