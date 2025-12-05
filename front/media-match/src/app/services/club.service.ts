import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  Club,
  ClubDetail,
  CreateClubDto,
  UpdateClubDto,
  Post,
  PostDetail,
  CreatePostDto,
  UpdatePostDto,
  Comment,
  CreateCommentDto,
  UpdateCommentDto
} from '../models/club.model';

interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class ClubService {
  private apiUrl = 'http://localhost:5042/api';

  constructor(private http: HttpClient) {}

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  // ========== CLUB ENDPOINTS ==========

  getClubs(
    search?: string, 
    page: number = 1, 
    pageSize: number = 20,
    sortBy: string = 'newest',
    sortOrder: string = 'desc'
  ): Observable<Club[]> {
    let url = `${this.apiUrl}/club?page=${page}&pageSize=${pageSize}&sortBy=${sortBy}&sortOrder=${sortOrder}`;
    if (search) {
      url += `&searchTerm=${encodeURIComponent(search)}`;
    }
    // GET /club é AllowAnonymous, mas envia token se existir para pegar isMember/isOwner
    const token = localStorage.getItem('token');
    const headers = token ? new HttpHeaders({ 'Authorization': `Bearer ${token}` }) : undefined;
    return this.http.get<PagedResult<Club>>(url, headers ? { headers } : {}).pipe(
      map(result => result.items)
    );
  }

  getClubById(id: number): Observable<ClubDetail> {
    // GET /club/{id} é AllowAnonymous, mas envia token se existir
    const token = localStorage.getItem('token');
    const headers = token ? new HttpHeaders({ 'Authorization': `Bearer ${token}` }) : undefined;
    return this.http.get<ClubDetail>(`${this.apiUrl}/club/${id}`, headers ? { headers } : {});
  }

  getMyClubs(): Observable<Club[]> {
    return this.http.get<Club[]>(`${this.apiUrl}/club/my-clubs`, {
      headers: this.getAuthHeaders()
    });
  }

  createClub(dto: CreateClubDto, image?: File): Observable<Club> {
    const formData = new FormData();
    formData.append('Name', dto.name);
    if (dto.description) {
      formData.append('Description', dto.description);
    }
    if (image) {
      formData.append('image', image);
    }

    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    return this.http.post<Club>(`${this.apiUrl}/club`, formData, { headers });
  }

  updateClub(id: number, dto: UpdateClubDto, image?: File): Observable<Club> {
    const formData = new FormData();
    formData.append('Name', dto.name);
    if (dto.description) {
      formData.append('Description', dto.description);
    }
    if (dto.removeImage) {
      formData.append('RemoveImage', 'true');
    }
    if (image) {
      formData.append('image', image);
    }

    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    return this.http.put<Club>(`${this.apiUrl}/club/${id}`, formData, { headers });
  }

  deleteClub(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/club/${id}`, {
      headers: this.getAuthHeaders()
    });
  }

  joinClub(clubId: number): Observable<void> {
    const userId = this.getCurrentUserId();
    return this.http.post<void>(
      `${this.apiUrl}/club/${clubId}/members`,
      { userId: userId },
      { headers: this.getAuthHeaders() }
    );
  }

  private getCurrentUserId(): number {
    // Decodifica o token JWT para pegar o userId
    const token = localStorage.getItem('token');
    if (!token) return 0;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // O ClaimTypes.NameIdentifier no .NET gera "nameid" no JWT
      return parseInt(payload.nameid || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || '0');
    } catch {
      return 0;
    }
  }

  leaveClub(clubId: number): Observable<void> {
    const userId = this.getCurrentUserId();
    return this.http.delete<void>(
      `${this.apiUrl}/club/${clubId}/members/${userId}`,
      { headers: this.getAuthHeaders() }
    );
  }

  // ========== POST ENDPOINTS ==========

  getClubPosts(clubId: number, skip: number = 0, take: number = 20): Observable<Post[]> {
    return this.http.get<Post[]>(
      `${this.apiUrl}/club/${clubId}/post?skip=${skip}&take=${take}`,
      { headers: this.getAuthHeaders() }
    );
  }

  getPostById(clubId: number, postId: number): Observable<PostDetail> {
    return this.http.get<PostDetail>(
      `${this.apiUrl}/club/${clubId}/post/${postId}`,
      { headers: this.getAuthHeaders() }
    );
  }

  createPost(clubId: number, dto: CreatePostDto, images?: File[]): Observable<Post> {
    const formData = new FormData();
    formData.append('Content', dto.content);
    if (images && images.length > 0) {
      images.forEach((image, index) => {
        formData.append('images', image);
      });
    }

    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    return this.http.post<Post>(
      `${this.apiUrl}/club/${clubId}/post`,
      formData,
      { headers }
    );
  }

  updatePost(clubId: number, postId: number, dto: UpdatePostDto, images?: File[]): Observable<Post> {
    const formData = new FormData();
    formData.append('Content', dto.content);
    if (dto.removeImage) {
      formData.append('RemoveImage', 'true');
    }
    if (images && images.length > 0) {
      images.forEach((image, index) => {
        formData.append('images', image);
      });
    }

    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    return this.http.put<Post>(
      `${this.apiUrl}/club/${clubId}/post/${postId}`,
      formData,
      { headers }
    );
  }

  deletePost(clubId: number, postId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/club/${clubId}/post/${postId}`,
      { headers: this.getAuthHeaders() }
    );
  }

  // ========== COMMENT ENDPOINTS ==========

  getPostComments(clubId: number, postId: number): Observable<Comment[]> {
    return this.http.get<Comment[]>(
      `${this.apiUrl}/club/${clubId}/post/${postId}/comment`,
      { headers: this.getAuthHeaders() }
    );
  }

  createComment(clubId: number, postId: number, dto: CreateCommentDto): Observable<Comment> {
    return this.http.post<Comment>(
      `${this.apiUrl}/club/${clubId}/post/${postId}/comment`,
      dto,
      { headers: this.getAuthHeaders() }
    );
  }

  updateComment(clubId: number, postId: number, commentId: number, dto: UpdateCommentDto): Observable<Comment> {
    return this.http.put<Comment>(
      `${this.apiUrl}/club/${clubId}/post/${postId}/comment/${commentId}`,
      dto,
      { headers: this.getAuthHeaders() }
    );
  }

  deleteComment(clubId: number, postId: number, commentId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/club/${clubId}/post/${postId}/comment/${commentId}`,
      { headers: this.getAuthHeaders() }
    );
  }
}
