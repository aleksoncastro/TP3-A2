import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TmdbListResponse<T> {
  page: number;
  results: T[];
}

export interface TmdbMovie {
  id: number;
  title: string;
  poster_path: string | null;
  overview: string;
  release_date?: string;
  backdrop_path?: string | null;
  vote_average?: number;
}

const API_BASE = environment.apiBase;

@Injectable({ providedIn: 'root' })
export class MoviesService {
  private readonly http = inject(HttpClient);

  private parseJson<T>(text$: Observable<string>): Observable<T> {
    return text$.pipe(map((txt) => JSON.parse(txt)));
  }

  getPopular(language: string = 'pt-BR', page: number = 1, region?: string): Observable<TmdbListResponse<TmdbMovie>> {
    const params: string[] = [`language=${encodeURIComponent(language)}`, `page=${page}`];
    if (region && region.trim().length > 0) params.push(`region=${encodeURIComponent(region)}`);
    const url = `${API_BASE}/Tmdb/movies/popular?${params.join('&')}`;
    return this.parseJson<TmdbListResponse<TmdbMovie>>(this.http.get(url, { responseType: 'text' }));
  }

  getNowPlaying(): Observable<TmdbListResponse<TmdbMovie>> {
    const url = `${API_BASE}/Tmdb/movies/now_playing`;
    return this.http.get<TmdbListResponse<TmdbMovie>>(url);
  }

  getTopRated(language: string = 'pt-BR', page: number = 1, region?: string): Observable<TmdbListResponse<TmdbMovie>> {
    const params: string[] = [`language=${encodeURIComponent(language)}`, `page=${page}`];
    if (region && region.trim().length > 0) params.push(`region=${encodeURIComponent(region)}`);
    const url = `${API_BASE}/Tmdb/movies/top_rated?${params.join('&')}`;
    return this.parseJson<TmdbListResponse<TmdbMovie>>(this.http.get(url, { responseType: 'text' }));
  }

  getUpcoming(language: string = 'pt-BR', page: number = 1, region?: string): Observable<TmdbListResponse<TmdbMovie>> {
    const params: string[] = [`language=${encodeURIComponent(language)}`, `page=${page}`];
    if (region && region.trim().length > 0) params.push(`region=${encodeURIComponent(region)}`);
    const url = `${API_BASE}/Tmdb/movies/upcoming?${params.join('&')}`;
    return this.parseJson<TmdbListResponse<TmdbMovie>>(this.http.get(url, { responseType: 'text' }));
  }

  getTrending(language: string = 'pt-BR', page: number = 1, region?: string, timeWindow: 'day' | 'week' = 'day'): Observable<TmdbListResponse<TmdbMovie>> {
    const params: string[] = [`language=${encodeURIComponent(language)}`, `page=${page}`, `timeWindow=${encodeURIComponent(timeWindow)}`];
    if (region && region.trim().length > 0) params.push(`region=${encodeURIComponent(region)}`);
    const url = `${API_BASE}/Tmdb/movies/trending?${params.join('&')}`;
    return this.parseJson<TmdbListResponse<TmdbMovie>>(this.http.get(url, { responseType: 'text' }));
  }

  getDetails(id: number): Observable<any> {
    const url = `${API_BASE}/Tmdb/movies/details?id=${id}`;
    return this.parseJson<any>(this.http.get(url, { responseType: 'text' }));
  }

  search(
    q: string,
    includeAdult: boolean = false,
    language: string = 'pt-BR',
    primaryReleaseYear?: number,
    page: number = 1,
    region?: string,
    year?: string
  ): Observable<TmdbListResponse<TmdbMovie>> {
    const params: string[] = [
      `q=${encodeURIComponent(q)}`,
      `include_adult=${includeAdult}`,
      `language=${encodeURIComponent(language)}`,
      `page=${page}`
    ];
    if (primaryReleaseYear !== undefined && primaryReleaseYear !== null) params.push(`primary_release_year=${primaryReleaseYear}`);
    if (region && region.trim().length > 0) params.push(`region=${encodeURIComponent(region)}`);
    if (year && year.trim().length > 0) params.push(`year=${encodeURIComponent(year)}`);
    const url = `${API_BASE}/Tmdb/movies/search?${params.join('&')}`;
    return this.parseJson<TmdbListResponse<TmdbMovie>>(this.http.get(url, { responseType: 'text' }));
  }

  getCredits(id: number): Observable<any> {
    const url = `${API_BASE}/Tmdb/movies/credits?id=${id}`;
    return this.parseJson<any>(this.http.get(url, { responseType: 'text' }));
  }

  getColletion(id: number): Observable<any> {
    const url = `${API_BASE}/Tmdb/collection/details?id=${id}`;
    return this.parseJson<any>(this.http.get(url, { responseType: 'text' }));
  }
}
