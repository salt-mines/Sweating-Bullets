using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class JoinEvent : UnityEvent<string>
{
}

public class ServerSelection : MonoBehaviour
{
    public TMP_InputField inputField;

    public JoinEvent onJoin;

    private void Start()
    {
        if (onJoin == null)
            onJoin = new JoinEvent();
    }

    public void OnJoin()
    {
        onJoin.Invoke(inputField.text);
    }

    public void OnCancel()
    {
        Destroy(gameObject);
    }
}
