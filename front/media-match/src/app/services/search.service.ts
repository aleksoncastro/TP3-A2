import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

const API_BASE = environment.apiBase;

@Injectable({ providedIn: 'root' })
export class SearchService {
  private readonly http = inject(HttpClient);

  private parseJson<T>(text$: Observable<string>): Observable<T> {
    return text$.pipe(map((txt) => JSON.parse(txt)));
  }

  searchMulti(q: string, includeAdult = false, page = 1): Observable<any> {
    const url = `${API_BASE}/Tmdb/multi/search?q=${encodeURIComponent(q)}&include_adult=${includeAdult}&page=${page}`;
    return this.parseJson<any>(this.http.get(url, { responseType: 'text' }));
  }
}
