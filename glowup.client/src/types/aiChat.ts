// Mirrors GlowUp.Api AiChatRequestDto / AiChatResponseDto.
export interface AiChatRequest {
  question: string;
}

export interface AiChatResponse {
  answer: string;
  usedContextSummary: string;
  createdAt: string;
}
