import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable, switchMap, map } from 'rxjs';

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

  ngOnInit(): void {
    // Reage automaticamente a mudanças na rota ou parâmetros
    this.movies$ = this.route.data.pipe(
      switchMap(data => {
        const type = data['type'];
        
        switch (type) {
          case 'popular':
            this.pageTitle = 'Filmes Populares';
            return this.moviesService.getPopular().pipe(map(r => r.results));
            
          case 'upcoming':
            this.pageTitle = 'Em Breve nos Cinemas';
            return this.moviesService.getUpcoming().pipe(map(r => r.results));
            
          case 'top_rated':
            this.pageTitle = 'Melhores Avaliados';
            return this.moviesService.getTopRated().pipe(map(r => r.results));
            
          case 'search':
            this.pageTitle = 'Resultados da Busca';
            return this.route.queryParams.pipe(
              switchMap(params => {
                const query = params['q'] || '';
                this.pageTitle = query ? `Resultados para: "${query}"` : 'Busca';
                return this.moviesService.search(query).pipe(map(r => r.results));
              })
            );

          default:
            this.pageTitle = 'Filmes';
            return this.moviesService.getPopular().pipe(map(r => r.results));
        }
      })
    );
  }

  getImageUrl(path: string | null): string {
    // Usando w342 para ser mais leve que o original w500
    return path ? `https://image.tmdb.org/t/p/w342${path}` : 'assets/placeholder.png';
  }

  formatRating(vote: number | undefined): string {
    return vote ? vote.toFixed(1) : 'N/A';
  }
}