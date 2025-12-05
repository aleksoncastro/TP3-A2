import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { AdminGuard } from './guards/admin.guard';

export const routes: Routes = [
  // { path: '', loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent) },
  // { path: 'collection/:id', loadComponent: () => import('./pages/collection/collection.component').then(m => m.CollectionDetailComponent) },
  // { path: '**', redirectTo: '' }
  {path: '', component: HomeComponent, title: 'home'},
  { path: 'serie/:id', loadComponent: () => import('./pages/detail/detail.component').then(m => m.DetailComponent), data: { kind: 'serie' } },
  { path: 'movie/:id', loadComponent: () => import('./pages/detail/detail.component').then(m => m.DetailComponent), data: { kind: 'movie' } },
  { path: 'movie', loadComponent: () => import('./pages/movie-list/movie-list.component').then(m => m.MovieListComponent) },
  { path: 'search', loadComponent: () => import('./pages/search-results/search-results.component').then(m => m.SearchResultsComponent) },
  { path: 'series', loadComponent: () => import('./pages/series-list/series-list.component').then(m => m.SeriesListComponent) },
  { path: 'music', loadComponent: () => import('./pages/music-list/music-list.component').then(m => m.MusicListComponent) },

  {
    path: 'auth',
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'login' },
      { path: 'login', loadComponent: () => import('./auth/components/login/login.component').then(m => m.LoginComponent) },
      { path: 'register', loadComponent: () => import('./auth/components/register/register.component').then(m => m.RegisterComponent) },
      { path: 'forgot-password', loadComponent: () => import('./auth/components/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent) },
      { path: 'reset-password', loadComponent: () => import('./auth/components/reset-password/reset-password.component').then(m => m.ResetPasswordComponent) },
    ],
  },
  // Profile route
  { path: 'perfil', loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent) },
  
  // Admin routes
  { path: 'admin', loadComponent: () => import('./pages/admin-users/admin-users.component').then(m => m.AdminUsersComponent), canMatch: [AdminGuard] },
  { path: 'admin/dashboard', loadComponent: () => import('./pages/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent), canMatch: [AdminGuard] },
  
  // Club routes
  { path: 'clubs', loadComponent: () => import('./pages/clubs/club-list/club-list.component').then(m => m.ClubListComponent) },
  { path: 'clubs/new', loadComponent: () => import('./pages/clubs/club-form/club-form').then(m => m.ClubFormComponent) },
  { path: 'clubs/:id', loadComponent: () => import('./pages/clubs/club-detail/club-detail').then(m => m.ClubDetailComponent) },
  { path: 'clubs/:id/edit', loadComponent: () => import('./pages/clubs/club-form/club-form').then(m => m.ClubFormComponent) },
  
  // Redirects
  { path: 'login', redirectTo: 'auth/login' },
  { path: 'register', redirectTo: 'auth/register' },
  { path: 'forgot-password', redirectTo: 'auth/forgot-password' },
  { path: 'reset-password', redirectTo: 'auth/reset-password' },
];
