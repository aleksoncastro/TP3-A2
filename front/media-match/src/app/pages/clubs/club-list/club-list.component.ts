import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { ClubService } from '../../../services/club.service';
import { Club } from '../../../models/club.model';

@Component({
  selector: 'app-club-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSelectModule
  ],
  templateUrl: './club-list.component.html',
  styleUrl: './club-list.component.css'
})
export class ClubListComponent implements OnInit {
  clubs: Club[] = [];
  myClubs: Club[] = [];
  managedClubs: Club[] = [];
  searchTerm: string = '';
  loading: boolean = true;
  viewMode: 'all' | 'member' | 'owner' = 'all';
  sortBy: string = 'newest';
  sortOrder: string = 'desc';

  sortOptions = [
    { value: 'newest', label: 'Mais Recentes' },
    { value: 'oldest', label: 'Mais Antigos' },
    { value: 'name', label: 'Nome (A-Z)' },
    { value: 'members', label: 'Mais Membros' }
  ];

  constructor(
    private clubService: ClubService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  getSortLabel(): string {
    const option = this.sortOptions.find(o => o.value === this.sortBy);
    return option ? option.label : 'Mais Recentes';
  }

  ngOnInit(): void {
    this.loadClubs();
    // Não carrega myClubs/managedClubs automaticamente - só quando usuário clicar nas abas
    // Isso evita erro 500 se o token existir mas for inválido
  }

  loadClubs(): void {
    this.clubService.getClubs(this.searchTerm, 1, 20, this.sortBy, this.sortOrder).subscribe({
      next: (clubs) => {
        this.clubs = clubs;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erro ao carregar clubes:', error);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onSortChange(): void {
    this.loading = true;
    this.loadClubs();
  }

  loadMyClubs(): void {
    // Verifica se há token antes de tentar carregar
    const token = localStorage.getItem('token');
    if (!token) {
      this.loading = false;
      this.myClubs = [];
      this.managedClubs = [];
      alert('Você precisa estar autenticado para ver seus clubes. Por favor, faça login.');
      this.viewMode = 'all';
      this.cdr.detectChanges();
      return;
    }

    this.clubService.getMyClubs().subscribe({
      next: (clubs) => {
        // Separa clubes em que é membro vs dono
        this.myClubs = clubs.filter(c => c.isMember && !c.isOwner);
        this.managedClubs = clubs.filter(c => c.isOwner);
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erro ao carregar meus clubes:', error);
        this.loading = false;
        // Se houver erro (token inválido, não autenticado, etc), mostra mensagem
        if (error.status === 401 || error.status === 500) {
          this.myClubs = [];
          this.managedClubs = [];
          alert('Você precisa estar autenticado para ver seus clubes. Por favor, faça login.');
          // Volta para a visualização de todos os clubes
          this.viewMode = 'all';
        }
        this.cdr.detectChanges();
      }
    });
  }

  onSearch(): void {
    this.loading = true;
    this.loadClubs();
  }

  switchView(mode: 'all' | 'member' | 'owner'): void {
    this.viewMode = mode;
    // Se trocar para "Meus Clubes" ou "Gerenciando" e ainda não carregou, carrega agora
    if ((mode === 'member' || mode === 'owner') && this.myClubs.length === 0 && this.managedClubs.length === 0) {
      this.loading = true;
      this.loadMyClubs();
    }
  }

  navigateToClub(clubId: number): void {
    this.router.navigate(['/clubs', clubId]);
  }

  navigateToCreateClub(): void {
    this.router.navigate(['/clubs/new']);
  }

  joinClub(club: Club, event: Event): void {
    event.stopPropagation();
    this.clubService.joinClub(club.id).subscribe({
      next: () => {
        club.isMember = true;
        club.membersCount++;
        // Reload clubs to update the list
        this.loadClubs();
        const token = localStorage.getItem('token');
        if (token) {
          this.loadMyClubs();
        }
      },
      error: (error) => {
        console.error('Erro ao entrar no clube:', error);
        alert('Erro ao entrar no clube. Verifique se você está autenticado.');
      }
    });
  }

  leaveClub(club: Club, event: Event): void {
    event.stopPropagation();
    if (confirm('Deseja realmente sair deste clube?')) {
      this.clubService.leaveClub(club.id).subscribe({
        next: () => {
          club.isMember = false;
          club.membersCount--;
          // Reload clubs to update the list
          this.loadClubs();
          const token = localStorage.getItem('token');
          if (token) {
            this.loadMyClubs();
          }
        },
        error: (error) => {
          console.error('Erro ao sair do clube:', error);
          alert('Erro ao sair do clube.');
        }
      });
    }
  }

  getImageUrl(imageUrl?: string): string {
    if (!imageUrl) {
      return 'assets/images/club-placeholder.jpg';
    }
    return `http://localhost:5042${imageUrl}`;
  }

  get displayedClubs(): Club[] {
    switch (this.viewMode) {
      case 'member':
        return this.myClubs;
      case 'owner':
        return this.managedClubs;
      default:
        return this.clubs;
    }
  }

  get emptyStateMessage(): { title: string; subtitle: string } {
    switch (this.viewMode) {
      case 'member':
        return {
          title: 'Você ainda não participa de nenhum clube',
          subtitle: 'Entre em um clube existente para começar a interagir com outros membros!'
        };
      case 'owner':
        return {
          title: 'Você ainda não gerencia nenhum clube',
          subtitle: 'Crie seu primeiro clube e comece a construir sua comunidade!'
        };
      default:
        return {
          title: 'Nenhum clube encontrado',
          subtitle: 'Tente ajustar sua busca ou crie um novo clube'
        };
    }
  }
}
