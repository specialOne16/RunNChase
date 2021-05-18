using UnityEngine;

public class PlayfabPersist : MonoBehaviour
{
    public static PlayfabPersist instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // Transfer playerData to the new instance
            var oldLoginManager = instance.gameObject.GetComponent<PlayfabLoginRegister>();
            var newLoginManager = gameObject.GetComponent<PlayfabLoginRegister>();
            newLoginManager.playerData = oldLoginManager.playerData;
            newLoginManager.hasLogin = oldLoginManager.hasLogin;

            Destroy(instance.gameObject);
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }
}
