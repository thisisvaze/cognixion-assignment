using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenAIController : MonoBehaviour
{
    //[TextArea]
    //public string textField;
    private OpenAIAPI api;
    private List<ChatMessage> messages;

    [SerializeField]
    private GameObject chatScrollView;
    [SerializeField]
    private GameObject userMessagePrefab;
    [SerializeField]
    private GameObject assistantMessagePrefab;

    //move to secret key file or env
    private string OPENAI_API_KEY = "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";
    // Start is called before the first frame update
    void Awake()
    {
            messages = new List<ChatMessage>
        {
            new ChatMessage(ChatMessageRole.System, "Respond with a small response in 10 words or less.")
        };
    }

    // In OpenAIController.cs
public void AddUserMessage(string messageContent)
{
    if (messages == null)
    {
        messages = new List<ChatMessage>
        {
            new ChatMessage(ChatMessageRole.System, "Respond with a small response in 10 words or less.")
        };
    }
    messages.Add(new ChatMessage(ChatMessageRole.User, messageContent));
    GameObject newMessage = Instantiate(userMessagePrefab, chatScrollView.transform);
    newMessage.GetComponentInChildren<TextMeshProUGUI>().text = "User: " + messageContent;
}
public async void GetShortResponseForObject(Action onCompleted)
{
    try
    {
        //Call OpenAI GPT 
        var key = new APIAuthentication(OPENAI_API_KEY);
        api = new OpenAIAPI(key);
    

        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = Model.GPT4,
            Temperature = 0.7,
            MaxTokens = 400,
            Messages = messages
        });

        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;
        Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));

        messages.Add(responseMessage);

        GameObject newMessage = Instantiate(assistantMessagePrefab, chatScrollView.transform);
        newMessage.GetComponentInChildren<TextMeshProUGUI>().text = "Assistant: " + responseMessage.Content;

        onCompleted?.Invoke();
    }
    catch (Exception ex)
    {
        Debug.LogError("Error generating response: " + ex.Message);

        // Display the error message using the userMessagePrefab
        GameObject errorMessage = Instantiate(userMessagePrefab, chatScrollView.transform);
        errorMessage.GetComponent<TextMeshProUGUI>().text = "Error: " + ex.Message;

        // Optionally, invoke the callback even in case of error
        onCompleted?.Invoke();
    }
}
    
}
