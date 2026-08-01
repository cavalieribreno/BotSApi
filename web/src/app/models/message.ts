// A message in a conversation (matches the API's MessageResponse).
export interface Message {
  role: string;      // "User" or "Assistant"
  content: string;
  createdAt: string;
}

// The conversation detail from GET /api/conversations/{id}/messages.
export interface ConversationDetail {
  customerPhone: string;
  messages: Message[];
}
