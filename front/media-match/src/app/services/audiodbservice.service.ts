import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';


// ==========================================
// ARTISTA
// ==========================================
export interface AudioDbArtist {
  idArtist: string;
  strArtist: string;
  strBiographyEN?: string;
  strCountry?: string;
  strArtistThumb?: string;
  // Adicione outros campos se o seu DTO C# tiver mais
}

// ==========================================
// ÁLBUM
// ==========================================
export interface AudioDbAlbum {
  idAlbum: string;
  strAlbum: string;
  intYearReleased?: string;
  strGenre?: string;
  strLabel?: string;
  strAlbumThumb?: string;
  idArtist?: string;
}

// ==========================================
// MÚSICA (TRACK)
// ==========================================
export interface AudioDbTrack {
  idTrack: string;
  strTrack: string;
  intDuration?: string;
  strTrackThumb?: string;
  idAlbum?: string;
  strGenre?: string;
  strMood?: string;
  strDescriptionEN?: string;
  strMusicVid?: string;
}

// Ajuste conforme seu environment ou coloque a URL direta aqui
const API_BASE = environment.apiBase; 
// Exemplo: 'https://localhost:7001/api/music/search'

@Injectable({
  providedIn: 'root'
})
export class AudioDbService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE}/music/search`;

  // =================================================================
  // 1. MÉTODOS DE ARTISTA
  // =================================================================

  /**
   * Busca artistas pelo nome.
   * Endpoint: GET /search-artist?name=...
   */
  searchArtist(name: string): Observable<AudioDbArtist[]> {
    const params = new HttpParams().set('name', name);
    return this.http.get<AudioDbArtist[]>(`${this.baseUrl}/search-artist`, { params });
  }

  // =================================================================
  // 2. MÉTODOS DE ÁLBUM
  // =================================================================

  /**
   * Busca todos os álbuns pelo ID do Artista.
   * Endpoint: GET /albums-by-id?artistId=...
   */
  getAlbumsByArtistId(artistId: number): Observable<AudioDbAlbum[]> {
    const params = new HttpParams().set('artistId', artistId);
    return this.http.get<AudioDbAlbum[]>(`${this.baseUrl}/albums-by-id`, { params });
  }

  /**
   * Busca todos os álbuns pelo Nome do Artista.
   * Endpoint: GET /albums-by-name?artistName=...
   */
  getAlbumsByArtistName(artistName: string): Observable<AudioDbAlbum[]> {
    const params = new HttpParams().set('artistName', artistName);
    return this.http.get<AudioDbAlbum[]>(`${this.baseUrl}/albums-by-name`, { params });
  }

  /**
   * Busca um álbum específico.
   * Endpoint: GET /album?artist=...&album=...
   */
  searchAlbum(artist: string, album: string): Observable<AudioDbAlbum[]> {
    const params = new HttpParams()
      .set('artist', artist)
      .set('album', album);
    return this.http.get<AudioDbAlbum[]>(`${this.baseUrl}/album`, { params });
  }

  /**
   * Busca a discografia (Geralmente lista simplificada com capas).
   * Endpoint: GET /discography?artistName=...
   */
  getDiscography(artistName: string): Observable<AudioDbAlbum[]> {
    const params = new HttpParams().set('artistName', artistName);
    return this.http.get<AudioDbAlbum[]>(`${this.baseUrl}/discography`, { params });
  }

  /**
   * Busca os álbuns mais populares (Most Loved).
   * Endpoint: GET /popular-albums
   */
  getPopularAlbums(): Observable<AudioDbAlbum[]> {
    return this.http.get<AudioDbAlbum[]>(`${this.baseUrl}/popular-albums`);
  }

  // =================================================================
  // 3. MÉTODOS DE MÚSICA (TRACKS)
  // =================================================================

  /**
   * Busca músicas por nome (query) e opcionalmente filtra por artista.
   * Endpoint: GET /track?query=...&artist=...
   */
  searchTrack(query: string, artist?: string): Observable<AudioDbTrack[]> {
    let params = new HttpParams().set('query', query);
    
    if (artist && artist.trim().length > 0) {
      params = params.set('artist', artist);
    }

    return this.http.get<AudioDbTrack[]>(`${this.baseUrl}/track`, { params });
  }

  /**
   * Busca todas as faixas de um álbum específico.
   * Endpoint: GET /tracks-by-album?albumId=...
   */
  getTracksByAlbumId(albumId: number): Observable<AudioDbTrack[]> {
    const params = new HttpParams().set('albumId', albumId);
    return this.http.get<AudioDbTrack[]>(`${this.baseUrl}/tracks-by-album`, { params });
  }

  /**
   * Busca as músicas mais populares (Most Loved).
   * Endpoint: GET /popular-tracks
   */
  getPopularTracks(): Observable<AudioDbTrack[]> {
    return this.http.get<AudioDbTrack[]>(`${this.baseUrl}/popular-tracks`);
  }
}