import { Component, ElementRef, HostListener, ViewChild, inject, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SearchService } from '../../services/search.service';
import { BrnNavigationMenuImports } from '@spartan-ng/brain/navigation-menu';
import { BrnButtonImports } from '@spartan-ng/brain/button';
import { BrnLabelImports } from '@spartan-ng/brain/label';
import { Subject, Observable, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, map, tap } from 'rxjs/operators';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    BrnNavigationMenuImports,
    BrnButtonImports,
    BrnLabelImports,
  ],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnDestroy {
  private readonly router = inject(Router);
  private readonly searchService = inject(SearchService);
  private readonly cdr = inject(ChangeDetectorRef); // <--- Injeção necessária para forçar atualização da tela

  q = '';
  private searchSubject = new Subject<string>();
  
  // Referência ao container que engloba o input e o dropdown
  @ViewChild('searchContainer') searchContainer?: ElementRef;
  @ViewChild('dropdownPanel') dropdownPanel?: ElementRef;
  @ViewChild('searchInput') searchInput?: ElementRef;
  
  isDropdownOpen = false;
  
  // Observable de resultados
  results$: Observable<any[]> = this.searchSubject.pipe(
    debounceTime(300),
    distinctUntilChanged(),
    switchMap((term) => {
      if (!term || term.length < 2) {
        this.isDropdownOpen = false;
        return of([]);
      }
      return this.searchService.searchMulti(term).pipe(
        map((response: any) => response.results || [])
      );
    }),
    tap((results) => {
      // Abre o dropdown automaticamente se houver resultados
      this.isDropdownOpen = results.length > 0;
      this.cdr.markForCheck(); // Garante que o Angular detecte a abertura
    })
  );

  // --- LÓGICA DE FECHAR AO CLICAR FORA ---
  
  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    const target = event.target as Node;
    const panelEl = this.dropdownPanel?.nativeElement as HTMLElement | undefined;
    const inputEl = this.searchInput?.nativeElement as HTMLElement | undefined;
    const insidePanel = !!(panelEl && panelEl.contains(target));
    const insideInput = !!(inputEl && inputEl.contains(target));
    if (!insidePanel && !insideInput) {
      if (this.isDropdownOpen) {
        this.isDropdownOpen = false;
        this.cdr.markForCheck();
      }
    }
  }

  // Opcional: Para suportar melhor mobile (touch)
  @HostListener('document:touchstart', ['$event'])
  onTouch(event: TouchEvent) {
    const target = event.target as Node;
    const panelEl = this.dropdownPanel?.nativeElement as HTMLElement | undefined;
    const inputEl = this.searchInput?.nativeElement as HTMLElement | undefined;
    const insidePanel = !!(panelEl && panelEl.contains(target));
    const insideInput = !!(inputEl && inputEl.contains(target));
    if (!insidePanel && !insideInput) {
      if (this.isDropdownOpen) {
        this.isDropdownOpen = false;
        this.cdr.markForCheck();
      }
    }
  }

  // ---------------------------------------

  onKeyUp(event: KeyboardEvent) {
    this.searchSubject.next(this.q);
  }

  goToDetails(item: any) {
    this.q = ''; 
    this.searchSubject.next(''); 
    this.isDropdownOpen = false; 
    const route = item.media_type === 'tv' ? '/serie' : '/movie'; 
    this.router.navigate([route, item.id]);
  }

  onSearch() {
    this.searchSubject.next(''); 
    this.isDropdownOpen = false; 
    
    if (this.q) {
      const queryParams = { q: this.q, page: 1 };
      this.router.navigate(['/search'], { queryParams });
    }
  }

  onImgError(ev: Event) {
    const img = ev.target as HTMLImageElement;
    img.src = 'assets/placeholder.svg';
  }

  onEnter(event: Event) {
    event.preventDefault();
    this.searchSubject.next(this.q);
  }

  ngOnDestroy(): void {
    // Lifecycle hook mantido para boas práticas
  }
}
