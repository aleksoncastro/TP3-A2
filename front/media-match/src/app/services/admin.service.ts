import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface UserListItemDto {
  id: number;
  userName: string;
  email: string;
  createdAt: string;
  role: string;
}

export interface UsersPagedResultDto {
  items: UserListItemDto[];
  total: number;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);
  private base = environment.apiBase;

  getUsers(params: {
    page?: number;
    pageSize?: number;
    name?: string;
    email?: string;
    createdFrom?: string;
    createdTo?: string;
    role?: string;
  }) {
    return this.http.get<UsersPagedResultDto>(`${this.base}/Auth/users`, { params });
  }

  changeRole(request: { userId: number; role: string }) {
    return this.http.put<void>(`${this.base}/Auth/roles`, request);
  }

  deleteUser(userId: number) {
    return this.http.delete<void>(`${this.base}/Auth/users/${userId}`);
  }
}

