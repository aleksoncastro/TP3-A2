import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable, switchMap, map, BehaviorSubject, scan, tap } from 'rxjs';

import { MoviesService, TmdbMovie } from '../../services/movies.service';

@Component({
  selector: 'app-movie-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './movie-list.component.html',
  styleUrls: ['./movie-list.component.css']
})
export class MovieListComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private moviesService = inject(MoviesService);

  public movies$!: Observable<TmdbMovie[]>;
  public pageTitle: string = 'Filmes';
  public loadingMore = false;
  public canLoadMore = true;
  private page$ = new BehaviorSubject<number>(1);
  private currentType: 'popular' | 'upcoming' | 'top_rated' | 'search' | 'default' = 'default';

  ngOnInit(): void {
    // Reage automaticamente a mudanças na rota ou parâmetros
    this.movies$ = this.route.data.pipe(
      switchMap(data => {
        const type = (data['type'] as any) ?? 'popular';
        this.currentType = type;
        switch (type) {
          case 'popular':
            this.pageTitle = 'Filmes Populares';
            break;
          case 'upcoming':
            this.pageTitle = 'Em Breve nos Cinemas';
            break;
          case 'top_rated':
            this.pageTitle = 'Melhores Avaliados';
            break;
          default:
            this.pageTitle = 'Filmes';
            this.currentType = 'popular';
            break;
        }
        this.page$.next(1);
        return this.page$.pipe(
          switchMap(page => {
            switch (this.currentType) {
              case 'popular':
                return this.moviesService.getPopular('pt-BR', page).pipe(map(r => ({ page, batch: r.results })));
              case 'upcoming':
                return this.moviesService.getUpcoming('pt-BR', page).pipe(map(r => ({ page, batch: r.results })));
              case 'top_rated':
                return this.moviesService.getTopRated('pt-BR', page).pipe(map(r => ({ page, batch: r.results })));
              default:
                return this.moviesService.getPopular('pt-BR', page).pipe(map(r => ({ page, batch: r.results })));
            }
          }),
          tap(({ batch }) => {
            this.loadingMore = false;
            this.canLoadMore = batch.length > 0;
          }),
          scan((acc, cur) => cur.page === 1 ? cur.batch : acc.concat(cur.batch), [] as TmdbMovie[])
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
