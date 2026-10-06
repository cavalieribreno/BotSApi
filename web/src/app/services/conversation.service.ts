import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/env.production';
import { ConversationDetail } from '../models/message';

// Reads a conversation (phone + messages) — the chat behind an appointment.
@Injectable({ providedIn: 'root' })
export class ConversationService {
  private readonly api = `${environment.apiUrl}/conversations`;

  constructor(private http: HttpClient) {}

  getMessages(conversationId: string): Observable<ConversationDetail> {
    return this.http.get<ConversationDetail>(`${this.api}/${conversationId}/messages`);
  }
}
