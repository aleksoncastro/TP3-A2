import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';

export const routes: Routes = [
  // { path: '', loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent) },
  // { path: 'movie', loadComponent: () => import('./pages/movie-list/movie-list.component').then(m => m.MovieListComponent) },
  // { path: 'series', loadComponent: () => import('./pages/series-list/series-list.component').then(m => m.SeriesListComponent) },
  // { path: 'movie/:id', loadComponent: () => import('./pages/detail/detail.component').then(m => m.DetailComponent), data: { kind: 'movie' } },
  // { path: 'collection/:id', loadComponent: () => import('./pages/collection/collection.component').then(m => m.CollectionDetailComponent) },
  // { path: '**', redirectTo: '' }
  {path: '', component: HomeComponent, title: 'home'},
  { path: 'serie/:id', loadComponent: () => import('./pages/detail/detail.component').then(m => m.DetailComponent), data: { kind: 'serie' } },
  { path: 'movie/:id', loadComponent: () => import('./pages/detail/detail.component').then(m => m.DetailComponent), data: { kind: 'movie' } },
];
