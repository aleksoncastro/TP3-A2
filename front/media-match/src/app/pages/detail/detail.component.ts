import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { BrnSeparatorImports } from '@spartan-ng/brain/separator';
import { MoviesService } from '../../services/movies.service';
import { SeriesService } from '../../services/series.service';
import { SoundtrackService, SoundtrackDto } from '../../services/soundtrack.service';
import { Observable, switchMap, map, shareReplay } from 'rxjs';
import { Title } from '@angular/platform-browser';

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

  kind!: Kind;
  id!: number;

  details$!: Observable<any>;
  credits$!: Observable<any>;
  soundtrack$!: Observable<SoundtrackDto>;

  ngOnInit(): void {
    this.details$ = this.route.data.pipe(
      switchMap((d) => {
        this.kind = (d['kind'] as Kind) ?? 'movie';
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

    this.credits$ = this.route.data.pipe(
      switchMap((d) => {
        const kind = (d['kind'] as Kind) ?? 'movie';
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

    this.soundtrack$ = this.route.data.pipe(
      switchMap((d) => {
        const kind = (d['kind'] as Kind) ?? 'movie';
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

  posterUrl(path: string | null | undefined): string {
    return path ? `https://image.tmdb.org/t/p/w300${path}` : 'https://via.placeholder.com/300x450?text=Sem+Imagem';
  }

  backdropUrl(path: string | null | undefined): string {
    return path ? `https://image.tmdb.org/t/p/w1280${path}` : '';
  }

  spotifyEmbed(albumId?: string): string | null {
    return albumId ? `https://open.spotify.com/embed/album/${albumId}` : null;
  }
}
