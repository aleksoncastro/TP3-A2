import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { DomSanitizer, SafeResourceUrl, Title } from '@angular/platform-browser'; // Importações adicionadas
import { Observable, switchMap, map, shareReplay } from 'rxjs';

// Seus imports de serviço
import { MoviesService } from '../../services/movies.service';
import { SeriesService } from '../../services/series.service';
import { SoundtrackService, SoundtrackDto } from '../../services/soundtrack.service';
import { BrnSeparatorImports } from '@spartan-ng/brain/separator';

type Kind = 'movie' | 'serie';

@Component({
  selector: 'app-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, BrnSeparatorImports],
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class DetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly movies = inject(MoviesService);
  private readonly series = inject(SeriesService);
  private readonly soundtrack = inject(SoundtrackService);
  private readonly titleSvc = inject(Title);
  private readonly sanitizer = inject(DomSanitizer); // Injeção do Sanitizer

  kind!: Kind;
  id!: number;

  details$!: Observable<any>;
  credits$!: Observable<any>;
  soundtrack$!: Observable<SoundtrackDto>;

 ngOnInit(): void {
  this.details$ = this.route.data.pipe(
    switchMap((d) => {

      // ✅ Validação correta para 'movie' e 'serie'
      const validKinds: Kind[] = ['movie', 'serie'];
      const k = d['kind'] as string;
      this.kind = validKinds.includes(k as Kind) ? (k as Kind) : 'movie';

      return this.route.paramMap.pipe(
        switchMap((p) => {
          this.id = Number(p.get('id'));
          if (this.kind === 'movie') {
            return this.movies.getDetails(this.id).pipe(
              map((det) => {
                this.titleSvc.setTitle(`MediaMatch • ${det.title}`);
                return det;
              })
            );
          }
          return this.series.getDetails(this.id).pipe(
            map((det) => {
              this.titleSvc.setTitle(`MediaMatch • ${det.name}`);
              return det;
            })
          );
        })
      );
    }),
    shareReplay(1)
  );

  // créditos
  this.credits$ = this.route.data.pipe(
    switchMap((d) => {
      const validKinds: Kind[] = ['movie', 'serie'];
      const k = d['kind'] as string;
      const kind = validKinds.includes(k as Kind) ? (k as Kind) : 'movie';

      return this.route.paramMap.pipe(
        switchMap((p) => {
          const id = Number(p.get('id'));
          if (kind === 'movie') return this.movies.getCredits(id);
          return this.series.getCredits(id);
        })
      );
    }),
    shareReplay(1)
  );

  // soundtrack
  this.soundtrack$ = this.route.data.pipe(
    switchMap((d) => {
      const validKinds: Kind[] = ['movie', 'serie'];
      const k = d['kind'] as string;
      const kind = validKinds.includes(k as Kind) ? (k as Kind) : 'movie';

      return this.route.paramMap.pipe(
        switchMap((p) => {
          const id = Number(p.get('id'));
          if (kind === 'movie') return this.soundtrack.getMovieSoundtrack(id);
          return this.soundtrack.getTvSoundtrack(id);
        })
      );
    })
  );
}


  // --- HELPERS DE IMAGEM ---

  posterUrl(path: string | null | undefined): string {
    return path ? `https://image.tmdb.org/t/p/w300${path}` : 'https://via.placeholder.com/300x450?text=No+Poster';
  }

  backdropUrl(path: string | null | undefined): string {
    return path ? `https://image.tmdb.org/t/p/w1280${path}` : '';
  }

  // Nova função para imagem dos atores
  getProfileUrl(path: string | null | undefined): string {
    // w185 é um tamanho bom para avatares
    return path ? `https://image.tmdb.org/t/p/w185${path}` : 'assets/avatar-placeholder.png'; 
    // Dica: Crie uma imagem avatar-placeholder.png em src/assets ou use uma URL externa temporária
  }

 

  spotifyEmbed(albumId?: string): SafeResourceUrl | null {
    if (!albumId) return null;

    // URL Oficial de Embed do Spotify (Não precisa de chave, apenas do ID)
    // Exemplo de ID: 41MnTivkwTO3UUJ8DrqEJJ
    const url = `https://open.spotify.com/embed/album/${albumId}?utm_source=generator&theme=0`;
    
    // O sanitizer permite que o Angular confie nessa URL no iframe
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  }
}