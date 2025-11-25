import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Observable, map, shareReplay } from 'rxjs';

// --- Angular Material Imports ---
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

// --- Services e Interfaces ---
import { MoviesService, TmdbMovie } from '../../services/movies.service';
import { SeriesService, TmdbTv } from '../../services/series.service';

@Component({
  selector: 'app-home',
  standalone: true,
  // Importante: Adicionei MatIconModule e MatButtonModule aqui
  imports: [CommonModule, RouterLink, MatIconModule, MatButtonModule], 
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  // --- Injeção de Dependências ---
  private moviesService = inject(MoviesService);
  private seriesService = inject(SeriesService);

  // --- Observables de Filmes ---
  public popularMovies$!: Observable<TmdbMovie[]>;
  public upcomingMovies$!: Observable<TmdbMovie[]>;
  public trendingMovies$!: Observable<TmdbMovie[]>;

  // --- Observables de Séries ---
  public popularSeries$!: Observable<TmdbTv[]>;
  public topRatedSeries$!: Observable<TmdbTv[]>;
  public airingTodaySeries$!: Observable<TmdbTv[]>;

  ngOnInit(): void {
    this.initMovieStreams();
    this.initSeriesStreams();
  }

  // --- Lógica de Inicialização ---

  private initMovieStreams(): void {
    this.popularMovies$ = this.moviesService.getPopular('pt-BR').pipe(
      map(res => res.results), 
      shareReplay(1)
    );

    this.upcomingMovies$ = this.moviesService.getUpcoming('pt-BR').pipe(
      map(res => res.results),
      shareReplay(1)
    );

    this.trendingMovies$ = this.moviesService.getTrending('pt-BR', 1, undefined, 'week').pipe(
      map(res => res.results),
      shareReplay(1)
    );
  }

  private initSeriesStreams(): void {
    this.popularSeries$ = this.seriesService.getPopular('pt-BR').pipe(
      map(res => res.results),
      shareReplay(1)
    );

    this.topRatedSeries$ = this.seriesService.getTopRated('pt-BR').pipe(
      map(res => res.results),
      shareReplay(1)
    );

    this.airingTodaySeries$ = this.seriesService.getAiringToday('pt-BR').pipe(
      map(res => res.results),
      shareReplay(1)
    );
  }

  // --- Métodos Auxiliares para o HTML ---

  /**
   * Monta a URL completa da imagem do TMDB.
   */
  getImageUrl(path: string | null | undefined): string {
    if (!path) {
      return 'assets/placeholder-image.png'; // Caminho para uma imagem de fallback, crie se necessário
    }
    return `https://image.tmdb.org/t/p/w500${path}`;
  }

  /**
   * Formata a nota (ex: 7.843 -> 7.8).
   * Obs: Se a interface TmdbTv não tiver vote_average, o TS pode reclamar. 
   * Certifique-se de atualizar a interface no service se necessário.
   */
  formatRating(vote: number | undefined): string {
    return vote ? vote.toFixed(1) : 'N/A';
  }

  /**
   * Lógica de Scroll do Carrossel
   * Recebe o elemento HTML direto do template variable (#ref)
   */
  scrollCarousel(element: HTMLElement, direction: 'left' | 'right'): void {
    const scrollAmount = element.clientWidth * 0.8; // Rola 80% da largura visível
    
    if (direction === 'left') {
      element.scrollBy({ left: -scrollAmount, behavior: 'smooth' });
    } else {
      element.scrollBy({ left: scrollAmount, behavior: 'smooth' });
    }
  }
}