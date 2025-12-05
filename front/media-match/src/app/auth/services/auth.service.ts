import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, tap } from 'rxjs';

export interface AuthResponseDto {
  token: string;
  userName: string;
  role: string;
}

export interface RegisterDto {
  userName: string;
  email: string;
  password: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface ForgotPasswordRequestDto {
  email: string;
}

export interface ResetPasswordRequestDto {
  email: string;
  code: string;
  newPassword: string;
}

export interface MeDto {
  id: number;
  userName: string;
  email: string;
  createdAt: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private storage: Storage = localStorage;
  private base = environment.apiBase;

  register(dto: RegisterDto): Observable<AuthResponseDto> {
    return this.http
      .post<AuthResponseDto>(`${this.base}/Auth/register`, dto)
      .pipe(tap((res) => this.persist(res)));
  }

  login(dto: LoginDto): Observable<AuthResponseDto> {
    return this.http
      .post<AuthResponseDto>(`${this.base}/Auth/login`, dto)
      .pipe(tap((res) => this.persist(res)));
  }

  me(): Observable<MeDto> {
    return this.http.get<MeDto>(`${this.base}/Auth/me`).pipe(
      tap((res) => {
        if (res?.role) this.storage.setItem('role', res.role);
      })
    );
  }

  setSessionStorage(): void {
    this.storage = sessionStorage;
  }

  getToken(): string | null {
    return this.storage.getItem('token');
  }

  getRole(): string | null {
    return this.storage.getItem('role');
  }

  isAdmin(): boolean {
    const role = this.getRole();
    return role === 'admin';
  }

  logout(): void {
    this.storage.removeItem('token');
    this.storage.removeItem('role');
  }

  requestPasswordReset(dto: ForgotPasswordRequestDto): Observable<void> {
    return this.http.post<void>(`${this.base}/Auth/forgot-password`, dto);
  }

  resetPassword(dto: ResetPasswordRequestDto): Observable<void> {
    return this.http.post<void>(`${this.base}/Auth/reset-password`, dto);
  }

  private persist(res: AuthResponseDto): void {
    this.storage.setItem('token', res.token);
    if (res.role) this.storage.setItem('role', res.role);
  }
}
