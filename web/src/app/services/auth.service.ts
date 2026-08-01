import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

// Auth: logs in against the API and holds the JWT (in localStorage).
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = 'http://localhost:5069/api/auth';
  private readonly tokenKey = 'botsaas_token';

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<{ token: string }> {
    return this.http
      .post<{ token: string }>(`${this.api}/login`, { email, password })
      .pipe(tap(res => localStorage.setItem(this.tokenKey, res.token)));
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
  }

  get token(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  get isLoggedIn(): boolean {
    return this.token !== null;
  }
}
