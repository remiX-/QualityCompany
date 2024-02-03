namespace QualityCompany.Utils;

public class HudUtils
{
    public static void DisplayNotification(string message)
    {
        HUDManager.Instance.globalNotificationAnimator.SetTrigger("TriggerNotif");
        HUDManager.Instance.globalNotificationText.text = message;
        HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.globalNotificationSFX);
    }
}