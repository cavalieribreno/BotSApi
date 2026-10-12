import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/env.production';

export interface ChannelResponse {
  id: string;
  companyId: string;
  type: number;
  identifier: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ConnectChannelRequest {
  type: number;
  identifier: string;
}

@Injectable({
  providedIn: 'root'
})
export class ChannelService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/channels`;

  getChannels(): Observable<ChannelResponse[]> {
    return this.http.get<ChannelResponse[]>(this.apiUrl);
  }

  registerChannel(payload: ConnectChannelRequest): Observable<ChannelResponse> {
    return this.http.post<ChannelResponse>(this.apiUrl, payload);
  }

  deleteChannel(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
