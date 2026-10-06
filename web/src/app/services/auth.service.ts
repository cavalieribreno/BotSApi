import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/env.production';

// Auth: logs in against the API and holds the JWT (in localStorage).
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = `${environment.apiUrl}/auth`;
  private readonly tokenKey = 'botsaas_token';
  private readonly emailKey = 'botsaas_email';
  private readonly companyKey = 'botsaas_company';

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<{ token: string; companyName: string }> {
    return this.http
      .post<{ token: string; companyName: string }>(`${this.api}/login`, { email, password })
      .pipe(tap(res => {
        localStorage.setItem(this.tokenKey, res.token);
        localStorage.setItem(this.emailKey, email);          // identity in the shell
        localStorage.setItem(this.companyKey, res.companyName); // tenant brand (white-label)
      }));
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.emailKey);
    localStorage.removeItem(this.companyKey);
  }

  get token(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  get userEmail(): string | null {
    return localStorage.getItem(this.emailKey);
  }

  get companyName(): string | null {
    return localStorage.getItem(this.companyKey);
  }

  get isLoggedIn(): boolean {
    const token = this.token;
    if (!token) {
      return false;
    }

    try {
      const parts = token.split('.');
      if (parts.length !== 3) {
        this.logout();
        return false;
      }

      const payload = JSON.parse(atob(parts[1]));
      if (payload.exp && payload.exp * 1000 < Date.now()) {
        this.logout();
        return false;
      }

      return true;
    } catch {
      this.logout();
      return false;
    }
  }
}
