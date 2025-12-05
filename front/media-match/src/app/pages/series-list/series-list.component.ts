import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { BehaviorSubject, Observable, map, of, scan, switchMap, tap } from 'rxjs';

import { SeriesService, TmdbTv } from '../../services/series.service';

type SeriesCollectionType = 'popular' | 'top_rated' | 'airing_today' | 'on_the_air' | 'search';

interface SearchState {
  query: string;
  includeAdult: boolean;
  language: string;
  region?: string;
  firstAirYear?: number;
}

interface CollectionOption {
  type: Exclude<SeriesCollectionType, 'search'>;
  label: string;
  icon: string;
  description: string;
}

interface FilterFormValue {
  query: string;
  includeAdult: boolean;
  language: string;
  region: string;
  firstAirYear: string;
}

@Component({
  selector: 'app-serie-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule, MatProgressSpinnerModule, ReactiveFormsModule],
  templateUrl: './series-list.component.html',
  styleUrls: ['./series-list.component.css']
})
export class SeriesListComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private seriesService = inject(SeriesService);
  private fb = inject(NonNullableFormBuilder);

  private readonly page$ = new BehaviorSubject<number>(1);
  private readonly type$ = new BehaviorSubject<SeriesCollectionType>('popular');

  public series$!: Observable<TmdbTv[]>;
  public pageTitle = 'Séries';
  public loadingMore = false;
  public canLoadMore = true;
  public filterError: string | null = null;
  public searchBadgeText: string | null = null;
  public currentCollectionType: SeriesCollectionType = 'popular';

  private activeSearch: SearchState | null = null;
  private lastNonSearchType: Exclude<SeriesCollectionType, 'search'> = 'popular';

  private readonly initialFilters: FilterFormValue = {
    query: '',
    includeAdult: false,
    language: 'pt-BR',
    region: '',
    firstAirYear: ''
  };

  public readonly filterForm = this.fb.group({
    query: this.initialFilters.query,
    includeAdult: this.initialFilters.includeAdult,
    language: this.initialFilters.language,
    region: this.initialFilters.region,
    firstAirYear: this.initialFilters.firstAirYear
  });

  public readonly collectionOptions: CollectionOption[] = [
    { type: 'popular', label: 'Populares', icon: 'local_fire_department', description: 'Séries mais buscadas' },
    { type: 'top_rated', label: 'Bem avaliadas', icon: 'star', description: 'Favoritas da crítica' },
    { type: 'airing_today', label: 'Hoje na TV', icon: 'today', description: 'Capítulos inéditos' },
    { type: 'on_the_air', label: 'No ar', icon: 'live_tv', description: 'Em exibição contínua' }
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
    const initialType = (this.route.snapshot.data?.['type'] as SeriesCollectionType) ?? 'popular';
    if (initialType !== 'search') {
      this.lastNonSearchType = initialType;
    }

    this.series$ = this.type$.pipe(
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
          switchMap(page => this.fetchSeries(type, page).pipe(map(batch => ({ page, batch })))),
          tap(({ batch }) => {
            this.loadingMore = false;
            this.canLoadMore = batch.length > 0;
          }),
          scan((acc, cur) => (cur.page === 1 ? cur.batch : acc.concat(cur.batch)), [] as TmdbTv[])
        );
      })
    );

    if (initialType !== 'popular') {
      this.type$.next(initialType);
    }
  }

  public switchCollection(type: Exclude<SeriesCollectionType, 'search'>): void {
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
    const trimmedFirstAirYear = raw.firstAirYear.trim();
    const parsedFirstAirYear = trimmedFirstAirYear ? Number(trimmedFirstAirYear) : undefined;

    if (parsedFirstAirYear !== undefined && (Number.isNaN(parsedFirstAirYear) || trimmedFirstAirYear.length !== 4)) {
      this.filterError = 'Informe o ano de estreia com 4 dígitos (ex: 2015).';
      return;
    }

    this.filterError = null;

    this.activeSearch = {
      query: trimmedQuery,
      includeAdult: raw.includeAdult,
      language: raw.language,
      region: trimmedRegion || undefined,
      firstAirYear: parsedFirstAirYear
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

  public isActiveCollection(type: Exclude<SeriesCollectionType, 'search'>): boolean {
    return this.currentCollectionType === type;
  }

  private fetchSeries(type: SeriesCollectionType, page: number): Observable<TmdbTv[]> {
    switch (type) {
      case 'popular':
        return this.seriesService.getPopular('pt-BR', page).pipe(map(r => r.results));
      case 'top_rated':
        return this.seriesService.getTopRated('pt-BR', page).pipe(map(r => r.results));
      case 'airing_today':
        return this.seriesService.getAiringToday('pt-BR', page).pipe(map(r => r.results));
      case 'on_the_air':
        return this.seriesService.getOnTheAir('pt-BR', page).pipe(map(r => r.results));
      case 'search':
        if (!this.activeSearch) {
          return of([]);
        }
        return this.seriesService
          .search(
            this.activeSearch.query,
            this.activeSearch.includeAdult,
            this.activeSearch.language,
            this.activeSearch.firstAirYear,
            page,
            this.activeSearch.region
          )
          .pipe(map(r => r.results));
      default:
        return this.seriesService.getPopular('pt-BR', page).pipe(map(r => r.results));
    }
  }

  private updatePageTitle(type: SeriesCollectionType): void {
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
      case 'search':
        this.pageTitle = this.activeSearch ? `Resultados para "${this.activeSearch.query}"` : 'Resultados da Busca';
        break;
      default:
        this.pageTitle = 'Séries';
        break;
    }
  }

  private buildSearchBadge(search: SearchState): string {
    const parts: string[] = [search.query];
    parts.push(this.resolveLanguageLabel(search.language));
    if (search.region) {
      parts.push(this.resolveRegionLabel(search.region));
    }
    if (search.firstAirYear) {
      parts.push(`Estreia: ${search.firstAirYear}`);
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
