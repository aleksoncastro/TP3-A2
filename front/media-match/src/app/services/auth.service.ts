import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
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

export interface MeDto {
  id: number;
  userName: string;
  email: string;
  createdAt: string;
  role: string;
}

export interface UserProfileDto {
  id: number;
  userName: string;
  email: string;
  profilePictureUrl?: string;
  phoneNumber?: string;
  bio?: string;
  createdAt: string;
  role: string;
}

export interface UpdateProfileDto {
  userName?: string;
  email?: string;
  phoneNumber?: string;
  bio?: string;
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

  // Métodos administrativos removidos do serviço de autenticação

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

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getUserName(): string | null {
    return this.storage.getItem('userName');
  }

  getProfile(): Observable<UserProfileDto> {
    return this.http.get<UserProfileDto>(`${this.base}/Auth/profile`);
  }

  updateProfile(dto: UpdateProfileDto): Observable<UserProfileDto> {
    return this.http.put<UserProfileDto>(`${this.base}/Auth/profile`, dto);
  }

  updateProfilePicture(file: File): Observable<{ profilePictureUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ profilePictureUrl: string }>(
      `${this.base}/Auth/profile/picture`,
      formData
    );
  }

  logout(): void {
    this.storage.removeItem('token');
    this.storage.removeItem('role');
    this.storage.removeItem('userName');
  }

  private persist(res: AuthResponseDto): void {
    this.storage.setItem('token', res.token);
    if (res.role) this.storage.setItem('role', res.role);
    if (res.userName) this.storage.setItem('userName', res.userName);
  }
}
