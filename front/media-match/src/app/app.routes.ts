import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { DetailComponent } from './pages/detail/detail.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'movie/:id', component: DetailComponent, data: { kind: 'movie' } },
  { path: 'serie/:id', component: DetailComponent, data: { kind: 'serie' } },
  { path: '**', redirectTo: '' }
];
