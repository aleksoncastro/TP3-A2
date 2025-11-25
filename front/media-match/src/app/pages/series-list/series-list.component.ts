import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable, switchMap, map } from 'rxjs';

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

  ngOnInit(): void {
    // Reage automaticamente à mudança de rota
    this.series$ = this.route.data.pipe(
      switchMap(data => {
        const type = data['type']; // Configurado no app.routes.ts
        
        switch (type) {
          case 'popular':
            this.pageTitle = 'Séries Populares';
            return this.seriesService.getPopular().pipe(map(r => r.results));
            
          case 'top_rated':
            this.pageTitle = 'Séries Bem Avaliadas';
            return this.seriesService.getTopRated().pipe(map(r => r.results));
            
          case 'airing_today':
            this.pageTitle = 'Exibidas Hoje';
            return this.seriesService.getAiringToday().pipe(map(r => r.results));
            
          case 'on_the_air':
            this.pageTitle = 'No Ar Atualmente';
            return this.seriesService.getOnTheAir().pipe(map(r => r.results));
            
          case 'search':
            this.pageTitle = 'Resultados da Busca';
            // Escuta os queryParams (?q=...)
            return this.route.queryParams.pipe(
              switchMap(params => {
                const query = params['q'] || '';
                this.pageTitle = query ? `Resultados para: "${query}"` : 'Busca de Séries';
                // O método search espera: q, includeAdult, language, firstAirYear, page
                return this.seriesService.search(query).pipe(map(r => r.results));
              })
            );

          default:
            this.pageTitle = 'Séries';
            return this.seriesService.getPopular().pipe(map(r => r.results));
        }
      })
    );
  }

  getImageUrl(path: string | null): string {
    // w342: Qualidade boa e leve para grids
    return path ? `https://image.tmdb.org/t/p/w342${path}` : 'assets/placeholder.png';
  }

  formatRating(vote: number | undefined): string {
    return vote ? vote.toFixed(1) : 'N/A';
  }
}