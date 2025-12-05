import { Component, OnInit, ChangeDetectorRef, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';
import { ClubService } from '../../../services/club.service';
import { ClubDetail, Post, CreatePostDto, CreateCommentDto } from '../../../models/club.model';
import { MediaListsComponent } from '../../../components/media-lists/media-lists';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-club-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatMenuModule,
    MediaListsComponent
  ],
  templateUrl: './club-detail.html',
  styleUrl: './club-detail.css',
})
export class ClubDetailComponent implements OnInit {
  club?: ClubDetail;
  posts: Post[] = [];
  loading = true;
  clubId!: number;
  newPostContent = '';
  selectedPostImages: File[] = [];
  imagePreviews: string[] = [];
  newCommentContent: { [postId: number]: string } = {};
  expandedPosts: Set<number> = new Set();
  currentImageIndexes: { [postId: number]: number } = {};
  isAuthenticated = false;

  constructor(
    @Inject(ActivatedRoute) private route: ActivatedRoute,
    @Inject(Router) private router: Router,
    @Inject(ClubService) private clubService: ClubService,
    @Inject(ChangeDetectorRef) private cdr: ChangeDetectorRef,
    @Inject(AuthService) private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.isAuthenticated = this.authService.isAuthenticated();
    this.route.params.subscribe(params => {
      this.clubId = +params['id'];
      this.loadClub();
      this.loadPosts();
    });
  }

  loadClub(): void {
    this.clubService.getClubById(this.clubId).subscribe({
      next: (club) => {
        this.club = club;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erro ao carregar clube:', error);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadPosts(): void {
    this.clubService.getClubPosts(this.clubId).subscribe({
      next: (posts) => {
        this.posts = posts;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erro ao carregar posts:', error);
        this.cdr.detectChanges();
      }
    });
  }

  onFileSelected(event: any): void {
    const files = Array.from(event.target.files) as File[];
    const imageFiles = files.filter(file => file.type.startsWith('image/'));
    
    if (imageFiles.length > 0) {
      this.selectedPostImages = [...this.selectedPostImages, ...imageFiles].slice(0, 5); // Máximo 5 imagens
      
      // Gerar previews
      imageFiles.forEach(file => {
        const reader = new FileReader();
        reader.onload = (e: any) => {
          this.imagePreviews.push(e.target.result);
        };
        reader.readAsDataURL(file);
      });
    }
    
    // Limpar input para permitir selecionar o mesmo arquivo novamente
    event.target.value = '';
  }

  removeImage(index: number): void {
    this.selectedPostImages.splice(index, 1);
    this.imagePreviews.splice(index, 1);
  }

  createPost(): void {
    if (!this.newPostContent.trim()) return;

    const dto: CreatePostDto = {
      content: this.newPostContent
    };

    // Envia todas as imagens selecionadas
    const images = this.selectedPostImages.length > 0 ? this.selectedPostImages : undefined;

    this.clubService.createPost(this.clubId, dto, images).subscribe({
      next: () => {
        this.newPostContent = '';
        this.selectedPostImages = [];
        this.imagePreviews = [];
        this.loadPosts();
      },
      error: (error) => console.error('Erro ao criar post:', error)
    });
  }

  deletePost(postId: number): void {
    if (confirm('Deseja realmente excluir este post?')) {
      this.clubService.deletePost(this.clubId, postId).subscribe({
        next: () => this.loadPosts(),
        error: (error) => console.error('Erro ao deletar post:', error)
      });
    }
  }

  toggleComments(postId: number): void {
    if (this.expandedPosts.has(postId)) {
      this.expandedPosts.delete(postId);
    } else {
      this.expandedPosts.add(postId);
      // Carregar comentários se ainda não foram carregados
      const post = this.posts.find(p => p.id === postId);
      if (post && (!post.comments || post.comments.length === 0)) {
        this.loadComments(postId);
      }
    }
  }

  loadComments(postId: number): void {
    this.clubService.getPostComments(this.clubId, postId).subscribe({
      next: (comments) => {
        const post = this.posts.find(p => p.id === postId);
        if (post) {
          post.comments = comments;
          this.cdr.detectChanges();
        }
      },
      error: (error) => console.error('Erro ao carregar comentários:', error)
    });
  }

  createComment(postId: number): void {
    const content = this.newCommentContent[postId];
    if (!content?.trim()) return;

    const dto: CreateCommentDto = { content };

    this.clubService.createComment(this.clubId, postId, dto).subscribe({
      next: () => {
        this.newCommentContent[postId] = '';
        this.loadComments(postId);
      },
      error: (error) => console.error('Erro ao criar comentário:', error)
    });
  }

  deleteComment(postId: number, commentId: number): void {
    if (confirm('Deseja realmente excluir este comentário?')) {
      this.clubService.deleteComment(this.clubId, postId, commentId).subscribe({
        next: () => this.loadComments(postId),
        error: (error) => console.error('Erro ao deletar comentário:', error)
      });
    }
  }

  joinClub(): void {
    if (!this.isAuthenticated) {
      alert('Faça login para entrar no clube.');
      return;
    }
    this.clubService.joinClub(this.clubId).subscribe({
      next: () => this.loadClub(),
      error: (error) => console.error('Erro ao entrar no clube:', error)
    });
  }

  leaveClub(): void {
    if (confirm('Deseja realmente sair deste clube?')) {
      this.clubService.leaveClub(this.clubId).subscribe({
        next: () => this.router.navigate(['/clubs']),
        error: (error) => console.error('Erro ao sair do clube:', error)
      });
    }
  }

  getImageUrl(url?: string): string {
    if (!url) return 'assets/images/club-placeholder.jpg';
    return `http://localhost:5042${url}`;
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  isExpanded(postId: number): boolean {
    return this.expandedPosts.has(postId);
  }

  // Carousel methods
  getCurrentImageIndex(postId: number): number {
    return this.currentImageIndexes[postId] || 0;
  }

  nextImage(postId: number, totalImages: number): void {
    const currentIndex = this.getCurrentImageIndex(postId);
    this.currentImageIndexes[postId] = (currentIndex + 1) % totalImages;
  }

  previousImage(postId: number, totalImages: number): void {
    const currentIndex = this.getCurrentImageIndex(postId);
    this.currentImageIndexes[postId] = currentIndex === 0 ? totalImages - 1 : currentIndex - 1;
  }

  isCurrentUserModerator(): boolean {
    if (!this.club) return false;
    const currentUserId = this.getCurrentUserId();
    return this.club.members?.some(m => m.userId === currentUserId && m.isModerator) || false;
  }

  private getCurrentUserId(): number {
    // Assumindo que o ID do usuário está no localStorage após login
    const userStr = localStorage.getItem('user');
    if (userStr) {
      const user = JSON.parse(userStr);
      return user.id;
    }
    return 0;
  }
}
