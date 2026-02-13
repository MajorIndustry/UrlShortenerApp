using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortenerApp.Data;
using UrlShortenerApp.Models;
using System.Security.Cryptography; // Для генерации случайных чисел

namespace UrlShortenerApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

 // 1. Главная страница со списком ссылок
        public async Task<IActionResult> Index()
        {
            var urls = await _context.UrlEntries
                                     .OrderByDescending(u => u.CreatedDate)
                                     .ToListAsync();
            return View(urls);
        }

        // 2. Метод создания сокращенной ссылки
        [HttpPost]
        public async Task<IActionResult> Create(string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl) || !Uri.IsWellFormedUriString(originalUrl, UriKind.Absolute))
            {
         // Простая валидация на сервере [cite: 8]
                TempData["Error"] = "Пожалуйста, введите корректный URL (например, https://google.com)";
                return RedirectToAction(nameof(Index));
            }

            // Генерируем уникальный код
            string shortCode = GenerateRandomString(6);

            // Проверяем на коллизии (маловероятно, но нужно для надежности)
            while (await _context.UrlEntries.AnyAsync(u => u.ShortCode == shortCode))
            {
                shortCode = GenerateRandomString(6);
            }

            var entry = new UrlEntry
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedDate = DateTime.Now
            };

            _context.Add(entry);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

 // 3. Редирект по короткой ссылке
        [HttpGet("/{code}")]
        public async Task<IActionResult> RedirectToOriginal(string code)
        {
            var entry = await _context.UrlEntries.FirstOrDefaultAsync(u => u.ShortCode == code);

            if (entry == null)
            {
                return NotFound();
            }

            // Увеличиваем счетчик переходов
            entry.ClickCount++;
            await _context.SaveChangesAsync(); // Сохраняем изменение счетчика

            return Redirect(entry.OriginalUrl);
        }

 // 4. Удаление ссылки 
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var entry = await _context.UrlEntries.FindAsync(id);
            if (entry != null)
            {
                _context.UrlEntries.Remove(entry);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

 // 5. Редактирование ссылки 
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string newUrl)
        {
            if (string.IsNullOrWhiteSpace(newUrl) || !Uri.IsWellFormedUriString(newUrl, UriKind.Absolute))
            {
                TempData["Error"] = "Некорректный новый URL";
                return RedirectToAction(nameof(Index));
            }

            var entry = await _context.UrlEntries.FindAsync(id);
            if (entry != null)
            {
                entry.OriginalUrl = newUrl;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

 // Вспомогательный метод для генерации "непредсказуемого" кода 
        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            // RandomNumberGenerator используется для криптографической стойкости, 
            // чтобы последовательность была действительно случайной, а не псевдослучайной.
            return string.Create(length, chars, (span, charSet) =>
            {
                for (int i = 0; i < span.Length; i++)
                {
                    span[i] = charSet[RandomNumberGenerator.GetInt32(charSet.Length)];
                }
            });
        }
    }
}