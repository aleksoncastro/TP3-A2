import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MoviesService } from '../../services/movies.service';
import { Observable, switchMap } from 'rxjs';

@Component({
  selector: 'app-collection-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './collection.component.html',
  styleUrls: ['./collection.component.css']
})
export class CollectionDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly moviesSvc = inject(MoviesService);

  details$!: Observable<any>;

  ngOnInit() {
    this.details$ = this.route.paramMap.pipe(
      switchMap((params) => this.moviesSvc.getColletion(Number(params.get('id'))))
    );
  }

  posterUrl(path: string | null | undefined): string {
    return path ? `https://image.tmdb.org/t/p/w300${path}` : 'https://via.placeholder.com/300x450?text=Sem+Imagem';
  }

  backdropUrl(path: string | null | undefined): string {
    return path ? `https://image.tmdb.org/t/p/w1280${path}` : '';
  }

  heroBgUrl(col: any): string {
    const p = col?.backdrop_path || col?.poster_path || col?.parts?.[0]?.backdrop_path || col?.parts?.[0]?.poster_path || null;
    return p ? `https://image.tmdb.org/t/p/w1280${p}` : 'https://via.placeholder.com/1280x720?text=Sem+Imagem';
  }
}

