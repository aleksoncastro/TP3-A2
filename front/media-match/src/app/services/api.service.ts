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
}

export interface TmdbTv {
  id: number;
  name: string;
  poster_path: string | null;
  overview: string;
  first_air_date?: string;
}

export interface SoundtrackAlbum {
  id: string;
  name: string;
  url: string;
  release_date?: string;
  artists: string[];
}

export interface SoundtrackTrack {
  id: string;
  title: string;
  artists: string[];
  duration_ms: number;
  url: string;
  preview_url?: string | null;
  genre?: string;
  mood?: string;
  description?: string;
  thumb_url?: string;
  video_url?: string;
}

export interface SoundtrackDto {
  source: string;
  composer: string;
  album: SoundtrackAlbum;
  tracks: SoundtrackTrack[];
  confidence?: number;
}

const API_BASE = environment.apiBase;

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);

  private parseJson<T>(text$: Observable<string>): Observable<T> {
    return text$.pipe(map((txt) => JSON.parse(txt)));
  }

  getPopularMovies(): Observable<TmdbListResponse<TmdbMovie>> {
    const url = `${API_BASE}/Tmdb/movies/popular`;
    return this.parseJson<TmdbListResponse<TmdbMovie>>(this.http.get(url, { responseType: 'text' }));
  }

  getPopularSeries(): Observable<TmdbListResponse<TmdbTv>> {
    const url = `${API_BASE}/Tmdb/series/popular`;
    return this.parseJson<TmdbListResponse<TmdbTv>>(this.http.get(url, { responseType: 'text' }));
  }

  getMovieDetails(id: number): Observable<any> {
    const url = `${API_BASE}/Tmdb/movies/details?id=${id}`;
    return this.parseJson<any>(this.http.get(url, { responseType: 'text' }));
  }

  getSeriesDetails(id: number): Observable<any> {
    const url = `${API_BASE}/Tmdb/series/details?id=${id}`;
    return this.parseJson<any>(this.http.get(url, { responseType: 'text' }));
  }

  searchMovies(q: string): Observable<TmdbListResponse<TmdbMovie>> {
    const url = `${API_BASE}/Tmdb/movies/search?q=${encodeURIComponent(q)}`;
    return this.parseJson<TmdbListResponse<TmdbMovie>>(this.http.get(url, { responseType: 'text' }));
  }

  searchSeries(q: string): Observable<TmdbListResponse<TmdbTv>> {
    const url = `${API_BASE}/Tmdb/series/search?q=${encodeURIComponent(q)}`;
    return this.parseJson<TmdbListResponse<TmdbTv>>(this.http.get(url, { responseType: 'text' }));
  }

  getMovieSoundtrack(id: number): Observable<SoundtrackDto> {
    const url = `${API_BASE}/Soundtrack/movie/${id}`;
    return this.parseJson<SoundtrackDto>(this.http.get(url, { responseType: 'text' }));
  }
  // tv = serie
  getTvSoundtrack(id: number): Observable<SoundtrackDto> {
    const url = `${API_BASE}/Soundtrack/tv/${id}`;
    return this.parseJson<SoundtrackDto>(this.http.get(url, { responseType: 'text' }));
  }
}