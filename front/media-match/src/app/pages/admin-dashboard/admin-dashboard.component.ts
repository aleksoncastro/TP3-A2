import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent {
  readonly authService = inject(AuthService);

  adminCards = [
    {
      title: 'Gerenciar Usuários',
      description: 'Visualize e gerencie todos os usuários do sistema',
      icon: 'people',
      route: '/admin'
    },
    {
      title: 'Gerenciar Clubes',
      description: 'Administre todos os clubes da plataforma',
      icon: 'groups',
      route: '/clubs'
    },
    {
      title: 'Moderação de Conteúdo',
      description: 'Modere posts e comentários reportados',
      icon: 'flag',
      route: '/clubs'
    }
  ];
}
