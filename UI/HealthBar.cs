using UnityEngine;

public class HealthBar : MonoBehaviour
{
    #region Header GameObject References

    [Space(10)]
    [Header("GameObject References")]

    #endregion Header GameObject References

    #region Tooltip

    [Tooltip("Populate with the child Bar gameobject ")]

    #endregion Tooltip

    [SerializeField] private GameObject healthBar;

    //Enable the health bar
    public void EnableHealthBar()
    {
        gameObject.SetActive(true);
    }

    //Disable the health bar
    public void DisableHealthBar()
    {
        gameObject.SetActive(false);
    }

    //Set health bar value with health percent between 0 and 1
    public void SetHealthBarValue(float healthPercent)
    {
        healthBar.transform.localScale = new Vector3(healthPercent, 1f, 1f);
    }
}
