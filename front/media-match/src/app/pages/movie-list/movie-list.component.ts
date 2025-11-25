import { Component, OnInit, inject, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner'; // Opcional: para mostrar loading
import { switchMap, tap, take } from 'rxjs';

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

  // --- Estado da Lista ---
  public movies: TmdbMovie[] = []; // Agora é um Array, não Observable
  public pageTitle: string = 'Filmes';
  
  // --- Controle de Paginação ---
  private currentPage = 1;
  private currentType: string = 'popular'; // guarda o tipo atual (popular, search, etc)
  private searchQuery: string = ''; // guarda o termo se for busca
  public isLoading = false; // evita chamadas duplicadas
  public hasMore = true; // flag para parar se a API acabar

  ngOnInit(): void {
    // Escuta a mudança de rota (ex: clicou no menu 'Filmes' vindo de 'Séries')
    this.route.data.subscribe(data => {
      this.currentType = data['type'] || 'popular';
      this.resetList(); // Limpa tudo e começa do zero
      
      // Se for busca, precisamos ler os queryParams primeiro
      if (this.currentType === 'search') {
        this.route.queryParams.subscribe(params => {
          this.searchQuery = params['q'] || '';
          this.pageTitle = this.searchQuery ? `Resultados para: "${this.searchQuery}"` : 'Busca';
          this.resetList();
          this.loadMovies();
        });
      } else {
        // Configura títulos para as rotas normais
        this.setPageTitle();
        this.loadMovies();
      }
    });
  }

  /**
   * Detecta o Scroll da janela
   */
  @HostListener('window:scroll', [])
  onScroll(): void {
    // Se já estiver carregando ou não tiver mais itens, pare.
    if (this.isLoading || !this.hasMore) return;

    // Calcula se chegou no fim da página (com uma margem de 400px antes do fim)
    const pos = (document.documentElement.scrollTop || document.body.scrollTop) + document.documentElement.offsetHeight;
    const max = document.documentElement.scrollHeight;

    if (pos >= max - 400) {
      this.loadMovies();
    }
  }

  loadMovies(): void {
    if (this.isLoading) return;
    this.isLoading = true;

    let request;

    // Seleciona qual requisição fazer baseada no tipo e na página atual
    switch (this.currentType) {
      case 'popular':
        request = this.moviesService.getPopular('pt-BR', this.currentPage);
        break;
      case 'upcoming':
        request = this.moviesService.getUpcoming('pt-BR', this.currentPage);
        break;
      case 'top_rated':
        request = this.moviesService.getTopRated('pt-BR', this.currentPage);
        break;
      case 'search':
        request = this.moviesService.search(this.searchQuery, false, 'pt-BR', undefined, this.currentPage);
        break;
      default:
        request = this.moviesService.getPopular('pt-BR', this.currentPage);
    }

    request.pipe(take(1)).subscribe({
      next: (response) => {
        // Adiciona os novos resultados ao array existente
        this.movies.push(...response.results);
        
        // Prepara para a próxima página
        this.currentPage++;
        this.isLoading = false;

        // Se a API retornar vazio, paramos de tentar carregar
        if (response.results.length === 0) {
          this.hasMore = false;
        }
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  private resetList(): void {
    this.movies = [];
    this.currentPage = 1;
    this.hasMore = true;
    this.isLoading = false;
  }

  private setPageTitle(): void {
    switch (this.currentType) {
      case 'popular': this.pageTitle = 'Filmes Populares'; break;
      case 'upcoming': this.pageTitle = 'Em Breve nos Cinemas'; break;
      case 'top_rated': this.pageTitle = 'Melhores Avaliados'; break;
    }
  }

  // Helpers de Template
  getImageUrl(path: string | null): string {
    return path ? `https://image.tmdb.org/t/p/w500${path}` : 'assets/placeholder.png';
  }

  formatRating(vote: number | undefined): string {
    return vote ? vote.toFixed(1) : 'N/A';
  }
}