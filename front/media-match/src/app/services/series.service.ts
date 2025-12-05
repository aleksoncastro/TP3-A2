import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TmdbListResponse<T> {
  page: number;
  results: T[];
}

export interface TmdbTv {
  id: number;
  name: string;
  poster_path: string | null;
  overview: string;
  first_air_date?: string;
  vote_average?: number;      // <--- ADICIONADO: Corrige o erro TS2339
  backdrop_path?: string | null; // <--- ADICIONADO: Útil para detalhes
}

const API_BASE = environment.apiBase;

@Injectable({ providedIn: 'root' })
export class SeriesService {
  private readonly http = inject(HttpClient);

  private parseJson<T>(text$: Observable<string>): Observable<T> {
    return text$.pipe(map((txt) => JSON.parse(txt)));
  }

  getPopular(language: string = 'pt-BR', page: number = 1, region?: string): Observable<TmdbListResponse<TmdbTv>> {
    const params: string[] = [`language=${encodeURIComponent(language)}`, `page=${page}`];
    if (region && region.trim().length > 0) params.push(`region=${encodeURIComponent(region)}`);
    const url = `${API_BASE}/Tmdb/series/popular?${params.join('&')}`;
    return this.parseJson<TmdbListResponse<TmdbTv>>(this.http.get(url, { responseType: 'text' }));
  }

  getAiringToday(language: string = 'pt-BR', page: number = 1, region?: string): Observable<TmdbListResponse<TmdbTv>> {
    const params: string[] = [`language=${encodeURIComponent(language)}`, `page=${page}`];
    if (region && region.trim().length > 0) params.push(`region=${encodeURIComponent(region)}`);
    const url = `${API_BASE}/Tmdb/series/airing_today?${params.join('&')}`;
    return this.parseJson<TmdbListResponse<TmdbTv>>(this.http.get(url, { responseType: 'text' }));
  }

  getOnTheAir(language: string = 'pt-BR', page: number = 1, region?: string): Observable<TmdbListResponse<TmdbTv>> {
    const params: string[] = [`language=${encodeURIComponent(language)}`, `page=${page}`];
    if (region && region.trim().length > 0) params.push(`region=${encodeURIComponent(region)}`);
    const url = `${API_BASE}/Tmdb/series/on_the_air?${params.join('&')}`;
    return this.parseJson<TmdbListResponse<TmdbTv>>(this.http.get(url, { responseType: 'text' }));
  }

  getTopRated(language: string = 'pt-BR', page: number = 1, region?: string): Observable<TmdbListResponse<TmdbTv>> {
    const params: string[] = [`language=${encodeURIComponent(language)}`, `page=${page}`];
    if (region && region.trim().length > 0) params.push(`region=${encodeURIComponent(region)}`);
    const url = `${API_BASE}/Tmdb/series/top_rated?${params.join('&')}`;
    return this.parseJson<TmdbListResponse<TmdbTv>>(this.http.get(url, { responseType: 'text' }));
  }

  getDetails(id: number): Observable<any> {
    const url = `${API_BASE}/Tmdb/series/details?id=${id}`;
    return this.parseJson<any>(this.http.get(url, { responseType: 'text' }));
  }

  search(
    q: string,
    includeAdult: boolean = false,
    language: string = 'pt-BR',
    firstAirYear?: number,
    page: number = 1,
    region?: string
  ): Observable<TmdbListResponse<TmdbTv>> {
    const params: string[] = [
      `q=${encodeURIComponent(q)}`,
      `include_adult=${includeAdult}`,
      `language=${encodeURIComponent(language)}`,
      `page=${page}`
    ];
    if (firstAirYear !== undefined && firstAirYear !== null) params.push(`first_air_year=${firstAirYear}`);
    if (region && region.trim().length > 0) params.push(`region=${encodeURIComponent(region)}`);
    const url = `${API_BASE}/Tmdb/series/search?${params.join('&')}`;
    return this.parseJson<TmdbListResponse<TmdbTv>>(this.http.get(url, { responseType: 'text' }));
  }

  getCredits(id: number): Observable<any> {
    const url = `${API_BASE}/Tmdb/series/credits?id=${id}`;
    return this.parseJson<any>(this.http.get(url, { responseType: 'text' }));
  }
}
