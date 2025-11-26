import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BrnNavigationMenuImports } from '@spartan-ng/brain/navigation-menu';
import { BrnButtonImports } from '@spartan-ng/brain/button';
import { BrnLabelImports } from '@spartan-ng/brain/label';
import { SearchService } from '../../services/search.service'; //
import { Subject, Observable, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, map } from 'rxjs/operators';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, BrnNavigationMenuImports, BrnButtonImports, BrnLabelImports],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent {
  private readonly router = inject(Router);
  private readonly searchService = inject(SearchService);

  q = '';
  private searchSubject = new Subject<string>();
  
  // Observable que conterá os resultados da busca instantânea
  results$: Observable<any[]> = this.searchSubject.pipe(
    debounceTime(300), // Espera 300ms após o usuário parar de digitar
    distinctUntilChanged(), // Evita buscar se o texto for igual ao anterior
    switchMap((term) => {
      if (!term || term.length < 2) return of([]); // Não busca se tiver menos de 2 letras
      // Usa o searchMulti para trazer filmes e séries juntos
      return this.searchService.searchMulti(term).pipe(
        map((response: any) => response.results || [])
      );
    })
  );

  // Chamado a cada tecla digitada no input
  onKeyUp(event: KeyboardEvent) {
    this.searchSubject.next(this.q);
  }

  // Chamado ao clicar em um resultado do autocomplete
  goToDetails(item: any) {
    this.q = ''; // Limpa a busca
    this.searchSubject.next(''); // Limpa resultados

    // Verifica se é filme ou série para navegar corretamente
    const route = item.media_type === 'tv' ? '/serie' : '/movie';
    this.router.navigate([route, item.id]);
  }

  // Chamado ao pressionar Enter (Busca completa)
  onSearch() {
    this.searchSubject.next(''); // Fecha o autocomplete
    
    if (this.q) {
      // Como é uma busca global, idealmente você teria uma rota '/search'.
      // Por enquanto, mantive sua lógica, mas enviando para '/movie' (ajuste conforme sua rota de resultados)
      const queryParams = { q: this.q, page: 1 };
      this.router.navigate(['/movie'], { queryParams });
    }
  }
}