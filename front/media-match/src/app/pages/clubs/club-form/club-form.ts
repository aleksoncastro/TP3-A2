import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ClubService } from '../../../services/club.service';
import { CreateClubDto, UpdateClubDto } from '../../../models/club.model';

@Component({
  selector: 'app-club-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './club-form.html',
  styleUrl: './club-form.css'
})
export class ClubFormComponent implements OnInit {
  clubForm!: FormGroup;
  selectedFile?: File;
  imagePreview?: string;
  isEditMode = false;
  clubId?: number;
  loading = false;
  existingImageUrl?: string;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private clubService: ClubService
  ) {}

  ngOnInit(): void {
    this.clubForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)]
    });

    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.clubId = +params['id'];
        this.loadClub();
      }
    });
  }

  loadClub(): void {
    if (!this.clubId) return;

    this.loading = true;
    this.clubService.getClubById(this.clubId).subscribe({
      next: (club) => {
        this.clubForm.patchValue({
          name: club.name,
          description: club.description
        });
        this.existingImageUrl = club.imageUrl;
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar clube:', error);
        this.loading = false;
        this.router.navigate(['/clubs']);
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file && file.type.startsWith('image/')) {
      this.selectedFile = file;
      
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imagePreview = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  removeImage(): void {
    this.selectedFile = undefined;
    this.imagePreview = undefined;
  }

  onSubmit(): void {
    if (this.clubForm.invalid) return;

    this.loading = true;

    if (this.isEditMode && this.clubId) {
      const dto: UpdateClubDto = {
        name: this.clubForm.value.name,
        description: this.clubForm.value.description,
        removeImage: !this.selectedFile && !this.existingImageUrl
      };

      this.clubService.updateClub(this.clubId, dto, this.selectedFile).subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/clubs', this.clubId]);
        },
        error: (error) => {
          console.error('Erro ao atualizar clube:', error);
          this.loading = false;
        }
      });
    } else {
      const dto: CreateClubDto = {
        name: this.clubForm.value.name,
        description: this.clubForm.value.description
      };

      this.clubService.createClub(dto, this.selectedFile).subscribe({
        next: (club) => {
          this.loading = false;
          this.router.navigate(['/clubs', club.id]);
        },
        error: (error) => {
          console.error('Erro ao criar clube:', error);
          this.loading = false;
        }
      });
    }
  }

  getImageUrl(url?: string): string {
    if (!url) return '';
    return `http://localhost:5042${url}`;
  }
}
