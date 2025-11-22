using Microsoft.EntityFrameworkCore;
using MediaMatch.Models;
using MediaMatch.Models.TMDB; // Assumindo que moveu as classes para cá
using MediaMatch.Models.TADB; // Assumindo que moveu as classes para cá

namespace MediaMatch.Data
{
    public class MediaMatchContext : DbContext
    {
        public MediaMatchContext(DbContextOptions<MediaMatchContext> options)
            : base(options)
        {
        }

        // ========================================================================
        // 1. TABELAS (DBSETS)
        // ========================================================================

        // --- Auth ---
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        // --- TMDB (Filmes/Séries) ---
        public DbSet<MediaItem> MediaItems { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<MediaGenre> MediaGenres { get; set; } // Tabela de Junção
        public DbSet<Person> People { get; set; }
        public DbSet<Credit> Credits { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<ClubMember> ClubMembers { get; set; }
        public DbSet<MediaList> MediaLists { get; set; }
        public DbSet<MediaListItem> MediaListItems { get; set; }

        // --- TADB (Música) ---
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<MusicGenre> MusicGenres { get; set; }
        public DbSet<ArtistGenre> ArtistGenres { get; set; } // Tabela de Junção

        // --- Cross-Reference (Mídia + Música) ---
        public DbSet<MediaSoundtrack> MediaSoundtracks { get; set; } // Tabela de Junção Principal

        // ========================================================================
        // 2. CONFIGURAÇÕES DE RELACIONAMENTO (FLUENT API)
        // ========================================================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ------------------------------------------------------------
            // 1. AUTENTICAÇÃO (UserRole)
            // ------------------------------------------------------------
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId }); // Chave Composta

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // ------------------------------------------------------------
            // 2. TMDB - FILMES E SÉRIES
            // ------------------------------------------------------------

            // MediaGenre (N:N)
            modelBuilder.Entity<MediaGenre>()
                .HasKey(mg => new { mg.MediaItemId, mg.GenreId }); // Chave Composta

            modelBuilder.Entity<MediaGenre>()
                .HasOne(mg => mg.MediaItem)
                .WithMany(m => m.MediaGenres)
                .HasForeignKey(mg => mg.MediaItemId);

            modelBuilder.Entity<MediaGenre>()
                .HasOne(mg => mg.Genre)
                .WithMany(g => g.MediaGenres)
                .HasForeignKey(mg => mg.GenreId);

            // Cascade Delete: Apagar Série -> Apaga Temporadas
            modelBuilder.Entity<Season>()
                .HasOne(s => s.MediaItem)
                .WithMany(m => m.Seasons)
                .HasForeignKey(s => s.MediaItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cascade Delete: Apagar Temporada -> Apaga Episódios
            modelBuilder.Entity<Episode>()
                .HasOne(e => e.Season)
                .WithMany(s => s.Episodes)
                .HasForeignKey(e => e.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------------------------------------------------
            // 3. TADB - MÚSICA
            // ------------------------------------------------------------

            // ArtistGenre (N:N)
            modelBuilder.Entity<ArtistGenre>()
                .HasKey(ag => new { ag.ArtistId, ag.MusicGenreId }); // Chave Composta

            modelBuilder.Entity<ArtistGenre>()
                .HasOne(ag => ag.Artist)
                .WithMany(a => a.ArtistGenres)
                .HasForeignKey(ag => ag.ArtistId);

            modelBuilder.Entity<ArtistGenre>()
                .HasOne(ag => ag.MusicGenre)
                .WithMany(mg => mg.ArtistGenres)
                .HasForeignKey(ag => ag.MusicGenreId);

            // Cascade Delete: Apagar Artista -> Apaga Álbuns
            modelBuilder.Entity<Album>()
                .HasOne(a => a.Artist)
                .WithMany(art => art.Albums)
                .HasForeignKey(a => a.ArtistId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cascade Delete: Apagar Álbum -> Apaga Faixas
            modelBuilder.Entity<Track>()
                .HasOne(t => t.Album)
                .WithMany(a => a.Tracks)
                .HasForeignKey(t => t.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------------------------------------------------
            // 4. CROSS-REFERENCE (MediaSoundtrack)
            // ------------------------------------------------------------

            // Chave Composta: Impede duplicar a mesma música no mesmo filme
            modelBuilder.Entity<MediaSoundtrack>()
                .HasKey(ms => new { ms.MediaItemId, ms.TrackId });

            modelBuilder.Entity<MediaSoundtrack>()
                .HasOne(ms => ms.MediaItem)
                .WithMany() // Assume-se unidirecional ou adicione ICollection<MediaSoundtrack> em MediaItem
                .HasForeignKey(ms => ms.MediaItemId);

            modelBuilder.Entity<MediaSoundtrack>()
                .HasOne(ms => ms.Track)
                .WithMany(t => t.MediaSoundtracks)
                .HasForeignKey(ms => ms.TrackId);

            // RELAÇÃO COM USUÁRIO (Quem adicionou)
            // Use Restrict para evitar que deletar um usuário quebre o histórico de trilhas sonoras
            modelBuilder.Entity<MediaSoundtrack>()
                .HasOne(ms => ms.User)
                .WithMany()
                .HasForeignKey(ms => ms.AddedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================================
            // CONFIGURAÇÃO DE CLUBES
            // ============================================================

            // ClubMember (N:N entre User e Club)
            modelBuilder.Entity<ClubMember>()
                .HasKey(cm => new { cm.ClubId, cm.UserId });

            modelBuilder.Entity<ClubMember>()
                .HasOne(cm => cm.Club)
                .WithMany(c => c.Members)
                .HasForeignKey(cm => cm.ClubId)
                .OnDelete(DeleteBehavior.Cascade); // Se apagar o clube, remove as associações de membros

            modelBuilder.Entity<ClubMember>()
                .HasOne(cm => cm.User)
                .WithMany() // Adicione ICollection<ClubMember> em User se quiser navegar User -> Clubes
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Evita apagar User e levar o clube junto incorretamente

            // Dono do Clube
            modelBuilder.Entity<Club>()
                .HasOne(c => c.Owner)
                .WithMany()
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================================
            // CONFIGURAÇÃO DE LISTAS (MediaList)
            // ============================================================

            // Lista pertencente a Usuário
            modelBuilder.Entity<MediaList>()
                .HasOne(ml => ml.User)
                .WithMany() // Adicione ICollection<MediaList> em User se quiser
                .HasForeignKey(ml => ml.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Lista pertencente a Clube
            modelBuilder.Entity<MediaList>()
                .HasOne(ml => ml.Club)
                .WithMany(c => c.MediaLists)
                .HasForeignKey(ml => ml.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            // Itens da Lista
            modelBuilder.Entity<MediaListItem>()
                .HasOne(i => i.MediaList)
                .WithMany(l => l.Items)
                .HasForeignKey(i => i.MediaListId)
                .OnDelete(DeleteBehavior.Cascade); // Apagar lista apaga os itens

            modelBuilder.Entity<MediaListItem>()
                .HasOne(i => i.MediaItem)
                .WithMany()
                .HasForeignKey(i => i.MediaItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}