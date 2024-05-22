
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;

namespace Knowit.POC.AiTranslationButton.Api;

public static class AiClient
{
    private static OpenAIAPI? _gptApi;

    private static void EnsureAiClient()
    {
        if(_gptApi != null) return;
        _gptApi = OpenAIAPI.ForAzure("xxxxxx", "gpt-35-turbo", "xxxxxxxxxxxxxxxxxxx");
        _gptApi.ApiVersion = "2024-02-01";
    }


    public static async Task<ChatResult> TranslateAsync(object request)
    {
        EnsureAiClient();

        return await _gptApi.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Messages =  BuildPrompt(request),
            ResponseFormat = ChatRequest.ResponseFormats.Text
        });
    }

    private static List<ChatMessage> BuildPrompt(object request)
    {
        var messages = new List<ChatMessage>()
        {
            new ChatMessage()
            {
                Role = ChatMessageRole.System, TextContent =
                    @"
		You are a sophisticated translation service interacting with a JSON object that contains multiple 'Items,' each identified by a 'key' (language) and 'value' (object with multiple properties in that language). While many of these 'values' may initially be empty with at least one filled for each item, your task is to translate and complete these empty fields, ensuring a comprehensive multilingual dataset.

		Your final output should be the enriched JSON, with all values filled, maintaining the integrity and structure of the original format."
            }
        };


        messages.Add(new ChatMessage()
        {
            Role = ChatMessageRole.User,
            TextContent = $"please translate missing values in the following json: " + JsonConvert.SerializeObject(request)
        });

        return messages;
    }

}