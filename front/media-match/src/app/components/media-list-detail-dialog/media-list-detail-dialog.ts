import { Component, Inject, OnInit, ChangeDetectorRef, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, Observable, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, map, tap } from 'rxjs/operators';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MediaListService } from '../../services/media-list.service';
import { SearchService } from '../../services/search.service';
import { MediaListDetail, CreateMediaListCommentDto } from '../../models/media-list.model';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-media-list-detail-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatChipsModule
  ],
  templateUrl: './media-list-detail-dialog.html',
  styleUrl: './media-list-detail-dialog.css'
})
export class MediaListDetailDialogComponent implements OnInit, OnDestroy {
  @ViewChild('searchInput') searchInput?: ElementRef;
  list?: MediaListDetail;
  loading = true;
  
  // Search
  searchQuery = '';
  searchResults: any[] = [];
  searching = false;
  isSearchDropdownOpen = false;
  private searchSubject = new Subject<string>();
  searchResults$!: Observable<any[]>;
  
  // Comments
  newCommentContent = '';
  commentType: 'comment' | 'suggestion' = 'comment';
  selectedSuggestion: any = null;
  
  // Adding item
  addingItem = false;
  itemNote = '';

  constructor(
    public dialogRef: MatDialogRef<MediaListDetailDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { clubId: number; listId: number; canManage: boolean },
    private mediaListService: MediaListService,
    private searchService: SearchService,
    private snackBar: MatSnackBar,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.loadListDetail();
    this.initializeSearch();
  }

  initializeSearch() {
    this.searchResults$ = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      tap(() => this.searching = true),
      switchMap((term) => {
        if (!term || term.length < 2) {
          this.isSearchDropdownOpen = false;
          this.searching = false;
          return of([]);
        }
        return this.searchService.searchMulti(term).pipe(
          map((response: any) => {
            const results = (response.results || []).filter((r: any) => 
              r.media_type === 'movie' || r.media_type === 'tv'
            );
            this.searching = false;
            return results;
          })
        );
      }),
      tap((results) => {
        this.isSearchDropdownOpen = results.length > 0;
        this.cdr.detectChanges();
      })
    );
  }

  loadListDetail() {
    this.loading = true;
    this.cdr.detectChanges();
    this.mediaListService.getListDetail(this.data.clubId, this.data.listId).subscribe({
      next: (list: MediaListDetail) => {
        this.list = list;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Erro ao carregar lista:', error);
        this.snackBar.open('Erro ao carregar lista', 'Fechar', { duration: 3000 });
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onSearchInput() {
    this.searchSubject.next(this.searchQuery);
  }

  closeSearchDropdown() {
    this.isSearchDropdownOpen = false;
    this.cdr.detectChanges();
  }

  selectMediaFromDropdown(media: any) {
    if (this.commentType === 'suggestion') {
      this.selectForSuggestion(media);
    } else {
      this.addItemToList(media);
    }
    this.searchQuery = '';
    this.searchSubject.next('');
    this.closeSearchDropdown();
  }

  getPosterUrl(path: string): string {
    return path ? `https://image.tmdb.org/t/p/w500${path}` : 'assets/no-poster.png';
  }

  addItemToList(media: any) {
    if (this.addingItem) return;

    this.addingItem = true;
    const dto = {
      tmdbId: media.id,
      mediaType: media.media_type,
      note: this.itemNote.trim()
    };

    this.mediaListService.addItemToList(this.data.clubId, this.data.listId, dto).subscribe({
      next: () => {
        this.snackBar.open('Item adicionado com sucesso', 'Fechar', { duration: 3000 });
        this.itemNote = '';
        this.searchQuery = '';
        this.searchResults = [];
        this.loadListDetail();
        this.addingItem = false;
      },
      error: (error: any) => {
        console.error('Erro ao adicionar item:', error);
        const message = error.error?.message || 'Erro ao adicionar item';
        this.snackBar.open(message, 'Fechar', { duration: 3000 });
        this.addingItem = false;
      }
    });
  }

  removeItem(itemId: number) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Remover item',
        message: 'Tem certeza que deseja remover este item da lista?',
        confirmText: 'Remover',
        cancelText: 'Cancelar',
        type: 'danger'
      },
      width: '400px',
      panelClass: 'dark-dialog'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.mediaListService.removeItemFromList(this.data.clubId, this.data.listId, itemId).subscribe({
          next: () => {
            this.snackBar.open('Item removido', 'Fechar', { duration: 3000 });
            this.loadListDetail();
          },
          error: (error: any) => {
            console.error('Erro ao remover item:', error);
            this.snackBar.open('Erro ao remover item', 'Fechar', { duration: 3000 });
          }
        });
      }
    });
  }

  selectForSuggestion(media: any) {
    this.selectedSuggestion = media;
    this.commentType = 'suggestion';
  }

  clearSuggestion() {
    this.selectedSuggestion = null;
    this.commentType = 'comment';
  }

  createComment() {
    if (!this.newCommentContent.trim()) return;

    const dto: CreateMediaListCommentDto = {
      content: this.newCommentContent,
      type: this.commentType
    };

    if (this.commentType === 'suggestion' && this.selectedSuggestion) {
      dto.suggestedMediaId = this.selectedSuggestion.id;
      dto.suggestedMediaType = this.selectedSuggestion.media_type;
      dto.suggestedMediaTitle = this.selectedSuggestion.title || this.selectedSuggestion.name;
      dto.suggestedMediaPosterPath = this.selectedSuggestion.poster_path;
    }

    this.mediaListService.createComment(this.data.clubId, this.data.listId, dto).subscribe({
      next: () => {
        this.newCommentContent = '';
        this.selectedSuggestion = null;
        this.commentType = 'comment';
        this.loadListDetail();
        this.snackBar.open('Comentário criado com sucesso!', 'Fechar', { duration: 3000 });
      },
      error: (error: any) => {
        console.error('Erro ao comentar:', error);
        const errorMsg = error.error?.message || error.message || 'Erro ao criar comentário';
        this.snackBar.open(errorMsg, 'Fechar', { duration: 5000 });
      }
    });
  }

  deleteComment(commentId: number) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Deletar comentário',
        message: 'Tem certeza que deseja deletar este comentário? Esta ação não pode ser desfeita.',
        confirmText: 'Deletar',
        cancelText: 'Cancelar',
        type: 'danger'
      },
      width: '400px',
      panelClass: 'dark-dialog'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.mediaListService.deleteComment(this.data.clubId, this.data.listId, commentId).subscribe({
          next: () => {
            this.snackBar.open('Comentário deletado', 'Fechar', { duration: 3000 });
            this.loadListDetail();
          },
          error: (error: any) => {
            console.error('Erro ao deletar comentário:', error);
            this.snackBar.open('Erro ao deletar comentário', 'Fechar', { duration: 3000 });
          }
        });
      }
    });
  }

  addSuggestedItem(comment: any) {
    if (!comment.suggestedMediaId) return;

    const media = {
      id: comment.suggestedMediaId,
      media_type: comment.suggestedMediaType,
      title: comment.suggestedMediaTitle,
      name: comment.suggestedMediaTitle,
      poster_path: comment.suggestedMediaPosterPath
    };

    this.addItemToList(media);
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('pt-BR');
  }

  getMediaTitle(media: any): string {
    return media.title || media.name || 'Sem título';
  }

  getMediaYear(media: any): string {
    const date = media.release_date || media.first_air_date;
    return date ? new Date(date).getFullYear().toString() : '';
  }

  onClose() {
    this.dialogRef.close();
  }

  ngOnDestroy() {
    this.searchSubject.complete();
  }
}
