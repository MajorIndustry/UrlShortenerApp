using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace UrlShortenerApp.Models
{
// Индекс нужен для быстрого поиска по короткому коду при редиректе 
    [Index(nameof(ShortCode), IsUnique = true)]
    public class UrlEntry
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите длинный URL")]
        [Url(ErrorMessage = "Некорректный формат URL")] // Валидация ввода [cite: 8]
        public string OriginalUrl { get; set; }

        // Короткий код, который мы генерируем
        [MaxLength(10)]
        public string ShortCode { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int ClickCount { get; set; } = 0;
    }
}