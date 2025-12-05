using MediaMatch.Exceptions;
using Microsoft.Extensions.Logging;

namespace MediaMatch.Services
{
    public class FileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly string _uploadsFolder;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;

            // Ensure WebRootPath is not null
            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _uploadsFolder = Path.Combine(webRootPath, "uploads", "clubs");

            // Create directory if it doesn't exist
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
                _logger.LogInformation("Diretório de uploads criado: {UploadsFolder}", _uploadsFolder);
            }
        }

        /// <summary>
        /// Faz upload de uma imagem para uma subfolder específica e retorna a URL relativa.
        /// </summary>
        public async Task<string> UploadImageAsync(IFormFile file, string subfolder, string? oldImageUrl = null)
        {
            try
            {
                // Validações
                ValidateFile(file);

                var targetFolder = Path.Combine(_environment.WebRootPath, "uploads", subfolder);
                
                // Criar diretório se não existir
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                    _logger.LogInformation("Diretório de uploads criado: {TargetFolder}", targetFolder);
                }

                // Deletar imagem antiga se existir
                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    await DeleteImageAsync(oldImageUrl, subfolder);
                }

                // Gerar nome único
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLower()}";
                var filePath = Path.Combine(targetFolder, fileName);

                // Salvar arquivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Retornar URL relativa
                var imageUrl = $"/uploads/{subfolder}/{fileName}";
                _logger.LogInformation("Imagem salva com sucesso: {ImageUrl}", imageUrl);
                return imageUrl;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer upload da imagem");
                throw new BusinessException("Erro ao fazer upload da imagem");
            }
        }

        /// <summary>
        /// Faz upload de uma imagem e retorna a URL relativa.
        /// </summary>
        public async Task<string> UploadImageAsync(IFormFile file, string? oldImageUrl = null)
        {
            try
            {
                // Validações
                ValidateFile(file);

                // Deletar imagem antiga se existir
                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    await DeleteImageAsync(oldImageUrl);
                }

                // Gerar nome único
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLower()}";
                var filePath = Path.Combine(_uploadsFolder, fileName);

                // Salvar arquivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Retornar URL relativa
                var imageUrl = $"/uploads/clubs/{fileName}";
                _logger.LogInformation("Imagem salva com sucesso: {ImageUrl}", imageUrl);
                return imageUrl;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer upload da imagem");
                throw new BusinessException("Erro ao fazer upload da imagem");
            }
        }

        /// <summary>
        /// Deleta uma imagem do servidor.
        /// </summary>
        public Task DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return Task.CompletedTask;

                // Extrair nome do arquivo da URL
                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_uploadsFolder, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Imagem deletada: {ImageUrl}", imageUrl);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar imagem: {ImageUrl}", imageUrl);
                // Não lançar exceção, apenas logar
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Deleta uma imagem do servidor de uma subfolder específica.
        /// </summary>
        public Task DeleteImageAsync(string imageUrl, string subfolder)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return Task.CompletedTask;

                // Extrair nome do arquivo da URL
                var fileName = Path.GetFileName(imageUrl);
                var targetFolder = Path.Combine(_environment.WebRootPath, "uploads", subfolder);
                var filePath = Path.Combine(targetFolder, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Imagem deletada: {ImageUrl}", imageUrl);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar imagem: {ImageUrl}", imageUrl);
                // Não lançar exceção, apenas logar
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Valida o arquivo de imagem.
        /// </summary>
        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ValidationException("Nenhum arquivo foi enviado");

            if (file.Length > _maxFileSize)
                throw new ValidationException($"Arquivo muito grande. Máximo permitido: {_maxFileSize / 1024 / 1024}MB");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
                throw new ValidationException($"Tipo de arquivo não permitido. Permitidos: {string.Join(", ", _allowedExtensions)}");

            // Validar MIME type
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLower()))
                throw new ValidationException("Tipo de arquivo inválido");
        }

        /// <summary>
        /// Obtém o caminho físico completo de uma imagem.
        /// </summary>
        public string GetPhysicalPath(string imageUrl)
        {
            var fileName = Path.GetFileName(imageUrl);
            return Path.Combine(_uploadsFolder, fileName);
        }
    }
}
