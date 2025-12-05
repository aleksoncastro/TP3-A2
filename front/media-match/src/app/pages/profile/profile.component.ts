import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService, UserProfileDto, UpdateProfileDto } from '../../services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  profile: UserProfileDto | null = null;
  profileForm!: FormGroup;
  isEditMode = false;
  isLoading = false;
  selectedFile: File | null = null;
  avatarPreview: string | null = null;

  ngOnInit(): void {
    this.initForm();
    this.loadProfile();
  }

  initForm(): void {
    this.profileForm = this.fb.group({
      userName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: [''],
      bio: ['', Validators.maxLength(500)]
    });
  }

  loadProfile(): void {
    this.isLoading = true;
    this.authService.getProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        // Guardar a URL original do servidor (não construir URL completa aqui)
        this.avatarPreview = profile.profilePictureUrl || null;
        this.profileForm.patchValue({
          userName: profile.userName,
          email: profile.email,
          phoneNumber: profile.phoneNumber || '',
          bio: profile.bio || ''
        });
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.snackBar.open('Erro ao carregar perfil', 'Fechar', { duration: 3000 });
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  toggleEditMode(): void {
    if (this.isEditMode) {
      // Cancelar edição - restaurar valores originais
      this.profileForm.patchValue({
        userName: this.profile?.userName,
        email: this.profile?.email,
        phoneNumber: this.profile?.phoneNumber || '',
        bio: this.profile?.bio || ''
      });
      this.selectedFile = null;
      this.avatarPreview = this.profile?.profilePictureUrl || null;
    }
    this.isEditMode = !this.isEditMode;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      
      // Validar tipo de arquivo
      if (!file.type.startsWith('image/')) {
        this.snackBar.open('Por favor, selecione uma imagem', 'Fechar', { duration: 3000 });
        return;
      }

      // Validar tamanho (5MB max)
      if (file.size > 5 * 1024 * 1024) {
        this.snackBar.open('Imagem muito grande. Máximo 5MB', 'Fechar', { duration: 3000 });
        return;
      }

      this.selectedFile = file;

      // Preview da imagem
      const reader = new FileReader();
      reader.onload = (e) => {
        this.avatarPreview = e.target?.result as string;
        this.cdr.markForCheck();
      };
      reader.readAsDataURL(file);
    }
  }

  saveProfile(): void {
    if (this.profileForm.invalid) {
      this.snackBar.open('Por favor, corrija os erros no formulário', 'Fechar', { duration: 3000 });
      return;
    }

    this.isLoading = true;

    // Primeiro, atualizar avatar se houver arquivo selecionado
    if (this.selectedFile) {
      this.authService.updateProfilePicture(this.selectedFile).subscribe({
        next: (response) => {
          // Atualizar o preview com a URL do servidor
          this.avatarPreview = response.profilePictureUrl;
          // Atualizar o perfil também
          if (this.profile) {
            this.profile.profilePictureUrl = response.profilePictureUrl;
          }
          this.selectedFile = null;
          this.cdr.markForCheck();
          this.updateProfileData();
        },
        error: (err) => {
          this.snackBar.open('Erro ao atualizar foto de perfil', 'Fechar', { duration: 3000 });
          this.isLoading = false;
          this.cdr.markForCheck();
        }
      });
    } else {
      this.updateProfileData();
    }
  }

  private updateProfileData(): void {
    const dto: UpdateProfileDto = this.profileForm.value;
    
    this.authService.updateProfile(dto).subscribe({
      next: (profile) => {
        this.profile = profile;
        this.isEditMode = false;
        this.selectedFile = null;
        this.isLoading = false;
        this.cdr.markForCheck();
        this.snackBar.open('Perfil atualizado com sucesso!', 'Fechar', { duration: 3000 });
      },
      error: (err) => {
        this.snackBar.open(
          err.error?.message || 'Erro ao atualizar perfil',
          'Fechar',
          { duration: 3000 }
        );
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  getUserInitials(): string {
    if (!this.profile?.userName) return '?';
    const names = this.profile.userName.split(' ');
    if (names.length >= 2) {
      return (names[0][0] + names[names.length - 1][0]).toUpperCase();
    }
    return this.profile.userName.substring(0, 2).toUpperCase();
  }

  getFormattedDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR', { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric' 
    });
  }

  getImageUrl(imageUrl?: string): string {
    if (!imageUrl) return '';
    // Se já for uma URL completa (data: ou http), retorna diretamente
    if (imageUrl.startsWith('data:') || imageUrl.startsWith('http')) {
      return imageUrl;
    }
    // Caso contrário, constrói URL completa com base API
    return `http://localhost:5042${imageUrl}`;
  }
}
