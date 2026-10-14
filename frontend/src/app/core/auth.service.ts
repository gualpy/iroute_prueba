import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

interface LoginResponse { token: string; expiresAt: string; username: string; }

const TOKEN_KEY = 'iroute_token';
const USER_KEY = 'iroute_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _token = signal<string | null>(sessionStorage.getItem(TOKEN_KEY));
  readonly username = signal<string | null>(sessionStorage.getItem(USER_KEY));
  readonly isLoggedIn = computed(() => !!this._token());

  get token(): string | null { return this._token(); }

  login(username: string, password: string) {
    return this.http
      .post<LoginResponse>('http://localhost:5009/api/auth/login', { username, password })
      .pipe(tap(r => {
        sessionStorage.setItem(TOKEN_KEY, r.token);
        sessionStorage.setItem(USER_KEY, r.username);
        this._token.set(r.token);
        this.username.set(r.username);
      }));
  }

  logout() {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
    this._token.set(null);
    this.username.set(null);
    this.router.navigate(['/login']);
  }
}
