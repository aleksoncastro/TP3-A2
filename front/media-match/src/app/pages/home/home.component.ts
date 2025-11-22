import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService, TmdbMovie, TmdbTv, TmdbListResponse } from '../../services/api.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  private readonly api = inject(ApiService);
  movies$!: Observable<TmdbListResponse<TmdbMovie>>;
  series$!: Observable<TmdbListResponse<TmdbTv>>;

  ngOnInit() {
    this.movies$ = this.api.getPopularMovies();
    this.series$ = this.api.getPopularSeries();
  }

  posterUrl(path: string | null): string {
    return path ? `https://image.tmdb.org/t/p/w300${path}` : 'https://via.placeholder.com/300x450?text=Sem+Imagem';
  }
}