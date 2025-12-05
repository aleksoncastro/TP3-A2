import { Component, OnInit, Input, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MediaListService } from '../../services/media-list.service';
import { MediaList } from '../../models/media-list.model';
import { MediaListFormDialogComponent } from '../media-list-form-dialog/media-list-form-dialog';
import { MediaListDetailDialogComponent } from '../media-list-detail-dialog/media-list-detail-dialog';

@Component({
  selector: 'app-media-lists',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule,
    MatTooltipModule
  ],
  templateUrl: './media-lists.html',
  styleUrl: './media-lists.css'
})
export class MediaListsComponent implements OnInit {
  @Input() clubId!: number;
  @Input() canManage = false;

  lists: MediaList[] = [];
  loading = true;

  constructor(
    private mediaListService: MediaListService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadLists();
  }

  loadLists() {
    this.loading = true;
    this.cdr.detectChanges();
    this.mediaListService.getClubLists(this.clubId).subscribe({
      next: (lists) => {
        this.lists = lists;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erro ao carregar listas:', error);
        this.snackBar.open('Erro ao carregar listas', 'Fechar', { duration: 3000 });
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openCreateDialog() {
    const dialogRef = this.dialog.open(MediaListFormDialogComponent, {
      width: '500px',
      data: { clubId: this.clubId }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadLists();
      }
    });
  }

  openListDetail(list: MediaList) {
    const dialogRef = this.dialog.open(MediaListDetailDialogComponent, {
      width: '900px',
      maxHeight: '90vh',
      data: { clubId: this.clubId, listId: list.id, canManage: list.canEdit }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === 'updated' || result === 'deleted') {
        this.loadLists();
      }
    });
  }

  openEditDialog(list: MediaList, event: Event) {
    event.stopPropagation();
    const dialogRef = this.dialog.open(MediaListFormDialogComponent, {
      width: '500px',
      data: { clubId: this.clubId, list }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadLists();
      }
    });
  }

  deleteList(list: MediaList, event: Event) {
    event.stopPropagation();
    if (confirm(`Tem certeza que deseja deletar a lista "${list.name}"?`)) {
      this.mediaListService.deleteList(this.clubId, list.id).subscribe({
        next: () => {
          this.snackBar.open('Lista deletada com sucesso', 'Fechar', { duration: 3000 });
          this.loadLists();
        },
        error: (error) => {
          console.error('Erro ao deletar lista:', error);
          this.snackBar.open('Erro ao deletar lista', 'Fechar', { duration: 3000 });
        }
      });
    }
  }
}
