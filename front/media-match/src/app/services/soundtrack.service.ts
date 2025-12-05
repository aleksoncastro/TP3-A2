import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

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
export class SoundtrackService {
  private readonly http = inject(HttpClient);

  private parseJson<T>(text$: Observable<string>): Observable<T> {
    return text$.pipe(map((txt) => JSON.parse(txt)));
  }

  getMovieSoundtrack(id: number): Observable<SoundtrackDto> {
    const url = `${API_BASE}/Soundtrack/movie/${id}`;
    return this.parseJson<SoundtrackDto>(this.http.get(url, { responseType: 'text' }));
  }

  getTvSoundtrack(id: number): Observable<SoundtrackDto> {
    const url = `${API_BASE}/Soundtrack/serie/${id}`;
    return this.parseJson<SoundtrackDto>(this.http.get(url, { responseType: 'text' }));
  }
}
