import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable, switchMap, map, BehaviorSubject, scan, tap } from 'rxjs';
import { SearchService } from '../../services/search.service';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './search-results.component.html',
  styleUrls: ['./search-results.component.css']
})
export class SearchResultsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private searchService = inject(SearchService);

  public items$!: Observable<any[]>;
  public pageTitle: string = 'Resultados da Busca';
  public loadingMore = false;
  public canLoadMore = true;
  private page$ = new BehaviorSubject<number>(1);

  ngOnInit(): void {
    this.items$ = this.route.queryParams.pipe(
      switchMap(params => {
        const query = params['q'] || '';
        this.pageTitle = query ? `Resultados para: "${query}"` : 'Busca';
        this.page$.next(1);
        return this.page$.pipe(
          switchMap(page =>
            this.searchService.searchMulti(query, false, page).pipe(
              map(r => ({ page, batch: (r.results || []).filter((it: any) => it.media_type === 'movie' || it.media_type === 'tv') })),
              tap(({ batch }) => {
                this.loadingMore = false;
                this.canLoadMore = batch.length > 0;
              })
            )
          ),
          scan((acc, cur) => cur.page === 1 ? cur.batch : acc.concat(cur.batch), [] as any[])
        );
      })
    );
  }

  getImageUrl(path: string | null): string {
    return path ? `https://image.tmdb.org/t/p/w342${path}` : 'assets/placeholder.svg';
  }

  formatRating(vote: number | undefined): string {
    return vote ? vote.toFixed(1) : 'N/A';
  }

  loadMore(): void {
    if (this.loadingMore || !this.canLoadMore) return;
    this.loadingMore = true;
    const next = this.page$.getValue() + 1;
    this.page$.next(next);
  }

  onImgError(ev: Event): void {
    const img = ev.target as HTMLImageElement;
    img.src = 'assets/placeholder.svg';
  }
}
