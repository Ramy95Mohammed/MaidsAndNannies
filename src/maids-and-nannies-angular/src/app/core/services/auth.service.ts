import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface User {
  id: string;
  fullName: string;
  email: string;
  role: 'Admin' | 'Homeowner' | 'Worker';
  preferredLanguage: string;
  isVerified: boolean;
}

export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  fullName:string,
  role:string,
  preferredLanguage:string,
  verificationStatus:number,
  email:string
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = environment.apiUrl;
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'auth_user';

  currentUser = signal<AuthResponse | null>(this.loadUser());
  isAuthenticated = computed(() => !!this.currentUser());
  isAdmin = computed(() => this.currentUser()?.role === 'Admin');
  isHomeowner = computed(() => this.currentUser()?.role === 'Homeowner');
  isWorker = computed(() => this.currentUser()?.role === 'Worker');

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  private loadUser(): AuthResponse | null {
    const userJson = localStorage.getItem(this.USER_KEY);
    return userJson ? JSON.parse(userJson) : null;
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  registerHomeowner(data: any): Observable<any> {
    return this.http.post(`${this.API_URL}/auth/register/homeowner`, data);
  }

registerWorker(data: FormData): Observable<{ message: string }> {
  return this.http.post<{ message: string }>(`${this.API_URL}/auth/register/worker`, data);
}
  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/login`, { email, password })
      .pipe(
        tap(response => {
          localStorage.setItem(this.TOKEN_KEY, response.accessToken);
          localStorage.setItem(this.USER_KEY, JSON.stringify(response));
          this.currentUser.set(response);
        })
      );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUser.set(null);
    this.router.navigate(['/auth/login']);
  }

  getCurrentUser(): Observable<AuthResponse> {
    return this.http.get<AuthResponse>(`${this.API_URL}/auth/me`)
      .pipe(
        tap(user => {
          localStorage.setItem(this.USER_KEY, JSON.stringify(user));
          this.currentUser.set(user);
        })
      );
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }
}
