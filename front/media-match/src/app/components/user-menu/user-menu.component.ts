import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService, UserProfileDto } from '../../services/auth.service';

@Component({
  selector: 'app-user-menu',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatMenuModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
  ],
  templateUrl: './user-menu.component.html',
  styleUrls: ['./user-menu.component.css']
})
export class UserMenuComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  userProfile: UserProfileDto | null = null;
  isAdmin = false;

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    if (this.authService.isAuthenticated()) {
      this.authService.getProfile().subscribe({
        next: (profile) => {
          this.userProfile = profile;
          this.isAdmin = this.authService.isAdmin();
          this.cdr.markForCheck();
        },
        error: () => {
          // Em caso de erro, usar dados do storage
          this.isAdmin = this.authService.isAdmin();
          this.cdr.markForCheck();
        }
      });
    }
  }

  getUserInitials(): string {
    if (!this.userProfile?.userName) return '?';
    const names = this.userProfile.userName.split(' ');
    if (names.length >= 2) {
      return (names[0][0] + names[names.length - 1][0]).toUpperCase();
    }
    return this.userProfile.userName.substring(0, 2).toUpperCase();
  }

  getImageUrl(imageUrl?: string): string {
    if (!imageUrl) return '';
    if (imageUrl.startsWith('data:') || imageUrl.startsWith('http')) {
      return imageUrl;
    }
    return `http://localhost:5042${imageUrl}`;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
