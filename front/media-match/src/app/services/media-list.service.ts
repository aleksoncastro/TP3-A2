import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  MediaList,
  MediaListDetail,
  CreateMediaListDto,
  UpdateMediaListDto,
  AddMediaListItemDto,
  CreateMediaListCommentDto,
  MediaListItem,
  MediaListComment
} from '../models/media-list.model';

@Injectable({
  providedIn: 'root'
})
export class MediaListService {
  private apiUrl = `${environment.apiBase}/club`;

  constructor(private http: HttpClient) {}

  // ===== CRUD de Listas =====
  
  createList(clubId: number, dto: CreateMediaListDto): Observable<MediaList> {
    return this.http.post<MediaList>(`${this.apiUrl}/${clubId}/list`, dto);
  }

  getClubLists(clubId: number): Observable<MediaList[]> {
    return this.http.get<MediaList[]>(`${this.apiUrl}/${clubId}/list`);
  }

  getListDetail(clubId: number, listId: number): Observable<MediaListDetail> {
    return this.http.get<MediaListDetail>(`${this.apiUrl}/${clubId}/list/${listId}`);
  }

  updateList(clubId: number, listId: number, dto: UpdateMediaListDto): Observable<MediaList> {
    return this.http.put<MediaList>(`${this.apiUrl}/${clubId}/list/${listId}`, dto);
  }

  deleteList(clubId: number, listId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${clubId}/list/${listId}`);
  }

  // ===== Itens da Lista =====

  addItemToList(clubId: number, listId: number, dto: AddMediaListItemDto): Observable<MediaListItem> {
    return this.http.post<MediaListItem>(`${this.apiUrl}/${clubId}/list/${listId}/item`, dto);
  }

  removeItemFromList(clubId: number, listId: number, itemId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${clubId}/list/${listId}/item/${itemId}`);
  }

  // ===== Comentários =====

  createComment(clubId: number, listId: number, dto: CreateMediaListCommentDto): Observable<MediaListComment> {
    return this.http.post<MediaListComment>(`${this.apiUrl}/${clubId}/list/${listId}/comment`, dto);
  }

  deleteComment(clubId: number, listId: number, commentId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${clubId}/list/${listId}/comment/${commentId}`);
  }
}
