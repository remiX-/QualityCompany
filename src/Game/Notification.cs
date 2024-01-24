namespace AdvancedCompany.Game;

internal class Notification
{
    public static void Display(string message)
    {
        HUDManager.Instance.globalNotificationAnimator.SetTrigger("TriggerNotif");
        HUDManager.Instance.globalNotificationText.text = message;
        HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.globalNotificationSFX);
    }
}