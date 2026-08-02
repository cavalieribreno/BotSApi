using System.Text.Json.Serialization;

namespace BotSaaS.Api.Channels.Telegram;

// Minimal shapes of the Telegram Bot API payloads we actually read.

// getUpdates response: { ok, result: [ update... ] }
public record TgResponse(bool Ok, List<TgUpdate> Result);

// One update. We only need update_id (to advance the offset) and the message.
public record TgUpdate([property: JsonPropertyName("update_id")] long UpdateId, TgMessage? Message);

// A message: which chat it came from + the text (null for non-text messages).
public record TgMessage(TgChat Chat, string? Text);

// The chat/customer identifier -- used as the conversation key.
public record TgChat(long Id);

// sendMessage body: reply text back to a chat.
public record TgSendMessage([property: JsonPropertyName("chat_id")] long ChatId, string Text);
