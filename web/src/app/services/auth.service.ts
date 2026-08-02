import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

// Auth: logs in against the API and holds the JWT (in localStorage).
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = 'http://localhost:5069/api/auth';
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
    return this.token !== null;
  }
}
