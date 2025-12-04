import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'warning' | 'danger' | 'info';
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="confirm-dialog">
      <div class="dialog-header" [class]="'type-' + data.type">
        <mat-icon>{{ getIcon() }}</mat-icon>
        <h2 mat-dialog-title>{{ data.title }}</h2>
      </div>
      
      <mat-dialog-content>
        <p>{{ data.message }}</p>
      </mat-dialog-content>
      
      <mat-dialog-actions align="end">
        <button mat-button (click)="onCancel()">
          {{ data.cancelText || 'Cancelar' }}
        </button>
        <button 
          mat-raised-button 
          [color]="data.type === 'danger' ? 'warn' : 'primary'"
          (click)="onConfirm()">
          {{ data.confirmText || 'Confirmar' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .confirm-dialog {
      min-width: 350px;
      background-color: #1E1E1E;
      color: #F5F5F5;
    }

    .dialog-header {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 20px 24px 0;
      margin-bottom: 8px;
    }

    .dialog-header mat-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
    }

    .dialog-header h2 {
      margin: 0;
      font-size: 1.25rem;
      font-weight: 600;
      color: #F5F5F5;
    }

    .type-danger mat-icon {
      color: #E50914;
    }

    .type-warning mat-icon {
      color: #FF9800;
    }

    .type-info mat-icon {
      color: #2196F3;
    }

    mat-dialog-content {
      padding: 0 24px 20px;
      color: #E0E0E0;
      font-size: 0.95rem;
      line-height: 1.5;
    }

    mat-dialog-content p {
      margin: 0;
    }

    mat-dialog-actions {
      padding: 0 24px 20px;
      gap: 8px;
    }

    /* Estilização dos botões do Material */
    ::ng-deep .confirm-dialog .mat-mdc-dialog-container {
      background-color: #1E1E1E;
    }

    ::ng-deep .confirm-dialog button {
      color: #F5F5F5;
    }

    ::ng-deep .confirm-dialog .mat-mdc-button:not(.mat-mdc-raised-button) {
      color: #B3B3B3;
    }

    ::ng-deep .confirm-dialog .mat-mdc-button:not(.mat-mdc-raised-button):hover {
      background-color: rgba(255, 255, 255, 0.08);
    }
  `]
})
export class ConfirmDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) {
    // Define tipo padrão como warning
    this.data.type = this.data.type || 'warning';
  }

  getIcon(): string {
    switch (this.data.type) {
      case 'danger':
        return 'error';
      case 'warning':
        return 'warning';
      case 'info':
        return 'info';
      default:
        return 'help';
    }
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
