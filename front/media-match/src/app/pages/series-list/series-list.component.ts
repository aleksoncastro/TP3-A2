import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable, switchMap, map, BehaviorSubject, scan, tap } from 'rxjs';

import { SeriesService, TmdbTv } from '../../services/series.service';

@Component({
  selector: 'app-serie-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './series-list.component.html',
  styleUrls: ['./series-list.component.css']
})
export class SeriesListComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private seriesService = inject(SeriesService);

  public series$!: Observable<TmdbTv[]>;
  public pageTitle: string = 'Séries';
  public loadingMore = false;
  public canLoadMore = true;
  private page$ = new BehaviorSubject<number>(1);
  private currentType: 'popular' | 'top_rated' | 'airing_today' | 'on_the_air' | 'default' = 'default';

  ngOnInit(): void {
    // Reage automaticamente à mudança de rota
    this.series$ = this.route.data.pipe(
      switchMap(data => {
        const type = (data['type'] as any) ?? 'popular';
        this.currentType = type;
        switch (type) {
          case 'popular':
            this.pageTitle = 'Séries Populares';
            break;
          case 'top_rated':
            this.pageTitle = 'Séries Bem Avaliadas';
            break;
          case 'airing_today':
            this.pageTitle = 'Exibidas Hoje';
            break;
          case 'on_the_air':
            this.pageTitle = 'No Ar Atualmente';
            break;
          default:
            this.pageTitle = 'Séries';
            this.currentType = 'popular';
            break;
        }
        this.page$.next(1);
        return this.page$.pipe(
          switchMap(page => {
            switch (this.currentType) {
              case 'popular':
                return this.seriesService.getPopular('pt-BR', page).pipe(map(r => ({ page, batch: r.results })));
              case 'top_rated':
                return this.seriesService.getTopRated('pt-BR', page).pipe(map(r => ({ page, batch: r.results })));
              case 'airing_today':
                return this.seriesService.getAiringToday('pt-BR', page).pipe(map(r => ({ page, batch: r.results })));
              case 'on_the_air':
                return this.seriesService.getOnTheAir('pt-BR', page).pipe(map(r => ({ page, batch: r.results })));
              default:
                return this.seriesService.getPopular('pt-BR', page).pipe(map(r => ({ page, batch: r.results })));
            }
          }),
          tap(({ batch }) => {
            this.loadingMore = false;
            this.canLoadMore = batch.length > 0;
          }),
          scan((acc, cur) => cur.page === 1 ? cur.batch : acc.concat(cur.batch), [] as TmdbTv[])
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
