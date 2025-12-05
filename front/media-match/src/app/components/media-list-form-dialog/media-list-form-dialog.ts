import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
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
    MatButtonModule,
    MatCheckboxModule,
    MatSnackBarModule
  ],
  template: `
    <h2 mat-dialog-title>{{ isEditing ? 'Editar Lista' : 'Nova Lista' }}</h2>
    <mat-dialog-content>
      <div class="app-input-wrapper">
        <label class="app-input-label" for="listNameInput">Nome da Lista</label>
        <div class="app-input-inner">
          <input
            id="listNameInput"
            type="text"
            [(ngModel)]="formData.name"
            maxlength="100"
            required
            placeholder="Informe um nome memorável para a lista"
          >
        </div>
        <div class="app-input-hint">{{ formData.name.length }}/100</div>
      </div>

      <div class="app-input-wrapper">
        <label class="app-input-label" for="listDescriptionInput">Descrição</label>
        <div class="app-input-inner app-textarea-inner">
          <textarea
            id="listDescriptionInput"
            [(ngModel)]="formData.description"
            rows="4"
            maxlength="500"
            placeholder="Conte aos membros sobre a proposta desta lista"
          ></textarea>
        </div>
        <div class="app-input-hint">{{ formData.description.length }}/500</div>
      </div>

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
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .app-input-hint {
      color: #B3B3B3;
    }

    /* Dark Theme Overrides */
    ::ng-deep .mat-mdc-dialog-container .mdc-dialog__surface {
      background-color: #141414 !important;
      border: 1px solid #333;
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
  private readonly dialogRef = inject(MatDialogRef<MediaListFormDialogComponent>);
  readonly data = inject<{ clubId: number; list?: MediaList }>(MAT_DIALOG_DATA);
  private readonly mediaListService = inject(MediaListService);
  private readonly snackBar = inject(MatSnackBar);
  formData: CreateMediaListDto | UpdateMediaListDto = {
    name: '',
    description: '',
    isPublic: true
  };
  isEditing = false;
  saving = false;

  constructor() {
    const existingList = this.data.list;
    if (existingList) {
      this.isEditing = true;
      this.formData = {
        name: existingList.name,
        description: existingList.description,
        isPublic: existingList.isPublic
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
