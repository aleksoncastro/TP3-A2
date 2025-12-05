import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { BehaviorSubject, Observable, of, map, scan, switchMap, tap } from 'rxjs';

import { MoviesService, TmdbMovie } from '../../services/movies.service';

type MovieCollectionType = 'popular' | 'upcoming' | 'top_rated' | 'trending_day' | 'trending_week' | 'search';

interface SearchState {
  query: string;
  includeAdult: boolean;
  language: string;
  region?: string;
  year?: string;
  primaryReleaseYear?: number;
}

interface CollectionOption {
  type: Exclude<MovieCollectionType, 'search'>;
  label: string;
  icon: string;
  description: string;
}

interface FilterFormValue {
  query: string;
  includeAdult: boolean;
  language: string;
  region: string;
  year: string;
  primaryReleaseYear: string;
}

@Component({
  selector: 'app-movie-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule, MatProgressSpinnerModule, ReactiveFormsModule],
  templateUrl: './movie-list.component.html',
  styleUrls: ['./movie-list.component.css']
})
export class MovieListComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private moviesService = inject(MoviesService);
  private fb = inject(NonNullableFormBuilder);

  private readonly page$ = new BehaviorSubject<number>(1);
  private readonly type$ = new BehaviorSubject<MovieCollectionType>('popular');

  public movies$!: Observable<TmdbMovie[]>;
  public pageTitle = 'Filmes';
  public loadingMore = false;
  public canLoadMore = true;
  public filterError: string | null = null;
  public searchBadgeText: string | null = null;
  public currentCollectionType: MovieCollectionType = 'popular';

  private activeSearch: SearchState | null = null;
  private lastNonSearchType: Exclude<MovieCollectionType, 'search'> = 'popular';

  private readonly initialFilters: FilterFormValue = {
    query: '',
    includeAdult: false,
    language: 'pt-BR',
    region: '',
    year: '',
    primaryReleaseYear: ''
  };

  public readonly filterForm = this.fb.group({
    query: this.initialFilters.query,
    includeAdult: this.initialFilters.includeAdult,
    language: this.initialFilters.language,
    region: this.initialFilters.region,
    year: this.initialFilters.year,
    primaryReleaseYear: this.initialFilters.primaryReleaseYear
  });

  public readonly collectionOptions: CollectionOption[] = [
    { type: 'popular', label: 'Populares', icon: 'local_fire_department', description: 'Os favoritos do momento' },
    { type: 'upcoming', label: 'Em breve', icon: 'event', description: 'Lançamentos programados' },
    { type: 'top_rated', label: 'Melhores notas', icon: 'star', description: 'Aclamados pela crítica' },
    { type: 'trending_day', label: 'Tendência (24h)', icon: 'bolt', description: 'Bombando hoje' },
    { type: 'trending_week', label: 'Tendência (7d)', icon: 'rocket', description: 'Queridinhos da semana' }
  ];

  public readonly languageOptions = [
    { label: 'Português (Brasil)', value: 'pt-BR' },
    { label: 'Inglês (EUA)', value: 'en-US' },
    { label: 'Espanhol (Espanha)', value: 'es-ES' },
    { label: 'Francês', value: 'fr-FR' },
    { label: 'Alemão', value: 'de-DE' }
  ];

  public readonly regionOptions = [
    { label: 'Global', value: '' },
    { label: 'Brasil', value: 'BR' },
    { label: 'Estados Unidos', value: 'US' },
    { label: 'Reino Unido', value: 'GB' },
    { label: 'Canadá', value: 'CA' },
    { label: 'Japão', value: 'JP' }
  ];

  ngOnInit(): void {
    const initialType = (this.route.snapshot.data?.['type'] as MovieCollectionType) ?? 'popular';
    if (initialType !== 'search') {
      this.lastNonSearchType = initialType;
    }

    this.movies$ = this.type$.pipe(
      switchMap(type => {
        this.currentCollectionType = type;
        if (type !== 'search') {
          this.lastNonSearchType = type;
          this.searchBadgeText = null;
          this.activeSearch = null;
        }
        this.updatePageTitle(type);
        this.page$.next(1);
        return this.page$.pipe(
          switchMap(page => this.fetchMovies(type, page).pipe(map(batch => ({ page, batch })))),
          tap(({ batch }) => {
            this.loadingMore = false;
            this.canLoadMore = batch.length > 0;
          }),
          scan((acc, cur) => (cur.page === 1 ? cur.batch : acc.concat(cur.batch)), [] as TmdbMovie[])
        );
      })
    );

    if (initialType !== 'popular') {
      this.type$.next(initialType);
    }
  }

  public switchCollection(type: Exclude<MovieCollectionType, 'search'>): void {
    if (this.currentCollectionType === type) {
      return;
    }
    this.filterError = null;
    this.searchBadgeText = null;
    this.activeSearch = null;
    this.type$.next(type);
  }

  public applyFilters(): void {
    const raw = this.filterForm.getRawValue();
    const trimmedQuery = raw.query.trim();

    if (trimmedQuery.length < 2) {
      this.filterError = 'Digite pelo menos 2 caracteres para realizar a busca.';
      return;
    }

    const trimmedRegion = raw.region.trim();
    const trimmedYear = raw.year.trim();
    const trimmedPrimaryYear = raw.primaryReleaseYear.trim();
    const parsedPrimaryYear = trimmedPrimaryYear ? Number(trimmedPrimaryYear) : undefined;

    if (parsedPrimaryYear !== undefined && (Number.isNaN(parsedPrimaryYear) || trimmedPrimaryYear.length !== 4)) {
      this.filterError = 'Informe o ano de lançamento com 4 dígitos (ex: 1999).';
      return;
    }

    this.filterError = null;

    this.activeSearch = {
      query: trimmedQuery,
      includeAdult: raw.includeAdult,
      language: raw.language,
      region: trimmedRegion || undefined,
      year: trimmedYear || undefined,
      primaryReleaseYear: parsedPrimaryYear
    };

    this.searchBadgeText = this.buildSearchBadge(this.activeSearch);
    this.type$.next('search');
  }

  public resetFilters(): void {
    this.filterForm.setValue({ ...this.initialFilters });
    this.filterError = null;
    this.searchBadgeText = null;
    this.activeSearch = null;
    this.type$.next(this.lastNonSearchType);
  }

  public clearFilterError(): void {
    this.filterError = null;
  }

  public getImageUrl(path: string | null): string {
    return path ? `https://image.tmdb.org/t/p/w342${path}` : 'assets/placeholder.svg';
  }

  public formatRating(vote: number | undefined): string {
    return vote ? vote.toFixed(1) : 'N/A';
  }

  public loadMore(): void {
    if (this.loadingMore || !this.canLoadMore) {
      return;
    }
    this.loadingMore = true;
    const next = this.page$.getValue() + 1;
    this.page$.next(next);
  }

  public onImgError(ev: Event): void {
    const img = ev.target as HTMLImageElement;
    img.src = 'assets/placeholder.svg';
  }

  public isActiveCollection(type: Exclude<MovieCollectionType, 'search'>): boolean {
    return this.currentCollectionType === type;
  }

  private fetchMovies(type: MovieCollectionType, page: number): Observable<TmdbMovie[]> {
    switch (type) {
      case 'popular':
        return this.moviesService.getPopular('pt-BR', page).pipe(map(r => r.results));
      case 'upcoming':
        return this.moviesService.getUpcoming('pt-BR', page).pipe(map(r => r.results));
      case 'top_rated':
        return this.moviesService.getTopRated('pt-BR', page).pipe(map(r => r.results));
      case 'trending_day':
        return this.moviesService.getTrending('pt-BR', page, undefined, 'day').pipe(map(r => r.results));
      case 'trending_week':
        return this.moviesService.getTrending('pt-BR', page, undefined, 'week').pipe(map(r => r.results));
      case 'search':
        if (!this.activeSearch) {
          return of([]);
        }
        return this.moviesService
          .search(
            this.activeSearch.query,
            this.activeSearch.includeAdult,
            this.activeSearch.language,
            this.activeSearch.primaryReleaseYear,
            page,
            this.activeSearch.region,
            this.activeSearch.year
          )
          .pipe(map(r => r.results));
      default:
        return this.moviesService.getPopular('pt-BR', page).pipe(map(r => r.results));
    }
  }

  private updatePageTitle(type: MovieCollectionType): void {
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
      case 'trending_day':
        this.pageTitle = 'Tendências do Dia';
        break;
      case 'trending_week':
        this.pageTitle = 'Tendências da Semana';
        break;
      case 'search':
        this.pageTitle = this.activeSearch ? `Resultados para "${this.activeSearch.query}"` : 'Resultados da Busca';
        break;
      default:
        this.pageTitle = 'Filmes';
        break;
    }
  }

  private buildSearchBadge(search: SearchState): string {
    const parts: string[] = [search.query];
    parts.push(this.resolveLanguageLabel(search.language));
    if (search.region) {
      parts.push(this.resolveRegionLabel(search.region));
    }
    if (search.year) {
      parts.push(`Lançamento: ${search.year}`);
    }
    if (search.primaryReleaseYear) {
      parts.push(`Ano primário: ${search.primaryReleaseYear}`);
    }
    if (search.includeAdult) {
      parts.push('18+');
    }
    return parts.join(' · ');
  }

  private resolveLanguageLabel(value: string): string {
    return this.languageOptions.find(lang => lang.value === value)?.label ?? value;
  }

  private resolveRegionLabel(value: string): string {
    return this.regionOptions.find(region => region.value === value)?.label ?? value;
  }
}
