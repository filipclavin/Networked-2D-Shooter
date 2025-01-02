using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class Chat : NetworkBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private RectTransform chatLog;
    [SerializeField] private int maxMessagesToDisplay = 10;
    [SerializeField] private InputActionAsset controls;

    private List<TextMeshProUGUI> messages = new();

    private void OnEnable()
    {
        InputActionMap actionMap = controls.FindActionMap("Player");
        actionMap.FindAction("Chat").performed += ctx => inputField.ActivateInputField();

        inputField.onSubmit.AddListener(OnSubmit);
        inputField.onSelect.AddListener(ctx => actionMap.Disable());
        inputField.onEndEdit.AddListener(ctx => actionMap.Enable());
        inputField.onDeselect.AddListener(ctx => actionMap.Enable());
    }

    private void OnSubmit(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        SendMessageRpc(message);

        inputField.text = string.Empty;
    }

    [Rpc(SendTo.Everyone)]
    private void SendMessageRpc(string message, RpcParams rpcParams = default)
    {
        bool fromSelf = rpcParams.Receive.SenderClientId == NetworkManager.Singleton.LocalClientId;

        messages.Add(CreateMessage(message, fromSelf));
        if (messages.Count > maxMessagesToDisplay)
        {
            Destroy(messages[0].gameObject);
            messages.RemoveAt(0);
        }

        UpdateChatLog();
    }

    private TextMeshProUGUI CreateMessage(string message, bool fromSelf)
    {
        GameObject messageObject = new("Message");
        messageObject.transform.SetParent(chatLog);
        TextMeshProUGUI text = messageObject.AddComponent<TextMeshProUGUI>();
        text.text = message;
        text.fontSize = chatLog.sizeDelta.y / maxMessagesToDisplay;
        text.color = fromSelf ? Color.green : Color.red;
        RectTransform rectTransform = messageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0f, 0f);
        rectTransform.sizeDelta = new Vector2(chatLog.sizeDelta.x, text.fontSize);
        return text;
    }

    private void UpdateChatLog()
    {
        for (int i = 0; i < messages.Count; i++)
        {
            RectTransform rectTransform = messages[i].GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, rectTransform.sizeDelta.y * (messages.Count - i - 1));
        }
    }
}
