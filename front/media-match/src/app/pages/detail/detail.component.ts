import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, SoundtrackDto } from '../../services/api.service';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Observable, switchMap } from 'rxjs';

@Component({
  selector: 'app-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class DetailComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);

  kind: 'movie' | 'tv' = 'movie';
  id!: number;
  details$!: Observable<any>;
  soundtrack$!: Observable<SoundtrackDto>;

  ngOnInit() {
    this.details$ = this.route.paramMap.pipe(
      switchMap((params) => {
        const type = this.route.snapshot.data['kind'] as 'movie' | 'tv';
        this.kind = type;
        this.id = Number(params.get('id'));
        if (type === 'movie') {
          this.soundtrack$ = this.api.getMovieSoundtrack(this.id);
          return this.api.getMovieDetails(this.id);
        } else {
          this.soundtrack$ = this.api.getTvSoundtrack(this.id);
          return this.api.getSeriesDetails(this.id);
        }
      })
    );
  }

  posterUrl(path: string | null): string {
    return path ? `https://image.tmdb.org/t/p/w500${path}` : 'https://via.placeholder.com/500x750?text=Sem+Imagem';
  }
}