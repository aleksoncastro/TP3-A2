import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MediaListService } from '../../services/media-list.service';
import { MediaList, CreateMediaListDto, UpdateMediaListDto } from '../../models/media-list.model';

@Component({
  selector: 'app-media-list-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatSnackBarModule
  ],
  template: `
    <h2 mat-dialog-title>{{ isEditing ? 'Editar Lista' : 'Nova Lista' }}</h2>
    <mat-dialog-content>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Nome da Lista</mat-label>
        <input matInput [(ngModel)]="formData.name" maxlength="100" required>
        <mat-hint align="end">{{ formData.name.length }}/100</mat-hint>
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Descrição</mat-label>
        <textarea 
          matInput 
          [(ngModel)]="formData.description" 
          rows="4" 
          maxlength="500"></textarea>
        <mat-hint align="end">{{ formData.description.length }}/500</mat-hint>
      </mat-form-field>

      <mat-checkbox [(ngModel)]="formData.isPublic">
        Lista pública (visível para todos os membros)
      </mat-checkbox>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancelar</button>
      <button 
        mat-raised-button 
        color="primary" 
        (click)="onSave()" 
        [disabled]="!formData.name.trim() || saving">
        {{ saving ? 'Salvando...' : 'Salvar' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    :host {
      display: block;
      background: #141414;
      color: #F5F5F5;
    }

    h2[mat-dialog-title] {
      color: #E50914;
      font-family: 'Montserrat', sans-serif;
      font-weight: 700;
      background: #1E1E1E;
      margin: 0;
      padding: 24px;
      border-bottom: 1px solid #333;
    }

    mat-dialog-content {
      min-width: 400px;
      padding: 24px !important;
      background: #141414;
    }

    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    /* Dark Theme Overrides */
    ::ng-deep .mat-mdc-dialog-container .mdc-dialog__surface {
      background-color: #141414 !important;
      border: 1px solid #333;
    }

    ::ng-deep .mat-mdc-text-field-wrapper {
      background-color: #1E1E1E !important;
    }

    ::ng-deep .mat-mdc-form-field .mat-mdc-input-element {
      color: #F5F5F5 !important;
    }

    ::ng-deep .mat-mdc-form-field .mat-mdc-floating-label {
      color: #B3B3B3 !important;
    }

    ::ng-deep .mat-mdc-form-field.mat-focused .mat-mdc-floating-label {
      color: #E50914 !important;
    }

    ::ng-deep .mat-mdc-checkbox .mdc-checkbox .mdc-checkbox__native-control:enabled:checked~.mdc-checkbox__background {
      background-color: #E50914 !important;
      border-color: #E50914 !important;
    }

    ::ng-deep .mat-mdc-checkbox .mdc-label {
      color: #F5F5F5 !important;
    }

    mat-dialog-actions {
      background: #1E1E1E;
      padding: 16px 24px !important;
      border-top: 1px solid #333;
      margin: 0 !important;
    }

    button[mat-raised-button] {
      background-color: #E50914 !important;
      color: white !important;
    }

    button[mat-button] {
      color: #B3B3B3 !important;
    }
  `]
})
export class MediaListFormDialogComponent {
  formData: CreateMediaListDto | UpdateMediaListDto = {
    name: '',
    description: '',
    isPublic: true
  };
  isEditing = false;
  saving = false;

  constructor(
    public dialogRef: MatDialogRef<MediaListFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { clubId: number; list?: MediaList },
    private mediaListService: MediaListService,
    private snackBar: MatSnackBar
  ) {
    if (data.list) {
      this.isEditing = true;
      this.formData = {
        name: data.list.name,
        description: data.list.description,
        isPublic: data.list.isPublic
      };
    }
  }

  onCancel() {
    this.dialogRef.close();
  }

  onSave() {
    if (!this.formData.name.trim()) {
      return;
    }

    this.saving = true;

    if (this.isEditing && this.data.list) {
      this.mediaListService.updateList(this.data.clubId, this.data.list.id, this.formData).subscribe({
        next: () => {
          this.snackBar.open('Lista atualizada com sucesso', 'Fechar', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: (error: any) => {
          console.error('Erro ao atualizar lista:', error);
          this.snackBar.open('Erro ao atualizar lista', 'Fechar', { duration: 3000 });
          this.saving = false;
        }
      });
    } else {
      this.mediaListService.createList(this.data.clubId, this.formData).subscribe({
        next: () => {
          this.snackBar.open('Lista criada com sucesso', 'Fechar', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: (error: any) => {
          console.error('Erro ao criar lista:', error);
          this.snackBar.open('Erro ao criar lista', 'Fechar', { duration: 3000 });
          this.saving = false;
        }
      });
    }
  }
}
