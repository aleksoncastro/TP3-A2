import { Component, inject, OnInit, ViewChildren, QueryList, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AudioDbService, AudioDbTrack } from '../../services/audiodbservice.service'; 
import { catchError, finalize, of } from 'rxjs';

// Interface auxiliar para agrupar as faixas
interface TrackGroup {
  title: string;
  tracks: AudioDbTrack[];
}

@Component({
  selector: 'app-music-list',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatRippleModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './music-list.component.html',
  styleUrls: ['./music-list.component.css']
})
export class MusicListComponent implements OnInit {
  private service = inject(AudioDbService);

  trackGroups: TrackGroup[] = [];
  isLoading = true;

  @ViewChildren('scrollContainer') scrollContainers!: QueryList<ElementRef>;

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    
    this.service.getPopularTracks()
      .pipe(
        finalize(() => this.isLoading = false),
        catchError((err) => {
          console.error('Erro ao carregar:', err);
          return of({ results: [] }); // Retorna objeto vazio compatível com a estrutura
        })
      )
      .subscribe((response: any) => {
        // === CORREÇÃO AQUI ===
        // O backend retorna { page: 1, results: [...] }
        // Precisamos acessar a propriedade .results
        const tracks = response.results || [];
        
        if (Array.isArray(tracks)) {
          this.organizeData(tracks);
        } else {
          console.error('Formato de dados inválido:', response);
        }
      });
  }

  private organizeData(tracks: AudioDbTrack[]) {
    // Agora 'tracks' é garantidamente um array, o slice vai funcionar
    const highlights: TrackGroup = {
      title: 'Início (Destaques)',
      tracks: tracks.slice(0, 8) 
    };

    const genreMap: { [key: string]: AudioDbTrack[] } = {};
    
    tracks.forEach(track => {
      const genre = track.strGenre || 'Outros';
      if (!genreMap[genre]) genreMap[genre] = [];
      genreMap[genre].push(track);
    });

    const genreGroups: TrackGroup[] = Object.keys(genreMap)
      .sort()
      .map(key => ({
        title: key,
        tracks: genreMap[key]
      }));

    this.trackGroups = [highlights, ...genreGroups];
  }

  scroll(index: number, direction: 'left' | 'right') {
    // Verificação de segurança caso o container ainda não exista
    if (!this.scrollContainers || !this.scrollContainers.toArray()[index]) return;

    const container = this.scrollContainers.toArray()[index].nativeElement;
    const scrollAmount = 600;

    if (direction === 'left') {
      container.scrollBy({ left: -scrollAmount, behavior: 'smooth' });
    } else {
      container.scrollBy({ left: scrollAmount, behavior: 'smooth' });
    }
  }

  playTrack(track: AudioDbTrack) {
    if(track.strMusicVid) window.open(track.strMusicVid, '_blank');
  }

  // Helper para imagem (caso não tenha thumb, usa placeholder)
  getImage(track: AudioDbTrack): string {
    return track.strTrackThumb || track.strTrackThumb || 'assets/placeholder-music.png';
  }
}